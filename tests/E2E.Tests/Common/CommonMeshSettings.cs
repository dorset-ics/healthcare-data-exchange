namespace E2E.Tests.Common;

public record CommonMeshSettings(string Url, int MaxChunkSizeInMegabytes)
{
    public const string SectionKey = "Mesh";
}