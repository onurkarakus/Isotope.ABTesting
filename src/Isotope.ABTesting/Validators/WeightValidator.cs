using Isotope.ABTesting.Exceptions;
using Isotope.ABTesting.Models;

namespace Isotope.ABTesting.Validators;

public static class WeightValidator
{
    public static WeightValidationResult Validate(string experimentId, (string Name, int Weight)[] variants)
    {
        if (variants.Length < 2)
        {
            throw ExceptionFactory.InsufficientVariants(experimentId, variants.Length);
        }

        foreach (var variant in variants)
        {
            if (variant.Weight < 0)
            {
                throw ExceptionFactory.NegativeWeight(experimentId, variant.Name, variant.Weight);
            }
        }

        var dublicateNames = variants
            .GroupBy(v => v.Name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(g => g.Count() > 1)?.Key;

        if (dublicateNames is not null)
        {
            throw ExceptionFactory.DuplicateVariants(experimentId, dublicateNames);
        }

        var emptyNameVariant = variants.FirstOrDefault(v => string.IsNullOrWhiteSpace(v.Name));

        if (emptyNameVariant != default)
        {
            throw ExceptionFactory.EmptyVariantNames(experimentId);
        }

        var sum = variants.Sum(v => v.Weight);

        if (sum > 100)
        {
            throw ExceptionFactory.WeightSumExceeds100(experimentId, sum);
        }

        if (sum == 0)
        {
            throw ExceptionFactory.SumEqualsZero(experimentId);
        }

        var wasNormalized = sum < 100;

        var normalizedVariants = wasNormalized
            ? NormalizeWeights(variants, sum)
            : CreateVariantDefinitions(variants);

        return new WeightValidationResult(
             validVariants: normalizedVariants,
             wasNormalized: wasNormalized,
             originalSum: sum);
    }

    private static List<VariantDefinition> NormalizeWeights((string Name, int Weight)[] variants, int originalSum)
    {
        var result = new List<VariantDefinition>(variants.Length);
        var normalizedSum = 0;

        for (int i = 0; i < variants.Length; i++)
        {
            var variant = variants[i];

            int normalizedWeight;

            if (i == variants.Length -1)
            {
                normalizedWeight = 100 - normalizedSum;
            }
            else
            {
                normalizedWeight = (int)Math.Round((double)variant.Weight / originalSum * 100);
                normalizedSum += normalizedWeight;
            }

            result.Add(new VariantDefinition(
                Name: variant.Name,
                Weight: variant.Weight,
                NormalizedWeight: normalizedWeight));
        }

        return result;
    }

    private static List<VariantDefinition> CreateVariantDefinitions(
            (string Name, int Weight)[] variants)
    {
        return variants
            .Select(v => new VariantDefinition(
                Name: v.Name,
                Weight: v.Weight,
                NormalizedWeight: v.Weight))
            .ToList();
    }

}
