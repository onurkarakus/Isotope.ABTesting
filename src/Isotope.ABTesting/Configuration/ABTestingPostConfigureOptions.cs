using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Isotope.ABTesting.Configuration;

public sealed class ABTestingPostConfigureOptions:IPostConfigureOptions<ABTestingOptions>
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ABTestingPostConfigureOptions> _logger;

    public ABTestingPostConfigureOptions(IConfiguration configuration, ILogger<ABTestingPostConfigureOptions> logger)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(logger);

        _configuration = configuration;
        _logger = logger;
    }

    public void PostConfigure(string? name, ABTestingOptions options)
    {
        if (!string.IsNullOrEmpty(options.ServiceName))
        {
            return;
        }

        const string FallbackKey = "DefaultABTestingService";
        var fallbackValue = _configuration[FallbackKey];

        if (!string.IsNullOrEmpty(fallbackValue))
        {
            options.ServiceName = fallbackValue;

            _logger.LogInformation("ABTestingOptions.ServiceName was not set. Using fallback value from configuration key '{FallbackKey}': '{ServiceName}'", FallbackKey, fallbackValue);
        }
        else
        {
            _logger.LogWarning("ABTestingOptions.ServiceName was not set and no fallback value found in configuration key '{FallbackKey}'.", FallbackKey);
        }
    }
}
