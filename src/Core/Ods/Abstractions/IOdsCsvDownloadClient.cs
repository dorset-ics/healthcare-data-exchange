using Core.Ods.Enums;

namespace Core.Ods.Abstractions;

public interface IOdsCsvDownloadClient
{
    Task<Stream> DownloadOrganisationsFromCsvSource(OdsCsvDownloadSource csvSource, CancellationToken cancellationToken);
}
