using StackExchange.Redis;
using System.Text.Json;
using Serilog;

namespace Stockr.Infrastructure.Services;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
    Task RemoveAsync(string key);
    Task RemoveByPatternAsync(string pattern);
}

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _redis;
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly ILogger _logger;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(10);

    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer, ILogger logger)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _redis = connectionMultiplexer.GetDatabase();
        _logger = logger.ForContext<RedisCacheService>();
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            var value = await _redis.StringGetAsync(key);

            if (!value.HasValue)
            {
                _logger.Debug("Cache miss para a chave: {Key}", key);
                return null;
            }

            _logger.Debug("Cache hit para a chave: {Key}", key);
            return JsonSerializer.Deserialize<T>(value!);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erro ao obter valor do cache para a chave: {Key}", key);
            return null;
        }
    }
    
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value);
            var expirationTime = expiration ?? _defaultExpiration;

            await _redis.StringSetAsync(key, serializedValue, expirationTime);

            _logger.Debug("Valor armazenado no cache com a chave: {Key}, expira em: {Expiration}",
                key, expirationTime);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erro ao armazenar valor no cache para a chave: {Key}", key);
        }
    }
    
    public async Task RemoveAsync(string key)
    {
        try
        {
            await _redis.KeyDeleteAsync(key);
            _logger.Debug("Chave removida do cache: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erro ao remover chave do cache: {Key}", key);
        }
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            var endpoints = _connectionMultiplexer.GetEndPoints();
            var server = _connectionMultiplexer.GetServer(endpoints.First());

            var keys = server.Keys(pattern: pattern).ToArray();

            if (keys.Length > 0)
            {
                await _redis.KeyDeleteAsync(keys);
                _logger.Debug("Removidas {Count} chaves com o padrão: {Pattern}", keys.Length, pattern);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erro ao remover chaves por padrão: {Pattern}", pattern);
        }
    }
}