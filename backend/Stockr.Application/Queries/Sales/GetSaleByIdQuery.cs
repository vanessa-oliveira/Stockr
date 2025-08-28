using MediatR;
using Stockr.Application.Models;

namespace Stockr.Application.Queries.Sales;

public class GetSaleByIdQuery : IRequest<SaleViewModel?>
{
    public Guid Id { get; set; }
}