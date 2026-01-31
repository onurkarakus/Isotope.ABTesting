using Isotope.ABTesting.Abstractions.Contexts;
using Isotope.ABTesting.Abstractions.Results;
using Isotope.ABTesting.Enums;
using Isotope.ABTesting.Models;
using NSubstitute;
using Xunit;

namespace Isotope.ABTesting.Tests.Models;

public class ModelsTests
{
    private readonly IServiceProvider _serviceProvider = Substitute.For<IServiceProvider>();

    [Fact]
    public void VariantDefinition_ShouldInitializeCorrectly()
    {
        var variant = new VariantDefinition("A", 50, 50);

        Assert.Equal("A", variant.Name);
        Assert.Equal(50, variant.Weight);
        Assert.Equal(50, variant.NormalizedWeight);
    }

    [Fact]
    public void AllocationResult_From_ShouldCreateInstance()
    {
        var variant = new VariantDefinition("A", 100, 100);
        var result = AllocationResult.From(variant, "exp1", "user1", AllocationSource.Calculated);

        Assert.Equal("A", result.Variant?.Name);
        Assert.Equal("exp1", result.ExperimentId);
        Assert.Equal("user1", result.SubjectKey);
        Assert.Equal(AllocationSource.Calculated, result.AllocationSource);
    }

    [Fact]
    public void AllocationContext_ShouldStoreValues()
    {
        var variants = new[] { new VariantDefinition("A", 100, 100) };
        var context = new AllocationContext("exp1", "user1", variants, _serviceProvider);

        Assert.Equal("exp1", context.ExperimentId);
        Assert.Equal("user1", context.SubjectKey);
        Assert.Single(context.Variants);
        Assert.Same(_serviceProvider, context.ServiceProvider);
    }

    [Fact]
    public void FallbackContext_ShouldStoreValues()
    {
        var variants = new[] { new VariantDefinition("A", 100, 100) };
        var exception = new Exception("fail");

        var context = new FallbackContext("exp1", "user1", variants, exception, _serviceProvider);

        Assert.Equal("exp1", context.ExperimentId);
        Assert.Equal("user1", context.SubjectKey);
        Assert.Same(exception, context.OriginalException);
        Assert.Same(_serviceProvider, context.ServiceProvider);
    }

    [Fact]
    public void WeightValidationResult_ShouldInitialize()
    {
        var variants = new List<VariantDefinition> { new VariantDefinition("A", 100, 100) };

        // DÜZELTME: Parametre sırası (List, bool, int) olarak düzeltildi.
        var result = new WeightValidationResult(variants, true, 100);

        Assert.True(result.WasNormalized);
        Assert.Equal(100, result.OriginalSum);
        Assert.Single(result.ValidVariants);
    }
}