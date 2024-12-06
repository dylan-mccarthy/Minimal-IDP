using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace PlatformAPI.Services;

public class AzureAdService
{
    private readonly HttpClient _httpClient;
    private readonly string _tenantId;

    public AzureAdService(HttpClient httpClient, string tenantId)
    {
        _httpClient = httpClient;
        _tenantId = tenantId;
    }

    public async Task<string> GetAccessTokenAsync(string authority, string clientId, string clientSecret, string[] scopes)
    {
        var tokenEndpoint = $"{authority}/oauth2/v2.0/token";
        using var req = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint);
        var form = new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["scope"] = string.Join(' ', scopes),
            ["client_secret"] = clientSecret,
            ["grant_type"] = "client_credentials"
        };
        req.Content = new FormUrlEncodedContent(form);
        var response = await _httpClient.SendAsync(req);
        response.EnsureSuccessStatusCode();
        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);
        return doc.RootElement.GetProperty("access_token").GetString()!;
    }

    public async Task<CreateApplicationResponse> CreateAzureAdAppAsync(string accessToken, string displayName)
    {
        var requestBody = new { displayName = displayName };
        using var req = new HttpRequestMessage(HttpMethod.Post, $"https://graph.microsoft.com/v1.0/{_tenantId}/applications");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        req.Content = JsonContent.Create(requestBody);

        var response = await _httpClient.SendAsync(req);
        response.EnsureSuccessStatusCode();
        var responseJson = await response.Content.ReadFromJsonAsync<CreateApplicationResponse>();
        return responseJson!;
    }

    public async Task AddFederatedCredentialsAsync(string accessToken, string appObjectId, string repoFullName)
    {
        var subject = $"repo:{repoFullName}:ref:refs/heads/main";

        var ficRequest = new FederatedIdentityCredentialRequest(
            name: "GitHubActionsOIDC",
            issuer: "https://token.actions.githubusercontent.com",
            subject: subject,
            audiences: new[] { "api://AzureADTokenExchange" },
            description: "Federated credentials for GitHub Actions OIDC"
        );

        using var req = new HttpRequestMessage(HttpMethod.Post, $"https://graph.microsoft.com/v1.0/applications/{appObjectId}/federatedIdentityCredentials");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        req.Content = JsonContent.Create(ficRequest);

        var res = await _httpClient.SendAsync(req);
        res.EnsureSuccessStatusCode();
    }
}