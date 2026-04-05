# Shelton Tool-Hire Review Portal — Back End

ASP.NET Core Web API (.NET 8) built with **Clean Architecture**.

> This project is part of a monorepo. The front-end client lives in the sibling [`../FrontEnd/`](../FrontEnd/) directory.

## Solution Structure

```
BackEnd/
├── src/
│   ├── ReviewPortal.Domain/           # Entities, Enums, Value Objects, Domain Interfaces
│   ├── ReviewPortal.Application/      # Use Cases, DTOs, Service Interfaces, Validators
│   ├── ReviewPortal.Infrastructure/   # EF Core DbContext, Repositories, External Services
│   └── ReviewPortal.API/              # Controllers, Middleware, DI Configuration
├── tests/
│   ├── ReviewPortal.UnitTests/        # Unit tests (Application + Domain)
│   └── ReviewPortal.IntegrationTests/ # Integration tests (test database)
├── docs/                              # Requirements, ERD, agile artefacts
├── ReviewPortal.slnx                  # .NET solution file
├── CLAUDE.md                          # Full architecture & coding conventions guide
└── AGENTS.md                          # AI agent quick-reference
```

## Quick Start

```bash
# From the BackEnd/ directory:

# Restore & build
dotnet build ReviewPortal.slnx

# Run the API (https://localhost:5001)
dotnet run --project src/ReviewPortal.API

# Run all tests
dotnet test ReviewPortal.slnx
```

## Key Documentation

| Document | Path |
|----------|------|
| Architecture & Conventions | [`CLAUDE.md`](CLAUDE.md) |
| AI Agent Instructions | [`AGENTS.md`](AGENTS.md) |
| Entity Relationship Diagram | [`docs/ERD.md`](docs/ERD.md) |
| Requirements Specification | [`docs/REQUIREMENTS-SPECIFICATION.md`](docs/REQUIREMENTS-SPECIFICATION.md) |
| Non-Functional Requirements | [`docs/NON-FUNCTIONAL-REQUIREMENTS.md`](docs/NON-FUNCTIONAL-REQUIREMENTS.md) |
| Testing Strategy | [`docs/TESTING-STRATEGY.md`](docs/TESTING-STRATEGY.md) |
| Product Backlog | [`docs/agile/PRODUCT-BACKLOG.md`](docs/agile/PRODUCT-BACKLOG.md) |
| Sprint Planning | [`docs/agile/SPRINT-PLANNING.md`](docs/agile/SPRINT-PLANNING.md) |

## Tech Stack

- **Runtime:** .NET 8 / ASP.NET Core
- **Database:** SQL Server + EF Core (Code-First)
- **Auth:** ASP.NET Identity + JWT Bearer Tokens
- **Validation:** FluentValidation
- **Testing:** xUnit, FluentAssertions, Bogus
