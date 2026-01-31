using Isotope.ABTesting.Configuration;
using Isotope.ABTesting.StateStores;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using StackExchange.Redis;
using Xunit;

namespace Isotope.ABTesting.Tests.StateStores;

public class RedisStateStoreTests
{
    private readonly IConnectionMultiplexer _connection;
    private readonly IDatabase _database;
    private readonly ILogger<RedisStateStore> _logger;
    private readonly IOptions<RedisOptions> _options;

    public RedisStateStoreTests()
    {
        _connection = Substitute.For<IConnectionMultiplexer>();
        _database = Substitute.For<IDatabase>();
        _logger = Substitute.For<ILogger<RedisStateStore>>();

        _connection.GetDatabase(default, default).ReturnsForAnyArgs(_database);

        _options = Options.Create(new RedisOptions
        {
            ConnectionString = "localhost",
            KeyPrefix = "test:"
        });
    }

    [Fact]
    public async Task GetAsync_ShouldCallRedisStringGet()
    {
        var store = new RedisStateStore(_connection, _options, _logger);
        _database.StringGetAsync((RedisKey)"test:user1", Arg.Any<CommandFlags>())
                 .Returns(Task.FromResult(new RedisValue("VariantA")));

        var result = await store.GetAsync("user1");

        Assert.Equal("VariantA", result);
    }

    [Fact]
    public async Task GetAsync_ShouldThrow_WhenRedisConnectionExceptionOccurs()
    {
        var store = new RedisStateStore(_connection, _options, _logger);
        _database.StringGetAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>())
                 .Returns(Task.FromException<RedisValue>(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "ConnErr")));

        await Assert.ThrowsAsync<RedisConnectionException>(() => store.GetAsync("user1"));

        // LogError çağrıldı mı?
        _logger.Received(1).Log(LogLevel.Error, Arg.Any<EventId>(), Arg.Any<object>(), Arg.Is<Exception>(e => e is RedisConnectionException), Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task GetAsync_ShouldThrow_WhenRedisTimeoutExceptionOccurs()
    {
        var store = new RedisStateStore(_connection, _options, _logger);
        _database.StringGetAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>())
                 .Returns(Task.FromException<RedisValue>(new RedisTimeoutException("Timeout", CommandStatus.Sent)));

        await Assert.ThrowsAsync<RedisTimeoutException>(() => store.GetAsync("user1"));

        _logger.Received(1).Log(LogLevel.Error, Arg.Any<EventId>(), Arg.Any<object>(), Arg.Is<Exception>(e => e is RedisTimeoutException), Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task GetAsync_ShouldThrow_WhenGeneralExceptionOccurs()
    {
        var store = new RedisStateStore(_connection, _options, _logger);
        _database.StringGetAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>())
                 .Returns(Task.FromException<RedisValue>(new Exception("GeneralErr")));

        await Assert.ThrowsAsync<Exception>(() => store.GetAsync("user1"));

        _logger.Received(1).Log(LogLevel.Error, Arg.Any<EventId>(), Arg.Any<object>(), Arg.Is<Exception>(e => e.Message == "GeneralErr"), Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task SetAsync_ShouldCallRedisStringSet()
    {
        var store = new RedisStateStore(_connection, _options, _logger);
        await store.SetAsync("user1", "VariantA", null);

        await _database.Received(1).StringSetAsync((RedisKey)"test:user1", (RedisValue)"VariantA", null, false, Arg.Any<When>(), Arg.Any<CommandFlags>());
    }

    [Fact]
    public async Task SetAsync_ShouldThrow_WhenRedisConnectionExceptionOccurs()
    {
        var store = new RedisStateStore(_connection, _options, _logger);
        _database.StringSetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<TimeSpan?>(), Arg.Any<bool>(), Arg.Any<When>(), Arg.Any<CommandFlags>())
                 .Returns(Task.FromException<bool>(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "ConnErr")));

        await Assert.ThrowsAsync<RedisConnectionException>(() => store.SetAsync("user1", "A"));
    }

    [Fact]
    public async Task SetAsync_ShouldThrow_WhenRedisTimeoutExceptionOccurs()
    {
        var store = new RedisStateStore(_connection, _options, _logger);
        _database.StringSetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<TimeSpan?>(), Arg.Any<bool>(), Arg.Any<When>(), Arg.Any<CommandFlags>())
                 .Returns(Task.FromException<bool>(new RedisTimeoutException("Timeout", CommandStatus.Sent)));

        await Assert.ThrowsAsync<RedisTimeoutException>(() => store.SetAsync("user1", "A"));
    }

    [Fact]
    public async Task SetAsync_ShouldThrow_WhenGeneralExceptionOccurs()
    {
        var store = new RedisStateStore(_connection, _options, _logger);
        _database.StringSetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<TimeSpan?>(), Arg.Any<bool>(), Arg.Any<When>(), Arg.Any<CommandFlags>())
                 .Returns(Task.FromException<bool>(new Exception("GeneralErr")));

        await Assert.ThrowsAsync<Exception>(() => store.SetAsync("user1", "A"));
    }

    [Fact]
    public async Task ExistsAsync_ShouldCallRedisKeyExists()
    {
        var store = new RedisStateStore(_connection, _options, _logger);
        _database.KeyExistsAsync((RedisKey)"test:user1").Returns(Task.FromResult(true));

        Assert.True(await store.ExistsAsync("user1"));
    }

    [Fact]
    public async Task ExistsAsync_ShouldThrow_WhenRedisConnectionExceptionOccurs()
    {
        var store = new RedisStateStore(_connection, _options, _logger);
        _database.KeyExistsAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>())
                 .Returns(Task.FromException<bool>(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "ConnErr")));

        await Assert.ThrowsAsync<RedisConnectionException>(() => store.ExistsAsync("user1"));
    }

    [Fact]
    public async Task ExistsAsync_ShouldThrow_WhenRedisTimeoutExceptionOccurs()
    {
        var store = new RedisStateStore(_connection, _options, _logger);
        _database.KeyExistsAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>())
                 .Returns(Task.FromException<bool>(new RedisTimeoutException("Timeout", CommandStatus.Sent)));

        await Assert.ThrowsAsync<RedisTimeoutException>(() => store.ExistsAsync("user1"));
    }

    [Fact]
    public async Task ExistsAsync_ShouldThrow_WhenGeneralExceptionOccurs()
    {
        var store = new RedisStateStore(_connection, _options, _logger);
        _database.KeyExistsAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>())
                 .Returns(Task.FromException<bool>(new Exception("GeneralErr")));

        await Assert.ThrowsAsync<Exception>(() => store.ExistsAsync("user1"));
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallRedisKeyDelete()
    {
        var store = new RedisStateStore(_connection, _options, _logger);
        _database.KeyDeleteAsync((RedisKey)"test:user1").Returns(Task.FromResult(true));

        Assert.True(await store.DeleteAsync("user1"));
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenRedisConnectionExceptionOccurs()
    {
        var store = new RedisStateStore(_connection, _options, _logger);
        _database.KeyDeleteAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>())
                 .Returns(Task.FromException<bool>(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "ConnErr")));

        await Assert.ThrowsAsync<RedisConnectionException>(() => store.DeleteAsync("user1"));
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenRedisTimeoutExceptionOccurs()
    {
        var store = new RedisStateStore(_connection, _options, _logger);
        _database.KeyDeleteAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>())
                 .Returns(Task.FromException<bool>(new RedisTimeoutException("Timeout", CommandStatus.Sent)));

        await Assert.ThrowsAsync<RedisTimeoutException>(() => store.DeleteAsync("user1"));
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenGeneralExceptionOccurs()
    {
        var store = new RedisStateStore(_connection, _options, _logger);
        _database.KeyDeleteAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>())
                 .Returns(Task.FromException<bool>(new Exception("GeneralErr")));

        await Assert.ThrowsAsync<Exception>(() => store.DeleteAsync("user1"));
    }

    [Fact]
    public void Dispose_ShouldDisposeConnection_WhenNotOwned()
    {
        // OwnsConnection = false (Constructor injection ile)
        var store = new RedisStateStore(_connection, _options, _logger);
        store.Dispose();

        // Constructor ile verildiği için dispose edilmemeli (kodun mantığına göre değişebilir, 
        // ama senin kodunda _ownsConnection = false atanıyor ve Dispose içinde _ownsConnection kontrolü var)
        _connection.DidNotReceive().Dispose();
    }
}