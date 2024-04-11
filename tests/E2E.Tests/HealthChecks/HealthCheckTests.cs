namespace E2E.Tests.HealthChecks;

[Trait("Category", "Smoke")]
public class HealthCheckTests(ITestOutputHelper outputHelper) : BaseApiTest(outputHelper)
{
    [Fact]
    public async Task WhenCallingHealthCheck_ThenServiceAndAllDependentServicesShouldBeHealthy()
    {
        var healthRequest = Get("/_health");

        var response = await RetryUntilSuccessful(
            () => ApiClient.Execute(healthRequest),
            6,
            10
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = JToken.Parse(response.Content!);
        content.Value<string>("status").ShouldBe("Healthy");
        content.SelectTokens("entries.*.status").All(s => s.Value<string>() == "Healthy").ShouldBeTrue();
    }
}