using Isotope.ABTesting.Models;

namespace Isotope.ABTesting.Abstractions.Contexts;

public sealed record AllocationContext(string ExperimentId, string SubjectKey, IReadOnlyList<VariantDefinition> Variants, IServiceProvider ServiceProvider);
