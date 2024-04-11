namespace Templates.Tests.DataProviders;

internal static class TestPaths
{
    internal static readonly string InputDirectoryPath =
        Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "input");

    internal static readonly string OutputDirectoryPath =
        Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "output");

    internal static readonly string EnvFilePath =
        Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "docker", ".env");
}