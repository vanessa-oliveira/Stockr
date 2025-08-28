using MediatR;
using Stockr.Domain.Enums;

namespace Stockr.Application.Commands.Sales;

public class UpdateSaleCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid SalesPersonId { get; set; }
    public string SaleStatus { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime SaleDate { get; set; }
    public IList<UpdateSaleItems> SaleItems { get; set; } = new List<UpdateSaleItems>();
}

public class UpdateSaleItems
{
    public Guid? Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public bool ToDelete { get; set; } = false;
}