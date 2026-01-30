using Isotope.ABTesting.Abstractions.Contexts;
using Isotope.ABTesting.Abstractions.Results;

namespace Isotope.ABTesting.Abstractions.Fallback;

public interface IFallbackPolicy
{
    ValueTask<AllocationResult> ExecuteAsync(FallbackContext context, CancellationToken cancellationToken = default);

    bool IsStatelessFallback { get; }
}
