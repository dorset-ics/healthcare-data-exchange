using Core.Common.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Task = System.Threading.Tasks.Task;

namespace Integration.Tests.Infrastructure.AzureTableStorageCache;

public class AzureTableStorageCacheTests : IDisposable
{
    private readonly DistributedCacheEntryOptions _distributedCacheEntryOptions =
        new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) };

    private readonly IDistributedCache _azureTableCache;
    private readonly ApiWebApplicationFactory _webApplicationFactory;

    public AzureTableStorageCacheTests()
    {
        _webApplicationFactory = new ApiWebApplicationFactory();
        _azureTableCache = _webApplicationFactory.Services.GetService<IDistributedCache>()
                           ?? throw new Exception("Failed to resolve IDistributedCache from the service provider.");

    }

    [Fact]
    public async Task WhenSettingAListRecordItIsReturnedOnGet()
    {
        var key = "key";
        var value = new List<string> { "value1", "value2" };
        await _azureTableCache.SetAsync(key, value, _distributedCacheEntryOptions);
        var result = await _azureTableCache.GetAsync<List<string>>(key);
        result.ShouldNotBeNull();
        result.ShouldBe(value);
    }

    [Fact]
    public async Task WhenSettingABooleanRecordItIsReturnedOnGet()
    {
        var key = "key";
        var value = true;
        await _azureTableCache.SetAsync(key, value, _distributedCacheEntryOptions);
        var result = await _azureTableCache.GetAsync<Boolean>(key);
        result.ShouldBe(value);
    }

    [Fact]
    public async Task WhenSettingAStringRecordItIsReturnedOnGet()
    {
        var key = "key";
        var value = "value";
        await _azureTableCache.SetAsync(key, value, _distributedCacheEntryOptions);
        var result = await _azureTableCache.GetAsync<String>(key);
        result.ShouldNotBeNull();
        result.ShouldBe(value);
    }

    [Fact]
    public async Task WhenSettingAValueWithShortExpirationRecordIsNotFetchedAfterItsExpired()
    {
        var key = "key";
        var value = new List<string> { "value1", "value2" };
        await _azureTableCache.SetAsync(key, value,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMicroseconds(1) });
        await Task.Delay(1000);
        var result = await _azureTableCache.GetAsync<List<string>>(key);
        result.ShouldBeNull();
    }


    [Fact]
    public async Task WhenSettingAValueWithShortExpirationRecordIsIsRefreshedOnAnotherSet()
    {
        var keyToRefresh = "keyToRefresh";
        var value = "value";
        await _azureTableCache.SetAsync(keyToRefresh, value,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMicroseconds(1) });
        await Task.Delay(1000);
        await _azureTableCache.SetAsync("key", value,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMicroseconds(1) });
        var result = await _azureTableCache.GetAsync<List<string>>(keyToRefresh);
        result.ShouldBeNull();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(500)]
    [InlineData(1000)]
    [InlineData(5000)]
    public async Task SettingMultipleLengthNhsIdsLists(int recordNumber)
    {
        var key = "key";
        await _azureTableCache.SetAsync(key, GetMockNhsNumbers(recordNumber), _distributedCacheEntryOptions);
        var result = await _azureTableCache.GetAsync<List<string>>(key);
        result.ShouldNotBeNull();
        result.Count.ShouldBe(recordNumber);
    }

    [Fact]
    public async Task SettingLongLengthNhsIdsListsFailsOnRecordSize()
    {
        await _azureTableCache.SetAsync("key", GetMockNhsNumbers(10000), _distributedCacheEntryOptions).ShouldThrowAsync<Azure.RequestFailedException>();
    }

    [Fact]
    public async Task WhenSettingAValueTwiceForTheSameKeyThenTheUpdatedValueIfFetchedOnGet()
    {
        var key = "key";
        var value = "value";
        await _azureTableCache.SetAsync(key, "oldValue", _distributedCacheEntryOptions);
        await _azureTableCache.SetAsync(key, value, _distributedCacheEntryOptions);
        var result = await _azureTableCache.GetAsync<string>(key);
        result.ShouldNotBeNull();
        result.ShouldBe(value);
    }

    [Fact]
    public async Task WhenSettingPastExpirationThenExceptionIsThrown()
    {
        await _azureTableCache.SetAsync("key", "value", new() { AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(-1) }).ShouldThrowAsync<Exception>();
    }

    [Fact]
    public void WhenCreatingDistributedCacheEntryOptionsExpirationToNowThenExceptionIsThrown()
    {
        try
        {
            _ = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.Zero };
        }
        catch (Exception e)
        {
            e.ShouldNotBeNull();
            e.ShouldBeOfType<ArgumentOutOfRangeException>();
        }
    }

    private static List<string> GetMockNhsNumbers(int recordNumber)
    {
        var r = new Random();
#pragma warning disable CA5394
        string s = string.Empty;
        return Enumerable.Range(0, recordNumber).Select(x =>
        {
            string s = string.Empty;
            for (int i = 0; i < 10; i++)
                s = String.Concat(s, r.Next(10).ToString());
            return s;
        }).ToList();
#pragma warning restore CA5394
    }


    public void Dispose()
    {
        _webApplicationFactory.Dispose();
        GC.SuppressFinalize(this);
    }

}