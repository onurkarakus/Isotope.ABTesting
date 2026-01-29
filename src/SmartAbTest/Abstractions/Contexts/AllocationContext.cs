using SmartAbTest.Models;

namespace SmartAbTest.Abstractions.Contexts;

public sealed record AllocationContext(
    string ExperimentId,
    string SubjectKey,
    IReadOnlyList<VariantDefinition> Variants,
    IServiceProvider ServiceProvider);
