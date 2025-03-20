using System.Text;
using System.Text.Json.Serialization;
using System.Net.Http.Json;
//using System.Security.Cryptography;
using System.Text.Json;
using Octokit;
using Sodium;

namespace PlatformAPI.Services
{
    public class GitHubService
    {
        private readonly string _org;
        private readonly string _templateRepo;
        private readonly GitHubAppAuthService _gitHubAppAuthService;

        public GitHubService(string org, string templateRepo, GitHubAppAuthService gitHubAppAuthService)
        {
            _org = org;
            _templateRepo = templateRepo;
            _gitHubAppAuthService = gitHubAppAuthService;
        }

        public async Task<string?> CreateRepositoryFromTemplateAsync(string newRepoName)
        {
            try{
                var client = await _gitHubAppAuthService.CreateGitHubClientAsync();
                if(client == null)
                {
                    return null;
                }
                var newRepo = new NewRepositoryFromTemplate(newRepoName);
                newRepo.Owner = _org;
                newRepo.Description = "A new repository created from a template";
                var result = await client.Repository.Generate(_org, _templateRepo, newRepo);
                return result?.CloneUrl;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }

        }

        public async Task SetRepoSecretAsync(string repoName, string secretName, string secretValue)
        {
            var client = await _gitHubAppAuthService.CreateGitHubClientAsync();
            if(client == null)
            {
                throw new Exception("Failed to create GitHub client");
            }
            
            // Get the public key only once
            var publicKeyData = await GetRepoPublicKeyAsync(repoName);
            if (publicKeyData.KeyId == null || publicKeyData.Key == null)
            {
                throw new Exception("Failed to get public key");
            }
            
            var secret = new UpsertRepositorySecret()
            {
                EncryptedValue = EncryptSecret(secretValue, publicKeyData.Key),
                KeyId = publicKeyData.KeyId
            };
            
            await client.Repository.Actions.Secrets.CreateOrUpdate(_org, repoName, secretName, secret);
        }

        private string EncryptSecret(string secretValue, string publicKey)
        {
            var binkey = Convert.FromBase64String(publicKey);
            var binsecret = System.Text.Encoding.UTF8.GetBytes(secretValue);

            var sealedPublicKeyBox = Sodium.SealedPublicKeyBox.Create(binsecret, binkey);
            return Convert.ToBase64String(sealedPublicKeyBox);
        }

        private async Task<(string KeyId, string Key)> GetRepoPublicKeyAsync(string repoName)
        {
            var client = await _gitHubAppAuthService.CreateGitHubClientAsync();
            if(client == null)
            {
                return (null!, null!);
            }
            var result = await client.Repository.Actions.Secrets.GetPublicKey(_org, repoName);
            var keyId = result.KeyId;
            var key = result.Key;
            return (keyId!, key!);
        }
    }


}