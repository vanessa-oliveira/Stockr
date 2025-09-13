using MediatR;

namespace Stockr.Application.Commands.Purchase;

public class UpdatePurchaseCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public Guid? UserId { get; set; }
    public string? Notes { get; set; }
    public string InvoiceNumber { get; set; }
    public IList<UpdatePurchaseItemCommand> PurchaseItems { get; set; } = new List<UpdatePurchaseItemCommand>();
}

public class UpdatePurchaseItemCommand
{
    public Guid? Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public bool ToDelete { get; set; }
}