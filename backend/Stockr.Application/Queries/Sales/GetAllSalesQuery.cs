using MediatR;
using Stockr.Application.Models;

namespace Stockr.Application.Queries.Sales;

public class GetAllSalesQuery : IRequest<IEnumerable<SaleViewModel>>
{
}