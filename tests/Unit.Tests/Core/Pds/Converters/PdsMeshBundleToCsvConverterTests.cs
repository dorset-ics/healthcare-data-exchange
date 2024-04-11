using Core.Pds.Converters;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;

namespace Unit.Tests.Core.Pds.Converters;

public class PdsMeshBundleToCsvConverterTests
{
    private const string PdsMeshHeaders =
        "UNIQUE REFERENCE,NHS_NO,FAMILY_NAME,GIVEN_NAME,OTHER_GIVEN_NAME,GENDER,DATE_OF_BIRTH,POSTCODE,DATE_OF_DEATH,ADDRESS_LINE1,ADDRESS_LINE2,ADDRESS_LINE3,ADDRESS_LINE4,ADDRESS_LINE5,ADDRESS_DATE,GP_PRACTICE_CODE,GP_REGISTRATION_DATE,NHAIS_POSTING_ID,AS_AT_DATE,LOCAL_PATIENT_ID,INTERNAL_ID,TELEPHONE_NUMBER,MOBILE_NUMBER,EMAIL_ADDRESS\r\n";

    private readonly PdsMeshBundleToCsvConverter _pdsMeshBundleToCsvConverter;

    public PdsMeshBundleToCsvConverterTests()
    {
        var loggerMock = Substitute.For<ILogger<PdsMeshBundleToCsvConverter>>();
        _pdsMeshBundleToCsvConverter = new PdsMeshBundleToCsvConverter(loggerMock);
    }

    [Fact]
    public void Convert_WhenBundleIsNull_ShouldThrow()
    {
        var result = _pdsMeshBundleToCsvConverter.Convert(null!);

        result.IsSuccess.ShouldBeFalse();
        result.Exception.ShouldBeOfType<ArgumentNullException>();
    }

    [Fact]
    public void Convert_WhenBundleIsEmpty_ShouldReturnEmptyCsv()
    {
        var bundle = new Bundle();

        var result = _pdsMeshBundleToCsvConverter.Convert(bundle);
        result.Value.Csv.ShouldBe(PdsMeshHeaders);
    }

    [Fact]
    public void Convert_WhenBundleContainsNoEntries_ShouldReturnEmptyCsv()
    {
        var bundle = new Bundle { Entry = [] };

        var result = _pdsMeshBundleToCsvConverter.Convert(bundle);
        result.Value.Csv.ShouldBe(PdsMeshHeaders);
    }

    [Fact]
    public void Convert_WhenBundleContainsNoPatientEntries_ShouldThrow()
    {
        var bundle = new Bundle
        {
            Entry =
            [
                new Bundle.EntryComponent { Resource = new Patient() }
            ]
        };

        Should.Throw<Exception>(() => _pdsMeshBundleToCsvConverter.Convert(bundle));
    }

    [Fact]
    public void Convert_WhenBundleContainsNoPatientEntriesWithIdentifiers_ShouldThrow()
    {
        var bundle = new Bundle
        {
            Entry =
            [
                new() { Resource = new Patient { Identifier = [] } }
            ]
        };

        Should.Throw<Exception>(() => _pdsMeshBundleToCsvConverter.Convert(bundle));
    }

    [Fact]
    public void Convert_WhenBundleContainsNoPatientEntriesWithNhsNumberIdentifier_ShouldReturnCsv()
    {
        var bundle = new Bundle
        {
            Entry =
            [
                new Bundle.EntryComponent
                {
                    Resource = new Patient
                    {
                        Identifier =
                        [
                            new Identifier { System = "https://fhir.nhs.uk/Id/nhs-number", Value = "1234567890" }
                        ]
                    }
                }
            ]
        };

        var result = _pdsMeshBundleToCsvConverter.Convert(bundle);
        result.Value.Csv.ShouldBe($"{PdsMeshHeaders},1234567890,,,,,,,,,,,,,,,,,,,,,,\r\n", result.Value.Csv);
    }
}