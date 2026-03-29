# Epic 3 – Back-Office Management & Moderation

**As a Shelton Tool-Hire staff member, I need a secure admin area where I can manage the equipment catalogue, update pricing, handle images, and moderate customer reviews before they go public.**

This epic covers all the behind-the-scenes work that keeps the portal running properly. Without it, the data goes stale and the reviews section becomes unusable. This is essentially the control panel for the business.

---

## User Stories

### US-3.1 – Admin login and role-based access

**As a** Shelton staff member,
**I want to** log into a secure admin area,
**so that** I can manage equipment and moderate reviews without unauthorized people getting in.

**Acceptance Criteria:**
- Admin users log in with email and password through a separate admin login page
- Admins are assigned the "Admin" or "Moderator" role in the system
- Only users with the correct role can access admin pages — everyone else gets a 403
- The admin area has its own layout/navigation separate from the public-facing site
- JWT tokens include role claims that the front end uses to show/hide admin features
- Failed login attempts show a generic error message (no hints about which field was wrong)

**Story Points:** 5
**Sprint:** 2

---

### US-3.2 – Add new equipment to the catalogue

**As an** admin,
**I want to** add a new tool or piece of equipment to the system,
**so that** customers can see it in the catalogue and hire it.

**Acceptance Criteria:**
- The admin area has an "Add Equipment" form
- Required fields: Name, Description, Category (dropdown from existing categories), Hourly Rate, Daily Rate, Weekly Rate
- Optional fields: Special notes, deposit required (yes/no and amount)
- At least one image must be uploaded before the item can be saved
- Validation catches missing required fields and shows clear error messages
- On save, the tool appears in the catalogue immediately (status = Active)
- A success message confirms the addition

**Story Points:** 5
**Sprint:** 4

---

### US-3.3 – Edit existing equipment details and pricing

**As an** admin,
**I want to** update the description, pricing, or other details of a tool,
**so that** the catalogue stays accurate as prices change or we get better descriptions.

**Acceptance Criteria:**
- Each tool in the admin equipment list has an "Edit" button
- The edit form is pre-filled with the current values
- All fields from the add form are editable
- An "UpdatedDate" timestamp is set automatically on save
- Changes are reflected on the public site immediately after saving
- There is a confirmation step before saving ("Are you sure you want to update this item?")

**Story Points:** 5
**Sprint:** 4

---

### US-3.4 – Manage tool images

**As an** admin,
**I want to** upload, replace, or remove images for a tool,
**so that** customers see up-to-date and accurate photos.

**Acceptance Criteria:**
- On the equipment edit page, existing images are displayed with options to delete each one
- New images can be uploaded (supported formats: JPG, PNG, WebP; max file size: 5MB per image)
- At least one image must remain — the system should not allow deleting the last image
- Images are stored in a sensible location (e.g. Azure Blob Storage or a local uploads folder for the prototype)
- A thumbnail preview is shown after upload before saving

**Story Points:** 5
**Sprint:** 4

---

### US-3.5 – Deactivate or remove equipment

**As an** admin,
**I want to** remove a tool from the public catalogue or mark it as inactive,
**so that** customers do not try to hire something that is no longer available.

**Acceptance Criteria:**
- Each tool has a "Deactivate" option in the admin list
- Deactivated tools do not appear in the public catalogue or search results
- Deactivated tools are still visible in the admin area with a clear "Inactive" label
- There is also a "Reactivate" option to bring it back
- We are not doing hard deletes — everything is soft-deleted so review history is preserved

**Story Points:** 3
**Sprint:** 4

---

### US-3.6 – Review moderation queue

**As a** moderator,
**I want to** see all pending reviews and comments in one place,
**so that** I can approve or reject them efficiently before they go live.

**Acceptance Criteria:**
- The admin area has a "Moderation Queue" page showing all reviews and comments with status = Pending
- Each item shows: reviewer name, tool name, the review/comment text, date submitted, and the star ratings (for reviews)
- Moderator can click "Approve" to make it visible on the public site
- Moderator can click "Reject" and must provide a brief reason (this reason is shown to the reviewer on their "My Reviews" page)
- Items in the queue are sorted by oldest first so nothing gets buried
- The queue shows a count badge in the admin navigation so moderators can see at a glance how many items are waiting

**Story Points:** 8
**Sprint:** 3

---

### US-3.7 – Manage categories

**As an** admin,
**I want to** add, rename, or reorganise tool categories,
**so that** the catalogue stays well structured as the business grows.

**Acceptance Criteria:**
- Admin area has a "Categories" management page
- Admins can add a new category with a name, description, and image
- Existing categories can be renamed or have their description/image updated
- Categories cannot be deleted if they still contain tools — the system shows a warning
- Changes to category names are reflected across the whole site immediately

**Story Points:** 3
**Sprint:** 4

---

### US-3.8 – Admin dashboard with overview stats

**As an** admin,
**I want to** see a quick summary of key numbers when I log in,
**so that** I have an idea of how the portal is doing without having to dig into each section.

**Acceptance Criteria:**
- The admin home page shows:
  - Total number of tools in the catalogue (active vs inactive)
  - Number of reviews pending moderation
  - Number of reviews published this month
  - Top 5 highest-rated tools
  - Top 5 most-reviewed tools
- Data is fetched from the API and displayed in a clean layout (cards or simple charts)
- Stats refresh when the page is loaded (no need for real-time updates)

**Story Points:** 5
**Sprint:** 4

---

### US-3.9 – Admin API endpoints

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
- All endpoints return appropriate status codes
- Unit tests cover authorisation checks and business logic
- Unauthorised requests return 401, forbidden requests return 403

**Story Points:** 13
**Sprint:** 3 & 4

---

### US-3.10 – Playwright end-to-end tests for critical flows

**As a** developer and part of the team's quality process,
**I want** automated end-to-end tests for the most important user journeys,
**so that** we catch regressions before they reach the main branch.

**Acceptance Criteria:**
- Playwright tests cover the following flows:
  1. Browse categories → view tool detail → use rental calculator
  2. Submit a review → verify it appears in moderation queue → approve it → verify it appears on the tool page
  3. Admin login → add a new tool → verify it appears in the catalogue
- Tests run as part of the GitHub Actions CI pipeline
- Tests use a test database or seeded data so they are repeatable
- Tests pass consistently (no flaky failures)

**Story Points:** 8
**Sprint:** 4

---

## Summary for Sprint Planning

| Sprint | Stories | Total Points |
|--------|---------|-------------|
| Sprint 2 | US-3.1 | 5 |
| Sprint 3 | US-3.6, US-3.9 (partial) | 14 |
| Sprint 4 | US-3.2, US-3.3, US-3.4, US-3.5, US-3.7, US-3.8, US-3.9 (remainder), US-3.10 | 47 |
