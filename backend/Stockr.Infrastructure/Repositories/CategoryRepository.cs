using Microsoft.EntityFrameworkCore;
using Stockr.Domain.Entities;
using Stockr.Infrastructure.Context;

namespace Stockr.Infrastructure.Repositories;

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<Category?> GetWithProductsAsync(Guid id);
}

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(DataContext context) : base(context)
    {
    }

    public async Task<Category?> GetWithProductsAsync(Guid id)
    {
        return await _dbSet.AsNoTracking()
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}