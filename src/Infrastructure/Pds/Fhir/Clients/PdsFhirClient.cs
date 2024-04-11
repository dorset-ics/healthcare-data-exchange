using System.Net;
using Core.Common.Results;
using Core.Pds.Abstractions;
using Core.Pds.Exceptions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Infrastructure.Pds.Fhir.Clients.Abstractions;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Pds.Fhir.Clients;

public class PdsFhirClient(ILogger<PdsFhirClient> logger, IPdsFhirClientWrapper pdsFhirClientWrapper) : IPdsFhirClient
{
    public async Task<Result<Patient>> GetPatientByIdAsync(string id)
    {
        logger.LogDebug("Fetching Patient/{Id} from PDS.", id);

        try
        {
            return await pdsFhirClientWrapper.ReadAsync<Patient>($"Patient/{id}");
        }
        catch (FhirOperationException ex) when (ex.Status == HttpStatusCode.NotFound)
        {
            logger.LogDebug("Patient/{Id} not found in PDS.", id);
            return new PdsSearchPatientNotFoundException($"Patient not found for NHS Number {id}");
        }
        catch (Exception ex)
        {
            logger.LogError("Error fetching Patient/{Id} from PDS: {ErrorMessage}", id, ex.Message);
            return ex;
        }
    }

    public async Task<Result<Patient>> SearchPatientAsync(SearchParams searchParameters)
    {
        try
        {
            var searchResult = await pdsFhirClientWrapper.SearchAsync<Patient>(searchParameters);

            var isPatientFound = searchResult.Entry.Count > 0;
            logger.LogDebug("PDS Patient search returned {Count} results for the given search parameters: {SearchParameters}", searchResult.Entry.Count, searchParameters.ToUriParamList());

            return isPatientFound
                ? searchResult.Entry.FirstOrDefault()?.Resource as Patient
                : new PdsSearchPatientNotFoundException("Pds Patient search returned no results for the given search parameters: " + searchParameters.ToUriParamList());
        }
        catch (FhirOperationException ex) when (ex.Status == HttpStatusCode.BadRequest)
        {
            logger.LogError(ex, "Error while searching for patient in PDS.");
            return new PdsSearchFailedException("Error while searching for patient in PDS.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while searching for patient in PDS.");
            return new ApplicationException("Error while searching for patient in PDS.");
        }
    }
}