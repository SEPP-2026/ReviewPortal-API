# AGENTS.md - AI Agent Instructions

> This file is read by AI coding agents working in the repository root.
> All paths below are relative to the repository root.

## Quick Start

```bash
# Restore and build
dotnet build ReviewPortal.slnx

# Run the API (default: https://localhost:5001)
dotnet run --project src/ReviewPortal.API

# Run all tests
dotnet test ReviewPortal.slnx
```

## Architecture

This project uses Clean Architecture with four layers. Read `CLAUDE.md` for the full guide.

| Layer | Project | Dependencies |
|-------|---------|-------------|
| Domain | `ReviewPortal.Domain` | None |
| Application | `ReviewPortal.Application` | Domain |
| Infrastructure | `ReviewPortal.Infrastructure` | Domain, Application |
| Presentation | `ReviewPortal.API` | Application, Infrastructure |

The dependency rule is absolute: inner layers must never reference outer layers.

## Key Files

| What | Where |
|------|-------|
| Full ERD | `docs/ERD.md` |
| Architecture guide | `CLAUDE.md` |
| Requirements specification | `docs/REQUIREMENTS-SPECIFICATION.md` |
| Non-functional requirements | `docs/NON-FUNCTIONAL-REQUIREMENTS.md` |
| Testing strategy | `docs/TESTING-STRATEGY.md` |
| Agile backlog | `docs/agile/PRODUCT-BACKLOG.md` |
| Sprint plan | `docs/agile/SPRINT-PLANNING.md` |
| Epic 1 (Catalogue) | `docs/agile/EPIC-1-CATALOGUE-AND-CALCULATOR.md` |
| Epic 2 (Reviews) | `docs/agile/EPIC-2-REVIEWS-AND-RATINGS.md` |
| Epic 3 (Admin) | `docs/agile/EPIC-3-BACKOFFICE-AND-MODERATION.md` |
| Domain entities | `src/ReviewPortal.Domain/Entities/` |
| EF DbContext | `src/ReviewPortal.Infrastructure/Data/AppDbContext.cs` |
| API controllers | `src/ReviewPortal.API/Controllers/` |

## Before Making Changes

1. Read `CLAUDE.md` for coding conventions and architecture rules.
2. Check `docs/ERD.md` for the data model.
3. Ensure your changes follow Clean Architecture with no layer violations.
4. If you change the EF Core model or persisted seed data, add a migration, generate a SQL script in `scripts/sql/`, and run `dotnet ef database update`.
5. Run `dotnet build` to verify compilation.
6. Run `dotnet test` to verify no regressions.

## Code Style

- File-scoped namespaces
- Async/await with `CancellationToken`
- FluentValidation for input validation
- Fluent API for EF Core configuration, not data annotations
- DTOs for all API request and response models
- Result pattern for service returns

## Database Changes

If a task changes the database schema or persisted EF Core seed data, complete the full database workflow in the same change:

1. Create a migration:
   `dotnet ef migrations add <MigrationName> --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API`
2. Generate a SQL deployment script:
   `dotnet ef migrations script <PreviousMigration> <MigrationName> --idempotent --output scripts/sql/<MigrationName>.sql --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API`
3. Apply the migration locally:
   `dotnet ef database update --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API`
4. Update schema documentation such as `docs/ERD.md` when the data model changes.
5. Include the migration files and SQL script in the same change.

If EF Core reports that there are no pending model changes, do not add an empty migration. Record that no migration was required.

## Testing

- Unit tests: `tests/ReviewPortal.UnitTests/`
- Integration tests: `tests/ReviewPortal.IntegrationTests/`
- Naming: `MethodName_Condition_ExpectedResult`
- Use FluentAssertions and Bogus
