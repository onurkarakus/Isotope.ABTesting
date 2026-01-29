namespace SmartAbTest.Configuration;

public class RedisOptions
{
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// Key prefix for all A/B testing keys.
    /// Default: "ab:"
    /// Final key format: {KeyPrefix}{ServiceName}:exp:{ExperimentId}:assign:{SubjectKey}
    /// </summary>
    public string KeyPrefix { get; set; } = "ab:";

    public int Database { get; set; } = 0;
    public int ConnectTimeout { get; set; } = 5000;
    public int SyncTimeout { get; set; } = 1000;
    public int RetryCount { get; set; } = 3;
    public bool AbortOnConnectFail { get; set; } = false;
}
