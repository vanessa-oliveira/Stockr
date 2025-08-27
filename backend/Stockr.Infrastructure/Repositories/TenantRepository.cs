using Microsoft.EntityFrameworkCore;
using Stockr.Domain.Entities;
using Stockr.Domain.Enums;
using Stockr.Infrastructure.Context;

namespace Stockr.Infrastructure.Repositories;

public interface ITenantRepository : IGenericRepository<Tenant>
{
    Task<Tenant?> GetByDomainAsync(string domain);
    Task<bool> DomainExistsAsync(string domain);
    Task<IEnumerable<Tenant>> GetByPlanTypeAsync(PlanType planType);
}

public class TenantRepository : GenericRepository<Tenant>, ITenantRepository
{
    public TenantRepository(DataContext context) : base(context)
    {
    }

    public async Task<Tenant?> GetByDomainAsync(string domain)
    {
        return await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Domain == domain);
    }

    public async Task<bool> DomainExistsAsync(string domain)
    {
        return await _dbSet.AsNoTracking()
            .AnyAsync(t => t.Domain == domain);
    }

    public async Task<IEnumerable<Tenant>> GetByPlanTypeAsync(PlanType planType)
    {
        return await _dbSet.AsNoTracking()
            .Where(t => t.PlanType == planType)
            .ToListAsync();
    }
}