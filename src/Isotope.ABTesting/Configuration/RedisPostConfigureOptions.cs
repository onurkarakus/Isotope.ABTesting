using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Isotope.ABTesting.Configuration;

public sealed class RedisPostConfigureOptions: IPostConfigureOptions<RedisOptions>
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RedisPostConfigureOptions> _logger;

    public RedisPostConfigureOptions(IConfiguration configuration, ILogger<RedisPostConfigureOptions> logger)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(logger);

        _configuration = configuration;
        _logger = logger;        
    }

    public void PostConfigure(string? name, RedisOptions options)
    {
        if (!string.IsNullOrEmpty(options.ConnectionString))
        {
            return;
        }

        const string FallbackConnectionStringKey = "DefaultRedisConnectionString";
        var fallbackConnectionStringValue = _configuration[FallbackConnectionStringKey];

        if (!string.IsNullOrEmpty(fallbackConnectionStringValue))
        {
            options.ConnectionString = fallbackConnectionStringValue;

            _logger.LogInformation("RedisOptions.ConnectionString was not set. Using fallback value from configuration key '{FallbackConnectionString}': '{ConnectionString}'", FallbackConnectionStringKey, fallbackConnectionStringValue);
        }
        else
        {
            _logger.LogWarning("RedisOptions.ConnectionString was not set and no fallback value found in configuration key '{FallbackConnectionString}'.", FallbackConnectionStringKey);
        }
    }
}
