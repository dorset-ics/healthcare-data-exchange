namespace Core.Ndop.Models;

public record NdopMeshEnrichedRecordResponse(string NhsNumber, bool IsOptedOut) : NdopMeshRecordResponse(NhsNumber);