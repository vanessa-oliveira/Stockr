using Microsoft.EntityFrameworkCore;
using Stockr.Domain.Entities;
using Stockr.Infrastructure.Context;

namespace Stockr.Infrastructure.Repositories;

public interface IPurchaseRepository : IGenericRepository<Purchase>
{
    new Task<IEnumerable<Purchase>> GetAllAsync();
    Task<IEnumerable<Purchase>> GetBySupplierAsync(Guid supplierId);
    Task<IEnumerable<Purchase>> GetByPeriodAsync(DateTime startDate, DateTime endDate);
    Task<Purchase?> GetWithItemsAsync(Guid id);
    Task<decimal> GetTotalPurchasesByPeriodAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Purchase>> GetByInvoiceNumberAsync(string invoiceNumber);
}

public class PurchaseRepository : GenericRepository<Purchase>, IPurchaseRepository
{
    public PurchaseRepository(DataContext context) : base(context)
    {
    }

    public new async Task<IEnumerable<Purchase>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.PurchaseItems)
            .OrderByDescending(p => p.PurchaseDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Purchase>> GetBySupplierAsync(Guid supplierId)
    {
        return await _dbSet.AsNoTracking()
            .Include(p => p.Supplier)
            .Where(p => p.SupplierId == supplierId)
            .OrderByDescending(p => p.PurchaseDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Purchase>> GetByPeriodAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet.AsNoTracking()
            .Include(p => p.Supplier)
            .Where(p => p.PurchaseDate >= startDate && p.PurchaseDate <= endDate)
            .OrderByDescending(p => p.PurchaseDate)
            .ToListAsync();
    }

    public async Task<Purchase?> GetWithItemsAsync(Guid id)
    {
        return await _dbSet.AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.PurchaseItems)
                .ThenInclude(pi => pi.Product)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<decimal> GetTotalPurchasesByPeriodAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet.AsNoTracking()
            .Where(p => p.PurchaseDate >= startDate && p.PurchaseDate <= endDate)
            .SumAsync(p => p.TotalAmount);
    }

    public async Task<IEnumerable<Purchase>> GetByInvoiceNumberAsync(string invoiceNumber)
    {
        return await _dbSet.AsNoTracking()
            .Include(p => p.Supplier)
            .Where(p => p.InvoiceNumber.Contains(invoiceNumber))
            .OrderByDescending(p => p.PurchaseDate)
            .ToListAsync();
    }
}