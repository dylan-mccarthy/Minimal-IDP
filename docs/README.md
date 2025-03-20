# Project Documentation

This directory contains comprehensive technical documentation for both the frontend application and backend API.

## Documentation Structure

### Frontend Documentation
- [Technical Documentation](./frontend/technical-documentation.md) - Detailed technical documentation of the frontend application

### Backend Documentation
- [API Documentation](./backend/api-documentation.md) - Comprehensive documentation of the backend API

### Architecture and Design
- [Architecture Diagrams](./diagrams/architecture-overview.md) - System architecture and data flow diagrams

## How to Use This Documentation

1. Start with the architecture diagrams to understand the overall system design
2. Review the frontend and backend documentation based on your specific needs
3. Use the API documentation as a reference when integrating with the backend

## Local Development Environment Setup

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download) or later
- [Node.js](https://nodejs.org/) (LTS version recommended)
- [npm](https://www.npmjs.com/) (comes with Node.js)
- GitHub App registration with a private key for authentication
- Azure account with appropriate permissions to create app registrations
- Azure Table Storage account or the Azure Storage Emulator

### Configuration

#### Backend API Configuration

Create a `appsettings.Development.json` file in the `PlatformAPI` directory with the following structure:

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

#### User Secrets (Optional)

For local development, you can use the .NET user secrets to store sensitive configuration:

```bash
cd PlatformAPI
dotnet user-secrets init
dotnet user-secrets set "GitHub:App:ClientId" "your-github-app-id"
dotnet user-secrets set "GitHub:App:PrivateKeyPath" "c:\path\to\private-key.pem"
dotnet user-secrets set "Graph:ClientSecret" "your-graph-client-secret"
dotnet user-secrets set "Storage:ConnectionString" "your-storage-connection-string"
```

### Running the Applications

#### Start the Backend API

```bash
cd PlatformAPI
dotnet build
dotnet run
```

The API will be available at: http://localhost:5264

#### Start the Frontend Development Server

```bash
cd frontend
npm install
npm run dev
```

The frontend will be available at: http://localhost:5173 (or another port if 5173 is in use)

### Testing the Applications Together

1. Make sure both the backend API and frontend servers are running simultaneously
2. Open a browser and navigate to http://localhost:5173
3. The frontend should connect to the backend API at http://localhost:5264
4. You can now test the full application workflow:
   - Create a new application
   - Register the application in Entra ID
   - Add repository secrets

### Common Issues and Troubleshooting

#### CORS Issues
If you encounter CORS errors in the browser console, ensure the backend API has CORS properly configured. The current implementation includes a "AllowAll" CORS policy for development.

#### GitHub App Authentication
If GitHub API calls fail, verify your GitHub App private key is correctly formatted and accessible at the path specified in the configuration.

#### Azure AD / Graph API Issues
If Azure AD operations fail, check that the Graph API credentials have sufficient permissions (Application.ReadWrite.All) and that the tenant ID is correct.

#### Port Conflicts
If either application fails to start due to port conflicts, you can change the ports:
- For the API: Edit `Properties/launchSettings.json`
- For the frontend: Edit the `vite.config.js` file

## Updating Documentation

Please keep this documentation up-to-date as the codebase evolves. Documentation should be updated whenever:

- New features are added
- APIs change
- Architecture is modified
- Significant refactoring occurs

## Documentation Best Practices

1. Use clear, concise language
2. Include code examples where helpful
3. Keep diagrams simple and focused on key concepts
4. Ensure all API endpoints are properly documented
5. Include error handling information
6. Document assumptions and limitations
