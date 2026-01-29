using SmartAbTest.Abstractions.Contexts;
using SmartAbTest.Abstractions.Fallback;
using SmartAbTest.Abstractions.Results;
using SmartAbTest.Abstractions.Strategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAbTest.Abstractions.Builders;

/// <summary>
/// A/B testi deneylerini yapılandırmak ve çalıştırmak için Fluent Builder arayüzü.
/// </summary>
public interface IExperimentBuilder
{
    // --- KONFİGÜRASYON METOTLARI ---

    /// <summary>
    /// Deneyin varyantlarını ve ağırlıklarını tanımlar.
    /// </summary>
    IExperimentBuilder WithVariants(params (string VariantName, int Weight)[] variants);

    /// <summary>
    /// Sistemde kayıtlı (DI Container) bir stratejiyi kullanır.
    /// Örn: .UseAlgorithm<WeightedRandomStrategy>()
    /// </summary>
    IExperimentBuilder UseAlgorithm<TStrategy>()
        where TStrategy : class, IAllocationStrategy;

    /// <summary>
    /// Stratejiyi kullanırken konfigüre etmemizi sağlar.
    /// </summary>
    IExperimentBuilder UseAlgorithm<TStrategy>(Action<TStrategy> configure)
        where TStrategy : class, IAllocationStrategy;

    /// <summary>
    /// Özel bir fonksiyonu algoritma olarak kullanır (Custom Logic).
    /// </summary>
    IExperimentBuilder UseAlgorithm(
        Func<AllocationContext, CancellationToken, ValueTask<AllocationResult>> algorithm);

    /// <summary>
    /// Hata durumunda ne yapılacağını belirleyen politika.
    /// </summary>
    IExperimentBuilder OnFailure(IFallbackPolicy policy);

    /// <summary>
    /// Hata durumunda çalışacak özel fonksiyon.
    /// </summary>
    IExperimentBuilder OnFailure(
        Func<FallbackContext, CancellationToken, ValueTask<AllocationResult>> fallbackHandler);

    /// <summary>
    /// Sonucu zorla kaydet (Stateless algoritmalar için bile).
    /// </summary>
    IExperimentBuilder RequirePersistence();

    // --- ÇALIŞTIRMA (EXECUTION) METOTLARI ---

    /// <summary>
    /// Yapılandırılan deneyi çalıştırır ve bir varyant seçer.
    /// </summary>
    Task<AllocationResult> GetVariantAsync(
        string subjectKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcı anahtarının asenkron bulunduğu durumlar için.
    /// </summary>
    Task<AllocationResult> GetVariantAsync(
        Func<CancellationToken, Task<string>> keyResolver,
        CancellationToken cancellationToken = default);
}
