using Core.Common.Results;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace Core.Pds.Abstractions;

public interface IPdsFhirClient
{
    Task<Result<Patient>> GetPatientByIdAsync(string id);
    Task<Result<Patient>> SearchPatientAsync(SearchParams searchParameters);
}