namespace SmartAbTest.Configuration;

public sealed class LoggingOptions
{
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Sampling rate for logs (0.0 to 1.0).
    /// 0.1 means log only 10% of allocations.
    /// </summary>
    public double SamplingRate { get; set; } = 1.0;

    /// <summary>
    /// Hash subject keys in logs for PII protection.
    /// "user@email.com" becomes "a1b2..."
    /// </summary>
    public bool HashSubjectKey { get; set; } = true;

    public bool IncludeSubjectKey { get; set; } = true;
    public bool LogOnlyNewAllocations { get; set; } = false;
}
