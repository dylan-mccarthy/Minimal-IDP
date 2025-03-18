using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using PlatformAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var config = builder.Configuration;

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});


builder.Services.AddHttpClient("GitHubClient", client => {
    client.BaseAddress = new Uri("https://api.github.com");
    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.baptiste-preview+json");
    client.DefaultRequestHeaders.Add("User-Agent", "PlatformAPI");
});

builder.Services.AddHttpClient<AzureAdService>("AzureAdClient");

var org = config["GitHub:Organization"];
var templateRepo = config["GitHub:TemplateRepo"];
var githubAppId = config["GitHub:AppId"];
var githubAppPrivateKey = config["GitHub:PrivateKey"];

var tenantId = config["AzureAd:TenantId"];
var subscriptionId = config["AzureAd:SubscriptionId"];

var graphAuthority = config["Graph:Authority"];
var graphClientId = config["Graph:ClientId"];
var graphClientSecret = config["Graph:ClientSecret"];

if(string.IsNullOrEmpty(org) || string.IsNullOrEmpty(templateRepo) || string.IsNullOrEmpty(githubAppId) || string.IsNullOrEmpty(githubAppPrivateKey))
{
    throw new InvalidOperationException("GitHub configuration is missing");
}

if(string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(subscriptionId))
{
    throw new InvalidOperationException("Azure AD configuration is missing");
}

if(string.IsNullOrEmpty(graphAuthority) || string.IsNullOrEmpty(graphClientId) || string.IsNullOrEmpty(graphClientSecret))
{
    throw new InvalidOperationException("Graph configuration is missing");
}

builder.Services.AddSingleton<GitHubAppAuthService>(sp => 
{
    var appId = int.Parse(githubAppId);
    var privateKey = File.ReadAllText(githubAppPrivateKey);
    return new GitHubAppAuthService(appId, githubAppPrivateKey);
});

builder.Services.AddSingleton<GitHubService>(sp => 
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("GitHubClient");
    var gitHubAppAuthService = sp.GetRequiredService<GitHubAppAuthService>();
    return new GitHubService(httpClient, org, templateRepo, gitHubAppAuthService);
});

builder.Services.AddSingleton<AzureAdService>(sp => 
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("AzureAdClient");
    return new AzureAdService(httpClient, tenantId);
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.MapPost("/api/apps", async ([FromBody]AppCreationRequest request, GitHubService gitHubService, AzureAdService azureAdService) =>
{

    try{
        if(string.IsNullOrWhiteSpace(request.AppName))
        {
            return Results.BadRequest("App name is required");
        }

        var sanitaisedAppName = request.AppName.Trim().Replace(" ", "-").ToLowerInvariant();

        var repoUrl = await gitHubService.CreateRepositoryFromTemplateAsync(sanitaisedAppName);
        var repoFullName = $"{org}/{sanitaisedAppName}";

        var accessToken = await azureAdService.GetAccessTokenAsync(graphAuthority, graphClientId, graphClientSecret, new[] { "https://graph.microsoft.com/.default" });

        var appResp = await azureAdService.CreateAzureAdAppAsync(accessToken, sanitaisedAppName);
        var azureAppClientId = appResp.appId;

        await azureAdService.AddFederatedCredentialsAsync(accessToken, appResp.id, repoFullName);

        gitHubService.SetRepoSecretAsync(sanitaisedAppName, "AZURE_TEANT_ID", tenantId);
        gitHubService.SetRepoSecretAsync(sanitaisedAppName, "AZURE_SUBSCRIPTION_ID", subscriptionId);
        gitHubService.SetRepoSecretAsync(sanitaisedAppName, "AZURE_CLIENT_ID", azureAppClientId);

        return Results.Ok(new AppCreationResponse($"https://github.com/{org}/{repoUrl}"!, "created", azureAppClientId));

    }
    catch(Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});	

app.UseCors("AllowAll");
app.UseRouting();
app.Run();