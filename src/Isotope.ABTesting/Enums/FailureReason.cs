namespace Isotope.ABTesting.Enums;

public enum FailureReason
{
    Unknown = 0,

    StateStoreUnavailable,

    StateStoreTimeout,

    InvalidConfiguration,

    MissingConfiguration,

    InvalidAlgorithmFallbackCombination,

    InvalidWeightConfiguration,

    IncompleteExperimentConfiguration,

    VariantNotFound,

    KeyResolutionFailed,

    AlgorithmExecutionFailed,

    FallbackExecutionFailed,

    ReservoirFull,

    StrategyResolutionFailed
}
