namespace Infrastructure.AzureTableStorageCache.Model;

using System;
using Azure;
using Azure.Data.Tables;

public class CachedItem : ITableEntity
{

    public string PartitionKey { get; set; } = null!;

    public string RowKey { get; set; } = null!;

    public DateTimeOffset? Timestamp { get; set; }

    public ETag ETag { get; set; }

    public byte[]? Data { get; set; }

    public DateTimeOffset AbsoluteExpiration { get; set; }

}