using Azure.Data.Tables;
using PlatformAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlatformAPI.Services;

public class ApplicationStorageService
{
    private readonly TableClient _tableClient;

    public ApplicationStorageService(string connectionString, string tableName)
    {
        var serviceClient = new TableServiceClient(connectionString);
        serviceClient.CreateTableIfNotExists(tableName);
        _tableClient = new TableClient(connectionString, tableName);
    }

    public async Task AddApplicationAsync(ApplicationEntity application)
    {
        await _tableClient.AddEntityAsync(application);
    }

    public async Task UpdateApplicationAsync(ApplicationEntity application)
    {
        await _tableClient.UpdateEntityAsync(application, application.ETag);
    }

    public async Task<ApplicationEntity> GetApplicationAsync(string appName)
    {
        return await _tableClient.GetEntityAsync<ApplicationEntity>("Application", appName.ToLowerInvariant());
    }

    public async Task<IEnumerable<ApplicationEntity>> GetApplicationsAsync()
    {
        var apps = new List<ApplicationEntity>();
        var query = _tableClient.QueryAsync<ApplicationEntity>(filter: $"PartitionKey eq 'Application'");
        await foreach (var app in query)
        {
            apps.Add(app);
        }

        return apps;
    }

    // Delete an application from storage
    public async Task DeleteApplicationAsync(string appName)
    {
        await _tableClient.DeleteEntityAsync("Application", appName.ToLowerInvariant());
    }
}