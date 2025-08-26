using Stockr.Domain.Enums;

namespace Stockr.Domain.Entities;

public class User : BaseEntity
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public UserRole Role { get; set; }
    public bool IsBlocked { get; set; } = false;
    public DateTime? BlockedUntil { get; set; }
    public Guid? TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public DateTime? LastPasswordChange { get; set; }
    public int LoginAttempts { get; set; } = 0;
    public DateTime? LastFailedLogin { get; set; }
}