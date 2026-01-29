namespace SmartAbTest.Enums;

public enum FailureReason
{
    Unknown = 0,
    StateStoreUnavailable,       // Redis yok
    StateStoreTimeout,           // Redis zaman aşımı
    InvalidConfiguration,        // Genel konfigürasyon hatası
    MissingConfiguration,        // Eksik ayar
    InvalidAlgorithmFallbackCombination, // Reservoir + Stateless Fallback hatası gibi
    InvalidWeightConfiguration,  // Ağırlık toplamı > 100 veya negatif
    IncompleteExperimentConfiguration, // Build edilmemiş deney
    VariantNotFound,             // İstenen varyant yok
    KeyResolutionFailed,         // SubjectKey bulunamadı
    AlgorithmExecutionFailed,    // Strateji patladı
    FallbackExecutionFailed,     // Fallback de patladı
    ReservoirFull,            // Kota doldu
    StrategyResolutionFailed // Strateji DI konteynerde bulunamadı
}
