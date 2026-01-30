using Isotope.ABTesting.Abstractions.Storage;
using Isotope.ABTesting.Models;
using System.Collections.Concurrent;

namespace Isotope.ABTesting.StateStores;

public sealed class InMemoryStateStore : IStateStore, IDisposable
{
    private readonly ConcurrentDictionary<string, InMemoryCacheEntry> _cache = new();
    private readonly Timer? _cleanupTimer;
    private readonly TimeSpan _cleanupInterval;
    private bool _disposed;

    public int Count => _cache.Count;

    public InMemoryStateStore() : this(TimeSpan.FromMinutes(5))
    {
    }

    public InMemoryStateStore(TimeSpan cleanupInterval)
    {
        _cleanupInterval = cleanupInterval;
        _cleanupTimer = new Timer(
            callback: _ => CleanupExpiredEntries(),
            state: null,
            dueTime: _cleanupInterval,
            period: _cleanupInterval);

    }

    public Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.IsExpired)
            {
                _cache.TryRemove(key, out _);

                return Task.FromResult<string?>(null);
            }

            return Task.FromResult<string?>(entry.Value);
        }

        return Task.FromResult<string?>(null);
    }

    public Task SetAsync(string key, string variantName, TimeSpan? ttl, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(variantName);

        var entry = new InMemoryCacheEntry(variantName, ttl);
        _cache[key] = entry;

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.IsExpired)
            {
                _cache.TryRemove(key, out _);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public void Clear()
    {
        _cache.Clear();
    }

    public Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var result = _cache.TryRemove(key, out _);

        return Task.FromResult(result);
    }

    private void CleanupExpiredEntries()
    {
        if (_disposed)
        {
            return;
        }

        var expiredKeys = _cache
            .Where(kvp => kvp.Value.IsExpired)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _cache.TryRemove(key, out _);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _cleanupTimer?.Dispose();
        _cache.Clear();
    }

}
