namespace Stockr.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; }
    public string? SKU { get; set; }
    public string Description { get; set; }
    public Guid CategoryId { get; set; }
    public Category Category { get; set; }
    public Guid? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }
    public Guid? TenantId { get; set; }
    public Tenant? Tenant { get; set; }
}