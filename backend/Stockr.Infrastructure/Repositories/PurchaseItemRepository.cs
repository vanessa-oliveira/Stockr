using Microsoft.EntityFrameworkCore;
using Stockr.Domain.Entities;
using Stockr.Infrastructure.Context;

namespace Stockr.Infrastructure.Repositories;

public interface IPurchaseItemRepository : IGenericRepository<PurchaseItem>
{
    Task<IList<PurchaseItem>> GetByPurchaseAsync(Guid purchaseId);
}

public class PurchaseItemRepository : GenericRepository<PurchaseItem>, IPurchaseItemRepository
{
    public PurchaseItemRepository(DataContext context) : base(context)
    {
    }

    public async Task<IList<PurchaseItem>> GetByPurchaseAsync(Guid purchaseId)
    {
        return await _dbSet.AsNoTracking()
            .Include(pi => pi.Product)
            .Where(pi => pi.PurchaseId == purchaseId)
            .ToListAsync();
    }
}