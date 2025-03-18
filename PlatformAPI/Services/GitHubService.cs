using System.Text;
using System.Text.Json.Serialization;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using Octokit;

namespace PlatformAPI.Services
{
    public class GitHubService
    {
        private readonly string _org;
        private readonly string _templateRepo;
        private readonly GitHubAppAuthService _gitHubAppAuthService;

        public GitHubService(HttpClient httpClient, string org, string templateRepo, GitHubAppAuthService gitHubAppAuthService)
        {
            _org = org;
            _templateRepo = templateRepo;
            _gitHubAppAuthService = gitHubAppAuthService;
        }

        public async Task<string?> CreateRepositoryFromTemplateAsync(string newRepoName)
        {
            var client = await _gitHubAppAuthService.CreateGitHubClientAsync();
            var result = await client.Repository.Generate(_org, _templateRepo, new NewRepositoryFromTemplate(newRepoName));
            return result?.CloneUrl;
        }

        public async void SetRepoSecretAsync(string repoName, string secretName, string secretValue)
        {
            var client = await _gitHubAppAuthService.CreateGitHubClientAsync();
            var secret = new UpsertRepositorySecret()
            {
                EncryptedValue = EncryptSecret(secretValue, (await GetRepoPublicKeyAsync(repoName)).Key),
                KeyId = (await GetRepoPublicKeyAsync(repoName)).KeyId
            };
            await client.Repository.Actions.Secrets.CreateOrUpdate(_org, repoName, secretName, secret);
        }

        private string EncryptSecret(string secretValue, string publicKey)
        {
            // Encrypt secretValue with publicKey
            var rsa = RSA.Create();
            rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);
            var encryptedBytes = rsa.Encrypt(Encoding.UTF8.GetBytes(secretValue), RSAEncryptionPadding.OaepSHA256);
            return Convert.ToBase64String(encryptedBytes);
        }

        private async Task<(string KeyId, string Key)> GetRepoPublicKeyAsync(string repoName)
        {
            var client = await _gitHubAppAuthService.CreateGitHubClientAsync();
            var result = await client.Repository.Actions.Secrets.GetPublicKey(_org, repoName);
            var keyId = result.KeyId;
            var key = result.Key;
            return (keyId!, key!);
        }
    }


}