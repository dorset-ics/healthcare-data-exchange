using Core;
using Microsoft.AspNetCore.Mvc;

namespace Api.Models.Patient;

public class SearchModel
{
    [FromQuery(Name = Globals.PdsSearchQueryStringNames.FamilyName)] public string? FamilyName { get; set; }
    [FromQuery(Name = Globals.PdsSearchQueryStringNames.GivenName)] public string? GivenName { get; set; }
    [FromQuery(Name = Globals.PdsSearchQueryStringNames.Gender)] public string? Gender { get; set; }
    [FromQuery(Name = Globals.PdsSearchQueryStringNames.Postcode)] public string? Postcode { get; set; }
    [FromQuery(Name = Globals.PdsSearchQueryStringNames.DateOfBirth)] public string? DateOfBirth { get; set; }
    [FromQuery(Name = Globals.PdsSearchQueryStringNames.DateOfDeath)] public string? DateOfDeath { get; set; }
    [FromQuery(Name = Globals.PdsSearchQueryStringNames.RegisteredGpPractice)] public string? RegisteredGpPractice { get; set; }
    [FromQuery(Name = Globals.PdsSearchQueryStringNames.EmailAddress)] public string? EmailAddress { get; set; }
    [FromQuery(Name = Globals.PdsSearchQueryStringNames.PhoneNumber)] public string? PhoneNumber { get; set; }
    [FromQuery(Name = Globals.PdsSearchQueryStringNames.Identifier)] public string? Identifier { get; set; }
    [FromQuery(Name = Globals.PdsSearchQueryStringNames.IsFuzzyMatch)] public string? IsFuzzyMatch { get; set; }
    [FromQuery(Name = Globals.PdsSearchQueryStringNames.IsExactMatch)] public string? IsExactMatch { get; set; }
    [FromQuery(Name = Globals.PdsSearchQueryStringNames.IsHistorySearch)] public string? IsHistorySearch { get; set; }
}