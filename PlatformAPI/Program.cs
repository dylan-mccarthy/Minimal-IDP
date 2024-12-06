using PlatformAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var config = builder.Configuration;

builder.Services.AddHttpClient<GitHubService>(client => {
    client.BaseAddress = new Uri("https://api.github.com");
    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.baptiste-preview+json");
    client.DefaultRequestHeaders.Add("User-Agent", "PlatformAPI");
});

var org = config["GitHub:Organization"];
var templateRepo = config["GitHub:TemplateRepo"];
var pat = config["GitHub:PersonalAccessToken"];

if(string.IsNullOrEmpty(org) || string.IsNullOrEmpty(templateRepo) || string.IsNullOrEmpty(pat))
{
    throw new InvalidOperationException("GitHub configuration is missing");
}

builder.Services.AddSingleton<GitHubService>(sp => 
    new GitHubService(sp.GetRequiredService<HttpClient>(), org, templateRepo, pat)
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/api/apps", async (AppCreationRequest request, GitHubService gitHubService) =>
{
    var repoUrl = await gitHubService.CreateRepositoryFromTemplateAsync(request.AppName);
    return repoUrl is not null
        ? Results.Created(repoUrl, new AppCreationResponse(repoUrl, "Repository created successfully"))
        : Results.BadRequest("Failed to create repository from template");
});	