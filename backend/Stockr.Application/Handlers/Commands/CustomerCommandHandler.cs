using MediatR;
using Stockr.Application.Commands.Customers;
using Stockr.Application.Services;
using Stockr.Domain.Entities;
using Stockr.Infrastructure.Repositories;

namespace Stockr.Application.Handlers.Commands;

public class CustomerCommandHandler :
    IRequestHandler<CreateCustomerCommand, Unit>,
    IRequestHandler<UpdateCustomerCommand, Unit>,
    IRequestHandler<DeleteCustomerCommand, Unit>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ITenantService _tenantService;

    public CustomerCommandHandler(ICustomerRepository customerRepository, ITenantService tenantService)
    {
        _customerRepository = customerRepository;
        _tenantService = tenantService;
    }

    public async Task<Unit> Handle(CreateCustomerCommand command, CancellationToken cancellationToken)
    {
        var currentTenantId = _tenantService.GetCurrentTenantId();
        if (!currentTenantId.HasValue)
        {
            throw new UnauthorizedAccessException("User must belong to a tenant");
        }

        var customer = new Customer
        {
            Name = command.Name,
            Email = command.Email,
            Phone = command.Phone,
            CPF = command.CPF,
            CNPJ = command.CNPJ,
            TenantId = currentTenantId.Value
        };

        await _customerRepository.AddAsync(customer);
        return Unit.Value;
    }

    public async Task<Unit> Handle(UpdateCustomerCommand command, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(command.Id);
        if (customer == null)
        {
            throw new ArgumentException("Customer not found");
        }
        
        customer.Name = command.Name;
        customer.Email = command.Email;
        customer.Phone = command.Phone;
        customer.CPF = command.CPF;
        customer.CNPJ = command.CNPJ;
        
        await _customerRepository.UpdateAsync(customer);
        return Unit.Value;
    }

    public async Task<Unit> Handle(DeleteCustomerCommand command, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(command.Id);
        if (customer == null)
        {
            throw new ArgumentException("Customer not found");
        }
        
        await _customerRepository.DeleteAsync(customer);
        return Unit.Value;
    }
}