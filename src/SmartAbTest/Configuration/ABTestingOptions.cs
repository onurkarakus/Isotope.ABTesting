namespace SmartAbTest.Configuration;

/// <summary>
/// Main configuration options for A/B testing.
/// Bind this from appsettings.json section "ABTesting".
/// </summary>
public sealed class ABTestingOptions
{
    public const string SectionName = "ABTesting";

    /// <summary>
    /// Critical: Isolates experiments across microservices.
    /// Must be unique per service (e.g., "order-api", "payment-service").
    /// </summary>
    public string? ServiceName { get; set; }

    public TimeSpan DefaultTtl { get; set; } = TimeSpan.FromDays(30);

    public LoggingOptions Logging { get; set; } = new();  
}
