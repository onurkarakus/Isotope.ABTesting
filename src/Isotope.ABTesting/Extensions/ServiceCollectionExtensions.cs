using Isotope.ABTesting.Abstractions;
using Isotope.ABTesting.Abstractions.Storage;
using Isotope.ABTesting.Configuration;
using Isotope.ABTesting.StateStores;
using Isotope.ABTesting.Strategies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Isotope.ABTesting.Extensions;

public static class ServiceCollectionExtensions
{
    public static ABTestingBuilder AddABTesting(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<ABTestingOptions>()
            .ValidateOnStart();

        services.TryAddSingleton<IValidateOptions<ABTestingOptions>, ABTestingOptionsValidator>();

        services.TryAddSingleton<IPostConfigureOptions<ABTestingOptions>, ABTestingPostConfigureOptions>();

        services.TryAddSingleton<IABTestClient, ABTestClient>();

        services.TryAddSingleton<IStateStore, InMemoryStateStore>();

        services.TryAddSingleton<DeterministicHashStrategy>();
        services.TryAddSingleton<WeightedRandomStrategy>();

        return new ABTestingBuilder(services);
    }

    public static ABTestingBuilder AddABTesting(this IServiceCollection services, Action<ABTestingOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.Configure(configure);

        return services.AddABTesting();
    }
}
