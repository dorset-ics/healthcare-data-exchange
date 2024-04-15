using System.Diagnostics;
using System.Net;
using Hl7.Fhir.Rest;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using static Hl7.Fhir.Model.OperationOutcome;
using Task = System.Threading.Tasks.Task;

namespace Api.Exceptions;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment) : IExceptionHandler
{
    private const bool IsLastStopInPipeline = true;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        logger.LogError(exception, "Could not process a request on machine {MachineName} with trace id {TraceId}", Environment.MachineName, traceId);

        if (exception is FhirOperationException fhirOperationException)
        {
            await WriteOperationOutcomeAsync(fhirOperationException, httpContext, cancellationToken);
        }
        else
        {
            await WriteProblemDetails(exception, traceId, httpContext, cancellationToken);
        }

        return IsLastStopInPipeline;
    }

    private async Task WriteOperationOutcomeAsync(FhirOperationException fhirOperationException, HttpContext httpContext, CancellationToken cancellationToken)
    {
        var issueType = MapFhirOperationException(fhirOperationException);
        var operationOutcome = ForException(fhirOperationException, issueType);

        if (!environment.IsDevelopment())
        {
            operationOutcome.Issue.ForEach(x => x.Diagnostics = null);
        }

        httpContext.Response.StatusCode = (int)fhirOperationException.Status;
        await httpContext.Response.WriteAsJsonAsync(operationOutcome, cancellationToken);
    }

    private IssueType MapFhirOperationException(FhirOperationException fhirOperationException) => fhirOperationException.Status switch
    {
        HttpStatusCode.NotFound => IssueType.NotFound,
        HttpStatusCode.BadRequest => IssueType.Invalid,
        _ => IssueType.Exception
    };

    private async Task WriteProblemDetails(Exception exception, string traceId, HttpContext httpContext, CancellationToken cancellationToken)
    {
        var (statusCode, title) = MapException(exception);

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Extensions = { ["traceId"] = traceId },
            Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",
            Detail = environment.IsDevelopment() ? exception.Message : null
        };

        httpContext.Response.StatusCode = (int)statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
    }

    private (HttpStatusCode httpStatusCode, string title) MapException(Exception exception) => exception switch
    {
        TaskCanceledException _ => (HttpStatusCode.GatewayTimeout, "Request Timeout"),
        _ => (HttpStatusCode.InternalServerError, "Internal Server Error")
    };
}