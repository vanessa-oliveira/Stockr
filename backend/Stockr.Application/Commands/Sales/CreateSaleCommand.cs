using MediatR;
using Stockr.Domain.Enums;

namespace Stockr.Application.Commands.Sales;

public class CreateSaleCommand : IRequest<Unit>
{
    public Guid? CustomerId { get; set; }
    public Guid? SalespersonId { get; set; }
    public string SaleStatus { get; set; } = "Pending";
    public DateTime? SaleDate { get; set; }
    public IList<CreateSaleItemCommand> SaleItems { get; set; } = new List<CreateSaleItemCommand>();
}

public class CreateSaleItemCommand
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
}