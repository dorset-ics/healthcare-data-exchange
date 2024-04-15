using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Api.Exceptions;
using Core.Pds.Exceptions;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static Hl7.Fhir.Model.OperationOutcome;
using Task = System.Threading.Tasks.Task;

namespace Unit.Tests.Api.Exceptions;

public class GlobalExceptionHandlerTests
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    [Theory]
    [InlineData("Development", typeof(PdsSearchFailedException), HttpStatusCode.BadRequest, IssueType.Invalid)]
    [InlineData("Production", typeof(PdsSearchPatientNotFoundException), HttpStatusCode.NotFound, IssueType.NotFound)]
    public async Task TryHandleAsync_ShouldWriteOperationOutcomeToResponse_WhenFhirOperationExceptionOccurs(string environmentName, Type exceptionType, HttpStatusCode expectedStatusCode, IssueType expectedIssueType)
    {
        var exception = (Exception)Activator.CreateInstance(exceptionType, "Test Exception")!;
        var cancellationToken = new CancellationToken();
        var (handler, httpContext) = CreateHandler(environmentName);

        var isLastStopInPipeline = await handler.TryHandleAsync(httpContext, exception, cancellationToken);
        isLastStopInPipeline.ShouldBeTrue();
        httpContext.Response.StatusCode.ShouldBe((int)expectedStatusCode);

        var expectedOperationOutcome = new OperationOutcome
        {
            Issue =
                [
                    new() 
                    { 
                        Severity = IssueSeverity.Error,
                        Code = expectedIssueType,
                        Diagnostics = environmentName == Environments.Development ? exception.Message : null
                    }
                ]
        };

        var expectedBody = JsonSerializer.Serialize(expectedOperationOutcome, _jsonSerializerOptions);

        var body = await ReadResponseBody(httpContext, cancellationToken);
        body.ShouldBe(expectedBody);
    }

    [Theory]
    [InlineData("Development", typeof(TaskCanceledException), HttpStatusCode.GatewayTimeout, "Request Timeout")]
    [InlineData("Production", typeof(TaskCanceledException), HttpStatusCode.GatewayTimeout, "Request Timeout")]
    [InlineData("Development", typeof(Exception), HttpStatusCode.InternalServerError, "Internal Server Error")]
    [InlineData("Production", typeof(Exception), HttpStatusCode.InternalServerError, "Internal Server Error")]
    public async Task TryHandleAsync_ShouldWriteProblemDetailsToResponse_WhenExceptionOccurs(string environmentName, Type exceptionType, HttpStatusCode expectedStatusCode, string expectedTitle)
    {
        var exception = (Exception)Activator.CreateInstance(exceptionType, "Test Exception")!;
        var cancellationToken = new CancellationToken();
        var (handler, httpContext) = CreateHandler(environmentName);

        var isLastStopInPipeline = await handler.TryHandleAsync(httpContext, exception, cancellationToken);
        isLastStopInPipeline.ShouldBeTrue();
        httpContext.Response.StatusCode.ShouldBe((int)expectedStatusCode);

        var expectedProblemDetails = new ProblemDetails
        {
            Status = (int)expectedStatusCode,
            Title = expectedTitle,
            Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",
            Extensions = { ["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier },
            Detail = environmentName == Environments.Development ? exception.Message : null
        };
        var expectedBody = JsonSerializer.Serialize(expectedProblemDetails, _jsonSerializerOptions);

        var body = await ReadResponseBody(httpContext, cancellationToken);
        body.ShouldBe(expectedBody);
    }

    private (GlobalExceptionHandler, HttpContext) CreateHandler(string environmentName)
    {
        var logger = Substitute.For<ILogger<GlobalExceptionHandler>>();
        var environment = Substitute.For<IWebHostEnvironment>();
        environment.EnvironmentName.Returns(environmentName);

        var httpContext = Substitute.For<HttpContext>();
        httpContext.RequestServices = Substitute.For<IServiceProvider>();
        httpContext.Response.Body.Returns(new MemoryStream());
        httpContext.Request.Method.Returns("GET");
        httpContext.Request.Path.Returns(new PathString("/test"));

        var handler = new GlobalExceptionHandler(logger, environment);
        return (handler, httpContext);
    }

    private async Task<string> ReadResponseBody(HttpContext httpContext, CancellationToken cancellationToken)
    {
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(httpContext.Response.Body);
        return await reader.ReadToEndAsync(cancellationToken);
    }
}