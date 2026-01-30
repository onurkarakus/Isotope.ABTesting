using Isotope.ABTesting.Abstractions.Storage;
using Isotope.ABTesting.Configuration;
using Isotope.ABTesting.StateStores;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Isotope.ABTesting.Extensions;

public sealed class ABTestingBuilder
{
    public IServiceCollection Services { get; }

    internal ABTestingBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public ABTestingBuilder Configure(Action<ABTestingOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        Services.Configure(configure);

        return this;
    }

    public ABTestingBuilder UseRedis(Action<RedisOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        Services.Configure(configure);

        Services.TryAddSingleton<IPostConfigureOptions<RedisOptions>, RedisPostConfigureOptions>();
        Services.TryAddSingleton<IValidateOptions<RedisOptions>, RedisOptionsValidator>();
        Services.AddOptions<RedisOptions>().ValidateOnStart();
        
        RemoveExistingStateStore();

        Services.AddSingleton<IStateStore, RedisStateStore>();

        return this;
    }

    public ABTestingBuilder UseStateStore<TStateStore>()
        where TStateStore : class, IStateStore
    {
        RemoveExistingStateStore();
        Services.AddSingleton<IStateStore, TStateStore>();

        return this;
    }

    public ABTestingBuilder UseStateStore(IStateStore stateStore)
    {
        ArgumentNullException.ThrowIfNull(stateStore);

        RemoveExistingStateStore();
        Services.AddSingleton(stateStore);

        return this;
    }

    private void RemoveExistingStateStore()
    {
        var descriptor = Services.FirstOrDefault(d => d.ServiceType == typeof(IStateStore));

        if (descriptor != null)
        {
            Services.Remove(descriptor);
        }
    }
}
