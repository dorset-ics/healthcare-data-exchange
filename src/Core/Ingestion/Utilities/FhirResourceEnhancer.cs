using Core.Common.Abstractions.Services;
using Core.Common.Results;
using Core.Ingestion.Abstractions;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;

namespace Core.Common.Utilities;

public class FhirResourceEnhancer : IFhirResourceEnhancer
{
    private readonly ITerminologyService _terminologyService;
    private readonly ILogger _logger;
    private const string SystemUrl = "http://snomed.info/sct";

    public FhirResourceEnhancer(ITerminologyService terminologyService, ILogger<FhirResourceEnhancer> logger)
    {
        _terminologyService = terminologyService;
        _logger = logger;
    }
    public Result<Resource> Enrichment(Resource resource)
    {
        if (resource is Bundle bundle)
        {
            foreach (var entry in bundle.Entry)
            {
                foreach (var child in entry.NamedChildren)
                {
                    AddDisplay(child.Value);
                }

            }
            return bundle;
        }
        AddDisplay(resource);

        return resource;
    }

    private void AddDisplay(Base resource)
    {
        resource.NamedChildren
            .Select(x => x.Value)
            .OfType<CodeableConcept>()
            .SelectMany(x => x.Coding)
            .Where(x => x.System == SystemUrl && x.DisplayElement is null)
            .ToList()
            .ForEach(x =>
            {
                var snomedDisplay = _terminologyService.GetSnomedDisplay(x.Code!.ToString());
                if (!string.IsNullOrWhiteSpace(snomedDisplay))
                {
                    x.DisplayElement = new FhirString(snomedDisplay);
                }
                else
                {
                    _logger.LogWarning($"Unable to append a snomed display for snomed code {x.Code}");
                }
            });
    }
}