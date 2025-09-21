using MediatR;
using Microsoft.Extensions.Logging;
using Stockr.Application.Commands.Sales;
using Stockr.Application.Services;
using Stockr.Domain.Entities;
using Stockr.Domain.Enums;
using Stockr.Infrastructure.Repositories;

namespace Stockr.Application.Handlers.Commands;

public class SaleCommandHandler : 
    IRequestHandler<CreateSaleCommand, Unit>,
    IRequestHandler<UpdateSaleCommand, Unit>,
    IRequestHandler<DeleteSaleCommand, Unit>,
    IRequestHandler<ChangeSaleStatusCommand, Unit>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IProductRepository _productRepository;
    private readonly ISaleItemRepository _saleItemRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IInventoryMovementRepository _inventoryMovementRepository;
    private readonly ISaleItemService _saleItemService;
    private readonly ISaleInventoryService _saleInventoryService;
    private readonly ILogger<SaleCommandHandler> _logger;

    public SaleCommandHandler(
        ISaleRepository saleRepository,
        IProductRepository productRepository,
        ISaleItemRepository saleItemRepository,
        IInventoryRepository inventoryRepository,
        IInventoryMovementRepository inventoryMovementRepository,
        ISaleItemService saleItemService,
        ISaleInventoryService saleInventoryService,
        ILogger<SaleCommandHandler> logger)
    {
        _saleRepository = saleRepository;
        _productRepository = productRepository;
        _saleItemRepository = saleItemRepository;
        _inventoryRepository = inventoryRepository;
        _inventoryMovementRepository = inventoryMovementRepository;
        _saleItemService = saleItemService;
        _saleInventoryService = saleInventoryService;
        _logger = logger;
    }

    public async Task<Unit> Handle(CreateSaleCommand command, CancellationToken cancellationToken)
    {
        ValidateCreateCommand(command);

        var productIds = command.SaleItems.Select(item => item.ProductId).Distinct().ToList();
        var products = await _productRepository.GetByIdsAsync(productIds);
        var productLookup = products.ToDictionary(p => p.Id);

        // Validar estoque ANTES de criar a venda
        var inventories = await _inventoryRepository.GetByProductIdsAsync(productIds);
        var inventoryLookup = inventories.ToDictionary(i => i.ProductId);

        var productsWithoutInventory = productIds.Where(pid => !inventoryLookup.ContainsKey(pid)).ToList();
        if (productsWithoutInventory.Any())
        {
            var productNames = productsWithoutInventory
                .Select(pid => productLookup.TryGetValue(pid, out var p) ? p.Name : pid.ToString());
            _logger.LogError("Produtos sem configuração de estoque: {ProductIds}",
                string.Join(", ", productsWithoutInventory));
            throw new InvalidOperationException(
                $"Os seguintes produtos não possuem estoque cadastrado: {string.Join(", ", productNames)}");
        }

        var insufficientStockItems = new List<(Guid ProductId, string ProductName, int RequiredQuantity, int AvailableStock)>();
        foreach (var item in command.SaleItems)
        {
            var inventory = inventoryLookup[item.ProductId];
            if (inventory.CurrentStock < item.Quantity)
            {
                var productName = productLookup[item.ProductId].Name;
                insufficientStockItems.Add((item.ProductId, productName, item.Quantity, inventory.CurrentStock));
            }
        }

        if (insufficientStockItems.Any())
        {
            var errorMessage = string.Join("; ", insufficientStockItems.Select(item =>
                $"Produto {item.ProductName}: requerido {item.RequiredQuantity}, disponível {item.AvailableStock}"));
            _logger.LogWarning("Venda rejeitada por estoque insuficiente: {ErrorMessage}", errorMessage);
            throw new InvalidOperationException($"Estoque insuficiente para completar a venda: {errorMessage}");
        }

        var sale = CreateSale(command, productLookup);
        await _saleRepository.AddAsync(sale);

        var saleItems = await _saleItemService.CreateSaleItemsAsync(sale.Id, command.SaleItems, productLookup);

        await _saleInventoryService.ProcessSaleInventoryAsync(sale.Id, saleItems, sale.SaleDate, command.SalespersonId);

        _logger.LogInformation("Venda {SaleId} criada com sucesso para {ItemCount} itens",
            sale.Id, saleItems.Count);

        return Unit.Value;
    }

    public async Task<Unit> Handle(UpdateSaleCommand command, CancellationToken cancellationToken)
    {
        var existingSale = await _saleRepository.GetByIdAsync(command.Id);
        if (existingSale == null)
        {
            throw new ArgumentException("Sale not found");
        }

        // Validar se a venda pode ser alterada
        if (existingSale.SaleStatus == SaleStatus.Confirmed || existingSale.SaleStatus == SaleStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot update completed or cancelled sales");
        }

        // Buscar dados necessários em batch
        var existingItems = await _saleItemRepository.GetBySaleAsync(command.Id);
        var productIds = command.SaleItems.Select(x => x.ProductId).Distinct().ToList();
        var products = await _productRepository.GetByIdsAsync(productIds);
        var productLookup = products.ToDictionary(p => p.Id);
        var inventories = await _inventoryRepository.GetByProductIdsAsync(productIds);
        var inventoryLookup = inventories.ToDictionary(i => i.ProductId);

        // Validar se todos os produtos possuem estoque cadastrado
        var productsWithoutInventory = productIds.Where(pid => !inventoryLookup.ContainsKey(pid)).ToList();
        if (productsWithoutInventory.Any())
        {
            var productNames = productsWithoutInventory
                .Select(pid => productLookup.TryGetValue(pid, out var p) ? p.Name : pid.ToString());
            throw new InvalidOperationException(
                $"Os seguintes produtos não possuem estoque cadastrado: {string.Join(", ", productNames)}");
        }

        // Preparar listas para operações em batch
        var movements = new List<InventoryMovement>();
        var inventoriesToUpdate = new List<Inventory>();
        var itemsToDelete = command.SaleItems.Where(x => x.ToDelete && x.Id.HasValue).ToList();

        // 1. Processar deleções (estornar estoque)
        foreach (var itemToDelete in itemsToDelete)
        {
            var existingItem = existingItems?.FirstOrDefault(x => x.Id == itemToDelete.Id);
            if (existingItem != null)
            {
                // Estornar estoque
                var inventory = inventoryLookup[existingItem.ProductId];
                inventory.CurrentStock += existingItem.Quantity;
                inventoriesToUpdate.Add(inventory);

                // Criar movimentação de entrada (estorno)
                movements.Add(new InventoryMovement
                {
                    ProductId = existingItem.ProductId,
                    InventoryId = inventory.Id,
                    Quantity = existingItem.Quantity,
                    Direction = MovementDirection.In,
                    SaleId = existingSale.Id,
                    UserId = command.SalesPersonId,
                    MovementDate = DateTime.Now,
                    UnitCost = existingItem.UnitPrice,
                });

                await _saleItemRepository.DeleteAsync(existingItem);
            }
        }

        decimal totalAmount = 0;
        var itemsToProcess = command.SaleItems.Where(x => !x.ToDelete).ToList();

        // 2. Processar atualizações e novos itens
        foreach (var item in itemsToProcess)
        {
            var product = productLookup[item.ProductId];
            var unitPrice = item.UnitPrice ?? product.SalePrice;
            var totalPrice = unitPrice * item.Quantity;
            var inventory = inventoryLookup[item.ProductId];

            if (item.Id.HasValue)
            {
                // Atualizar item existente
                var existingItem = existingItems?.FirstOrDefault(x => x.Id == item.Id);
                if (existingItem != null)
                {
                    var quantityDifference = item.Quantity - existingItem.Quantity;
                    
                    if (quantityDifference > 0)
                    {
                        // Aumento de quantidade - verificar estoque
                        if (inventory.CurrentStock < quantityDifference)
                        {
                            throw new InvalidOperationException(
                                $"Estoque insuficiente para produto {product.Name}. Disponível: {inventory.CurrentStock}, Necessário: {quantityDifference}");
                        }
                        
                        // Reduzir estoque
                        inventory.CurrentStock -= quantityDifference;
                        
                        // Movimentação de saída
                        movements.Add(new InventoryMovement
                        {
                            ProductId = item.ProductId,
                            InventoryId = inventory.Id,
                            Quantity = quantityDifference,
                            Direction = MovementDirection.Out,
                            SaleId = existingSale.Id,
                            UserId = command.SalesPersonId,
                            MovementDate = DateTime.Now,
                            UnitCost = unitPrice,
                        });
                    }
                    else if (quantityDifference < 0)
                    {
                        // Diminuição de quantidade - estornar estoque
                        var returnQuantity = Math.Abs(quantityDifference);
                        inventory.CurrentStock += returnQuantity;
                        
                        // Movimentação de entrada (estorno)
                        movements.Add(new InventoryMovement
                        {
                            ProductId = item.ProductId,
                            InventoryId = inventory.Id,
                            Quantity = returnQuantity,
                            Direction = MovementDirection.In,
                            SaleId = existingSale.Id,
                            UserId = command.SalesPersonId,
                            MovementDate = DateTime.Now,
                            UnitCost = unitPrice,
                        });
                    }

                    existingItem.ProductId = item.ProductId;
                    existingItem.Quantity = item.Quantity;
                    existingItem.UnitPrice = unitPrice;
                    existingItem.TotalPrice = totalPrice;
                    await _saleItemRepository.UpdateAsync(existingItem);
                    
                    if (quantityDifference != 0)
                    {
                        inventoriesToUpdate.Add(inventory);
                    }
                }
            }
            else
            {
                // Novo item - verificar estoque
                if (inventory.CurrentStock < item.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Estoque insuficiente para produto {product.Name}. Disponível: {inventory.CurrentStock}, Necessário: {item.Quantity}");
                }

                // Criar novo item
                var newSaleItem = new SaleItem
                {
                    SaleId = existingSale.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice
                };
                await _saleItemRepository.AddAsync(newSaleItem);

                // Reduzir estoque
                inventory.CurrentStock -= item.Quantity;
                inventoriesToUpdate.Add(inventory);

                // Movimentação de saída
                movements.Add(new InventoryMovement
                {
                    ProductId = item.ProductId,
                    InventoryId = inventory.Id,
                    Quantity = item.Quantity,
                    Direction = MovementDirection.Out,
                    SaleId = existingSale.Id,
                    UserId = command.SalesPersonId,
                    MovementDate = DateTime.Now,
                    UnitCost = unitPrice,
                });
            }

            totalAmount += totalPrice;
        }

        // Executar operações em lote
        await _inventoryMovementRepository.AddRangeAsync(movements);
        await _inventoryRepository.UpdateRangeAsync(inventoriesToUpdate.Distinct().ToList());

        // Atualizar venda
        existingSale.SaleDate = command.SaleDate;
        existingSale.CustomerId = command.CustomerId;
        existingSale.SalesPersonId = command.SalesPersonId;
        existingSale.SaleStatus = Enum.Parse<SaleStatus>(command.SaleStatus);
        existingSale.TotalAmount = totalAmount;
        existingSale.UpdatedAt = DateTime.UtcNow;

        await _saleRepository.UpdateAsync(existingSale);

        _logger.LogInformation("Venda {SaleId} atualizada com sucesso", existingSale.Id);

        return Unit.Value;
    }

    public async Task<Unit> Handle(DeleteSaleCommand command, CancellationToken cancellationToken)
    {
        var sale = await GetAndValidateSaleAsync(command.Id);

        // Validar se a venda pode ser deletada
        if (sale.SaleStatus == SaleStatus.Completed)
        {
            throw new InvalidOperationException("Cannot delete completed sales. Consider cancelling instead.");
        }

        var saleItems = await _saleItemRepository.GetBySaleAsync(command.Id);
        if (!saleItems.Any())
        {
            _logger.LogInformation("Venda {SaleId} não possui itens. Deletando apenas o registro principal", sale.Id);
            await _saleRepository.DeleteAsync(sale);
            return Unit.Value;
        }

        try
        {
            // Reverter estoque
            await _saleInventoryService.RevertSaleInventoryAsync(sale.Id, saleItems, command.UserId);

            // Deletar itens da venda
            await _saleItemService.DeleteSaleItemsAsync(saleItems);

            // Deletar a venda
            await _saleRepository.DeleteAsync(sale);

            _logger.LogInformation("Venda {SaleId} deletada com sucesso. {ItemCount} itens estornados ao estoque.",
                sale.Id, saleItems.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar venda {SaleId}.", sale.Id);
            throw;
        }

        return Unit.Value;
    }

    public async Task<Unit> Handle(ChangeSaleStatusCommand command, CancellationToken cancellationToken)
    {
        var sale = await GetAndValidateSaleAsync(command.Id);

        sale.SaleStatus = Enum.Parse<SaleStatus>(command.NewStatus);

        await _saleRepository.UpdateAsync(sale);
        return Unit.Value;
    }

    private static void ValidateCreateCommand(CreateSaleCommand command)
    {
        if (!command.SaleItems.Any())
        {
            throw new ArgumentException("Sale must have at least one item");
        }

        if (command.SaleItems.Any(item => item.Quantity <= 0))
        {
            throw new ArgumentException("All sale items must have positive quantities");
        }
    }

    private async Task<Sale> GetAndValidateSaleAsync(Guid saleId)
    {
        var sale = await _saleRepository.GetByIdAsync(saleId);
        if (sale == null)
        {
            throw new ArgumentException("Sale not found");
        }
        return sale;
    }

    private static Sale CreateSale(CreateSaleCommand command, Dictionary<Guid, Product> productLookup)
    {
        var totalAmount = command.SaleItems.Sum(item =>
        {
            var unitPrice = item.UnitPrice ?? productLookup[item.ProductId].SalePrice;
            return unitPrice * item.Quantity;
        });

        return new Sale
        {
            CustomerId = command.CustomerId,
            SalesPersonId = command.SalespersonId,
            SaleStatus = Enum.Parse<SaleStatus>(command.SaleStatus),
            SaleDate = command.SaleDate ?? DateTime.Now,
            TotalAmount = totalAmount
        };
    }

    private void ValidateStockAvailability(
        List<(Guid ProductId, int RequiredQuantity, int AvailableStock)> insufficientStockItems,
        List<Guid> productsWithoutInventory)
    {
        if (productsWithoutInventory.Any())
        {
            _logger.LogError("Produtos sem configuração de estoque: {ProductIds}",
                string.Join(", ", productsWithoutInventory));
            throw new InvalidOperationException($"Produtos sem configuração de estoque: {string.Join(", ", productsWithoutInventory)}");
        }

        if (insufficientStockItems.Any())
        {
            var errorMessage = string.Join("; ", insufficientStockItems.Select(item =>
                $"Produto {item.ProductId}: requerido {item.RequiredQuantity}, disponível {item.AvailableStock}"));

            _logger.LogWarning("Venda rejeitada por estoque insuficiente: {ErrorMessage}", errorMessage);
            throw new InvalidOperationException($"Estoque insuficiente para completar a venda: {errorMessage}");
        }
    }
}