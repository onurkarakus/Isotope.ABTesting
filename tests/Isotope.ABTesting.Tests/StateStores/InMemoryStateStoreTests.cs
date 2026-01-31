using Isotope.ABTesting.StateStores;
using Xunit;

namespace Isotope.ABTesting.Tests.StateStores;

public class InMemoryStateStoreTests
{
    [Fact]
    public async Task SetAsync_ShouldStoreValue()
    {
        // Arrange
        using var store = new InMemoryStateStore();
        var key = "user1";
        var variant = "A";

        // Act
        // Düzeltme: ttl parametresi zorunlu olduğu için 'null' olarak geçiyoruz.
        await store.SetAsync(key, variant, ttl: null);

        // Assert
        var result = await store.GetAsync(key);
        Assert.Equal(variant, result);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenKeyDoesNotExist()
    {
        using var store = new InMemoryStateStore();
        var result = await store.GetAsync("non-existent");
        Assert.Null(result);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenKeyExists()
    {
        using var store = new InMemoryStateStore();
        // Düzeltme: ttl parametresi eklendi
        await store.SetAsync("user1", "A", ttl: null);

        var exists = await store.ExistsAsync("user1");
        Assert.True(exists);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveKey()
    {
        using var store = new InMemoryStateStore();
        // Düzeltme: ttl parametresi eklendi
        await store.SetAsync("user1", "A", ttl: null);

        var deleted = await store.DeleteAsync("user1");
        var exists = await store.ExistsAsync("user1");

        Assert.True(deleted);
        Assert.False(exists);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenItemIsExpired()
    {
        // Arrange
        using var store = new InMemoryStateStore();

        // Burada zaten ttl parametresi veriliyordu, bu kullanım doğru.
        await store.SetAsync("user1", "A", TimeSpan.FromMilliseconds(50));

        // Act
        // Sürenin dolması için bekliyoruz
        await Task.Delay(200);

        var result = await store.GetAsync("user1");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultInterval()
    {
        using var store = new InMemoryStateStore();
        Assert.NotNull(store);
    }
}