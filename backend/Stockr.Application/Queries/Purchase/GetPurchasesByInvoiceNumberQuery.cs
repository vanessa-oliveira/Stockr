using MediatR;
using Stockr.Application.Models;

namespace Stockr.Application.Queries.Purchase;

public class GetPurchasesByInvoiceNumberQuery : IRequest<IEnumerable<PurchaseViewModel>>
{
    public string InvoiceNumber { get; set; }
}