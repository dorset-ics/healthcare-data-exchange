using CsvHelper.Configuration.Attributes;

namespace Core.Pds.Models;

public record PdsMeshRecordRequest
{
    [Index(0)][Name("UNIQUE REFERENCE")] public required string UniqueReference { get; set; }

    [Index(1)][Name("NHS_NO")] public string? NhsNumber { get; set; }

    [Index(2)][Name("FAMILY_NAME")] public string? FamilyName { get; set; }

    [Index(3)][Name("GIVEN_NAME")] public string? GivenName { get; set; }

    [Index(4)][Name("OTHER_GIVEN_NAME")] public string? OtherGivenName { get; set; }

    [Index(5)][Name("GENDER")] public string? Gender { get; set; }

    [Index(6)][Name("DATE_OF_BIRTH")] public string? DateOfBirth { get; set; }

    [Index(7)][Name("POSTCODE")] public string? Postcode { get; set; }

    [Index(8)][Name("DATE_OF_DEATH")] public string? DateOfDeath { get; set; }

    [Index(9)][Name("ADDRESS_LINE1")] public string? AddressLine1 { get; set; }

    [Index(10)][Name("ADDRESS_LINE2")] public string? AddressLine2 { get; set; }

    [Index(11)][Name("ADDRESS_LINE3")] public string? AddressLine3 { get; set; }

    [Index(12)][Name("ADDRESS_LINE4")] public string? AddressLine4 { get; set; }

    [Index(13)][Name("ADDRESS_LINE5")] public string? AddressLine5 { get; set; }

    [Index(14)][Name("ADDRESS_DATE")] public string? AddressDate { get; set; }

    [Index(15)][Name("GP_PRACTICE_CODE")] public string? GpPracticeCode { get; set; }

    [Index(16)][Name("GP_REGISTRATION_DATE")] public string? GpRegistrationDate { get; set; }

    [Index(17)][Name("NHAIS_POSTING_ID")] public string? NhaisPostingId { get; set; }

    [Index(18)][Name("AS_AT_DATE")] public string? AsAtDate { get; set; }

    [Index(19)][Name("LOCAL_PATIENT_ID")] public string? LocalPatientId { get; set; }

    [Index(20)][Name("INTERNAL_ID")] public string? InternalId { get; set; }

    [Index(21)][Name("TELEPHONE_NUMBER")] public string? TelephoneNumber { get; set; }

    [Index(22)][Name("MOBILE_NUMBER")] public string? MobileNumber { get; set; }

    [Index(23)][Name("EMAIL_ADDRESS")] public string? EmailAddress { get; set; }
}