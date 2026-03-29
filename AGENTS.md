# AGENTS.md – AI Agent Instructions

> This file is read by AI coding agents (Codex, Devin, Copilot Workspace, etc.) to understand how to work in this repository.

## Quick Start

```bash
# Restore and build
dotnet build ReviewPortal.sln

# Run the API (default: https://localhost:5001)
dotnet run --project src/ReviewPortal.API

# Run all tests
dotnet test ReviewPortal.sln
```

## Architecture

This project uses **Clean Architecture** with 4 layers. Read `CLAUDE.md` for the full guide.

| Layer | Project | Dependencies |
|-------|---------|-------------|
| Domain | `ReviewPortal.Domain` | None |
| Application | `ReviewPortal.Application` | Domain |
| Infrastructure | `ReviewPortal.Infrastructure` | Domain, Application |
| Presentation | `ReviewPortal.API` | Application, Infrastructure |

**The dependency rule is absolute:** inner layers must never reference outer layers.

## Key Files

| What | Where |
|------|-------|
| Full ERD | `docs/ERD.md` |
| Architecture guide | `CLAUDE.md` |
| Agile backlog | `docs/agile/PRODUCT-BACKLOG.md` |
| Sprint plan | `docs/agile/SPRINT-PLANNING.md` |
| Epic 1 (Catalogue) | `docs/agile/EPIC-1-CATALOGUE-AND-CALCULATOR.md` |
| Epic 2 (Reviews) | `docs/agile/EPIC-2-REVIEWS-AND-RATINGS.md` |
| Epic 3 (Admin) | `docs/agile/EPIC-3-BACKOFFICE-AND-MODERATION.md` |
| Domain entities | `src/ReviewPortal.Domain/Entities/` |
| EF DbContext | `src/ReviewPortal.Infrastructure/Data/AppDbContext.cs` |
| API controllers | `src/ReviewPortal.API/Controllers/` |

## Before Making Changes

1. Read `CLAUDE.md` for coding conventions and architecture rules
2. Check `docs/ERD.md` for the data model
3. Ensure your changes follow Clean Architecture (no layer violations)
4. Run `dotnet build` to verify compilation
5. Run `dotnet test` to verify no regressions

## Code Style

- File-scoped namespaces
- Async/await with CancellationToken
- FluentValidation for input validation
- Fluent API for EF Core configuration (not data annotations)
- DTOs for all API request/response models (never expose entities)
- Result pattern for service returns

## Testing

- Unit tests: `tests/ReviewPortal.UnitTests/`
- Integration tests: `tests/ReviewPortal.IntegrationTests/`
- Naming: `MethodName_Condition_ExpectedResult`
- Use FluentAssertions and Bogus
