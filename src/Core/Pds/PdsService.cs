using System.Net;
using System.Text;
using System.Text.Json;
using Core.Common.Abstractions.Clients;
using Core.Common.Abstractions.Converters;
using Core.Common.Models;
using Core.Common.Results;
using Core.Pds.Abstractions;
using Core.Pds.Extensions;
using Core.Pds.Models;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.Extensions.Logging;
using Task = System.Threading.Tasks.Task;

namespace Core.Pds;

public class PdsService(
    ILogger<PdsService> logger,
    IPdsMeshClient pdsMeshClient,
    IPdsFhirClient pdsFhirClient,
    IDataHubFhirClient fhirClient,
    IConverter<string, Result<string>> csvToJsonConverter,
    IConverter<Bundle, Result<PdsMeshBundleToCsvConversionResult>> bundleToCsvConverter) : IPdsService
{
    public async Task SendMeshMessages(CancellationToken cancellationToken)
    {
        logger.LogInformation("Sending PDS message to MESH");

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
                    logger.LogInformation("Cancellation requested while processing bundle into PDS MESH message");
                    return;
                }

                var sendMessageResult = await SendMeshMessage(searchResult.Value);
                if (sendMessageResult.IsFailure)
                    logger.LogError("Failed to send message to MESH with error: {error}", sendMessageResult.Exception.Message);

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
            logger.LogError(ex, "Error sending PDS message to MESH");
        }
    }

    public async Task RetrieveMeshMessages(CancellationToken cancellationToken)
    {
        logger.LogInformation("Checking for PDS messages in MESH");

        var messagesResult = await pdsMeshClient.RetrieveMessages();
        if (messagesResult.IsFailure)
        {
            logger.LogError("Failed to retrieve messages from MESH with error: {error}", messagesResult.Exception.Message);
            return;
        }

        logger.LogInformation("Retrieved {count} PDS messages from MESH", messagesResult.Value.Count);

        for (var i = 0; i < messagesResult.Value.Count; i++)
        {
            var messageId = messagesResult.Value[i];
            if (cancellationToken.IsCancellationRequested)
            {
                logger.LogInformation("Cancellation requested while processing message {message} - {i}/{Count}", messageId, i + 1, messagesResult.Value.Count);
                return;
            }

            var retrieveMessageResult = await RetrieveAndProcessMeshMessage(messageId);
            if (retrieveMessageResult.IsFailure)
            {
                logger.LogError("Failed to retrieve and process MESH message {message} with error: {error}", messageId, retrieveMessageResult.Exception.Message);
                continue;
            }
            var acknowledgeResult = await pdsMeshClient.AcknowledgeMessage(messageId);
            if (acknowledgeResult.IsFailure || !acknowledgeResult.Value)
            {
                logger.LogError("Failed to acknowledge MESH message {message}", messageId);
                continue;
            }

            logger.LogInformation("Message {message} acknowledged in MESH", messageId);
        }
    }

    public async Task<Result<Patient>> Search(PdsSearchParameters searchParameters)
    {
        var fhirSearchParameters = searchParameters.ToFhirSearchParameters();

        var searchPdsPatientResult = await pdsFhirClient.SearchPatientAsync(fhirSearchParameters);
        if (searchPdsPatientResult.IsFailure) return searchPdsPatientResult;

        return await PersistPdsPatient(searchPdsPatientResult.Value);
    }

    public async Task<Result<Patient>> GetPatientById(string nhsNumber)
    {
        var getResult = await fhirClient.GetResource<Patient>(nhsNumber);
        if (getResult.IsSuccess) return getResult;
        if (getResult.Exception is not FhirOperationException { Status: HttpStatusCode.NotFound })
            return new ApplicationException($"Error fetching data from fhir server", getResult.Exception);

        logger.LogDebug("Patient with NHS number {NhsNumber} was not found in DataHub FHIR. Searching in PDS...", nhsNumber);
        var getPatientResult = await pdsFhirClient.GetPatientByIdAsync(nhsNumber);
        if (getPatientResult.IsFailure) return getPatientResult;

        logger.LogDebug("Patient with NHS number {NhsNumber} was found in PDS. Creating in DataHub FHIR...", nhsNumber);
        return await PersistPdsPatient(getPatientResult.Value);
    }

    private async Task<Result<Patient>> PersistPdsPatient(Patient patientFromPds)
    {
        AddOrgSource(patientFromPds);
        var updatePatientResult = await fhirClient.UpdateResource(patientFromPds);
        if (updatePatientResult.IsFailure)
        {
            logger.LogError(updatePatientResult.Exception, "Error updating patient with NHS number {NhsNumber} in DataHub FHIR.", patientFromPds.Id);
            return new ApplicationException($"Error persisting patient with NHS number {patientFromPds.Id} in DataHub FHIR.", updatePatientResult.Exception);
        }

        logger.LogDebug("Patient with NHS number {NhsNumber} was persisted in DataHub FHIR", patientFromPds.Id);
        return updatePatientResult;
    }

    private static void AddOrgSource(Patient patient)
    {
        patient.Meta ??= new Meta();
        patient.Meta.Source = $"Organization/{Globals.X26OrganizationResourceId}";
    }

    private async Task<Result> SendMeshMessage(Bundle bundle)
    {
        var bundleToCsvResult = bundleToCsvConverter.Convert(bundle);
        if (bundleToCsvResult.IsFailure)
            return bundleToCsvResult;

        logger.LogInformation("Patient bundle converted to CSV");

        var sendMessageResult = await pdsMeshClient.SendMessage(bundleToCsvResult.Value.Csv);
        if (sendMessageResult.IsFailure)
            return sendMessageResult;

        logger.LogInformation("Message {message} sent to MESH", sendMessageResult.Value.MessageId);

        return Result.Success();
    }

    private async Task<Result> RetrieveAndProcessMeshMessage(string messageId)
    {
        var messageResult = await pdsMeshClient.RetrieveMessage(messageId);
        if (messageResult.IsFailure)
            return messageResult;

        logger.LogInformation("Message {message} retrieved from MESH", messageId);

        var csv = Encoding.UTF8.GetString(messageResult.Value.FileContent);
        var csvToJsonResult = csvToJsonConverter.Convert(csv);
        if (csvToJsonResult.IsFailure)
            return csvToJsonResult;

        logger.LogInformation("Message {message} converted to JSON", messageId);

        List<string> patientsToBeDeleted = new List<string>();
        var modifiedCsvToJsonResult = HandleInvalidPDSPatients(csvToJsonResult.Value, patientsToBeDeleted);

        await CallFHIRConvertAndUpdateResource(messageId, modifiedCsvToJsonResult, patientsToBeDeleted);

        return Result.Success();
    }

    private async Task<Result> CallFHIRConvertAndUpdateResource(string messageId, string modifiedCsvToJsonResult, List<string> patientsToBeDeleted)
    {
        var conversionRequest = new ConvertDataRequest(modifiedCsvToJsonResult, TemplateInfo.ForPdsMeshPatient());
        var jsonToBundleResult = await fhirClient.ConvertData(conversionRequest);
        if (jsonToBundleResult.IsFailure)
        {
            logger.LogError("Error while converting Message {message} converted to FHIR bundle", messageId);
            return jsonToBundleResult;
        }

        if (patientsToBeDeleted != null && patientsToBeDeleted.Count > 0)
            EnrichFhirBundleWithPatientsToBeDeleted(patientsToBeDeleted, jsonToBundleResult);

        logger.LogInformation("Message {message} converted to FHIR bundle", messageId);

        var transactionResult = await fhirClient.TransactionAsync<Patient>(jsonToBundleResult.Value);
        if (transactionResult.IsFailure)
        {
            logger.LogError("Error while calling FHIR for TransactionAsync with Message {message}", messageId);
            return transactionResult;
        }

        logger.LogInformation("Message {message} processed successfully", messageId);
        return Result.Success();
    }

    private void EnrichFhirBundleWithPatientsToBeDeleted(List<string> patientsToBeDeleted, Result<Bundle> jsonToBundleResult)
    {
        foreach (var patient in patientsToBeDeleted)
        {
            jsonToBundleResult.Value.Entry.Add(new()
            {
                Request = new Bundle.RequestComponent
                {
                    Method = Bundle.HTTPVerb.DELETE,
                    Url = $"Patient/{patient}"
                }
            });
        }
    }

    private string HandleInvalidPDSPatients(string csvToJsonResult, List<string> patientsToBeDeleted)
    {
        var response = JsonSerializer.Deserialize<Dictionary<string, List<PdsMeshRecordResponse>>>(csvToJsonResult);
        var records = response?["patients"]!;

        for (int i = records.Count - 1; i >= 0; i--)
        {
            var record = records[i];
            if (record.ErrorSuccessCode != "91") continue;

            patientsToBeDeleted.Add(record.NhsNumber!);

            if (record.MatchedNhsNo != "0000000000" && record.NhsNumber != null)
            {
                record.NhsNumber = record.MatchedNhsNo;
            }
            else
            {
                records.RemoveAt(i);
            }
        }

        return JsonSerializer.Serialize(response);
    }

}