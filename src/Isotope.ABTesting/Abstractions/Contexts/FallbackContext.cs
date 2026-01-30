using Isotope.ABTesting.Models;

namespace Isotope.ABTesting.Abstractions.Contexts;

public sealed record FallbackContext(string ExperimentId, string SubjectKey, IReadOnlyList<VariantDefinition> Variants, Exception? OriginalException, IServiceProvider ServiceProvider);
