# Implementation Tasks – API Backend

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

## Epic 1 – Tool/Service Catalogue & Rental Calculator

> Epic doc: [EPIC-1-CATALOGUE-AND-CALCULATOR.md](EPIC-1-CATALOGUE-AND-CALCULATOR.md)

### TASK-1: Seed missing catalogue categories and tools

**Links to:** [US-1.9 – Database schema for tools and categories](EPIC-1-CATALOGUE-AND-CALCULATOR.md#us-19--database-schema-for-tools-and-categories)
**Priority:** Must | **Gap IDs:** GAP-SD-1, GAP-SD-2, GAP-SD-3

The product backlog lists 8 tool categories plus a Services category, but only 6 are seeded. Add the missing ones.

**Sub-tasks:**

- [x] **1.1** Create a new EF Core migration to seed the **Painting & Decorating** category (Id: 1007) with description and image URL
- [x] **1.2** Seed 3–4 tools under Painting & Decorating (e.g. Paint Sprayer, Wallpaper Steamer, Belt Sander, Heat Gun) with hourly/daily/weekly rates, images, and realistic descriptions
- [x] **1.3** Seed the **Plumbing & Drainage** category (Id: 1008) with description and image URL
- [x] **1.4** Seed 3–4 tools under Plumbing & Drainage (e.g. Pipe Freezing Kit, Drain Rod Set, Pipe Cutter, Plumber's Torch) with rates, images, and descriptions
- [x] **1.5** Seed a **Services** category (Id: 1009) with description "Non-physical hire services including delivery, operator hire, and compliance testing" and image URL
- [x] **1.6** Seed 3–4 service entries under Services (e.g. Equipment Delivery, Trained Operator Hire, PAT Testing Service, Site Survey) with hourly/daily/weekly rates, descriptions, and images
- [x] **1.7** Generate SQL deployment script: `dotnet ef migrations script <PreviousMigration> <NewMigration> --idempotent --output scripts/sql/<MigrationName>.sql --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API`
- [x] **1.8** Apply migration locally: `dotnet ef database update --project src/ReviewPortal.Infrastructure --startup-project src/ReviewPortal.API`
- [x] **1.9** Run `dotnet build ReviewPortal.slnx` and `dotnet test ReviewPortal.slnx` — confirm no regressions

---

## Epic 2 – Reviews, Ratings & Community Interaction

> Epic doc: [EPIC-2-REVIEWS-AND-RATINGS.md](EPIC-2-REVIEWS-AND-RATINGS.md)

### TASK-6: Implement moderation service methods (stubs → real logic)

**Links to:** [US-2.1 – Submit a review for a tool/service](EPIC-2-REVIEWS-AND-RATINGS.md#us-21--submit-a-review-for-a-toolservice) | [US-2.2 – Display approved reviews on the tool/service page](EPIC-2-REVIEWS-AND-RATINGS.md#us-22--display-approved-reviews-on-the-toolservice-page) | [US-2.3 – Overall tool/service ranking based on ratings](EPIC-2-REVIEWS-AND-RATINGS.md#us-23--overall-toolservice-ranking-based-on-ratings) | [US-2.4 – Comment on a review](EPIC-2-REVIEWS-AND-RATINGS.md#us-24--comment-on-someone-elses-review)
**Also links to (Epic 3):** [US-3.6 – Review moderation queue](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-36--review-moderation-queue) | [US-3.9 – Admin API endpoints](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-39--admin-api-endpoints)
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
- [x] **6.4** Write unit tests for `GetPendingReviewsAsync` — empty queue, multiple pending items, pagination, sort order
- [x] **6.5** Write unit tests for `ModerateReviewAsync` — approve flow, reject flow, 404 case, tool rating recalculation, re-rejection of approved review
- [x] **6.6** Write unit tests for `ModerateCommentAsync` — approve flow, reject flow, 404 case
- [x] **6.7** Run `dotnet build ReviewPortal.slnx` and `dotnet test ReviewPortal.slnx` — confirm all tests pass

---

### TASK-9: Seed review data

**Links to:** [US-2.1 – Submit a review for a tool/service](EPIC-2-REVIEWS-AND-RATINGS.md#us-21--submit-a-review-for-a-toolservice) | [US-2.2 – Display approved reviews on the tool/service page](EPIC-2-REVIEWS-AND-RATINGS.md#us-22--display-approved-reviews-on-the-toolservice-page) | [US-2.3 – Overall tool/service ranking based on ratings](EPIC-2-REVIEWS-AND-RATINGS.md#us-23--overall-toolservice-ranking-based-on-ratings)
**Priority:** Should | **Gap ID:** GAP-SD-4

No actual `Reviews` rows exist in the database. Sprint 1 tool detail pages rely on denormalised `OverallRating`/`ReviewCount` fields. Seed real review rows to back up those values.

**Sub-tasks:**

- [x] **9.1** Create a new EF Core migration to seed 6–8 reviews with `Status = Approved` across 3–4 different tools
- [x] **9.2** Include varied ratings per review (not all 5 stars) to demonstrate realistic aggregation
- [x] **9.3** Ensure the seeded `Tool.OverallRating` and `Tool.ReviewCount` values match the actual reviews seeded
- [x] **9.4** Include 1–2 reviews with `Status = Pending` to populate the moderation queue for demos
- [x] **9.5** Generate SQL script, apply migration locally
- [x] **9.6** Run `dotnet build` and `dotnet test` — confirm no regressions

---

### TASK-13: Fix company response — enforce approved reviews only

**Links to:** [US-2.5 – Company response to a review](EPIC-2-REVIEWS-AND-RATINGS.md#us-25--company-response-to-a-review)
**Priority:** Should | **Gap ID:** GAP-SVC-4

`AddCompanyResponseAsync` does NOT validate that the review is approved. Staff can currently respond to Pending or Rejected reviews, which breaks the moderation workflow.

**Sub-tasks:**

- [ ] **13.1** In `ReviewService.AddCompanyResponseAsync`, after the `review is null` check, add: if `review.Status != ReviewStatus.Approved`, return `Result<CompanyResponseDto>.Failure("Company responses can only be added to approved reviews.")`
- [ ] **13.2** Write unit test: attempting to add a response to a Pending review returns a validation failure
- [ ] **13.3** Write unit test: attempting to add a response to a Rejected review returns a validation failure
- [ ] **13.4** Write unit test: adding a response to an Approved review still works as before
- [ ] **13.5** Run `dotnet build` and `dotnet test` — confirm all tests pass

---

### TASK-14: Add "Not enough reviews" threshold to tool DTOs

**Links to:** [US-2.3 – Overall tool/service ranking based on ratings](EPIC-2-REVIEWS-AND-RATINGS.md#us-23--overall-toolservice-ranking-based-on-ratings)
**Priority:** Should | **Gap ID:** GAP-SVC-5

US-2.3 AC says: "If there are fewer than 2 reviews, 'Not enough reviews to rate' is shown instead of a number." Check if this is enforced in the API response DTOs.

**Sub-tasks:**

- [ ] **14.1** Check `ToolDto` and `ToolSummaryDto` for a `HasEnoughReviews` or equivalent flag
- [ ] **14.2** If missing, add a `bool HasEnoughReviews` property (true when `ReviewCount >= 2`) to both DTOs
- [ ] **14.3** Update `ToolService.GetToolByIdAsync` and `GetToolsByCategoryAsync` mapping logic to populate the flag
- [ ] **14.4** Write unit tests: tool with 0 reviews → `HasEnoughReviews = false`; tool with 1 review → `false`; tool with 2+ → `true`
- [ ] **14.5** Run `dotnet build` and `dotnet test` — confirm all tests pass

---

### TASK-15: Seed comments and company responses

**Links to:** [US-2.4 – Comment on a review](EPIC-2-REVIEWS-AND-RATINGS.md#us-24--comment-on-someone-elses-review) | [US-2.5 – Company response to a review](EPIC-2-REVIEWS-AND-RATINGS.md#us-25--company-response-to-a-review)
**Priority:** Could | **Gap ID:** GAP-SD-5

No demo data for comments or company responses.

**Sub-tasks:**

- [ ] **15.1** In the same or new migration as TASK-9, seed 2–3 approved comments on seeded reviews
- [ ] **15.2** Seed 1–2 `CompanyResponse` rows from the Admin seed user (Id: 2)
- [ ] **15.3** Include 1 pending comment to populate the moderation queue
- [ ] **15.4** Generate SQL script, apply migration, run tests

---

## Epic 3 – Back-Office Management & Moderation

> Epic doc: [EPIC-3-BACKOFFICE-AND-MODERATION.md](EPIC-3-BACKOFFICE-AND-MODERATION.md)

### TASK-2: Create AdminToolsController

**Links to:** [US-3.2 – Add new equipment or service to the catalogue](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-32--add-new-equipment-or-service-to-the-catalogue) | [US-3.3 – Edit existing equipment/service details and pricing](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-33--edit-existing-equipmentservice-details-and-pricing) | [US-3.5 – Deactivate or remove equipment/service](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-35--deactivate-or-remove-equipmentservice) | [US-3.9 – Admin API endpoints](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-39--admin-api-endpoints)
**Priority:** Must | **Gap IDs:** GAP-API-1, GAP-API-2, GAP-API-3

Service methods `CreateToolAsync`, `UpdateToolAsync`, and `SetToolStatusAsync` already exist in `IToolService` but have no admin controller route.

**Sub-tasks:**

- [ ] **2.1** Create `src/ReviewPortal.API/Controllers/Admin/AdminToolsController.cs`
- [ ] **2.2** Route: `[Route("api/admin/tools")]`, class-level `[Authorize(Roles = "Admin")]`
- [ ] **2.3** `[HttpPost]` → calls `_toolService.CreateToolAsync(request)` → returns `201 Created` with the created tool
- [ ] **2.4** `[HttpPut("{id:int}")]` → calls `_toolService.UpdateToolAsync(id, request)` → returns `200 OK`
- [ ] **2.5** `[HttpPatch("{id:int}/status")]` → accepts `{ "isActive": true/false }` → calls `_toolService.SetToolStatusAsync(id, isActive)` → returns `200 OK`
- [ ] **2.6** Register the controller in DI if needed (ASP.NET auto-discovers by convention)
- [ ] **2.7** Write unit tests: Create success, Create validation failure, Update success, Update 404, Status change success, Status change 404, Unauthorized (no token), Forbidden (Customer role)
- [ ] **2.8** Run `dotnet build` and `dotnet test` — confirm all tests pass

---

### TASK-3: Create AdminModerationController

**Links to:** [US-3.6 – Review moderation queue](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-36--review-moderation-queue) | [US-3.9 – Admin API endpoints](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-39--admin-api-endpoints)
**Priority:** Must | **Gap IDs:** GAP-API-6, GAP-API-7, GAP-API-8

Service methods `GetPendingReviewsAsync`, `ModerateReviewAsync`, and `ModerateCommentAsync` exist in `IReviewService` but have no admin route. (Note: TASK-6 implements the service logic; this task creates the controller.)

**Sub-tasks:**

- [x] **3.1** Create `src/ReviewPortal.API/Controllers/Admin/AdminModerationController.cs`
- [x] **3.2** Route: `[Route("api/admin/moderation")]`, class-level `[Authorize(Roles = "Admin,Moderator")]`
- [x] **3.3** `[HttpGet("pending")]` with `[FromQuery] int page = 1, int pageSize = 20` → calls `_reviewService.GetPendingReviewsAsync(page, pageSize)` → returns `200 OK`
- [x] **3.4** `[HttpPut("reviews/{id:int}")]` with `[FromBody] ModerateReviewRequest` → calls `_reviewService.ModerateReviewAsync(id, request)` → returns `200 OK`
- [x] **3.5** `[HttpPut("comments/{id:int}")]` with `[FromBody] ModerateReviewRequest` → calls `_reviewService.ModerateCommentAsync(id, request)` → returns `200 OK`
- [x] **3.6** Write unit tests: Get pending success, Get pending empty, Approve review, Reject review with reason, Approve comment, Reject comment, Unauthorized, Forbidden
- [x] **3.7** Run `dotnet build` and `dotnet test` — confirm all tests pass

---

### TASK-4: Implement ImageService and image endpoints

**Links to:** [US-3.4 – Manage tool/service images](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-34--manage-toolservice-images) | [US-3.9 – Admin API endpoints](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-39--admin-api-endpoints)
**Priority:** Must | **Gap IDs:** GAP-SVC-2, GAP-API-4, GAP-API-5

No image upload or delete functionality exists.

**Sub-tasks:**

- [ ] **4.1** Create `src/ReviewPortal.Application/Interfaces/IImageService.cs` with methods:
  - `Task<Result<ToolImageDto>> UploadImageAsync(int toolId, Stream fileStream, string fileName, CancellationToken)`
  - `Task<Result<bool>> DeleteImageAsync(int toolId, int imageId, CancellationToken)`
- [ ] **4.2** Create `src/ReviewPortal.Infrastructure/Services/ImageService.cs` implementing `IImageService`
- [ ] **4.3** Upload validation: only allow `.jpg`, `.jpeg`, `.png`, `.webp`; max 5MB file size
- [ ] **4.4** Upload storage: save files to a local `uploads/tools/` folder (configurable path in `appsettings.json`)
- [ ] **4.5** Create a `ToolImage` record in the database with the file path, set `DisplayOrder` to max + 1
- [ ] **4.6** Delete logic: look up image by `toolId` + `imageId`; if tool has only 1 image, return error "Cannot delete the last image"
- [ ] **4.7** Delete the file from disk and remove the `ToolImage` record
- [ ] **4.8** Register `IImageService` in DI (`Program.cs` or `DependencyInjection.cs`)
- [ ] **4.9** Add endpoints to `AdminToolsController`:
  - `[HttpPost("{id:int}/images")]` accepting `IFormFile` → calls `UploadImageAsync`
  - `[HttpDelete("{id:int}/images/{imageId:int}")]` → calls `DeleteImageAsync`
- [ ] **4.10** Write unit tests: upload success, upload invalid format (returns 400), upload too large (returns 400), delete success, delete last image (returns 400), delete image not found (returns 404)
- [ ] **4.11** Run `dotnet build` and `dotnet test` — confirm all tests pass

---

### TASK-5: Implement DashboardService and controller

**Links to:** [US-3.8 – Admin dashboard with overview stats](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-38--admin-dashboard-with-overview-stats) | [US-3.9 – Admin API endpoints](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-39--admin-api-endpoints)
**Priority:** Must | **Gap IDs:** GAP-SVC-1, GAP-API-9

No dashboard service or controller exists.

**Sub-tasks:**

- [ ] **5.1** Create `src/ReviewPortal.Application/DTOs/Dashboard/DashboardStatsDto.cs` with properties:
  - `int TotalActiveTools`, `int TotalInactiveTools`
  - `int PendingReviewsCount`
  - `int ReviewsPublishedThisMonth`
  - `IReadOnlyList<ToolRankingDto> TopRatedTools` (top 5)
  - `IReadOnlyList<ToolRankingDto> MostReviewedTools` (top 5)
- [ ] **5.2** Create `ToolRankingDto` record: `int ToolId`, `string ToolName`, `decimal? OverallRating`, `int ReviewCount`
- [ ] **5.3** Create `src/ReviewPortal.Application/Interfaces/IDashboardService.cs` with `Task<Result<DashboardStatsDto>> GetDashboardStatsAsync(CancellationToken)`
- [ ] **5.4** Create `src/ReviewPortal.Application/Services/DashboardService.cs` implementing `IDashboardService`
  - Query `Tools` for active/inactive counts
  - Query `Reviews` with `Status == Pending` for pending count
  - Query `Reviews` with `Status == Approved` and `CreatedDate` in current month for monthly count
  - Query `Tools` ordered by `OverallRating` desc (where `ReviewCount >= 2`) for top rated
  - Query `Tools` ordered by `ReviewCount` desc for most reviewed
- [ ] **5.5** Register `IDashboardService` in DI
- [ ] **5.6** Create `src/ReviewPortal.API/Controllers/Admin/AdminDashboardController.cs`
  - Route: `[Route("api/admin/dashboard")]`, `[Authorize(Roles = "Admin")]`
  - `[HttpGet("stats")]` → calls `GetDashboardStatsAsync` → returns `200 OK`
- [ ] **5.7** Write unit tests for `DashboardService`: all-zero case, mixed data, top 5 ordering, monthly boundary
- [ ] **5.8** Write unit tests for `AdminDashboardController`: success, unauthorised
- [ ] **5.9** Run `dotnet build` and `dotnet test` — confirm all tests pass

---

### TASK-16: Decide on admin category routing

**Links to:** [US-3.7 – Manage categories](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-37--manage-categories) | [US-3.9 – Admin API endpoints](EPIC-3-BACKOFFICE-AND-MODERATION.md#us-39--admin-api-endpoints)
**Priority:** Could | **Gap ID:** GAP-API-10

Category CRUD already exists on the public `CategoriesController` with `[Authorize(Roles = "Admin")]`. US-3.9 specifies `/api/admin/categories`.

**Sub-tasks:**

- [ ] **16.1** Decide: keep the current pattern (public route with auth) or create a separate `AdminCategoriesController` at `/api/admin/categories`
- [ ] **16.2** If separate: create controller, move admin-only methods (POST, PUT, DELETE), leave public GET methods on existing controller
- [ ] **16.3** Document the decision in `PRODUCT-BACKLOG.md` or this file

---

## Documentation Tasks

> These update the agile docs only — no code changes.

### TASK-7: Update user story wording — tool → tool/service

**Links to:** [EPIC-1](EPIC-1-CATALOGUE-AND-CALCULATOR.md) | [EPIC-2](EPIC-2-REVIEWS-AND-RATINGS.md) | [EPIC-3](EPIC-3-BACKOFFICE-AND-MODERATION.md) | [PRODUCT-BACKLOG](PRODUCT-BACKLOG.md) | [SPRINT-PLANNING](SPRINT-PLANNING.md)
**Priority:** Must | **Gap ID:** GAP-DOC-1

**Sub-tasks:**

- [x] **7.1** In EPIC-1, update user-facing story text to say "tool/service" instead of just "tool" (keep `Tool` as entity/class name in technical references)
- [x] **7.2** In EPIC-2, update user-facing story text: "Submit a review for a tool/service", "Display approved reviews on the tool/service page", etc.
- [x] **7.3** In EPIC-3, update user-facing story text: "Add new equipment or service", "Edit existing equipment/service", etc.
- [x] **7.4** In PRODUCT-BACKLOG.md, update the Epics table and Tool Categories section to mention services
- [x] **7.5** In SPRINT-PLANNING.md, update story titles in the sprint tables
- [x] **7.6** Do NOT change entity names, class names, variable names, or controller names — only docs wording

---

### TASK-8: Fix mandatory moderation wording

**Links to:** [US-2.1 – Submit a review for a tool/service](EPIC-2-REVIEWS-AND-RATINGS.md#us-21--submit-a-review-for-a-toolservice) | [US-2.4 – Comment on a review](EPIC-2-REVIEWS-AND-RATINGS.md#us-24--comment-on-someone-elses-review)
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
  - ~~"data integrity is maintained"~~ → "Saving a tool/service with a missing required field returns HTTP 400 with a validation error listing the missing field(s)"
  - ~~"works properly on mobile"~~ → "All catalogue pages render without horizontal scrolling on viewports 375px wide and above"
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
- [x] **11.3** Include: tool/service categories → US-1.9, review categories → US-2.1/US-2.9, rating aggregation → US-2.3, moderation → US-3.6, pricing → US-1.5
- [x] **11.4** Cross-reference to the Requirements Traceability Matrix in `GAP-ANALYSIS.md` §2

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
  ├──→ TASK-2  (AdminToolsController)
  │      └──→ TASK-4  (ImageService)
  ├──→ TASK-6  (Moderation service methods)
  │      └──→ TASK-3  (AdminModerationController)
  ├──→ TASK-5  (DashboardService)
  └──→ TASK-9  (Seed reviews)
         └──→ TASK-15 (Seed comments/responses)

TASK-13 (Fix company response) — independent, do anytime
TASK-14 (Review threshold)     — independent, do anytime

TASK-7  (Docs: tool/service wording) — independent
TASK-8  (Docs: moderation wording)   — independent
TASK-10 (Docs: vague ACs)            — independent
TASK-11 (Docs: traceability)         — independent
TASK-12 (Docs: definitions)          — independent
TASK-16 (Admin category routing)     — independent
```

---

## Progress Tracker

| Task | Status | Date Completed |
|------|--------|---------------|
| TASK-1: Seed missing categories | Done | 2026-04-22 |
| TASK-2: AdminToolsController | ⬜ Not started | |
| TASK-3: AdminModerationController | Done | 2026-04-22 |
| TASK-4: ImageService | ⬜ Not started | |
| TASK-5: DashboardService | ⬜ Not started | |
| TASK-6: Moderation service methods | Done | 2026-04-22 |
| TASK-7: Docs – tool/service wording | Done | 2026-04-23 |
| TASK-8: Docs – moderation wording | Done | 2026-04-23 |
| TASK-9: Seed review data | Done | 2026-04-22 |
| TASK-10: Docs – vague ACs | Done | 2026-04-23 |
| TASK-11: Docs – traceability | Done | 2026-04-23 |
| TASK-12: Docs – definitions | Done | 2026-04-23 |
| TASK-13: Fix company response | ⬜ Not started | |
| TASK-14: Review threshold | ⬜ Not started | |
| TASK-15: Seed comments/responses | ⬜ Not started | |
| TASK-16: Admin category routing | ⬜ Not started | |
