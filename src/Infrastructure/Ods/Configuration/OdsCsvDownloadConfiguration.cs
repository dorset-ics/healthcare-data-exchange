using Core.Ods.Enums;

namespace Infrastructure.Ods.Configuration;

public record OdsCsvDownloadConfiguration(
    string ImportSchedule,
    Dictionary<OdsCsvDownloadSource, string> DownloadLocations)
{
    public const string SectionKey = "Ods:CsvDownload";
};