using Core.Ndop.Extensions;

namespace Unit.Tests.Core.Ndop.Extensions;

public class NdopExtensionsTests
{
    [Theory]
    [InlineData(2000, 1, 1, 1, 1, 01, "NDOPREQ_20000101010101.dat")]
    [InlineData(2021, 12, 31, 23, 59, 59, "NDOPREQ_20211231235959.dat")]
    [InlineData(1990, 10, 2, 10, 5, 59, "NDOPREQ_19901002100559.dat")]
    public void ToNdopMeshMessageFileName_WhenDateTime_ShouldReturnCorrectFormat(int year, int month, int day, int hour, int minute, int second, string expected)
    {
        var dateTime = new DateTime(year, month, day, hour, minute, second);
        var result = dateTime.ToNdopMeshMessageFileName();
        result.ShouldBe(expected);
    }
}