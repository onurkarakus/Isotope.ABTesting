using Isotope.ABTesting.Abstractions.Contexts;
using Isotope.ABTesting.Abstractions.Results;

namespace Isotope.ABTesting.Abstractions.Strategies;

public interface IAllocationStrategy
{
    bool RequiresState { get; }

    bool IsDeterministic { get; }

    public ValueTask<AllocationResult> AllocateAsync(AllocationContext context, CancellationToken cancellationToken = default);
}
