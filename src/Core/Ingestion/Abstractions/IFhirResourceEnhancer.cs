using Core.Common.Results;
using Hl7.Fhir.Model;

namespace Core.Ingestion.Abstractions;

public interface IFhirResourceEnhancer
{
    Result<Resource> Enrichment(Resource resource);
}