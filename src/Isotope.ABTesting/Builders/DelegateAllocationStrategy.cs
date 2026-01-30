using Isotope.ABTesting.Abstractions.Contexts;
using Isotope.ABTesting.Abstractions.Results;
using Isotope.ABTesting.Abstractions.Strategies;
using Isotope.ABTesting.Configuration;

namespace Isotope.ABTesting.Builders;

internal class DelegateAllocationStrategy : IAllocationStrategy
{
    private readonly Func<AllocationContext, CancellationToken, ValueTask<AllocationResult>> _handler;
    private readonly CustomAlgorithmOptions _options;

    public DelegateAllocationStrategy(Func<AllocationContext, CancellationToken, ValueTask<AllocationResult>> handler, CustomAlgorithmOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(handler);

        _handler = handler;
        _options = options ?? new CustomAlgorithmOptions();
    }

    public bool RequireState => _options.RequireState;

    public bool IsDeterministic => _options.IsDeterministic;

    public bool RequiresState => throw new NotImplementedException();

    public ValueTask<AllocationResult> AllocateAsync(AllocationContext context, CancellationToken cancellationToken = default)
    {
        return _handler(context, cancellationToken);
    }
}
