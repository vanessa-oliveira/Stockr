using Microsoft.EntityFrameworkCore;
using Stockr.Domain.Entities;
using Stockr.Infrastructure.Context;

namespace Stockr.Infrastructure.Repositories;

public interface ISaleItemRepository : IGenericRepository<SaleItem>
{
    Task<IList<SaleItem>> GetBySaleAsync(Guid saleId);
}

public class SaleItemRepository : GenericRepository<SaleItem>, ISaleItemRepository
{
    public SaleItemRepository(DataContext context) : base(context)
    {
    }

    public async Task<IList<SaleItem>> GetBySaleAsync(Guid saleId)
    {
        return await _dbSet.AsNoTracking()
            .Include(si => si.Product)
            .Where(si => si.SaleId == saleId)
            .ToListAsync();
    }
}