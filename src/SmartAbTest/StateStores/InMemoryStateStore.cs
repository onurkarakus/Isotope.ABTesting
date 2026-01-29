using SmartAbTest.Abstractions.Storage;
using SmartAbTest.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAbTest.StateStores;

public sealed class InMemoryStateStore : IStateStore, IDisposable
{
    private readonly ConcurrentDictionary<string, InMemoryCacheEntry> _cache = new();
    private readonly Timer? _cleanupTimer;
    private readonly TimeSpan _cleanupInterval;
    private bool _disposed;

    public InMemoryStateStore() : this(TimeSpan.FromMinutes(5))
    {
    }

    public InMemoryStateStore(TimeSpan cleanupInterval)
    {
        _cleanupInterval = cleanupInterval;
        _cleanupTimer = new Timer(
            callback: _ => CleanupExpiredEntries,
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

    public Task<bool> ExistAsync(string key, CancellationToken cancellationToken = default)
    {

    }



}
