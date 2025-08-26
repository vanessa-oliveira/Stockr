namespace Stockr.Domain.Entities;

public class Supplier : BaseEntity
{
    public string Name { get; set; }
    public string Phone { get; set; }
    public Guid? TenantId { get; set; }
    public Tenant? Tenant { get; set; }
}