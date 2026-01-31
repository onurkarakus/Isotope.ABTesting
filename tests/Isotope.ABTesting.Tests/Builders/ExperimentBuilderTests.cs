using Isotope.ABTesting.Abstractions.Contexts;
using Isotope.ABTesting.Abstractions.Fallback;
using Isotope.ABTesting.Abstractions.Results;
using Isotope.ABTesting.Abstractions.Storage;
using Isotope.ABTesting.Abstractions.Strategies;
using Isotope.ABTesting.Builders;
using Isotope.ABTesting.Configuration;
using Isotope.ABTesting.Enums;
using Isotope.ABTesting.Exceptions;
using Isotope.ABTesting.Fallbacks;
using Isotope.ABTesting.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Isotope.ABTesting.Tests.Builders;

public class ExperimentBuilderTests
{
    private readonly string _experimentId = "test-exp";
    private readonly IStateStore _stateStore;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExperimentBuilder> _logger;
    private readonly IOptions<ABTestingOptions> _options;

    // Mock nesneler
    private readonly IAllocationStrategy _mockStrategy;
    private readonly IFallbackPolicy _mockFallback;

    public ExperimentBuilderTests()
    {
        _stateStore = Substitute.For<IStateStore>();
        _serviceProvider = Substitute.For<IServiceProvider>();
        _logger = Substitute.For<ILogger<ExperimentBuilder>>();
        _options = Options.Create(new ABTestingOptions());

        _mockStrategy = Substitute.For<IAllocationStrategy>();
        _mockFallback = Substitute.For<IFallbackPolicy>();
    }

    private ExperimentBuilder CreateBuilder()
    {
        return new ExperimentBuilder(
            _experimentId,
            _stateStore,
            _serviceProvider,
            _logger,
            _options);
    }

    [Fact]
    public async Task GetVariantAsync_ShouldThrow_WhenConfigurationIsMissing()
    {
        var builder = CreateBuilder();

        // Hiçbir şey yapılandırmadık (Variant, Algorithm, Fallback yok)
        var ex = await Assert.ThrowsAsync<ABTestingConfigurationException>(() =>
            builder.GetVariantAsync("user1"));

        Assert.Equal(FailureReason.IncompleteExperimentConfiguration, ex.Reason);
    }

    [Fact]
    public async Task GetVariantAsync_ShouldReturnFromCache_WhenPersistenceIsEnabled_And_CacheHit()
    {
        // Arrange
        var builder = CreateBuilder();

        // Cache dolu simülasyonu
        _stateStore.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                   .Returns(Task.FromResult<string?>("VariantA"));

        builder.WithVariants(("VariantA", 50), ("VariantB", 50))
               // DÜZELTME: _mockStrategy yerine _mockStrategy.AllocateAsync metodunu veriyoruz.
               .UseAlgorithm(_mockStrategy.AllocateAsync)
               .OnFailure(_mockFallback)
               .RequirePersistence();

        // Act
        var result = await builder.GetVariantAsync("user1");

        // Assert
        Assert.Equal("VariantA", result.Variant!.Name);
        Assert.Equal(AllocationSource.Cached, result.AllocationSource);

        // Strateji HİÇ çağrılmamalı
        await _mockStrategy.DidNotReceiveWithAnyArgs()
            .AllocateAsync(default!, default);
    }

    [Fact]
    public async Task GetVariantAsync_ShouldExecuteAlgorithm_And_CacheResult_WhenCacheMiss()
    {
        // Arrange
        var builder = CreateBuilder();

        // Cache boş (null)
        _stateStore.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                   .Returns(Task.FromResult<string?>(null));

        // Strateji 'VariantB' dönsün.
        var variantB = new VariantDefinition("VariantB", 50, 50);
        var expectedResult = AllocationResult.From(variantB, _experimentId, "user1", AllocationSource.Calculated);

        // Mock ayarı: ValueTask dönüş tipini garantiye almak için lambda kullanıyoruz
        _mockStrategy.AllocateAsync(Arg.Any<AllocationContext>(), Arg.Any<CancellationToken>())
                     .Returns(x => new ValueTask<AllocationResult>(expectedResult));

        builder.WithVariants(("VariantA", 50), ("VariantB", 50))
               // DÜZELTME: Metot referansı geçiyoruz
               .UseAlgorithm(_mockStrategy.AllocateAsync)
               .OnFailure(_mockFallback)
               .RequirePersistence();

        // Act
        var result = await builder.GetVariantAsync("user1");

        // Assert
        Assert.Equal("VariantB", result.Variant!.Name);
        Assert.Equal(AllocationSource.Calculated, result.AllocationSource);

        // Cache'e yazıldı mı?
        await _stateStore.Received(1).SetAsync(
            Arg.Is<string>(k => k.Contains(_experimentId) && k.Contains("user1")),
            "VariantB",
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetVariantAsync_ShouldUseFallback_WhenAlgorithmThrows()
    {
        // Arrange
        var builder = CreateBuilder();

        // Strateji hata fırlatsın.
        _mockStrategy.AllocateAsync(Arg.Any<AllocationContext>(), Arg.Any<CancellationToken>())
                     .Returns(x => ValueTask.FromException<AllocationResult>(new Exception("Algorithm crashed!")));

        // Fallback 'VariantA' dönsün.
        var variantA = new VariantDefinition("VariantA", 50, 50);
        var fallbackResult = AllocationResult.From(variantA, _experimentId, "user1", AllocationSource.Fallback);

        // Fallback için de aynı şekilde
        _mockFallback.ExecuteAsync(Arg.Any<FallbackContext>(), Arg.Any<CancellationToken>())
                     .Returns(x => new ValueTask<AllocationResult>(fallbackResult));

        builder.WithVariants(("VariantA", 50), ("VariantB", 50))
               // DÜZELTME: Metot referansı geçiyoruz
               .UseAlgorithm(_mockStrategy.AllocateAsync)
               .OnFailure(_mockFallback);

        // Act
        var result = await builder.GetVariantAsync("user1");

        // Assert
        Assert.Equal("VariantA", result.Variant!.Name);
        Assert.Equal(AllocationSource.Fallback, result.AllocationSource);

        // Fallback'in çağrıldığını doğrula
        await _mockFallback.Received(1).ExecuteAsync(
            Arg.Is<FallbackContext>(ctx => ctx.OriginalException!.Message == "Algorithm crashed!"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetVariantAsync_ShouldThrow_WhenKeyResolverFails()
    {
        // Arrange
        var builder = CreateBuilder();

        // DÜZELTME: ValidateConfiguration() engelini aşmak için sahte bir yapılandırma ekliyoruz.
        // Böylece kod ilerleyip keyResolver'ı çağırmaya çalışacak.
        builder.WithVariants(("VariantA", 50), ("VariantB", 50))
               .UseAlgorithm(_mockStrategy.AllocateAsync)
               .OnFailure(_mockFallback);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ABTestingException>(() =>
            builder.GetVariantAsync(
                keyResolver: ct => Task.FromException<string>(new Exception("Key generation failed")),
                cancellationToken: default));

        Assert.Equal(FailureReason.KeyResolutionFailed, ex.Reason);
    }

    [Fact]
    public async Task GetVariantAsync_ShouldIgnoreStateStoreError_AndContinueCalculation()
    {
        // Arrange
        var builder = CreateBuilder();

        // Cache hatası simülasyonu
        _stateStore.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                   .Returns(x => Task.FromException<string?>(new Exception("Redis connection failed")));

        // Strateji normal çalışsın
        var variantA = new VariantDefinition("VariantA", 50, 50);
        var expectedResult = AllocationResult.From(variantA, _experimentId, "user1", AllocationSource.Calculated);

        _mockStrategy.AllocateAsync(Arg.Any<AllocationContext>(), Arg.Any<CancellationToken>())
                     .Returns(x => new ValueTask<AllocationResult>(expectedResult));

        builder.WithVariants(("VariantA", 50), ("VariantB", 50))
               // DÜZELTME: Metot referansı geçiyoruz
               .UseAlgorithm(_mockStrategy.AllocateAsync)
               .OnFailure(_mockFallback)
               .RequirePersistence();

        // Act
        var result = await builder.GetVariantAsync("user1");

        // Assert
        Assert.Equal("VariantA", result.Variant!.Name);

        // LogWarning çağrısını doğrula
        _logger.ReceivedWithAnyArgs().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task OnFailure_WithDelegate_ShouldInvokeDelegate_And_ReturnResult()
    {
        // Arrange
        var builder = CreateBuilder();
        var delegateCalled = false;

        // Varyantlar ve Mock Strateji
        builder.WithVariants(("VariantA", 50), ("VariantB", 50))
               // DİKKAT: UseAlgorithm hata fırlatmalı ki OnFailure devreye girsin.
               .UseAlgorithm((ctx, ct) => ValueTask.FromException<AllocationResult>(new Exception("Algo Failed")))
               // OnFailure için Delegate (Func) tanımlıyoruz
               .OnFailure((ctx, ct) =>
               {
                   delegateCalled = true;
                   // Manuel olarak VariantA dönüyoruz
                   var variant = ctx.Variants.First(v => v.Name == "VariantA");
                   return new ValueTask<AllocationResult>(
                       AllocationResult.From(variant, _experimentId, "user1", AllocationSource.Fallback));
               });

        // Act
        var result = await builder.GetVariantAsync("user1");

        // Assert
        Assert.True(delegateCalled, "Delegate fallback was not called.");
        Assert.Equal("VariantA", result.Variant!.Name);
        Assert.Equal(AllocationSource.Fallback, result.AllocationSource);
    }

    [Fact]
    public async Task ValidateConfiguration_ShouldThrow_WhenDefaultVariantFallback_VariantNotFound()
    {
        var builder = CreateBuilder();

        builder.WithVariants(("VariantA", 50), ("VariantB", 50))
               .UseAlgorithm(_mockStrategy.AllocateAsync)
               .OnFailure(new DefaultVariantFallbackPolicy("VariantZ")); // Hata vermeli

        var ex = await Assert.ThrowsAsync<ABTestingConfigurationException>(() =>
            builder.GetVariantAsync("user1"));

        Assert.Equal(FailureReason.VariantNotFound, ex.Reason);
    }

    [Fact]
    public async Task ValidateConfiguration_ShouldThrow_WhenAlgorithmAndFallback_AreIncompatible()
    {
        var builder = CreateBuilder();
    }
}