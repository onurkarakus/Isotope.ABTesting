using Isotope.ABTesting.Enums;
using Isotope.ABTesting.Exceptions;
using Isotope.ABTesting.Validators;
using Xunit;

namespace Isotope.ABTesting.Tests.Validators;

public class WeightValidatorTests
{
    [Fact]
    public void Validate_ShouldThrow_WhenVariantCountIsLessThanTwo()
    {
        // Arrange
        var variants = new[] { ("A", 50) };

        // Act & Assert
        var ex = Assert.Throws<ABTestingConfigurationException>(() =>
            WeightValidator.Validate("exp1", variants));

        Assert.Equal(FailureReason.InvalidConfiguration, ex.Reason);
        Assert.Contains("At least 2 variants required", ex.Message);
    }

    [Fact]
    public void Validate_ShouldThrow_WhenWeightIsNegative()
    {
        // Arrange
        var variants = new[] { ("A", 50), ("B", -10) };

        // Act & Assert
        var ex = Assert.Throws<ABTestingConfigurationException>(() =>
            WeightValidator.Validate("exp1", variants));

        Assert.Equal(FailureReason.InvalidWeightConfiguration, ex.Reason);
        Assert.Contains("negative weight", ex.Message);
    }

    [Fact]
    public void Validate_ShouldThrow_WhenDuplicateVariantNamesExist()
    {
        // Arrange
        var variants = new[] { ("A", 50), ("A", 50) };

        // Act & Assert
        var ex = Assert.Throws<ABTestingConfigurationException>(() =>
            WeightValidator.Validate("exp1", variants));

        Assert.Equal(FailureReason.InvalidConfiguration, ex.Reason);
        Assert.Contains("Duplicate variant name", ex.Message);
    }

    [Fact]
    public void Validate_ShouldThrow_WhenVariantNameIsEmpty()
    {
        // Arrange
        var variants = new[] { ("A", 50), ("", 50) };

        // Act & Assert
        var ex = Assert.Throws<ABTestingConfigurationException>(() =>
            WeightValidator.Validate("exp1", variants));

        Assert.Equal(FailureReason.InvalidConfiguration, ex.Reason);
        Assert.Contains("Variant name cannot be empty", ex.Message);
    }

    [Fact]
    public void Validate_ShouldThrow_WhenTotalWeightExceeds100()
    {
        // Arrange
        var variants = new[] { ("A", 60), ("B", 50) };

        // Act & Assert
        var ex = Assert.Throws<ABTestingConfigurationException>(() =>
            WeightValidator.Validate("exp1", variants));

        Assert.Equal(FailureReason.InvalidWeightConfiguration, ex.Reason);
        Assert.Contains("exceeds 100", ex.Message);
    }

    [Fact]
    public void Validate_ShouldThrow_WhenTotalWeightIsZero()
    {
        // Arrange
        var variants = new[] { ("A", 0), ("B", 0) };

        // Act & Assert
        var ex = Assert.Throws<ABTestingConfigurationException>(() =>
            WeightValidator.Validate("exp1", variants));

        Assert.Equal(FailureReason.InvalidWeightConfiguration, ex.Reason);
        Assert.Contains("Total weight is 0", ex.Message);
    }

    [Fact]
    public void Validate_ShouldNormalizeWeights_WhenTotalIsLessThan100()
    {
        // Arrange
        // Toplam 80. A: %25 (20/80), B: %75 (60/80)
        var variants = new[] { ("A", 20), ("B", 60) };

        // Act
        var result = WeightValidator.Validate("exp1", variants);

        // Assert
        Assert.True(result.WasNormalized);
        Assert.Equal(80, result.OriginalSum);
        Assert.Equal(2, result.ValidVariants.Count);

        // Normalizasyon kontrolü: 20/80 = %25, 60/80 = %75
        Assert.Equal(25, result.ValidVariants[0].NormalizedWeight);
        Assert.Equal(75, result.ValidVariants[1].NormalizedWeight);
    }

    [Fact]
    public void Validate_ShouldNotNormalize_WhenTotalIs100()
    {
        // Arrange
        var variants = new[] { ("A", 40), ("B", 60) };

        // Act
        var result = WeightValidator.Validate("exp1", variants);

        // Assert
        Assert.False(result.WasNormalized);
        Assert.Equal(100, result.OriginalSum);

        Assert.Equal(40, result.ValidVariants[0].NormalizedWeight);
        Assert.Equal(60, result.ValidVariants[1].NormalizedWeight);
    }

    [Fact]
    public void Validate_ShouldHandleRoundingCorrectly_DuringNormalization()
    {
        // Arrange
        // Toplam 3. A: 1/3 (~33.3), B: 1/3 (~33.3), C: 1/3 (~33.3)
        // Son eleman yuvarlama farkını (100 - toplam) almalı.
        var variants = new[] { ("A", 1), ("B", 1), ("C", 1) };

        // Act
        var result = WeightValidator.Validate("exp1", variants);

        // Assert
        Assert.True(result.WasNormalized);

        // 1/3 -> 33, 1/3 -> 33, Sonuncusu: 100 - 66 = 34
        Assert.Equal(33, result.ValidVariants[0].NormalizedWeight);
        Assert.Equal(33, result.ValidVariants[1].NormalizedWeight);
        Assert.Equal(34, result.ValidVariants[2].NormalizedWeight);

        Assert.Equal(100, result.ValidVariants.Sum(v => v.NormalizedWeight));
    }
}