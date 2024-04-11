using Core.Common.Extensions;
using Core.Pds.Models;
using Hl7.Fhir.Model;

namespace Core.Pds.Extensions;

public static class PdsExtensions
{
    public static PdsMeshRecordRequest ToPdsMeshRecord(this Resource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        if (resource is not Patient patient)
        {
            throw new ArgumentException($"Resource {resource.Id} is not a patient", nameof(resource));
        }

        var nhsNumber = patient.GetNhsNumber();
        return string.IsNullOrEmpty(nhsNumber)
            ? throw new ApplicationException($"Patient {patient.Id} doesn't have NHS number")
            : new PdsMeshRecordRequest { UniqueReference = patient.Id, NhsNumber = nhsNumber };
    }

    public static string ToPdsMeshMessageFileName(this DateTime dateTime)
    {
        return $"{Globals.PdsMeshMessageFileNamePrefix}_{dateTime:yyyyMMddHHmmss}.csv";
    }
}