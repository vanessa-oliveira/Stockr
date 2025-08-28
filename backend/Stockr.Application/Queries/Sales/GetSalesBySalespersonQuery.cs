using MediatR;
using Stockr.Application.Models;

namespace Stockr.Application.Queries.Sales;

public class GetSalesBySalespersonQuery : IRequest<IEnumerable<SaleViewModel>>
{
    public Guid UserId { get; set; }
}