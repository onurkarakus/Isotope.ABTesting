using Isotope.ABTesting.Abstractions.Contexts;
using Isotope.ABTesting.Abstractions.Fallback;
using Isotope.ABTesting.Abstractions.Results;
using Isotope.ABTesting.Abstractions.Strategies;

namespace Isotope.ABTesting.Abstractions.Builders;

public interface IExperimentBuilder
{
    IExperimentBuilder WithVariants(params (string VariantName, int Weight)[] variants);

    IExperimentBuilder UseAlgorithm<TStrategy>()
        where TStrategy : class, IAllocationStrategy;

    IExperimentBuilder UseAlgorithm<TStrategy>(Action<TStrategy> configure)
        where TStrategy : class, IAllocationStrategy;

    IExperimentBuilder UseAlgorithm(
        Func<AllocationContext, CancellationToken, ValueTask<AllocationResult>> algorithm);

    IExperimentBuilder OnFailure(IFallbackPolicy policy);

    IExperimentBuilder OnFailure(
        Func<FallbackContext, CancellationToken, ValueTask<AllocationResult>> fallbackHandler);

    IExperimentBuilder RequirePersistence();

    Task<AllocationResult> GetVariantAsync(string subjectKey, CancellationToken cancellationToken = default);

    Task<AllocationResult> GetVariantAsync(Func<CancellationToken, Task<string>> keyResolver, CancellationToken cancellationToken = default);
}
