using MediatR;
using Stockr.Application.Models;
using Stockr.Domain.Common;

namespace Stockr.Application.Queries.Sales;

public class GetSalesPagedQuery : IRequest<PagedResult<SaleViewModel>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}