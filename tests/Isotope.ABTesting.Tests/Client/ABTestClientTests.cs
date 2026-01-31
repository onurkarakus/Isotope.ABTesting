using Isotope.ABTesting.Abstractions.Storage;
using Isotope.ABTesting.Builders;
using Isotope.ABTesting.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Isotope.ABTesting.Tests.Client;

public class ABTestClientTests
{
    private readonly IStateStore _stateStore = Substitute.For<IStateStore>();
    private readonly IServiceProvider _serviceProvider = Substitute.For<IServiceProvider>();
    private readonly ILogger<ExperimentBuilder> _logger = Substitute.For<ILogger<ExperimentBuilder>>();
    private readonly IOptions<ABTestingOptions> _options = Options.Create(new ABTestingOptions());

    [Fact]
    public void Constructor_ShouldThrow_WhenArgumentsAreNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ABTestClient(null!, _serviceProvider, _logger, _options));
        Assert.Throws<ArgumentNullException>(() => new ABTestClient(_stateStore, null!, _logger, _options));
        Assert.Throws<ArgumentNullException>(() => new ABTestClient(_stateStore, _serviceProvider, null!, _options));
        Assert.Throws<ArgumentNullException>(() => new ABTestClient(_stateStore, _serviceProvider, _logger, null!));
    }

    [Fact]
    public void Experiment_ShouldReturnBuilder_WhenIdIsValid()
    {
        // Arrange
        var client = new ABTestClient(_stateStore, _serviceProvider, _logger, _options);

        // Act
        var builder = client.Experiment("test-exp");

        // Assert
        Assert.NotNull(builder);
        Assert.IsType<ExperimentBuilder>(builder);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Experiment_ShouldThrow_WhenIdIsInvalid(string invalidId)
    {
        // Arrange
        var client = new ABTestClient(_stateStore, _serviceProvider, _logger, _options);

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => client.Experiment(invalidId));
    }
}