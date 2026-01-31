using Isotope.ABTesting.Hashing;
using Xunit;

namespace Isotope.ABTesting.Tests.Hashing;

public class MurmurHash3Tests
{
    [Theory]
    // Standart MurmurHash3 (x86, 32-bit, seed:0) bilinen çıktıları
    [InlineData("test", 3127628307)]
    [InlineData("hello", 613153351)]
    [InlineData("", 0)] // Boş string 0 dönmeli
    public void Hash32_ShouldReturnKnownValues(string input, uint expected)
    {
        // Act
        var result = MurmurHash3.Hash32(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetBucket_ShouldBeDeterministic()
    {
        // Arrange
        var expId = "button-color-experiment";
        var subject = "user-102938";

        // Act - Aynı girdilerle iki kez çağır
        var bucket1 = MurmurHash3.GetBucket(expId, subject);
        var bucket2 = MurmurHash3.GetBucket(expId, subject);

        // Assert - Sonuçlar birebir aynı olmalı
        Assert.Equal(bucket1, bucket2);
    }

    [Theory]
    [InlineData("exp1", "user1")]
    [InlineData("exp1", "user2")]
    [InlineData("exp1", "user3")]
    [InlineData("exp2", "user1")]
    [InlineData("very-long-experiment-name-with-lots-of-characters", "very-long-user-id-1234567890")]
    public void GetBucket_ShouldReturnValuesBetween0And99(string expId, string subject)
    {
        // Act
        var bucket = MurmurHash3.GetBucket(expId, subject);

        // Assert - Yüzdelik dilim hesabı olduğu için 0-99 arası olmalı
        Assert.InRange(bucket, 0, 99);
    }

    [Fact]
    public void Hash32_ShouldThrowArgumentNullException_WhenInputIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => MurmurHash3.Hash32((string)null!));
    }

    [Fact]
    public void Hash32_Bytes_ShouldThrowArgumentNullException_WhenInputIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => MurmurHash3.Hash32((byte[])null!));
    }
}