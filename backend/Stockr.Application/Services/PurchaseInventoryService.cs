using Microsoft.Extensions.Logging;
using Stockr.Domain.Entities;
using Stockr.Domain.Enums;
using Stockr.Infrastructure.Repositories;

namespace Stockr.Application.Services;

public interface IPurchaseInventoryService
{
    Task ProcessPurchaseInventoryAsync(Guid purchaseId, IList<PurchaseItem> purchaseItems, DateTime movementDate);
    Task ProcessInventoryUpdateAsync(Guid purchaseId, IList<PurchaseItem> existingItems, IList<PurchaseItem> newItems, Guid? userId = null);
    Task RevertPurchaseInventoryAsync(Guid purchaseId, IList<PurchaseItem> purchaseItems, Guid? userId = null);
}

public class PurchaseInventoryService : IPurchaseInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IInventoryMovementRepository _inventoryMovementRepository;
    private readonly ILogger<PurchaseInventoryService> _logger;

    public PurchaseInventoryService(
        IInventoryRepository inventoryRepository,
        IInventoryMovementRepository inventoryMovementRepository,
        ILogger<PurchaseInventoryService> logger)
    {
        _inventoryRepository = inventoryRepository;
        _inventoryMovementRepository = inventoryMovementRepository;
        _logger = logger;
    }

    public async Task ProcessPurchaseInventoryAsync(Guid purchaseId, IList<PurchaseItem> purchaseItems, DateTime movementDate)
    {
        var (inventoryLookup, movements, inventoriesToUpdate) = await PrepareInventoryOperationAsync(purchaseItems);

        foreach (var purchaseItem in purchaseItems)
        {
            if (inventoryLookup.TryGetValue(purchaseItem.ProductId, out var inventory))
            {
                var movement = CreateInventoryMovement(
                    inventory,
                    purchaseItem,
                    MovementDirection.In,
                    purchaseId,
                    movementDate
                );

                movements.Add(movement);
                inventory.CurrentStock += purchaseItem.Quantity;
                inventoriesToUpdate.Add(inventory);
            }
            else
            {
                _logger.LogWarning("Produto {ProductId} não possui configuração de estoque", purchaseItem.ProductId);
            }
        }

        await ExecuteBatchOperationsAsync(movements, inventoriesToUpdate);
    }

    public async Task ProcessInventoryUpdateAsync(Guid purchaseId, IList<PurchaseItem> existingItems, IList<PurchaseItem> newItems, Guid? userId = null)
    {
        var allProductIds = existingItems.Select(x => x.ProductId)
            .Concat(newItems.Select(x => x.ProductId))
            .Distinct().ToList();

        var inventories = await _inventoryRepository.GetByProductIdsAsync(allProductIds);
        var inventoryLookup = inventories.ToDictionary(i => i.ProductId);

        var movements = new List<InventoryMovement>();
        var inventoriesToUpdate = new List<Inventory>();

        ProcessRemovedItems(existingItems, newItems, inventoryLookup, movements, inventoriesToUpdate, purchaseId, userId);
        ProcessUpdatedItems(existingItems, newItems, inventoryLookup, movements, inventoriesToUpdate, purchaseId, userId);
        ProcessNewItems(newItems, inventoryLookup, movements, inventoriesToUpdate, purchaseId, userId);

        await ExecuteBatchOperationsAsync(movements, inventoriesToUpdate);
    }

    public async Task RevertPurchaseInventoryAsync(Guid purchaseId, IList<PurchaseItem> purchaseItems, Guid? userId = null)
    {
        var productIds = purchaseItems.Select(x => x.ProductId).Distinct().ToList();
        var inventories = await _inventoryRepository.GetByProductIdsAsync(productIds);
        var inventoryLookup = inventories.ToDictionary(i => i.ProductId);

        var movements = new List<InventoryMovement>();
        var inventoriesToUpdate = new List<Inventory>();

        foreach (var purchaseItem in purchaseItems)
        {
            if (!inventoryLookup.TryGetValue(purchaseItem.ProductId, out var inventory))
            {
                _logger.LogWarning("Estoque não encontrado para produto {ProductId} ao deletar compra {PurchaseId}",
                    purchaseItem.ProductId, purchaseId);
                continue;
            }

            if (inventory.CurrentStock < purchaseItem.Quantity)
            {
                _logger.LogWarning("Estoque insuficiente para estornar produto {ProductId} na compra {PurchaseId}. Disponível: {Available}, Necessário: {Required}",
                    purchaseItem.ProductId, purchaseId, inventory.CurrentStock, purchaseItem.Quantity);
            }

            inventory.CurrentStock -= purchaseItem.Quantity;
            inventoriesToUpdate.Add(inventory);

            movements.Add(new InventoryMovement
            {
                ProductId = purchaseItem.ProductId,
                InventoryId = inventory.Id,
                Quantity = purchaseItem.Quantity,
                Direction = MovementDirection.Out,
                PurchaseId = purchaseId,
                UserId = userId,
                MovementDate = DateTime.Now,
                UnitCost = purchaseItem.UnitPrice,
                Reason = $"Estorno por deleção da compra {purchaseId}"
            });
        }

        await ExecuteBatchOperationsAsync(movements, inventoriesToUpdate);
    }

    private void ProcessRemovedItems(IList<PurchaseItem> existingItems, IList<PurchaseItem> newItems,
        Dictionary<Guid, Inventory> inventoryLookup, List<InventoryMovement> movements,
        List<Inventory> inventoriesToUpdate, Guid purchaseId, Guid? userId)
    {
        var newItemIds = newItems.Select(x => x.Id).ToHashSet();
        var removedItems = existingItems.Where(x => !newItemIds.Contains(x.Id));

        foreach (var removedItem in removedItems)
        {
            if (inventoryLookup.TryGetValue(removedItem.ProductId, out var inventory))
            {
                inventory.CurrentStock -= removedItem.Quantity;
                inventoriesToUpdate.Add(inventory);

                movements.Add(new InventoryMovement
                {
                    ProductId = removedItem.ProductId,
                    InventoryId = inventory.Id,
                    Quantity = removedItem.Quantity,
                    Direction = MovementDirection.Out,
                    PurchaseId = purchaseId,
                    UserId = userId,
                    MovementDate = DateTime.Now,
                    UnitCost = removedItem.UnitPrice,
                    Reason = $"Estorno por remoção de item da compra {purchaseId}"
                });
            }
        }
    }

    private void ProcessUpdatedItems(IEnumerable<PurchaseItem> existingItems, IEnumerable<PurchaseItem> newItems,
        Dictionary<Guid, Inventory> inventoryLookup, List<InventoryMovement> movements,
        List<Inventory> inventoriesToUpdate, Guid purchaseId, Guid? userId)
    {
        var existingItemsLookup = existingItems.ToDictionary(x => x.Id);

        foreach (var newItem in newItems)
        {
            if (!existingItemsLookup.TryGetValue(newItem.Id, out var existingItem))
                continue;

            var quantityDifference = newItem.Quantity - existingItem.Quantity;

            if (quantityDifference != 0 && inventoryLookup.TryGetValue(newItem.ProductId, out var inventory))
            {
                ProcessQuantityChange(inventory, quantityDifference, newItem, movements, inventoriesToUpdate, purchaseId, userId);
            }
        }
    }

    private void ProcessNewItems(IEnumerable<PurchaseItem> newItems, Dictionary<Guid, Inventory> inventoryLookup,
        List<InventoryMovement> movements, List<Inventory> inventoriesToUpdate, Guid purchaseId, Guid? userId)
    {
        foreach (var newItem in newItems)
        {
            if (inventoryLookup.TryGetValue(newItem.ProductId, out var inventory))
            {
                inventory.CurrentStock += newItem.Quantity;
                inventoriesToUpdate.Add(inventory);

                movements.Add(new InventoryMovement
                {
                    ProductId = newItem.ProductId,
                    InventoryId = inventory.Id,
                    Quantity = newItem.Quantity,
                    Direction = MovementDirection.In,
                    PurchaseId = purchaseId,
                    UserId = userId,
                    MovementDate = DateTime.Now,
                    UnitCost = newItem.UnitPrice,
                    Reason = $"Entrada por novo item na compra {purchaseId}"
                });
            }
        }
    }

    private void ProcessQuantityChange(Inventory inventory, int quantityDifference, PurchaseItem item,
        List<InventoryMovement> movements, List<Inventory> inventoriesToUpdate, Guid purchaseId, Guid? userId)
    {
        if (quantityDifference > 0)
        {
            inventory.CurrentStock += quantityDifference;

            movements.Add(new InventoryMovement
            {
                ProductId = item.ProductId,
                InventoryId = inventory.Id,
                Quantity = quantityDifference,
                Direction = MovementDirection.In,
                PurchaseId = purchaseId,
                UserId = userId,
                MovementDate = DateTime.Now,
                UnitCost = item.UnitPrice,
                Reason = $"Entrada adicional por aumento na compra {purchaseId}"
            });
        }
        else
        {
            var reductionQuantity = Math.Abs(quantityDifference);
            inventory.CurrentStock -= reductionQuantity;

            movements.Add(new InventoryMovement
            {
                ProductId = item.ProductId,
                InventoryId = inventory.Id,
                Quantity = reductionQuantity,
                Direction = MovementDirection.Out,
                PurchaseId = purchaseId,
                UserId = userId,
                MovementDate = DateTime.Now,
                UnitCost = item.UnitPrice,
                Reason = $"Estorno por redução na compra {purchaseId}"
            });
        }

        inventoriesToUpdate.Add(inventory);
    }

    private async Task<(Dictionary<Guid, Inventory> inventoryLookup, List<InventoryMovement> movements, List<Inventory> inventoriesToUpdate)>
        PrepareInventoryOperationAsync(IList<PurchaseItem> purchaseItems)
    {
        var productIds = purchaseItems.Select(pi => pi.ProductId).Distinct().ToList();
        var inventories = await _inventoryRepository.GetByProductIdsAsync(productIds);
        var inventoryLookup = inventories.ToDictionary(i => i.ProductId);

        return (inventoryLookup, new List<InventoryMovement>(), new List<Inventory>());
    }

    private static InventoryMovement CreateInventoryMovement(
        Inventory inventory,
        PurchaseItem purchaseItem,
        MovementDirection direction,
        Guid purchaseId,
        DateTime? movementDate = null,
        Guid? userId = null,
        string? reason = null)
    {
        return new InventoryMovement
        {
            ProductId = purchaseItem.ProductId,
            InventoryId = inventory.Id,
            Quantity = purchaseItem.Quantity,
            Direction = direction,
            PurchaseId = purchaseId,
            MovementDate = movementDate ?? DateTime.Now,
            UnitCost = purchaseItem.UnitPrice,
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