using Hl7.Fhir.Model;

namespace Core.Common.Extensions;

public static class PatientExtensions
{
    public static string? GetNhsNumber(this Patient patient)
    {
        ArgumentNullException.ThrowIfNull(patient);
        return patient.Identifier.FirstOrDefault(t => t.System == Globals.NhsNumberSystem)?.Value;
    }
}