using SmartAbTest.Enums;

namespace SmartAbTest.Exceptions;

public class ABTestingException: Exception
{
    public string? ExperimentId { get; }

    public FailureReason Reason { get; }

    public ABTestingException(string message)
        : base(message)
    {
        Reason = FailureReason.Unknown;
    }

    public ABTestingException(string message, FailureReason reason)
        : base(message)
    {
        Reason = reason;
    }

    public ABTestingException(string experimentId, string message, FailureReason reason)
        : base(message)
    {
        ExperimentId = experimentId;
        Reason = reason;
    }

    public ABTestingException(string message, FailureReason reason, Exception innerException)
        : base(message, innerException)
    {
        Reason = reason;
    }

    public ABTestingException(
       string experimentId,
       string message,
       FailureReason reason,
       Exception innerException)
       : base(message, innerException)
    {
        ExperimentId = experimentId;
        Reason = reason;
    }
}
