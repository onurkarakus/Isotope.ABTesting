using SmartAbTest.Abstractions.Contexts;
using SmartAbTest.Abstractions.Results;

namespace SmartAbTest.Abstractions.Fallback;

public interface IFallbackPolicy
{
    ValueTask<AllocationResult> ExecuteAsync(
        FallbackContext context,
        CancellationToken cancellationToken = default);

    bool IsStatelessFallback { get; }
}
