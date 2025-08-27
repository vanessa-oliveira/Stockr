using Microsoft.EntityFrameworkCore;
using Stockr.Domain.Entities;
using Stockr.Infrastructure.Context;

namespace Stockr.Infrastructure.Repositories;

public interface ISupplierRepository : IGenericRepository<Supplier>
{
    Task<bool> HasProductsAsync(Guid supplierId);
}

public class SupplierRepository : GenericRepository<Supplier>, ISupplierRepository
{
    public SupplierRepository(DataContext context) : base(context)
    {
    }

    public async Task<bool> HasProductsAsync(Guid supplierId)
    {
        return await _context.Products.AsNoTracking()
            .AnyAsync(p => p.SupplierId == supplierId);
    }
}