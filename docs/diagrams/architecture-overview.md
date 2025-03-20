# Architecture Diagrams

## System Architecture Overview

The Minimal-IDP system consists of a SvelteKit frontend application, a .NET Core backend API, and integrations with GitHub and Azure services.

```mermaid
graph TD
    A[User Browser] -->|HTTP/HTTPS| B[SvelteKit Frontend]
    B -->|API Calls| C[.NET Core Backend API]
    C -->|Create Repository| D[GitHub API]
    C -->|Register Application| E[Azure Entra ID]
    C -->|Store Data| F[(Azure Table Storage)]
    C -->|Add Secrets| D
```

### Component Descriptions

#### Frontend Application

- **SvelteKit Application**: Provides the user interface for managing applications
- **Key Pages**: Dashboard, Application Creation, Application Details
- **Main Features**: Create repositories, register applications, manage secrets
- **Technologies**: SvelteKit, JavaScript/TypeScript, CSS

#### Backend API

- **.NET Core API**: Processes requests and orchestrates operations with external services
- **Key Services**:
  - `ApplicationStorageService`: Manages application data in Azure Table Storage
  - `GitHubService`: Creates repositories and manages repository secrets
  - `AzureAdService`: Creates Entra ID applications and federated credentials
  - `GitHubAppAuthService`: Handles GitHub App authentication
- **Key Endpoints**: `/api/apps` for CRUD operations on applications
- **Technologies**: ASP.NET Core, C#, Azure SDK, Octokit.NET, Microsoft Graph SDK

#### Database

- **Azure Table Storage**: NoSQL table storage for application metadata
- **Schema**: Simple table with application details including GitHub repository URL, registration status, and Azure identifiers
- **Partition Strategy**: All applications stored with partition key "Application" and row key based on application name

#### External Services

- **GitHub**:
  - Hosts application code repositories
  - Provides GitHub Actions for CI/CD
  - Used for secret storage (encrypted repository secrets)
  - Accessed via GitHub App authentication
- **Azure Entra ID**:
  - Manages application identities for Azure resources
  - Provides OIDC federation for GitHub Actions authentication
  - Enables workload identity for secure cloud deployments
- **Azure Table Storage**:
  - Stores application metadata and status
  - Provides persistence for the application lifecycle

## Data Flow Diagrams

### Application Creation Flow

```mermaid
sequenceDiagram
    actor User
    participant Frontend
    participant Backend
    participant GitHub
    participant EntraID
    participant Storage
    
    User->>Frontend: Enter app details
    Frontend->>Backend: POST /api/apps
    Backend->>GitHub: Create repository from template
    GitHub-->>Backend: Repository created
    Backend->>Storage: Store application metadata
    Storage-->>Backend: Confirm storage
    Backend-->>Frontend: Return repository details
    Frontend-->>User: Display success & next steps
    
    User->>Frontend: Click "Register Application"
    Frontend->>Backend: POST /api/apps/{name}/register
    Backend->>EntraID: Create application registration
    EntraID-->>Backend: Return app credentials
    Backend->>EntraID: Add federated credentials for GitHub Actions
    EntraID-->>Backend: Confirm credentials added
    Backend->>Storage: Update application status
    Storage-->>Backend: Confirm update
    Backend-->>Frontend: Return registration details
    Frontend-->>User: Display credentials & next steps
    
    User->>Frontend: Click "Add Repository Secrets"
    Frontend->>Backend: POST /api/apps/{name}/secrets
    Backend->>GitHub: Get repository public key
    GitHub-->>Backend: Return public key
    Backend->>GitHub: Add encrypted repository secrets
    GitHub-->>Backend: Secrets added
    Backend->>Storage: Update secrets status
    Storage-->>Backend: Confirm update
    Backend-->>Frontend: Return success status
    Frontend-->>User: Display completion status
```

## Entity Relationship Diagram

```mermaid
erDiagram
    APPLICATION {
        string partitionKey
        string rowKey PK
        string appName
        string repositoryUrl
        boolean isRegistered
        boolean secretsAdded
        string azureAppClientId
        string tenantId
        string subscriptionId
        dateTimeOffset createdAt
    }
    
    GITHUB-REPOSITORY {
        string name PK
        string url
        string owner
    }
    
    AZURE-APP-REGISTRATION {
        string clientId PK
        string tenantId
        string displayName
    }
    
    FEDERATED-CREDENTIALS {
        string name
        string issuer
        string subject
        string[] audiences
    }
    
    APPLICATION ||--o| GITHUB-REPOSITORY : "has"
    APPLICATION ||--o| AZURE-APP-REGISTRATION : "has"
    AZURE-APP-REGISTRATION ||--o{ FEDERATED-CREDENTIALS : "has"
```

## Deployment Architecture

The system can be deployed using Azure services for both the frontend and backend components.

```mermaid
graph TD
    A[GitHub Repository] -->|CI/CD| B[Azure Static Web Apps]
    A -->|CI/CD| C[Azure App Service]
    B -->|Hosts| D[SvelteKit Frontend]
    C -->|Hosts| E[.NET Core API]
    E -->|Connects to| F[(Azure Table Storage)]
    E -->|Authenticates with| G[Azure AD]
    E -->|Interacts with| H[GitHub API]
```

## Diagram Creation Guidelines

### Tools for Creating Diagrams

- [Draw.io](https://app.diagrams.net/) (free)
- [Lucidchart](https://www.lucidchart.com/)
- [Mermaid](https://mermaid-js.github.io/mermaid/#/) (for code-based diagrams)
- [PlantUML](https://plantuml.com/)

### Mermaid Example (Component Diagram)

```mermaid
graph TD
    A[Client] -->|HTTP/HTTPS| B[Load Balancer]
    B --> C[Web Server]
    C --> D[Application Server]
    D --> E[(Database)]
    D --> F[Cache]
    D --> G[External API]
```

### PlantUML Example (Sequence Diagram)

```plantuml
@startuml
actor User
participant "Frontend" as FE
participant "API Gateway" as API
participant "Auth Service" as Auth
participant "Database" as DB

User -> FE: Login Request
FE -> API: POST /auth/login
API -> Auth: Validate Credentials
Auth -> DB: Query User
DB --> Auth: Return User Data
Auth --> API: JWT Token
API --> FE: Return Token
FE --> User: Login Success
@enduml
```
