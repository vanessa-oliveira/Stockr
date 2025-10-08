using MediatR;
using Stockr.Application.Models;
using Stockr.Domain.Common;

namespace Stockr.Application.Queries.Purchase;

public class GetPurchasesPagedQuery : IRequest<PagedResult<PurchaseViewModel>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}