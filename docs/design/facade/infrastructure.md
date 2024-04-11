# Infrastructure

The `Infrastructure` project is a .NET library that provides an easy way to interact with the Personal Demographics
Service (PDS) and the DataHub FHIR server. It includes service configurations and clients for making requests to the PDS
and the FHIR server, as well as a client for sending messages to the NHS Mailbox (Mesh).

## Structure

The project is divided into several folders:

1. Server libraries -e.g. PDS and DataHub, each with its own configuration, health check, client and tests. The goal behind them is having the option in the future to split them up into their own independent projects.
1. Common - used across the different server libraries.

## Prerequisites

- .NET 5.0 or later
- A PDS account with the necessary permissions
- A Mesh mailbox set up
- An Azure Fhir server set up or running the [FHIR OSS](https://github.com/microsoft/fhir-server) server locally

## Configuration

The library uses the `PDS`,`Mesh` and `DataHubFhirServer` sections in the `appsettings.json` file for configuration. Here is an
example:

```json
{
    "PDS": {
        "BaseUrl": "<PROD_PDS_SERVER_URL_HERE>",
        "Authentication": {
            "IsEnabled": false,
            "TokenUrl": "<TOKEN_URL_HERE>",
            "ClientId": "<CLIENT_ID_HERE>",
            "Kid": "<KID_HERE>"
        },
        "Mesh": {
            "SendSchedule": "0 0 * * * ?",
            "RetrieveSchedule": "0 0 0 * * ?",
            "MailboxId": "<MAILBOX_ID>",
            "Key": "<KEY>",
            "WorkflowId": "SPINE_PDS_MESH_V1"
        }
    },
    "Mesh": {
        "Url": "http://host.docker.internal:8700",
        "MaxChunkSizeInMegabytes": 100
    },
    "DataHubFhirServer": {
        "BaseUrl": "<PROD_FHIR_SERVER_URL_HERE>"
    }
}
```

## Mesh Client Configuration

The `MeshClient` uses the `Mesh` section in the `appsettings.json` as a base for configuration. and every use case
implement it's own concrete mesh client, so for example the `PdsMeshClient` uses the `PDS.Mesh` section in the
`appsettings.json` file for configuration. Here is an example:

```json
{
    "Pds": {
        "Mesh": {
            "SendSchedule": "0 0 * * * ?",
            "RetrieveSchedule": "0 0 0 * * ?",
            "MailboxId": "<MAILBOX_ID>",
            "Key": "TestKey",
            "WorkflowId": "SPINE_PDS_MESH_V1"
        }
    }
}
```

## Usage

To use this library in your project, you need to add it to your service collection in the `Program.cs` file:

```csharp
builder.Services.AddInfrastructure(Configuration);
```

This will register the `IPdsServiceClient`, `IMeshClient` and `IDataHubFhirClient` services, which you can then inject
into your classes:

```csharp
public class MyClass
{
    private readonly IPdsClient _pdsClient;
    private readonly IMeshClient _meshClient;
    private readonly IDataHubFhirClient _DataHubFhirClient;
    public MyClass(IPdsClient pdsClient, IMeshClient meshClient, IDataHubFhirClient DataHubFhirClient)
    {
        _pdsClient = pdsClient;
        _meshClient = meshClient;
        _DataHubFhirClient = DataHubFhirClient;
    }
}

// or using default constructors
public class MyClass(IPdsClient pdsClient, IMeshClient meshClient, IDataHubFhirClient DataHubFhirClient)
```

note that `AddPdsInfrastructure` and `AddDataHubFhirInfrastructure` are private methods used internally
by `AddInfrastructure` to set up the necessary services.

```csharp
var sendMessageAsync = await meshClient.Mailbox.SendMessageAsync(
            mexTo: mailBoxId,
            mexWorkflowId: Guid.NewGuid().ToString(),
            content: "Hello World!",
            mexSubject: "Subject",
            mexLocalId: "1",
            mexFileName: "test.txt",
            contentType: MediaTypeNames.Text.Plain);
```

## Updates From PDS

Updates from PDS are received via a Mesh Mailbox update, with a process titled PDS Trace.
The process starts by requesting such trace data for every patient in the FHIR store, effectively exporting the entire FHIR patients store to a CSV file to be sent to the Mesh Mailbox.
The integration with PDS is described [here](https://digital.nhs.uk/services/personal-demographics-service/using-the-pds-mesh-service-with-mesh-ui#sending-the-trace-request-to-pds).
The API will schedule a PDS Trace process to run at configurable intervals, sending a full Patient record dataset to the Mesh Mailbox.
The configuration for this process looks like this:

```json
{
    "PDS": {
        "Mesh": {
            "SendSchedule": "0 0 0 * * ?",
            "MailboxId": "<MAILBOX_ID>"
        }
    }
}
```

The process is scheduled using the Quartz.NET library that triggers the main method of **ScheduledDataHubToPdsMesh** class instance, and the schedule is defined using a cron expression.

## Health Checks

This application includes health checks for any external service to the Api, such as: DataHub FHIR service, PDS FHIR
service, Mesh mailbox.
Health checks are used to probe the state of the application and its dependencies to determine its health.

### Overview

The health checks in this application are implemented using the `IHealthCheck` interface provided by
the `Microsoft.Extensions.Diagnostics.HealthChecks` package. This interface requires implementing a `CheckHealthAsync`
method that returns a `HealthCheckResult` as seen in
the [official docs](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-8.0)

The `DataHubFhirHealthCheck` and `PdsFhirHealthCheck` and `MeshMailboxHealthCheck`  classes are specific health checks
for the DataHub FHIR service
and the PDS FHIR service respectively. They inherit from the `BaseFhirHealthCheck` abstract class, which provides a
common implementation for calling a service health check endpoint, then gets validated by the `HealthCheckResult` class.

### How Health Checks Work

In the `CheckHealthAsync` method, an HTTP GET request is sent to the health check endpoint of the respective FHIR
service. This is done using an `HttpClient` that is retrieved from an `IHttpClientFactory`. The `HttpClient` is
configured with the name of the client (`ClientName`) and the health check endpoint (`HealthCheckEndpoint`), which are
both provided by the derived classes.

If the FHIR service responds with a successful HTTP status code (i.e., in the range 200-299), the health check is
considered to have passed and `HealthCheckResult.Healthy()` is returned. If the service responds with a non-successful
status code, or if an exception occurs during the request, the health check is considered to have failed
and `HealthCheckResult.Unhealthy()` is returned.

### Using the Health Checks

The health checks are registered in the dependency injection container with the `AddCheck` method over
the `services.AddHealthChecks()` extension. The health checks are then run by the health check
middleware when a GET request is sent to the `/_health` endpoint of the application.

The response from the `/_health` endpoint is a JSON written by `AspNetCore.HealthChecks.UI.Client.UIResponseWriter`
class,
that includes the overall health status of the application, the total duration of all health checks, and an array of
entries for each health check. Each entry includes the name of
the health check, the duration of the health check, the health status, and any tags associated with the health check.

Here is an example of what the response might look like:

```json
{
    "status": "Healthy",
    "totalDuration": "00:00:00.1612859",
    "entries": {
        "DataHub FHIR Health Check": {
            "data": {},
            "duration": "00:00:00.0147493",
            "status": "Healthy",
            "tags": [
                "FHIR",
                "DataHub",
                "Api"
            ]
        },
        "PDS FHIR Health Check": {
            "data": {},
            "duration": "00:00:00.1610403",
            "status": "Healthy",
            "tags": [
                "FHIR",
                "PDS",
                "Api"
            ]
        },
        "Mesh Mailbox Health Check": {
            "data": {},
            "duration": "00:00:00.0158644",
            "status": "Healthy",
            "tags": [
                "Mesh",
                "Background"
            ]
        }
    }
}
```
