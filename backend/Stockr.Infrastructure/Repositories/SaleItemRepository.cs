using Microsoft.EntityFrameworkCore;
using Stockr.Domain.Entities;
using Stockr.Infrastructure.Context;

namespace Stockr.Infrastructure.Repositories;

public interface ISaleItemRepository : IGenericRepository<SaleItem>
{
    Task<IEnumerable<SaleItem>> GetBySaleAsync(Guid saleId);
    Task<IEnumerable<SaleItem>> GetByProductAsync(Guid productId);
    Task<decimal> GetTotalSoldByProductAsync(Guid productId, DateTime startDate, DateTime endDate);
    Task<int> GetQuantitySoldByProductAsync(Guid productId, DateTime startDate, DateTime endDate);
}

public class SaleItemRepository : GenericRepository<SaleItem>, ISaleItemRepository
{
    public SaleItemRepository(DataContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SaleItem>> GetBySaleAsync(Guid saleId)
    {
        return await _dbSet.AsNoTracking()
            .Include(si => si.Product)
            .Where(si => si.SaleId == saleId)
            .ToListAsync();
    }

    public async Task<IEnumerable<SaleItem>> GetByProductAsync(Guid productId)
    {
        return await _dbSet.AsNoTracking()
            .Include(si => si.Sale)
            .Include(si => si.Product)
            .Where(si => si.ProductId == productId)
            .OrderByDescending(si => si.CreatedAt)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalSoldByProductAsync(Guid productId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet.AsNoTracking()
            .Where(si => si.ProductId == productId && si.CreatedAt >= startDate && si.CreatedAt <= endDate)
            .SumAsync(si => si.TotalPrice);
    }

    public async Task<int> GetQuantitySoldByProductAsync(Guid productId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet.AsNoTracking()
            .Where(si => si.ProductId == productId && si.CreatedAt >= startDate && si.CreatedAt <= endDate)
            .SumAsync(si => si.Quantity);
    }
}