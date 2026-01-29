using Microsoft.Extensions.DependencyInjection;
using SmartAbTest.Abstractions.Storage;
using SmartAbTest.Configuration;
using SmartAbTest.StateStores;

namespace SmartAbTest.Extensions;

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
