using System.Reflection;

namespace Core.Pds.Models;

public class PdsSearchParameters
{
    public string? FamilyName { get; set; }
    public string? GivenName { get; set; }
    public string? Gender { get; set; }
    public string? Postcode { get; set; }
    public string? DateOfBirth { get; set; }
    public string? DateOfDeath { get; set; }
    public string? RegisteredGpPractice { get; set; }
    public string? EmailAddress { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Identifier { get; set; }
    public string? IsFuzzyMatch { get; set; }
    public string? IsExactMatch { get; set; }
    public string? IsHistorySearch { get; set; }

    public static IEnumerable<PropertyInfo> GetFhirStandardPropertyInfos()
    {
        return typeof(PdsSearchParameters).GetProperties().ToArray();
    }
}