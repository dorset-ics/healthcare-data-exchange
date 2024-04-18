using System.Net.Http.Headers;
using Infrastructure.Common.Authentication;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Common.Handlers;

public class HttpClientAuthenticationHandler(
    ITokenFactory tokenFactory,
    HttpClientHandler handler,
    ILogger<HttpClientAuthenticationHandler> logger,
    bool isAuthEnabled = true)
    : DelegatingHandler(handler)
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (isAuthEnabled)
        {
            await AddAuthenticationHeader(request);
        }

        AddCustomHeaders(request);
        return await base.SendAsync(request, cancellationToken);
    }

    private async Task AddAuthenticationHeader(HttpRequestMessage request)
    {
        try
        {
            var authenticationToken = await tokenFactory.GetAccessToken();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authenticationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"failed to authenticate with backend service {request.Method} {request.RequestUri}");
            throw new HttpRequestException("Unable to authenticate with backend service.");
        }
    }

    private void AddCustomHeaders(HttpRequestMessage request)
    {
        request.Headers.Add("x-request-id", Guid.NewGuid().ToString());
        request.Headers.Add("accept", "application/fhir+json");
    }
}