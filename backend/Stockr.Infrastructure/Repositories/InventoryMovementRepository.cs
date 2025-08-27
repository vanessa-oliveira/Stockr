using Microsoft.EntityFrameworkCore;
using Stockr.Domain.Entities;
using Stockr.Domain.Enums;
using Stockr.Infrastructure.Context;

namespace Stockr.Infrastructure.Repositories;

public interface IInventoryMovementRepository : IGenericRepository<InventoryMovement>
{
    Task<IEnumerable<InventoryMovement>> GetByProductAsync(Guid productId);
    Task<IEnumerable<InventoryMovement>> GetByUserAsync(Guid userId);
    Task<IEnumerable<InventoryMovement>> GetByMovementTypeAsync(MovementType movementType);
    Task<IEnumerable<InventoryMovement>> GetByPeriodAsync(DateTime startDate, DateTime endDate);
}

public class InventoryMovementRepository : GenericRepository<InventoryMovement>, IInventoryMovementRepository
{
    public InventoryMovementRepository(DataContext context) : base(context)
    {
    }

    public async Task<IEnumerable<InventoryMovement>> GetByProductAsync(Guid productId)
    {
        return await _dbSet.AsNoTracking()
            .Include(im => im.Product)
            .Include(im => im.User)
            .Where(im => im.ProductId == productId)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryMovement>> GetByUserAsync(Guid userId)
    {
        return await _dbSet.AsNoTracking()
            .Include(im => im.Product)
            .Include(im => im.User)
            .Where(im => im.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryMovement>> GetByMovementTypeAsync(MovementType movementType)
    {
        return await _dbSet.AsNoTracking()
            .Include(im => im.Product)
            .Include(im => im.User)
            .Where(im => im.MovementType == movementType)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryMovement>> GetByPeriodAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet.AsNoTracking()
            .Include(im => im.Product)
            .Include(im => im.User)
            .Where(im => im.CreatedAt >= startDate && im.CreatedAt <= endDate)
            .ToListAsync();
    }
}