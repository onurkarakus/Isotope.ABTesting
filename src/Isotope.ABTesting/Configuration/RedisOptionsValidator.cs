using Microsoft.Extensions.Options;

namespace Isotope.ABTesting.Configuration;

public sealed class RedisOptionsValidator : IValidateOptions<RedisOptions>
{
    public ValidateOptionsResult Validate(string? name, RedisOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.ConnectionString) || options.ConnectionString == "localhost:6379")
        {
            errors.Add("Redis ConnectionString is required. " +
                       "Please set 'ConnectionString' in code, or define 'ConnectionStrings:Redis' in appsettings.json.");
        }

        if (string.IsNullOrWhiteSpace(options.KeyPrefix))
        {
            errors.Add("Redis KeyPrefix cannot be empty.");
        }

        else if (!options.KeyPrefix.EndsWith(":"))
        {
            errors.Add($"Redis KeyPrefix '{options.KeyPrefix}' must end with a colon (:).");
        }

        if (options.ConnectTimeout < 500)
        {
            errors.Add("ConnectTimeout is too low (min 500ms recommended).");
        }

        if (errors.Count > 0)
        {
            return ValidateOptionsResult.Fail(errors);
        }

        return ValidateOptionsResult.Success;
    }
}
