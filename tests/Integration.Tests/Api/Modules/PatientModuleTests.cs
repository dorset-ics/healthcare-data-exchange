using System.Net;
using System.Text.Json;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Quartz.Logging;
using Task = System.Threading.Tasks.Task;

namespace Integration.Tests.Api.Modules;

public class PatientModuleTests : IDisposable
{
    private readonly HttpClient _client;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions().ForFhir(ModelInfo.ModelInspector);
    private readonly ApiWebApplicationFactory _webApplicationFactory;

    public PatientModuleTests()
    {
        // https://github.com/quartznet/quartznet/issues/1781
        LogProvider.SetCurrentLogProvider(null);
        _webApplicationFactory = new ApiWebApplicationFactory();
        _client = _webApplicationFactory.CreateClient();
    }

    public void Dispose()
    {
        _webApplicationFactory.Dispose();
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task WhenSearch_WithValidParameters_ThenReturnsOk()
    {
        var response = await _client.GetAsync("/Patient?family=Smith&gender=female&birthdate=2010-10-22&email=jane.smith%40example.com&phone=01632960587");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task WhenSearch_WithValidParameters_AndPatientNotFound_ThenReturnsNotFound()
    {
        var response = await _client.GetAsync("/patient?family=not-a-person&birthdate=1948-10-12");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task WhenSearch_WithNoParameters_ThenReturnsBadRequest()
    {
        var response = await _client.GetAsync("/patient");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task WhenSearch_WithInvalidParameter_ThenReturnsBadRequest()
    {
        var response = await _client.GetAsync("/patient?family=pooley&birthdate=not-a-date");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task WhenGetPatient_WithInvalidNhsNumber_ThenReturnsBadRequest()
    {
        var response = await _client.GetAsync("/patient/9989011984");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task WhenGetPatient_WithNonExistingNhsNumber_ThenReturnsNotFound()
    {
        var response = await _client.GetAsync("/patient/8759655151");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task WhenGetPatient_WithValidNhsNumber_ThenReturnsPatient()
    {
        var response = await _client.GetAsync("/patient/9000000009");

        response.EnsureSuccessStatusCode();

        var patient = await response.Content.ReadFromJsonAsync<Patient>(_jsonSerializerOptions);
        patient.ShouldNotBeNull();
        patient.Name.First().Family.ShouldBe("Smith");
        patient.BirthDate.ShouldBe("2010-10-22");
    }

    [Fact]
    public async Task WhenGetPatient_WithValidNhsNumberAndSpaces_ThenReturnsPatient()
    {
        var response = await _client.GetAsync("/patient/900000 0025");

        response.EnsureSuccessStatusCode();

        var patient = await response.Content.ReadFromJsonAsync<Patient>(_jsonSerializerOptions);
        patient.ShouldNotBeNull();
        patient.Name.First().Family.ShouldBe("Smythe");
        patient.BirthDate.ShouldBe("2010-10-22");
    }
}