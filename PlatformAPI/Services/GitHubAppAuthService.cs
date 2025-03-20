using Microsoft.IdentityModel.Tokens;
using Octokit;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace PlatformAPI.Services;

public class GitHubAppAuthService
{
    private readonly string _appId;
    private readonly string _privateKey;
    private long _installationId;

    // Cache
    private string? _installationToken;
    private DateTime _tokenExpiry = DateTime.MinValue;
    private GitHubClient? _cachedClient;

    public GitHubAppAuthService(string appId, string privateKey)
    {
        _appId = appId;
        _privateKey = privateKey;
    }

    public async Task<string?> GetInstallationTokenAsync()
    {
        try
        {
            // Check if we have a valid token
            // if (_installationToken != null && DateTime.UtcNow < _tokenExpiry)
            // {
            //     return _installationToken;
            // }

            // Create a GitHub client authenticated as the GitHub App
            var appClient = new GitHubClient(new ProductHeaderValue("Minimal-IDP"))
            {
                Credentials = new Credentials(CreateJwtToken(), AuthenticationType.Bearer)
            };

            //var installations = await appClient.GitHubApps.GetAllInstallationsForCurrent();
            _installationId = 62891734;

            // Get a token for the installation
            var response = await appClient.GitHubApps.CreateInstallationToken(_installationId);
            
            _installationToken = response.Token;
            // GitHub installation tokens are valid for 1 hour
            // Setting expiry to 50 minutes to be safe
            _tokenExpiry = DateTime.UtcNow.AddMinutes(50);

            return _installationToken;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }

    // Create a JWT token for the GitHub App
    private string CreateJwtToken()
    {
       //var parsedKey = ParsePemPrivateKey(_privateKey);
       using var rsa = RSA.Create();
       rsa.ImportFromPem(_privateKey);

       var now = DateTime.UtcNow;
       var tokenHandler = new JwtSecurityTokenHandler();

       var securityKey = new RsaSecurityKey(rsa.ExportParameters(true));

       var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
       {
              Issuer = _appId,
              IssuedAt = DateTime.UtcNow.AddSeconds(-60),
              Expires = DateTime.UtcNow.AddMinutes(8),
              SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256)
       });

       return tokenHandler.WriteToken(token);
    }

    // Create a GitHub client with installation token
    public async Task<GitHubClient?> CreateGitHubClientAsync()
    {
        try
        {
            // Check if we have a client with valid token
            if (_cachedClient != null && _installationToken != null && DateTime.UtcNow < _tokenExpiry)
            {
                return _cachedClient;
            }

            var token = await GetInstallationTokenAsync();
            if (token == null)
            {
                return null;
            }

            _cachedClient = new GitHubClient(new ProductHeaderValue("Minimal-IDP"))
            {
                Credentials = new Credentials(token)
            };
            return _cachedClient;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }
}