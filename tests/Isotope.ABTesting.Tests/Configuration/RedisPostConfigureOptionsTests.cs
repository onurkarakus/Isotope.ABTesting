using Isotope.ABTesting.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Isotope.ABTesting.Tests.Configuration;

public class RedisPostConfigureOptionsTests
{
    [Fact]
    public void PostConfigure_ShouldSetConnectionString_FromConfiguration_WhenNotSet()
    {
        // Arrange
        // appsettings.json simülasyonu: "DefaultRedisConnectionString" anahtarı var
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "DefaultRedisConnectionString", "localhost:6379" }
            })
            .Build();

        var logger = Substitute.For<ILogger<RedisPostConfigureOptions>>();
        var postConfig = new RedisPostConfigureOptions(config, logger);

        var options = new RedisOptions { ConnectionString = "" }; // Boş verdik

        // Act
        postConfig.PostConfigure(null, options);

        // Assert
        // Config'den okuyup doldurmalı
        Assert.Equal("localhost:6379", options.ConnectionString);

        // Bilgilendirme logu atılmalı
        logger.ReceivedWithAnyArgs().Log(LogLevel.Information, default, default, default, default);
    }

    [Fact]
    public void PostConfigure_ShouldDoNothing_WhenConnectionStringIsAlreadySet()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();
        var logger = Substitute.For<ILogger<RedisPostConfigureOptions>>();
        var postConfig = new RedisPostConfigureOptions(config, logger);

        var options = new RedisOptions { ConnectionString = "redis.custom:6379" };

        // Act
        postConfig.PostConfigure(null, options);

        // Assert
        Assert.Equal("redis.custom:6379", options.ConnectionString);

        // Log atılmamalı (var olanı ezmiyoruz)
        logger.DidNotReceiveWithAnyArgs().Log(LogLevel.Information, default, default, default, default);
    }

    [Fact]
    public void PostConfigure_ShouldLogWarning_WhenFallbackIsMissing()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build(); // Boş config (Fallback yok)
        var logger = Substitute.For<ILogger<RedisPostConfigureOptions>>();
        var postConfig = new RedisPostConfigureOptions(config, logger);

        var options = new RedisOptions { ConnectionString = "" };

        // Act
        postConfig.PostConfigure(null, options);

        // Assert
        // Uyarı logu atılmalı
        logger.ReceivedWithAnyArgs().Log(LogLevel.Warning, default, default, default, default);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenArgumentsAreNull()
    {
        var config = new ConfigurationBuilder().Build();
        var logger = Substitute.For<ILogger<RedisPostConfigureOptions>>();

        Assert.Throws<ArgumentNullException>(() => new RedisPostConfigureOptions(null!, logger));
        Assert.Throws<ArgumentNullException>(() => new RedisPostConfigureOptions(config, null!));
    }
}