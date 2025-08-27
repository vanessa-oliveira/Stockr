using Microsoft.EntityFrameworkCore;
using Stockr.Domain.Entities;
using Stockr.Infrastructure.Context;

namespace Stockr.Infrastructure.Repositories;

public interface ICategoryRepository : IGenericRepository<Category>
{
    new Task<Category?> GetByIdAsync(Guid id);
    new Task<IEnumerable<Category>> GetAllAsync();
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
}