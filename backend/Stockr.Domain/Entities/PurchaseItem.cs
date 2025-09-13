namespace Stockr.Domain.Entities;

public class PurchaseItem : BaseEntity
{
    public Guid PurchaseId { get; set; }
    public Purchase? Purchase { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public Guid? TenantId { get; set; }
    public Tenant? Tenant { get; set; }
}