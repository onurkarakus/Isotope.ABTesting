using Isotope.ABTesting.Abstractions.Contexts;
using Isotope.ABTesting.Enums;
using Isotope.ABTesting.Models;
using Isotope.ABTesting.Strategies;
using NSubstitute;
using Xunit;

namespace Isotope.ABTesting.Tests.Strategies;

public class WeightedRandomStrategyTests
{
    private readonly WeightedRandomStrategy _strategy;
    private readonly IServiceProvider _serviceProvider;

    public WeightedRandomStrategyTests()
    {
        _strategy = new WeightedRandomStrategy();
        _serviceProvider = Substitute.For<IServiceProvider>();
    }

    [Fact]
    public void Properties_ShouldBeCorrect()
    {
        Assert.False(_strategy.RequiresState);
        Assert.False(_strategy.IsDeterministic);
    }

    [Fact]
    public async Task AllocateAsync_ShouldReturnEmpty_WhenVariantsAreNullOrEmpty()
    {
        // Arrange
        var context = new AllocationContext(
            "exp1",
            "user1",
            Array.Empty<VariantDefinition>(),
            _serviceProvider);

        // Act
        var result = await _strategy.AllocateAsync(context);

        // Assert
        Assert.Null(result.Variant);
        Assert.Equal(AllocationSource.Fallback, result.AllocationSource);
    }

    [Fact]
    public async Task AllocateAsync_ShouldReturnValidVariant()
    {
        // Arrange
        var variants = new[]
        {
            new VariantDefinition("A", 50, 50),
            new VariantDefinition("B", 50, 50)
        };

        var context = new AllocationContext("exp1", "user1", variants, _serviceProvider);

        // Act
        // Rastgelelik içerdiği için 100 kez çalıştırıp hep geçerli sonuç verdiğini teyit edelim
        for (int i = 0; i < 100; i++)
        {
            var result = await _strategy.AllocateAsync(context);

            // Assert
            Assert.NotNull(result.Variant);
            Assert.Contains(variants, v => v.Name == result.Variant!.Name);
            Assert.Equal(AllocationSource.Calculated, result.AllocationSource);
        }
    }
}