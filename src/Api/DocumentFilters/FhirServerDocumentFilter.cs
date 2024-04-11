using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api.DocumentFilters;

public class FhirServerDocumentFilter : IDocumentFilter
{

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var byIdParameter = new OpenApiParameter
        {
            Name = "id",
            Description = "The resource ID",
            In = ParameterLocation.Path,
            Required = true,
            Schema = new OpenApiSchema { Type = "string" }
        };
        var resourceParameter = new OpenApiParameter
        {
            Name = "resource",
            Description = "The name of the FHIR resource",
            In = ParameterLocation.Path,
            Required = true,
            Schema = new OpenApiSchema { Type = "string" }
        };

        AddHealthPath(swaggerDoc);
        AddPath(swaggerDoc, OperationType.Get, "/{resource}", "GetResources", "Endpoint to retrieve FHIR resources", [resourceParameter]);
        AddPath(swaggerDoc, OperationType.Get, "/{resource}/{id}", "GetResource", "Endpoint to retrieve a FHIR resource by ID", [
            resourceParameter,
            byIdParameter
        ]);

        AddPath(swaggerDoc, OperationType.Put, "/{resource}/{id}", "PutResource", "Endpoint to create or update a FHIR resource", [resourceParameter,
            byIdParameter]);
        AddPath(swaggerDoc, OperationType.Post, "/{resource}", "PostResource", "Endpoint to create a FHIR resource", [resourceParameter]);
    }

    private void AddPath(OpenApiDocument openApiDocument, OperationType operationType, string path, string operationId, string description, List<OpenApiParameter> parameters)
    {
        if (!openApiDocument.Paths.TryGetValue(path, out var pathItem))
        {
            pathItem = new OpenApiPathItem();
            openApiDocument.Paths.Add(path, pathItem);
        }

        pathItem.AddOperation(operationType, new OpenApiOperation
        {
            Description = description,
            OperationId = operationId,
            Responses = new OpenApiResponses
            {
                ["200"] = new OpenApiResponse { Description = "Success" }
            },
            Parameters = parameters,
            Tags = [new() { Name = "FhirBackend" }]
        });
    }

    private void AddHealthPath(OpenApiDocument openApiDocument)
    {
        var pathItem = new OpenApiPathItem();

        pathItem.AddOperation(OperationType.Get, new OpenApiOperation
        {
            Description = "Endpoint to check the health of the Data Hub",
            OperationId = "GetHealth",
            Responses = new OpenApiResponses
            {
                ["200"] = new OpenApiResponse { Description = "Success" }
            },
            Tags = [new() { Name = "HealthCheck" }]
        });

        openApiDocument.Paths.Add("/_health", pathItem);
    }
}
