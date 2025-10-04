namespace Stockr.Infrastructure.Interfaces;

public interface ITenantContext
{
    Guid? GetCurrentTenantId();
    bool ValidateTenantAccess(Guid? entityTenantId);
    void SetTenantId(Guid? tenantId);
}