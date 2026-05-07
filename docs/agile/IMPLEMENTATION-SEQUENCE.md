# Implementation Sequence

> Last updated: 2026-05-07
> Scope: remaining backend/API work for Epic 1, Epic 2, Epic 3, CI/security readiness, Azure App Service readiness, and Next.js API contract readiness.

## Goal

Use this file as the ordered backend runbook before moving Jira items to Done.

It answers:

- which Jira epic items already have backend API support
- which backend tasks are still missing
- which task to run next
- what to verify locally and on Azure
- what the Next.js frontend must call

## Current Backend Status By Jira Epic

### Epic 1 - Tools/Service Catalogue And Rental System

| Jira item | Backend/API status | Remaining backend task |
|----------|--------------------|------------------------|
| MP-3 Homepage with Featured Categories | API exists: `GET /api/categories` and `GET /api/categories/featured` | Done by TASK-29 |
| MP-10 Category Browsing Page | API exists: `GET /api/categories/{id}/tools` with paging/sort/filter | Done by TASK-29 |
| MP-13 Search for Tools/Service | API exists: `GET /api/tools/search?q=` | Done by TASK-29 |
| MP-15 Filter Tools/Service by Price Range | API exists through category tools query with `minPrice` and `maxPrice` | Done by TASK-29 |
| MP-11 Tool/Service Detail Page | API exists: `GET /api/tools/{id}` | Done by TASK-29 |
| MP-14 Rental Cost Calculator | API exists: `POST /api/tools/{id}/rental-calculation` | Done by TASK-29 |

Conclusion: Epic 1 feature API and final public catalogue API contract coverage are implemented.

### Epic 2 - User Accounts And Interaction

| Jira item | Backend/API status | Remaining backend task |
|----------|--------------------|------------------------|
| MP-24 User Registration and Login | API exists under `/api/auth/...` with JWT and role claims | Done by TASK-30 |
| MP-18 Submit a Review for a Tools/Service | API exists: `POST /api/tools/{toolId}/reviews` | Done by TASK-30 |
| MP-19 Display Approved Reviews on the Tools/Service Page | API exists: `GET /api/tools/{toolId}/reviews` | Done by TASK-30 |
| MP-20 Overall Tools/service Rating | API DTOs expose rating/count/threshold state | Done by TASK-30 |
| MP-21 Comment on a Review | API exists: `POST /api/reviews/{reviewId}/comments` and `GET /api/reviews/{reviewId}/comments` | Done by TASK-30 |
| MP-23 Company Response to a Review | API exists: `POST/PUT/DELETE /api/reviews/{reviewId}/response`; staff policy is `Admin,Moderator` | Done by TASK-30 |
| MP-25 My Reviews Page | API exists: `GET /api/users/me/reviews` | Done by TASK-30 |

Conclusion: Epic 2 feature API is implemented and covered by final HTTP-level contract tests.

### Epic 3 - Back-Office Management And Moderation

| Jira item | Backend/API status | Remaining backend task |
|----------|--------------------|------------------------|
| MP-26 Admin Login and Role-Based Access | Same `/api/auth/login` returns JWT role claims; admin APIs enforce roles | No backend feature gap. TASK-17 remains conditional only if full ASP.NET Identity is required. |
| MP-31 Review Moderation Queue | API exists: `GET /api/admin/moderation/pending`; item-level queue done | Done by TASK-27 and TASK-28 |
| MP-72 Approve or Reject Reviews | API exists: `PUT /api/admin/moderation/reviews/{id}` and comments endpoint | Done |
| MP-27 Add New Equipment/Service to the Catalogue | API exists: multipart `POST /api/admin/tools` with metadata plus required first image file | Done by TASK-26 |
| MP-28 Edit Equipment/Service Details and Pricing | API exists: `PUT /api/admin/tools/{id}` | Done |
| MP-29 Manage Tool/Service Images | Upload/delete endpoints exist, and first image during create is upload-backed | Done by TASK-4 and TASK-26 |
| MP-30 Deactivate or Remove Equipment/Service | API exists: `PATCH /api/admin/tools/{id}/status`; soft delete/reactivate supported | Done |
| MP-32 Manage Categories | API exists: `/api/admin/categories` create/update/delete plus public read endpoints | Done |
| MP-33 Admin Dashboard with Overview Statistics | API exists: `GET /api/admin/dashboard/stats` | Done by TASK-5 and TASK-28 |

Conclusion: Epic 3 backend feature APIs are implemented. Remaining work is CI/security/deployment evidence, not an Epic 3 feature gap.

## Still Open Backend Tasks

Run these in order unless the user/assessor changes scope.

1. TASK-24.2: rotate the previously exposed Azure SQL password and any leaked secrets.
2. TASK-23: expand CI/CD and coverage automation after the final contract tests exist.
3. Security backlog: apply the highest-priority security fixes from `docs/security/BACKEND-SECURITY-SCAN-2026-05-07.md`, especially SEC-02 and SEC-03 before next week's scan.
4. Final Azure smoke test and Next.js API contract check.

Do not run TASK-17 unless full ASP.NET Identity is explicitly required. The current project decision is to keep the custom JWT API flow with ASP.NET Core password hashing.

## Recommended One-By-One Run Sequence

### Step 1 - Secrets and scan blockers

Run:

- TASK-24.2
- SEC-02 from the security backlog: fix `scripts/security/scan-secrets.ps1` false positives
- SEC-03 from the security backlog: update vulnerable NuGet packages

Why:

- CI/security scan currently fails because the secret scanner matches its own pattern definitions.
- Vulnerability scan found transitive package advisories.
- Azure SQL credential rotation is a manual external action and should be done before any more Azure database work.

Verify:

```powershell
./scripts/security/scan-secrets.ps1
dotnet list ReviewPortal.slnx package --vulnerable --include-transitive --source https://api.nuget.org/v3/index.json
dotnet build ReviewPortal.slnx
dotnet test ReviewPortal.slnx
```

### Step 2 - Update CI and coverage after all final tests exist

Run:

- TASK-23

Why:

- CI should run the final unit and integration tests, not only older unit tests.
- Coverage automation should be added after the final test set exists.
- Security scan should run before restore/build/test in CI.

Minimum CI checks:

- secret scan
- NuGet vulnerability scan
- restore
- build
- unit tests
- integration tests
- coverage report

### Step 3 - Final Azure and Next.js readiness pass

Run after all code/test tasks are green.

Azure settings to confirm:

- `ASPNETCORE_ENVIRONMENT=Production`
- `ConnectionStrings__DefaultConnection=<rotated-production-connection-string>`
- `Jwt__Secret=<strong-secret>`
- `Jwt__Issuer=ReviewPortalAPI`
- `Jwt__Audience=ReviewPortalClient`
- `Jwt__ExpiryMinutes=60`
- `Cors__AllowedOrigins__0=<Next.js-production-origin>`
- additional `Cors__AllowedOrigins__N` values if needed

Azure smoke tests:

- `GET /health` returns `200`
- public category/search/detail/rental endpoints respond
- protected admin route without token returns `401`
- protected admin route with customer token returns `403`
- admin route with admin token returns success
- CORS preflight from the Next.js origin succeeds

## API Routes The Next.js App Should Use

Public catalogue:

- `GET /api/categories`
- `GET /api/categories/featured`
- `GET /api/categories/{id}`
- `GET /api/categories/{id}/tools?page=&pageSize=&sortBy=&sortOrder=&minPrice=&maxPrice=`
- `GET /api/tools/search?q=&page=&pageSize=`
- `GET /api/tools/{id}`
- `POST /api/tools/{id}/rental-calculation`

Auth and user:

- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/auth/me`
- `POST /api/auth/change-password`
- `POST /api/auth/forgot-password`
- `POST /api/auth/reset-password`
- `GET /api/users/me/reviews`

Reviews and community:

- `POST /api/tools/{toolId}/reviews`
- `GET /api/tools/{toolId}/reviews`
- `POST /api/reviews/{reviewId}/comments`
- `GET /api/reviews/{reviewId}/comments`
- `POST /api/reviews/{reviewId}/response`
- `PUT /api/reviews/{reviewId}/response`
- `DELETE /api/reviews/{reviewId}/response`

Admin:

- `GET /api/admin/tools`
- `GET /api/admin/tools/{id}`
- `POST /api/admin/tools` multipart form data with metadata fields plus required `file`
- `PUT /api/admin/tools/{id}`
- `PATCH /api/admin/tools/{id}/status`
- `POST /api/admin/tools/{id}/images`
- `DELETE /api/admin/tools/{id}/images/{imageId}`
- `POST /api/admin/categories`
- `PUT /api/admin/categories/{id}`
- `DELETE /api/admin/categories/{id}`
- `GET /api/admin/moderation/pending`
- `PUT /api/admin/moderation/reviews/{id}`
- `PUT /api/admin/moderation/comments/{id}`
- `GET /api/admin/dashboard/stats`

Do not assume there is a `GET /api/tools` endpoint. It does not exist.

## Command Checklist For Every Implementation Task

Use this checklist after every code task:

1. `dotnet build ReviewPortal.slnx`
2. `dotnet test ReviewPortal.slnx`
3. If schema or persisted seed data changed:
   - create EF migration
   - generate idempotent SQL script in `scripts/sql/`
   - run local database update
   - update schema docs if needed
4. If local migration is valid, apply it to Azure using rotated credentials.
5. Re-run key API smoke checks.

## Definition Of Done For Backend Jira Items

Backend Jira items can be moved to Done when:

- the linked API/service behavior exists
- unit tests pass
- integration tests cover the real HTTP endpoint for success and key failure paths
- auth/role behavior is tested where relevant
- migrations/scripts are present for schema or persisted seed changes
- Azure settings and CORS are verified for frontend calls
- no committed-secret or package-vulnerability scan blockers remain

## Related Files

- [IMPLEMENTATION-TASKS.md](./IMPLEMENTATION-TASKS.md)
- [EPIC-1-CATALOGUE-AND-CALCULATOR.md](./EPIC-1-CATALOGUE-AND-CALCULATOR.md)
- [EPIC-2-REVIEWS-AND-RATINGS.md](./EPIC-2-REVIEWS-AND-RATINGS.md)
- [EPIC-3-BACKOFFICE-AND-MODERATION.md](./EPIC-3-BACKOFFICE-AND-MODERATION.md)
- [BACKEND-SECURITY-SCAN-2026-05-07.md](../security/BACKEND-SECURITY-SCAN-2026-05-07.md)
- [DEPLOYMENT-TO-AZURE-APP-SERVICE.md](../DEPLOYMENT-TO-AZURE-APP-SERVICE.md)
