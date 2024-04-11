using Core.Pds.Models;
using Hl7.Fhir.Rest;

namespace Core.Pds.Extensions;

public static class PdsSearchParametersExtensions
{
    public static SearchParams ToFhirSearchParameters(this PdsSearchParameters pdsSearchParameters)
    {
        var fhirSearchParams = new SearchParams();

        var properties = pdsSearchParameters
            .GetType()
            .GetProperties()
            .Where(property => !string.IsNullOrWhiteSpace(property.GetValue(pdsSearchParameters)?.ToString()));

        foreach (var property in properties)
        {
            var queryStringName = typeof(Globals.PdsSearchQueryStringNames)?.GetField(property.Name)?.GetValue(null)?.ToString()
                ?? throw new ApplicationException("Could not access PDS querystring name");

            fhirSearchParams.Add(queryStringName, property.GetValue(pdsSearchParameters)?.ToString());
        }

        return fhirSearchParams;
    }
}