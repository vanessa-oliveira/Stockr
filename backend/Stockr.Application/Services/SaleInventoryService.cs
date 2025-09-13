using Microsoft.Extensions.Logging;
using Stockr.Domain.Entities;
using Stockr.Domain.Enums;
using Stockr.Infrastructure.Repositories;

namespace Stockr.Application.Services;

public interface ISaleInventoryService
{
    Task ProcessSaleInventoryAsync(Guid saleId, IList<SaleItem> saleItems, DateTime saleDate, Guid? userId = null);
    Task ProcessSaleInventoryUpdateAsync(Guid saleId, IList<SaleItem> existingItems, IList<SaleItem> newItems, Guid? userId = null);
    Task RevertSaleInventoryAsync(Guid saleId, IList<SaleItem> saleItems, Guid? userId = null);
    Task<(List<(Guid ProductId, int RequiredQuantity, int AvailableStock)> insufficientStockItems, List<Guid> productsWithoutInventory)>
        ValidateStockAvailabilityAsync(IEnumerable<SaleItem> saleItems);
}

public class SaleInventoryService : ISaleInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IInventoryMovementRepository _inventoryMovementRepository;
    private readonly ILogger<SaleInventoryService> _logger;

    public SaleInventoryService(
        IInventoryRepository inventoryRepository,
        IInventoryMovementRepository inventoryMovementRepository,
        ILogger<SaleInventoryService> logger)
    {
        _inventoryRepository = inventoryRepository;
        _inventoryMovementRepository = inventoryMovementRepository;
        _logger = logger;
    }

    public async Task ProcessSaleInventoryAsync(Guid saleId, IList<SaleItem> saleItems, DateTime saleDate, Guid? userId = null)
    {
        var (inventoryLookup, movements, inventoriesToUpdate) = await PrepareInventoryOperationAsync(saleItems);

        foreach (var saleItem in saleItems)
        {
            if (inventoryLookup.TryGetValue(saleItem.ProductId, out var inventory))
            {
                var movement = CreateInventoryMovement(
                    inventory,
                    saleItem,
                    MovementDirection.Out,
                    saleId,
                    saleDate,
                    userId
                );

                movements.Add(movement);
                inventory.CurrentStock -= saleItem.Quantity;
                inventoriesToUpdate.Add(inventory);
            }
            else
            {
                _logger.LogWarning("Produto {ProductId} não possui configuração de estoque", saleItem.ProductId);
            }
        }

        await ExecuteBatchOperationsAsync(movements, inventoriesToUpdate);
    }

    public async Task ProcessSaleInventoryUpdateAsync(Guid saleId, IList<SaleItem> existingItems, IList<SaleItem> newItems, Guid? userId = null)
    {
        var allProductIds = existingItems.Select(x => x.ProductId)
            .Concat(newItems.Select(x => x.ProductId))
            .Distinct().ToList();

        var inventories = await _inventoryRepository.GetByProductIdsAsync(allProductIds);
        var inventoryLookup = inventories.ToDictionary(i => i.ProductId);

        var movements = new List<InventoryMovement>();
        var inventoriesToUpdate = new List<Inventory>();

        ProcessRemovedItems(existingItems, newItems, inventoryLookup, movements, inventoriesToUpdate, saleId, userId);
        ProcessUpdatedItems(existingItems, newItems, inventoryLookup, movements, inventoriesToUpdate, saleId, userId);
        ProcessNewItems(newItems, inventoryLookup, movements, inventoriesToUpdate, saleId, userId);

        await ExecuteBatchOperationsAsync(movements, inventoriesToUpdate);
    }

    public async Task RevertSaleInventoryAsync(Guid saleId, IList<SaleItem> saleItems, Guid? userId = null)
    {
        var productIds = saleItems.Select(x => x.ProductId).Distinct().ToList();
        var inventories = await _inventoryRepository.GetByProductIdsAsync(productIds);
        var inventoryLookup = inventories.ToDictionary(i => i.ProductId);

        var movements = new List<InventoryMovement>();
        var inventoriesToUpdate = new List<Inventory>();

        foreach (var saleItem in saleItems)
        {
            if (!inventoryLookup.TryGetValue(saleItem.ProductId, out var inventory))
            {
                _logger.LogWarning("Estoque não encontrado para produto {ProductId} ao deletar venda {SaleId}",
                    saleItem.ProductId, saleId);
                continue;
            }

            inventory.CurrentStock += saleItem.Quantity;
            inventoriesToUpdate.Add(inventory);

            var movement = CreateInventoryMovement(
                inventory,
                saleItem,
                MovementDirection.In,
                saleId,
                userId: userId,
                reason: $"Estorno por deleção da venda {saleId}"
            );

            movements.Add(movement);
        }

        await ExecuteBatchOperationsAsync(movements, inventoriesToUpdate);
    }

    public async Task<(List<(Guid ProductId, int RequiredQuantity, int AvailableStock)> insufficientStockItems, List<Guid> productsWithoutInventory)>
        ValidateStockAvailabilityAsync(IEnumerable<SaleItem> saleItems)
    {
        var productIds = saleItems.Select(item => item.ProductId).Distinct().ToList();
        var inventories = await _inventoryRepository.GetByProductIdsAsync(productIds);
        var inventoryLookup = inventories.ToDictionary(i => i.ProductId);

        var insufficientStockItems = new List<(Guid ProductId, int RequiredQuantity, int AvailableStock)>();
        var productsWithoutInventory = new List<Guid>();

        foreach (var item in saleItems)
        {
            if (!inventoryLookup.TryGetValue(item.ProductId, out var inventory))
            {
                productsWithoutInventory.Add(item.ProductId);
                continue;
            }

            if (inventory.CurrentStock < item.Quantity)
            {
                insufficientStockItems.Add((item.ProductId, item.Quantity, inventory.CurrentStock));
            }
        }

        return (insufficientStockItems, productsWithoutInventory);
    }

    private void ProcessRemovedItems(IList<SaleItem> existingItems, IList<SaleItem> newItems,
        Dictionary<Guid, Inventory> inventoryLookup, List<InventoryMovement> movements,
        List<Inventory> inventoriesToUpdate, Guid saleId, Guid? userId)
    {
        var newItemIds = newItems.Select(x => x.Id).ToHashSet();
        var removedItems = existingItems.Where(x => !newItemIds.Contains(x.Id));

        foreach (var removedItem in removedItems)
        {
            if (inventoryLookup.TryGetValue(removedItem.ProductId, out var inventory))
            {
                inventory.CurrentStock += removedItem.Quantity;
                inventoriesToUpdate.Add(inventory);

                var movement = CreateInventoryMovement(
                    inventory,
                    removedItem,
                    MovementDirection.In,
                    saleId,
                    userId: userId,
                    reason: $"Estorno por remoção de item da venda {saleId}"
                );

                movements.Add(movement);
            }
        }
    }

    private void ProcessUpdatedItems(IEnumerable<SaleItem> existingItems, IEnumerable<SaleItem> newItems,
        Dictionary<Guid, Inventory> inventoryLookup, List<InventoryMovement> movements,
        List<Inventory> inventoriesToUpdate, Guid saleId, Guid? userId)
    {
        var existingItemsLookup = existingItems.ToDictionary(x => x.Id);

        foreach (var newItem in newItems)
        {
            if (!existingItemsLookup.TryGetValue(newItem.Id, out var existingItem))
                continue;

            var quantityDifference = newItem.Quantity - existingItem.Quantity;

            if (quantityDifference != 0 && inventoryLookup.TryGetValue(newItem.ProductId, out var inventory))
            {
                ProcessQuantityChange(inventory, quantityDifference, newItem, movements, inventoriesToUpdate, saleId, userId);
            }
        }
    }

    private void ProcessNewItems(IEnumerable<SaleItem> newItems, Dictionary<Guid, Inventory> inventoryLookup,
        List<InventoryMovement> movements, List<Inventory> inventoriesToUpdate, Guid saleId, Guid? userId)
    {
        foreach (var newItem in newItems)
        {
            if (inventoryLookup.TryGetValue(newItem.ProductId, out var inventory))
            {
                inventory.CurrentStock -= newItem.Quantity;
                inventoriesToUpdate.Add(inventory);

                var movement = CreateInventoryMovement(
                    inventory,
                    newItem,
                    MovementDirection.Out,
                    saleId,
                    userId: userId,
                    reason: $"Saída por novo item na venda {saleId}"
                );

                movements.Add(movement);
            }
        }
    }

    private void ProcessQuantityChange(Inventory inventory, int quantityDifference, SaleItem item,
        List<InventoryMovement> movements, List<Inventory> inventoriesToUpdate, Guid saleId, Guid? userId)
    {
        if (quantityDifference > 0)
        {
            // Aumento de quantidade - reduzir mais estoque
            inventory.CurrentStock -= quantityDifference;

            var movement = CreateInventoryMovement(
                inventory,
                item,
                MovementDirection.Out,
                saleId,
                userId: userId,
                reason: $"Saída adicional por aumento na venda {saleId}",
                quantity: quantityDifference
            );

            movements.Add(movement);
        }
        else
        {
            // Diminuição de quantidade - devolver estoque
            var returnQuantity = Math.Abs(quantityDifference);
            inventory.CurrentStock += returnQuantity;

            var movement = CreateInventoryMovement(
                inventory,
                item,
                MovementDirection.In,
                saleId,
                userId: userId,
                reason: $"Estorno por redução na venda {saleId}",
                quantity: returnQuantity
            );

            movements.Add(movement);
        }

        inventoriesToUpdate.Add(inventory);
    }

    private async Task<(Dictionary<Guid, Inventory> inventoryLookup, List<InventoryMovement> movements, List<Inventory> inventoriesToUpdate)>
        PrepareInventoryOperationAsync(IList<SaleItem> saleItems)
    {
        var productIds = saleItems.Select(si => si.ProductId).Distinct().ToList();
        var inventories = await _inventoryRepository.GetByProductIdsAsync(productIds);
        var inventoryLookup = inventories.ToDictionary(i => i.ProductId);

        return (inventoryLookup, new List<InventoryMovement>(), new List<Inventory>());
    }

    private static InventoryMovement CreateInventoryMovement(
        Inventory inventory,
        SaleItem saleItem,
        MovementDirection direction,
        Guid saleId,
        DateTime? movementDate = null,
        Guid? userId = null,
        string? reason = null,
        int? quantity = null)
    {
        return new InventoryMovement
        {
            ProductId = saleItem.ProductId,
            InventoryId = inventory.Id,
            Quantity = quantity ?? saleItem.Quantity,
            Direction = direction,
            SaleId = saleId,
            MovementDate = movementDate ?? DateTime.Now,
            UnitCost = saleItem.UnitPrice,
            UserId = userId,
            Reason = reason
        };
    }

    private async Task ExecuteBatchOperationsAsync(List<InventoryMovement> movements, List<Inventory> inventoriesToUpdate)
    {
        if (movements.Any())
        {
            await _inventoryMovementRepository.AddRangeAsync(movements);
        }

        if (inventoriesToUpdate.Any())
        {
            await _inventoryRepository.UpdateRangeAsync(inventoriesToUpdate.Distinct().ToList());
        }
    }
}