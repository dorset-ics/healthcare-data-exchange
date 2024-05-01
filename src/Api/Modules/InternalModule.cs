using Api.BackgroundServices;
using Carter;

namespace Api.Modules;

public class InternalModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/internal/run/ods", app.ServiceProvider.GetRequiredService<OdsCsvDownloadBackgroundService>().Execute).WithName("IngestCsvDownloads").WithTags("RequiredRole=DataAdministrator");
        app.MapPost("/internal/run/pds", app.ServiceProvider.GetRequiredService<PdsMeshRetrieveBackgroundService>().Execute).WithName("RetrievePdsMeshMessages").WithTags("RequiredRole=DataAdministrator");
    }
}