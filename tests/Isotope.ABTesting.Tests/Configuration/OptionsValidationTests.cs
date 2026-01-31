using Isotope.ABTesting.Configuration;
using Xunit;

namespace Isotope.ABTesting.Tests.Configuration;

public class OptionsValidationTests
{
    // --- ABTestingOptionsValidator Testleri ---

    [Fact]
    public void ABTestingOptionsValidator_ShouldFail_WhenServiceNameIsEmpty()
    {
        var validator = new ABTestingOptionsValidator();
        var options = new ABTestingOptions { ServiceName = "" };

        var result = validator.Validate(null, options);

        Assert.True(result.Failed);
        // DÜZELTME: Parametre sırası değiştirildi (Beklenen, Gerçek)
        Assert.Contains("ServiceName cannot be null or empty", result.FailureMessage);
    }

    [Theory]
    [InlineData("ValidService")] // Büyük harf içeriyor
    [InlineData("1service")] // Rakamla başlıyor
    [InlineData("-service")] // Tire ile başlıyor
    [InlineData("service-")] // Tire ile bitiyor
    [InlineData("service_name")] // Alt çizgi içeriyor
    [InlineData("service name")] // Boşluk içeriyor
    public void ABTestingOptionsValidator_ShouldFail_ForInvalidServiceNames(string serviceName)
    {
        var validator = new ABTestingOptionsValidator();
        var options = new ABTestingOptions { ServiceName = serviceName };

        var result = validator.Validate(null, options);

        Assert.True(result.Failed);
    }

    [Fact]
    public void ABTestingOptionsValidator_ShouldPass_ForValidServiceName()
    {
        var validator = new ABTestingOptionsValidator();
        var options = new ABTestingOptions { ServiceName = "my-valid-service-1" };

        var result = validator.Validate(null, options);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void ABTestingOptionsValidator_ShouldFail_WhenTtlIsNegative()
    {
        var validator = new ABTestingOptionsValidator();
        var options = new ABTestingOptions
        {
            ServiceName = "valid",
            DefaultTtl = TimeSpan.FromMinutes(-1)
        };

        var result = validator.Validate(null, options);

        Assert.True(result.Failed);
        // DÜZELTME: Parametre sırası değiştirildi
        Assert.Contains("DefaultTtl cannot be negative", result.FailureMessage);
    }

    // --- RedisOptionsValidator Testleri ---

    [Fact]
    public void RedisOptionsValidator_ShouldFail_WhenConnectionStringIsEmpty()
    {
        var validator = new RedisOptionsValidator();
        var options = new RedisOptions { ConnectionString = "" };

        var result = validator.Validate(null, options);

        Assert.True(result.Failed);
        // DÜZELTME: Parametre sırası değiştirildi
        Assert.Contains("ConnectionString is required", result.FailureMessage);
    }

    [Fact]
    public void RedisOptionsValidator_ShouldFail_WhenKeyPrefixIsInvalid()
    {
        var validator = new RedisOptionsValidator();

        // Boş prefix
        var result1 = validator.Validate(null, new RedisOptions { ConnectionString = "localhost", KeyPrefix = "" });
        Assert.True(result1.Failed);

        // İki nokta ile bitmeyen prefix
        var result2 = validator.Validate(null, new RedisOptions { ConnectionString = "localhost", KeyPrefix = "test" });
        Assert.True(result2.Failed);
        // DÜZELTME: Parametre sırası değiştirildi
        Assert.Contains("must end with a colon", result2.FailureMessage);
    }

    [Fact]
    public void RedisOptionsValidator_ShouldFail_WhenTimeoutIsTooLow()
    {
        var validator = new RedisOptionsValidator();
        var options = new RedisOptions
        {
            ConnectionString = "localhost",
            KeyPrefix = "test:",
            ConnectTimeout = 100
        };

        var result = validator.Validate(null, options);

        Assert.True(result.Failed);
        // DÜZELTME: Parametre sırası değiştirildi
        Assert.Contains("too low", result.FailureMessage);
    }
}