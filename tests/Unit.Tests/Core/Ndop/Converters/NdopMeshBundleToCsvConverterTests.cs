using Core.Ndop.Converters;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;

namespace Unit.Tests.Core.Ndop.Converters;

public class NdopMeshBundleToCsvConverterTests
{
    private readonly NdopMeshBundleToCsvConverter _sut = new(Substitute.For<ILogger<NdopMeshBundleToCsvConverter>>());

    [Fact]
    public void Convert_WhenBundleIsNull_ShouldThrow()
    {
        var result = _sut.Convert(null!);

        result.IsSuccess.ShouldBeFalse();
        result.Exception.ShouldBeOfType<ArgumentNullException>();
    }

    [Fact]
    public void Convert_WhenBundleIsEmpty_ShouldReturnEmptyCsv()
    {
        var bundle = new Bundle();

        var result = _sut.Convert(bundle);

        result.Value.Csv.ShouldBe(string.Empty);
        result.Value.NhsNumbers.ShouldBe(Enumerable.Empty<string>());
    }

    [Fact]
    public void Convert_WhenBundleContainsNoEntries_ShouldReturnEmptyCsv()
    {
        var bundle = new Bundle { Entry = [] };

        var result = _sut.Convert(bundle);

        result.Value.Csv.ShouldBe(string.Empty);
        result.Value.NhsNumbers.ShouldBe(Enumerable.Empty<string>());
    }

    [Fact]
    public void Convert_WhenBundleContainsNoPatientEntries_ShouldReturnEmptyCsv()
    {
        var bundle = new Bundle
        {
            Entry =
            [
                new Bundle.EntryComponent { Resource = new Organization() }
            ]
        };

        var result = _sut.Convert(bundle);

        result.Value.Csv.ShouldBe(string.Empty);
        result.Value.NhsNumbers.ShouldBe(Enumerable.Empty<string>());
    }

    [Fact]
    public void Convert_WhenBundleContainsSinglePatient_ShouldReturnCsv()
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
                            new Identifier { System = "https://fhir.nhs.uk/Id/nhs-number", Value = "1111111111" }
                        ]
                    }
                }
            ]
        };

        var result = _sut.Convert(bundle);

        result.Value.Csv.ShouldBe($"1111111111,\r\n", result.Value.Csv);
        result.Value.NhsNumbers.ShouldBe(Enumerable.Repeat("1111111111", 1));
    }

    [Fact]
    public void Convert_WhenBundleContainsMultiplePatients_ShouldReturnCsv()
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
                            new Identifier { System = "https://fhir.nhs.uk/Id/nhs-number", Value = "1111111111" }
                        ]
                    }
                },
                new Bundle.EntryComponent
                {
                    Resource = new Patient
                    {
                        Identifier =
                        [
                            new Identifier { System = "https://fhir.nhs.uk/Id/nhs-number", Value = "2222222222" }
                        ]
                    }
                },
                new Bundle.EntryComponent
                {
                    Resource = new Patient
                    {
                        Identifier =
                        [
                            new Identifier { System = "https://fhir.nhs.uk/Id/nhs-number", Value = "3333333333" }
                        ]
                    }
                }
            ]
        };

        var result = _sut.Convert(bundle);

        result.Value.Csv.ShouldBe($"1111111111,\r\n2222222222,\r\n3333333333,\r\n", result.Value.Csv);
        var expectedNhsNumbers = new[] { "1111111111", "2222222222", "3333333333" };
        result.Value.NhsNumbers.ShouldBe(expectedNhsNumbers);
    }
}