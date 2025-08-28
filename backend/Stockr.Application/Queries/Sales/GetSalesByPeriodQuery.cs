using MediatR;
using Stockr.Application.Models;

namespace Stockr.Application.Queries.Sales;

public class GetSalesByPeriodQuery : IRequest<IEnumerable<SaleViewModel>>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}