namespace Core.Ods.Abstractions;

public interface IOdsService
{
    Task IngestCsvDownloads(CancellationToken cancellationToken);
}