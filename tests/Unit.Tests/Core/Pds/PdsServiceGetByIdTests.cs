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

public class PdsServiceGetByIdTests
{
    private readonly IDataHubFhirClient _fhirClient;
    private readonly IPdsFhirClient _pdsFhirClient;
    private readonly PdsService _sut;

    public PdsServiceGetByIdTests()
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

    [Fact]
    public async Task GivenValidNhsNumber_WhenPatientExistsInDataHub_ShouldReturnPatient()
    {
        const string nhsNumber = "9730524319";
        var existingPatient = new Patient { Id = nhsNumber };
        _fhirClient.GetResource<Patient>(nhsNumber).Returns(existingPatient);

        var result = await _sut.GetPatientById(nhsNumber);

        await _pdsFhirClient.DidNotReceive().GetPatientByIdAsync(nhsNumber);
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Value.ShouldBe(existingPatient);
        result.Exception.ShouldBeNull();
    }

    [Fact]
    public async Task GivenValidNhsNumber_WhenPatientDoesNotExistInDataHubButExistsInPds_ShouldReturnPatientFromPds()
    {
        const string nhsNumber = "9730524319";
        var existingPatient = new Patient { Id = nhsNumber };
        _fhirClient.GetResource<Patient>(nhsNumber).Returns(new FhirOperationException("Not found", HttpStatusCode.NotFound));
        _pdsFhirClient.GetPatientByIdAsync(nhsNumber).Returns(existingPatient);
        _fhirClient.UpdateResource(existingPatient).Returns(existingPatient);
        var result = await _sut.GetPatientById(nhsNumber);

        await _fhirClient.Received(1).UpdateResource(existingPatient);
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Value.ShouldBe(existingPatient);
        result.Exception.ShouldBeNull();
    }

    [Fact]
    public async Task GivenValidNhsNumber_WhenPatientDoesNotExistInDataHubAndPds_ShouldReturnFailure()
    {
        const string nhsNumber = "9730524319";
        _fhirClient.GetResource<Patient>(nhsNumber).Returns(new FhirOperationException("Not found", HttpStatusCode.NotFound));
        _pdsFhirClient.GetPatientByIdAsync(nhsNumber).Returns(new PdsSearchPatientNotFoundException("Not found"));

        var result = await _sut.GetPatientById(nhsNumber);

        result.IsFailure.ShouldBeTrue();
        result.Exception.ShouldBeOfType<PdsSearchPatientNotFoundException>();
    }

    [Fact]
    public async Task GivenValidNhsNumber_WhenPatientExistsInPdsButErrorOccursWhileUpdatingInDataHub_ShouldReturnFailure()
    {
        const string nhsNumber = "9730524319";
        var existingPatient = new Patient { Id = nhsNumber };
        _fhirClient.GetResource<Patient>(nhsNumber).Returns(new FhirOperationException("Not found", HttpStatusCode.NotFound));
        _pdsFhirClient.GetPatientByIdAsync(nhsNumber).Returns(existingPatient);
        _fhirClient.UpdateResource(existingPatient).Returns(new Exception("error"));

        var result = await _sut.GetPatientById(nhsNumber);

        result.IsFailure.ShouldBeTrue();
        result.Exception.ShouldBeOfType<ApplicationException>();
    }

    [Fact]
    public async Task GivenValidNhsNumber_WhenErrorOccursWhileFetchingPatientFromDataHub_ShouldReturnFailure()
    {
        const string nhsNumber = "9730524319";
        _fhirClient.GetResource<Patient>(nhsNumber).Returns(new Exception("error"));

        var result = await _sut.GetPatientById(nhsNumber);

        result.IsFailure.ShouldBeTrue();
        result.Exception.ShouldBeOfType<ApplicationException>();
    }
}