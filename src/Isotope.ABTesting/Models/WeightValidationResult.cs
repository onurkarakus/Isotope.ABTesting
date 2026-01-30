namespace Isotope.ABTesting.Models;

public sealed class WeightValidationResult
{
    public IReadOnlyList<VariantDefinition> ValidVariants { get; }

    public bool WasNormalized { get; }

    public int OriginalSum { get; }

    public WeightValidationResult(IReadOnlyList<VariantDefinition> validVariants, bool wasNormalized, int originalSum)
    {
        ValidVariants = validVariants;
        WasNormalized = wasNormalized;
        OriginalSum = originalSum;
    }

}
