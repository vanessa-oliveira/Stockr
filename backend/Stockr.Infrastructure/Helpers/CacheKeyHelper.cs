namespace Stockr.Infrastructure.Helpers;

public static class CacheKeyHelper
{
    private const string Prefix = "stockr:";
    
    public static string ProductById(Guid tenantId, Guid productId) =>
        $"{Prefix}tenant:{tenantId}:product:{productId}";

    public static string ProductsList(Guid tenantId) =>
        $"{Prefix}tenant:{tenantId}:products:all";

    public static string ProductsPaged(Guid tenantId, int pageNumber, int pageSize) =>
        $"{Prefix}tenant:{tenantId}:products:page:{pageNumber}:size:{pageSize}";

    public static string ProductsPattern(Guid tenantId) =>
        $"{Prefix}tenant:{tenantId}:product*";
    
}