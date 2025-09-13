using MediatR;
using Stockr.Application.Models;

namespace Stockr.Application.Queries.Purchase;

public class GetPurchasesByPeriodQuery : IRequest<IEnumerable<PurchaseViewModel>>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}