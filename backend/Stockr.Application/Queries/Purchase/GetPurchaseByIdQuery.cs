using MediatR;
using Stockr.Application.Models;

namespace Stockr.Application.Queries.Purchase;

public class GetPurchaseByIdQuery : IRequest<PurchaseViewModel?>
{
    public Guid Id { get; set; }
}