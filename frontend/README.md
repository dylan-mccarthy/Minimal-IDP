# Minimal-IDP Frontend

A SvelteKit application for managing application deployments through a user-friendly interface. This application works with the Minimal-IDP Platform API to create GitHub repositories, register applications in Azure Entra ID, and configure repository secrets.

## Features

- Dashboard view of all applications
- Step-by-step application creation wizard
- Detailed application management page
- Status indicators for deployment progress

## Prerequisites

- [Node.js](https://nodejs.org/) (LTS version recommended)
- [npm](https://www.npmjs.com/) (comes with Node.js)
- Backend API running locally or at a configured endpoint

## Setup

Install dependencies:

```bash
npm install
```

Configure the API endpoint (if different from default):

The application is configured to use `http://localhost:5264` as the API endpoint. If you need to change this, update the fetch URLs in the following files:

- `src/routes/+page.svelte`
- `src/routes/create/+page.svelte`
- `src/routes/app/[appName]/+page.svelte`

## Running the Application

Start the development server:

```bash
npm run dev
```

The frontend will be available at <http://localhost:5173>.

## Application Structure

- `/` - Applications dashboard
- `/create` - Create new application
- `/app/[appName]` - Application details and management

## Building for Production

```bash
npm run build
```

## Detailed Documentation

For detailed technical documentation, see:

- [Frontend Documentation](../docs/frontend/technical-documentation.md)
- [Architecture Overview](../docs/diagrams/architecture-overview.md)
