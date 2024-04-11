using Infrastructure.Terminology;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Unit.Tests.Infrastructure.Terminology;

public class FileTerminologyServiceTests
{
    [Fact]
    public void GetSnomedDisplay_ShouldReturnSnomedDescription_WhenCodeIsInCache()
    {
        var memoryCache = Substitute.For<IMemoryCache>();
        var logger = Substitute.For<ILogger<FileTerminologyService>>();

        var anyStringArg = Arg.Any<string>();
        memoryCache.TryGetValue("123", out anyStringArg).Returns(x =>
        {
            x[1] = "Example code display";
            return true;
        });

        var terminologyService = new FileTerminologyService(memoryCache, logger);

        var result = terminologyService.GetSnomedDisplay("123");
        result.ShouldBe("Example code display");
    }

    [Fact]
    public void GetSnomedDisplay_ShouldReturnEmptyString_WhenCodeIsNotInCache()
    {
        var memoryCache = Substitute.For<IMemoryCache>();
        var logger = Substitute.For<ILogger<FileTerminologyService>>();

        var anyStringArg = Arg.Any<string>();
        memoryCache.TryGetValue("123", out anyStringArg).Returns(false);

        var terminologyService = new FileTerminologyService(memoryCache, logger);

        var result = terminologyService.GetSnomedDisplay("123");
        result.ShouldBe(string.Empty);
    }

    [Fact]
    public void GetSnomedDisplay_ShouldLogWarning_WhenCodeIsNotInCache()
    {
        var memoryCache = Substitute.For<IMemoryCache>();
        var logger = Substitute.For<ILogger<FileTerminologyService>>();

        var anyStringArg = Arg.Any<string>();
        memoryCache.TryGetValue("123", out anyStringArg).Returns(false);

        var terminologyService = new FileTerminologyService(memoryCache, logger);
        var result = terminologyService.GetSnomedDisplay("123");

        logger.Received(1).AnyLogOfType(LogLevel.Warning);
    }
}