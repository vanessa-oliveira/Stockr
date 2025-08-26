using Stockr.Domain.Enums;

namespace Stockr.Domain.Entities;

public class Sale : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; }
    public Guid UserId { get; set; }
    public User Salesperson { get; set; }
    public SaleStatus SaleStatus { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime SaleDate { get; set; }
    public Guid? TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
}