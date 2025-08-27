using MediatR;
using Stockr.Application.Models;

namespace Stockr.Application.Queries.Customers;

public class GetCustomerByIdQuery : IRequest<CustomerViewModel?>
{
    public Guid Id { get; set; }
}