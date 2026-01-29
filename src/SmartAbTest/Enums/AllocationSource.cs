namespace SmartAbTest.Enums;

public enum AllocationSource
{
    Calculated, // Algoritma hesapladı
    Cached,     // Redis'ten geldi
    Fallback    // Hata oldu, default değer dönüldü
}
