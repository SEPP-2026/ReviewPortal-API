# Epic 3 – Back-Office Management & Moderation

**As a Shelton Tool-Hire staff member, I need a secure admin area where I can manage the equipment/service catalogue, update pricing, handle images, and moderate customer reviews before they go public.**

This epic covers all the behind-the-scenes work that keeps the portal running properly. Without it, the data goes stale and the reviews section becomes unusable. This is essentially the control panel for the business.

---

## User Stories

### US-3.1 – Admin login and role-based access

**As a** Shelton staff member,
**I want to** log into a secure admin area,
**so that** I can manage equipment/services and moderate reviews without unauthorised people gaining access.

**Acceptance Criteria:**
- Admin users log in with email and password through a separate admin login page
- Admins are assigned the "Admin" or "Moderator" role in the system
- Only users with the correct role can access admin pages — everyone else gets a 403
- The admin area has its own layout/navigation separate from the public-facing site
- JWT tokens include role claims that the front end uses to show/hide admin features
- Failed login attempts show a generic error message (no hints about which field was wrong)

**Story Points:** 5
**Priority:** Must
**Sprint:** 2

**Tasks:**

| # | Task | Owner |
|---|------|-------|
| 1 | Configure role-based authorisation policies for "Admin" and "Moderator" roles | Backend |
| 2 | Include role claims in JWT token generation for admin users | Backend |
| 3 | Build the admin login page (separate from the public login page) | Frontend |
| 4 | Implement admin area layout and navigation (distinct from the public site) | Frontend |
| 5 | Enforce role-based access on frontend routes — redirect unauthorised users | Frontend |
| 6 | Display a generic error message on failed login (no field-specific hints) | Frontend |
| 7 | Write unit tests for role-based authorisation and token claim logic | Testing |

---

### US-3.2 – Add new equipment or service to the catalogue

**As an** admin,
**I want to** add a new tool, piece of equipment, or service to the system,
**so that** customers can see it in the catalogue and hire it.

**Acceptance Criteria:**
- The admin area has an "Add Equipment" form
- Required fields: Name, Description, Category (dropdown from existing categories), Hourly Rate, Daily Rate, Weekly Rate
- Optional fields: Special notes, deposit required (yes/no and amount)
- At least one image must be uploaded before the item can be saved
- Submitting the form with missing required fields is blocked and each missing field is identified in the validation response
- On save, the equipment/service appears in the catalogue immediately (status = Active)
- A success message confirms the addition

**Story Points:** 5
**Priority:** Must
**Sprint:** 4

**Tasks:**

| # | Task | Owner |
|---|------|-------|
| 1 | Create `ToolManagementService.CreateTool()` method with validation for all required fields | Backend |
| 2 | Enforce the minimum one-image requirement before allowing the tool to be saved | Backend |
| 3 | Build the "Add Equipment/Service" form in the admin area with all required and optional fields | Frontend |
| 4 | Implement category dropdown populated from existing categories | Frontend |
| 5 | Add client-side validation with clear error messages for missing required fields | Frontend |
| 6 | Integrate image upload into the form (at least one image required) | Frontend |
| 7 | Display a success confirmation message after the tool is saved | Frontend |
| 8 | Write unit tests for the tool creation service, including validation edge cases | Testing |

---

### US-3.3 – Edit existing equipment/service details and pricing

**As an** admin,
**I want to** update the description, pricing, or other details of a tool/service,
**so that** the catalogue stays accurate as prices change or we get better descriptions.

**Acceptance Criteria:**
- Each tool/service in the admin equipment list has an "Edit" button
- The edit form is pre-filled with the current values
- All fields from the add form are editable
- An "UpdatedDate" timestamp is set automatically on save
- A subsequent fetch of the public tool/service page returns the updated values immediately after saving
- There is a confirmation step before saving ("Are you sure you want to update this item?")

**Story Points:** 5
**Priority:** Must
**Sprint:** 4

**Tasks:**

| # | Task | Owner |
|---|------|-------|
| 1 | Create `ToolManagementService.UpdateTool()` method that sets `UpdatedDate` automatically | Backend |
| 2 | Add "Edit" button to each tool/service in the admin equipment list | Frontend |
| 3 | Build the edit form pre-filled with the tool/service's current values | Frontend |
| 4 | Implement a confirmation dialogue before saving changes ("Are you sure you want to update this item?") | Frontend |
| 5 | Verify that changes are reflected on the public site immediately after saving | Frontend |
| 6 | Write unit tests for the tool update service, including timestamp auto-setting | Testing |

---

### US-3.4 – Manage tool/service images

**As an** admin,
**I want to** upload, replace, or remove images for a tool/service,
**so that** customers see up-to-date and accurate photos.

**Acceptance Criteria:**
- On the equipment edit page, existing images are displayed with options to delete each one
- New images can be uploaded (supported formats: JPG, PNG, WebP; max file size: 5MB per image)
- At least one image must remain — the system should not allow deleting the last image
- Uploaded images are stored in the configured storage location (Azure Blob Storage or the prototype's local uploads folder)
- A thumbnail preview is shown after upload before saving

**Story Points:** 5
**Priority:** Must
**Sprint:** 4

**Tasks:**

| # | Task | Owner |
|---|------|-------|
| 1 | Create image upload service with file format validation (JPG, PNG, WebP) and size limit (5MB) | Backend |
| 2 | Create image deletion service that enforces the minimum one-image constraint | Backend |
| 3 | Configure image storage location (Azure Blob Storage or local uploads folder for the prototype) | Backend |
| 4 | Display existing images on the equipment edit page with individual delete buttons | Frontend |
| 5 | Build image upload component with thumbnail preview shown before saving | Frontend |
| 6 | Prevent deletion of the last remaining image (show a warning message) | Frontend |
| 7 | Write unit tests for image upload validation and the one-image minimum constraint | Testing |

---

### US-3.5 – Deactivate or remove equipment/service

**As an** admin,
**I want to** remove a tool/service from the public catalogue or mark it as inactive,
**so that** customers do not try to hire something that is no longer available.

**Acceptance Criteria:**
- Each tool/service has a "Deactivate" option in the admin list
- Deactivated tools/services do not appear in the public catalogue or search results
- Deactivated tools/services are still visible in the admin area with an "Inactive" label
- There is also a "Reactivate" option to bring it back
- We are not doing hard deletes — everything is soft-deleted so review history is preserved

**Story Points:** 3
**Priority:** Should
**Sprint:** 4

**Tasks:**

| # | Task | Owner |
|---|------|-------|
| 1 | Implement soft-delete logic in `ToolManagementService` (set `IsActive = false`; exclude from public queries) | Backend |
| 2 | Add a reactivation method that sets `IsActive = true` | Backend |
| 3 | Add "Deactivate" and "Reactivate" toggle buttons in the admin equipment list | Frontend |
| 4 | Show an "Inactive" label for deactivated tools/services in the admin view | Frontend |
| 5 | Verify that deactivated tools/services are hidden from the public catalogue and search results | Testing |
| 6 | Write unit tests for soft-delete and reactivation logic | Testing |

---

### US-3.6 – Review moderation queue

**As a** moderator,
**I want to** see all pending reviews and comments in one place,
**so that** I can approve or reject them efficiently before they go live.

**Acceptance Criteria:**
- The admin area has a "Moderation Queue" page showing all reviews and comments with status = Pending
- Each item shows: reviewer name, tool/service name, the review/comment text, date submitted, and the star ratings (for reviews)
- Moderator can click "Approve" to make it visible on the public site
- Moderator can click "Reject" and must provide a brief reason (this reason is shown to the reviewer on their "My Reviews" page)
- Items in the queue are sorted by oldest first so nothing gets buried
- The admin navigation shows a count badge with the exact total number of pending reviews and comments

**Story Points:** 8
**Priority:** Must
**Sprint:** 3

**Tasks:**

| # | Task | Owner |
|---|------|-------|
| 1 | Create `ModerationService` to return all pending reviews and comments, sorted by oldest first | Backend |
| 2 | Implement approve action — update status to "Approved" and make content visible on the public site | Backend |
| 3 | Implement reject action — update status to "Rejected" and store the rejection reason | Backend |
| 4 | Build the "Moderation Queue" page displaying pending items (reviewer name, tool/service name, text, date, star ratings) | Frontend |
| 5 | Add "Approve" and "Reject" buttons for each item, with a text input for the rejection reason | Frontend |
| 6 | Add a pending-count badge in the admin navigation to indicate how many items are waiting | Frontend |
| 7 | Ensure rejection reasons are transparent and visible to reviewers on their "My Reviews" page (links to US-2.8) | Frontend |
| 8 | Write unit tests for moderation service logic (approve, reject, sorting, reason storage) | Testing |

---

### US-3.7 – Manage categories

**As an** admin,
**I want to** add, rename, or reorganise tool/service categories,
**so that** the catalogue stays well structured as the business grows.

**Acceptance Criteria:**
- Admin area has a "Categories" management page
- Admins can add a new category with a name, description, and image
- Existing categories can be renamed or have their description/image updated
- Deletion is blocked when a category still has one or more assigned tools/services, and the warning message explains why
- A subsequent fetch of the category on the public site returns the updated name, description, and image after save

**Story Points:** 3
**Priority:** Should
**Sprint:** 4

**Tasks:**

| # | Task | Owner |
|---|------|-------|
| 1 | Create `CategoryManagementService` with methods to add, update, and validate categories | Backend |
| 2 | Implement a safeguard that prevents deletion of categories that still contain tools/services | Backend |
| 3 | Build the "Categories" management page in the admin area | Frontend |
| 4 | Add a form for creating a new category (name, description, image upload) | Frontend |
| 5 | Allow editing of existing category details (name, description, image) | Frontend |
| 6 | Show a warning message when an admin attempts to delete a category that has tools/services assigned | Frontend |
| 7 | Write unit tests for category management service, including the deletion safeguard | Testing |

---

### US-3.8 – Admin dashboard with overview stats

**As an** admin,
**I want to** see a quick summary of key numbers when I log in,
**so that** I have an idea of how the portal is doing without having to look through each section.

**Acceptance Criteria:**
- The admin home page shows:
  - Total number of tools/services in the catalogue (active vs inactive)
  - Number of reviews pending moderation
  - Number of reviews published this month
  - Top 5 highest-rated tools/services
  - Top 5 most-reviewed tools/services
- Data is fetched from the API on page load and displayed as labelled summary cards or chart sections
- Stats refresh when the page is loaded (no need for real-time updates)

**Story Points:** 5
**Priority:** Could
**Sprint:** 4

**Tasks:**

| # | Task | Owner |
|---|------|-------|
| 1 | Create `DashboardService` to calculate and return summary statistics (tool/service counts, pending reviews, monthly reviews, top-rated, most-reviewed) | Backend |
| 2 | Build the admin home page layout with summary cards or simple charts | Frontend |
| 3 | Display tool/service counts (active vs inactive) and review statistics | Frontend |
| 4 | Display the top 5 highest-rated and top 5 most-reviewed tools/services | Frontend |
| 5 | Ensure dashboard has adequate colour contrast and screen reader support for accessibility | Frontend |
| 6 | Write unit tests for the dashboard statistics service | Testing |

---

### US-3.9 – Admin API endpoints

*[Technical — kept for reference]*

**As a** developer building the admin features,
**I want** secured API endpoints for managing equipment, categories, and moderation,
**so that** all admin actions go through a proper backend.

**Acceptance Criteria:**
- All admin endpoints require authentication and the "Admin" role
- Equipment management:
  - `POST /api/admin/tools` – create a new tool
  - `PUT /api/admin/tools/{id}` – update a tool
  - `PATCH /api/admin/tools/{id}/status` – activate/deactivate
  - `POST /api/admin/tools/{id}/images` – upload image
  - `DELETE /api/admin/tools/{id}/images/{imageId}` – remove image
- Category management:
  - `POST /api/admin/categories` – create category
  - `PUT /api/admin/categories/{id}` – update category
- Moderation:
  - `GET /api/admin/moderation/pending` – get pending items
  - `PUT /api/admin/moderation/reviews/{id}` – approve or reject (with reason)
  - `PUT /api/admin/moderation/comments/{id}` – approve or reject
- Dashboard:
  - `GET /api/admin/dashboard/stats` – returns summary stats
- Endpoints return 200/201 for success, 400 for validation failure, 401 for unauthenticated requests, 403 for forbidden requests, and 404 when the target resource does not exist
- Unit tests cover success, validation failure, authorisation, and not-found paths
- Unauthorised requests return 401, forbidden requests return 403

**Story Points:** 13
**Priority:** Must
**Sprint:** 3 & 4

---

### US-3.10 – Playwright end-to-end tests for critical flows

*[Technical — kept for reference]*

**As a** developer and part of the team's quality process,
**I want** automated end-to-end tests for the most important user journeys,
**so that** we catch regressions before they reach the main branch.

**Acceptance Criteria:**
- Playwright tests cover the following flows:
  1. Browse categories → view tool/service detail → use rental calculator
  2. Submit a review → verify it appears in moderation queue → approve it → verify it appears on the tool/service page
  3. Admin login → add a new tool/service → verify it appears in the catalogue
- Tests run as part of the GitHub Actions CI pipeline
- Tests use a test database or seeded data so they are repeatable
- The Playwright suite passes on repeated local or CI runs without test changes

**Story Points:** 8
**Priority:** Should
**Sprint:** 4

---

## Summary for Sprint Planning

| Sprint | Stories | Total Points |
|--------|---------|-------------|
| Sprint 2 | US-3.1 | 5 |
| Sprint 3 | US-3.6, US-3.9 (partial) | 14 |
| Sprint 4 | US-3.2, US-3.3, US-3.4, US-3.5, US-3.7, US-3.8, US-3.9 (remainder), US-3.10 | 47 |
