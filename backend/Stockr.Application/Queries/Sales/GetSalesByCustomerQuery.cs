using MediatR;
using Stockr.Application.Models;

namespace Stockr.Application.Queries.Sales;

public class GetSalesByCustomerQuery : IRequest<IEnumerable<SaleViewModel>>
{
    public Guid CustomerId { get; set; }
}