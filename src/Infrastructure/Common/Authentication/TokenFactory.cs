using System.Text.Json.Nodes;

namespace Infrastructure.Common.Authentication;

public class TokenFactory(HttpClient httpClient, JwtHandler jwtHandler) : ITokenFactory, IDisposable
{
    public async Task<string> GetAccessToken()
    {
        var jwt = jwtHandler.GenerateJwt();
        var values = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" }, { "client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer" }, { "client_assertion", jwt }
        };
        var content = new FormUrlEncodedContent(values);

        using var response = await httpClient.PostAsync("oauth2/token", content);
        response.EnsureSuccessStatusCode();
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var responseObject = JsonNode.Parse(responseBody)
                             ?? throw new Exception($"Authentication failed - Unable to parse response:\n{response.Content}");

        return responseObject["access_token"]?.ToString()
               ?? throw new Exception($"Authentication failed - Unable to retrieve access token:\n{response.Content}");
    }

    public void Dispose()
    {
        httpClient?.Dispose();
        GC.SuppressFinalize(this);
    }
}