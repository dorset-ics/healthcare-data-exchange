namespace Core.Ndop.Models;

public record NdopMeshBundleToCsvConversionResult(string Csv, IEnumerable<string?> NhsNumbers);