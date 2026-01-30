using Isotope.ABTesting.Abstractions.Contexts;
using Isotope.ABTesting.Abstractions.Fallback;
using Isotope.ABTesting.Abstractions.Results;

namespace Isotope.ABTesting.Fallbacks;

public sealed class DefaultVariantFallbackPolicy : IFallbackPolicy
{
    public string VariantName { get; }

    public bool IsStatelessFallback => false;

    public DefaultVariantFallbackPolicy(string variantName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(variantName);
        VariantName = variantName;
    }

    public ValueTask<AllocationResult> ExecuteAsync(FallbackContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var selectedVariant = context.Variants.FirstOrDefault(v => v.Name == VariantName)
            ?? throw new InvalidOperationException($"The variant '{VariantName}' does not exist in experiment '{context.ExperimentId}'.");

        return ValueTask.FromResult(AllocationResult.From(selectedVariant, context.ExperimentId, context.SubjectKey, Enums.AllocationSource.Fallback));
    }
}
