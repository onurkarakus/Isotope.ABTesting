using Isotope.ABTesting.Abstractions.Fallback;
using Isotope.ABTesting.Abstractions.Strategies;
using Microsoft.Extensions.Logging;
using Isotope.ABTesting.Exceptions;

namespace Isotope.ABTesting.Validators;

public static class AlgorithmFallbackValidator
{
    public static void Validate(
        string experimentId,
        IAllocationStrategy algorithm,
        IFallbackPolicy fallback,
        ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(algorithm);
        ArgumentNullException.ThrowIfNull(fallback);

        var algorithmName = algorithm.GetType().Name;
        var fallbackName = fallback.GetType().Name;

        if (algorithm.RequiresState && fallback.IsStatelessFallback)
        {
            throw ExceptionFactory.InvalidAlgorithmFallbackCombination(
                experimentId,
                algorithmName,
                fallbackName);
        }

        if (!algorithm.IsDeterministic && fallback.IsStatelessFallback && logger != null)
        {
            logger.LogError(
                "Experiment {ExperimentId} is using a non-deterministic algorithm {AlgorithmName} with a stateless fallback {FallbackName}. This may lead to inconsistent user experiences.",
                experimentId,
                algorithmName,
                fallbackName);            
        }
    }
}
