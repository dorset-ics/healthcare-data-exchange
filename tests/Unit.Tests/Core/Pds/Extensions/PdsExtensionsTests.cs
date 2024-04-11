using Core.Pds.Extensions;
using Core.Pds.Models;
using Hl7.Fhir.Model;

namespace Unit.Tests.Core.Pds.Extensions;

public class PdsExtensionsTests
{
    [Fact]
    public void ToPdsMeshRecord_WhenResourceIsNotPatient_ShouldThrowArgumentException()
    {
        var resource = new Observation();
        var exception = Should.Throw<ArgumentException>(() => resource.ToPdsMeshRecord());
    }

    [Fact]
    public void ToPdsMeshRecord_WhenPatientDoesNotHaveNhsNumber_ShouldThrowApplicationException()
    {
        var resource = new Patient();
        var exception = Should.Throw<ApplicationException>(() => resource.ToPdsMeshRecord());
    }

    [Fact]
    public void ToPdsMeshRecord_WhenPatientHasNhsNumber_ShouldReturnPdsMeshRecord()
    {
        var resource = new Patient
        {
            Identifier =
            [
                new Identifier { System = "https://fhir.nhs.uk/Id/nhs-number", Value = "1234567890" }
            ]
        };
        var result = resource.ToPdsMeshRecord();
        result.ShouldBeOfType<PdsMeshRecordRequest>();
    }

    [Theory]
    [InlineData(2000, 1, 1, 1, 1, 01, "MPTREQ_20000101010101.csv")]
    [InlineData(2021, 12, 31, 23, 59, 59, "MPTREQ_20211231235959.csv")]
    [InlineData(1990, 10, 2, 10, 5, 59, "MPTREQ_19901002100559.csv")]
    public void ToPdsMeshMessageFileName_WhenDateTime_ShouldReturnCorrectFormat(int year, int month, int day, int hour, int minute, int second, string expected)
    {
        var dateTime = new DateTime(year, month, day, hour, minute, second);
        var result = dateTime.ToPdsMeshMessageFileName();
        result.ShouldBe(expected);
    }
}