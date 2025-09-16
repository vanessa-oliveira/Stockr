using MediatR;

namespace Stockr.Application.Commands.Products;

public class CreateProductCommand : IRequest<Unit>
{
    public string Name { get; set; }
    public string SKU { get; set; }
    public string Description { get; set; }
    public Guid CategoryId { get; set; }
    public Guid? SupplierId { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }
}