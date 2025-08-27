using Microsoft.EntityFrameworkCore;
using Stockr.Domain.Entities;
using Stockr.Infrastructure.Context;

namespace Stockr.Infrastructure.Repositories;

public interface IInventoryRepository : IGenericRepository<Inventory>
{
    Task<Inventory?> GetByProductIdAsync(Guid productId);
    Task<bool> UpdateStockAsync(Guid productId, int quantity);
    Task<bool> AddStockAsync(Guid productId, int quantity);
    Task<bool> RemoveStockAsync(Guid productId, int quantity);
}

public class InventoryRepository : GenericRepository<Inventory>, IInventoryRepository
{
    public InventoryRepository(DataContext context) : base(context)
    {
    }

    public async Task<Inventory?> GetByProductIdAsync(Guid productId)
    {
        return await _dbSet.AsNoTracking()
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.ProductId == productId);
    }

    public async Task<bool> UpdateStockAsync(Guid productId, int quantity)
    {
        var inventory = await _dbSet.FirstOrDefaultAsync(i => i.ProductId == productId);
        if (inventory == null) return false;

        inventory.Quantity = quantity;
        _dbSet.Update(inventory);
        
        return await SaveChanges();
    }

    public async Task<bool> AddStockAsync(Guid productId, int quantity)
    {
        var inventory = await _dbSet.FirstOrDefaultAsync(i => i.ProductId == productId);
        if (inventory == null) return false;

        inventory.Quantity += quantity;
        _dbSet.Update(inventory);
        
        return await SaveChanges();
    }

    public async Task<bool> RemoveStockAsync(Guid productId, int quantity)
    {
        var inventory = await _dbSet.FirstOrDefaultAsync(i => i.ProductId == productId);
        if (inventory == null || inventory.Quantity < quantity) return false;

        inventory.Quantity -= quantity;
        _dbSet.Update(inventory);
        
        return await SaveChanges();
    }

    private async Task<bool> SaveChanges()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}