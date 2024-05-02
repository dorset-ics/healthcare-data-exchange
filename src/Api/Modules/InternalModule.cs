using Api.BackgroundServices;
using Carter;
using Core.Ods.Abstractions;
using Core.Pds.Abstractions;

namespace Api.Modules;

public class InternalModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/internal/run/ods", RunOds)
            .WithName("IngestCsvDownloads").WithTags("RequiredRole=DataAdministrator");
        app.MapPost("/internal/run/pds", RunPds)
            .WithName("RetrievePdsMeshMessages").WithTags("RequiredRole=DataAdministrator");
    }

    private static async Task<IResult> RunOds(HttpContext context, IOdsService odsService,
        ILogger<InternalModule> logger)
    {
        logger.LogInformation("Starting ODS CSV download ingestion job");
        await odsService.IngestCsvDownloads(context.RequestAborted);
        return Results.Ok();
    }

    private static async Task<IResult> RunPds(HttpContext context, IPdsService pdsService,
        ILogger<InternalModule> logger)
    {
        logger.LogInformation("Starting PDS mesh message retrieval job");
        await pdsService.RetrieveMeshMessages(context.RequestAborted);
        return Results.Ok();
    }
}