using System.Net;
using Quartz.Logging;

namespace Integration.Tests.Api.Modules;

public class InternalModuleTests : IDisposable
{
    private readonly HttpClient _client;
    private readonly ApiWebApplicationFactory _webApplicationFactory;

    public InternalModuleTests()
    {
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
    public async Task GivenValidPayload_WhenInternalRunOdsIsCalled_ThenRunSuccessfully()
    {
        var response = await _client.PostAsync("/internal/run/ods", null);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GivenValidPayload_WhenInternalRunPdsIsCalled_ThenRunSuccessfully()
    {
        var response = await _client.PostAsync("/internal/run/pds", null);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}