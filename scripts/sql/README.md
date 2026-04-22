# SQL Scripts

These scripts mirror the checked-in EF Core migrations and can be applied manually when required.

## Included scripts

- `InitialCreate.sql`
  Creates the baseline schema, including `Reviews`, `ReviewComments`, `CompanyResponses`, and the review indexes required by Epic 2.
- `SeedEpic1CatalogueData.sql`
  Applies the seeded category, tool, and tool-image data from migration `20260409234000_SeedEpic1CatalogueData`.
- `AddUserPasswordResetFields.sql`
  Adds the password-reset columns introduced by migration `20260412090000_AddUserPasswordResetFields`.
- `SeedRemainingEpic1CatalogueData.sql`
  Applies the missing Painting & Decorating, Plumbing & Drainage, and Services catalogue data from migration `20260422213000_SeedRemainingEpic1CatalogueData`.
- `AddReviewCommentRejectionReason.sql`
  Adds the comment moderation rejection reason column introduced by migration `20260422221054_AddReviewCommentRejectionReason`.
- `SeedEpic2ReviewData.sql`
  Applies approved and pending review demo data from migration `20260422223500_SeedEpic2ReviewData`.

## Recommended commands

Check for pending model changes:

```powershell
dotnet ef migrations has-pending-model-changes --no-build --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API
```

Apply migrations to the configured database:

```powershell
dotnet ef database update --no-build --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API
```

Generate a SQL script for a new migration:

```powershell
dotnet ef migrations script <from-migration> <to-migration> --no-build --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API -o scripts/sql/<MigrationName>.sql
```

## Current Epic 2 status

As of `2026-04-16`, `dotnet ef migrations has-pending-model-changes` reports no pending model changes for the current Epic 2 backend implementation.

`dotnet ef database update` is still environment-dependent. In this workspace it fails before applying anything because the configured SQL Server requires encryption support that this machine does not currently provide.
