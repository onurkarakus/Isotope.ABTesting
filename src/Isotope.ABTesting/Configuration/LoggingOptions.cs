namespace Isotope.ABTesting.Configuration;

public sealed class LoggingOptions
{
    public bool Enabled { get; set; } = true;

    public double SamplingRate { get; set; } = 1.0;

    public bool HashSubjectKey { get; set; } = true;

    public bool IncludeSubjectKey { get; set; } = true;

    public bool LogOnlyNewAllocations { get; set; } = false;
}
