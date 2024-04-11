namespace Core.Ndop.Models;

public record NdopMeshConversionRequest(string Csv, IEnumerable<string> RequestIdsSentToMesh);

