namespace Stockr.Domain.Entities;

public class BaseEntity
{
    public Guid Id { get; set; }
    public bool Active { get; set; } = true;
    public bool Deleted { get; set; } = false;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}