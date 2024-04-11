using Core.Common.Abstractions.Clients;
using Core.Common.Results;
using Core.Ingestion.Abstractions;
using Core.Ingestion.Extensions;
using Core.Ingestion.Models;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;

namespace Core.Common.Strategies;

public class HL7v2IngestionStrategy(IDataHubFhirClient dataHubClient, IFhirResourceEnhancer fhirResourceEnhancer, ILogger<HL7v2IngestionStrategy> logger)
    : IIngestionStrategy
{
    public async Task<Result> Ingest(IngestionRequest ingestionRequest)
    {
        logger.LogInformation("Ingesting HL7v2 message with organisation code {OrganisationCode} and source domain {SourceDomain}", ingestionRequest.OrganisationCode, ingestionRequest.SourceDomain);
        var conversionResult = await dataHubClient.ConvertData(ingestionRequest.ToConvertDataRequest());
        if (conversionResult.IsFailure) return conversionResult;

        var amenedSnomedDisplayResult = fhirResourceEnhancer.Enrichment(conversionResult.Value);
        if (amenedSnomedDisplayResult.IsFailure) { return amenedSnomedDisplayResult; };

        var validationResult = await dataHubClient.ValidateData(amenedSnomedDisplayResult.Value);
        if (validationResult.IsFailure) return validationResult;

        var transactionResult = await dataHubClient.TransactionAsync<Bundle>(conversionResult.Value);
        logger.LogInformation("Ingestion completed with status {IngestionTransactionStatus}", transactionResult.IsSuccess ? "Success" : "Failure");
        return transactionResult;
    }
}