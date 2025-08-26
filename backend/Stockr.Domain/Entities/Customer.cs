namespace Stockr.Domain.Entities;

public class Customer : BaseEntity
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string? CPF { get; set; }
    public string? CNPJ { get; set; }
    public Guid? TenantId { get; set; }
    public Tenant? Tenant { get; set; }
}