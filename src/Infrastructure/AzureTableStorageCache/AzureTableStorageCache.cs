using Azure.Data.Tables;
using Infrastructure.AzureTableStorageCache.Configuration;
using Infrastructure.AzureTableStorageCache.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.AzureTableStorageCache;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

public class AzureTableStorageCache(TableClient tableClient, ILogger<AzureTableStorageCache> logger, AzureTableStorageCacheConfiguration configuration) : IDistributedCache
{
    private readonly TimeSpan? _expiredItemsDeletionInterval = TimeSpan.FromMinutes(5);
    private DateTimeOffset _lastExpirationScan;
    private readonly string _partitionKey = configuration.PartitionKey;
    private readonly TableClient _tableClient = GetOrCreateTableSet(tableClient, configuration);
    private readonly ILogger _logger = logger;

    public async Task<byte[]?> GetAsync(string key, CancellationToken token)
    {
        await RefreshAsync(key, token).ConfigureAwait(false);
        var item = await RetrieveAsync(key, token).ConfigureAwait(false);
        ScanForExpiredItemsIfRequired();
        return item?.Data;
    }


    public async Task RefreshAsync(string key, CancellationToken token)
    {
        var item = await RetrieveAsync(key, token).ConfigureAwait(false);
        if (item != null)
        {

            if (ShouldDelete(item))
            {
                await _tableClient.DeleteEntityAsync(item.PartitionKey, item.RowKey, cancellationToken: token).ConfigureAwait(false);
                return;
            }
        }
        ScanForExpiredItemsIfRequired();
    }

    public async Task RemoveAsync(string key, CancellationToken token)
    {
        var item = await RetrieveAsync(key, token).ConfigureAwait(false);
        if (item != null)
        {
            await _tableClient.DeleteEntityAsync(item.PartitionKey, item.RowKey, cancellationToken: token)
                .ConfigureAwait(false);
        }

        ScanForExpiredItemsIfRequired();
    }


    public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default(CancellationToken))
    {
        var utcNow = DateTimeOffset.UtcNow;
        var expiresAtTime = GetExpiresAtTime(options, utcNow);

        var item = new CachedItem
        {
            PartitionKey = _partitionKey,
            RowKey = key,
            Data = value,
            AbsoluteExpiration = expiresAtTime
        };

        await _tableClient.UpsertEntityAsync(item, TableUpdateMode.Replace, token);
        ScanForExpiredItemsIfRequired();
    }

    private static DateTimeOffset GetExpiresAtTime(DistributedCacheEntryOptions options, DateTimeOffset currentTime)
    {
        if (options.AbsoluteExpirationRelativeToNow.HasValue)
        {
            return currentTime.Add(options.AbsoluteExpirationRelativeToNow.Value);
        }
        if (options.AbsoluteExpiration.HasValue)
        {
            if (options.AbsoluteExpiration.Value <= currentTime)
            {
                throw new Exception("Absolute expiration value must be in the future.");
            }
            return options.AbsoluteExpiration.Value;
        }
        throw new NotSupportedException("Expecting either 'AbsoluteExpirationRelativeToNow' or 'AbsoluteExpiration' as those are supported in Azure Table Storage.");
    }

    private ValueTask<CachedItem?> RetrieveAsync(string key, CancellationToken token)
    {
        return _tableClient
            .QueryAsync<CachedItem>(e => e.PartitionKey == _partitionKey && e.RowKey == key, maxPerPage: 1,
                cancellationToken: token).FirstOrDefaultAsync(cancellationToken: token);
    }

    private static bool ShouldDelete(CachedItem item)
    {
        return item.AbsoluteExpiration <= DateTimeOffset.UtcNow;
    }

    private void ScanForExpiredItemsIfRequired()
    {

        if (DateTimeOffset.UtcNow - _lastExpirationScan > _expiredItemsDeletionInterval)
        {
            _lastExpirationScan = DateTimeOffset.UtcNow;
            Task.Run(DeleteExpiredCacheItems);
        }
    }

    public byte[]? Get(string key)
    {
        throw new NotImplementedException();
    }

    public void Refresh(string key)
    {
        throw new NotImplementedException();
    }

    public void Remove(string key)
    {
        throw new NotImplementedException();
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        throw new NotImplementedException();
    }

    private static TableClient GetOrCreateTableSet(TableClient tableClient,
        AzureTableStorageCacheConfiguration configuration)
    {
        if (configuration.CreateTableIfNotExists)
        {
            tableClient.CreateIfNotExistsAsync();
        }
        return tableClient;
    }

    private async Task DeleteExpiredCacheItems()
    {
        var itemsToDelete = await _tableClient.QueryAsync<CachedItem>()
            .Where(item => item.PartitionKey == _partitionKey && ShouldDelete(item))
            .ToListAsync()
            .ConfigureAwait(false);

        foreach (var item in itemsToDelete)
        {
            try
            {
                await _tableClient
                    .DeleteEntityAsync(item.PartitionKey, item.RowKey)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to delete expired cache items: {ErrorMessage}", ex.Message);
            }
        }
    }

}