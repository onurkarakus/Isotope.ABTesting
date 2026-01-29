using SmartAbTest.Abstractions.Contexts;
using SmartAbTest.Abstractions.Fallback;
using SmartAbTest.Abstractions.Results;
using SmartAbTest.Hashing;
using SmartAbTest.Models;

namespace SmartAbTest.Fallbacks;

public class StatelessFallbackPolicy : IFallbackPolicy
{
    public static readonly StatelessFallbackPolicy Instance = new();

    private StatelessFallbackPolicy() { }

    public bool IsStatelessFallback => true;

    public ValueTask<AllocationResult> ExecuteAsync(
        FallbackContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var variantDefinition = CalculateVariant(context);

        return ValueTask.FromResult(AllocationResult.From(variantDefinition, context.ExperimentId, context.SubjectKey, Enums.AllocationSource.Fallback));
    }

    private static VariantDefinition CalculateVariant(FallbackContext context)
    {
        var bucket = MurmurHash3.GetBucket(context.ExperimentId, context.SubjectKey);

        var cumulative = 0;

        foreach (var variant in context.Variants)
        {
            cumulative += variant.NormalizedWeight;

            if (bucket < cumulative)
            {
                return variant;
            }
        }

        return context.Variants[^1];
    }
}
