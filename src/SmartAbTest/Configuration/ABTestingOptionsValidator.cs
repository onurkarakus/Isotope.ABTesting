using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAbTest.Configuration;

public sealed class ABTestingOptionsValidator: IValidateOptions<ABTestingOptions>
{
    public ValidateOptionsResult Validate(string? name, ABTestingOptions options)
    {
        var errors = new List<string>();

        // 1. ServiceName Kontrolü
        if (string.IsNullOrWhiteSpace(options.ServiceName))
        {
            errors.Add("ServiceName cannot be null or empty.");
        }
        else if (options.ServiceName.Length > 50)
        {
            errors.Add("ServiceName must be 50 characters or less.");
        }
        else if (!IsValidServiceName(options.ServiceName))
        {
            errors.Add(
                "ServiceName contains invalid characters. " +
                "Use only lowercase letters, numbers, and hyphens. " +
                "Must start with a letter (e.g., 'payment-service').");
        }

        // 2. TTL Kontrolü
        if (options.DefaultTtl < TimeSpan.Zero)
        {
            errors.Add("DefaultTtl cannot be negative.");
        }

        // 3. Logging Sampling Rate Kontrolü
        if (options.Logging.SamplingRate < 0.0 || options.Logging.SamplingRate > 1.0)
        {
            errors.Add("Logging:SamplingRate must be between 0.0 and 1.0.");
        }

        if (errors.Count > 0)
        {
            return ValidateOptionsResult.Fail(errors);
        }

        return ValidateOptionsResult.Success;
    }

    // Regex kullanmak yerine manuel kontrol (Daha hızlı ve allocation-free)
    private static bool IsValidServiceName(string serviceName)
    {
        if (string.IsNullOrEmpty(serviceName)) return false;

        // Harfle başlamalı
        if (!char.IsLetter(serviceName[0]) || !char.IsLower(serviceName[0])) return false;

        foreach (var c in serviceName)
        {
            // Sadece küçük harf, rakam ve tire (-) kabul ediyoruz
            if (!char.IsLower(c) && !char.IsDigit(c) && c != '-') return false;
        }

        // Tire ile bitemez
        if (serviceName.EndsWith('-')) return false;

        return true;
    }
}
