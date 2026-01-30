using Isotope.ABTesting.Abstractions.Contexts;
using Isotope.ABTesting.Abstractions.Fallback;
using Isotope.ABTesting.Abstractions.Results;
using Isotope.ABTesting.Exceptions;

namespace Isotope.ABTesting.Fallbacks;

public sealed class ThrowFallbackPolicy : IFallbackPolicy
{
    public static readonly ThrowFallbackPolicy Instance = new();

    private ThrowFallbackPolicy() { }

    public bool IsStatelessFallback => false;

    public ValueTask<AllocationResult> ExecuteAsync(FallbackContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        throw ExceptionFactory.StateStoreUnavailable(context.ExperimentId, context.OriginalException);
    }
}
