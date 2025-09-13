﻿using Microsoft.EntityFrameworkCore;
using MoreLinq;
using Stockr.Domain.Entities;
using Stockr.Infrastructure.Context;

namespace Stockr.Infrastructure.Repositories;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> GetAllActiveAsync();
    Task<bool> AddAsync(T entity);
    Task<bool> AddRangeAsync(IList<T> entities);
    Task<bool> UpdateAsync(T entity);
    Task<bool> UpdateRangeAsync(IList<T> entities);
    Task<bool> DeleteAsync(T entity);
    public Task<bool> DeleteRangeAsync(IList<T> entities);
}

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly DataContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(DataContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<T>> GetAllActiveAsync()
    {
        return await _dbSet.AsNoTracking().Where(e => e.Active).ToListAsync();
    }

    public async Task<bool> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return await SaveChanges();
    }

    public async Task<bool> AddRangeAsync(IList<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
        return await SaveChanges();
    }

    public async Task<bool> UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        return await SaveChanges();
    }

    public async Task<bool> UpdateRangeAsync(IList<T> entities)
    {
        _dbSet.UpdateRange(entities);
        return await SaveChanges();
    }

    public async Task<bool> DeleteAsync(T entity)
    {
        entity.Deleted = true;
        _dbSet.Update(entity);
        return await SaveChanges();
    }
    
    public async Task<bool> DeleteRangeAsync(IList<T> entities)
    {
        entities.ForEach(x => x.Deleted = true);
        _dbSet.UpdateRange(entities);
        return await SaveChanges();
    }

    private async Task<bool> SaveChanges()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}