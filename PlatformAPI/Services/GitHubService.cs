using System.Text;
using System.Text.Json.Serialization;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;

namespace PlatformAPI.Services
{
    public class GitHubService
    {
        private readonly HttpClient _httpClient;
        private readonly string _org;
        private readonly string _templateRepo;

        public GitHubService(HttpClient httpClient, string org, string templateRepo, string pat)
        {
            _httpClient = httpClient;
            _org = org;
            _templateRepo = templateRepo;

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {pat}");
        }

        public async Task<string?> CreateRepositoryFromTemplateAsync(string newRepoName)
        {
            var requestBody = new
            {
                owner = _org,
                name = newRepoName,
                include_all_branches = false,
                @private = false
            };

            var response = await _httpClient.PostAsJsonAsync($"repos/{_org}/{_templateRepo}/generate", requestBody);

            if(!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to create repository from template: {errorContent}");
            }

            var responseJson = await response.Content.ReadFromJsonAsync<CreateRepoResponse>();
            return responseJson?.Owner?.HtmlUrl;

        }
        private class Owner
        {
            [JsonPropertyName("html_url")]
            public string? HtmlUrl { get; set; }
        }

        private class CreateRepoResponse
        {
            public Owner? Owner { get; set; }
        }

        public async Task SetRepoSecretAsync(string repoName, string secretName, string secretValue)
        {
            var publicKey = await GetRepoPublicKeyAsync(repoName);
            var encrypedValue = EncryptSecret(secretValue, publicKey.Key);

            var url = $"repos/{_org}/{repoName}/actions/secrets/{secretName}";
            var body = new 
            {
                encrypted_value = encrypedValue,
                key_id = publicKey.KeyId
            };

            var res = await _httpClient.PutAsJsonAsync(url, body);
            if(!res.IsSuccessStatusCode)
            {
                var errorContent = await res.Content.ReadAsStringAsync();
                throw new Exception($"Failed to set repository secret: {errorContent}");
            }
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
            var res = await _httpClient.GetAsync($"repos/{_org}/{repoName}/actions/secrets/public-key");
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadFromJsonAsync<JsonElement>();
            var keyId = json.GetProperty("key_id").GetString();
            var key = json.GetProperty("key").GetString();
            return (keyId!, key!);
        }
    }


}