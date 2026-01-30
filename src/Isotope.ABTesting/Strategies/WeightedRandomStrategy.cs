using Isotope.ABTesting.Abstractions.Contexts;
using Isotope.ABTesting.Abstractions.Results;
using Isotope.ABTesting.Abstractions.Strategies;
using Isotope.ABTesting.Enums;

namespace Isotope.ABTesting.Strategies;

public sealed class WeightedRandomStrategy : IAllocationStrategy
{
    public bool RequiresState => false;

    public bool IsDeterministic => false;

    public ValueTask<AllocationResult> AllocateAsync(AllocationContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var variantInformation = Allocate(context);

        return variantInformation;
    }

    private static ValueTask<AllocationResult> Allocate(AllocationContext context)
    {
        if (context.Variants == null || context.Variants.Count == 0)
        {
            return ValueTask.FromResult(AllocationResult.Empty(
                experimentId: context.ExperimentId,
                subjectKey: context.SubjectKey,
                allocationSource: AllocationSource.Fallback));
        }

        var randomValue = Random.Shared.Next(100);

        var cumulative = 0;

        foreach (var variant in context.Variants)
        {
            cumulative += variant.NormalizedWeight;

            if (randomValue < cumulative)
            {
                return new ValueTask<AllocationResult>(
                    AllocationResult.From(
                        variant,
                        context.ExperimentId,
                        context.SubjectKey,
                        AllocationSource.Calculated));
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
