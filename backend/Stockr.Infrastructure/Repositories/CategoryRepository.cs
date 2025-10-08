using Microsoft.EntityFrameworkCore;
using Stockr.Domain.Common;
using Stockr.Domain.Entities;
using Stockr.Infrastructure.Context;

namespace Stockr.Infrastructure.Repositories;

public interface ICategoryRepository : IGenericRepository<Category>
{
    new Task<Category?> GetByIdAsync(Guid id);
    new Task<IEnumerable<Category>> GetAllAsync();
    new Task<PagedResult<Category>> GetPagedAsync(PaginationParams paginationParams);
}

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(DataContext context) : base(context)
    {
    }

    public new async Task<Category?> GetByIdAsync(Guid id)
    {
        return await _dbSet.AsNoTracking()
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public new async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking()
            .Include(c => c.Products)
            .ToListAsync();
    }

    public override async Task<PagedResult<Category>> GetPagedAsync(PaginationParams paginationParams)
    {
        var query = _dbSet.AsNoTracking()
            .Where(e => !e.Deleted)
            .Include(c => c.Products);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        return new PagedResult<Category>(items, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
    }
}