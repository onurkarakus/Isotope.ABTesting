using Isotope.ABTesting.Abstractions.Contexts;
using Isotope.ABTesting.Abstractions.Results;
using Isotope.ABTesting.Builders;
using Isotope.ABTesting.Enums;
using Isotope.ABTesting.Exceptions;
using Isotope.ABTesting.Fallbacks;
using Isotope.ABTesting.Models;
using NSubstitute;
using Xunit;

namespace Isotope.ABTesting.Tests.Fallbacks;

public class FallbackPoliciesTests
{
    private readonly IServiceProvider _serviceProvider = Substitute.For<IServiceProvider>();

    [Fact]
    public async Task ThrowFallbackPolicy_ShouldThrowException()
    {
        var policy = ThrowFallbackPolicy.Instance;
        var originalEx = new Exception("Original crash");

        var context = new FallbackContext(
            "exp1",
            "user1",
            Array.Empty<VariantDefinition>(),
            originalEx,
            _serviceProvider);

        var ex = await Assert.ThrowsAsync<ABTestingException>(() => policy.ExecuteAsync(context).AsTask());
        Assert.Same(originalEx, ex.InnerException);
    }

    [Fact]
    public async Task DefaultVariantFallbackPolicy_ShouldReturnSpecifiedVariant()
    {
        var policy = new DefaultVariantFallbackPolicy("VariantA");
        var variants = new[]
        {
            new VariantDefinition("VariantA", 50, 50),
            new VariantDefinition("VariantB", 50, 50)
        };

        var context = new FallbackContext(
            "exp1",
            "user1",
            variants,
            new Exception("Boom"),
            _serviceProvider);

        var result = await policy.ExecuteAsync(context);

        Assert.NotNull(result);
        Assert.Equal("VariantA", result.Variant!.Name);
        Assert.Equal(AllocationSource.Fallback, result.AllocationSource);
    }

    // YENİ EKLENEN TEST
    [Fact]
    public async Task StatelessFallbackPolicy_ShouldInvokeDelegate()
    {
        var variants = new[] { new VariantDefinition("VariantA", 100, 100) };
        var context = new FallbackContext("exp1", "user1", variants, new Exception(), _serviceProvider);

        // DelegateFallbackPolicy'yi test ediyoruz
        bool delegateCalled = false;
        var policy = new DelegateFallbackPolicy((ctx, ct) =>
        {
            delegateCalled = true;
            return new ValueTask<AllocationResult>(
                AllocationResult.From(variants[0], "exp1", "user1", AllocationSource.Fallback));
        });

        var result = await policy.ExecuteAsync(context);

        Assert.True(delegateCalled);
        Assert.Equal("VariantA", result.Variant!.Name);
    }
}