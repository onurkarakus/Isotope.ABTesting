namespace SmartAbTest.Models;

internal sealed class InMemoryCacheEntry
{
    public string Value { get; }
    public DateTimeOffset? ExpiresAt { get; }

    public InMemoryCacheEntry(string value, TimeSpan? ttl)
    {
        Value = value;
        ExpiresAt = ttl.HasValue ? DateTimeOffset.UtcNow.Add(ttl.Value) : null;
    }

    public bool IsExpired => ExpiresAt.HasValue && DateTimeOffset.UtcNow >= ExpiresAt.Value;
}
