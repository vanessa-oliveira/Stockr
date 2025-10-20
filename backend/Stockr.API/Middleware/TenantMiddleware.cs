using System.Security.Claims;
using Stockr.Application.Services;

namespace Stockr.API.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;

    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
    {
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = context.User.FindFirst("TenantId");

            if (tenantIdClaim != null && Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            {
                context.Items["TenantId"] = tenantId;
                tenantService.SetTenantId(tenantId);

                _logger.LogDebug("Tenant context set: {TenantId}", tenantId);
            }
            else
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogWarning("Authenticated user {UserId} has no TenantId claim", userId);
            }
        }

        await _next(context);
    }
}