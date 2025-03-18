using Microsoft.IdentityModel.Tokens;
using Octokit;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace PlatformAPI.Services;

public class GitHubAppAuthService
{
    private readonly int _appId;
    private readonly string _privateKey;
    private readonly long _installationId;

    // Cache
    private string? _installationToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public GitHubAppAuthService(int appId, string privateKey)
    {
        _appId = appId;
        _privateKey = privateKey;
    }

    public async Task<string> GetInstallationTokenAsync()
    {
        // Return cached token if valid
        if (_installationToken != null && DateTime.UtcNow < _tokenExpiry.AddMinutes(-5))
            return _installationToken;

        // Create a GitHub client authenticated as the GitHub App
        var appClient = new GitHubClient(new ProductHeaderValue("Minimal-IDP"))
        {
            Credentials = new Credentials(CreateJwtToken(), AuthenticationType.Bearer)
        };

        // Get a token for the installation
        var response = await appClient.GitHubApps.CreateInstallationToken(_installationId);
        
        _installationToken = response.Token;

        return _installationToken;
    }

    // Create a JWT token for the GitHub App
    private string CreateJwtToken()
    {
        var now = DateTimeOffset.UtcNow;
        var payload = new JwtPayload
        {
            { "iat", now.ToUnixTimeSeconds() },
            { "exp", now.AddMinutes(10).ToUnixTimeSeconds() },
            { "iss", _appId }
        };

        using (var rsa = RSA.Create())
        {
            rsa.ImportRSAPrivateKey(Convert.FromBase64String(_privateKey), out _);
            var jwtHeader = new JwtHeader(new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256));
            var jwtPayload = payload;
            var jwtToken = new JwtSecurityToken(jwtHeader, jwtPayload);
            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }
    }

    // Create a GitHub client with installation token
    public async Task<GitHubClient> CreateGitHubClientAsync()
    {
        var token = await GetInstallationTokenAsync();
        var client = new GitHubClient(new ProductHeaderValue("Minimal-IDP"))
        {
            Credentials = new Credentials(token)
        };
        return client;
    }
}