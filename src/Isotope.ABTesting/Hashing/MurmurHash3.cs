using System.Runtime.CompilerServices;
using System.Text;

namespace Isotope.ABTesting.Hashing;

public static class MurmurHash3
{
    private const uint DefaultSeed = 0;

    private const uint C1 = 0xcc9e2d51;
    private const uint C2 = 0x1b873593;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetBucket(string experimentId, string subjectKey)
    {
        var combined = $"{experimentId}:{subjectKey}";
        var hash = Hash32(combined);

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

    public static uint Hash32(ReadOnlySpan<byte> data, uint seed = DefaultSeed)
    {
        var length = data.Length;
        var h1 = seed;
        var blockCount = length / 4;

        for (int i = 0; i < blockCount; i++)
        {
            var k1 = ReadUInt32LittleEndian(data.Slice(i * 4));

            k1 *= C1;
            k1 = RotateLeft(k1, 15);
            k1 *= C2;

            h1 ^= k1;
            h1 = RotateLeft(h1, 13);
            h1 = h1 * 5 + 0xe6546b64;
        }

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

        h1 ^= (uint)length;
        h1 = FMix32(h1);

        return h1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint RotateLeft(uint x, int r)
    {
        return (x << r) | (x >> (32 - r));
    }

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint ReadUInt32LittleEndian(ReadOnlySpan<byte> source)
    {
        return (uint)(source[0] | (source[1] << 8) | (source[2] << 16) | (source[3] << 24));
    }
}
