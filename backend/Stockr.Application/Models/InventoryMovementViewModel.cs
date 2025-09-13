namespace Stockr.Application.Models;

public class InventoryMovementViewModel
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public Guid InventoryId { get; set; }
    public int Quantity { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public string? Reason { get; set; }
    public DateTime MovementDate { get; set; }
    public decimal? UnitCost { get; set; }
    public string? Notes { get; set; }
}