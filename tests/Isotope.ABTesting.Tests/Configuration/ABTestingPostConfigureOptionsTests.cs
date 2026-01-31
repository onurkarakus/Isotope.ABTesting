using Isotope.ABTesting.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Isotope.ABTesting.Tests.Configuration;

public class ABTestingPostConfigureOptionsTests
{
    [Fact]
    public void PostConfigure_ShouldSetServiceName_FromConfiguration_WhenNotSet()
    {
        // Arrange
        // appsettings.json simülasyonu
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "DefaultABTestingService", "fallback-service" }
            })
            .Build();

        var logger = Substitute.For<ILogger<ABTestingPostConfigureOptions>>();
        var postConfig = new ABTestingPostConfigureOptions(config, logger);

        var options = new ABTestingOptions { ServiceName = "" }; // Boş verdik

        // Act
        postConfig.PostConfigure(null, options);

        // Assert
        // Config'den okuyup doldurmalı
        Assert.Equal("fallback-service", options.ServiceName);

        // LogInformation çağrıldığını doğrula
        logger.ReceivedWithAnyArgs().Log(LogLevel.Information, default, default, default, default);
    }

    [Fact]
    public void PostConfigure_ShouldDoNothing_WhenServiceNameIsAlreadySet()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();
        var logger = Substitute.For<ILogger<ABTestingPostConfigureOptions>>();
        var postConfig = new ABTestingPostConfigureOptions(config, logger);

        var options = new ABTestingOptions { ServiceName = "existing-service" };

        // Act
        postConfig.PostConfigure(null, options);

        // Assert
        Assert.Equal("existing-service", options.ServiceName);

        // Log atılmamalı
        logger.DidNotReceiveWithAnyArgs().Log(LogLevel.Information, default, default, default, default);
    }

    [Fact]
    public void PostConfigure_ShouldLogWarning_WhenFallbackIsMissing()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build(); // Boş config
        var logger = Substitute.For<ILogger<ABTestingPostConfigureOptions>>();
        var postConfig = new ABTestingPostConfigureOptions(config, logger);

        var options = new ABTestingOptions { ServiceName = "" };

        // Act
        postConfig.PostConfigure(null, options);

        // Assert
        // LogWarning çağrıldığını doğrula
        logger.ReceivedWithAnyArgs().Log(LogLevel.Warning, default, default, default, default);
    }
}