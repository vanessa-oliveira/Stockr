using Serilog;

namespace Stockr.Infrastructure.Services;

public class NoCacheService : ICacheService
{
    private readonly ILogger _logger;

    public NoCacheService(ILogger logger)
    {
        _logger = logger.ForContext<NoCacheService>();
    }

    public Task<T?> GetAsync<T>(string key) where T : class
    {
        _logger.Debug("Cache desabilitado - retornando null para chave: {Key}", key);
        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        _logger.Debug("Cache desabilitado - ignorando set para chave: {Key}", key);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _logger.Debug("Cache desabilitado - ignorando remoção para chave: {Key}", key);
        return Task.CompletedTask;
    }

    public Task RemoveByPatternAsync(string pattern)
    {
        _logger.Debug("Cache desabilitado - ignorando remoção por padrão: {Pattern}", pattern);
        return Task.CompletedTask;
    }
}