using Microsoft.Graph;
using Azure.Identity;
using Microsoft.Graph.Models;


namespace PlatformAPI.Services;

public class AzureAdService
{
    private readonly string _tenantId;
    private GraphServiceClient _graphClient;

    public AzureAdService(string tenantId, string authority, string clientId, string clientSecret, string[] scopes)
    {
        _tenantId = tenantId;
        var clientSecretCredential = new ClientSecretCredential(_tenantId, clientId, clientSecret);
        _graphClient = new GraphServiceClient(clientSecretCredential, scopes);
    }

    public async Task<string?> CreateAzureAdAppAsync(string displayName)
    {
        try{
            var newApp = new Application
            {
                DisplayName = displayName,
            };

            var createdApp = await _graphClient.Applications.PostAsync(newApp);
            return createdApp?.Id;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }

    public async Task AddFederatedCredentialsAsync(string appObjectId, string repoFullName)
    {
        try {
            var requestBody = new FederatedIdentityCredential
            {
                Name = "GitHubActionsOIDC",
                Issuer = "https://token.actions.githubusercontent.com",
                Subject = $"repo:{repoFullName}:ref:refs/heads/main",
                Audiences = new List<string>
                {
                    "api://AzureADTokenExchange",
                },
                Description = "Federated credentials for GitHub Actions OIDC"
            };

            var result = await _graphClient.Applications[appObjectId].FederatedIdentityCredentials.PostAsync(requestBody);

            if (result == null)
            {
                throw new InvalidOperationException("Failed to add federated credentials");
            }

            return;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return;
        }
        
    }

}