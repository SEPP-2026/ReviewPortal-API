# Gap Analysis - Shelton Tool-Hire Review Portal

> Last reviewed: 2026-05-07
> Scope: ReviewPortal-API backend, API contract, database, security, deployment readiness, and submission evidence.
>
> Purpose: show the current gap status against the functional requirements, non-functional requirements, lecture feedback, Jira epics, and implementation tasks.

## 1. Executive Summary

The backend API is now feature-complete for the main catalogue, rental calculator, reviews, ratings, moderation, authentication, admin dashboard, categories, image management, and back-office management flows.

The remaining work is not broad feature discovery. It is final sign-off work:

- close the strict first-image create-flow gap if required by the assessor
- finish CI/coverage automation
- complete security scan blockers and Azure credential rotation
- capture final Azure and Next.js integration evidence

## 2. Current 100 Percent Completion Gates

| Gate ID | Area | Status | Required Action |
|---------|------|--------|-----------------|
| CG-01 | Functional backend/API coverage | Mostly complete | Complete TASK-26 only if first image must be uploaded during initial tool/service creation instead of provided through `ImageUrl` then managed through image endpoints |
| CG-02 | Epic 1 API proof | Closed | TASK-29 added final public catalogue/rental-calculator contract tests |
| CG-03 | Epic 2 API proof | Closed | TASK-30 added final auth/review/comment/company-response contract tests |
| CG-04 | Epic 3 API proof | Mostly complete | TASK-28 is complete; keep TASK-26 separate because it changes the create contract |
| CG-05 | Security readiness | Open | Rotate Azure SQL credential, fix secret-scan false positives, and clear/package-review NuGet vulnerabilities |
| CG-06 | CI and coverage | Open | Complete TASK-23 so CI runs secret scan, vulnerability scan, build, unit tests, integration tests, and coverage |
| CG-07 | Azure deployment evidence | Open | Verify App Service settings, migrations, `/health`, public APIs, protected APIs, role checks, and CORS |
| CG-08 | Submission evidence | Partially complete | Attach final test results, dry-run results, usability evidence, diagrams, DB design, and completion report |

## 3. Lecture Feedback Closure

| Feedback ID | Feedback | Status | Evidence |
|-------------|----------|--------|----------|
| LF-1 | Treat services explicitly, not only tools | Closed | Services category and service seed data exist |
| LF-2 | Use "tool/service" wording in stories | Closed | Epic/user-story docs updated |
| LF-3 | Fix sprint sequencing between moderation queue and approval/rejection | Closed | Sprint/backlog docs updated |
| LF-4 | Add data for review/rating demonstration | Closed | Epic 2 review/comment/response seed migrations and SQL scripts exist |
| LF-5 | Make moderation mandatory for customer reviews/comments | Closed | Requirements and service logic enforce pending status |
| LF-6 | Replace vague acceptance criteria | Closed | Agile docs updated with measurable acceptance criteria |
| LF-7 | Add requirements traceability | Closed | Requirements specification and backlog traceability sections exist |
| LF-8 | Define categories, review categories, rating aggregation, moderation, and pricing logic | Closed | Requirements, database, ERD, and gap docs cover the definitions |

## 4. Functional Requirement Gap Matrix

| Functional Area | Requirement IDs | Current State | Gap Status |
|-----------------|-----------------|---------------|------------|
| Catalogue categories and featured categories | FR-01 to FR-03 | Implemented through `CategoriesController` and category services | No feature gap; covered by TASK-29 |
| Category browsing, sorting, filtering, pagination | FR-04, FR-10, FR-11 | Implemented through `GET /api/categories/{id}/tools` query parameters | No feature gap; covered by TASK-29 |
| Tool/service detail | FR-05, FR-06 | Implemented through `GET /api/tools/{id}` with images, rates, notes, deposit, and ratings | No feature gap; covered by TASK-29 |
| Search | FR-07 to FR-09 | Implemented through `GET /api/tools/search` | No feature gap; covered by TASK-29 |
| Rental cost calculator | FR-12 to FR-17 | Implemented through `POST /api/tools/{id}/rental-calculation` | No feature gap; covered by TASK-29 |
| Reviews and rating aggregation | FR-18 to FR-26 | Implemented with five ratings, pending moderation, approved-only display, cached rating/count, and not-enough-reviews state | No feature gap; covered by TASK-30 |
| Comments and company responses | FR-27 to FR-32 | Implemented with pending customer comments and approved-review-only official responses; official response staff policy is `Admin,Moderator` | No feature gap; covered by TASK-30 |
| User registration, login, password reset, My Reviews | FR-33 to FR-36 | Implemented with custom JWT flow and ASP.NET Core password hashing | No feature gap; covered by TASK-30 |
| Admin authentication and role-based access | FR-37 | Implemented through JWT role claims and admin/moderator authorization policies | No feature gap; full ASP.NET Identity is conditional only |
| Admin tool/service management | FR-38 to FR-41, FR-46 | Create/update/status endpoints and services exist | TASK-26 remains if first image must be a multipart upload during create |
| Image management | FR-40, FR-47 | Upload/delete endpoints and image service exist | No feature gap for post-create image management |
| Moderation queue and approve/reject | FR-42, FR-43, FR-48 | Item-level pending queue, exact counts, review/comment moderation, and tests exist | No feature gap |
| Category admin management | FR-44, FR-50 | `/api/admin/categories` endpoints exist | No feature gap |
| Admin dashboard | FR-45, FR-49 | Dashboard service/controller and integration tests exist | No feature gap |
| API validation and DB constraints | FR-51 to FR-53 | FluentValidation validators, comment status index, and review rating check constraints exist | No feature gap |

## 5. Non-Functional Requirement Gap Matrix

| Area | Requirement IDs | Current State | Gap Status |
|------|-----------------|---------------|------------|
| Performance | NFR-01 to NFR-05 | Pagination, indexed queries, cached rating/count, and scoped API DTOs are implemented | Needs final timing evidence |
| Security | NFR-06 to NFR-13, NFR-40 | Password hashing, JWT, role checks, validation, CORS settings, and externalised config are implemented | Needs Azure password rotation, scan cleanup, and vulnerability review |
| Usability | NFR-14 to NFR-18 | Backend supports frontend validation/error contracts | Needs frontend screenshots/usability result evidence |
| Accessibility | NFR-19 to NFR-23 | Frontend-owned requirement; backend supports image and validation data | Needs frontend Lighthouse/manual evidence |
| Reliability | NFR-24 to NFR-26 | Exception middleware, soft delete, and moderation/rating transaction behavior exist | Needs final dry-run evidence |
| Scalability | NFR-27 to NFR-29 | Indexes, denormalised ratings, and pagination exist | Needs migration/SQL evidence attached |
| Maintainability | NFR-30 to NFR-34 | Clean Architecture, DTOs, Fluent API, validators, and tests exist | Needs CI/coverage automation through TASK-23 |
| Data integrity | NFR-35 to NFR-39 | FKs, rating constraints, company-response uniqueness, and timestamps exist | Needs final migration/test evidence |

## 6. Jira Epic Backend Coverage

### Epic 1 - Tool/Service Catalogue and Rental System

| Jira Item | Backend/API Status | Remaining Work |
|----------|--------------------|----------------|
| MP-3 Homepage with Featured Categories | Implemented | Done by TASK-29 |
| MP-10 Category Browsing Page | Implemented | Done by TASK-29 |
| MP-13 Search for Tools/Service | Implemented | Done by TASK-29 |
| MP-15 Filter Tools/Service by Price Range | Implemented | Done by TASK-29 |
| MP-11 Tool/Service Detail Page | Implemented | Done by TASK-29 |
| MP-14 Rental Cost Calculator | Implemented | Done by TASK-29 |

### Epic 2 - Reviews, Ratings, and Community Interaction

| Jira Item | Backend/API Status | Remaining Work |
|----------|--------------------|----------------|
| MP-24 User Registration and Login | Implemented | Done by TASK-30 |
| MP-18 Submit a Review for a Tool/Service | Implemented | Done by TASK-30 |
| MP-19 Display Approved Reviews | Implemented | Done by TASK-30 |
| MP-20 Overall Tool/Service Rating | Implemented | Done by TASK-30 |
| MP-21 Comment on a Review | Implemented | Done by TASK-30 |
| MP-23 Company Response to a Review | Implemented; `Admin,Moderator` staff policy documented | Done by TASK-30 |
| MP-25 My Reviews Page | Implemented | Done by TASK-30 |

### Epic 3 - Back-Office Management and Moderation

| Jira Item | Backend/API Status | Remaining Work |
|----------|--------------------|----------------|
| MP-26 Admin Login and Role-Based Access | Implemented | Conditional TASK-17 only if full ASP.NET Identity is required |
| MP-31 Review Moderation Queue | Implemented | Done |
| MP-72 Approve or Reject Reviews | Implemented | Done |
| MP-27 Add New Equipment/Service | Implemented with `ImageUrl` create shortcut | TASK-26 if upload-backed first image is mandatory |
| MP-28 Edit Equipment/Service Details and Pricing | Implemented | Done |
| MP-29 Manage Tool/Service Images | Implemented for upload/delete after tool creation | TASK-26 only for first-image create alignment |
| MP-30 Deactivate or Remove Equipment/Service | Implemented with soft status changes | Done |
| MP-32 Manage Categories | Implemented | Done |
| MP-33 Admin Dashboard with Overview Statistics | Implemented | Done |

## 7. Closed Backend Gaps

| Gap | Closed By |
|-----|-----------|
| Missing Services category and remaining Epic 1 seed data | TASK-1 |
| Admin tools controller routes | TASK-2 |
| Admin moderation controller routes | TASK-3 |
| Image service and upload/delete endpoints | TASK-4 |
| Dashboard service and controller | TASK-5 |
| Moderation service methods | TASK-6 |
| Review seed data | TASK-9 |
| Company response approved-review enforcement | TASK-13 |
| Not-enough-reviews rating threshold | TASK-14 |
| Comment and company response seed data | TASK-15 |
| Admin category routing | TASK-16 |
| Review comment status index | TASK-18 |
| Admin tool management service methods | TASK-19 |
| FluentValidation request validation | TASK-20 |
| Review rating DB check constraints | TASK-21 |
| Baseline API integration tests | TASK-22 |
| Epic 1 public catalogue API contract coverage | TASK-29 |
| Admin list/detail read endpoints | TASK-25 |
| Item-level moderation queue and exact counts | TASK-27 |
| Epic 3 admin API integration coverage | TASK-28 |
| Epic 2 auth/reviews/community API contract coverage | TASK-30 |

## 8. Remaining Gap Backlog

| Gap ID | Task | Priority | Description | Done When |
|--------|------|----------|-------------|-----------|
| GAP-FLOW-3 | TASK-26 | Must if strict upload requirement applies | Admin create-tool first image currently uses `ImageUrl` in JSON rather than upload-backed multipart creation | Create flow accepts/links first uploaded image and tests prove required behavior |
| GAP-CI-1 | TASK-23 | Must before final sign-off | CI currently needs final coverage/automation alignment | CI runs scan, restore, build, unit tests, integration tests, and coverage |
| GAP-SEC-2 | Security backlog | Must before scan week | Secret scanner has false positives against its own pattern definitions | Scanner passes without weakening real secret detection |
| GAP-SEC-3 | Security backlog | Must before scan week | NuGet vulnerability review/update still required | Vulnerability scan is clean or accepted with documented mitigation |
| GAP-DEPLOY-1 | Deployment evidence | Must before final submission/demo | Azure App Service and database need final smoke-test proof | Health, public routes, admin auth, CORS, and migrations are verified on Azure |
| GAP-EVID-1 | Submission evidence | Must before final upload | Test plan exists but needs actual result evidence | Black-box, dry-run, usability, build/test, and screenshots/logs are attached |

## 9. Recommended Completion Sequence

1. Rotate Azure SQL password and complete the highest-priority security backlog items.
2. Complete TASK-26 if strict upload-backed first image creation is required.
3. Complete TASK-23 for CI, integration test execution, and coverage automation.
4. Run local build/test, migration checks, security scan, and package vulnerability scan.
5. Apply/verify Azure database migration state using rotated credentials.
6. Run Azure API smoke tests and CORS checks from the Next.js origin.
7. Attach final evidence to the submission pack and Jira tickets.

## 10. Evidence Files

| Evidence Area | File |
|---------------|------|
| Functional requirements | `docs/REQUIREMENTS-SPECIFICATION.md` |
| Non-functional requirements | `docs/NON-FUNCTIONAL-REQUIREMENTS.md` |
| Database design | `docs/DATABASE-DESIGN.md` |
| ERD | `docs/ERD.md` |
| Functional design diagrams | `docs/FUNCTIONAL-DESIGN-DIAGRAMS.md` |
| Test plan | `docs/TEST-PLAN.md` |
| Testing strategy | `docs/TESTING-STRATEGY.md` |
| Implementation task list | `docs/agile/IMPLEMENTATION-TASKS.md` |
| One-by-one sequence | `docs/agile/IMPLEMENTATION-SEQUENCE.md` |
| Security scan backlog | `docs/security/BACKEND-SECURITY-SCAN-2026-05-07.md` |
| Final completion report | `docs/PROJECT-COMPLETION-REPORT.md` |
