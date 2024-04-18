using CsvHelper.Configuration.Attributes;

namespace Core.Pds.Models;

public class PdsMeshRecordResponse
{
    [Index(0)][Name("UNIQUE REFERENCE")] public string? UniqueReference { get; set; }

    [Index(1)][Name("REQ_NHS_NUMBER")] public string? NhsNumber { get; set; }

    [Index(2)][Name("FAMILY_NAME")] public string? FamilyName { get; set; }

    [Index(3)][Name("GIVEN_NAME")] public string? GivenName { get; set; }

    [Index(4)][Name("OTHER_GIVEN_NAME")] public string? OtherGivenName { get; set; }

    [Index(5)][Name("GENDER")] public string? Gender { get; set; }

    [Index(6)][Name("DATE_OF_BIRTH")] public string? DateOfBirth { get; set; }

    [Index(7)][Name("DATE_OF_DEATH")] public string? DateOfDeath { get; set; }

    [Index(8)][Name("ADDRESS_LINE1")] public string? AddressLine1 { get; set; }

    [Index(9)][Name("ADDRESS_LINE2")] public string? AddressLine2 { get; set; }

    [Index(10)][Name("ADDRESS_LINE3")] public string? AddressLine3 { get; set; }

    [Index(11)][Name("ADDRESS_LINE4")] public string? AddressLine4 { get; set; }

    [Index(12)][Name("ADDRESS_LINE5")] public string? AddressLine5 { get; set; }

    [Index(13)][Name("POSTCODE")] public string? Postcode { get; set; }

    [Index(14)][Name("GP_PRACTICE_CODE")] public string? GpPracticeCode { get; set; }

    [Index(15)]
    [Name("GP_REGISTRATION_DATE")]
    public string? GpRegistrationDate { get; set; }

    [Index(16)][Name("NHAIS_POSTING_ID")] public string? NhaisPostingId { get; set; }

    [Index(17)][Name("AS_AT_DATE")] public string? AsAtDate { get; set; }

    [Index(18)][Name("LOCAL_PATIENT_ID")] public string? LocalPatientId { get; set; }

    [Index(19)][Name("INTERNAL_ID")] public string? InternalId { get; set; }

    [Index(20)][Name("TELEPHONE_NUMBER")] public string? TelephoneNumber { get; set; }

    [Index(21)][Name("MOBILE_NUMBER")] public string? MobileNumber { get; set; }

    [Index(22)][Name("EMAIL_ADDRESS")] public string? EmailAddress { get; set; }

    [Index(23)][Name("SENSITIVITY_FLAG")] public string? SensitivityFlag { get; set; }

    [Index(24)][Name("MPS_ID")] public string? MpsId { get; set; }

    [Index(25)]
    [Name("ERROR/SUCCESS_CODE")]
    public string? ErrorSuccessCode { get; set; }

    [Index(26)][Name("MATCHED_NHS_NO")] public string? MatchedNhsNo { get; set; }

    [Index(27)]
    [Name("MATCHED_ALGORITHM_INDICATOR")]
    public string? MatchedAlgorithmIndicator { get; set; }

    [Index(28)]
    [Name("MATCHED_CONFIDENCE_PERCENTAGE")]
    public string? MatchedConfidencePercentage { get; set; }
}