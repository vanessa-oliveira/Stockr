using MediatR;
using Stockr.Application.Models;

namespace Stockr.Application.Queries.Purchase;

public class GetPurchasesBySupplierQuery : IRequest<IEnumerable<PurchaseViewModel>>
{
    public Guid SupplierId { get; set; }
}