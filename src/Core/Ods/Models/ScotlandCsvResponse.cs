using CsvHelper.Configuration.Attributes;

namespace Core.Ods.Models;

public record ScotlandCsvResponse
{
    [Index(0)][Name("HospitalCode")] public string? HospitalCode { get; set; }

    [Index(1)][Name("HospitalName")] public string? HospitalName { get; set; }

    [Index(2)][Name("AddressLine1")] public string? AddressLine1 { get; set; }

    [Index(3)][Name("AddressLine2")] public string? AddressLine2 { get; set; }

    [Index(4)][Name("AddressLine2QF")] public string? AddressLine2qf { get; set; }

    [Index(5)][Name("AddressLine3")] public string? AddressLine3 { get; set; }

    [Index(6)][Name("AddressLine3QF")] public string? AddressLine3qf { get; set; }

    [Index(7)][Name("AddressLine4")] public string? AddressLine4 { get; set; }

    [Index(8)][Name("AddressLine4QF")] public string? AddressLine4qf { get; set; }

    [Index(9)][Name("Postcode")] public string? PostCode { get; set; }

    [Index(10)][Name("HealthBoard")] public string? HealthBoard { get; set; }

    [Index(11)][Name("HSCP")] public string? Hscp { get; set; }

    [Index(12)][Name("CouncilArea")] public string? CouncilArea { get; set; }

    [Index(13)][Name("IntermediateZone")] public string? IntermediateZone { get; set; }

    [Index(14)][Name("DataZone")] public string? DataZone { get; set; }
}
