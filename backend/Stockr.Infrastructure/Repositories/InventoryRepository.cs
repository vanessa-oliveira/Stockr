using Microsoft.EntityFrameworkCore;
using Stockr.Domain.Entities;
using Stockr.Infrastructure.Context;

namespace Stockr.Infrastructure.Repositories;

public interface IInventoryRepository : IGenericRepository<Inventory>
{
    new Task<Inventory?> GetByIdAsync(Guid id);
    new Task<IEnumerable<Inventory>> GetAllAsync();
    Task<Inventory?> GetByProductIdAsync(Guid productId);
    Task<List<Inventory>> GetByProductIdsAsync(List<Guid> productIds);
}

public class InventoryRepository : GenericRepository<Inventory>, IInventoryRepository
{
    public InventoryRepository(DataContext context) : base(context)
    {
    }

    public new async Task<Inventory?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(i => i.Product)
            .Include(i => i.Movements)
            .FirstOrDefaultAsync(i => i.Id == id);
    }
    
    public new async Task<IEnumerable<Inventory>> GetAllAsync()
    {
        return await _dbSet
            .Include(i => i.Product)
            .Include(i => i.Movements)
            .ToListAsync();
    }

    public async Task<Inventory?> GetByProductIdAsync(Guid productId)
    {
        return await _dbSet
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.ProductId == productId);
    }
    
    public async Task<List<Inventory>> GetByProductIdsAsync(List<Guid> productIds)
    {
        if (!productIds.Any())
            return new List<Inventory>();

        return await _context.Inventories
            .Where(i => productIds.Contains(i.ProductId))
            .ToListAsync();
    }
}