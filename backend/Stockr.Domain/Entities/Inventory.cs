namespace Stockr.Domain.Entities;

public class Inventory : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; }
    public int Quantity { get; set; }
    public Guid? TenantId { get; set; }
    public Tenant? Tenant { get; set; }
}