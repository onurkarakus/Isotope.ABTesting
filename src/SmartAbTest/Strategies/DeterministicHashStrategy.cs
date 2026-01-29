using SmartAbTest.Abstractions.Contexts;
using SmartAbTest.Abstractions.Results;
using SmartAbTest.Abstractions.Strategies;
using SmartAbTest.Enums;
using SmartAbTest.Hashing;

namespace SmartAbTest.Strategies;

public sealed class DeterministicHashStrategy : IAllocationStrategy
{
    // Bu strateji durumsuzdur (Stateless). Redis'e ihtiyaç duymaz.
    // Çünkü "User X" her zaman "Bucket Y"ye düşer. Bunu bir yere kaydetmeye gerek yoktur.
    public bool RequiresState => false;

    // Aynı girdi her zaman aynı çıktıyı verir.
    // Sticky Session (yapışkan oturum) mekanizmasına gerek kalmaz.
    public bool IsDeterministic => true;

    public ValueTask<AllocationResult> AllocateAsync(AllocationContext context, 
        CancellationToken cancellationToken = default)
    {
        // 1. GÜVENLİK KONTROLÜ (Guard Clause)
        // Eğer varyant listesi boşsa işlem yapamayız.
        if (context.Variants == null || context.Variants.Count == 0)
        {
            return ValueTask.FromResult(AllocationResult.Empty(
                experimentId: context.ExperimentId,
                subjectKey: context.SubjectKey,
                allocationSource: AllocationSource.Fallback));
        }

        // 2. KADER HESAPLAMA (Hashing)
        // MurmurHash3 motorunu çağırıyoruz. Bize 0-99 arasında bir sayı veriyor.
        // Bu sayı, kullanıcının o deneydeki değişmez kimliğidir.
        var bucket = MurmurHash3.GetBucket(context.ExperimentId, context.SubjectKey);

        // 3. SEÇİM ALGORİTMASI (Cumulative Weighting)
        // Varyantlar: A(%20), B(%30), C(%50) olsun.
        // 0-19 -> A
        // 20-49 -> B
        // 50-99 -> C
        var cumulative = 0;

        foreach (var variant in context.Variants)
        {
            // Validasyon sınıfında (WeightValidator) hesaplattığımız normalize edilmiş ağırlığı kullanıyoruz.
            cumulative += variant.Weight;

            // Eğer kullanıcının şans numarası (bucket), şu anki sınırın altındaysa
            // KAZANAN VARYANT BULUNDU!
            if (bucket < cumulative)
            {
                var result = AllocationResult.From(
                    variant: variant,
                    experimentId: context.ExperimentId,
                    subjectKey: context.SubjectKey,
                    allocationSource: AllocationSource.Calculated);

                return new ValueTask<AllocationResult>(result);
            }
        }

        // 4. EMNİYET SÜBAPI (Fallback)
        // Matematiksel olarak toplam ağırlık 100 ise buraya asla düşmemeliyiz.
        // Ama floating point hatası vb. risklere karşı son varyantı seçiyoruz.
        var fallbackVariant = context.Variants[^1];

        return new ValueTask<AllocationResult>(
            AllocationResult.From(
                fallbackVariant,
                context.ExperimentId,
                context.SubjectKey,
                AllocationSource.Calculated));
    }
}
