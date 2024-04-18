using Infrastructure.Common.Authentication;
using Infrastructure.Common.Handlers;
using Microsoft.Extensions.Logging;
using NSubstitute.ExceptionExtensions;

namespace Unit.Tests.Infrastructure.Common.Handlers;

public class HttpClientAuthenticationHandlersTests
{
    private readonly ITokenFactory _tokenFactoryMock = Substitute.For<ITokenFactory>();
    private const string Uri = "http://localhost";

    [Fact]
    public void SendAsync_ShouldAddAuthenticationHeader_WhenIsAuthEnabled()
    {
        var mockLogger = Substitute.For<ILogger<HttpClientAuthenticationHandler>>();
        var request = new HttpRequestMessage(HttpMethod.Get, Uri);
        var handler = new HttpClientHandler
        {
            CheckCertificateRevocationList = true
        };

        var httpClient = new HttpClient(new HttpClientAuthenticationHandler(_tokenFactoryMock, handler, mockLogger))
        {
            BaseAddress = new Uri(Uri)
        };
        httpClient.SendAsync(request);

        request.Headers.Authorization.ShouldNotBeNull();
        request.Headers.Authorization.Scheme.ShouldBe("Bearer");
        request.Headers.GetValues("x-request-id").ShouldHaveSingleItem();
        request.Headers.Accept.ShouldHaveSingleItem();
        request.Headers.Accept.First().MediaType.ShouldBe("application/fhir+json");
    }

    [Fact]
    public void SendAsync_ShouldNotAddAuthenticationHeader_WhenTokenFactoryThrowsException()
    {
        var mockLogger = Substitute.For<ILogger<HttpClientAuthenticationHandler>>();
        _tokenFactoryMock.GetAccessToken().Throws(new Exception());
        var request = new HttpRequestMessage(HttpMethod.Get, Uri);
        var handler = new HttpClientHandler
        {
            CheckCertificateRevocationList = true
        };

        var httpClient = new HttpClient(new HttpClientAuthenticationHandler(_tokenFactoryMock, handler, mockLogger))
        {
            BaseAddress = new Uri(Uri)
        };

        httpClient.SendAsync(request).Throws(new HttpRequestException());

        request.Headers.Authorization.ShouldBeNull();
    }

    [Fact]
    public void SendAsync_ShouldAddAuthenticationHeader_WhenIsAuthNotEnabled()
    {
        var mockLogger = Substitute.For<ILogger<HttpClientAuthenticationHandler>>();
        var request = new HttpRequestMessage(HttpMethod.Get, Uri);
        var handler = new HttpClientHandler
        {
            CheckCertificateRevocationList = true
        };

        var httpClient = new HttpClient(new HttpClientAuthenticationHandler(_tokenFactoryMock, handler, mockLogger, false))
        {
            BaseAddress = new Uri(Uri)
        };
        httpClient.SendAsync(request);

        request.Headers.Authorization.ShouldBeNull();
    }
}