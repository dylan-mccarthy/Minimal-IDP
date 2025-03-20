using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using PlatformAPI.Services;
using PlatformAPI.Models;

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

var org = config["GitHub:Organization"];
var templateRepo = config["GitHub:TemplateRepo"];
var githubAppId = config["GitHub:App:ClientId"];
var githubAppPrivateKey = config["GitHub:App:PrivateKeyPath"];

var tenantId = config["Azure:TenantId"];
var subscriptionId = config["Azure:SubscriptionId"];

var graphAuthority = config["Graph:Authority"];
var graphClientId = config["Graph:ClientId"];
var graphClientSecret = config["Graph:ClientSecret"];

var storageConnectionString = config["Storage:ConnectionString"];
var storageTableName = config["Storage:TableName"];

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

if(string.IsNullOrEmpty(storageConnectionString) || string.IsNullOrEmpty(storageTableName))
{
    throw new InvalidOperationException("Storage configuration is missing");
}

builder.Services.AddSingleton<ApplicationStorageService>(sp => 
{
    return new ApplicationStorageService(storageConnectionString, storageTableName);
});

builder.Services.AddScoped<GitHubAppAuthService>(sp => 
{
    var appId = githubAppId;
    var privateKey = File.ReadAllText(githubAppPrivateKey);
    return new GitHubAppAuthService(appId, privateKey);
});

builder.Services.AddScoped<GitHubService>(sp => 
{
    var gitHubAppAuthService = sp.GetRequiredService<GitHubAppAuthService>();
    return new GitHubService(org, templateRepo, gitHubAppAuthService);
});

builder.Services.AddSingleton<AzureAdService>(sp => 
{
    return new AzureAdService(tenantId, graphAuthority, graphClientId, graphClientSecret, new[] { "https://graph.microsoft.com/.default" });
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.MapPost("/api/apps", async ([FromBody]AppCreationRequest request, GitHubService gitHubService, AzureAdService azureAdService, ApplicationStorageService applicationStorageService) =>
{

    try{
        if(string.IsNullOrWhiteSpace(request.AppName))
        {
            return Results.BadRequest("App name is required");
        }

        var sanitaisedAppName = request.AppName.Trim().Replace(" ", "-").ToLowerInvariant();

        var repoUrl = await gitHubService.CreateRepositoryFromTemplateAsync(sanitaisedAppName);

        if(repoUrl == null)
        {
            return Results.BadRequest("Failed to create repository");
        }

        var repoFullName = $"{org}/{sanitaisedAppName}";

        var application = new ApplicationEntity(sanitaisedAppName)
        {
            RepositoryUrl = repoUrl,
            IsRegistered = false,
            SecretsAdded = false,
        };

        await applicationStorageService.AddApplicationAsync(application);

        return Results.Ok(new AppCreationResponse($"https://github.com/{org}/{repoUrl}"!, "created", null));

    }
    catch(Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapPost("/api/apps/{appName}/register", async ([FromRoute]string appName, [FromBody]AppRegistrationRequest request, GitHubService gitHubService, AzureAdService azureAdService, ApplicationStorageService applicationStorageService) =>
{
    try{
        var sanitaisedAppName = appName.Trim().Replace(" ", "-").ToLowerInvariant();
        var repoFullName = $"{org}/{sanitaisedAppName}";

        var appId = await azureAdService.CreateAzureAdAppAsync(sanitaisedAppName);

        if(string.IsNullOrEmpty(appId))
        {
            return Results.BadRequest("Failed to create Azure AD app");
        }

        await azureAdService.AddFederatedCredentialsAsync(appId, repoFullName);

        var application = await applicationStorageService.GetApplicationAsync(sanitaisedAppName);
        application.AzureAppClientId = appId;
        application.TenantId = tenantId;
        application.SubscriptionId = subscriptionId;
        application.IsRegistered = true;

        await applicationStorageService.UpdateApplicationAsync(application);

        return Results.Ok(new AppRegistrationResponse(appId, tenantId, subscriptionId, appId));
    }
    catch(Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapPost("/api/apps/{appName}/secrets", async ([FromRoute]string appName, [FromBody]AppAddSecretsRequest request, GitHubService gitHubService, ApplicationStorageService applicationStorageService) =>
{
    try{
        var sanitaisedAppName = appName.Trim().Replace(" ", "-").ToLowerInvariant();
        await gitHubService.SetRepoSecretAsync(sanitaisedAppName, "AZURE_TENANT_ID", request.TenantId);
        await gitHubService.SetRepoSecretAsync(sanitaisedAppName, "AZURE_SUBSCRIPTION_ID", request.SubscriptionId);
        await gitHubService.SetRepoSecretAsync(sanitaisedAppName, "AZURE_CLIENT_ID", request.ClientId);

        var application = await applicationStorageService.GetApplicationAsync(sanitaisedAppName);
        application.SecretsAdded = true;

        await applicationStorageService.UpdateApplicationAsync(application);

        return Results.Ok(new AppAddSecretsResponse("secrets added"));
    }
    catch(Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapGet("/api/apps", async (ApplicationStorageService applicationStorageService) =>
{
    var apps = await applicationStorageService.GetApplicationsAsync();
    return Results.Ok(apps);
});

app.MapGet("/api/apps/{appName}", async ([FromRoute]string appName, ApplicationStorageService applicationStorageService) =>
{
    var app = await applicationStorageService.GetApplicationAsync(appName);
    return app != null ? Results.Ok(app) : Results.NotFound();
});

app.UseCors("AllowAll");
app.UseRouting();
app.Run();