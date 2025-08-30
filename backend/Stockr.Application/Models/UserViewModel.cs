using Stockr.Domain.Enums;

namespace Stockr.Application.Models;

public class UserViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public UserRole Role { get; set; }
    public string RoleName { get; set; }
    public bool IsBlocked { get; set; }
    public DateTime? BlockedUntil { get; set; }
    public bool Active { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public DateTime? LastPasswordChange { get; set; }
    public int LoginAttempts { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}