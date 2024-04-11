using Api.Extensions.Ingest;
using Api.Models.Ingest;
using Api.ResponseMappers;
using Carter;
using Core.Ingestion.Abstractions;
using Core.Ingestion.Models;
using FluentValidation;

namespace Api.Modules;

public class IngestModule(
    IResponseMapperFactory responseMapperFactory
) : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("$ingest", Ingest).WithName("PostIngest").WithTags("RequiredRole=DataProvider");
    }

    private async Task<IResult> Ingest([AsParameters] IngestionRequestParameters ingestionRequestParameters,
        HttpRequest request, HttpResponse res, IValidator<IngestionRequest> validator, IIngestionService ingestionService
    )
    {
        using var reader = new StreamReader(request.Body);
        var body = await reader.ReadToEndAsync();
        var ingestionRequest = ingestionRequestParameters.ToIngestionRequest(body);
        var responseMapperResult = responseMapperFactory.Create(ingestionRequest);
        if (responseMapperResult.IsFailure)
        {
            return TypedResults.BadRequest(responseMapperResult.Exception.Message);
        }

        var responseMapper = responseMapperResult.Value;

        var validationResult = await validator.ValidateAsync(ingestionRequest);
        if (!validationResult.IsValid)
        {
            return responseMapper.MapValidationErrorsToErrorResult(validationResult);
        }

        var result = await ingestionService.Ingest(ingestionRequest);

        return result.Match(
            onSuccess: responseMapper.GenerateSuccessfulResult,
            onFailure: responseMapper.MapExceptionToErrorResult);
    }
}