# Backend API Documentation

## Overview
The Minimal-IDP backend API provides services for creating, registering, and managing applications. It handles GitHub repository creation, Entra ID application registration, and repository secret management through a RESTful API built with ASP.NET Core.

## Architecture
The backend is built with ASP.NET Core and integrates with Azure and GitHub services to provide a streamlined application deployment process. It uses a service-based architecture where each major functionality is encapsulated in dedicated service classes.

### Technology Stack
- Language/Framework: ASP.NET Core (.NET 9.0)
- Database: Azure Table Storage (12.10.0)
- External Services:
  - GitHub API (via Octokit 14.0.0)
  - Microsoft Graph API (5.74.0)
  - Azure Identity (1.13.2)
- Authentication Libraries:
  - Microsoft.Identity.Client (4.70.0)
  - System.IdentityModel.Tokens.Jwt (8.6.1)
- Encryption: Sodium.Core (1.3.5)
- API Documentation: Microsoft.AspNetCore.OpenApi (9.0.0)

### Package References
```xml
<PackageReference Include="Azure.Data.Tables" Version="12.10.0" />
<PackageReference Include="Azure.Identity" Version="1.13.2" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.0" />
<PackageReference Include="Microsoft.Graph" Version="5.74.0" />
<PackageReference Include="Microsoft.Identity.Client" Version="4.70.0" />
<PackageReference Include="Octokit" Version="14.0.0" />
<PackageReference Include="Sodium.Core" Version="1.3.5" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.6.1" />
```

## Core Services

### ApplicationStorageService
Handles all interactions with Azure Table Storage for storing application metadata.

**Key Methods:**
- `AddApplicationAsync`: Creates a new application record
- `UpdateApplicationAsync`: Updates an existing application
- `GetApplicationAsync`: Retrieves a specific application by name
- `GetApplicationsAsync`: Lists all applications
- `DeleteApplicationAsync`: Removes an application

### GitHubAppAuthService
Manages GitHub App authentication using JWT tokens and installation tokens.

**Key Methods:**
- `GetInstallationTokenAsync`: Obtains a GitHub App installation token
- `CreateGitHubClientAsync`: Creates an authenticated GitHub client

### GitHubService
Manages GitHub repository operations including creating repositories from templates and setting repository secrets.

**Key Methods:**
- `CreateRepositoryFromTemplateAsync`: Creates a new repository using a template
- `SetRepoSecretAsync`: Sets repository secrets encrypted with GitHub's public key
- `GetRepoPublicKeyAsync`: Retrieves the public key for secret encryption

### AzureAdService
Manages Azure Active Directory (Entra ID) operations including application registration and federated credentials.

**Key Methods:**
- `CreateAzureAdAppAsync`: Creates a new application in Entra ID
- `AddFederatedCredentialsAsync`: Adds OIDC federated credentials for GitHub Actions
- `DeleteAzureAdAppAsync`: Deletes an application from Entra ID

## API Design
The API follows RESTful design principles with resource-based endpoints and HTTP verbs. All operations are performed on the `apps` resource.

## Base URL
```
Development: http://localhost:5264
Production: [Production URL to be configured]
```

## Configuration
The API requires the following configuration settings:

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

## API Endpoints

### Resource: Applications

#### GET /api/apps
Retrieves a list of all applications.

**Response:**
```json
[
  {
    "rowKey": "app-name",
    "repositoryUrl": "https://github.com/org/app-name",
    "isRegistered": true,
    "secretsAdded": true,
    "azureAppClientId": "12345678-1234-1234-1234-123456789012",
    "tenantId": "12345678-1234-1234-1234-123456789012",
    "subscriptionId": "12345678-1234-1234-1234-123456789012"
  }
]
```

**Status Codes:**
- `200 OK`: Success
- `500 Internal Server Error`: Server error

#### GET /api/apps/{appName}
Retrieves details for a specific application.

**Path Parameters:**
- `appName`: The application name

**Response:**
```json
{
  "rowKey": "app-name",
  "repositoryUrl": "https://github.com/org/app-name",
  "isRegistered": true,
  "secretsAdded": true,
  "azureAppClientId": "12345678-1234-1234-1234-123456789012",
  "tenantId": "12345678-1234-1234-1234-123456789012",
  "subscriptionId": "12345678-1234-1234-1234-123456789012"
}
```

**Status Codes:**
- `200 OK`: Success
- `404 Not Found`: Application not found
- `500 Internal Server Error`: Server error

#### POST /api/apps
Creates a new application by generating a GitHub repository from a template.

**Request:**
```json
{
  "AppName": "new-app-name",
  "Stack": ".net" 
}
```

**Response:**
```json
{
  "repositoryUrl": "https://github.com/org/new-app-name",
  "status": "created"
}
```

**Status Codes:**
- `200 OK`: Success
- `400 Bad Request`: Invalid request parameters
- `500 Internal Server Error`: Server error

**Implementation Details:**
1. Validates and sanitizes the application name
2. Creates a new GitHub repository from the template 
3. Stores the application information in Azure Table Storage

#### POST /api/apps/{appName}/register
Registers an application in Entra ID and creates federated OIDC credentials for GitHub Actions.

**Path Parameters:**
- `appName`: The application name

**Request:**
```json
{
  "AppName": "app-name",
  "Stack": ".net"
}
```

**Response:**
```json
{
  "appId": "12345678-1234-1234-1234-123456789012",
  "tenantId": "12345678-1234-1234-1234-123456789012",
  "subscriptionId": "12345678-1234-1234-1234-123456789012",
  "clientId": "12345678-1234-1234-1234-123456789012"
}
```

**Status Codes:**
- `200 OK`: Success
- `400 Bad Request`: Invalid request parameters
- `404 Not Found`: Application not found
- `500 Internal Server Error`: Server error

**Implementation Details:**
1. Creates a new application in Entra ID
2. Adds federated OIDC credentials for GitHub Actions workflow
3. Updates the application record with Entra ID details

#### POST /api/apps/{appName}/secrets
Adds GitHub repository secrets for Azure deployment using workload identity federation.

**Path Parameters:**
- `appName`: The application name

**Request:**
```json
{
  "AppName": "app-name",
  "TenantId": "12345678-1234-1234-1234-123456789012",
  "SubscriptionId": "12345678-1234-1234-1234-123456789012",
  "ClientId": "12345678-1234-1234-1234-123456789012"
}
```

**Response:**
```json
{
  "status": "secrets added"
}
```

**Status Codes:**
- `200 OK`: Success
- `400 Bad Request`: Invalid request parameters
- `404 Not Found`: Application or repository not found
- `500 Internal Server Error`: Server error

**Implementation Details:**
1. Retrieves the repository's public key for secret encryption
2. Encrypts each secret value with the repository's public key using libsodium
3. Adds the encrypted secrets to the GitHub repository
4. Updates the application record to indicate secrets have been added

#### DELETE /api/apps/{appName}
Deletes an application from the system, including its Entra ID registration.

**Path Parameters:**
- `appName`: The application name

**Response:**
```json
{
  "status": "deleted",
  "message": "Application 'app-name' successfully deleted"
}
```

**Status Codes:**
- `200 OK`: Success
- `404 Not Found`: Application not found
- `400 Bad Request`: Error during deletion
- `500 Internal Server Error`: Server error

**Implementation Details:**
1. Retrieves the application details from storage
2. If an Azure application ID exists, deletes the application from Entra ID
3. Removes the application record from Azure Table Storage
4. Note: This does not delete the GitHub repository

## Data Models

### ApplicationEntity
Represents an application in the Azure Table Storage.

```csharp
public class ApplicationEntity : ITableEntity
{
    public string PartitionKey { get; set; }  // Always "Application"
    public string RowKey { get; set; }        // Application name (primary key)
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string AppName { get; set; }       // Application name
    public string RepositoryUrl { get; set; } // GitHub repository URL
    public string AzureAppClientId { get; set; } // Entra ID application client ID
    public string TenantId { get; set; }      // Azure Tenant ID
    public string SubscriptionId { get; set; } // Azure Subscription ID
    public DateTimeOffset CreatedAt { get; set; } // Creation timestamp
    public bool IsRegistered { get; set; }    // Whether registered in Entra ID
    public bool SecretsAdded { get; set; }    // Whether GitHub secrets added
}
```

### Request/Response DTOs

```csharp
// Application creation
public record AppCreationRequest(string AppName, string Stack);
public record AppCreationResponse(string RepositoryUrl, string Status);

// Application registration
public record AppRegistrationRequest(string AppName, string repositoryUrl);
public record AppRegistrationResponse(string AppId, string TenantId, string SubscriptionId, string ClientId);

// Add secrets
public record AppAddSecretsRequest(string AppName, string TenantId, string SubscriptionId, string ClientId);
public record AppAddSecretsResponse(string Status);
```

## Error Handling

### Error Response Format
```json
{
  "error": "Error message describing what went wrong"
}
```

The API returns appropriate HTTP status codes along with error messages for failed operations:
- 400 Bad Request: When the request is invalid
- 404 Not Found: When a resource doesn't exist
- 500 Internal Server Error: For unexpected server-side errors

## Security Considerations
- The API currently does not implement user authentication
- Service authentication is handled via:
  - GitHub App installation token for GitHub API access
  - Client credentials flow for Microsoft Graph API access
- All secrets are securely stored:
  - GitHub repository secrets are encrypted with the repository's public key
  - Azure credentials are stored in Azure configuration

## Known Limitations
- No built-in rate limiting
- Limited validation on application names
- No user-level authentication or authorization
- No audit logging for sensitive operations

## Future Improvements
- Add user authentication and authorization
- Implement more detailed error responses with error codes
- Add support for different repository templates based on stack selection
- Add endpoints for application deployment status monitoring
- Implement webhook receivers for GitHub and Azure events
- Add API versioning
