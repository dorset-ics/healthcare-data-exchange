namespace Core.Ndop.Extensions;

public static class NdopExtensions
{
    public static string ToNdopMeshMessageFileName(this DateTime dateTime)
    {
        return $"{Globals.NdopMeshMessageFileNamePrefix}_{dateTime:yyyyMMddHHmmss}.dat";
    }
}