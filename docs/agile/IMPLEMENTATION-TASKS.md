# Implementation Tasks â€“ API Backend

> **Generated:** 22 April 2026
> **Source:** [GAP-ANALYSIS.md](../GAP-ANALYSIS.md)
> **Scope:** All outstanding backend implementation work from Epics 1, 2, and 3.
>
> Each task below links to the user story it supports. Copy any task into a conversation and ask to implement it.

---

## How to Use This File

1. Pick a task below.
2. Copy the full task block (including the sub-tasks) into a new conversation.
3. Ask: _"Implement this task following AGENTS.md and CLAUDE.md conventions."_
4. Tick it off when done.

---

## Epic 1 â€“ Tool/Service Catalogue & Rental Calculator

> Epic doc: [EPIC-1-CATALOGUE-AND-CALCULATOR.md](EPIC-1-CATALOGUE-AND-CALCULATOR.md)

### TASK-1: Seed missing catalogue categories and tools

**Links to:** [US-1.9 â€“ Database schema for tools and categories](EPIC-1-CATALOGUE-AND-CALCULATOR.md#us-19--database-schema-for-tools-and-categories)
**Priority:** Must | **Gap IDs:** GAP-SD-1, GAP-SD-2, GAP-SD-3

The product backlog lists 8 tool categories plus a Services category, but only 6 are seeded. Add the missing ones.

**Sub-tasks:**

- [x] **1.1** Create a new EF Core migration to seed the **Painting & Decorating** category (Id: 1007) with description and image URL
- [x] **1.2** Seed 3â€“4 tools under Painting & Decorating (e.g. Paint Sprayer, Wallpaper Steamer, Belt Sander, Heat Gun) with hourly/daily/weekly rates, images, and realistic descriptions
- [x] **1.3** Seed the **Plumbing & Drainage** category (Id: 1008) with description and image URL
- [x] **1.4** Seed 3â€“4 tools under Plumbing & Drainage (e.g. Pipe Freezing Kit, Drain Rod Set, Pipe Cutter, Plumber's Torch) with rates, images, and descriptions
- [x] **1.5** Seed a **Services** category (Id: 1009) with description "Non-physical hire services including delivery, operator hire, and compliance testing" and image URL
- [x] **1.6** Seed 3â€“4 service entries under Services (e.g. Equipment Delivery, Trained Operator Hire, PAT Testing Service, Site Survey) with hourly/daily/weekly rates, descriptions, and images
- [x] **1.7** Generate SQL deployment script: `dotnet ef migrations script <PreviousMigration> <NewMigration> --idempotent --output scripts/sql/<MigrationName>.sql --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API`
- [x] **1.8** Apply migration locally: `dotnet ef database update --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API`
- [x] **1.9** Run `dotnet build ReviewPortal.slnx` and `dotnet test ReviewPortal.slnx` â€” confirm no regressions

---

## Epic 2 â€“ Reviews, Ratings & Community Interaction

> Epic doc: [EPIC-2-REVIEWS-AND-RATINGS.md](EPIC-2-REVIEWS-AND-RATINGS.md)

### TASK-6: Implement moderation service methods (stubs â†’ real logic)

**Links to:** [US-2.1 â€“ Submit a review for a tool/service](EPIC-2-REVIEWS-AND-RATINGS.md#us-21--submit-a-review-for-a-toolservice) | [US-2.2 â€“ Display approved reviews on the tool/service page](EPIC-2-REVIEWS-AND-RATINGS.md#us-22--display-approved-reviews-on-the-toolservice-page) | [US-2.3 â€“ Overall tool/service ranking based on ratings](EPIC-2-REVIEWS-AND-RATINGS.md#us-23--overall-toolservice-ranking-based-on-ratings) | [US-2.4 â€“ Comment on a review](EPIC-2-REVIEWS-AND-RATINGS.md#us-24--comment-on-someone-elses-review)
**Also links to (Epic 3):** [US-3.6 â€“ Review moderation queue](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-36--review-moderation-queue) | [US-3.9 â€“ Admin API endpoints](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-39--admin-api-endpoints)
**Priority:** Must | **Gap IDs:** GAP-SVC-3, GAP-SVC-6, GAP-SVC-7

Three methods in `ReviewService.cs` are currently stubs that return error strings. Implement them fully.

**Sub-tasks:**

- [x] **6.1** Implement `GetPendingReviewsAsync` in `ReviewService`:
  - Query all reviews with `Status == Pending`, sorted oldest-first
  - Include pending comments in the response (or as a separate count)
  - Return paginated `PagedList<ReviewDto>` with tool name, reviewer name, date, text, ratings
- [x] **6.2** Implement `ModerateReviewAsync` in `ReviewService`:
  - Look up review by ID (return 404 if not found)
  - If approving: set `Status = Approved`
  - If rejecting: set `Status = Rejected` and store `RejectionReason`
  - After approval: recalculate `Tool.OverallRating` as the average of all approved reviews' `OverallRating` values
  - After approval: update `Tool.ReviewCount` to the count of approved reviews for that tool
  - After rejecting a previously approved review: recalculate rating and decrement count
  - Save changes via `_unitOfWork.SaveChangesAsync`
- [x] **6.3** Implement `ModerateCommentAsync` in `ReviewService`:
  - Look up comment by ID (return 404 if not found)
  - If approving: set `Status = Approved`
  - If rejecting: set `Status = Rejected` and store `RejectionReason`
  - Save changes
- [x] **6.4** Write unit tests for `GetPendingReviewsAsync` â€” empty queue, multiple pending items, pagination, sort order
- [x] **6.5** Write unit tests for `ModerateReviewAsync` â€” approve flow, reject flow, 404 case, tool rating recalculation, re-rejection of approved review
- [x] **6.6** Write unit tests for `ModerateCommentAsync` â€” approve flow, reject flow, 404 case
- [x] **6.7** Run `dotnet build ReviewPortal.slnx` and `dotnet test ReviewPortal.slnx` â€” confirm all tests pass

---

### TASK-9: Seed review data

**Links to:** [US-2.1 â€“ Submit a review for a tool/service](EPIC-2-REVIEWS-AND-RATINGS.md#us-21--submit-a-review-for-a-toolservice) | [US-2.2 â€“ Display approved reviews on the tool/service page](EPIC-2-REVIEWS-AND-RATINGS.md#us-22--display-approved-reviews-on-the-toolservice-page) | [US-2.3 â€“ Overall tool/service ranking based on ratings](EPIC-2-REVIEWS-AND-RATINGS.md#us-23--overall-toolservice-ranking-based-on-ratings)
**Priority:** Should | **Gap ID:** GAP-SD-4

No actual `Reviews` rows exist in the database. Sprint 1 tool detail pages rely on denormalised `OverallRating`/`ReviewCount` fields. Seed real review rows to back up those values.

**Sub-tasks:**

- [x] **9.1** Create a new EF Core migration to seed 6â€“8 reviews with `Status = Approved` across 3â€“4 different tools
- [x] **9.2** Include varied ratings per review (not all 5 stars) to demonstrate realistic aggregation
- [x] **9.3** Ensure the seeded `Tool.OverallRating` and `Tool.ReviewCount` values match the actual reviews seeded
- [x] **9.4** Include 1â€“2 reviews with `Status = Pending` to populate the moderation queue for demos
- [x] **9.5** Generate SQL script, apply migration locally
- [x] **9.6** Run `dotnet build` and `dotnet test` â€” confirm no regressions

---

### TASK-13: Fix company response â€” enforce approved reviews only

**Links to:** [US-2.5 â€“ Company response to a review](EPIC-2-REVIEWS-AND-RATINGS.md#us-25--company-response-to-a-review)
**Priority:** Should | **Gap ID:** GAP-SVC-4

`AddCompanyResponseAsync` does NOT validate that the review is approved. Staff can currently respond to Pending or Rejected reviews, which breaks the moderation workflow.

**Sub-tasks:**

- [x] **13.1** In `ReviewService.AddCompanyResponseAsync`, after the `review is null` check, add: if `review.Status != ReviewStatus.Approved`, return `Result<CompanyResponseDto>.Failure("Company responses can only be added to approved reviews.")`
- [x] **13.2** Write unit test: attempting to add a response to a Pending review returns a validation failure
- [x] **13.3** Write unit test: attempting to add a response to a Rejected review returns a validation failure
- [x] **13.4** Write unit test: adding a response to an Approved review still works as before
- [x] **13.5** Run `dotnet build` and `dotnet test` â€” confirm all tests pass

---

### TASK-14: Add "Not enough reviews" threshold to tool DTOs

**Links to:** [US-2.3 â€“ Overall tool/service ranking based on ratings](EPIC-2-REVIEWS-AND-RATINGS.md#us-23--overall-toolservice-ranking-based-on-ratings)
**Priority:** Should | **Gap ID:** GAP-SVC-5

US-2.3 AC says: "If there are fewer than 2 reviews, 'Not enough reviews to rate' is shown instead of a number." Check if this is enforced in the API response DTOs.

**Sub-tasks:**

- [x] **14.1** Checked `ToolDto` and `ToolSummaryDto`; the existing equivalent flag is `HasEnoughReviewsToRate`
- [x] **14.2** No DTO rename was required because `HasEnoughReviewsToRate` already satisfies the `ReviewCount >= 2` requirement in both DTOs
- [x] **14.3** Confirmed `ToolService.GetToolByIdAsync` and `GetToolsByCategoryAsync` populate the flag/message, and tightened the threshold logic to use `ReviewCount >= 2` directly
- [x] **14.4** Unit tests now explicitly cover 0 reviews â†’ `false`, while existing tests cover 1 review â†’ `false` and 2+ reviews â†’ `true`
- [x] **14.5** Run `dotnet build` and `dotnet test` â€” confirm all tests pass

---

### TASK-15: Seed comments and company responses

**Links to:** [US-2.4 â€“ Comment on a review](EPIC-2-REVIEWS-AND-RATINGS.md#us-24--comment-on-someone-elses-review) | [US-2.5 â€“ Company response to a review](EPIC-2-REVIEWS-AND-RATINGS.md#us-25--company-response-to-a-review)
**Priority:** Could | **Gap ID:** GAP-SD-5

No demo data for comments or company responses.

**Sub-tasks:**

- [x] **15.1** Added a new seed migration with 3 approved comments attached to seeded approved reviews
- [x] **15.2** Seeded 2 `CompanyResponse` rows from the Admin seed user (Id: 2)
- [x] **15.3** Seeded 1 pending comment and updated the moderation queue query so pending comments on approved reviews appear in the admin queue
- [x] **15.4** Generate SQL script, apply migration, run tests

---

### TASK-17: Align auth stack with ASP.NET Identity requirement

**Links to:** [US-2.7 - User registration and login](EPIC-2-REVIEWS-AND-RATINGS.md#us-27--user-registration-and-login) | [REQUIREMENTS-SPECIFICATION](../REQUIREMENTS-SPECIFICATION.md) | [NON-FUNCTIONAL-REQUIREMENTS](../NON-FUNCTIONAL-REQUIREMENTS.md)
**Priority:** Conditional | **Gap IDs:** GAP-AUTH-1, GAP-AUTH-2, GAP-AUTH-3, GAP-AUTH-4, GAP-AUTH-5

The current implementation uses a custom `AuthService`, `IUserRepository`, `IPasswordHasher`, and hand-managed password reset flow. JWT bearer authentication is configured, and passwords are hashed through an ASP.NET Core `PasswordHasher<TUser>`-compatible implementation. The project is not wired to full ASP.NET Identity for user management, token providers, role stores, or EF-backed Identity tables. Do not implement this refactor unless the brief or assessor explicitly requires full ASP.NET Identity rather than the current custom JWT flow.

**Gap summary:**

- `AppDbContext` inherits from `DbContext`, not `IdentityDbContext`
- `User` is a custom entity, not an Identity user/store model
- DI registers `IJwtProvider` and a password hasher, but does not call `AddIdentity` / `AddIdentityCore`
- `AuthService` performs registration, login, password checks, and reset-token generation manually instead of via `UserManager` / `SignInManager`
- No Identity schema migration or SQL deployment script exists for the required auth tables/columns

**Sub-tasks:**

- [ ] **17.1** Add the required Identity packages and convert the auth persistence model to an ASP.NET Identity-compatible user type (either adapt `User` or introduce an `ApplicationUser` mapping strategy)
- [ ] **17.2** Update `AppDbContext` to use Identity EF stores and configure ASP.NET Identity in DI with the project password policy (minimum 8 chars, one number, one uppercase)
- [ ] **17.3** Refactor `AuthService` to use `UserManager` for registration, password verification, password changes, and password resets while keeping JWT token issuance for API auth
- [ ] **17.4** Replace manual reset-token generation/storage with Identity token providers or document and justify any compatible hybrid design if the schema must remain partially custom
- [ ] **17.5** Ensure role handling (`Customer`, `Admin`, `Moderator`) is supported through Identity role claims and that existing authorization attributes continue to work
- [ ] **17.6** Create the required EF Core migration and SQL deployment script for the Identity schema changes, and update seed/test-user scripts if the user table shape changes
- [ ] **17.7** Update unit and integration tests to cover Identity-backed registration, login, password change, forgot-password, reset-password, and JWT claim generation
- [ ] **17.8** Run `dotnet build ReviewPortal.slnx` and `dotnet test ReviewPortal.slnx` and confirm all auth flows still work through the Web API

### TASK-18: Add missing ReviewComments status index

**Links to:** [US-2.9 - Review database schema](EPIC-2-REVIEWS-AND-RATINGS.md#us-29--review-database-schema)
**Priority:** Must | **Gap ID:** GAP-DB-1

US-2.9 requires indexes on ReviewComments.ReviewId and ReviewComments.Status, but the current EF configuration and initial migration only create the ReviewId index.

**Sub-tasks:**

- [x] **18.1** Add HasIndex(rc => rc.Status) to ReviewCommentConfiguration
- [x] **18.2** Create a new EF Core migration and SQL deployment script for the missing IX_ReviewComments_Status index
- [x] **18.3** Add or update schema tests to verify the migration/configuration includes the ReviewComments.Status index
- [x] **18.4** Apply the migration locally with dotnet ef database update
- [x] **18.5** Run dotnet build and dotnet test - confirm all tests pass

---
## Epic 3 â€“ Back-Office Management & Moderation

> Epic doc: [EPIC-3-BACKOFFICE-AND-MODERATION.md](EPIC-3-BACKOFFICE-AND-MODERATION.md)

### TASK-2: Create AdminToolsController

**Links to:** [US-3.2 â€“ Add new equipment or service to the catalogue](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-32--add-new-equipment-or-service-to-the-catalogue) | [US-3.3 â€“ Edit existing equipment/service details and pricing](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-33--edit-existing-equipmentservice-details-and-pricing) | [US-3.5 â€“ Deactivate or remove equipment/service](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-35--deactivate-or-remove-equipmentservice) | [US-3.9 â€“ Admin API endpoints](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-39--admin-api-endpoints)
**Priority:** Must | **Gap IDs:** GAP-API-1, GAP-API-2, GAP-API-3

Service methods `CreateToolAsync`, `UpdateToolAsync`, and `SetToolStatusAsync` are exposed via `IToolService`, but `ToolService` still returns stub failures and there is no admin controller route yet. TASK-19 covers the underlying service implementation and the missing create-tool flow details; this task exposes the admin routes once that logic exists.

**Sub-tasks:**

- [x] **2.1** Create `src/ReviewPortal.API/Controllers/Admin/AdminToolsController.cs`
- [x] **2.2** Route: `[Route("api/admin/tools")]`, class-level `[Authorize(Roles = "Admin")]`
- [x] **2.3** `[HttpPost]` â†’ calls `_toolService.CreateToolAsync(request)` â†’ returns `201 Created` with the created tool
- [x] **2.4** `[HttpPut("{id:int}")]` â†’ calls `_toolService.UpdateToolAsync(id, request)` â†’ returns `200 OK`
- [x] **2.5** `[HttpPatch("{id:int}/status")]` â†’ accepts `{ "isActive": true/false }` â†’ calls `_toolService.SetToolStatusAsync(id, isActive)` â†’ returns `200 OK`
- [x] **2.6** Register the controller in DI if needed (ASP.NET auto-discovers by convention)
- [x] **2.7** Write unit tests: Create success, Create validation failure, Update success, Update 404, Status change success, Status change 404, Unauthorized (no token), Forbidden (Customer role)
- [x] **2.8** Run `dotnet build` and `dotnet test` â€” confirm all tests pass

---

### TASK-3: Create AdminModerationController

**Links to:** [US-3.6 â€“ Review moderation queue](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-36--review-moderation-queue) | [US-3.9 â€“ Admin API endpoints](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-39--admin-api-endpoints)
**Priority:** Must | **Gap IDs:** GAP-API-6, GAP-API-7, GAP-API-8

Service methods `GetPendingReviewsAsync`, `ModerateReviewAsync`, and `ModerateCommentAsync` exist in `IReviewService` but have no admin route. (Note: TASK-6 implements the service logic; this task creates the controller.)

**Sub-tasks:**

- [x] **3.1** Create `src/ReviewPortal.API/Controllers/Admin/AdminModerationController.cs`
- [x] **3.2** Route: `[Route("api/admin/moderation")]`, class-level `[Authorize(Roles = "Admin,Moderator")]`
- [x] **3.3** `[HttpGet("pending")]` with `[FromQuery] int page = 1, int pageSize = 20` â†’ calls `_reviewService.GetPendingReviewsAsync(page, pageSize)` â†’ returns `200 OK`
- [x] **3.4** `[HttpPut("reviews/{id:int}")]` with `[FromBody] ModerateReviewRequest` â†’ calls `_reviewService.ModerateReviewAsync(id, request)` â†’ returns `200 OK`
- [x] **3.5** `[HttpPut("comments/{id:int}")]` with `[FromBody] ModerateReviewRequest` â†’ calls `_reviewService.ModerateCommentAsync(id, request)` â†’ returns `200 OK`
- [x] **3.6** Write unit tests: Get pending success, Get pending empty, Approve review, Reject review with reason, Approve comment, Reject comment, Unauthorized, Forbidden
- [x] **3.7** Run `dotnet build` and `dotnet test` â€” confirm all tests pass

---

### TASK-4: Implement ImageService and image endpoints

**Links to:** [US-3.4 â€“ Manage tool/service images](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-34--manage-toolservice-images) | [US-3.9 â€“ Admin API endpoints](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-39--admin-api-endpoints)
**Priority:** Must | **Gap IDs:** GAP-SVC-2, GAP-API-4, GAP-API-5

No image upload or delete functionality exists.

**Sub-tasks:**

- [x] **4.1** Create `src/ReviewPortal.Application/Interfaces/IImageService.cs` with methods:
  - `Task<Result<ToolImageDto>> UploadImageAsync(int toolId, Stream fileStream, string fileName, CancellationToken)`
  - `Task<Result<bool>> DeleteImageAsync(int toolId, int imageId, CancellationToken)`
- [x] **4.2** Create `src/ReviewPortal.Infrastructure/Services/ImageService.cs` implementing `IImageService`
- [x] **4.3** Upload validation: only allow `.jpg`, `.jpeg`, `.png`, `.webp`; max 5MB file size
- [x] **4.4** Upload storage: save files to a local `uploads/tools/` folder (configurable path in `appsettings.json`)
- [x] **4.5** Create a `ToolImage` record in the database with the file path, set `DisplayOrder` to max + 1
- [x] **4.6** Delete logic: look up image by `toolId` + `imageId`; if tool has only 1 image, return error "Cannot delete the last image"
- [x] **4.7** Delete the file from disk and remove the `ToolImage` record
- [x] **4.8** Register `IImageService` in DI (`Program.cs` or `DependencyInjection.cs`)
- [x] **4.9** Add endpoints to `AdminToolsController`:
  - `[HttpPost("{id:int}/images")]` accepting `IFormFile` â†’ calls `UploadImageAsync`
  - `[HttpDelete("{id:int}/images/{imageId:int}")]` â†’ calls `DeleteImageAsync`
- [x] **4.10** Write unit tests: upload success, upload invalid format (returns 400), upload too large (returns 400), delete success, delete last image (returns 400), delete image not found (returns 404)
- [x] **4.11** Run `dotnet build` and `dotnet test` â€” confirm all tests pass

---

### TASK-5: Implement DashboardService and controller

**Links to:** [US-3.8 â€“ Admin dashboard with overview stats](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-38--admin-dashboard-with-overview-stats) | [US-3.9 â€“ Admin API endpoints](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-39--admin-api-endpoints)
**Priority:** Could | **Gap IDs:** GAP-SVC-1, GAP-API-9

No dashboard service or controller exists.

**Sub-tasks:**

- [x] **5.1** Create `src/ReviewPortal.Application/DTOs/Dashboard/DashboardStatsDto.cs` with properties:
  - `int TotalActiveTools`, `int TotalInactiveTools`
  - `int PendingModerationCount`
  - `int ReviewsPublishedThisMonth`
  - `IReadOnlyList<ToolRankingDto> TopRatedTools` (top 5)
  - `IReadOnlyList<ToolRankingDto> MostReviewedTools` (top 5)
- [x] **5.2** Create `ToolRankingDto` record: `int ToolId`, `string ToolName`, `decimal? OverallRating`, `int ReviewCount`
- [x] **5.3** Create `src/ReviewPortal.Application/Interfaces/IDashboardService.cs` with `Task<Result<DashboardStatsDto>> GetDashboardStatsAsync(CancellationToken)`
- [x] **5.4** Create `src/ReviewPortal.Application/Services/DashboardService.cs` implementing `IDashboardService`
  - Query `Tools` for active/inactive counts
  - Query pending reviews plus pending comments and return a combined moderation count
  - Query `Reviews` with `Status == Approved` and `CreatedDate` in current month for monthly count
  - Query `Tools` ordered by `OverallRating` desc (where `ReviewCount >= 2`) for top rated
  - Query `Tools` ordered by `ReviewCount` desc for most reviewed
- [x] **5.5** Register `IDashboardService` in DI
- [x] **5.6** Create `src/ReviewPortal.API/Controllers/Admin/AdminDashboardController.cs`
  - Route: `[Route("api/admin/dashboard")]`, `[Authorize(Roles = "Admin")]`
  - `[HttpGet("stats")]` â†’ calls `GetDashboardStatsAsync` â†’ returns `200 OK`
- [x] **5.7** Write unit tests for `DashboardService`: all-zero case, mixed data, top 5 ordering, monthly boundary
- [x] **5.8** Write unit tests for `AdminDashboardController`: success, unauthorised
- [x] **5.9** Run `dotnet build` and `dotnet test` â€” confirm all tests pass

---

### TASK-16: Decide on admin category routing

**Links to:** [US-3.7 â€“ Manage categories](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-37--manage-categories) | [US-3.9 â€“ Admin API endpoints](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-39--admin-api-endpoints)
**Priority:** Could | **Gap ID:** GAP-API-10

Category CRUD previously existed on the public `CategoriesController` with `[Authorize(Roles = "Admin")]`. US-3.9 specifies `/api/admin/categories`.

**Decision:** Use a separate `AdminCategoriesController` at `/api/admin/categories` for admin-only create, update, and delete operations. Keep `CategoriesController` read-only for public catalogue browsing under `/api/categories`.

**Sub-tasks:**

- [x] **16.1** Decide: keep the current pattern (public route with auth) or create a separate `AdminCategoriesController` at `/api/admin/categories`
- [x] **16.2** If separate: create controller, move admin-only methods (POST, PUT, DELETE), leave public GET methods on existing controller
- [x] **16.3** Document the decision in `PRODUCT-BACKLOG.md` or this file

---

## Cross-Cutting Technical Quality & Automation

### TASK-19: Implement admin tool management service methods and creation flow

**Links to:** [US-3.2 - Add new equipment or service to the catalogue](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-32--add-new-equipment-or-service-to-the-catalogue) | [US-3.3 - Edit existing equipment/service details and pricing](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-33--edit-existing-equipmentservice-details-and-pricing) | [US-3.5 - Deactivate or remove equipment/service](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-35--deactivate-or-remove-equipmentservice) | [US-3.9 - Admin API endpoints](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-39--admin-api-endpoints)
**Priority:** Must | **Gap IDs:** GAP-SVC-8, GAP-FLOW-1

`IToolService` exposes admin mutation methods. The first-save image flow is handled by requiring `CreateToolRequest.ImageUrl`, which creates the first `ToolImage` record in the same create operation. Additional images are managed through the admin image upload endpoints from TASK-4.

**Sub-tasks:**

- [x] **19.1** Implement `CreateToolAsync`, `UpdateToolAsync`, and `SetToolStatusAsync` in `ToolService` instead of returning stub failures
- [x] **19.2** Add validation for category existence, required text fields, non-negative/positive hire rates, and deposit amount consistency
- [x] **19.3** Decide and implement the first-save image flow so a tool/service cannot be successfully created without at least one image (for example: initial image payload, or draft tool plus first-image transaction), then update DTOs/controllers accordingly
- [x] **19.4** Ensure create returns a fully populated tool DTO, update returns refreshed data, and deactivate/reactivate uses `IsActive` without hard delete
- [x] **19.5** Add unit tests for create/update/status success, validation failures, not-found cases, and public-query behaviour for inactive tools
- [x] **19.6** Run `dotnet build` and `dotnet test` - confirm all tests pass

---

### TASK-20: Introduce FluentValidation for API request models

**Links to:** [US-1.5 - Rental cost calculator](EPIC-1-CATALOGUE-AND-CALCULATOR.md#us-15--rental-cost-calculator) | [US-2.1 - Submit a review for a tool/service](EPIC-2-REVIEWS-AND-RATINGS.md#us-21--submit-a-review-for-a-toolservice) | [US-2.4 - Comment on a review](EPIC-2-REVIEWS-AND-RATINGS.md#us-24--comment-on-someone-elses-review) | [US-2.5 - Company response to a review](EPIC-2-REVIEWS-AND-RATINGS.md#us-25--company-response-to-a-review) | [US-2.7 - User registration and login](EPIC-2-REVIEWS-AND-RATINGS.md#us-27--user-registration-and-login) | [US-3.2 - Add new equipment or service to the catalogue](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-32--add-new-equipment-or-service-to-the-catalogue) | [US-3.6 - Review moderation queue](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-36--review-moderation-queue)
**Also links to:** [NON-FUNCTIONAL-REQUIREMENTS](../NON-FUNCTIONAL-REQUIREMENTS.md)
**Priority:** Must | **Gap ID:** GAP-VAL-1

`AGENTS.md`, `CLAUDE.md`, and the non-functional requirements all say server-side validation should use FluentValidation, but there are currently no validators, no FluentValidation package references, and no validator registration in the API.

**Sub-tasks:**

- [x] **20.1** Add FluentValidation packages and register validators in the API startup/composition root
- [x] **20.2** Create validators for auth requests (`Register`, `Login`, `ChangePassword`, `ForgotPassword`, `ResetPassword`)
- [x] **20.3** Create validators for catalogue and review requests (`RentalCalculation`, `CreateReview`, `CreateComment`, `CreateCompanyResponse`, `ModerateReview`, `CreateCategory`, `UpdateCategory`, `CreateTool`, `UpdateTool`)
- [x] **20.4** Move request-shape validation out of services into validators while keeping cross-entity/business-rule validation in the service layer
- [x] **20.5** Add validator unit tests for every rule and at least one API/integration test that invalid payloads return consistent 400 validation responses
- [x] **20.6** Run `dotnet build` and `dotnet test` - confirm all tests pass

---

### TASK-21: Add DB check constraints for review rating values

**Links to:** [US-2.9 - Review database schema](EPIC-2-REVIEWS-AND-RATINGS.md#us-29--review-database-schema)
**Also links to:** [NON-FUNCTIONAL-REQUIREMENTS](../NON-FUNCTIONAL-REQUIREMENTS.md)
**Priority:** Must | **Gap ID:** GAP-DB-2

The five review rating columns are validated in code only. The database schema still allows out-of-range values because there are no `CHECK` constraints protecting `EquipmentRating`, `CustomerServiceRating`, `TechnicalSupportRating`, `AfterSalesRating`, and `ValueForMoneyRating`.

**Sub-tasks:**

- [x] **21.1** Add EF Core check constraint(s) enforcing rating values in the range `1` to `5` for all five review rating columns
- [x] **21.2** Create a new EF Core migration and SQL deployment script for the rating constraints
- [x] **21.3** Update `docs/ERD.md` (and any schema notes) to document the DB-level rating constraints
- [x] **21.4** Add schema/integration tests proving out-of-range review inserts fail at the database level
- [x] **21.5** Apply the migration locally and run `dotnet build` / `dotnet test`

---

### TASK-22: Add real API integration tests for critical backend flows

**Links to:** [US-1.8 - API endpoints for tools and categories](EPIC-1-CATALOGUE-AND-CALCULATOR.md#us-18--api-endpoints-for-tools-and-categories) | [US-2.6 - Review API endpoints](EPIC-2-REVIEWS-AND-RATINGS.md#us-26--review-api-endpoints) | [US-2.7 - User registration and login](EPIC-2-REVIEWS-AND-RATINGS.md#us-27--user-registration-and-login) | [US-2.8 - My reviews page](EPIC-2-REVIEWS-AND-RATINGS.md#us-28--my-reviews-page) | [US-3.6 - Review moderation queue](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-36--review-moderation-queue) | [US-3.9 - Admin API endpoints](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-39--admin-api-endpoints)
**Priority:** Must | **Gap ID:** GAP-QA-1

The current `ReviewPortal.IntegrationTests` project only checks JWT generation, password hashing, and the DbContext model. It does not exercise the real ASP.NET Core HTTP pipeline, routing, auth filters, or JSON responses via `WebApplicationFactory`.

**Sub-tasks:**

- [x] **22.1** Create a `WebApplicationFactory<Program>`-based test host with isolated SQL Server/LocalDB or Testcontainers setup and deterministic data reset
- [x] **22.2** Add public API integration tests for categories, category tools, search, tool detail, and rental calculation
- [x] **22.3** Add auth integration tests for register, login, `GET /api/auth/me`, change-password, forgot-password, and reset-password
- [x] **22.4** Add review lifecycle tests covering submit review -> moderation queue -> approve/reject -> approved list / my reviews visibility
- [x] **22.5** Add admin authorization tests for 401/403 behaviour on admin tools, moderation, and dashboard routes
- [x] **22.6** Run the integration suite through `dotnet test ReviewPortal.slnx`

---

### TASK-23: Expand CI/CD and coverage automation

**Links to:** [US-1.10 - Project scaffolding and CI pipeline](EPIC-1-CATALOGUE-AND-CALCULATOR.md#us-110--project-scaffolding-and-ci-pipeline) | [US-3.10 - Playwright end-to-end tests for critical flows](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-310--playwright-end-to-end-tests-for-critical-flows)
**Also links to:** [TESTING-STRATEGY](../TESTING-STRATEGY.md) | [NON-FUNCTIONAL-REQUIREMENTS](../NON-FUNCTIONAL-REQUIREMENTS.md)
**Priority:** Must | **Gap IDs:** GAP-CI-1, GAP-COV-1

GitHub Actions currently runs unit tests only, only on PRs to `main` and pushes to `main`, and does not publish coverage or run integration tests. The docs also describe Playwright/browser automation that does not currently exist in this backend-only repo.

**Sub-tasks:**

- [ ] **23.1** Update the workflow triggers so CI runs on every push and pull request, or explicitly document a narrower branch strategy if that is intentional
- [ ] **23.2** Add integration-test execution to the GitHub Actions workflow
- [ ] **23.3** Collect and publish coverage reports from unit and integration test projects using Coverlet
- [ ] **23.4** Add enforceable minimum coverage thresholds aligned with the testing strategy instead of claiming an arbitrary 100% global target
- [ ] **23.5** Reconcile the workflow and testing docs with the backend-only repo scope: either add the missing browser automation in the appropriate repo or clearly mark Playwright as out of scope here
- [ ] **23.6** Validate the updated workflow and document how to run the same checks locally

---

### TASK-24: Remove committed secrets and externalise environment configuration

**Links to:** [README](../../README.md) | [DEPLOYMENT-TO-AZURE-APP-SERVICE](../DEPLOYMENT-TO-AZURE-APP-SERVICE.md)
**Also links to:** [NON-FUNCTIONAL-REQUIREMENTS](../NON-FUNCTIONAL-REQUIREMENTS.md)
**Priority:** Must | **Gap ID:** GAP-SEC-1

`appsettings.Development.json` currently contains an Azure SQL connection string with credentials and a hard-coded JWT secret, which conflicts with the repository guidance that secrets must not be stored in source control.

**Sub-tasks:**

- [ ] **24.1** Remove tracked secrets from `src/ReviewPortal.API/appsettings.Development.json` and keep only safe placeholders or sample values
- [ ] **24.2** Rotate the exposed Azure SQL password and any other leaked secrets before further use
- [ ] **24.3** Move local/development secrets to user secrets or environment variables and update setup/deployment docs accordingly
- [ ] **24.4** Add a lightweight secret-scanning step or documented pre-commit/CI check to reduce repeat leaks
- [ ] **24.5** Smoke-test local startup with configuration coming from user secrets/environment variables

---

## Documentation Tasks

> These update the agile docs only â€” no code changes.

### TASK-7: Update user story wording â€” tool â†’ tool/service

**Links to:** [EPIC-1](EPIC-1-CATALOGUE-AND-CALCULATOR.md) | [EPIC-2](EPIC-2-REVIEWS-AND-RATINGS.md) | [EPIC-3](EPIC-3-BACKOFFICE-AND-MODERATION.md) | [PRODUCT-BACKLOG](PRODUCT-BACKLOG.md) | [SPRINT-PLANNING](SPRINT-PLANNING.md)
**Priority:** Must | **Gap ID:** GAP-DOC-1

**Sub-tasks:**

- [x] **7.1** In EPIC-1, update user-facing story text to say "tool/service" instead of just "tool" (keep `Tool` as entity/class name in technical references)
- [x] **7.2** In EPIC-2, update user-facing story text: "Submit a review for a tool/service", "Display approved reviews on the tool/service page", etc.
- [x] **7.3** In EPIC-3, update user-facing story text: "Add new equipment or service", "Edit existing equipment/service", etc.
- [x] **7.4** In PRODUCT-BACKLOG.md, update the Epics table and Tool Categories section to mention services
- [x] **7.5** In SPRINT-PLANNING.md, update story titles in the sprint tables
- [x] **7.6** Do NOT change entity names, class names, variable names, or controller names â€” only docs wording

---

### TASK-8: Fix mandatory moderation wording

**Links to:** [US-2.1 â€“ Submit a review for a tool/service](EPIC-2-REVIEWS-AND-RATINGS.md#us-21--submit-a-review-for-a-toolservice) | [US-2.4 â€“ Comment on a review](EPIC-2-REVIEWS-AND-RATINGS.md#us-24--comment-on-someone-elses-review)
**Also links to:** [REQUIREMENTS-SPECIFICATION](../REQUIREMENTS-SPECIFICATION.md)
**Priority:** Must | **Gap ID:** GAP-DOC-5

**Sub-tasks:**

- [x] **8.1** Search all docs for "if required by the system" or similar conditional moderation language
- [x] **8.2** Replace with: "All customer reviews and comments require moderation before publication"
- [x] **8.3** In US-2.1 AC, ensure it clearly states: "The review is saved with a status of 'Pending' and is not visible to other customers until approved by a moderator"
- [x] **8.4** In US-2.4 AC, ensure it clearly states: "Comments go through moderation (status = Pending) before being visible"
- [x] **8.5** In REQUIREMENTS-SPECIFICATION FR-21, confirm wording matches

---

### TASK-10: Fix vague acceptance criteria

**Links to:** [EPIC-1](EPIC-1-CATALOGUE-AND-CALCULATOR.md) | [EPIC-2](EPIC-2-REVIEWS-AND-RATINGS.md) | [EPIC-3](EPIC-3-BACKOFFICE-AND-MODERATION.md)
**Priority:** Should | **Gap ID:** GAP-DOC-2

**Sub-tasks:**

- [x] **10.1** Audit all acceptance criteria in EPIC-1 for vague language ("clear", "efficient", "data integrity is maintained", "works properly")
- [x] **10.2** Rewrite each to a measurable, testable criterion, e.g.:
  - ~~"data integrity is maintained"~~ â†’ "Saving a tool/service with a missing required field returns HTTP 400 with a validation error listing the missing field(s)"
  - ~~"works properly on mobile"~~ â†’ "All catalogue pages render without horizontal scrolling on viewports 375px wide and above"
- [x] **10.3** Audit and fix EPIC-2 acceptance criteria
- [x] **10.4** Audit and fix EPIC-3 acceptance criteria
- [x] **10.5** Ensure no AC uses subjective adjectives without a measurable benchmark

---

### TASK-11: Add requirements traceability section

**Links to:** [PRODUCT-BACKLOG](PRODUCT-BACKLOG.md) | [REQUIREMENTS-SPECIFICATION](../REQUIREMENTS-SPECIFICATION.md)
**Priority:** Should | **Gap ID:** GAP-DOC-3

**Sub-tasks:**

- [x] **11.1** Add a new "Requirements Traceability" section to `REQUIREMENTS-SPECIFICATION.md` (or `PRODUCT-BACKLOG.md`)
- [x] **11.2** Map each scenario requirement from the project brief to its backlog user story
- [x] **11.3** Include: tool/service categories â†’ US-1.9, review categories â†’ US-2.1/US-2.9, rating aggregation â†’ US-2.3, moderation â†’ US-3.6, pricing â†’ US-1.5
- [x] **11.4** Cross-reference to the Requirements Traceability Matrix in `GAP-ANALYSIS.md` Â§2

---

### TASK-12: Add explicit definitions section

**Links to:** [PRODUCT-BACKLOG](PRODUCT-BACKLOG.md)
**Priority:** Should | **Gap ID:** GAP-DOC-4

**Sub-tasks:**

- [x] **12.1** Add or expand a "Design Decisions" or "Definitions" section in `PRODUCT-BACKLOG.md`
- [x] **12.2** Define the chosen tool/service categories (all 9 including Services)
- [x] **12.3** Define the chosen review categories (5 listed with descriptions)
- [x] **12.4** Define the rating aggregation method: `Overall = (Equipment + Customer + Technical + AfterSales + Value) / 5`, only approved reviews, cached on Tool entity
- [x] **12.5** Define service handling: unified `Tool` model, Services category, no separate logic
- [x] **12.6** Define moderation rules: reviews and comments start as Pending, admin approve/reject with reason, company responses bypass moderation, rejection criteria (offensive/irrelevant/spam)
- [x] **12.7** Define pricing logic: hourly/daily/weekly rates, cheapest combination calculation, date validation, cost breakdown format

---

## Task Dependency Order

```
TASK-1  (Seed categories)
  â”œâ”€â”€â†’ TASK-2  (AdminToolsController)
  â”‚      â””â”€â”€â†’ TASK-4  (ImageService)
  â”œâ”€â”€â†’ TASK-6  (Moderation service methods)
  â”‚      â””â”€â”€â†’ TASK-3  (AdminModerationController)
  â”œâ”€â”€â†’ TASK-5  (DashboardService)
  â””â”€â”€â†’ TASK-9  (Seed reviews)
         â””â”€â”€â†’ TASK-15 (Seed comments/responses)

TASK-13 (Fix company response) â€” independent, do anytime
TASK-14 (Review threshold)     â€” independent, do anytime
TASK-17 (Auth alignment)       - conditional decision gate only if full ASP.NET Identity is mandatory
TASK-18 (Comment status index) - independent, do anytime
TASK-19 (Tool service logic)   - complete; create requires initial `ImageUrl`, additional images use TASK-4 endpoints
TASK-20 (FluentValidation)     -> supports TASK-17 and TASK-19
TASK-21 (Rating DB checks)     -> pair with TASK-18
TASK-22 (API integration)      -> after core API slices are stable
TASK-23 (CI + coverage)        -> after TASK-22
TASK-24 (Secret cleanup)       - immediate, independent

TASK-7  (Docs: tool/service wording) â€” independent
TASK-8  (Docs: moderation wording)   â€” independent
TASK-10 (Docs: vague ACs)            â€” independent
TASK-11 (Docs: traceability)         â€” independent
TASK-12 (Docs: definitions)          â€” independent
TASK-16 (Admin category routing)     â€” independent
```

---

## Progress Tracker

| Task | Status | Date Completed |
|------|--------|---------------|
| TASK-1: Seed missing categories | Done | 2026-04-22 |
| TASK-2: AdminToolsController | Done | 2026-04-26 |
| TASK-3: AdminModerationController | Done | 2026-04-22 |
| TASK-4: ImageService | Done | 2026-05-05 |
| TASK-5: DashboardService | Done | 2026-05-05 |
| TASK-6: Moderation service methods | Done | 2026-04-22 |
| TASK-7: Docs â€“ tool/service wording | Done | 2026-04-23 |
| TASK-8: Docs â€“ moderation wording | Done | 2026-04-23 |
| TASK-9: Seed review data | Done | 2026-04-22 |
| TASK-10: Docs â€“ vague ACs | Done | 2026-04-23 |
| TASK-11: Docs â€“ traceability | Done | 2026-04-23 |
| TASK-12: Docs â€“ definitions | Done | 2026-04-23 |
| TASK-13: Fix company response | Done | 2026-04-23 |
| TASK-14: Review threshold | Done | 2026-04-23 |
| TASK-15: Seed comments/responses | Done | 2026-04-23 |
| TASK-17: Identity auth alignment | Conditional / decision gate | |
| TASK-18: Review comment status index | Done | 2026-04-26 |
| TASK-19: Admin tool service logic | Done | 2026-05-05 |
| TASK-20: FluentValidation adoption | Done | 2026-05-05 |
| TASK-21: Review rating DB constraints | Done | 2026-04-26 |
| TASK-22: API integration tests | Done | 2026-05-05 |
| TASK-23: CI and coverage automation | â¬œ Not started | |
| TASK-24: Secret cleanup and config externalisation | â¬œ Not started | |
| TASK-16: Admin category routing | Done | 2026-05-05 |

