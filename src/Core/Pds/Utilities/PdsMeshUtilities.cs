using System.Reflection;
using Core.Pds.Models;
using CsvHelper.Configuration.Attributes;

namespace Core.Pds.Utilities;

public static class PdsMeshUtilities
{
    public static string GetPdsMeshRecordResponseHeaderLine()
    {
        return string.Join(",", typeof(PdsMeshRecordResponse).GetProperties()
            .OrderBy(p => p.GetCustomAttribute<IndexAttribute>()?.Index)
            .Select(p => (p.GetCustomAttributes<NameAttribute>()?.FirstOrDefault())?.Names.FirstOrDefault()));
    }

    public static string GetPdsMeshRecordRequestHeaderLine()
    {
        return string.Join(",", typeof(PdsMeshRecordRequest).GetProperties()
            .OrderBy(p => p.GetCustomAttribute<IndexAttribute>()?.Index)
            .Select(p => (p.GetCustomAttributes<NameAttribute>()?.FirstOrDefault())?.Names.FirstOrDefault()));
    }
}