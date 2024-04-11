# Api

## Ingestion Endpoint

The ingestion endpoint is a crucial part of our API, serving as the primary interaction point for the integration engines. It is responsible for receiving and processing HL7v2 messages from external sources, and is ready to be expanded to more types of messages.

### Endpoint Details

**URL:** `/$ingest`

**Method:** `POST`

**Content-Type:** `text/plain`

### Request

The ingestion endpoint expects an HL7v2 message in the body of the POST request. The `source-domain`, `organisation-code`, and `data-type` are expected to be provided in the headers of the request. Here is an example of a request:

```http
POST /$ingest HTTP/1.1
organisation-code: uhd
source-domain: agyleed
data-type: hl7v2
Content-Type: text/plain

MSH|^~\&|AGYLEED|R0D02|INTEGRATION-ENGINE|RDZ|20231127125907||ADT^A01|667151|P|2.4|||AL|NE
PID|1|0123456|0123456^^^NHS^HospitalNumber~9999999999^^^NHS^NhsNumber||SURNAME^NAME||19570830|M|||STREET^STREET2^POOLE^DORSET^BH15 3RS^^H||01234000000^PRN^PH~07000000000^PRN^CP|01234000000||||||||A
PV1|1|E||F||||||||||507291000000100|||||ED-AG-23-000000|||||||||||||||||||||||^^^R0D02||20231127125458
PV2|||Injury of shoulder / arm / elbow / wrist / hand|||||||||||||||||||||||||||||||||||1048071000000103
```

- `organisation-code`: The code representing the organisation sending the data.
- `source-domain`: The domain system from which the data is being sent.
- `data-type`: would be set by the integration engine based on what type of messages are we receiving (hl7v2/csv/etc)
- `Content-Type`: The type of the content being sent. In this case, it is `text/plain` as we are sending an HL7v2 message.

### Response

The ingestion endpoint responds with a `plain/text` result containing the status of the ingestion process. Here is an example of a response payload:

```text
MSH|^~\&|DEX|QVV|domain|org|20240131084246||ACK|3d749df9-caa5-42e2-91bb-4f9a24a04c9d|P|2.4
MSA|AA|some-string|Successfully processed
```

### Status Codes

- `200 OK`: The request was successful, and the data was ingested correctly.
- `400 Bad Request`: The request was malformed or missing required data, or the data type is not supported
- `500 Internal Server Error`: An error occurred on the server while processing the request, template not found, etc.

### Template Name Building

The template name is built using the organisation-code, source-domain and data-type from the headers and resource type from the request payload.

The format is `{organisation-code}_{source-domain}_{data-type}_{resourceType}`.

For example, if the source-domain is `agyleed`, organisation-code is `uhd`, data-type is `hl7v2` and resource type is `adt_a01`, the template name will be `agyleed_uhd_hl7v2_adta01`.

## Global Exception Handler

### Overview

The Global Exception Handler is a centralized mechanism for handling exceptions in our application. It provides a consistent response structure for all unhandled exceptions, making it easier for clients to handle errors.

### Implementation

The Global Exception Handler is implemented as a class that implements the `IExceptionHandler` interface. It uses the `ILogger` service to log exceptions and the `IHostEnvironment` service to determine the current environment.

The implementation of the `GlobalExceptionHandler` class is under the file `src/Api/Exceptions/GlobalExceptionHandler.cs`.

In that file we have a mapping function that maps an exception to a status code and title, and a `HandleAsync` method that handles the exception and returns a `ProblemDetails` object.

currently the `MapException` function only maps the `Exception` class to a 500 status code and "Internal Server Error" title.
but we can add more mapping for other exceptions like TimeoutException, ArgumentException, etc.

```csharp

    private static (int statusCode, string title) MapException(Exception exception)
        => exception switch
        {
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };
}
```

### Enabling the Global Exception Handler

To enable the Global Exception Handler, you need to register it in the `Startup` class:

```csharp
public static IServiceCollection AddApi(this IServiceCollection services)
    => services.AddRequestTimeouts()
        .AddGlobalExceptionHandling() // here
        .AddEndpointsAndSwagger()
        .AddFhirJsonSerializer()
        .AddPatientSearchStrategies()
        .AddFluentValidators();

private static IServiceCollection AddGlobalExceptionHandling(this IServiceCollection services) =>
    services.AddExceptionHandler<GlobalExceptionHandler>()
        .AddProblemDetails();
```

In the above code, the `AddGlobalExceptionHandling` extension method is used to register the `GlobalExceptionHandler` with the dependency injection container. This method is part of the `DependencyInjection` class in the `src/Api/DependencyInjection.cs` file.

## Error Response

The Global Exception Handler returns a JSON response with a `ProblemDetails` object. This object includes the status code, title, trace ID, and instance of the error.
In a development and staging environment, it also includes the detail of the error.

Here's an example of what the error response might look like in a development/staging environment:

```json
{
    "status": 500,
    "title": "Internal Server Error",
    "traceId": "0HLN8KNV7BB6S:00000001",
    "instance": "GET /Patient?nhs-number=1234567890",
    "detail": "An unhandled exception occurred."
}
```

In a production environment, the `detail` field is omitted:

```json
{
    "status": 500,
    "title": "Internal Server Error",
    "traceId": "0HLN8KNV7BB6S:00000001",
    "instance": "GET /api/values"
}
```
