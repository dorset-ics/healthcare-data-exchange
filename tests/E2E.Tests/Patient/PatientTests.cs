namespace E2E.Tests.Patient;

public class PatientTests(ITestOutputHelper outputHelper) : BaseApiTest(outputHelper)
{

    [Fact]
    public void PatientSearch_CalledWithoutParameters_ReturnsBadRequest()
    {
        ApiClient.Execute(Get("/Patient")).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void PatientSearch_CalledWithSearchParameters_ReturnsPatientThatExistsInFhirAndHasTheSameNhsNumber()
    {
        var searchRequest = Get("/Patient",
            new List<QueryParameter>
            {
                new("family", "Smith"), new("birthdate", "eq2010-10-22"), new("gender", "female")
            });

        var response = ApiClient.Execute(searchRequest);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.ShouldNotBeEmpty();
        var content = JToken.Parse(response.Content!);
        var id = content.Value<string>("id");
        var actualPatient = FhirClient.Execute(Get($"/Patient/{id}"));
        response.Content.ShouldNotBeEmpty();
        var actualPatientContent = JToken.Parse(actualPatient.Content!);
        actualPatientContent.SelectTokens("identifier[*].system").Any(s => s.Value<string>() == "https://fhir.nhs.uk/Id/nhs-number").ShouldBeTrue();
        actualPatientContent.SelectTokens("identifier[*].value").Any(s => s.Value<string>() == id).ShouldBeTrue();
    }

    [Fact]
    public void PatientSearch_CalledWithNotMatchingSearchParameters_ReturnsNotFoundError()
    {
        var searchRequest = Get("/Patient",
            new List<QueryParameter>
            {
                new("family", "Hart"), new("given", "Sharon"), new("birthdate", "eq1990-12-26"), new("gender", "male")
            });

        var response = ApiClient.Execute(searchRequest);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public void PatientSearch_CalledWithNhsNumber_ReturnsPatientThatExistsInFhirAndHasTheSameNhsNumberId()
    {
        const string nhsNumber = "9000000009";
        var searchRequest = Get($"/Patient/{nhsNumber}");
        var response = ApiClient.Execute(searchRequest);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.ShouldNotBeEmpty();

        var content = JToken.Parse(response.Content!);
        var id = content.Value<string>("id");
        id.ShouldBe(nhsNumber);

        var actualPatient = FhirClient.Execute(Get($"/Patient/{id}"));
        response.Content.ShouldNotBeEmpty();
        var actualPatientContent = JToken.Parse(actualPatient.Content!);
        actualPatientContent.SelectTokens("identifier[*].system").Any(s => s.Value<string>() == "https://fhir.nhs.uk/Id/nhs-number").ShouldBeTrue();
        actualPatientContent.SelectTokens("identifier[*].value").Any(s => s.Value<string>() == nhsNumber).ShouldBeTrue();
    }
}