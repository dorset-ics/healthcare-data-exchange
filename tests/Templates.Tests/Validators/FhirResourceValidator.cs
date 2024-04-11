using Hl7.Fhir.Model;

namespace Templates.Tests.Validators;

public static class FhirResourceValidator
{
    internal static void ValidateMetaProfileExistsInResource(Bundle bundle)
    {
        foreach (var entry in bundle.Entry.Where(e => e.Request.Method != Bundle.HTTPVerb.PATCH))
        {
            var profiles = entry.Resource.Meta?.Profile ?? [];
            profiles.ShouldNotBeEmpty($"Resource {entry.Resource.TypeName} is missing meta.profile");
        }
    }
}