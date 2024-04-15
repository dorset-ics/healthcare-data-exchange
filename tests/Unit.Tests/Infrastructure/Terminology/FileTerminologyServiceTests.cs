using Infrastructure.Terminology;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Unit.Tests.Infrastructure.Terminology;

public class FileTerminologyServiceTests
{
    [Fact]
    public void GetSnomedDisplay_ShouldReturnSnomedDescription_WhenCodeIsInCache()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var logger = Substitute.For<ILogger<FileTerminologyService>>();
        memoryCache.Set("22298006", "Myocardial infarction (disorder)");

        var terminologyService = new FileTerminologyService(memoryCache, logger);

        var result = terminologyService.GetSnomedDisplay("22298006");
        result.ShouldBe("Myocardial infarction (disorder)");
    }

    [Fact]
    public void GetSnomedDisplay_ShouldReturnEmptyString_WhenCodeIsNotInCache()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var logger = Substitute.For<ILogger<FileTerminologyService>>();

        var terminologyService = new FileTerminologyService(memoryCache, logger);

        var result = terminologyService.GetSnomedDisplay("12345678");
        result.ShouldBe(string.Empty);
    }

    [Fact]
    public void GetSnomedDisplay_ShouldLogWarning_WhenCodeIsNotInCache()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var logger = Substitute.For<ILogger<FileTerminologyService>>();

        var terminologyService = new FileTerminologyService(memoryCache, logger);
        var result = terminologyService.GetSnomedDisplay("12345678");

        logger.Received(1).AnyLogOfType(LogLevel.Warning);
    }
}