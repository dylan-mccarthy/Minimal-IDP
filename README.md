# Minimal-IDP: Streamlined Cloud Deployment Platform

![Version](https://img.shields.io/badge/version-0.1.0-blue)
![Platform](https://img.shields.io/badge/platform-.NET%209.0%20%7C%20SvelteKit-brightgreen)

## What Is This?

Minimal-IDP is a hassle-free platform that automates the tedious parts of setting up new cloud applications. With just a few clicks, you can:

- Create a GitHub repository from our templates
- Register your app in Azure Entra ID
- Configure all the GitHub secrets needed for deployment

No more juggling between multiple portals and services!

## Why Use It?

Setting up a new cloud application traditionally involves multiple manual steps across different services. Minimal-IDP streamlines this process by:

- **Saving time**: Complete in minutes what normally takes hours
- **Reducing errors**: Automated processes eliminate manual configuration mistakes
- **Standardizing deployments**: All applications follow consistent patterns
- **Simplifying management**: Manage all your apps in one place

## How It Works

[System Overview]

1. **Choose your tech stack** (.NET, Node.js, or Go)
2. **Name your application**
3. **Click through our guided process**
4. **Done!** Your application is ready for development

Behind the scenes, we're handling GitHub repository creation, Azure Entra ID registration, and setting up workload identity federation for secure cloud deployments.

## Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (LTS version)
- GitHub App credentials
- Azure account with app registration permissions

### Quick Start

1. **Clone the repository**

   ```bash
   git clone https://github.com/your-org/Minimal-IDP.git
   cd Minimal-IDP
   ```

2. **Set up the backend**

   ```bash
   cd PlatformAPI
   # Configure your settings (see PlatformAPI/README.md)
   dotnet run
   ```

3. **Set up the frontend**

   ```bash
   cd frontend
   npm install
   npm run dev
   ```

4. **Open** <http://localhost:5173> in your browser

For detailed setup instructions, see our [local development guide](./docs/README.md).

## Components

### Frontend Application (SvelteKit)

A clean, responsive interface for managing your applications with:

- Application dashboard for at-a-glance status
- Step-by-step creation wizard
- Detailed management page for each application

[Frontend documentation](./frontend/README.md)

### Backend API (.NET 9.0)

A RESTful API that handles:

- GitHub repository creation from templates
- Azure Entra ID application registration
- GitHub secrets management for CI/CD
- Application metadata storage

[Backend documentation](./PlatformAPI/README.md)

## Documentation

We've created extensive documentation to help you understand and extend the system:

- [API Documentation](./docs/backend/api-documentation.md) - Detailed API reference
- [Frontend Technical Documentation](./docs/frontend/technical-documentation.md) - Frontend architecture
- [Architecture Diagrams](./docs/diagrams/architecture-overview.md) - System design visualizations

## Use Cases

- **Developer onboarding**: Help new team members deploy applications quickly
- **Microservice creation**: Rapidly deploy new microservices following your standards
- **DevOps standardization**: Ensure all applications use the same deployment patterns
- **Multi-environment setup**: Configure applications across development, testing, and production

## License

This project is licensed under the MIT License - see the LICENSE file for details.
