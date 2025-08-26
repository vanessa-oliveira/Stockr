using Stockr.Domain.Enums;

namespace Stockr.Domain.Entities;

public class InventoryMovement : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; }
    public Guid? TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public int Quantity { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    public MovementType MovementType { get; set; }
}