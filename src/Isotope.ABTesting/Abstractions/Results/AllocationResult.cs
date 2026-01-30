using Isotope.ABTesting.Enums;
using Isotope.ABTesting.Models;

namespace Isotope.ABTesting.Abstractions.Results;

public sealed record AllocationResult
{
    public VariantDefinition? Variant { get; init; }

    public bool IsAssigned => Variant is not null;

    public string ExperimentId { get; init; } = default!;

    public string SubjectKey { get; init; } = default!;

    public AllocationSource AllocationSource { get; init; }

    public static AllocationResult From(VariantDefinition variant, string experimentId, string subjectKey, AllocationSource allocationSource) =>
        new()
        {
            Variant = variant,
            ExperimentId = experimentId,
            SubjectKey = subjectKey,
            AllocationSource = allocationSource
        };

    public static AllocationResult Empty(string experimentId, string subjectKey, AllocationSource allocationSource) =>
        new()
        {
            Variant = null,
            ExperimentId = experimentId,
            SubjectKey = subjectKey,
            AllocationSource = allocationSource
        };
}
