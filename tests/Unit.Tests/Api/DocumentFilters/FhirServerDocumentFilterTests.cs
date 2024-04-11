using Api.DocumentFilters;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Unit.Tests.Api.DocumentFilters;

public class FhirServerDocumentFilterTests
{
    private readonly FhirServerDocumentFilter _sut = new();

    [Fact]
    public void Apply_GivenOpenApiDocument_AddsGetResourcePathTest()
    {
        var openApiDocument = new OpenApiDocument { Paths = [] };

        _sut.Apply(openApiDocument, new DocumentFilterContext(null, null, null));

        var path = openApiDocument.Paths["/{resource}"];
        path.ShouldNotBeNull();

        var getOperation = path.Operations[OperationType.Get];
        getOperation.ShouldNotBeNull();
        getOperation.Tags.ShouldContain(tag => tag.Name == "FhirBackend");
        getOperation.Parameters.ShouldContain(parameter => parameter.Name == "resource");
    }

    [Fact]
    public void Apply_GivenOpenApiDocument_AddsGetResourcesPathTest()
    {
        var openApiDocument = new OpenApiDocument { Paths = [] };

        _sut.Apply(openApiDocument, new DocumentFilterContext(null, null, null));

        var path = openApiDocument.Paths["/{resource}/{id}"];
        path.ShouldNotBeNull();

        var getOperation = path.Operations[OperationType.Get];
        getOperation.ShouldNotBeNull();
        getOperation.Tags.ShouldContain(tag => tag.Name == "FhirBackend");
        getOperation.Parameters.ShouldContain(parameter => parameter.Name == "resource");
        getOperation.Parameters.ShouldContain(parameter => parameter.Name == "id");
    }

    [Fact]
    public void Apply_GivenOpenApiDocument_AddHealthPathTest()
    {
        var openApiDocument = new OpenApiDocument { Paths = [] };

        _sut.Apply(openApiDocument, new DocumentFilterContext(null, null, null));

        var path = openApiDocument.Paths["/_health"];
        path.ShouldNotBeNull();

        var getOperation = path.Operations[OperationType.Get];
        getOperation.ShouldNotBeNull();
    }

    [Fact]
    public void Apply_GivenOpenApiDocument_AddPutResourceTest()
    {
        var openApiDocument = new OpenApiDocument { Paths = [] };

        _sut.Apply(openApiDocument, new DocumentFilterContext(null, null, null));

        var path = openApiDocument.Paths["/{resource}/{id}"];
        path.ShouldNotBeNull();

        var getOperation = path.Operations[OperationType.Put];
        getOperation.ShouldNotBeNull();
        getOperation.Tags.ShouldContain(tag => tag.Name == "FhirBackend");
        getOperation.Parameters.ShouldContain(parameter => parameter.Name == "resource");
        getOperation.Parameters.ShouldContain(parameter => parameter.Name == "id");
    }

    [Fact]
    public void Apply_GivenOpenApiDocument_AddPostResourceTest()
    {
        var openApiDocument = new OpenApiDocument { Paths = [] };

        _sut.Apply(openApiDocument, new DocumentFilterContext(null, null, null));

        var path = openApiDocument.Paths["/{resource}"];
        path.ShouldNotBeNull();

        var getOperation = path.Operations[OperationType.Post];
        getOperation.ShouldNotBeNull();
        getOperation.Tags.ShouldContain(tag => tag.Name == "FhirBackend");
        getOperation.Parameters.ShouldContain(parameter => parameter.Name == "resource");
    }

}
