using System.Net;
using Core.Ingestion.Enums;
using Quartz.Logging;

namespace Integration.Tests.Api.Modules;

public class IngestModuleTests : IDisposable
{
    private readonly HttpClient _client;
    private readonly ApiWebApplicationFactory _webApplicationFactory;

    public IngestModuleTests()
    {
        // https://github.com/quartznet/quartznet/issues/1781
        LogProvider.SetCurrentLogProvider(null);
        _webApplicationFactory = new ApiWebApplicationFactory();
        _client = _webApplicationFactory.CreateClient();
    }

    public void Dispose()
    {
        _webApplicationFactory.Dispose();
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GivenInvalidPayload_WhenIngestIsCalled_ThenReturnsBadRequest()
    {
        _client.DefaultRequestHeaders.Add("organisation-code", "Uhd");
        _client.DefaultRequestHeaders.Add("data-type", IngestionDataType.HL7v2.ToString());
        _client.DefaultRequestHeaders.Add("source-domain", "AgyleEd");

        var response = await _client.PostAsync("/$ingest", null);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.ShouldContain("|AR||The hl7 message cannot be empty when provided.");
    }

    [Fact]
    public async Task GivenInvalidIngestionDataType_WhenIngestIsCalled_ThenReturnsBadRequest()
    {
        _client.DefaultRequestHeaders.Add("organisation-code", "Uhd");
        _client.DefaultRequestHeaders.Add("data-type", ((IngestionDataType)999).ToString());
        _client.DefaultRequestHeaders.Add("source-domain", "AgyleEd");

        var content = new StringContent("content");

        var response = await _client.PostAsync("/$ingest", content);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.ShouldContain("IngestionDataType 999 is not supported");
    }

    [Fact]
    public async Task GivenInvalidIngestionMessage_WhenIngestIsCalled_ThenReturnsBadRequest()
    {
        _client.DefaultRequestHeaders.Add("organisation-code", "Uhd");
        _client.DefaultRequestHeaders.Add("data-type", IngestionDataType.HL7v2.ToString());
        _client.DefaultRequestHeaders.Add("source-domain", "AgyleEd");


        var content = new StringContent("MSH|^~\\\\&|AGYLEED|R0D02|INTEGRATION-ENGINE|RDZ|20231127125907||ADT^A01|667151|P|2.4|||AL|NE");

        var response = await _client.PostAsync("/$ingest", content);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.ShouldContain("MSA|AR|667151|The message must be a valid HL7v2 message when the Data Type is HL7v2.");
    }
}
