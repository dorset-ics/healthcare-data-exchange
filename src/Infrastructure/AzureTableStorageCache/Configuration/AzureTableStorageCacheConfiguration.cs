namespace Infrastructure.AzureTableStorageCache.Configuration;

public record AzureTableStorageCacheConfiguration(string ConnectionString, string TableName, string PartitionKey, bool CreateTableIfNotExists, string? Endpoint = null)
{
    public const string SectionKey = "AzureTableStorageCache";
};