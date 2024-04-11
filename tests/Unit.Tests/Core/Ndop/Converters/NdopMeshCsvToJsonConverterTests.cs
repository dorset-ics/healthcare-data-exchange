using Core.Ndop.Converters;
using Core.Ndop.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Unit.Tests.Core.Ndop.Converters;

public class NdopMeshCsvToJsonConverterTests
{
    private readonly NdopMeshCsvToJsonConverter _sut = new(Substitute.For<ILogger<NdopMeshCsvToJsonConverter>>());

    [Fact]
    public void Convert_WhenRequestIsNull_ExceptionThrown()
    {
        var result = _sut.Convert(null!);

        result.IsSuccess.ShouldBeFalse();
        result.Exception.ShouldBeOfType<ArgumentNullException>();
    }

    [Fact]
    public void Convert_WhenRequestCsvIsEmpty_ExceptionThrown()
    {
        var request = new NdopMeshConversionRequest("", new List<string>());

        var result = _sut.Convert(request);

        result.IsSuccess.ShouldBeFalse();
        result.Exception.ShouldBeOfType<ApplicationException>();
    }

    [Fact]
    public void Convert_WhenRequestRequestIdsEmpty_EmptyConsentJsonListReturned()
    {
        var request = new NdopMeshConversionRequest("111,\n222,\n", new List<string>());

        var result = _sut.Convert(request);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("{\"consents\":[]}");
    }

    [Fact]
    public void Convert_WhenRequestIdsContainsOptedOutId_ItIsReturnedAsOptedOut()
    {
        var request = new NdopMeshConversionRequest("111,\n222,\n", new List<string> { "111", "222", "333" });

        var result = _sut.Convert(request);

        JToken.Parse(result.Value).ShouldBeOfType<JObject>().ShouldContainKey("consents");
        var consentsString = JToken.Parse(result.Value)["consents"];
        var consents = consentsString!.Select(c => new NdopMeshEnrichedRecordResponse(c["NhsNumber"]!.ToString(), Boolean.Parse(c["IsOptedOut"]!.ToString()))).ToList();
        consents.Count.ShouldBe(3);
        consents.ShouldContain(x => x.NhsNumber == "111" && x.IsOptedOut == false);
        consents.ShouldContain(x => x.NhsNumber == "222" && x.IsOptedOut == false);
        consents.ShouldContain(x => x.NhsNumber == "333" && x.IsOptedOut == true);
    }

    [Fact]
    public void Convert_WhenResponseContainsWhitespaces_CsvParsingIgnoresThem()
    {
        var request = new NdopMeshConversionRequest("\n 111 ,\n222  \n ,\n\n333\n", new List<string> { "111", "222", "333" });

        var result = _sut.Convert(request);

        JToken.Parse(result.Value).ShouldBeOfType<JObject>().ShouldContainKey("consents");
        var consentsString = JToken.Parse(result.Value)["consents"];
        var consents = consentsString!.Select(c => new NdopMeshEnrichedRecordResponse(c["NhsNumber"]!.ToString(), Boolean.Parse(c["IsOptedOut"]!.ToString()))).ToList();
        consents.Count.ShouldBe(3);
    }


    [Fact]
    public void Convert_WhenRequestIdsContainsExtraElements_TheyAreNotIncludedInTheResult()
    {
        var request = new NdopMeshConversionRequest("111,\n222,\n333,\n", new List<string> { "111", "222" });

        var result = _sut.Convert(request);

        JToken.Parse(result.Value).ShouldBeOfType<JObject>().ShouldContainKey("consents");
        var consentsString = JToken.Parse(result.Value)["consents"];
        var consents = consentsString!.Select(c => new NdopMeshEnrichedRecordResponse(c["NhsNumber"]!.ToString(), Boolean.Parse(c["IsOptedOut"]!.ToString()))).ToList();
        consents.Count.ShouldBe(2);
    }
}