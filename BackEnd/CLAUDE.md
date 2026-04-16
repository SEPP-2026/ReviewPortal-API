# CLAUDE.md – Shelton Tool-Hire Review Portal (Back End)

## Project Overview

This is the **back-end** component of the **Shelton Tool-Hire Review Portal** — a web application where customers browse hire equipment, calculate rental costs, and leave reviews. Staff manage the catalogue and moderate content from a back-office admin area.

> **Monorepo layout:** The repository root contains `BackEnd/` (this project) and `FrontEnd/` (the client application). All paths in this document are relative to the `BackEnd/` directory.

- **Backend:** ASP.NET Core Web API (.NET 8) with Clean Architecture
- **Frontend:** Lives in the sibling `../FrontEnd/` directory (separate from this .NET solution)
- **Database:** Microsoft SQL Server with EF Core (Code-First migrations)
- **Auth:** ASP.NET Identity with JWT bearer tokens
- **Testing:** xUnit (unit + integration), Playwright (E2E)
- **CI/CD:** GitHub Actions

---

## Solution Structure (Clean Architecture)

```
BackEnd/                           # ← You are here
├── src/
│   ├── ReviewPortal.Domain/           # Entities, Enums, Value Objects, Domain Interfaces
│   ├── ReviewPortal.Application/      # Use Cases, DTOs, Service Interfaces, Validators
│   ├── ReviewPortal.Infrastructure/   # EF Core DbContext, Repositories, External Services
│   └── ReviewPortal.API/              # Controllers, Middleware, DI Configuration
├── tests/
│   ├── ReviewPortal.UnitTests/        # Unit tests for Application + Domain
│   └── ReviewPortal.IntegrationTests/ # Integration tests with test database
├── docs/                              # Project documentation & agile artefacts
├── ReviewPortal.slnx                  # .NET solution file
├── CLAUDE.md                          # This file
└── AGENTS.md                          # AI agent instructions

FrontEnd/                          # Sibling directory (client app)
```

### Dependency Rule (STRICT)

```
Domain ← Application ← Infrastructure
                      ← API
```

- **Domain** has ZERO project references. No NuGet packages except pure utilities.
- **Application** references only Domain. No EF Core, no ASP.NET.
- **Infrastructure** references Application and Domain. This is where EF Core lives.
- **API** references Application and Infrastructure. Wires up DI.

> ⚠️ NEVER add EF Core or ASP.NET references to Domain or Application projects.

---

## Entities (Domain Layer)

There are 7 domain entities. See `/docs/ERD.md` for the full field-level diagram.

| Entity | Key Relationships |
|--------|-------------------|
| `User` | Writes reviews, authors company responses |
| `Category` | Has many tools |
| `Tool` | Belongs to category, has images and reviews |
| `ToolImage` | Belongs to a tool |
| `Review` | Belongs to tool + user, has comments and company response |
| `ReviewComment` | Belongs to a review |
| `CompanyResponse` | One-to-one with review, authored by staff |

### Entity Rules

- All entities inherit from `BaseEntity` which provides `Id` (int, auto-increment)
- Entities with timestamps inherit from `AuditableEntity` (adds `CreatedDate`, `UpdatedDate`)
- Use private setters on entity properties; mutate through domain methods
- Enums: `ReviewStatus` (Pending, Approved, Rejected), `UserRole` (Customer, Admin, Moderator)

---

## Coding Conventions

### C# / .NET

- Use **file-scoped namespaces** (`namespace X;` not `namespace X { }`)
- Use **primary constructors** where appropriate (.NET 8)
- Use **records** for DTOs and value objects
- Use `readonly` and `required` properties on entities where appropriate
- Async all the way — all service and repository methods return `Task<T>`
- Name async methods with `Async` suffix (e.g., `GetToolByIdAsync`)
- Use `CancellationToken` on all async methods
- Return `Result<T>` pattern from Application services, not exceptions for business logic
- Use **FluentValidation** for request validation in the Application layer
- Controllers should be thin — delegate to Application services immediately
- Use `ILogger<T>` for logging, never `Console.WriteLine`

### Naming

- Entities: singular (`Tool`, not `Tools`)
- DbSets: plural (`public DbSet<Tool> Tools { get; set; }`)
- Interfaces: `I` prefix (`IToolRepository`, `IReviewService`)
- DTOs: suffixed (`ToolDto`, `CreateReviewRequest`, `ReviewResponse`)
- Service implementations: no suffix (`ToolService` implements `IToolService`)

### Project Organisation

Each project follows this folder structure:

**Domain:**
```
Entities/        # Tool.cs, Review.cs, etc.
Enums/           # ReviewStatus.cs, UserRole.cs
Interfaces/      # IRepository.cs (generic), IUnitOfWork.cs
Common/          # BaseEntity.cs, AuditableEntity.cs
```

**Application:**
```
Common/          # Result.cs, PagedList.cs, MappingProfiles
DTOs/            # Organised by feature: Tools/, Reviews/, Users/
Interfaces/      # IToolService.cs, IReviewService.cs, etc.
Services/        # ToolService.cs, ReviewService.cs, etc.
Validators/      # FluentValidation validators
```

**Infrastructure:**
```
Data/            # AppDbContext.cs, Migrations/
Repositories/    # ToolRepository.cs, ReviewRepository.cs, etc.
Configuration/   # EF entity type configurations (Fluent API)
Identity/        # JWT service, token generation
```

**API:**
```
Controllers/     # ToolsController.cs, ReviewsController.cs, etc.
Middleware/       # ExceptionHandling, etc.
Extensions/      # ServiceCollectionExtensions for DI registration
```

---

## API Conventions

- RESTful routes: `/api/tools`, `/api/tools/{id}`, `/api/tools/{toolId}/reviews`
- Admin routes: `/api/admin/tools`, `/api/admin/moderation/pending`
- All admin endpoints require `[Authorize(Roles = "Admin")]` or `"Admin,Moderator"`
- Return `ActionResult<T>` from controllers
- Use `CreatedAtAction` for POST responses (201)
- Use `Problem()` for error responses with ProblemDetails
- Pagination: `?page=1&pageSize=10` — return `PagedList<T>` with metadata

---

## Database / EF Core

- Use **Fluent API** for entity configuration, not data annotations
- Each entity gets its own `IEntityTypeConfiguration<T>` class
- Seed data: at least 3 categories with 4–5 tools each (defined in configurations)
- Connection string: `appsettings.Development.json` (never commit production secrets)
- Whenever a change affects the database schema or persisted EF Core seed data, also add a migration, generate a SQL script in `scripts/sql/`, run `dotnet ef database update`, and update schema docs such as `docs/ERD.md`
- If EF Core reports no pending model changes, do not create an empty migration
- Migration commands run from the API project:
  ```bash
  dotnet ef migrations add <Name> --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API
  dotnet ef database update --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API
  dotnet ef migrations script <FromMigration> <ToMigration> --idempotent --output scripts/sql/<ToMigration>.sql --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API
  ```

---

## Authentication & Authorisation

- ASP.NET Identity for user management
- JWT bearer tokens for API authentication
- Roles: `Customer`, `Admin`, `Moderator`
- Token includes claims: `sub` (userId), `email`, `role`
- Token expiry: 60 minutes with refresh token support
- Password policy: minimum 8 chars, at least 1 uppercase, 1 number

---

## Testing

- **Unit tests:** Test Application services with mocked repositories
- **Integration tests:** Use `WebApplicationFactory<Program>` with a test SQL Server database
- Test naming: `MethodName_StateUnderTest_ExpectedBehaviour`
- Use `FluentAssertions` for readable assertions
- Use `Bogus` for test data generation

---

## Common Commands

> All commands below should be run from the `BackEnd/` directory.

```bash
# Build the solution
dotnet build

# Run tests
dotnet test

# Run the API
dotnet run --project src/ReviewPortal.API

# Add a migration
dotnet ef migrations add <MigrationName> --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API

# Generate a SQL script for a migration
dotnet ef migrations script <FromMigration> <ToMigration> --idempotent --output scripts/sql/<ToMigration>.sql --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API

# Update database
dotnet ef database update --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API
```

---

## Review Ratings

When implementing the review system, each review has 5 individual ratings (1–5 stars):

1. **EquipmentRating** — Equipment Performance
2. **CustomerServiceRating** — Booking & Customer Service
3. **TechnicalSupportRating** — Technical Support & Guidance
4. **AfterSalesRating** — After-Sales & Breakdown Support
5. **ValueForMoneyRating** — Value for Money

The `OverallRating` is the arithmetic average of all five.

---

## What NOT to Do

- ❌ Do NOT put business logic in controllers
- ❌ Do NOT reference EF Core in Domain or Application
- ❌ Do NOT use `DateTime.Now` — use `DateTime.UtcNow` or inject `IDateTimeProvider`
- ❌ Do NOT hard-delete records — use soft-delete (`IsActive = false`)
- ❌ Do NOT return entity objects from API endpoints — always map to DTOs
- ❌ Do NOT put connection strings or secrets in `appsettings.json` — use User Secrets or env vars
- ❌ Do NOT skip CancellationToken on async methods
