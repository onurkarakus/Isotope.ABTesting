using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SmartAbTest.Hashing;

/// <summary>
/// MurmurHash3 32-bit implementation.
/// A fast, non-cryptographic hash function with excellent distribution.
/// </summary>
public static class MurmurHash3
{
    private const uint DefaultSeed = 0;

    // MurmurHash3'ün sihirli sabitleri (Algoritmanın imzasıdır)
    private const uint C1 = 0xcc9e2d51;
    private const uint C2 = 0x1b873593;

    /// <summary>
    /// Kullanıcıyı ve deneyi alıp, 0-99 arasında sabit bir "Kova" (Bucket) numarası verir.
    /// Strateji sınıflarının tek muhatap olduğu metot budur.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetBucket(string experimentId, string subjectKey)
    {
        // Allocation yapmamak için string interpolation yerine string.Concat da kullanılabilir 
        // ama modern .NET'te bu yapı zaten oldukça optimize edilmiştir.
        var combined = $"{experimentId}:{subjectKey}";
        var hash = Hash32(combined);

        // Sonucu 0 ile 99 arasına indirgiyoruz.
        return (int)(hash % 100);
    }

    public static uint Hash32(string input, uint seed = DefaultSeed)
    {
        ArgumentNullException.ThrowIfNull(input);

        var bytes = Encoding.UTF8.GetBytes(input);
        return Hash32(bytes, seed);
    }

    public static uint Hash32(byte[] data, uint seed = DefaultSeed)
    {
        ArgumentNullException.ThrowIfNull(data);

        return Hash32(data.AsSpan(), seed);
    }

    /// <summary>
    /// Asıl işi yapan metot (Core Implementation).
    /// Span kullanarak Memory Allocation yapmadan çalışır.
    /// </summary>
    public static uint Hash32(ReadOnlySpan<byte> data, uint seed = DefaultSeed)
    {
        var length = data.Length;
        var h1 = seed;
        var blockCount = length / 4;

        // 1. ADIM: 4'er byte'lık bloklar halinde işle
        for (int i = 0; i < blockCount; i++)
        {
            // 4 byte'ı tek bir uint sayıya çevir (Manuel okuma)
            var k1 = ReadUInt32LittleEndian(data.Slice(i * 4));

            // Karıştırma (Mixing) işlemleri
            k1 *= C1;
            k1 = RotateLeft(k1, 15);
            k1 *= C2;

            h1 ^= k1;
            h1 = RotateLeft(h1, 13);
            h1 = h1 * 5 + 0xe6546b64;
        }

        // 2. ADIM: Kalan byte'ları işle (Tail processing)
        // Eğer verinin uzunluğu 4'ün katı değilse artan byte'lar burada işlenir.
        var tailIndex = blockCount * 4;
        uint k2 = 0;

        switch (length & 3)
        {
            case 3:
                k2 ^= (uint)data[tailIndex + 2] << 16;
                goto case 2;
            case 2:
                k2 ^= (uint)data[tailIndex + 1] << 8;
                goto case 1;
            case 1:
                k2 ^= data[tailIndex];
                k2 *= C1;
                k2 = RotateLeft(k2, 15);
                k2 *= C2;
                h1 ^= k2;
                break;
        }

        // 3. ADIM: Son karıştırma (Finalization)
        // Hash'in dağılım kalitesini (Avalanche effect) artıran son vuruşlar.
        h1 ^= (uint)length;
        h1 = FMix32(h1);

        return h1;
    }

    // --- YARDIMCI METOTLAR (INLINED) ---

    /// <summary>
    /// Bitleri sola kaydırır, taşanları sağdan geri sokar (Circular Shift).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint RotateLeft(uint x, int r)
    {
        return (x << r) | (x >> (32 - r));
    }

    /// <summary>
    /// Final Mix: Bitleri iyice birbirine karıştırır.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint FMix32(uint h)
    {
        h ^= h >> 16;
        h *= 0x85ebca6b;
        h ^= h >> 13;
        h *= 0xc2b2ae35;
        h ^= h >> 16;
        return h;
    }

    /// <summary>
    /// 4 adet byte'ı alıp (byte[0], byte[1]...) tek bir uint sayıya dönüştürür.
    /// BitConverter kullanmak yerine bunu elle yaparak performansı artırıyoruz.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint ReadUInt32LittleEndian(ReadOnlySpan<byte> source)
    {
        return (uint)(source[0] | (source[1] << 8) | (source[2] << 16) | (source[3] << 24));
    }
}
