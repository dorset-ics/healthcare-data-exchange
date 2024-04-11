using Microsoft.Extensions.Logging;

namespace Unit.Tests;

public static class TestExtensions
{
    public static void AnyLogOfType<T>(this ILogger<T> logger, LogLevel level) where T : class
    {
        logger.Log(level, Arg.Any<EventId>(), Arg.Any<object>(), Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}