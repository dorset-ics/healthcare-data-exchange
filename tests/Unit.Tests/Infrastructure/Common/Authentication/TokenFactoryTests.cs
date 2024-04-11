using System.IdentityModel.Tokens.Jwt;
using System.Net;
using FluentAssertions;
using Infrastructure.Common.Authentication;
using Infrastructure.Pds.Fhir.Configuration;
using Unit.Tests.Infrastructure.Common.HealthCheck;

namespace Unit.Tests.Infrastructure.Common.Authentication;

public class TokenFactoryTests
{
    private const string ClientId = "clientid";
    private const string TokenUrl = "https://token.url.nhs.uk";
    private static readonly PdsAuthConfiguration MockAuthConfig = GetMockPdsAuthConfiguration();

    [Fact]
    public void GenerateJwt_WhenGivingPdsAuthConfiguration_MatchingJwtTokenReturned()
    {
        var jwt = new JwtHandler(MockAuthConfig).GenerateJwt();

        jwt.Should().NotBeNullOrEmpty();
        var jwtSecurityToken = new JwtSecurityToken(jwt);
        jwtSecurityToken.Issuer.ShouldBe(ClientId);
        jwtSecurityToken.Subject.ShouldBe(ClientId);
        jwtSecurityToken.Claims.ShouldNotBeNull();
        jwtSecurityToken.Audiences.Should().Contain($"{TokenUrl}/oauth2/token");
    }

    [Fact]
    public async Task GetAccessToken_AndReturnedAccessTokenResponse_ResponseAccessTokenValueReturned()
    {
        var httpClientMock = HttpClientMocker.SetupHttpClient(Substitute.For<IHttpClientFactory>(), HttpStatusCode.OK,
            "{\"access_token\": \"token\"}");
        var jwtHandler = new JwtHandler(MockAuthConfig);

        var accessToken = await new TokenFactory(httpClientMock, jwtHandler).GetAccessToken();

        accessToken.ShouldBe("token");
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"not_access_token_key\": \"token\"}")]
    public void GetAccessToken_AndReturnedAccessTokenResponseMalformed_ThenExceptionThrown(string response)
    {
        var httpClientMock = HttpClientMocker.SetupHttpClient(Substitute.For<IHttpClientFactory>(), HttpStatusCode.OK, response);
        var jwtHandler = new JwtHandler(MockAuthConfig);

        Should.Throw<Exception>(async () => await new TokenFactory(httpClientMock, jwtHandler).GetAccessToken())
            .Message.Should().Contain("Authentication failed - Unable to retrieve access token");
    }

    [Fact]
    public void GetAccessToken_AndReturnedAccessTokenResponseNotOk_ThenExceptionThrown()
    {
        var httpClientMock = HttpClientMocker.SetupHttpClient(Substitute.For<IHttpClientFactory>(), HttpStatusCode.BadRequest, "{}");
        var jwtHandler = new JwtHandler(MockAuthConfig);

        Should.Throw<Exception>(async () => await new TokenFactory(httpClientMock, jwtHandler).GetAccessToken())
            .Message.Should().Contain("Response status code does not indicate success: 400 (Bad Request).");
    }

    [Fact]
    public void WhenDisposeCalled_AllDependenciesAreDisposed()
    {
        var httpClient = Substitute.ForPartsOf<HttpClient>();

        new TokenFactory(httpClient, null!).Dispose();

        httpClient.Received(1).Dispose();
    }

    private static PdsAuthConfiguration GetMockPdsAuthConfiguration()
    {
        return new PdsAuthConfiguration(true, TokenUrl, ClientId, "kid", Certificate: GetMockedPdsFhirCertificate());
    }

    private static string GetMockedPdsFhirCertificate()
        => """
           -----BEGIN RSA PRIVATE KEY-----
           MIICWwIBAAKBgGvOuDMRDJQEAYLQd443pVixCC4Vc7hijYhoW2gNsLb/sb/s1Ey+
           8bEhZt5hBAp/yWWRSuPgTGv8YgkTI+8C3YhqPb55icWG1tIVpNugAIaCMoe5Z2Bc
           vLzvfsesSDiEroEmo7E3ulVDU3Pl0yAT2WMg/Ocblk/PaH2LhnT5Yki5AgMBAAEC
           gYAPk/lt96K3qLSHMJR2CnhsDni+H/9uv17wPRQoPwIwD1aiAxjSVi0aiVcR/zbU
           RY7WjF4j+39Pg1KvOQLSQLm+EmTRMeO/v+4rde5cIe2vmxtpQTlMDtyU9J/LJ6Um
           2wNoIgFvArcwJv/jK7PfOwyPfsbWHzz+E/nu/gmwx0PkOQJBALNkr+Ud0HVpiBxI
           pwQkCMhQmBESD4BjBQ9+SJ1+zYxjuWDOXmuB1TJprofH8T3nMGhtsL3h7L0SDLPH
           uOyswMsCQQCZ2EHQkjD7NUspKK3siClczvML6C1V31Gonl+bWUXEtOFA5eTg7CLn
           /ApdsOwl6iCuVQN9CFCpxlSDYmw5IgALAkEAsgFo4AoTfU34N0iIIX24ETyXh+jJ
           5PVcYiFG4LCgOXwCyGI+IqMz79AZ1LW7VVeAGz8sr13s0TeFzyaRApfwvQJAAY+h
           M1WYa6QhzBwej6zeBpQPAUrs0tc+Q+C/hZsFSzaupnLuvJ2IySPUkxjNfKEAjeRM
           8cLY1rAtgVvJT1cZ+wJAfOsZiYQltc6O4F7aXFqIZT0mm8xbZkXtCz8D34HDhODP
           cPUAzzPhqiGq7ub9M1tcADmRIezTZjcsJnzUiqmQNQ==
           -----END RSA PRIVATE KEY-----
           """;
}