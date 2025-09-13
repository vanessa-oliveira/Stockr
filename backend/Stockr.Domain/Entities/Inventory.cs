namespace Stockr.Domain.Entities;

public class Inventory : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; }
    public int MinStock { get; set; }
    public int CurrentStock { get; set; }
    public Guid? TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public ICollection<InventoryMovement> Movements { get; set; } = new List<InventoryMovement>();
}