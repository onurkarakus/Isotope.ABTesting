using Isotope.ABTesting.Abstractions.Fallback;
using Isotope.ABTesting.Abstractions.Strategies;
using Isotope.ABTesting.Enums;
using Isotope.ABTesting.Exceptions;
using Isotope.ABTesting.Validators;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Isotope.ABTesting.Tests.Validators;

public class AlgorithmFallbackValidatorTests
{
    private readonly IAllocationStrategy _algorithm;
    private readonly IFallbackPolicy _fallback;
    private readonly ILogger _logger;

    public AlgorithmFallbackValidatorTests()
    {
        // Testlerimiz için sahte (mock) nesneler oluşturuyoruz.
        _algorithm = Substitute.For<IAllocationStrategy>();
        _fallback = Substitute.For<IFallbackPolicy>();
        _logger = Substitute.For<ILogger>();
    }

    [Fact]
    public void Validate_ShouldThrow_WhenAlgorithmIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            AlgorithmFallbackValidator.Validate("exp1", null!, _fallback));
    }

    [Fact]
    public void Validate_ShouldThrow_WhenFallbackIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            AlgorithmFallbackValidator.Validate("exp1", _algorithm, null!));
    }

    [Fact]
    public void Validate_ShouldThrow_WhenAlgorithmRequiresState_And_FallbackIsStateless()
    {
        // Arrange
        // Algoritma state istiyor ama fallback stateless. Bu uyumsuz bir kombinasyon.
        _algorithm.RequiresState.Returns(true);
        _fallback.IsStatelessFallback.Returns(true);

        // Act & Assert
        var ex = Assert.Throws<ABTestingConfigurationException>(() =>
            AlgorithmFallbackValidator.Validate("exp1", _algorithm, _fallback));

        Assert.Equal(FailureReason.InvalidAlgorithmFallbackCombination, ex.Reason);
        Assert.Contains("cannot use", ex.Message);
    }

    [Fact]
    public void Validate_ShouldPass_WhenAlgorithmRequiresState_And_FallbackIsNotStateless()
    {
        // Arrange
        // Algoritma state istiyor ve fallback de stateful (veya uyumlu). Sorun yok.
        _algorithm.RequiresState.Returns(true);
        _fallback.IsStatelessFallback.Returns(false);

        // Act
        AlgorithmFallbackValidator.Validate("exp1", _algorithm, _fallback);

        // Assert - Exception fırlatılmamalı
    }

    [Fact]
    public void Validate_ShouldPass_WhenAlgorithmDoesNotRequireState_And_FallbackIsStateless()
    {
        // Arrange
        _algorithm.RequiresState.Returns(false);
        _algorithm.IsDeterministic.Returns(true); // Log düşmemesi için deterministik olsun
        _fallback.IsStatelessFallback.Returns(true);

        // Act
        AlgorithmFallbackValidator.Validate("exp1", _algorithm, _fallback, _logger);

        // Assert - Exception fırlatılmamalı
    }

    [Fact]
    public void Validate_ShouldLog_WhenAlgorithmIsNonDeterministic_And_FallbackIsStateless()
    {
        // Arrange
        // Algoritma deterministik değil (rastgele) ve fallback stateless. 
        // Bu durumda kullanıcı deneyimi tutarsız olabilir, log yazılmalı.
        _algorithm.RequiresState.Returns(false);
        _algorithm.IsDeterministic.Returns(false);
        _fallback.IsStatelessFallback.Returns(true);

        // Act
        AlgorithmFallbackValidator.Validate("exp1", _algorithm, _fallback, _logger);

        // Assert
        // Logger'ın LogError (LogLevel.Error) metodunun çağrıldığını doğruluyoruz.
        _logger.Received().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}