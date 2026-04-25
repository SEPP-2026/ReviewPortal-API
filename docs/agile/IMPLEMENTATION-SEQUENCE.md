# Implementation Sequence

> Last updated: 2026-04-25
> Scope: remaining backend/API work for Epic 2, Epic 3, deployment hardening, Azure App Service readiness, and Next.js integration readiness.

## Goal

Use this file as the single ordered runbook for finishing the remaining backend work.

It is designed to answer:

- what is already done
- what is still missing
- what order to implement tasks in
- what must be verified locally
- what must be verified on Azure
- what the Next.js frontend needs in order to call the API without problems

## Current Status

### Already completed

- Epic 1 catalogue seed completion
- Epic 2 moderation service logic
- Epic 2 admin moderation controller
- Epic 2 review seed data
- Epic 2 company response approval guard
- Epic 2 review-threshold DTO support
- Epic 2 comment and company-response seed data
- documentation cleanup tasks (TASK-7, TASK-8, TASK-10, TASK-11, TASK-12)

### Still open

- TASK-18: review comment status index
- TASK-19: admin tool service logic
- TASK-2: admin tools controller
- TASK-4: image service and image endpoints
- TASK-5: dashboard service and admin dashboard controller
- TASK-16: admin category routing decision
- TASK-20: FluentValidation adoption
- TASK-21: database check constraints for review ratings
- TASK-22: real API integration tests
- TASK-23: CI/CD and coverage automation
- TASK-24: secret cleanup and externalised configuration
- TASK-17: ASP.NET Identity alignment, only if the brief truly requires full Identity rather than the current custom JWT flow

## Azure And Next.js Status

### Verified working

As of 2026-04-25, the deployed Azure API was checked live:

- `/health` returns `200`
- browser CORS preflight for authenticated calls succeeds
- the deployed frontend origin is allowed by CORS
- protected endpoints return `401` with `Bearer` when unauthenticated
- public API calls respond successfully from the deployed frontend origin

### Important frontend note

The frontend must use the actual API routes that exist today:

- `GET /api/categories`
- `GET /api/categories/featured`
- `GET /api/categories/{id}`
- `GET /api/categories/{id}/tools`
- `GET /api/tools/search`
- `GET /api/tools/{id}`
- `POST /api/tools/{id}/rental-calculation`
- auth routes under `/api/auth/...`

Do not assume there is a `GET /api/tools` list endpoint. That route does not exist.

## Recommended Delivery Order

Follow these phases in order.

## Phase 0 - Stabilise secrets and environment first

### Run first

1. TASK-24: Remove committed secrets and externalise environment configuration

### Why first

This is the highest-risk issue in the repository. The repo still contains a committed Azure SQL password and a hard-coded development JWT secret. Do not continue expanding deployment automation until this is fixed.

### Deliverables

- remove secrets from `src/ReviewPortal.API/appsettings.Development.json`
- rotate the exposed Azure SQL password
- move local secrets to user secrets or environment variables
- keep Azure App Service settings in Azure, not in tracked files
- update deployment/setup docs to match

### Verify

- local API starts using user secrets or environment variables
- Azure App Service still has:
  - `ConnectionStrings__DefaultConnection`
  - `Jwt__Secret`
  - `Jwt__Issuer`
  - `Jwt__Audience`
  - `Jwt__ExpiryMinutes`
  - `Cors__AllowedOrigins__0` and any additional origins

## Phase 1 - Fix schema-level integrity gaps

### Run next

2. TASK-18: Add missing `ReviewComments.Status` index
3. TASK-21: Add DB check constraints for review rating values

### Why now

These are small, contained database correctness tasks. They reduce risk before more API surface and admin flows are added.

### Deliverables

- EF configuration updates
- migrations
- SQL scripts in `scripts/sql/`
- local DB update
- Azure DB update
- schema tests

### Verify

- `dotnet build ReviewPortal.slnx`
- `dotnet test ReviewPortal.slnx`
- `dotnet ef database update --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API`
- run latest migrations against Azure after local verification

## Phase 2 - Complete Epic 3 core admin backend

This is the main feature-delivery phase for Epic 3.

### Run in this order

4. TASK-19: Implement admin tool management service methods and creation flow
5. TASK-2: Create `AdminToolsController`
6. TASK-4: Implement `ImageService` and admin image endpoints
7. TASK-5: Implement dashboard service and admin dashboard controller
8. TASK-16: Decide admin category routing

### Why this order

- TASK-19 is the missing core business logic
- TASK-2 depends on TASK-19
- TASK-4 depends on the admin tools slice existing
- TASK-5 is independent of image handling but is still Epic 3 core admin functionality
- TASK-16 is partly architectural and should be settled before frontend/admin integration is finalised

### Epic 3 completion target after this phase

After Phase 2, the backend should support:

- admin create/update/activate/deactivate tool or service
- admin image upload and delete
- admin dashboard stats
- admin moderation queue
- clear decision on category admin routes

### Verify

For each task in this phase:

- add unit tests for service + controller
- run `dotnet build ReviewPortal.slnx`
- run `dotnet test ReviewPortal.slnx`
- if DB model changes, create migration, SQL script, local DB update, and Azure DB update

## Phase 3 - Standardise validation

### Run next

9. TASK-20: Introduce FluentValidation for API request models

### Why here

By this point most of the remaining request models are known. Adding FluentValidation now avoids reworking validators multiple times while Epic 3 APIs are still moving.

### Deliverables

- FluentValidation packages
- validator registration in the API composition root
- validators for auth, catalogue, review, category, moderation, and admin tool requests
- consistent HTTP 400 validation responses

### Verify

- validator unit tests
- at least one integration test for invalid payload -> `400`

## Phase 4 - Auth decision gate

This phase depends on the project requirement decision.

### Recommended path for delivery stability

10A. Keep the current custom JWT auth flow and do not implement TASK-17

Use this path if:

- the real requirement is JWT-secured login for a Web API
- the current custom auth flow is acceptable for the MSc submission
- you want the lowest-risk path to finish Epic 3 and deployment hardening

### Alternative path only if the requirement is strict

10B. Implement TASK-17: Align auth stack with ASP.NET Identity requirement

Use this path only if:

- the brief or assessor explicitly requires full ASP.NET Identity
- you are willing to accept an auth-stack refactor touching schema, DI, services, tests, and seed scripts

### Important note

Even if TASK-17 is implemented, the frontend can still keep the same high-level pattern:

- call auth endpoints
- receive JWT token
- store token client-side
- send `Authorization: Bearer <token>`

The big change is server-side architecture and database schema, not the browser contract.

## Phase 5 - Build real confidence in the API

### Run next

11. TASK-22: Add real API integration tests for critical backend flows
12. TASK-23: Expand CI/CD and coverage automation

### Why this order

- TASK-22 proves the real HTTP pipeline, auth filters, routing, and JSON contracts
- TASK-23 should be added after those integration tests exist so CI can actually run them

### Minimum integration coverage

At minimum, cover:

- public category and tool endpoints
- rental calculation
- register/login/authenticated user flow
- review submission and moderation path
- admin route `401` and `403` behaviour
- dashboard and admin tool routes after Epic 3 work is complete

### Minimum CI coverage

- unit tests
- integration tests
- coverage report publishing
- enforceable coverage thresholds
- deployment smoke test or documented post-deploy check

## Phase 6 - Final Azure and Next.js readiness pass

Run this after the codebase is stable.

### Azure App Service checklist

Confirm these application settings exist in Azure:

- `ASPNETCORE_ENVIRONMENT=Production`
- `ConnectionStrings__DefaultConnection=<production connection string>`
- `Jwt__Secret=<strong secret>`
- `Jwt__Issuer=ReviewPortalAPI`
- `Jwt__Audience=ReviewPortalClient`
- `Jwt__ExpiryMinutes=60`
- `Cors__AllowedOrigins__0=<Next.js production URL>`
- additional `Cors__AllowedOrigins__N` values if needed

### Azure smoke tests

Verify:

- `GET /health` -> `200`
- `GET /swagger/v1/swagger.json` -> `200`
- public API call from browser origin returns `Access-Control-Allow-Origin`
- preflight request for authenticated route returns `204`
- unauthenticated protected route returns `401`

### Next.js checklist

Set:

- `NEXT_PUBLIC_API_BASE_URL=<Azure API base URL>`

The frontend must:

- use the real existing API routes
- send bearer tokens on protected requests
- handle `401` by redirecting to login or clearing invalid auth state
- avoid calling endpoints that do not exist such as `GET /api/tools`

## Command Checklist For Every Implementation Task

Use this checklist after every code task:

1. `dotnet build ReviewPortal.slnx`
2. `dotnet test ReviewPortal.slnx`
3. If schema or seed data changed:
   - `dotnet ef migrations add <MigrationName> --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API`
   - `dotnet ef migrations script <PreviousMigration> <MigrationName> --idempotent --output scripts/sql/<MigrationName>.sql --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API`
   - `dotnet ef database update --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API`
4. If the migration is valid locally, apply it to Azure
5. Re-run the key API smoke checks

## Recommended One-By-One Execution List

If you want the shortest practical sequence, run these in exactly this order:

1. TASK-24
2. TASK-18
3. TASK-21
4. TASK-19
5. TASK-2
6. TASK-4
7. TASK-5
8. TASK-16
9. TASK-20
10. Either skip TASK-17 or implement it only if full ASP.NET Identity is mandatory
11. TASK-22
12. TASK-23
13. Final Azure smoke test
14. Final Next.js end-to-end check against Azure

## Definition Of Done For The Whole Backend

The backend can be treated as fully delivery-ready when all of the following are true:

- all open required backend tasks are complete
- Epic 3 admin APIs are implemented
- secrets are no longer committed
- migrations and SQL scripts are up to date
- local and Azure databases are aligned
- integration tests cover the real HTTP pipeline
- CI runs unit + integration tests and publishes coverage
- Azure App Service starts from environment configuration only
- the deployed Next.js frontend can browse, calculate rental, authenticate, submit reviews, and call admin routes where authorised

## Related Files

- [IMPLEMENTATION-TASKS.md](./IMPLEMENTATION-TASKS.md)
- [EPIC-2-REVIEWS-AND-RATINGS.md](./EPIC-2-REVIEWS-AND-RATINGS.md)
- [EPIC-3-BACKOFFICE-AND-MODERATION.md](./EPIC-3-BACKOFFICE-AND-MODERATION.md)
- [DEPLOYMENT-TO-AZURE-APP-SERVICE.md](../DEPLOYMENT-TO-AZURE-APP-SERVICE.md)
