# CLAUDE.md - Shelton Tool-Hire Review Portal API

## Project Overview

This repository contains the backend-only MSc submission package for the Shelton Tool-Hire Review Portal.

- Backend: ASP.NET Core Web API on .NET 8 with Clean Architecture
- Database: Microsoft SQL Server with EF Core code-first migrations
- Authentication: ASP.NET Identity with JWT bearer tokens
- Testing: xUnit with unit and integration coverage
- Documentation: ERD, requirements, non-functional requirements, testing strategy, and agile artefacts

The original project planning described a separate frontend client, but that frontend is no longer part of this repository. All active code, tests, scripts, and automation here relate to the backend deliverable.

## Solution Structure

```text
ReviewPortal-API/
|-- src/
|   |-- ReviewPortal.Domain/           # Entities, enums, value objects, domain interfaces
|   |-- ReviewPortal.Application/      # Use cases, DTOs, service interfaces, validators
|   |-- ReviewPortal.Infrastructure/   # EF Core DbContext, repositories, auth providers, migrations
|   `-- ReviewPortal.API/              # Controllers, middleware, DI configuration
|-- tests/
|   |-- ReviewPortal.UnitTests/        # Application, domain, infrastructure, and controller tests
|   `-- ReviewPortal.IntegrationTests/ # Authentication and DbContext integration tests
|-- docs/                              # Academic and project documentation
|-- scripts/sql/                       # Migration and seed SQL scripts
|-- ReviewPortal.slnx                  # .NET solution
|-- README.md                          # Submission-oriented repository guide
|-- AGENTS.md                          # AI agent instructions
`-- .github/workflows/ci.yml           # Build and test automation
```

### Dependency Rule

```text
Domain <- Application <- Infrastructure
                     <- API
```

- Domain has zero project references.
- Application references only Domain.
- Infrastructure references Application and Domain.
- API references Application and Infrastructure.

Never add EF Core or ASP.NET dependencies to Domain or Application.

## Domain Model

There are seven core domain entities. See `docs/ERD.md` for the field-level diagram.

| Entity | Key Relationships |
|--------|-------------------|
| `User` | Writes reviews and authors company responses |
| `Category` | Has many tools |
| `Tool` | Belongs to a category and has images and reviews |
| `ToolImage` | Belongs to a tool |
| `Review` | Belongs to a tool and user; has comments and a company response |
| `ReviewComment` | Belongs to a review |
| `CompanyResponse` | One-to-one with a review and authored by staff |

### Entity Rules

- All entities inherit from `BaseEntity` which provides the integer `Id`
- Timestamped entities inherit from `AuditableEntity`
- Prefer private setters and domain methods for state changes
- Enums include `ReviewStatus` and `UserRole`

## Coding Conventions

### C# and .NET

- Use file-scoped namespaces
- Use async all the way and include `CancellationToken`
- Name async methods with the `Async` suffix
- Use records for DTOs where appropriate
- Keep controllers thin and delegate to application services
- Return `Result<T>` for business outcomes instead of using exceptions as control flow
- Use `ILogger<T>` for logging
- Use `DateTime.UtcNow` or an injected time provider, never `DateTime.Now`

### Validation and Mapping

- Use FluentValidation in the Application layer
- Keep mapping between entities and DTOs in the Application layer
- Never expose EF Core entities directly from API endpoints

### Project Organisation

Each project follows a consistent structure:

**Domain**

```text
Common/
Entities/
Enums/
Interfaces/
```

**Application**

```text
Common/
DTOs/
Interfaces/
Services/
Validators/
```

**Infrastructure**

```text
Authentication/
Configuration/
Data/
Migrations/
Repositories/
```

**API**

```text
Controllers/
Extensions/
Middleware/
Properties/
```

## API Conventions

- Use RESTful routes such as `/api/tools` and `/api/tools/{id}`
- Keep admin concerns behind role-based authorisation
- Return `ActionResult<T>` from controllers
- Use `CreatedAtAction` for successful POST responses
- Use `ProblemDetails` for consistent error responses
- Support pagination using query parameters and `PagedList<T>`

## Database and EF Core

- Use Fluent API configuration, not data annotations
- Keep one `IEntityTypeConfiguration<T>` per entity
- Store migrations in `src/ReviewPortal.Infrastructure/Migrations/`
- Store generated SQL scripts in `scripts/sql/`
- Update `docs/ERD.md` whenever the schema meaningfully changes

Migration workflow:

```bash
dotnet ef migrations add <Name> --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API
dotnet ef migrations script <FromMigration> <ToMigration> --idempotent --output scripts/sql/<ToMigration>.sql --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API
dotnet ef database update --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API
```

If EF Core reports no pending model changes, do not create an empty migration.

## Authentication and Authorisation

- ASP.NET Identity is used for user management and password hashing
- JWT bearer tokens secure authenticated endpoints
- Supported roles are `Customer`, `Admin`, and `Moderator`
- Tokens include user and role claims
- Passwords require at least eight characters, one uppercase letter, and one number

## Testing

- Unit tests live in `tests/ReviewPortal.UnitTests/`
- Integration tests live in `tests/ReviewPortal.IntegrationTests/`
- Test names follow `MethodName_StateUnderTest_ExpectedBehaviour`
- Use FluentAssertions for readable assertions
- Use Bogus for generated test data where useful

## Common Commands

Run all commands from the repository root.

```bash
dotnet build ReviewPortal.slnx
dotnet test ReviewPortal.slnx
dotnet run --project src/ReviewPortal.API
dotnet ef migrations add <MigrationName> --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API
dotnet ef migrations script <FromMigration> <ToMigration> --idempotent --output scripts/sql/<MigrationName>.sql --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API
dotnet ef database update --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API
```

## Review Ratings

Each review stores five rating dimensions:

1. `EquipmentRating`
2. `CustomerServiceRating`
3. `TechnicalSupportRating`
4. `AfterSalesRating`
5. `ValueForMoneyRating`

`OverallRating` is the arithmetic average of the five category ratings.

## What Not To Do

- Do not put business logic in controllers
- Do not reference EF Core from Domain or Application
- Do not return entity objects from API endpoints
- Do not hard-code secrets into `appsettings.json`
- Do not skip `CancellationToken` on async methods
- Do not create schema changes without the matching migration and SQL script workflow
