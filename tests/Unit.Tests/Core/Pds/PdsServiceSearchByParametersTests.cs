using System.Net;
using Core.Common.Abstractions.Clients;
using Core.Common.Abstractions.Converters;
using Core.Common.Results;
using Core.Pds;
using Core.Pds.Abstractions;
using Core.Pds.Exceptions;
using Core.Pds.Models;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.Extensions.Logging;
using Task = System.Threading.Tasks.Task;

namespace Unit.Tests.Core.Pds;

public class PdsServiceSearchByParametersTests
{
    private readonly IDataHubFhirClient _fhirClient;
    private readonly IPdsFhirClient _pdsFhirClient;
    private readonly PdsService _sut;

    public PdsServiceSearchByParametersTests()
    {
        var logger = Substitute.For<ILogger<PdsService>>();
        var pdsMeshClient = Substitute.For<IPdsMeshClient>();
        var csvToJsonConverter = Substitute.For<IConverter<string, Result<string>>>();
        var bundleToCsvConverter = Substitute.For<IConverter<Bundle, Result<PdsMeshBundleToCsvConversionResult>>>();

        _pdsFhirClient = Substitute.For<IPdsFhirClient>();
        _fhirClient = Substitute.For<IDataHubFhirClient>();
        _sut = new PdsService(
            logger, pdsMeshClient, _pdsFhirClient, _fhirClient, csvToJsonConverter, bundleToCsvConverter);
    }

    private static readonly FhirOperationException NotFoundFhirOperationException =
        new("Not found", HttpStatusCode.NotFound);

    private static readonly PdsSearchParameters Model = new() { FamilyName = "Smith" };
    private static readonly Patient ExistingPatient = new() { Id = "1234567890" };
    private static readonly Patient NewPatient = new() { Id = Guid.NewGuid().ToString() };

    [Fact]
    public async Task Search_ShouldReturnApplicationException_WhenPatientDoesNotExistInPds()
    {
        _pdsFhirClient.SearchPatientAsync(Arg.Any<SearchParams>()).Returns(new PdsSearchPatientNotFoundException("Not found"));

        var result = await _sut.Search(Model);

        result.IsSuccess.ShouldBeFalse();
        result.Exception.ShouldBeOfType<PdsSearchPatientNotFoundException>();
    }

    [Fact]
    public async Task Search_ShouldReturnFailure_WhenPatientExistsInPdsButErrorOccursWhileUpdatingInDataHub()
    {
        var model = new PdsSearchParameters { FamilyName = "Smith" };
        var existingPatient = new Patient { Id = "1234567890" };
        _fhirClient.SearchResourceByParams<Patient>(Arg.Any<SearchParams>()).Returns(NotFoundFhirOperationException);
        _fhirClient.UpdateResource(existingPatient).Returns(new Exception("Error"));
        _pdsFhirClient.SearchPatientAsync(Arg.Any<SearchParams>()).Returns(existingPatient);

        var result = await _sut.Search(model);

        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public async Task Search_ShouldReturnPatientFromPds_WhenUpdateIsSuccessful()
    {
        var existingPatient = new Patient { Id = "1234567890" };
        _pdsFhirClient.SearchPatientAsync(Arg.Any<SearchParams>())!.Returns(existingPatient);
        _fhirClient.UpdateResource(Arg.Any<Patient>()).Returns(existingPatient);

        var result = await _sut.Search(Model);

        existingPatient.Meta = new Meta { Source = "Organization/cec48f09-f30e-cb9f-adc1-50e79d71796d" };
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(existingPatient);
    }
}