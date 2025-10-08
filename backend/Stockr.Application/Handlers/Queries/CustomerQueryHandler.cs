using Mapster;
using MediatR;
using Stockr.Application.Models;
using Stockr.Application.Queries.Customers;
using Stockr.Domain.Common;
using Stockr.Infrastructure.Repositories;

namespace Stockr.Application.Handlers.Queries;

public class CustomerQueryHandler :
    IRequestHandler<GetCustomerByIdQuery, CustomerViewModel?>,
    IRequestHandler<GetCustomersPagedQuery, PagedResult<CustomerViewModel>>
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<CustomerViewModel?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id);
        return customer.Adapt<CustomerViewModel>();
    }

    public async Task<PagedResult<CustomerViewModel>> Handle(GetCustomersPagedQuery request, CancellationToken cancellationToken)
    {
        var paginationParams = new PaginationParams
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
        
        var customers = await _customerRepository.GetPagedAsync(paginationParams);
        return customers.Adapt<PagedResult<CustomerViewModel>>();
    }
}