using Mapster;
using MediatR;
using Stockr.Application.Models;
using Stockr.Application.Queries.Customers;
using Stockr.Infrastructure.Repositories;

namespace Stockr.Application.Handlers.Queries.Customers;

public class CustomerQueryHandler :
    IRequestHandler<GetAllCustomersQuery, IEnumerable<CustomerViewModel>>,
    IRequestHandler<GetCustomerByIdQuery, CustomerViewModel?>
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<IEnumerable<CustomerViewModel>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        var customers = await _customerRepository.GetAllAsync();
        return customers.Adapt<IEnumerable<CustomerViewModel>>();
    }

    public async Task<CustomerViewModel?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id);
        return customer.Adapt<CustomerViewModel>();
    }
}