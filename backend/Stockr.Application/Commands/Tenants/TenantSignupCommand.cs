using MediatR;
using Stockr.Domain.Enums;
using AuthenticationResult = Stockr.Application.Services.AuthenticationResult;

namespace Stockr.Application.Commands.Tenants;

public class TenantSignupCommand : IRequest<AuthenticationResult>
{
    public string TenantName { get; set; }
    public PlanType PlanType { get; set; } = PlanType.Basic;
    public string AdminName { get; set; }
    public string AdminEmail { get; set; }
    public string Password { get; set; }
}