# Minimal-IDP Platform API

A .NET API for managing application deployments with GitHub repository creation, Entra ID application registration, and GitHub Action secret management.

## Features

- Create GitHub repositories from templates
- Register applications in Azure Entra ID
- Configure GitHub repository secrets for Azure deployments
- Manage application metadata in Azure Table Storage

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download) or later
- GitHub App registration with a private key
- Azure account with permissions to create app registrations
- Azure Table Storage account

## Configuration

Create an `appsettings.Development.json` file with the following structure:

```json
{
  "GitHub": {
    "Organization": "your-github-org",
    "TemplateRepo": "your-template-repo",
    "App": {
      "ClientId": "github-app-id",
      "PrivateKeyPath": "path-to-private-key-file"
    }
  },
  "Azure": {
    "TenantId": "your-azure-tenant-id",
    "SubscriptionId": "your-azure-subscription-id"
  },
  "Graph": {
    "Authority": "https://login.microsoftonline.com/your-tenant-id",
    "ClientId": "graph-client-id",
    "ClientSecret": "graph-client-secret"
  },
  "Storage": {
    "ConnectionString": "your-storage-connection-string",
    "TableName": "your-table-name"
  }
}
```

For sensitive data, you can use .NET User Secrets instead:

```bash
dotnet user-secrets init
dotnet user-secrets set "GitHub:App:ClientId" "your-github-app-id"
dotnet user-secrets set "GitHub:App:PrivateKeyPath" "c:\path\to\private-key.pem"
dotnet user-secrets set "Graph:ClientSecret" "your-graph-client-secret"
dotnet user-secrets set "Storage:ConnectionString" "your-storage-connection-string"
```

## Running the API

```bash
dotnet build
dotnet run
```

The API will be available at <http://localhost:5264>.

## API Endpoints

- `GET /api/apps` - List all applications
- `GET /api/apps/{appName}` - Get application details
- `POST /api/apps` - Create a new application
- `POST /api/apps/{appName}/register` - Register in Entra ID
- `POST /api/apps/{appName}/secrets` - Add GitHub repository secrets
- `DELETE /api/apps/{appName}` - Delete an application

## Detailed Documentation

For detailed technical documentation, see:

- [API Documentation](../docs/backend/api-documentation.md)
- [Architecture Overview](../docs/diagrams/architecture-overview.md)
