using Core.Common.Abstractions.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Infrastructure.Terminology;

public class FileTerminologyService : ITerminologyService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<FileTerminologyService> _logger;

    public FileTerminologyService(IMemoryCache memoryCache, ILogger<FileTerminologyService> logger)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _logger = logger;

        LoadDataIntoCache();
    }

    public string GetSnomedDisplay(string code)
    {
        var valueInCache = _memoryCache.TryGetValue(code, out string? value);
        if (valueInCache)
        {
            return value!;
        }

        _logger.LogWarning("Snomed code {code} not found in cache", code);
        return string.Empty;
    }

    private void LoadDataIntoCache()
    {
        const string filePath = @"Terminology/SnomedCodes.json";
        var jsonContent = File.ReadAllText(filePath);
        var jsonData = JObject.Parse(jsonContent);

        foreach (var (key, value) in jsonData)
        {
            _memoryCache.Set(key, value!.Value<string>(), new MemoryCacheEntryOptions { });
        }
    }

}