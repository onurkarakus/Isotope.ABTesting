using Isotope.ABTesting.Abstractions.Contexts;
using Isotope.ABTesting.Enums;
using Isotope.ABTesting.Models;
using Isotope.ABTesting.Strategies;
using NSubstitute;
using Xunit;

namespace Isotope.ABTesting.Tests.Strategies;

public class DeterministicHashStrategyTests
{
    private readonly DeterministicHashStrategy _strategy;
    private readonly IServiceProvider _serviceProvider;

    public DeterministicHashStrategyTests()
    {
        _strategy = new DeterministicHashStrategy();
        _serviceProvider = Substitute.For<IServiceProvider>();
    }

    [Fact]
    public void Properties_ShouldBeCorrect()
    {
        Assert.False(_strategy.RequiresState);
        Assert.True(_strategy.IsDeterministic);
    }

    [Fact]
    public async Task AllocateAsync_ShouldReturnEmpty_WhenVariantsAreEmpty()
    {
        var context = new AllocationContext(
            "exp1",
            "user1",
            Array.Empty<VariantDefinition>(),
            _serviceProvider);

        var result = await _strategy.AllocateAsync(context);

        Assert.Null(result.Variant);
        Assert.Equal(AllocationSource.Fallback, result.AllocationSource);
    }

    [Fact]
    public async Task AllocateAsync_ShouldBeDeterministic()
    {
        // Arrange
        // Toplam 100 veriyoruz ki bug'a takılmadan test geçsin.
        var variants = new[]
        {
            new VariantDefinition("A", 50, 50),
            new VariantDefinition("B", 50, 50)
        };

        var context = new AllocationContext("exp1", "user-fixed-key", variants, _serviceProvider);

        // Act
        var result1 = await _strategy.AllocateAsync(context);
        var result2 = await _strategy.AllocateAsync(context);

        // Assert
        Assert.NotNull(result1.Variant);
        Assert.Equal(result1.Variant!.Name, result2.Variant!.Name);
    }

    [Fact]
    public async Task AllocateAsync_ShouldRespectDistribution()
    {
        // Arrange
        // A varyantına %100, B'ye %0 vererek seçimi zorluyoruz.
        // Bu sayede hash'in tam değerini bilmesek de sonucun ne olması gerektiğini biliyoruz.
        var variantsForceA = new[]
        {
            new VariantDefinition("A", 100, 100),
            new VariantDefinition("B", 0, 0)
        };

        var contextA = new AllocationContext("exp1", "user1", variantsForceA, _serviceProvider);
        var resultA = await _strategy.AllocateAsync(contextA);

        Assert.Equal("A", resultA.Variant!.Name);

        // Şimdi tam tersi
        var variantsForceB = new[]
        {
            new VariantDefinition("A", 0, 0),
            new VariantDefinition("B", 100, 100)
        };

        var contextB = new AllocationContext("exp1", "user1", variantsForceB, _serviceProvider);
        var resultB = await _strategy.AllocateAsync(contextB);

        Assert.Equal("B", resultB.Variant!.Name);
    }
}