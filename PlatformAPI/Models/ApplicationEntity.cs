using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http.HttpResults;
using System;

namespace PlatformAPI.Models;

public class ApplicationEntity : ITableEntity
{
    public string PartitionKey {get; set;}
    public string RowKey {get; set;}
    public DateTimeOffset? Timestamp {get; set;}
    public ETag ETag {get; set;}

    public string AppName {get; set;}
    public string RepositoryUrl {get; set;}
    public string AzureAppClientId {get; set;}
    public string TenantId {get; set;}
    public string SubscriptionId {get; set;}
    public DateTimeOffset CreatedAt {get; set;}
    public bool IsRegistered {get; set;}
    public bool SecretsAdded {get; set;}

    public ApplicationEntity()
    {
        PartitionKey = "Application";
    }

    public ApplicationEntity(string appName) : this()
    {
        AppName = appName;
        RowKey = appName.ToLowerInvariant();
        CreatedAt = DateTimeOffset.UtcNow;
        IsRegistered = false;
        SecretsAdded = false;
    }
}