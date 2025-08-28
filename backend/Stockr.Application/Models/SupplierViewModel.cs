namespace Stockr.Application.Models;

public class SupplierViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public bool Active { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}