using MediatR;

namespace Stockr.Application.Commands.Purchase;

public class CreatePurchaseCommand : IRequest<Unit>
{
    public Guid SupplierId { get; set; }
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public string InvoiceNumber { get; set; }
    public IList<CreatePurchaseItemCommand> PurchaseItems { get; set; } = new List<CreatePurchaseItemCommand>();
}

public class CreatePurchaseItemCommand
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}