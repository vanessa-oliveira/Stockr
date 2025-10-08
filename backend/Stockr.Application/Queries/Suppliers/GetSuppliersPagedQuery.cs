using MediatR;
using Stockr.Application.Models;
using Stockr.Domain.Common;
using Stockr.Domain.Entities;

namespace Stockr.Application.Queries.Suppliers;

public class GetSuppliersPagedQuery : IRequest<PagedResult<SupplierViewModel>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}