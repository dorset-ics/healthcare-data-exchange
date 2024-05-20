using Api.Modules;
using Core.Ods.Abstractions;
using Core.Pds.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Integration.Tests.Api.Modules;

public class InternalModuleTests
{
    [Fact]
    public void GivenValidPayload_WhenInternalRunOdsIsCalled_ThenRunSuccessfully()
    {
        var loggerMock = Substitute.For<ILogger<InternalModule>>();
        var odsService = Substitute.For<IOdsService>();
        var httpContext = Substitute.For<HttpContext>();
        var result = InternalModule.RunOds(httpContext, odsService, loggerMock);

        odsService.Received().IngestCsvDownloads(httpContext.RequestAborted);
        result.IsCompletedSuccessfully.ShouldBe(true);
    }

    [Fact]
    public void GivenValidPayload_WhenInternalRunPdsIsCalled_ThenRunSuccessfully()
    {
        var loggerMock = Substitute.For<ILogger<InternalModule>>();
        var pdsService = Substitute.For<IPdsService>();
        var httpContext = Substitute.For<HttpContext>();
        var result = InternalModule.RunPds(httpContext, pdsService, loggerMock);

        pdsService.Received().RetrieveMeshMessages(httpContext.RequestAborted);
        result.IsCompletedSuccessfully.ShouldBe(true);
    }
}