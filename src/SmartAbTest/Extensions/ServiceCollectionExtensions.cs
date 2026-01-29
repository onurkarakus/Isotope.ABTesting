using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SmartAbTest.Abstractions;
using SmartAbTest.Configuration;
using SmartAbTest.Strategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAbTest.Extensions;

public static class ServiceCollectionExtensions
{
    public static ABTestingBuilder AddABTesting(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IValidateOptions<ABTestingOptions>, ABTestingOptionsValidator>();

        services.TryAddSingleton<IABTestClient, ABTestClient>();



        services.TryAddSingleton<DeterministicHashStrategy>();

        return new ABTestingBuilder(services);
    }

    public static ABTestingBuilder AddABTesting(
       this IServiceCollection services,
       Action<ABTestingOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.Configure(configure);

        return services.AddABTesting();
    }
}
