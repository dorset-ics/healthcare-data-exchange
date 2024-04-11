using Core.Common.Models;
using Core.Common.Results;
using Hl7.Fhir.Model;

namespace Core.Common.Abstractions.Clients;

public interface IDataHubFhirServiceClient
{
    Task<Result<Bundle>> ConvertData(ConvertDataRequest convertDataRequest);
    Task<Result<OperationOutcome>> ValidateData<T>(T? resource) where T : Resource;
}