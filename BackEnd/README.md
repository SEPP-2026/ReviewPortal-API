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
dotnet restore ReviewPortal.slnx
dotnet build ReviewPortal.slnx
```

### 3. Create or update the database

Apply all EF Core migrations:

```powershell
dotnet ef database update --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API
```

This applies the full schema, including the password reset columns used by the auth API, and inserts the seeded test users plus the Epic 1 catalogue categories, tools, and tool images automatically.

If you need to apply only the latest auth-related schema update, you can target the password reset migration explicitly:

```powershell
dotnet ef database update 20260412090000_AddUserPasswordResetFields --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API
```

Recommended order for a local refresh:

```powershell
dotnet restore ReviewPortal.slnx
dotnet build ReviewPortal.slnx
dotnet ef database update --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API
sqlcmd -S "<server>" -d "<database>" -U "<username>" -P "<password>" -i "C:\Users\user\source\repos\ReviewPortal\BackEnd\scripts\sql\SeedTestUsers.sql"
sqlcmd -S "<server>" -d "<database>" -U "<username>" -P "<password>" -i "C:\Users\user\source\repos\ReviewPortal\BackEnd\scripts\sql\SeedFullTestData.sql"
dotnet run --project src/ReviewPortal.API
```

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
- authenticated `/api/auth/change-password`
- `/api/auth/forgot-password`
- `/api/auth/reset-password`
- role claims for `Customer`, `Admin`, and `Moderator`

Password rules:

- minimum 8 characters
- at least one uppercase letter
- at least one number

Passwords are stored using ASP.NET Identity password hashing.

Forgot/reset password note:

- the current system does not send email
- `POST /api/auth/forgot-password` returns a reset token in the API response for local/dev use
- `POST /api/auth/reset-password` consumes that token and updates the password
- rerunning the seed scripts clears stored reset tokens for the seeded test users

## Seeded Test Users

These users are seeded into `dbo.Users` by the migration:

| Role | Email | Password |
|------|-------|----------|
| Customer | `customer.test@reviewportal.local` | `Customer123!` |
| Admin | `admin.test@reviewportal.local` | `Admin123!` |
| Moderator | `moderator.test@reviewportal.local` | `Moderator123!` |

## SQL Scripts

Generated SQL files are stored in [`scripts/sql/`](scripts/sql/):

- `InitialCreate.sql` - initial schema migration SQL
- `AddUserPasswordResetFields.sql` - idempotent SQL script to add the password reset columns
- `SeedTestUsers.sql` - standalone rerunnable user seed script
- `SeedFullTestData.sql` - standalone rerunnable full relational seed script for all current tables, including Epic 1 catalogue data with at least three categories containing four tools each

Run the password reset schema SQL manually with `sqlcmd` if you are not using `dotnet ef database update`:

```powershell
sqlcmd -S "<server>" -d "<database>" -U "<username>" -P "<password>" -i "C:\Users\user\source\repos\ReviewPortal\BackEnd\scripts\sql\AddUserPasswordResetFields.sql"
```

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
dotnet ef database update 20260412090000_AddUserPasswordResetFields --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API
dotnet ef migrations script 0 <MigrationName> --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API --output scripts/sql/<MigrationName>.sql
sqlcmd -S "<server>" -d "<database>" -U "<username>" -P "<password>" -i "C:\Users\user\source\repos\ReviewPortal\BackEnd\scripts\sql\SeedTestUsers.sql"
sqlcmd -S "<server>" -d "<database>" -U "<username>" -P "<password>" -i "C:\Users\user\source\repos\ReviewPortal\BackEnd\scripts\sql\SeedFullTestData.sql"
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
