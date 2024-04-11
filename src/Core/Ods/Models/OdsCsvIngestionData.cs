using Core.Common.Models;
using Core.Ods.Enums;

namespace Core.Ods.Models;

public record OdsCsvIngestionData(
    string Headers,
    Type ClassType,
    string CsvData,
    TemplateInfo OrgTemplateInfo
    )
{

    private static readonly Dictionary<OdsCsvDownloadSource, string> OdsCsvDownloadTemplateMap = new Dictionary<OdsCsvDownloadSource, string> {
        {OdsCsvDownloadSource.EnglandAndWales, "england-and-wales"},
        {OdsCsvDownloadSource.Scotland, "scotland"},
        {OdsCsvDownloadSource.NorthernIreland, "northern-ireland"}
    };

    public static OdsCsvIngestionData GetDataBySource(string csvData,
        OdsCsvDownloadSource downloadSource)
    {
        switch (downloadSource)
        {
            case OdsCsvDownloadSource.EnglandAndWales:
                return new OdsCsvIngestionData(
                    "ORGANISATIONCODE,NAME,NATIONAL GROUPING,HIGH LEVEL HEALTH GEOGRAPHY,ADDRESS LINE 1,ADDRESS LINE 2,ADDRESS LINE 3,ADDRESS LINE 4,ADDRESS LINE 5,POSTCODE,OPEN DATE,CLOSE DATE,,,,,,,,,,AMENDED RECORD INDICATOR,,GOR CODE",
                    typeof(EnglandWalesCsvResponse),
                    csvData,
                    TemplateInfo.ForOdsCsvDownloadCountry(OdsCsvDownloadTemplateMap[OdsCsvDownloadSource.EnglandAndWales])
                );

            case OdsCsvDownloadSource.Scotland:
                return new OdsCsvIngestionData(
                    "HospitalCode,HospitalName,AddressLine1,AddressLine2,AddressLine2QF,AddressLine3,AddressLine3QF,AddressLine4,AddressLine4QF,Postcode,HealthBoard,HSCP,CouncilArea,IntermediateZone,DataZone",
                   typeof(ScotlandCsvResponse),
                    csvData,
                    TemplateInfo.ForOdsCsvDownloadCountry(OdsCsvDownloadTemplateMap[OdsCsvDownloadSource.Scotland])
                );

            case OdsCsvDownloadSource.NorthernIreland:
                return new OdsCsvIngestionData(
                    "ORGANISATIONCODE,NAME,,HIGH LEVEL HEALTH GEOGRAPHY,ADDRESS LINE 1,ADDRESS LINE 2,ADDRESS LINE 3,ADDRESS LINE 4,ADDRESS LINE 5,POSTCODE,OPEN DATE,CLOSE DATE,STATUS CODE,ORGANISATION SUBTYPE CODE,PARENT ORGANISATION CODE,,,CONTACT TELEPHONE NUMBER,,,,AMENDED RECORD INDICATOR,,",
                    typeof(NorthernIrelandCsvResponse),
                    csvData,
                    TemplateInfo.ForOdsCsvDownloadCountry(OdsCsvDownloadTemplateMap[OdsCsvDownloadSource.NorthernIreland])
                );
            default:
                throw new NotImplementedException($"{nameof(downloadSource)} download source is not implemented.");
        }
    }
}