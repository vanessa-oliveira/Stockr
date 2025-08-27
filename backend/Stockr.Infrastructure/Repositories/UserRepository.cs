using Microsoft.EntityFrameworkCore;
using Stockr.Domain.Entities;
using Stockr.Infrastructure.Context;

namespace Stockr.Infrastructure.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByEmailAndPasswordAsync(string email, string password);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> BlockUserAsync(Guid userId, DateTime? blockedUntil = null);
    Task<bool> UnblockUserAsync(Guid userId);
}

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(DataContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByEmailAndPasswordAsync(string email, string password)
    {
        return await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet.AsNoTracking()
            .AnyAsync(u => u.Email == email);
    }

    public async Task<bool> BlockUserAsync(Guid userId, DateTime? blockedUntil = null)
    {
        var user = await _dbSet.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

        user.IsBlocked = true;
        user.BlockedUntil = blockedUntil;
        _dbSet.Update(user);
        
        return await SaveChanges();
    }

    public async Task<bool> UnblockUserAsync(Guid userId)
    {
        var user = await _dbSet.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

        user.IsBlocked = false;
        user.BlockedUntil = null;
        user.LoginAttempts = 0;
        _dbSet.Update(user);
        
        return await SaveChanges();
    }

    private async Task<bool> SaveChanges()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}