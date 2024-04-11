using Core.Common.Abstractions.Clients;
using Core.Common.Abstractions.Converters;
using Core.Common.Models;
using Core.Common.Results;
using Core.Common.Utilities;
using Core.Ods.Abstractions;
using Core.Ods.Enums;
using Core.Ods.Models;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;
using Task = System.Threading.Tasks.Task;

namespace Core.Ods.Strategies;

public class OdsCsvIngestionStrategy(
    ILogger<OdsCsvIngestionStrategy> logger,
    IDataHubFhirClient fhirClient,
    IConverter<OdsCsvIngestionData, Result<string>> oConverter) : IOdsCsvIngestionStrategy
{
    public Task Ingest(OdsCsvDownloadSource odsCsvSource, Stream downloadStream)
    {
        var batchFileReader = new BatchFileReader(downloadStream, 500);

        return ReadAndHandleFileBatches(odsCsvSource, batchFileReader);
    }

    private async Task ReadAndHandleFileBatches(OdsCsvDownloadSource odsCsvSource, BatchFileReader reader)
    {
        var batches = reader.ReadBatches();
        foreach (var batch in batches)
        {
            var orgData =
                OdsCsvIngestionData.GetDataBySource(string.Join(Environment.NewLine, batch), odsCsvSource);

            var jsonData = oConverter.Convert(orgData);

            if (jsonData.IsFailure)
            {
                logger.LogError(jsonData.Exception, $"Error converting CSV to json for {odsCsvSource}");
                continue;
            }

            await ConvertAndSaveOrganisationData(jsonData, orgData.OrgTemplateInfo);
        }
    }

    private async Task ConvertAndSaveOrganisationData(Result<string> json,
        TemplateInfo orgDataOrgTemplateInfo)
    {
        var conversionRequest = new ConvertDataRequest(json.Value, orgDataOrgTemplateInfo);
        var jsonToBundleResult = await fhirClient.ConvertData(conversionRequest);
        if (jsonToBundleResult.IsFailure)
        {
            logger.LogError(jsonToBundleResult.Exception, $"Error converting ODS data to FHIR from {json.Value}");
            return;
        }

        var transactionResult = await fhirClient.TransactionAsync<Bundle>(jsonToBundleResult.Value);
        if (transactionResult.IsFailure)
        {
            logger.LogError(transactionResult.Exception, $"Error persisting ODS data to FHIR for {jsonToBundleResult.Value}");
        }
    }
}
