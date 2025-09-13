namespace Stockr.Application.Models;

public class InventoryViewModel
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public int MinStock { get; set; }
    public int CurrentStock { get; set; }
}