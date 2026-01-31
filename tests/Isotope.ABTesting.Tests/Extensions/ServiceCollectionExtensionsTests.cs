using Isotope.ABTesting.Abstractions;
using Isotope.ABTesting.Abstractions.Storage;
using Isotope.ABTesting.Configuration;
using Isotope.ABTesting.Extensions;
using Isotope.ABTesting.StateStores;
using Isotope.ABTesting.Strategies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Isotope.ABTesting.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    private IConfiguration GetEmptyConfiguration() => new ConfigurationBuilder().Build();

    [Fact]
    public void AddABTesting_ThrowsOnNullServices()
    {
        Assert.Throws<ArgumentNullException>(() => ServiceCollectionExtensions.AddABTesting(null!));
    }

    [Fact]
    public void AddABTestingWithConfigure_ThrowsOnNullParameters()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>(() => ServiceCollectionExtensions.AddABTesting(null!, (Action<ABTestingOptions>)(_ => { })));
        Assert.Throws<ArgumentNullException>(() => ServiceCollectionExtensions.AddABTesting(services, null!));
    }

    [Fact]
    public void AddABTesting_ReturnsBuilderWithSameServiceCollection()
    {
        var services = new ServiceCollection();
        services.AddSingleton(GetEmptyConfiguration());
        services.AddLogging(); // DÜZELTME: Logger servisi eklendi

        var builder = services.AddABTesting();

        Assert.NotNull(builder);
        Assert.Same(services, builder.Services);
    }

    [Fact]
    public void AddABTesting_WithConfigure_AppliesOptions()
    {
        var services = new ServiceCollection();
        services.AddSingleton(GetEmptyConfiguration());
        services.AddLogging(); // DÜZELTME: Logger servisi eklendi

        services.AddABTesting(opts => opts.ServiceName = "my-service");

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ABTestingOptions>>().Value;

        Assert.Equal("my-service", options.ServiceName);
    }

    [Fact]
    public void AddABTesting_RegistersIABTestClientAndIsSingleton()
    {
        var services = new ServiceCollection();
        services.AddSingleton(GetEmptyConfiguration());
        services.AddLogging(); // DÜZELTME: Logger servisi eklendi

        services.AddABTesting();

        var provider = services.BuildServiceProvider();

        var client1 = provider.GetRequiredService<IABTestClient>();
        var client2 = provider.GetRequiredService<IABTestClient>();

        Assert.Same(client1, client2);
        Assert.IsType<ABTestClient>(client1);
    }

    [Fact]
    public void AddABTesting_DefaultsToInMemoryStateStore_WhenNotPreRegistered()
    {
        var services = new ServiceCollection();
        services.AddSingleton(GetEmptyConfiguration());
        services.AddLogging(); // DÜZELTME: Logger servisi eklendi

        services.AddABTesting();

        var provider = services.BuildServiceProvider();
        var stateStore = provider.GetRequiredService<IStateStore>();

        Assert.IsType<InMemoryStateStore>(stateStore);
    }

    [Fact]
    public void AddABTesting_DoesNotOverridePreRegisteredStateStore()
    {
        var services = new ServiceCollection();
        services.AddSingleton(GetEmptyConfiguration());
        services.AddLogging(); // DÜZELTME: Logger servisi eklendi

        var preRegistered = new PreRegisteredStateStore();
        services.AddSingleton<IStateStore>(preRegistered);

        services.AddABTesting();

        var provider = services.BuildServiceProvider();
        var resolved = provider.GetRequiredService<IStateStore>();

        Assert.Same(preRegistered, resolved);
    }

    [Fact]
    public void Builder_UseStateStore_InstanceReplacesDefault()
    {
        var services = new ServiceCollection();
        services.AddSingleton(GetEmptyConfiguration());
        services.AddLogging(); // DÜZELTME: Logger servisi eklendi

        var builder = services.AddABTesting();
        var custom = new PreRegisteredStateStore();

        builder.UseStateStore(custom);

        var provider = services.BuildServiceProvider();
        var resolved = provider.GetRequiredService<IStateStore>();

        Assert.Same(custom, resolved);
    }

    [Fact]
    public void Strategies_AreRegisteredAsSingletons()
    {
        var services = new ServiceCollection();
        services.AddSingleton(GetEmptyConfiguration());
        services.AddLogging(); // DÜZELTME: Logger servisi eklendi

        services.AddABTesting();

        var provider = services.BuildServiceProvider();

        var dh1 = provider.GetRequiredService<DeterministicHashStrategy>();
        var dh2 = provider.GetRequiredService<DeterministicHashStrategy>();

        var wr1 = provider.GetRequiredService<WeightedRandomStrategy>();
        var wr2 = provider.GetRequiredService<WeightedRandomStrategy>();

        Assert.Same(dh1, dh2);
        Assert.Same(wr1, wr2);
    }

    [Fact]
    public void AddABTesting_RegistersOptionsValidatorAndPostConfigure()
    {
        var services = new ServiceCollection();
        services.AddSingleton(GetEmptyConfiguration());
        services.AddLogging(); // DÜZELTME: Logger servisi eklendi

        services.AddABTesting();

        var provider = services.BuildServiceProvider();

        var validate = provider.GetService<IValidateOptions<ABTestingOptions>>();
        var postConfigure = provider.GetService<IPostConfigureOptions<ABTestingOptions>>();

        Assert.NotNull(validate);
        Assert.NotNull(postConfigure);
    }

    private sealed class PreRegisteredStateStore : IStateStore
    {
        public Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
            => Task.FromResult<string?>(null);

        public Task SetAsync(string key, string variantName, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default)
            => Task.FromResult(false);
    }
}