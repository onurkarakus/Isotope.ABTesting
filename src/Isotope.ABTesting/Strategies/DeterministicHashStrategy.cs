using Isotope.ABTesting.Abstractions.Contexts;
using Isotope.ABTesting.Abstractions.Results;
using Isotope.ABTesting.Abstractions.Strategies;
using Isotope.ABTesting.Enums;
using Isotope.ABTesting.Hashing;

namespace Isotope.ABTesting.Strategies;

public sealed class DeterministicHashStrategy : IAllocationStrategy
{
    public bool RequiresState => false;

    public bool IsDeterministic => true;

    public ValueTask<AllocationResult> AllocateAsync(AllocationContext context, 
        CancellationToken cancellationToken = default)
    {
        if (context.Variants == null || context.Variants.Count == 0)
        {
            return ValueTask.FromResult(AllocationResult.Empty(
                experimentId: context.ExperimentId,
                subjectKey: context.SubjectKey,
                allocationSource: AllocationSource.Fallback));
        }

        var bucket = MurmurHash3.GetBucket(context.ExperimentId, context.SubjectKey);

        var cumulative = 0;

        foreach (var variant in context.Variants)
        {
            cumulative += variant.Weight;

            if (bucket < cumulative)
            {
                var result = AllocationResult.From(
                    variant: variant,
                    experimentId: context.ExperimentId,
                    subjectKey: context.SubjectKey,
                    allocationSource: AllocationSource.Calculated);

                return new ValueTask<AllocationResult>(result);
            }
        }
               
        var fallbackVariant = context.Variants[^1];

        return new ValueTask<AllocationResult>(
            AllocationResult.From(
                fallbackVariant,
                context.ExperimentId,
                context.SubjectKey,
                AllocationSource.Calculated));
    }
}
