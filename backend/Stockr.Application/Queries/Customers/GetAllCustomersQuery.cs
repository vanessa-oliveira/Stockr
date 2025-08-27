using MediatR;
using Stockr.Application.Models;

namespace Stockr.Application.Queries.Customers;

public class GetAllCustomersQuery : IRequest<IEnumerable<CustomerViewModel>>
{
}