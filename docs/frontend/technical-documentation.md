# Frontend Technical Documentation

## Overview

The Minimal-IDP frontend is a web application that allows users to create, view, and manage application deployments. It provides a user-friendly interface for generating GitHub repositories, registering applications in Entra ID, and configuring repository secrets for CI/CD pipelines.

## Architecture

The frontend follows a component-based architecture using SvelteKit as the framework. It uses client-side routing to navigate between pages while maintaining a cohesive user experience.

### Technology Stack

- Framework: SvelteKit
- State Management: Svelte stores and component-local state
- UI: Custom CSS for styling with a clean, responsive design
- Build Tools: Vite (bundled with SvelteKit)
- HTTP Client: Native fetch API

## Component Structure

The application is organized into route-based components following SvelteKit's file-system based routing:

```bash
Routes
├── +page.svelte (Application Dashboard/Home)
├── create/
│   └── +page.svelte (Create Application Form)
└── app/
    └── [appName]/
        └── +page.svelte (Application Detail View)
```

## Core Components

### ApplicationDashboard (+page.svelte)

- **Purpose**: Displays a list of all applications with their key properties and statuses
- **State**:
  - `applications`: Application[] - List of all applications
  - `loadingApps`: boolean - Loading state indicator
  - `errorMessage`: string - Error message if applications fail to load
- **Key Functions**:
  - `navigateToCreate()`: Navigates to the application creation page
  - `navigateToAppDetail(appName)`: Navigates to a specific application's detail page
- **Dependencies**: SvelteKit navigation utilities

### CreateApplication (create/+page.svelte)

- **Purpose**: Form interface to create new applications and complete the setup process
- **State**:
  - `appName`: string - Name of the application to create
  - `stack`: string - Technology stack selection (.net, node, go)
  - Various state variables for tracking loading, success, and error states for each step
  - Step completion indicators for the multi-step process
- **Key Functions**:
  - `submitForm()`: Creates a new GitHub repository
  - `registerApp()`: Registers the application in Entra ID
  - `addSecrets()`: Adds GitHub repository secrets for CI/CD
  - `goBack()`: Navigates back to the main dashboard
- **Dependencies**: SvelteKit navigation utilities

### ApplicationDetail (app/[appName]/+page.svelte)

- **Purpose**: Displays detailed information about a specific application and provides management actions
- **State**:
  - `application`: Application - Detailed application data
  - `loading`: boolean - Loading state indicator
  - `errorMessage`: string - Error message if application details fail to load
  - Various state variables for tracking loading, success, and error states for different actions
  - Confirmation dialog state for delete operation
- **Key Functions**:
  - `loadApplicationDetails()`: Fetches application details from the API
  - `recreateAppRegistration()`: Recreates the Entra ID application registration
  - `addRepositorySecrets()`: Updates GitHub repository secrets
  - `deleteApplication()`: Deletes the application
  - `goBack()`: Navigates back to the main dashboard
- **Dependencies**: SvelteKit navigation and page parameter utilities

## Data Models

### Application

```typescript
interface Application {
  rowKey: string;           // Application name (primary key)
  repositoryUrl?: string;   // GitHub repository URL
  isRegistered: boolean;    // Whether the application is registered in Entra ID
  secretsAdded: boolean;    // Whether GitHub secrets have been added
  azureAppClientId?: string; // Azure Application (Client) ID 
  tenantId?: string;        // Azure Tenant ID
  subscriptionId?: string;  // Azure Subscription ID
}
```

## Routing

The application uses SvelteKit's file-system based routing with the following structure:

| Route | Component | Description | Access Control |
|-------|-----------|-------------|----------------|
| `/` | ApplicationDashboard | Shows all applications | Public |
| `/create` | CreateApplication | Form to create new applications | Public |
| `/app/[appName]` | ApplicationDetail | Details and management for a specific application | Public |

## API Integration

The frontend communicates with a .NET backend API hosted at `http://localhost:5264` during development.

### Request/Response Flow

1. The frontend makes HTTP requests to the backend API endpoints
2. Responses are handled with appropriate success and error states
3. Data is processed and displayed in the UI components

## Error Handling

Error handling is implemented at multiple levels:

- API request errors are caught and displayed to the user
- Form validation prevents invalid submissions
- Loading states prevent multiple submissions
- Confirmation dialogs prevent accidental destructive actions

## UI/UX Features

- Step-by-step progress indicators for multi-stage processes
- Status indicators with color coding (green for complete, yellow for pending)
- Confirmation dialogs for destructive actions
- Responsive button states during API operations
- Success and error message feedback

## Known Limitations

- No authentication system currently implemented
- Limited error details from API responses
- No offline support or caching mechanism

## Future Improvements

- Add user authentication and authorization
- Implement real-time updates for application status
- Add pagination for the applications list
- Create a more detailed application monitoring dashboard
- Add support for additional deployment options and cloud providers
