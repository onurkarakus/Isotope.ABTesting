using Isotope.ABTesting.Enums;
using Isotope.ABTesting.Exceptions;
using Isotope.ABTesting.Fallbacks;
using Isotope.ABTesting.Strategies;
using Xunit;

namespace Isotope.ABTesting.Tests.Exceptions;

public class ExceptionFactoryTests
{
    private const string ExpId = "test-exp";

    [Fact]
    public void WeightSumExceeds100_ShouldReturnCorrectException()
    {
        var ex = ExceptionFactory.WeightSumExceeds100(ExpId, 150);

        Assert.Equal(ExpId, ex.ExperimentId);
        Assert.Equal(FailureReason.InvalidWeightConfiguration, ex.Reason);
        Assert.Contains("150", ex.Message);
    }

    [Fact]
    public void NegativeWeight_ShouldReturnCorrectException()
    {
        var ex = ExceptionFactory.NegativeWeight(ExpId, "VariantA", -5);

        Assert.Equal(ExpId, ex.ExperimentId);
        Assert.Equal(FailureReason.InvalidWeightConfiguration, ex.Reason);
        Assert.Contains("VariantA", ex.Message);
    }

    [Fact]
    public void InsufficientVariants_ShouldReturnCorrectException()
    {
        var ex = ExceptionFactory.InsufficientVariants(ExpId, 1);

        Assert.Equal(ExpId, ex.ExperimentId);
        Assert.Equal(FailureReason.InvalidConfiguration, ex.Reason);
    }

    [Theory]
    [InlineData("Variants", "WithVariants()")]
    [InlineData("Algorithm", "UseAlgorithm()")]
    [InlineData("FallbackPolicy", "OnFailure()")]
    [InlineData("Other", "Other")]
    public void MissingConfiguration_ShouldFormatMessageCorrectly(string property, string expectedMethod)
    {
        var ex = ExceptionFactory.MissingConfiguration(ExpId, property);

        Assert.Equal(FailureReason.IncompleteExperimentConfiguration, ex.Reason);
        Assert.Contains(expectedMethod, ex.Message);
    }

    // DİKKAT: Bu metotlar Factory içinde 'throw' ettiği için Assert.Throws kullanıyoruz.
    [Fact]
    public void DuplicateVariants_ShouldThrowImmediately()
    {
        var ex = Assert.Throws<ABTestingConfigurationException>(() =>
            ExceptionFactory.DuplicateVariants(ExpId, "VariantA"));

        Assert.Equal(FailureReason.InvalidConfiguration, ex.Reason);
        Assert.Contains("Duplicate", ex.Message);
    }

    [Fact]
    public void EmptyVariantNames_ShouldThrowImmediately()
    {
        var ex = Assert.Throws<ABTestingConfigurationException>(() =>
            ExceptionFactory.EmptyVariantNames(ExpId));

        Assert.Equal(FailureReason.InvalidConfiguration, ex.Reason);
    }

    [Fact]
    public void SumEqualsZero_ShouldThrowImmediately()
    {
        var ex = Assert.Throws<ABTestingConfigurationException>(() =>
            ExceptionFactory.SumEqualsZero(ExpId));

        Assert.Equal(FailureReason.InvalidWeightConfiguration, ex.Reason);
    }

    [Fact]
    public void StrategyResolutionFailed_ShouldIncludeInnerException()
    {
        var inner = new Exception("Constructor failed");
        var ex = ExceptionFactory.StrategyResolutionFailed(ExpId, typeof(WeightedRandomStrategy), inner);

        Assert.Equal(FailureReason.StrategyResolutionFailed, ex.Reason);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void StateStoreUnavailable_ShouldHandleInnerException()
    {
        // Inner exception varsa onu kullanmalı
        var inner = new TimeoutException();
        var ex1 = ExceptionFactory.StateStoreUnavailable(ExpId, inner);
        Assert.Same(inner, ex1.InnerException);

        // Yoksa varsayılan bir tane oluşturmalı
        var ex2 = ExceptionFactory.StateStoreUnavailable(ExpId, null);
        Assert.NotNull(ex2.InnerException);
        Assert.IsType<InvalidOperationException>(ex2.InnerException);
    }

    [Fact]
    public void FallbackVariantNotFound_ShouldListAvailableVariants()
    {
        var variants = new[] { "A", "B" };
        var ex = ExceptionFactory.FallbackVariantNotFound(ExpId, "C", variants);

        Assert.Equal(FailureReason.VariantNotFound, ex.Reason);
        Assert.Contains("'A', 'B'", ex.Message);
    }

    [Fact]
    public void InvalidAlgorithmFallbackCombination_ShouldReturnCorrectException()
    {
        var ex = ExceptionFactory.InvalidAlgorithmFallbackCombination(ExpId, "Algo", "Fallback");
        Assert.Equal(FailureReason.InvalidAlgorithmFallbackCombination, ex.Reason);
    }

    [Fact]
    public void FallbackExecutionFailed_ShouldReturnCorrectException()
    {
        var inner = new Exception("Boom");
        var ex = ExceptionFactory.FallbackExecutionFailed(ExpId, inner);

        Assert.Equal(FailureReason.FallbackExecutionFailed, ex.Reason);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void AlgorithmExecutionFailed_ShouldReturnCorrectException()
    {
        var inner = new Exception("Boom");
        var ex = ExceptionFactory.AlgorithmExecutionFailed(ExpId, inner);

        Assert.Equal(FailureReason.AlgorithmExecutionFailed, ex.Reason);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void KeyResolutionFailed_ShouldReturnCorrectException()
    {
        var inner = new Exception("Boom");
        var ex = ExceptionFactory.KeyResolutionFailed(ExpId, inner);

        Assert.Equal(FailureReason.KeyResolutionFailed, ex.Reason);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void KeyResolverReturnedNull_ShouldReturnCorrectException()
    {
        var ex = ExceptionFactory.KeyResolverReturnedNull(ExpId);
        Assert.Equal(FailureReason.KeyResolutionFailed, ex.Reason);
    }
}