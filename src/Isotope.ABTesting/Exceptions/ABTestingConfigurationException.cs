using Isotope.ABTesting.Enums;

namespace Isotope.ABTesting.Exceptions;

public sealed class ABTestingConfigurationException : Exception
{
    public string? ExperimentId { get; }

    public string? PropertyName { get; }

    public FailureReason Reason { get; }


    public ABTestingConfigurationException(string experimentId, string propertyName, string message, FailureReason reason)
        : base(message)
    {
        ExperimentId = experimentId;
        PropertyName = propertyName;
        Reason = reason;
    }

    public ABTestingConfigurationException(string experimentId, string propertyName, string message, FailureReason reason, Exception innerException)
        : base(message, innerException)
    {
        ExperimentId = experimentId;
        PropertyName = propertyName;
        Reason = reason;
    }
}
