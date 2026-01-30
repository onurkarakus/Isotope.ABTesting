namespace Isotope.ABTesting.Configuration;

public sealed class ABTestingOptions
{
    public const string SectionName = "ABTesting";

    /// </summary>
    public string? ServiceName { get; set; }

    public TimeSpan DefaultTtl { get; set; } = TimeSpan.FromMinutes(30);

    public LoggingOptions Logging { get; set; } = new();  
}
