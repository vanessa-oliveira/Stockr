namespace Stockr.Application.Models;

public class ProductViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string SKU { get; set; }
    public string Description { get; set; }
    public CategoryViewModel? Category { get; set; }
    public SupplierViewModel? Supplier { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }
    public int MinStock { get; set; }
}