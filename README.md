# Shelton Tool-Hire Review Portal API

Backend API for the Shelton Tool-Hire Review Portal. This repository contains the ASP.NET Core Web API, Clean Architecture application layers, EF Core migrations, SQL scripts, automated tests, GitHub Actions workflows, Azure deployment configuration, and supporting project documentation.

## Current Implementation Snapshot

- Platform: ASP.NET Core Web API on .NET 8
- Architecture: Clean Architecture with Domain, Application, Infrastructure, and API layers
- Database: SQL Server or Azure SQL with EF Core code-first migrations
- Authentication: Custom JWT bearer authentication with ASP.NET Core password hashing
- Authorization: Role-based access for Customer, Moderator, and Admin users
- Image storage: Azure Blob Storage for uploaded tool/service images
- Observability: Optional Azure Application Insights telemetry and logging
- API docs: Swagger UI and OpenAPI JSON, including multipart upload endpoints
- Testing: xUnit unit tests and Windows LocalDB integration tests
- CI/CD: GitHub Actions runs unit and integration tests on `development` and `main`; Azure App Service deployment is restricted to `main`
- Dependency updates: Dependabot opens NuGet and GitHub Actions PRs into `development`

## Repository Layout

```text
ReviewPortal-API/
|-- src/
|   |-- ReviewPortal.Domain/           # Entities, enums, and domain contracts
|   |-- ReviewPortal.Application/      # DTOs, services, validators, and use-case logic
|   |-- ReviewPortal.Infrastructure/   # EF Core, repositories, authentication, Blob storage, migrations
|   `-- ReviewPortal.API/              # Controllers, middleware, DI, Swagger, hosting configuration
|-- tests/
|   |-- ReviewPortal.UnitTests/        # Fast unit and controller tests
|   `-- ReviewPortal.IntegrationTests/ # WebApplicationFactory and SQL Server LocalDB tests
|-- docs/                              # Architecture, deployment, testing, security, and agile documents
|-- scripts/
|   |-- local/                         # Local run and Azure SQL migration helpers
|   |-- security/                      # Secret scan helper
|   `-- sql/                           # Checked-in migration and seed scripts
|-- .github/
|   |-- dependabot.yml                 # Weekly dependency update configuration
|   `-- workflows/
|       |-- ci.yml                     # Unit tests, integration tests, main-only publish/deploy
|       `-- trigger-qa-automation.yml  # Triggers Playwright QA after successful main workflow
|-- AGENTS.md
|-- CLAUDE.md
|-- README.md
`-- ReviewPortal.slnx
```

## Architecture

The solution follows a four-layer Clean Architecture structure:

```text
Domain <- Application <- Infrastructure
                     <- API
```

Key rules:

- `ReviewPortal.Domain` has no dependency on outer layers.
- `ReviewPortal.Application` depends only on the domain layer.
- `ReviewPortal.Infrastructure` implements persistence, JWT generation, repositories, and Azure Blob image storage.
- `ReviewPortal.API` wires dependency injection, authentication, authorization, Swagger, Application Insights, controllers, and HTTP middleware.

## Implemented API Features

- Public category and tool catalogue browsing
- Tool search, tool details, and rental cost calculation
- Customer registration, login, current-user lookup, password change, forgot password, and password reset
- JWT-secured customer review submission and personal review history
- Review comments and company responses
- Moderator/Admin review and comment moderation
- Admin category create, update, and delete operations
- Admin tool create, update, status update, image upload, and image delete operations
- Admin dashboard statistics
- Uploaded image validation, Azure Blob upload/delete, and persisted public Blob image URLs
- Swagger/OpenAPI document generation for local and deployed inspection
- Health endpoint for smoke checks

## Main API Endpoints

| Area | Endpoint examples |
|------|-------------------|
| Auth | `POST /api/auth/register`, `POST /api/auth/login`, `GET /api/auth/me`, `POST /api/auth/change-password` |
| Categories | `GET /api/categories`, `GET /api/categories/featured`, `GET /api/categories/{id}`, `GET /api/categories/{id}/tools` |
| Tools | `GET /api/tools/search`, `GET /api/tools/{id}`, `POST /api/tools/{id}/rental-calculation` |
| Reviews | `GET /api/tools/{toolId}/reviews`, `POST /api/tools/{toolId}/reviews`, `GET /api/users/me/reviews` |
| Comments | `GET /api/reviews/{reviewId}/comments`, `POST /api/reviews/{reviewId}/comments` |
| Responses | `POST /api/reviews/{reviewId}/response`, `PUT /api/reviews/{reviewId}/response`, `DELETE /api/reviews/{reviewId}/response` |
| Admin tools | `GET /api/admin/tools`, `POST /api/admin/tools`, `PUT /api/admin/tools/{id}`, `PATCH /api/admin/tools/{id}/status`, `POST /api/admin/tools/{id}/images` |
| Admin categories | `POST /api/admin/categories`, `PUT /api/admin/categories/{id}`, `DELETE /api/admin/categories/{id}` |
| Admin moderation | `GET /api/admin/moderation/pending`, `PUT /api/admin/moderation/reviews/{id}`, `PUT /api/admin/moderation/comments/{id}` |
| Admin dashboard | `GET /api/admin/dashboard/stats` |
| Diagnostics | `GET /health`, `/swagger`, `/swagger/v1/swagger.json` |

## Getting Started

Run all commands from the repository root.

### Prerequisites

- .NET SDK 8
- SQL Server, SQL Server LocalDB, or Azure SQL Database
- Azure Blob Storage container for uploaded tool/service images
- Optional Azure Application Insights resource
- Optional Azure CLI or Visual Studio sign-in for passwordless Blob Storage access with `DefaultAzureCredential`

### 1. Configure Local Secrets

Secrets are not stored in source control. Use .NET user secrets for local development:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your-sql-connection-string>" --project src/ReviewPortal.API
dotnet user-secrets set "Jwt:Secret" "<your-32-plus-character-secret>" --project src/ReviewPortal.API
dotnet user-secrets set "Jwt:Issuer" "ReviewPortalAPI" --project src/ReviewPortal.API
dotnet user-secrets set "Jwt:Audience" "ReviewPortalClient" --project src/ReviewPortal.API
dotnet user-secrets set "Jwt:ExpiryMinutes" "60" --project src/ReviewPortal.API
```

Configure Azure Blob Storage with managed identity or a local development connection string:

```powershell
dotnet user-secrets set "ImageStorage:ServiceUri" "https://<storage-account-name>.blob.core.windows.net" --project src/ReviewPortal.API
dotnet user-secrets set "ImageStorage:ContainerName" "tool-images" --project src/ReviewPortal.API
dotnet user-secrets set "ImageStorage:PublicBaseUrl" "https://<storage-account-name>.blob.core.windows.net/tool-images" --project src/ReviewPortal.API
dotnet user-secrets set "ImageStorage:BlobNamePrefix" "tools" --project src/ReviewPortal.API
```

If you are not using Azure identity locally, use this fallback instead of `ImageStorage:ServiceUri`:

```powershell
dotnet user-secrets set "ImageStorage:ConnectionString" "<your-storage-connection-string>" --project src/ReviewPortal.API
```

Application Insights is optional locally and enabled only when a connection string is present:

```powershell
dotnet user-secrets set "ApplicationInsights:ConnectionString" "<your-application-insights-connection-string>" --project src/ReviewPortal.API
```

Do not commit real SQL passwords, JWT secrets, storage keys, publish profiles, or Application Insights connection strings.

### 2. Optional Local Settings File

For Azure SQL migration smoke tests or local API runs against Azure resources, copy the ignored local settings template:

```powershell
Copy-Item src/ReviewPortal.API/appsettings.Local.example.json src/ReviewPortal.API/appsettings.Local.json
notepad src/ReviewPortal.API/appsettings.Local.json
```

`appsettings.Local.json` is ignored by git and loads after checked-in settings files.

### 3. Restore And Build

```powershell
dotnet restore ReviewPortal.slnx
dotnet build ReviewPortal.slnx
```

### 4. Apply Database Migrations

```powershell
dotnet ef database update --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API
```

To apply migrations to Azure SQL using `appsettings.Local.json`:

```powershell
.\scripts\local\Update-AzureDatabase.ps1
```

### 5. Optional Seed Data

The EF migration applies the core schema and seeded identity/reference data. Extra catalogue and relational demo data scripts are in `scripts/sql/`.

```powershell
sqlcmd -S "<server>" -d "<database>" -U "<username>" -P "<password>" -i "scripts/sql/SeedTestUsers.sql"
sqlcmd -S "<server>" -d "<database>" -U "<username>" -P "<password>" -i "scripts/sql/SeedFullTestData.sql"
```

### 6. Run The API

```powershell
dotnet run --project src/ReviewPortal.API
```

To run using the ignored local settings file:

```powershell
.\scripts\local\Run-ApiLocal.ps1
```

Useful development endpoints:

- `/swagger`
- `/swagger/v1/swagger.json`
- `/health`

## Application Insights Logging

Application Insights support is implemented in `src/ReviewPortal.API/Program.cs` and uses `Microsoft.ApplicationInsights.AspNetCore`.

- Local setting: `ApplicationInsights:ConnectionString`
- Azure App Service setting: `APPLICATIONINSIGHTS_CONNECTION_STRING`
- Checked-in appsettings files contain blank placeholders only.
- When the setting is missing, the API starts normally and logs that telemetry is disabled.
- When the setting is present, request telemetry, dependency telemetry, exceptions, and configured logs are sent to the Application Insights resource.

In Azure Portal, add the connection string under:

`reviewportal-api -> Settings -> Environment variables`

Then restart the App Service and check Application Insights `Logs`, `Failures`, `Performance`, and `Live Metrics`.

## Azure Blob Image Storage

Tool and service images are stored in Azure Blob Storage instead of the App Service filesystem.

Required production settings:

```text
ImageStorage__ServiceUri = https://<storage-account-name>.blob.core.windows.net
ImageStorage__ContainerName = tool-images
ImageStorage__PublicBaseUrl = https://<storage-account-name>.blob.core.windows.net/tool-images
ImageStorage__BlobNamePrefix = tools
```

Recommended Azure permission:

- Assign the App Service managed identity the `Storage Blob Data Contributor` role on the storage account.

Use `ImageStorage__ConnectionString` only as a local development fallback. Full setup steps are documented in `docs/azure-blob-storage/README.md`.

## Testing

Run all tests:

```powershell
dotnet test ReviewPortal.slnx
```

Run the suites separately:

```powershell
dotnet test tests/ReviewPortal.UnitTests/ReviewPortal.UnitTests.csproj
dotnet test tests/ReviewPortal.IntegrationTests/ReviewPortal.IntegrationTests.csproj
```

Notes:

- Unit tests are fast and do not require SQL Server.
- Integration tests use `WebApplicationFactory<Program>` and SQL Server LocalDB, so CI runs them on `windows-latest`.
- Integration tests replace Azure Blob access with an in-memory test Blob storage double.
- Swagger JSON is covered by an integration test so `/swagger/v1/swagger.json` does not regress to a 500 error.

## CI/CD And Branch Flow

The main workflow is `.github/workflows/ci.yml`.

| Trigger | Branch | What happens |
|---------|--------|--------------|
| Pull request | `development` | Unit tests and integration tests run |
| Pull request | `main` | Unit tests and integration tests run |
| Push | `development` | Unit tests and integration tests run only |
| Push | `main` | Unit tests and integration tests run, API is published, then deployment waits for `production` approval |

Important deployment rule:

- The `development` branch never publishes or deploys to Azure App Service.
- Azure App Service deployment happens only after changes reach `main`, tests pass, and the `production` GitHub environment approves the deploy job.

The deploy job uses:

- Azure App Service name: `reviewportal-api`
- Production URL: `https://reviewportal-api-escdb3f2epg8eeha.southeastasia-01.azurewebsites.net`
- GitHub environment: `production`
- Required secret: `AZURE_WEBAPP_PUBLISH_PROFILE`

The QA trigger workflow is `.github/workflows/trigger-qa-automation.yml`. It dispatches the Playwright QA automation repository only after a successful `main` workflow, or when manually dispatched from `main`.

Detailed setup is in `docs/DEPLOYMENT-TO-AZURE-APP-SERVICE.md`.

## Required Azure App Service Settings

Set these in Azure Portal under `reviewportal-api -> Settings -> Environment variables`:

```text
ASPNETCORE_ENVIRONMENT = Production
Jwt__Secret = <strong-random-secret>
Jwt__Issuer = ReviewPortalAPI
Jwt__Audience = ReviewPortalClient
Jwt__ExpiryMinutes = 60
APPLICATIONINSIGHTS_CONNECTION_STRING = <connection-string-from-application-insights>
ImageStorage__ServiceUri = https://<storage-account-name>.blob.core.windows.net
ImageStorage__ContainerName = tool-images
ImageStorage__PublicBaseUrl = https://<storage-account-name>.blob.core.windows.net/tool-images
ImageStorage__BlobNamePrefix = tools
ConnectionStrings__DefaultConnection = Server=tcp:<azure-sql-server>.database.windows.net,1433;Initial Catalog=<database-name>;User ID=<azure-sql-user>;Password=<rotated-password>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

Optional frontend CORS settings:

```text
Cors__AllowedOrigins__0 = https://reviewportal-frontend-dccvarataff4a8hg.southeastasia-01.azurewebsites.net
Cors__AllowedOrigins__1 = http://localhost:3000
```

Restart the App Service after changing settings.

## GitHub Settings Needed For Deployment

In the API repository:

1. Create a GitHub environment named `production`.
2. Enable required reviewers for the `production` environment.
3. Restrict the environment to branch `main`.
4. Add the `AZURE_WEBAPP_PUBLISH_PROFILE` secret to the `production` environment.
5. Add `QA_AUTOMATION_TRIGGER_TOKEN` as an Actions secret if the API workflow should trigger the Playwright QA repository.

Recommended release path:

1. Work on a feature branch.
2. Open a PR into `development`.
3. Merge only after unit and integration tests pass.
4. Open a PR from `development` to `main`.
5. Merge to `main` only after approval and passing checks.
6. Approve the `production` deployment job.
7. Confirm Azure App Service health and Playwright QA results.

## Dependabot

Dependabot is configured in `.github/dependabot.yml`.

- NuGet package updates are checked weekly for the API, Application, Infrastructure, UnitTests, and IntegrationTests projects.
- GitHub Actions updates are checked weekly.
- PRs target the `development` branch.
- Minor and patch updates are grouped to reduce PR noise.

## Seeded Test Users

| Role | Email | Password |
|------|-------|----------|
| Customer | `customer.test@reviewportal.local` | `Customer123!` |
| Admin | `admin.test@reviewportal.local` | `Admin123!` |
| Moderator | `moderator.test@reviewportal.local` | `Moderator123!` |

## Common Commands

```powershell
dotnet restore ReviewPortal.slnx
dotnet build ReviewPortal.slnx
dotnet test ReviewPortal.slnx
dotnet test tests/ReviewPortal.UnitTests/ReviewPortal.UnitTests.csproj
dotnet test tests/ReviewPortal.IntegrationTests/ReviewPortal.IntegrationTests.csproj
dotnet run --project src/ReviewPortal.API
.\scripts\local\Run-ApiLocal.ps1
.\scripts\local\Update-AzureDatabase.ps1
.\scripts\security\scan-secrets.ps1
dotnet ef migrations add <MigrationName> --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API
dotnet ef migrations script <FromMigration> <ToMigration> --idempotent --output scripts/sql/<MigrationName>.sql --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API
dotnet ef database update --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API
```

## Security Checklist Before PR

Run the local secret scan before opening a PR:

```powershell
.\scripts\security\scan-secrets.ps1
```

Check that no real values are committed for:

- Azure SQL passwords
- JWT secrets
- Azure publish profiles
- Storage account keys or connection strings
- Application Insights connection strings
- GitHub tokens

## Key Documentation

| Document | Path |
|----------|------|
| Architecture and coding conventions | `CLAUDE.md` |
| AI agent working instructions | `AGENTS.md` |
| Azure App Service deployment guide | `docs/DEPLOYMENT-TO-AZURE-APP-SERVICE.md` |
| Azure Blob Storage setup | `docs/azure-blob-storage/README.md` |
| Functional design diagrams | `docs/FUNCTIONAL-DESIGN-DIAGRAMS.md` |
| Database design | `docs/DATABASE-DESIGN.md` |
| Entity relationship diagram | `docs/ERD.md` |
| Requirements specification | `docs/REQUIREMENTS-SPECIFICATION.md` |
| Non-functional requirements | `docs/NON-FUNCTIONAL-REQUIREMENTS.md` |
| Testing strategy | `docs/TESTING-STRATEGY.md` |
| Test plan | `docs/TEST-PLAN.md` |
| Security scan notes | `docs/security/BACKEND-SECURITY-SCAN-2026-05-07.md` |
| Agile backlog and sprint artefacts | `docs/agile/` |
| SQL script usage notes | `scripts/sql/README.md` |

## Notes For Assessors

- This repository is the backend API package for the Review Portal.
- Some planning documents still describe the wider full-stack product because they capture the original project context.
- The active code, tests, workflows, deployment setup, and README instructions are backend API specific.
