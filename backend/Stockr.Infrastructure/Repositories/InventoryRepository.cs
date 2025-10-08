using Microsoft.EntityFrameworkCore;
using Stockr.Domain.Common;
using Stockr.Domain.Entities;
using Stockr.Infrastructure.Context;

namespace Stockr.Infrastructure.Repositories;

public interface IInventoryRepository : IGenericRepository<Inventory>
{
    new Task<Inventory?> GetByIdAsync(Guid id);
    new Task<IEnumerable<Inventory>> GetAllAsync();
    new Task<PagedResult<Inventory>> GetPagedAsync(PaginationParams paginationParams);
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
    
    public new async Task<PagedResult<Inventory>> GetPagedAsync(PaginationParams paginationParams)
    {
        var query = _dbSet.AsNoTracking()
            .Where(e => !e.Deleted)
            .Include(p => p.Product);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        return new PagedResult<Inventory>(items, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
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