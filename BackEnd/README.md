# Shelton Tool-Hire Review Portal - Back End

ASP.NET Core Web API (.NET 8) built with Clean Architecture.

This project is part of a monorepo. The front-end client lives in the sibling [`../FrontEnd/`](../FrontEnd/) directory.

## Solution Structure

```text
BackEnd/
|-- src/
|   |-- ReviewPortal.Domain/           # Entities, enums, domain interfaces
|   |-- ReviewPortal.Application/      # DTOs, services, application interfaces
|   |-- ReviewPortal.Infrastructure/   # EF Core, repositories, auth, migrations
|   `-- ReviewPortal.API/              # Controllers, middleware, DI, startup
|-- tests/
|   |-- ReviewPortal.UnitTests/        # Unit tests
|   `-- ReviewPortal.IntegrationTests/ # Integration tests
|-- scripts/
|   `-- sql/                           # Generated migration and seed scripts
|-- docs/                              # Requirements, ERD, agile artefacts
|-- ReviewPortal.slnx
|-- CLAUDE.md
`-- AGENTS.md
```

## Prerequisites

- .NET SDK 8
- SQL Server or Azure SQL Database
- A valid SQL connection string

Check your installed SDK:

```powershell
dotnet --info
```

## Backend Setup

Run all commands from the `BackEnd/` directory.

### 1. Configure local secrets

This project does not store secrets in source-controlled `appsettings.json`.
Use .NET user-secrets for local development:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your-sql-connection-string>" --project src/ReviewPortal.API
dotnet user-secrets set "Jwt:Secret" "<your-32-plus-character-secret>" --project src/ReviewPortal.API
dotnet user-secrets set "Jwt:Issuer" "ReviewPortalAPI" --project src/ReviewPortal.API
dotnet user-secrets set "Jwt:Audience" "ReviewPortalClient" --project src/ReviewPortal.API
```

Notes:

- `Jwt:Secret` should be at least 32 characters long
- do not commit local secrets to Git
- the API will fail at startup if `DefaultConnection` or `Jwt:Secret` is missing

### 2. Restore and build

```powershell
dotnet build ReviewPortal.slnx
```

### 3. Create or update the database

Apply the EF Core migration:

```powershell
dotnet ef database update --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API
```

This creates the schema and inserts the seeded test users automatically.
Run the full seed script afterwards if you want populated catalogue, review, comment, and response data across every table.

### 4. Run the API

```powershell
dotnet run --project src/ReviewPortal.API
```

Useful development endpoint:

```text
/health
```

### 5. Run tests

```powershell
dotnet test ReviewPortal.slnx
```

## Authentication Setup

The current backend supports:

- user registration with name, email, and password
- login returning a JWT token
- authenticated `/api/auth/me`
- role claims for `Customer`, `Admin`, and `Moderator`

Password rules:

- minimum 8 characters
- at least one uppercase letter
- at least one number

Passwords are stored using ASP.NET Identity password hashing.

## Seeded Test Users

These users are seeded into `dbo.Users` by the migration:

| Role | Email | Password |
|------|-------|----------|
| Customer | `customer.test@reviewportal.local` | `Customer123!` |
| Admin | `admin.test@reviewportal.local` | `Admin123!` |
| Moderator | `moderator.test@reviewportal.local` | `Moderator123!` |

## SQL Scripts

Generated SQL files are stored in [`scripts/sql/`](scripts/sql/):

- `InitialCreate.sql` - full migration script
- `SeedTestUsers.sql` - standalone user seed script
- `SeedFullTestData.sql` - standalone full relational seed script for all current tables

Run the standalone user seed script manually with `sqlcmd` if needed:

```powershell
sqlcmd -S "<server>" -d "<database>" -U "<username>" -P "<password>" -i "C:\Users\user\source\repos\ReviewPortal\BackEnd\scripts\sql\SeedTestUsers.sql"
```

Run the full relational seed script to populate categories, tools, images, reviews, comments, and company responses:

```powershell
sqlcmd -S "<server>" -d "<database>" -U "<username>" -P "<password>" -i "C:\Users\user\source\repos\ReviewPortal\BackEnd\scripts\sql\SeedFullTestData.sql"
```

## Common Commands

```powershell
dotnet build ReviewPortal.slnx
dotnet test ReviewPortal.slnx
dotnet run --project src/ReviewPortal.API
dotnet ef migrations add <MigrationName> --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API
dotnet ef database update --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API
dotnet ef migrations script 0 <MigrationName> --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API --output scripts/sql/<MigrationName>.sql
```

## Troubleshooting

If Visual Studio shows DLL copy errors such as `MSB3021` or `MSB3027`, stop the running API/debug session and rebuild.

If the API throws `JWT configuration is missing a secure secret`, set `Jwt:Secret` with user-secrets and rebuild.

If the API throws `Database connection string 'DefaultConnection' is not configured`, set `ConnectionStrings:DefaultConnection` with user-secrets.

## Key Documentation

| Document | Path |
|----------|------|
| Architecture and conventions | [`CLAUDE.md`](CLAUDE.md) |
| AI agent instructions | [`AGENTS.md`](AGENTS.md) |
| Entity relationship diagram | [`docs/ERD.md`](docs/ERD.md) |
| Requirements specification | [`docs/REQUIREMENTS-SPECIFICATION.md`](docs/REQUIREMENTS-SPECIFICATION.md) |
| Non-functional requirements | [`docs/NON-FUNCTIONAL-REQUIREMENTS.md`](docs/NON-FUNCTIONAL-REQUIREMENTS.md) |
| Testing strategy | [`docs/TESTING-STRATEGY.md`](docs/TESTING-STRATEGY.md) |
| Product backlog | [`docs/agile/PRODUCT-BACKLOG.md`](docs/agile/PRODUCT-BACKLOG.md) |
| Sprint planning | [`docs/agile/SPRINT-PLANNING.md`](docs/agile/SPRINT-PLANNING.md) |
