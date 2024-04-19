using Core.Common.Results;
using Core.Pds.Models;
using Hl7.Fhir.Model;
using Task = System.Threading.Tasks.Task;

namespace Core.Pds.Abstractions;

public interface IPdsService
{
    Task RetrieveMeshMessages(CancellationToken cancellationToken);
    Task SendMeshMessages(CancellationToken cancellationToken);
    Task<Result<Patient>> Search(PdsSearchParameters searchParameters);
    Task<Result<Patient>> GetPatientById(string nhsNumber);
}