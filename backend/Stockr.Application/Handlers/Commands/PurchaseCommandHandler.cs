using MediatR;
using Microsoft.Extensions.Logging;
using Stockr.Application.Commands.Purchase;
using Stockr.Application.Services;
using Stockr.Domain.Entities;
using Stockr.Infrastructure.Repositories;

namespace Stockr.Application.Handlers.Commands;

public class PurchaseCommandHandler :
    IRequestHandler<CreatePurchaseCommand, Unit>,
    IRequestHandler<UpdatePurchaseCommand, Unit>,
    IRequestHandler<DeletePurchaseCommand, Unit>
{
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly IPurchaseItemRepository _purchaseItemRepository;
    private readonly IPurchaseInventoryService _purchaseInventoryService;
    private readonly IPurchaseItemService _purchaseItemService;
    private readonly ITenantService _tenantService;
    private readonly ILogger<PurchaseCommandHandler> _logger;

    public PurchaseCommandHandler(IPurchaseRepository purchaseRepository,
        IPurchaseItemRepository purchaseItemRepository,
        IPurchaseInventoryService purchaseInventoryService,
        IPurchaseItemService purchaseItemService,
        ITenantService tenantService,
        ILogger<PurchaseCommandHandler> logger)
    {
        _purchaseRepository = purchaseRepository;
        _purchaseItemRepository = purchaseItemRepository;
        _purchaseInventoryService = purchaseInventoryService;
        _purchaseItemService = purchaseItemService;
        _tenantService = tenantService;
        _logger = logger;
    }

    public async Task<Unit> Handle(CreatePurchaseCommand command, CancellationToken cancellationToken)
    {
        var currentTenantId = _tenantService.GetCurrentTenantId();
        if (!currentTenantId.HasValue)
        {
            throw new UnauthorizedAccessException("User must belong to a tenant");
        }

        if (!command.PurchaseItems.Any())
        {
            throw new InvalidOperationException("Purchase must have at least one item");
        }

        var purchase = new Purchase
        {
            SupplierId = command.SupplierId,
            PurchaseDate = command.PurchaseDate,
            Notes = command.Notes,
            InvoiceNumber = command.InvoiceNumber,
            TotalAmount = _purchaseItemService.CalculateTotalAmount(command.PurchaseItems),
            TenantId = currentTenantId.Value
        };

        await _purchaseRepository.AddAsync(purchase);

        var purchaseItems = await _purchaseItemService.CreatePurchaseItemsAsync(purchase.Id, command.PurchaseItems);

        await _purchaseInventoryService.ProcessPurchaseInventoryAsync(purchase.Id, purchaseItems, purchase.PurchaseDate);

        return Unit.Value;
    }

    public async Task<Unit> Handle(UpdatePurchaseCommand command, CancellationToken cancellationToken)
    {
        var purchase = await GetAndValidatePurchaseAsync(command.Id);
        if (!command.PurchaseItems.Any())
        {
            throw new InvalidOperationException("Purchase must have at least one item");
        }

        var existingItems = await _purchaseItemRepository.GetByPurchaseAsync(command.Id);
        
        var purchaseItems = command.PurchaseItems.Select(x => new PurchaseItem()
        {
            ProductId = x.ProductId,
            Quantity = x.Quantity,
            UnitPrice = x.UnitPrice,
            TotalPrice = x.Quantity * x.UnitPrice
        }).ToList();

        await _purchaseInventoryService.ProcessInventoryUpdateAsync(purchase.Id, existingItems, purchaseItems, command.UserId);

        var totalAmount = await _purchaseItemService.UpdatePurchaseItemsAsync(purchase.Id, command.PurchaseItems, existingItems);

        purchase.SupplierId = command.SupplierId;
        purchase.PurchaseDate = command.PurchaseDate;
        purchase.Notes = command.Notes;
        purchase.InvoiceNumber = command.InvoiceNumber;
        purchase.TotalAmount = totalAmount;
        purchase.UpdatedAt = DateTime.UtcNow;

        await _purchaseRepository.UpdateAsync(purchase);

        _logger.LogInformation("Compra {PurchaseId} atualizada com sucesso", purchase.Id);

        return Unit.Value;
    }

    public async Task<Unit> Handle(DeletePurchaseCommand command, CancellationToken cancellationToken)
    {
        var purchase = await GetAndValidatePurchaseAsync(command.Id);
        var purchaseItems = await _purchaseItemRepository.GetByPurchaseAsync(command.Id);

        if (!purchaseItems.Any())
        {
            _logger.LogInformation("Compra {PurchaseId} não possui itens, deletando apenas o registro principal", purchase.Id);
            await _purchaseRepository.DeleteAsync(purchase);
            return Unit.Value;
        }

        try
        {
            await _purchaseInventoryService.RevertPurchaseInventoryAsync(purchase.Id, purchaseItems, command.UserId);
            await _purchaseItemService.DeletePurchaseItemsAsync(purchaseItems);
            await _purchaseRepository.DeleteAsync(purchase);

            _logger.LogInformation("Compra {PurchaseId} deletada com sucesso. {ItemCount} itens estornados do estoque",
                purchase.Id, purchaseItems.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar compra {PurchaseId}. Operação pode ter sido parcialmente executada", purchase.Id);
            throw;
        }

        return Unit.Value;
    }

    private async Task<Purchase> GetAndValidatePurchaseAsync(Guid id)
    {
        var purchase = await _purchaseRepository.GetByIdAsync(id);
        if (purchase == null)
            throw new ArgumentException("Purchase not found");
        return purchase;
    }
}