using Core.Ods.Enums;

namespace Core.Ods.Abstractions;

public interface IOdsCsvIngestionStrategy
{
    Task Ingest(OdsCsvDownloadSource odsCsvSource, Stream downloadStream);
}