using System.Text;
using System.Text.Json.Serialization;
using System.Net.Http.Json;

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
                decription = "This is a new repository created from a template",
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
    }


}