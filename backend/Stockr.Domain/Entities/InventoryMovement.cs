using Stockr.Domain.Enums;

namespace Stockr.Domain.Entities;

public class InventoryMovement : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; }
    public Guid InventoryId { get; set; }
    public Inventory Inventory { get; set; }
    public Guid? TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public int Quantity { get; set; }
    public Guid? UserId { get; set; }
    public User User { get; set; }
    public MovementDirection Direction { get; set; }
    public string? Reason { get; set; }
    public DateTime MovementDate { get; set; } = DateTime.UtcNow;
    public decimal? UnitCost { get; set; }
    public string? Notes { get; set; }
    public Guid? SaleId { get; set; }
    public Sale? Sale { get; set; }
    public Guid? PurchaseId { get; set; }
    public Purchase? Purchase { get; set; }
}