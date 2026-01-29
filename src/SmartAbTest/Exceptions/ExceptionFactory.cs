using SmartAbTest.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAbTest.Exceptions;

internal static class ExceptionFactory
{
    public static ABTestingConfigurationException WeightSumExceeds100(string experimentId, int actualSum)
    {
        return new ABTestingConfigurationException(
            experimentId,
            propertyName: "Variants",
            message: $"Experiment '{experimentId}': Total weight ({actualSum}) exceeds 100. " +
                     $"Reduce variant weights so they sum to 100 or less.",
            reason: FailureReason.InvalidWeightConfiguration);
    }

    public static ABTestingConfigurationException NegativeWeight(string experimentId, string variantName, int weight)
    {
        return new ABTestingConfigurationException(
            experimentId,
            propertyName: "Variants",
            message: $"Experiment '{experimentId}': Variant '{variantName}' has negative weight ({weight}). " +
                     $"Weight must be 0 or greater.",
            reason: FailureReason.InvalidWeightConfiguration);
    }

    public static ABTestingConfigurationException InsufficientVariants(
        string experimentId,
        int variantCount)
    {
        return new ABTestingConfigurationException(
            experimentId,
            propertyName: "Variants",
            message: $"Experiment '{experimentId}': At least 2 variants required, but only {variantCount} provided. " +
                     $"An A/B test needs at least 2 variants to compare.",
            reason: FailureReason.InvalidConfiguration);
    }

    public static ABTestingConfigurationException MissingConfiguration(
        string experimentId,
        string propertyName)
    {
        var methodName = propertyName switch
        {
            "Variants" => "WithVariants()",
            "Algorithm" => "UseAlgorithm()",
            "FallbackPolicy" => "OnFailure()",
            _ => propertyName
        };

        return new ABTestingConfigurationException(
            experimentId,
            propertyName: propertyName,
            message: $"Experiment '{experimentId}': {methodName} must be called before GetVariantAsync().",
            reason: FailureReason.IncompleteExperimentConfiguration);
    }

    public static ABTestingConfigurationException DuplicateVariants(string experimentId, string duplicateName)
    {
        throw new ABTestingConfigurationException(
                experimentId,
                propertyName: "Variants",
                message: $"Experiment '{experimentId}': Duplicate variant name '{duplicateName}'. " +
                         $"Each variant must have a unique name.",
                reason: FailureReason.InvalidConfiguration);
    }

    public static ABTestingConfigurationException EmptyVariantNames(string experimentId)
    {
        throw new ABTestingConfigurationException(
                experimentId,
                propertyName: "Variants",
                message: $"Experiment '{experimentId}': Variant name cannot be empty or whitespace.",
                reason: FailureReason.InvalidConfiguration);
    }

    public static ABTestingConfigurationException SumEqualsZero(string experimentId)
    {
        throw new ABTestingConfigurationException(
                experimentId,
                propertyName: "Variants",
                message: $"Experiment '{experimentId}': Total weight is 0. " +
                         $"At least one variant must have a positive weight.",
                reason: FailureReason.InvalidWeightConfiguration);
    }

    public static ABTestingConfigurationException StrategyResolutionFailed(
        string experimentId,
        Type strategyType,
        Exception innerException)
    {
        return new ABTestingConfigurationException(
            experimentId,
            propertyName: "Algorithm",
            message: $"Experiment '{experimentId}': Could not resolve or create an instance of strategy type '{strategyType.FullName}'.",
            reason: FailureReason.StrategyResolutionFailed,
            innerException: innerException);
    }

    public static ABTestingException StateStoreUnavailable(string experimentId, Exception? innerException)
    {
        return new ABTestingException(
            experimentId,
            $"Experiment '{experimentId}': State store is unavailable and fallback policy is set to Throw. " +
            "Either ensure Redis is available or use a different fallback policy (e.g., DefaultVariant).",
            FailureReason.StateStoreUnavailable,
            innerException ?? new InvalidOperationException("State store unavailable"));
    }

    public static ABTestingConfigurationException FallbackVariantNotFound(
        string experimentId,
        string variantName,
        IEnumerable<string> availableVariants)
    {
        var available = string.Join(", ", availableVariants.Select(v => $"'{v}'"));

        return new ABTestingConfigurationException(
            experimentId,
            propertyName: "FallbackPolicy",
            message: $"Experiment '{experimentId}': Fallback variant '{variantName}' not found. " +
                     $"Available variants: {available}.",
            reason: FailureReason.VariantNotFound);
    }

    public static ABTestingConfigurationException InvalidAlgorithmFallbackCombination(
        string experimentId,
        string algorithmName,
        string fallbackName)
    {
        return new ABTestingConfigurationException(
            experimentId,
            propertyName: "FallbackPolicy",
            message: $"Experiment '{experimentId}': Algorithm '{algorithmName}' cannot use '{fallbackName}' as fallback. " +
                     $"Stateful algorithms require explicit fallback (Throw, ToVariant, or Custom). " +
                     $"ToStateless is not allowed because the algorithm requires state to function correctly.",
            reason: FailureReason.InvalidAlgorithmFallbackCombination);
    }

    public static ABTestingException FallbackExecutionFailed(
       string experimentId,
       Exception innerException)
    {
        return new ABTestingException(
            experimentId,
            message: $"Experiment '{experimentId}': Fallback policy execution threw an exception. " +
                     $"Both primary operation and fallback failed.",
            reason: FailureReason.FallbackExecutionFailed,
            innerException: innerException);
    }

    public static ABTestingException AlgorithmExecutionFailed(
       string experimentId,
       Exception innerException)
    {
        return new ABTestingException(
            experimentId,
            message: $"Experiment '{experimentId}': Algorithm execution threw an exception.",
            reason: FailureReason.AlgorithmExecutionFailed,
            innerException: innerException);
    }

    public static ABTestingException KeyResolutionFailed(
        string experimentId,
        Exception innerException)
    {
        return new ABTestingException(
            experimentId,
            message: $"Experiment '{experimentId}': Key resolver function threw an exception. " +
                     $"Check the async key resolver implementation.",
            reason: FailureReason.KeyResolutionFailed,
            innerException: innerException);
    }

    public static ABTestingConfigurationException KeyResolverReturnedNull(string experimentId)
    {
        return new ABTestingConfigurationException(
            experimentId: experimentId,
            propertyName: "SubjectKey",
            message: $"Key resolver returned null or empty subject key for experiment '{experimentId}'.",
            reason: FailureReason.KeyResolutionFailed);
    }
}
