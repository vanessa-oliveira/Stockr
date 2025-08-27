using Microsoft.EntityFrameworkCore;
using Stockr.Domain.Entities;
using Stockr.Infrastructure.Context;

namespace Stockr.Infrastructure.Repositories;

public interface IProductRepository : IGenericRepository<Product>
{
    new Task<Product?> GetByIdAsync(Guid id);
    new Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetBySkuAsync(string sku);
    Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId);
    Task<IEnumerable<Product>> GetBySupplierAsync(Guid supplierId);
    Task<bool> SkuExistsAsync(string sku);
}

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(DataContext context) : base(context)
    {
    }

    public new async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _dbSet.AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public new async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .ToListAsync();
    }

    public async Task<Product?> GetBySkuAsync(string sku)
    {
        return await _dbSet.AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.SKU == sku);
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId)
    {
        return await _dbSet.AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetBySupplierAsync(Guid supplierId)
    {
        return await _dbSet.AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Where(p => p.SupplierId == supplierId)
            .ToListAsync();
    }

    public async Task<bool> SkuExistsAsync(string sku)
    {
        return await _dbSet.AsNoTracking()
            .AnyAsync(p => p.SKU == sku);
    }
}