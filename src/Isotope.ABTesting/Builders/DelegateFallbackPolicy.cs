using Isotope.ABTesting.Abstractions.Contexts;
using Isotope.ABTesting.Abstractions.Fallback;
using Isotope.ABTesting.Abstractions.Results;

namespace Isotope.ABTesting.Builders;

public sealed class DelegateFallbackPolicy : IFallbackPolicy
{
    private readonly Func<FallbackContext, CancellationToken, ValueTask<AllocationResult>> _handler;

    public DelegateFallbackPolicy(Func<FallbackContext, CancellationToken, ValueTask<AllocationResult>> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        _handler = handler;
    }

    public bool IsStatelessFallback => false;

    public ValueTask<AllocationResult> ExecuteAsync(FallbackContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        return _handler(context, cancellationToken);
    }
}
