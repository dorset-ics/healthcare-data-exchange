using System.Net;
using Core.Pds.Exceptions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Infrastructure.Pds.Fhir.Clients;
using Infrastructure.Pds.Fhir.Clients.Abstractions;
using Microsoft.Extensions.Logging;
using NSubstitute.ExceptionExtensions;
using Task = System.Threading.Tasks.Task;


namespace Unit.Tests.Infrastructure.Pds.Fhir.Clients;

public class PdsFhirClientTests
{
    private readonly IPdsFhirClientWrapper _pdsFhirClientMock;
    private readonly PdsFhirClient _pdsFhirClient;
    private static readonly string ResourceId = "123";
    private readonly Patient _existingResource = new() { Id = ResourceId };

    public PdsFhirClientTests()
    {
        var loggerMock = Substitute.For<ILogger<PdsFhirClient>>();
        _pdsFhirClientMock = Substitute.For<IPdsFhirClientWrapper>();
        _pdsFhirClient = new PdsFhirClient(loggerMock, _pdsFhirClientMock);
    }

    [Fact]
    public async Task GetPatientByIdAsync_ShouldReturnPatient_WhenPatientExists()
    {
        _pdsFhirClientMock.ReadAsync<Patient>($"Patient/{ResourceId}").Returns(_existingResource);

        var result = await _pdsFhirClient.GetPatientByIdAsync(ResourceId);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBe(_existingResource);
    }

    [Fact]
    public async Task GetPatientByIdAsync_ShouldReturnNull_WhenPatientDoesNotExist()
    {
        _pdsFhirClientMock.ReadAsync<Patient>($"Patient/{ResourceId}")
            .Throws(new FhirOperationException("Not Found", HttpStatusCode.NotFound));

        var result = await _pdsFhirClient.GetPatientByIdAsync(ResourceId);

        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Exception.ShouldBeOfType<PdsSearchPatientNotFoundException>();
    }

    [Fact]
    public async Task GetPatientByIdAsync_ShouldRethrowException_WhenPdsThrowsException()
    {
        _pdsFhirClientMock.ReadAsync<Patient>($"Patient/{ResourceId}")
            .Throws(new FhirOperationException("Bad Request", HttpStatusCode.BadRequest));

        var result = await _pdsFhirClient.GetPatientByIdAsync(ResourceId);

        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Exception.ShouldBeOfType<FhirOperationException>();
    }
}