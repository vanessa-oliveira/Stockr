namespace Stockr.Domain.Entities;

public class Purchase : BaseEntity
{
    public Guid SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string? Notes { get; set; }
    public string InvoiceNumber { get; set; }
    public Guid? TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
}