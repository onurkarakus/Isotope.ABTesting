namespace Isotope.ABTesting.Models;

public sealed record VariantDefinition(string Name, int Weight, int NormalizedWeight);
