using Microsoft.AspNetCore.Http;
using Stockr.Infrastructure.Interfaces;

namespace Stockr.Application.Services;

public interface ITenantService : ITenantContext
{
}

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private Guid? _tenantId;

    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? GetCurrentTenantId()
    {
        if (_tenantId.HasValue)
        {
            return _tenantId;
        }
        
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Items.ContainsKey("TenantId") == true)
        {
            return (Guid)httpContext.Items["TenantId"];
        }
        
        var tenantIdClaim = httpContext?.User?.FindFirst("TenantId");
        if (tenantIdClaim != null && Guid.TryParse(tenantIdClaim.Value, out var tenantId))
        {
            return tenantId;
        }

        return null;
    }

    public bool ValidateTenantAccess(Guid? entityTenantId)
    {
        var currentTenantId = GetCurrentTenantId();
        
        if (!currentTenantId.HasValue)
        {
            return false;
        }
        
        if (!entityTenantId.HasValue)
        {
            return false;
        }
        
        return entityTenantId.Value == currentTenantId.Value;
    }

    public void SetTenantId(Guid? tenantId)
    {
        _tenantId = tenantId;
    }
}