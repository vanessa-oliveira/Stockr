using Microsoft.EntityFrameworkCore;
using Stockr.Domain.Entities;
using Stockr.Infrastructure.Context;

namespace Stockr.Infrastructure.Repositories;

public interface ICustomerRepository : IGenericRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(string email);
    Task<Customer?> GetByCpfAsync(string cpf);
    Task<Customer?> GetByCnpjAsync(string cnpj);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> CpfExistsAsync(string cpf);
    Task<bool> CnpjExistsAsync(string cnpj);
}

public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(DataContext context) : base(context)
    {
    }

    public async Task<Customer?> GetByEmailAsync(string email)
    {
        return await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Email == email);
    }

    public async Task<Customer?> GetByCpfAsync(string cpf)
    {
        return await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(c => c.CPF == cpf);
    }

    public async Task<Customer?> GetByCnpjAsync(string cnpj)
    {
        return await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(c => c.CNPJ == cnpj);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet.AsNoTracking()
            .AnyAsync(c => c.Email == email);
    }

    public async Task<bool> CpfExistsAsync(string cpf)
    {
        return await _dbSet.AsNoTracking()
            .AnyAsync(c => c.CPF == cpf);
    }

    public async Task<bool> CnpjExistsAsync(string cnpj)
    {
        return await _dbSet.AsNoTracking()
            .AnyAsync(c => c.CNPJ == cnpj);
    }
}