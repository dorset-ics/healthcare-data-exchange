using System.Net;
using System.Text;
using System.Text.Json;
using Core.Common.Abstractions.Clients;
using Core.Common.Exceptions;
using Core.Common.Models;
using Core.Common.Results;
using FluentValidation;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using Infrastructure.Configuration;
using Infrastructure.DataHub.Clients.Abstractions;
using Infrastructure.DataHub.Models;
using Microsoft.Extensions.Logging;

namespace Infrastructure.DataHub.Clients;

public class DataHubFhirClient(
    ILogger<DataHubFhirClient> logger,
    IDataHubFhirClientWrapper dataHubFhirClient,
    IHttpClientFactory clientFactory,
    DataHubFhirServerConfiguration configuration,
    IValidator<OperationOutcome> validator)
    : IDataHubFhirClient
{
    private readonly FhirJsonParser _parser = new();

    public async Task<Result<T>> GetResource<T>(string resourceId) where T : Resource
    {
        var resourceType = ModelInfo.GetFhirTypeNameForType(typeof(T));
        logger.LogInformation("Fetching resource {ResourceType}/{ResourceId} from FHIR service.", resourceType, resourceId);

        try
        {
            var response = await dataHubFhirClient.ReadAsync<T>($"{resourceType}/{resourceId}");
            return response;
        }
        catch (FhirOperationException ex) when (ex.Status == HttpStatusCode.NotFound)
        {
            logger.LogError("Resource {ResourceType}/{ResourceId} not found in FHIR service.", resourceType, resourceId);
            return ex;
        }
        catch (Exception ex)
        {
            logger.LogError("Error fetching resource {ResourceType}/{ResourceId} from FHIR service: {ErrorMessage}", resourceType, resourceId, ex.Message);
            return ex;
        }
    }

    public async Task<Result<T>> UpdateResource<T>(T? resource) where T : Resource
    {
        if (resource == null)
        {
            logger.LogError("Error updating resource: input resource argument is null");
            return new ArgumentNullException(nameof(resource));
        }

        logger.LogInformation("Updating resource {ResourceType}/{ResourceId} to FHIR service.", resource.TypeName, resource.Id);

        try
        {
            var response = await dataHubFhirClient.UpdateAsync(resource);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError("Error updating resource {ResourceType}/{ResourceId} to FHIR service: {ErrorMessage}", resource.TypeName, resource.Id, ex.Message);
            return ex;
        }
    }

    public async Task<Result<Bundle>> TransactionAsync<T>(Bundle bundle) where T : Resource
    {
        if (bundle == null)
        {
            logger.LogError("Error creating transaction bundle: input bundle argument is null");
            return new ArgumentNullException(nameof(bundle));
        }

        bundle!.IdElement = new Id(Guid.NewGuid().ToString());

        logger.LogInformation("Transaction bundle with total of {Total}", bundle.Entry.Count);

        try
        {
            var result = await dataHubFhirClient.TransactionAsync<T>(bundle);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating transaction bundle: {ErrorMessage}", ex.Message);
            return ex;
        }

    }

    public async Task<Result<T>> CreateResource<T>(T resource) where T : Resource
    {
        resource.IdElement = null;

        logger.LogInformation("Creating resource {ResourceType} to FHIR service.", resource.TypeName);

        try
        {
            return await dataHubFhirClient.CreateResource(resource);
        }
        catch (Exception ex)
        {
            logger.LogError("Error creating resource {ResourceType} to FHIR service: {ErrorMessage}", resource.TypeName, ex.Message);
            return ex;
        }

    }

    public async Task<Result<T>> SearchResourceByIdentifier<T>(string identifier) where T : Resource, new()
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            logger.LogError("Error searching resource: input identifier argument is null or empty");
            return new ArgumentNullException(nameof(identifier));
        }

        var resourceType = ModelInfo.GetFhirTypeNameForType(typeof(T));

        logger.LogInformation("Fetching resource {ResourceType} with Identifier {Identifier} from FHIR service.", resourceType, identifier);

        try
        {
            var response = await dataHubFhirClient.SearchResourceByIdentifier<T>(identifier);

            var isResourceFound = response?.Entry is { Count: > 0 };
            if (isResourceFound)
            {
                return response!.Entry!.First().Resource as T;
            }

            var errorMessage = $"Resource {resourceType} with Identifier {identifier} not found in FHIR service.";
            logger.LogDebug(errorMessage);
            return new FhirOperationException(errorMessage, HttpStatusCode.NotFound);
        }
        catch (Exception ex)
        {
            logger.LogError("Error fetching resource {ResourceType} with Identifier {Identifier} from FHIR service: {ErrorMessage}", resourceType, identifier, ex.Message);
            return ex;
        }
    }

    public async Task<Result<Bundle>> SearchResourceByParams<T>(SearchParams searchParams) where T : Resource, new()
    {
        var resourceType = ModelInfo.GetFhirTypeNameForType(typeof(T));
        logger.LogInformation("Searching for resource {ResourceType} from FHIR service using search parameters: {SearchParams}", resourceType, searchParams.ToUriParamList());

        try
        {
            var resources = await dataHubFhirClient.SearchResourceByParams<T>(searchParams);

            var isResourceFound = resources?.Entry is { Count: > 0 };
            if (isResourceFound)
            {
                return resources;
            }

            logger.LogDebug("Resource {ResourceType} not found in FHIR service.", resourceType);
            return new FhirOperationException(
                $"Resource {resourceType} not found in FHIR service.", HttpStatusCode.NotFound);
        }
        catch (FhirOperationException ex) when (ex.Status == HttpStatusCode.BadRequest)
        {
            logger.LogError("Bad request searching for resource {ResourceType} from FHIR service using search parameters: {SearchParams}", resourceType,
                searchParams.ToUriParamList());
            return ex;
        }
        catch (Exception ex)
        {
            logger.LogError("Error fetching resource {ResourceType} from FHIR service: {ErrorMessage}", resourceType, ex.Message);
            return ex;
        }
    }

    public async Task<Result<Bundle>> ContinueAsync(Bundle current)
    {
        logger.LogInformation("Continue search on bundle of type {Type}, with total of {Total}", current.Type, current.Total);
        return await dataHubFhirClient.ContinueAsync(current);
    }

    public async Task<Result<Bundle>> ConvertData(ConvertDataRequest convertDataRequest)
    {
        var (inputData, templateInfo) = convertDataRequest;
        // TODO: replace with fluent validator for ConvertDataRequest
        if (string.IsNullOrEmpty(inputData))
        {
            logger.LogError("Error converting {DataType} data with template {Name}: input data is null or empty", templateInfo.DataType, templateInfo.Name);
            return new ArgumentNullException(nameof(convertDataRequest), "Input data is null or empty");
        }

        logger.LogInformation($"Converting {templateInfo.DataType} data with template {templateInfo.Name}");

        try
        {
            var request = CreateRequest(inputData, templateInfo.DataType, templateInfo.Name);
            var response = await SendRequest(request);
            return await MapResponseToResult(response, templateInfo);
        }
        catch (Exception ex)
        {
            logger.LogError($"Error converting {templateInfo.DataType} data with template {templateInfo.Name}: {ex.Message}");
            return ex;
        }
    }

    private static bool IsTemplateNotFoundError(string resultMessage, TemplateInfo templateInfo)
    {
        return resultMessage.Contains($"Template '{templateInfo.Name}' not found", StringComparison.InvariantCultureIgnoreCase);
    }

    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private HttpRequestMessage CreateRequest(string inputData, string inputDataType, string mappingTemplate)
    {
        var payload = new ConvertPayload(inputData, inputDataType, configuration.TemplateImage, mappingTemplate);
        var payloadJson = JsonSerializer.Serialize(payload, Options);

        var request = new HttpRequestMessage(HttpMethod.Post, "/$convert-data") { Content = new StringContent(payloadJson, Encoding.UTF8, "application/json") };

        return request;
    }

    private async Task<HttpResponseMessage> SendRequest(HttpRequestMessage request)
    {
        var httpClient = clientFactory.CreateClient("DataHubFhirClient");

        return await httpClient.SendAsync(request);
    }

    private async Task<Result<Bundle>> MapResponseToResult(HttpResponseMessage response, TemplateInfo templateInfo)
    {
        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.StatusCode != HttpStatusCode.OK)
        {
            logger.LogError("Error converting data: {response.StatusCode} - {responseContent}", response.StatusCode, responseContent);
            if (IsTemplateNotFoundError(responseContent, templateInfo))
            {
                return new UnsupportedTemplateException(responseContent);
            }

            return new FhirOperationException(responseContent, response.StatusCode);
        }

        var parsedElement = await _parser.ParseAsync(responseContent);

        if (parsedElement is Bundle bundle)
        {
            return bundle;
        }

        logger.LogError("Error converting data: {response.StatusCode} - {responseContent}", response.StatusCode, responseContent);
        return new FhirOperationException(
            $"Error converting data: {response.StatusCode} - {responseContent}", HttpStatusCode.InternalServerError);
    }

    public async Task<Result<OperationOutcome>> ValidateData<T>(T? resource) where T : Resource
    {
        if (resource is null)
        {
            logger.LogError("Error validating resource: input resource argument is null");
            return new ArgumentNullException(nameof(resource));
        }

        logger.LogInformation($"Validating resource type {resource.TypeName} for identifier {resource.Id}");
        var httpClient = clientFactory.CreateClient("DataHubFhirClient");

        var request = CreateValidateRequest(resource, resource.TypeName);
        var response = await httpClient.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var operationOutcome = new FhirJsonParser().Parse<OperationOutcome>(responseString);

        var validationResult = await validator.ValidateAsync(operationOutcome);
        if (validationResult.IsValid)
        {
            return operationOutcome;
        }

        var exception = new ValidationOperationOutcomeException(operationOutcome);
        logger.LogError(exception, "Validation failed with operationOutcome: {ValidationOperationOutcome}", responseString);
        return exception;
    }

    private HttpRequestMessage CreateValidateRequest(Resource resource, string inputDataType)
    {
        var fhirSerializer = new FhirJsonSerializer();
        var payloadJson = fhirSerializer.SerializeToString(resource);
        var request = new HttpRequestMessage(HttpMethod.Post, $"{inputDataType}/$validate") { Content = new StringContent(payloadJson, Encoding.UTF8, "application/json") };
        return request;
    }
}