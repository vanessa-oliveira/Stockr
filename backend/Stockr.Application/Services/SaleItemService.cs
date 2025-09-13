using Stockr.Application.Commands.Sales;
using Stockr.Domain.Entities;
using Stockr.Infrastructure.Repositories;

namespace Stockr.Application.Services;

public interface ISaleItemService
{
    Task<IList<SaleItem>> CreateSaleItemsAsync(Guid saleId, IList<CreateSaleItemCommand> itemDtos, Dictionary<Guid, Product> productLookup);
    Task<decimal> UpdateSaleItemsAsync(Guid saleId, IList<UpdateSaleItems> itemDtos, IList<SaleItem> existingItems, Dictionary<Guid, Product> productLookup);
    Task DeleteSaleItemsAsync(IList<SaleItem> saleItems);
    decimal CalculateTotalAmount(IList<CreateSaleItemCommand> items, Dictionary<Guid, Product> productLookup);
    decimal CalculateTotalAmount(IList<UpdateSaleItems> items, Dictionary<Guid, Product> productLookup);
}

public class SaleItemService : ISaleItemService
{
    private readonly ISaleItemRepository _saleItemRepository;

    public SaleItemService(ISaleItemRepository saleItemRepository)
    {
        _saleItemRepository = saleItemRepository;
    }

    public async Task<IList<SaleItem>> CreateSaleItemsAsync(Guid saleId, IList<CreateSaleItemCommand> itemDtos, Dictionary<Guid, Product> productLookup)
    {
        var saleItems = itemDtos.Select(item => CreateSaleItem(saleId, item, productLookup)).ToList();

        await _saleItemRepository.AddRangeAsync(saleItems);
        return saleItems;
    }

    public async Task<decimal> UpdateSaleItemsAsync(Guid saleId, IList<UpdateSaleItems> itemDtos, IList<SaleItem> existingItems, Dictionary<Guid, Product> productLookup)
    {
        var itemsToDelete = itemDtos.Where(x => x.ToDelete && x.Id.HasValue).ToList();

        await ProcessItemDeletions(itemsToDelete, existingItems);

        decimal totalAmount = 0;
        var itemsToProcess = itemDtos.Where(x => !x.ToDelete).ToList();

        foreach (var item in itemsToProcess)
        {
            var totalPrice = CalculateItemTotal(item.Quantity, item.UnitPrice, item.ProductId, productLookup);

            if (item.Id.HasValue)
            {
                await UpdateExistingItem(item, existingItems, productLookup);
            }
            else
            {
                await CreateNewItem(saleId, item, productLookup);
            }

            totalAmount += totalPrice;
        }

        return totalAmount;
    }

    public async Task DeleteSaleItemsAsync(IList<SaleItem> saleItems)
    {
        await _saleItemRepository.DeleteRangeAsync(saleItems.ToList());
    }

    public decimal CalculateTotalAmount(IList<CreateSaleItemCommand> items, Dictionary<Guid, Product> productLookup)
    {
        return items.Sum(item => CalculateItemTotal(item.Quantity, item.UnitPrice, item.ProductId, productLookup));
    }

    public decimal CalculateTotalAmount(IList<UpdateSaleItems> items, Dictionary<Guid, Product> productLookup)
    {
        return items.Where(x => !x.ToDelete).Sum(item => CalculateItemTotal(item.Quantity, item.UnitPrice, item.ProductId, productLookup));
    }

    private async Task ProcessItemDeletions(IList<UpdateSaleItems> itemsToDelete, IList<SaleItem> existingItems)
    {
        foreach (var itemToDelete in itemsToDelete)
        {
            var existingItem = existingItems?.FirstOrDefault(x => x.Id == itemToDelete.Id);
            if (existingItem != null)
            {
                await _saleItemRepository.DeleteAsync(existingItem);
            }
        }
    }

    private async Task UpdateExistingItem(UpdateSaleItems item, IList<SaleItem> existingItems, Dictionary<Guid, Product> productLookup)
    {
        var existingItem = existingItems?.FirstOrDefault(x => x.Id == item.Id);
        if (existingItem != null)
        {
            var unitPrice = GetEffectiveUnitPrice(item.UnitPrice, item.ProductId, productLookup);

            existingItem.ProductId = item.ProductId;
            existingItem.Quantity = item.Quantity;
            existingItem.UnitPrice = unitPrice;
            existingItem.TotalPrice = CalculateItemTotal(item.Quantity, unitPrice);
            await _saleItemRepository.UpdateAsync(existingItem);
        }
    }

    private async Task CreateNewItem(Guid saleId, UpdateSaleItems item, Dictionary<Guid, Product> productLookup)
    {
        var newSaleItem = CreateSaleItemFromUpdate(saleId, item, productLookup);
        await _saleItemRepository.AddAsync(newSaleItem);
    }

    private static SaleItem CreateSaleItem(Guid saleId, CreateSaleItemCommand item, Dictionary<Guid, Product> productLookup)
    {
        var unitPrice = GetEffectiveUnitPrice(item.UnitPrice, item.ProductId, productLookup);

        return new SaleItem
        {
            SaleId = saleId,
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            UnitPrice = unitPrice,
            TotalPrice = CalculateItemTotal(item.Quantity, unitPrice)
        };
    }

    private static SaleItem CreateSaleItemFromUpdate(Guid saleId, UpdateSaleItems item, Dictionary<Guid, Product> productLookup)
    {
        var unitPrice = GetEffectiveUnitPrice(item.UnitPrice, item.ProductId, productLookup);

        return new SaleItem
        {
            SaleId = saleId,
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            UnitPrice = unitPrice,
            TotalPrice = CalculateItemTotal(item.Quantity, unitPrice)
        };
    }

    private static decimal GetEffectiveUnitPrice(decimal? itemUnitPrice, Guid productId, Dictionary<Guid, Product> productLookup)
    {
        return itemUnitPrice ?? productLookup[productId].SalePrice;
    }

    private static decimal CalculateItemTotal(int quantity, decimal unitPrice) => quantity * unitPrice;

    private static decimal CalculateItemTotal(int quantity, decimal? unitPrice, Guid productId, Dictionary<Guid, Product> productLookup)
    {
        var effectiveUnitPrice = GetEffectiveUnitPrice(unitPrice, productId, productLookup);
        return CalculateItemTotal(quantity, effectiveUnitPrice);
    }
}