using Microsoft.EntityFrameworkCore;
using Stockr.Domain.Common;
using Stockr.Domain.Entities;
using Stockr.Domain.Enums;
using Stockr.Infrastructure.Context;

namespace Stockr.Infrastructure.Repositories;

public interface ISaleRepository : IGenericRepository<Sale>
{
    new Task<IEnumerable<Sale>> GetAllAsync();
    new Task<PagedResult<Sale>> GetPagedAsync(PaginationParams paginationParams);
    Task<IEnumerable<Sale>> GetByCustomerAsync(Guid customerId);
    Task<IEnumerable<Sale>> GetBySalespersonAsync(Guid userId);
    Task<IEnumerable<Sale>> GetByStatusAsync(SaleStatus status);
    Task<IEnumerable<Sale>> GetByPeriodAsync(DateTime startDate, DateTime endDate);
    Task<Sale?> GetWithItemsAsync(Guid id);
    Task<decimal> GetTotalSalesByPeriodAsync(DateTime startDate, DateTime endDate);
}

public class SaleRepository : GenericRepository<Sale>, ISaleRepository
{
    public SaleRepository(DataContext context) : base(context)
    {
    }

    public new async Task<IEnumerable<Sale>> GetAllAsync()
    {
        var sales = await _dbSet.AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.Salesperson)
            .Include(s => s.SaleItems).ToListAsync();
        return sales;
    }

    public async Task<IEnumerable<Sale>> GetByCustomerAsync(Guid customerId)
    {
        return await _dbSet.AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.Salesperson)
            .Where(s => s.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Sale>> GetBySalespersonAsync(Guid userId)
    {
        return await _dbSet.AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.Salesperson)
            .Where(s => s.SalesPersonId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Sale>> GetByStatusAsync(SaleStatus status)
    {
        return await _dbSet.AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.Salesperson)
            .Where(s => s.SaleStatus == status)
            .ToListAsync();
    }

    public async Task<IEnumerable<Sale>> GetByPeriodAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet.AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.Salesperson)
            .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate)
            .ToListAsync();
    }

    public async Task<Sale?> GetWithItemsAsync(Guid id)
    {
        return await _dbSet.AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.Salesperson)
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<decimal> GetTotalSalesByPeriodAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet.AsNoTracking()
            .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate
                       && s.SaleStatus == SaleStatus.Confirmed)
            .SumAsync(s => s.TotalAmount);
    }

    public override async Task<PagedResult<Sale>> GetPagedAsync(PaginationParams paginationParams)
    {
        var query = _dbSet.AsNoTracking()
            .Where(e => !e.Deleted)
            .Include(s => s.Customer)
            .Include(s => s.Salesperson)
            .Include(s => s.SaleItems)
            .OrderByDescending(s => s.SaleDate);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        return new PagedResult<Sale>(items, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
    }
}