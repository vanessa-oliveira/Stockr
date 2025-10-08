using MediatR;
using Stockr.Application.Models;
using Stockr.Domain.Common;

namespace Stockr.Application.Queries.Inventory;

public class GetInventoriesPagedQuery : IRequest<PagedResult<InventoryViewModel>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}