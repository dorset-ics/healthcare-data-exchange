using System.Text;
using System.Xml.Linq;
using Core.Common.Abstractions.Clients;
using Core.Common.Abstractions.Converters;
using Core.Common.Extensions;
using Core.Common.Models;
using Core.Common.Results;
using Core.Ndop.Abstractions;
using Core.Ndop.Models;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Task = System.Threading.Tasks.Task;

namespace Core.Ndop;

public class NdopService(
    ILogger<NdopService> logger,
    INdopMeshClient meshClient,
    IDataHubFhirClient fhirClient,
    IDistributedCache cache,
    IConverter<NdopMeshConversionRequest, Result<string>> csvToJsonConverter,
    IConverter<Bundle, Result<NdopMeshBundleToCsvConversionResult>> bundleToCsvConverter) : INdopService
{
    public async Task SendMeshMessages(CancellationToken cancellationToken)
    {
        logger.LogInformation("Sending NDOP message to MESH");

        try
        {
            // TODO - we need to look at this implementation. Once we have fully synchronised
            // the Dorset EMPI it will result in 2-4000 API calls to data hub, I suggest we look
            // at the bulk export function. It will also result in 2-4000 trace files to MESH.
            // MESH has a trace record limit of 500'000 so we need to adhere to this so multiple files
            // may be required, but it will be limited

            var searchParams = new SearchParams().LimitTo(Globals.FhirServerMaxPageSize);

            var searchResult = await fhirClient.SearchResourceByParams<Patient>(searchParams);
            if (searchResult.IsFailure)
            {
                logger.LogError("Search of data hub FHIR client failed with error: {error}", searchResult.Exception.Message);
                return;
            }

            logger.LogInformation("{count} resources returned when searching data hub FHIR client", searchResult.Value.Entry.Count);

            while (searchResult.IsSuccess)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    logger.LogInformation("Cancellation requested while processing bundle into NDOP MESH message");
                    return;
                }

                var bundleToCsvResult = bundleToCsvConverter.Convert(searchResult.Value);
                if (bundleToCsvResult.IsFailure)
                    break;

                logger.LogInformation("Patient bundle converted to CSV");

                if (bundleToCsvResult.Value.Csv.Length > 0)
                {
                    var message = await meshClient.SendMessage(bundleToCsvResult.Value.Csv);
                    if (message.IsSuccess)
                    {
                        await cache.SetAsync(message.Value.TrackingInfo.LocalId, bundleToCsvResult.Value.NhsNumbers,
                            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(48) },
                            cancellationToken);
                    }
                }

                if (searchResult.Value.Entry.Count < Globals.FhirServerMaxPageSize)
                    break;

                searchResult = await fhirClient.ContinueAsync(searchResult.Value);
                if (searchResult.IsFailure)
                {
                    logger.LogError("Search of data hub FHIR client failed with error: {error}", searchResult.Exception.Message);
                    break;
                }
            }

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending NDOP message to MESH");
        }
    }

    public async Task RetrieveMeshMessages(CancellationToken cancellationToken)
    {
        logger.LogInformation("Checking for NDOP messages in MESH");

        var messagesResult = await meshClient.RetrieveMessages();
        if (messagesResult.IsFailure)
        {
            logger.LogError("Failed to retrieve messages from MESH with error: {error}", messagesResult.Exception.Message);
            return;
        }

        logger.LogInformation("Retrieved {count} NDOP messages from MESH", messagesResult.Value.Count);

        for (var i = 0; i < messagesResult.Value.Count; i++)
        {
            var messageId = messagesResult.Value[i];
            if (cancellationToken.IsCancellationRequested)
            {
                logger.LogInformation("Cancellation requested while processing message {message} - {i}/{Count}", messageId, i + 1, messagesResult.Value.Count);
                return;
            }
            var retrieveMessageResult = await RetrieveAndProcessMeshMessage(messageId, cancellationToken);
            if (retrieveMessageResult.IsFailure)
            {
                logger.LogError("Failed to retrieve and process MESH message {message} with error: {error}", messageId, retrieveMessageResult.Exception.Message);
                continue;
            }

            logger.LogInformation("MESH message {message} has been processed", messageId);
            var acknowledgeResult = await meshClient.AcknowledgeMessage(messageId);
            if (acknowledgeResult.IsFailure || !acknowledgeResult.Value)
            {
                logger.LogError("Failed to acknowledge MESH message {message}", messageId);
                continue;
            }
            logger.LogInformation("Message {message} acknowledged in MESH", messageId);
        }
    }

    private async Task<Result> RetrieveAndProcessMeshMessage(string messageId, CancellationToken cancellationToken)
    {
        var messageResult = await meshClient.RetrieveMessage(messageId);
        if (messageResult.IsFailure)
        {
            return messageResult;
        }
        logger.LogInformation("Message {message} retrieved from MESH", messageId);
        if (messageResult.IsNull || messageResult.Value.FileContent?.Length == 0)
        {
            logger.LogDebug("Got empty message {message} from MESH", messageId);
            return Result.Success();
        }

        var filename = messageResult.Value.Headers[Globals.MeshFileNameHeader].First();
        var (fileNameWithoutExtension, extension) = (Path.GetFileNameWithoutExtension(filename), Path.GetExtension(filename));
        var (isControlFile, isDataFile) = (extension.Equals(".ctl"), filename.StartsWith(Globals.NdopMeshMessageFileNamePrefix) && extension.Equals(".dat"));
        var content = Encoding.Default.GetString(messageResult.Value.FileContent!);
        if (isControlFile)
        {
            await SaveLocalIdToFileNameReference(fileNameWithoutExtension, GetLocalIdFromControlFile(content), cancellationToken);
            return Result.Success();
        }

        if (isDataFile)
        {
            var messageRequestIds = GetNhsNumbersPreviouslySentToMesh(fileNameWithoutExtension, cancellationToken);
            if (messageRequestIds.Result == null || !messageRequestIds.Result.Any())
            {
                return new ApplicationException(
                    $"Request data for MESH message {messageId} could not be found in cache.");
            }

            logger.LogInformation("Request data for MESH message {message} was found in cache", messageId);
            var csvToJsonResult = csvToJsonConverter.Convert(new NdopMeshConversionRequest(content, messageRequestIds.Result));
            if (csvToJsonResult.IsFailure)
                return csvToJsonResult;

            logger.LogInformation("Message {message} converted to JSON", messageId);

            var conversionRequest = new ConvertDataRequest(csvToJsonResult.Value, TemplateInfo.ForNdopMeshConsent());
            var jsonToBundleResult = await fhirClient.ConvertData(conversionRequest);
            if (jsonToBundleResult.IsFailure)
                return jsonToBundleResult;

            logger.LogInformation("Message {message} converted to FHIR bundle", messageId);

            var transactionResult = await fhirClient.TransactionAsync<Consent>(jsonToBundleResult.Value);
            if (transactionResult.IsFailure)
                return transactionResult;

            return Result.Success();
        }
        logger.LogDebug("Got a trace message without actual data: {message}", Encoding.Default.GetString(messageResult.Value.FileContent!));
        return Result.Success();

    }

    private async Task<IEnumerable<string>?> GetNhsNumbersPreviouslySentToMesh(string fileNameWithoutExtension, CancellationToken cancellationToken)
    {
        var localId = await cache.GetAsync<string>(fileNameWithoutExtension, cancellationToken);
        if (localId is null)
        {
            logger.LogError($"Local ID for MESH message with name {fileNameWithoutExtension} could not be found in cache.");
            return Enumerable.Empty<string>();
        }
        return await cache.GetAsync<IEnumerable<string>>(localId, cancellationToken);
    }

    private async Task SaveLocalIdToFileNameReference(string fileNameWithoutExtension,
        string localId, CancellationToken cancellationToken)
    {
        await cache.SetAsync(fileNameWithoutExtension, localId,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(48) },
            cancellationToken);
    }

    private string GetLocalIdFromControlFile(string content)
    {
        XElement root = XElement.Parse(content);
        var value = root.Element("LocalId")?.Value ?? "";
        return value;
    }
}