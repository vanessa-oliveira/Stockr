using MediatR;
using Stockr.Application.Models;

namespace Stockr.Application.Queries.Purchase;

public class GetAllPurchasesQuery : IRequest<IEnumerable<PurchaseViewModel>>
{
}