using Isotope.ABTesting.Abstractions.Storage;
using Isotope.ABTesting.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Isotope.ABTesting.StateStores;

public sealed class RedisStateStore : IStateStore, IDisposable
{
    private readonly IConnectionMultiplexer _connection;
    private readonly IDatabase _database;
    private readonly RedisOptions _options;
    private readonly ILogger<RedisStateStore> _logger;
    private readonly bool _ownsConnection;

    public RedisStateStore(IOptions<RedisOptions> options, ILogger<RedisStateStore> logger)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        _options = options.Value;
        _logger = logger;
        _ownsConnection = true;

        var configOptions = BuildConfigurationOptions(_options);
        _connection = ConnectionMultiplexer.Connect(configOptions);
        _database = _connection.GetDatabase(_options.Database);

        _logger.LogInformation("Connected to Redis at {RedisEndpoint}", _connection.GetEndPoints().FirstOrDefault());
    }

    public RedisStateStore(IConnectionMultiplexer connection, IOptions<RedisOptions> options, ILogger<RedisStateStore> logger)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _connection = connection;
        _options = options.Value;
        _logger = logger;
        _ownsConnection = false;
        _database = _connection.GetDatabase(_options.Database);

        _logger.LogInformation("Using provided Redis connection at {RedisEndpoint}", _connection.GetEndPoints().FirstOrDefault());
    }

    public async Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));

        var redisKey = BuildKey(key);

        try
        {
            var value = await _database.StringGetAsync(redisKey);

            return value.HasValue ? value.ToString() : null;
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogError(ex, "Redis connection error while getting key {RedisKey}", redisKey);
            throw;
        }
        catch (RedisTimeoutException ex)
        {
            _logger.LogError(ex, "Redis timeout while getting key {RedisKey}", redisKey);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting key {RedisKey}", redisKey);
            throw;
        }
    }

    public async Task SetAsync(string key, string variantName, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        ArgumentNullException.ThrowIfNull(variantName, nameof(variantName));

        var redisKey = BuildKey(key);

        try
        {
            await _database.StringSetAsync(redisKey, variantName, ttl);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogError(ex, "Redis connection error while setting key {RedisKey}", redisKey);
            throw;
        }
        catch (RedisTimeoutException ex)
        {
            _logger.LogError(ex, "Redis timeout while setting key {RedisKey}", redisKey);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while setting key {RedisKey}", redisKey);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));

        var redisKey = BuildKey(key);

        try
        {
            return await _database.KeyExistsAsync(redisKey);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogError(ex, "Redis connection error while checking existence of key {RedisKey}", redisKey);
            throw;
        }
        catch (RedisTimeoutException ex)
        {
            _logger.LogError(ex, "Redis timeout while checking existence of key {RedisKey}", redisKey);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while checking existence of key {RedisKey}", redisKey);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));

        var redisKey = BuildKey(key);

        try
        {
            return await _database.KeyDeleteAsync(redisKey);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogError(ex, "Redis connection error while deleting key {RedisKey}", redisKey);
            throw;
        }
        catch (RedisTimeoutException ex)
        {
            _logger.LogError(ex, "Redis timeout while deleting key {RedisKey}", redisKey);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting key {RedisKey}", redisKey);
            throw;
        }
    }

    private string BuildKey(string key)
    {
        return $"{_options.KeyPrefix}{key}";
    }

    private static ConfigurationOptions BuildConfigurationOptions(RedisOptions options)
    {
        var config = ConfigurationOptions.Parse(options.ConnectionString);
        config.ConnectTimeout = options.ConnectTimeout;
        config.SyncTimeout = options.SyncTimeout;
        config.AbortOnConnectFail = options.AbortOnConnectFail;
        config.ConnectRetry = options.RetryCount;
        return config;
    }

    public void Dispose()
    {
        if (_ownsConnection)
        {
            _connection.Dispose();
            _logger.LogInformation("Redis connection disposed.");
        }
    }
}
