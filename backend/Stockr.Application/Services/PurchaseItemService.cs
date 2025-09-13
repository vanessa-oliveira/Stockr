using Stockr.Application.Commands.Purchase;
using Stockr.Domain.Entities;
using Stockr.Infrastructure.Repositories;

namespace Stockr.Application.Services;

public interface IPurchaseItemService
{
    Task<IList<PurchaseItem>> CreatePurchaseItemsAsync(Guid purchaseId, IList<CreatePurchaseItemCommand> itemDtos);
    Task<decimal> UpdatePurchaseItemsAsync(Guid purchaseId, IEnumerable<UpdatePurchaseItemCommand> itemDtos, IEnumerable<PurchaseItem> existingItems);
    Task DeletePurchaseItemsAsync(IEnumerable<PurchaseItem> purchaseItems);
    decimal CalculateTotalAmount(IEnumerable<CreatePurchaseItemCommand> items);
    decimal CalculateTotalAmount(IEnumerable<UpdatePurchaseItemCommand> items);
}

public class PurchaseItemService : IPurchaseItemService
{
    private readonly IPurchaseItemRepository _purchaseItemRepository;

    public PurchaseItemService(IPurchaseItemRepository purchaseItemRepository)
    {
        _purchaseItemRepository = purchaseItemRepository;
    }

    public async Task<IList<PurchaseItem>> CreatePurchaseItemsAsync(Guid purchaseId, IList<CreatePurchaseItemCommand> itemDtos)
    {
        var purchaseItems = itemDtos.Select(item => CreatePurchaseItem(purchaseId, item)).ToList();

        await _purchaseItemRepository.AddRangeAsync(purchaseItems);
        return purchaseItems;
    }

    public async Task<decimal> UpdatePurchaseItemsAsync(Guid purchaseId, IEnumerable<UpdatePurchaseItemCommand> itemDtos, IEnumerable<PurchaseItem> existingItems)
    {
        var itemsToDelete = itemDtos.Where(x => x.ToDelete && x.Id.HasValue).ToList();

        await ProcessItemDeletions(itemsToDelete, existingItems);

        decimal totalAmount = 0;
        var itemsToProcess = itemDtos.Where(x => !x.ToDelete).ToList();

        foreach (var item in itemsToProcess)
        {
            var totalPrice = CalculateItemTotal(item.Quantity, item.UnitPrice);

            if (item.Id.HasValue)
            {
                await UpdateExistingItem(item, existingItems);
            }
            else
            {
                await CreateNewItem(purchaseId, item);
            }

            totalAmount += totalPrice;
        }

        return totalAmount;
    }

    public async Task DeletePurchaseItemsAsync(IEnumerable<PurchaseItem> purchaseItems)
    {
        await _purchaseItemRepository.DeleteRangeAsync(purchaseItems.ToList());
    }

    public decimal CalculateTotalAmount(IEnumerable<CreatePurchaseItemCommand> items)
    {
        return items.Sum(item => item.Quantity * item.UnitPrice);
    }

    public decimal CalculateTotalAmount(IEnumerable<UpdatePurchaseItemCommand> items)
    {
        return items.Where(x => !x.ToDelete).Sum(item => item.Quantity * item.UnitPrice);
    }

    private async Task ProcessItemDeletions(IEnumerable<UpdatePurchaseItemCommand> itemsToDelete, IEnumerable<PurchaseItem> existingItems)
    {
        foreach (var itemToDelete in itemsToDelete)
        {
            var existingItem = existingItems?.FirstOrDefault(x => x.Id == itemToDelete.Id);
            if (existingItem != null)
            {
                await _purchaseItemRepository.DeleteAsync(existingItem);
            }
        }
    }

    private async Task UpdateExistingItem(UpdatePurchaseItemCommand item, IEnumerable<PurchaseItem> existingItems)
    {
        var existingItem = existingItems?.FirstOrDefault(x => x.Id == item.Id);
        if (existingItem != null)
        {
            existingItem.ProductId = item.ProductId;
            existingItem.Quantity = item.Quantity;
            existingItem.UnitPrice = item.UnitPrice;
            existingItem.TotalPrice = item.Quantity * item.UnitPrice;
            await _purchaseItemRepository.UpdateAsync(existingItem);
        }
    }

    private async Task CreateNewItem(Guid purchaseId, UpdatePurchaseItemCommand item)
    {
        var newPurchaseItem = CreatePurchaseItemFromUpdate(purchaseId, item);
        await _purchaseItemRepository.AddAsync(newPurchaseItem);
    }

    private static PurchaseItem CreatePurchaseItem(Guid purchaseId, CreatePurchaseItemCommand item)
    {
        return new PurchaseItem
        {
            PurchaseId = purchaseId,
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            TotalPrice = CalculateItemTotal(item.Quantity, item.UnitPrice)
        };
    }

    private static PurchaseItem CreatePurchaseItemFromUpdate(Guid purchaseId, UpdatePurchaseItemCommand item)
    {
        return new PurchaseItem
        {
            PurchaseId = purchaseId,
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            TotalPrice = CalculateItemTotal(item.Quantity, item.UnitPrice)
        };
    }

    private static decimal CalculateItemTotal(int quantity, decimal unitPrice) => quantity * unitPrice;
}