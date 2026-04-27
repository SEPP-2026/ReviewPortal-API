# Shelton Tool-Hire Review Portal API

Backend-only MSc project submission for the Shelton Tool-Hire Review Portal. This repository contains the ASP.NET Core Web API, EF Core migrations, SQL scripts, automated tests, and the supporting academic documentation for the backend deliverable.

## Submission Snapshot

- Platform: ASP.NET Core Web API on .NET 8
- Architecture: Clean Architecture
- Database: SQL Server with EF Core code-first migrations
- Authentication: Custom JWT bearer auth with ASP.NET Core password hashing
- Testing: xUnit, FluentAssertions, and Bogus
- Scope: catalogue browsing, rental cost calculation, review workflows, comments, company responses, and role-based authentication

## Repository Layout

```text
ReviewPortal-API/
|-- src/
|   |-- ReviewPortal.Domain/           # Entities, enums, and domain interfaces
|   |-- ReviewPortal.Application/      # DTOs, services, validators, and use-case logic
|   |-- ReviewPortal.Infrastructure/   # EF Core, repositories, authentication, migrations
|   `-- ReviewPortal.API/              # Controllers, middleware, DI, and startup
|-- tests/
|   |-- ReviewPortal.UnitTests/        # Fast unit and controller tests
|   `-- ReviewPortal.IntegrationTests/ # Infrastructure and authentication integration tests
|-- docs/                              # Requirements, design diagrams, ERD, testing strategy, agile artefacts
|-- scripts/
|   `-- sql/                           # Checked-in migration and seed scripts
|-- .github/workflows/ci.yml           # GitHub Actions build and test pipeline
|-- AGENTS.md                          # AI agent working instructions
|-- CLAUDE.md                          # Architecture and coding conventions
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
- `ReviewPortal.Infrastructure` implements persistence and external concerns.
- `ReviewPortal.API` wires up dependency injection and HTTP endpoints.

## Implemented Backend Features

- Category and tool catalogue endpoints
- Tool detail retrieval with pricing information
- Rental cost calculation support
- Review submission, moderation, and user review history
- Review comments and company responses
- JWT-based registration, login, password change, forgot password, and reset password flows
- Seeded reference data and seeded test users for local demonstration

## Getting Started

Run all commands from the repository root.

### Prerequisites

- .NET SDK 8
- SQL Server or Azure SQL Database
- A valid SQL connection string

### 1. Configure local secrets

This project does not keep secrets in source control. Use .NET user secrets for local development:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your-sql-connection-string>" --project src/ReviewPortal.API
dotnet user-secrets set "Jwt:Secret" "<your-32-plus-character-secret>" --project src/ReviewPortal.API
dotnet user-secrets set "Jwt:Issuer" "ReviewPortalAPI" --project src/ReviewPortal.API
dotnet user-secrets set "Jwt:Audience" "ReviewPortalClient" --project src/ReviewPortal.API
```

### 2. Restore and build

```powershell
dotnet restore ReviewPortal.slnx
dotnet build ReviewPortal.slnx
```

### 3. Apply the database schema

```powershell
dotnet ef database update --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API
```

### 4. Optional: seed full demonstration data

The migration applies the core schema and seeded users automatically. Additional SQL scripts for catalogue and relational demo data live in [`scripts/sql/`](scripts/sql/).

```powershell
sqlcmd -S "<server>" -d "<database>" -U "<username>" -P "<password>" -i "scripts/sql/SeedTestUsers.sql"
sqlcmd -S "<server>" -d "<database>" -U "<username>" -P "<password>" -i "scripts/sql/SeedFullTestData.sql"
```

### 5. Run the API

```powershell
dotnet run --project src/ReviewPortal.API
```

Useful endpoints during development:

- `/swagger`
- `/health`

### 6. Run the automated tests

```powershell
dotnet test ReviewPortal.slnx
```

## Seeded Test Users

| Role | Email | Password |
|------|-------|----------|
| Customer | `customer.test@reviewportal.local` | `Customer123!` |
| Admin | `admin.test@reviewportal.local` | `Admin123!` |
| Moderator | `moderator.test@reviewportal.local` | `Moderator123!` |

## Common Commands

```powershell
dotnet build ReviewPortal.slnx
dotnet test ReviewPortal.slnx
dotnet run --project src/ReviewPortal.API
dotnet ef migrations add <MigrationName> --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API
dotnet ef migrations script <FromMigration> <ToMigration> --idempotent --output scripts/sql/<MigrationName>.sql --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API
dotnet ef database update --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API
```

## Key Documentation

| Document | Path |
|----------|------|
| Architecture and coding conventions | [`CLAUDE.md`](CLAUDE.md) |
| AI agent working instructions | [`AGENTS.md`](AGENTS.md) |
| Functional design diagrams | [`docs/FUNCTIONAL-DESIGN-DIAGRAMS.md`](docs/FUNCTIONAL-DESIGN-DIAGRAMS.md) |
| Database design | [`docs/DATABASE-DESIGN.md`](docs/DATABASE-DESIGN.md) |
| Entity relationship diagram | [`docs/ERD.md`](docs/ERD.md) |
| Requirements specification | [`docs/REQUIREMENTS-SPECIFICATION.md`](docs/REQUIREMENTS-SPECIFICATION.md) |
| Non-functional requirements | [`docs/NON-FUNCTIONAL-REQUIREMENTS.md`](docs/NON-FUNCTIONAL-REQUIREMENTS.md) |
| Testing strategy | [`docs/TESTING-STRATEGY.md`](docs/TESTING-STRATEGY.md) |
| Test plan | [`docs/TEST-PLAN.md`](docs/TEST-PLAN.md) |
| Azure deployment guide | [`docs/DEPLOYMENT-TO-AZURE-APP-SERVICE.md`](docs/DEPLOYMENT-TO-AZURE-APP-SERVICE.md) |
| Agile backlog and sprint artefacts | [`docs/agile/`](docs/agile/) |
| SQL script usage notes | [`scripts/sql/README.md`](scripts/sql/README.md) |

## Notes For Assessors

- This repository now contains only the backend submission package.
- Some agile documents still describe the wider full-stack product vision because they capture the original project planning context.
- The active code, tests, pipelines, and setup instructions in this repository are backend-specific and self-contained.
