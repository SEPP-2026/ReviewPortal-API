# Epic 2 – Reviews, Ratings & Community Interaction

**As a customer who has hired a tool from Shelton, I want to share my honest experience and read what other people think, so that future customers and the company can benefit from real feedback.**

This epic covers the entire review lifecycle — writing a review, rating different aspects of the service, reading other customers' reviews, commenting on them, and giving the company the ability to respond. It also includes the rating system that feeds into overall tool rankings.

---

## User Stories

### US-2.1 – Submit a review for a tool

**As a** customer who has recently hired a tool,
**I want to** write a review and rate my experience,
**so that** I can help other customers decide and give Shelton useful feedback.

**Acceptance Criteria:**
- A "Write a Review" button is visible on every tool detail page
- The user must provide their name and email (or be logged in) to submit
- The review form includes:
  - A text field for the written review (minimum 20 characters, maximum 2000)
  - Star ratings (1–5) for each review category: Equipment Performance, Booking & Customer Service, Technical Support, After-Sales Support, Value for Money
- All five ratings are required before the form can be submitted
- On submission, the user sees a confirmation message: "Thanks for your review – it will be visible once our team has checked it"
- The review is saved with a status of "Pending" (not visible to other customers until approved)

**Story Points:** 8
**Priority:** Must
**Sprint:** 3

**Tasks:**

| # | Task | Owner |
|---|------|-------|
| 1 | Create `ReviewService.SubmitReview()` method with validation (min 20 chars, all 5 ratings required) | Backend |
| 2 | Set review status to "Pending" on creation; save to database | Backend |
| 3 | Add "Write a Review" button to the tool detail page (visible to all users) | Frontend |
| 4 | Build the review submission form with text field and five star-rating input components | Frontend |
| 5 | Implement client-side form validation (character count, all ratings required) | Frontend |
| 6 | Handle authentication check — prompt login or collect name/email for anonymous submissions | Frontend |
| 7 | Display confirmation message on successful submission | Frontend |
| 8 | Ensure minimal personal data collection in line with GDPR principles (collect only name, email, and review content) | Frontend |
| 9 | Write unit tests for review submission validation and service logic | Testing |

---

### US-2.2 – Display approved reviews on the tool page

**As a** customer browsing tools,
**I want to** read reviews left by other customers,
**so that** I can see whether the tool is reliable and whether the service was good.

**Acceptance Criteria:**
- Approved reviews appear on the tool detail page, sorted by most recent first
- Each review shows: reviewer name, date, the written text, and the individual star ratings
- An overall average rating is calculated and shown at the top of the reviews section
- If there are no reviews yet, a friendly message is shown: "No reviews yet – be the first to share your experience"
- Reviews can be paginated if there are more than 10

**Story Points:** 5
**Priority:** Must
**Sprint:** 3

**Tasks:**

| # | Task | Owner |
|---|------|-------|
| 1 | Create `ReviewService` method to return approved reviews for a tool, sorted by most recent first | Backend |
| 2 | Implement average rating calculation logic across all approved reviews | Backend |
| 3 | Build the reviews section on the tool detail page | Frontend |
| 4 | Create a reusable `ReviewCard` component (reviewer name, date, text, individual star ratings) | Frontend |
| 5 | Display calculated overall average rating at the top of the reviews section | Frontend |
| 6 | Show a "No reviews yet" message for tools without approved reviews | Frontend |
| 7 | Implement pagination for reviews when more than 10 are present | Frontend |
| 8 | Write unit tests for review retrieval and average rating calculation | Testing |

---

### US-2.3 – Overall tool ranking based on ratings

**As a** customer comparing tools,
**I want to** see an overall star rating for each tool,
**so that** I can quickly tell which ones are well regarded.

**Acceptance Criteria:**
- Each tool has a calculated overall rating (average of all review category averages)
- The overall rating is displayed on catalogue listing pages (category pages, search results) as well as the detail page
- Tools can be sorted by rating on the category page
- The number of reviews is shown alongside the rating (e.g. "4.3 ★ (17 reviews)")
- If there are fewer than 2 reviews, "Not enough reviews to rate" is shown instead of a number

**Story Points:** 5
**Priority:** Must
**Sprint:** 3

**Tasks:**

| # | Task | Owner |
|---|------|-------|
| 1 | Implement overall rating calculation in the service layer (average of all five category averages) | Backend |
| 2 | Update tool listing queries to include overall rating and review count in the response | Backend |
| 3 | Display star rating and review count on catalogue listing pages (category pages, search results) | Frontend |
| 4 | Add a "sort by rating" option to the category page sort controls | Frontend |
| 5 | Show "Not enough reviews to rate" for tools with fewer than 2 reviews | Frontend |
| 6 | Write unit tests for rating calculation logic and the two-review threshold rule | Testing |

---

### US-2.4 – Comment on someone else's review

**As a** customer reading reviews,
**I want to** leave a comment on a review,
**so that** I can agree, disagree, or add my own experience to the conversation.

**Acceptance Criteria:**
- Each review has a "Reply" or "Add Comment" option
- Comments require a name and the comment text (minimum 10 characters)
- Comments appear below the parent review, indented or visually nested
- Comments also go through moderation (status = Pending) before being visible
- A review can have multiple comments but we are not doing threaded/nested replies within comments (one level deep only)

**Story Points:** 5
**Priority:** Must
**Sprint:** 3

**Tasks:**

| # | Task | Owner |
|---|------|-------|
| 1 | Create `CommentService.AddComment()` method with validation (min 10 chars, name required) | Backend |
| 2 | Set comment status to "Pending" on creation; save to database | Backend |
| 3 | Add "Reply" or "Add Comment" button beneath each review on the tool page | Frontend |
| 4 | Build comment form with name and text fields, including client-side validation | Frontend |
| 5 | Display approved comments beneath the parent review, visually nested (one level only) | Frontend |
| 6 | Write unit tests for comment submission and validation logic | Testing |

---

### US-2.5 – Company response to a review

**As a** Shelton staff member,
**I want to** respond to a customer review,
**so that** we can thank people for positive feedback or address any concerns publicly.

**Acceptance Criteria:**
- On the admin side, each review has a "Respond" button
- The response appears on the public tool page beneath the review, clearly labelled as "Shelton Tool-Hire Response"
- Only one official response per review is allowed
- Company responses do not require moderation (they are posted by staff and go live immediately)
- The response can be edited or removed by admin users

**Story Points:** 5
**Priority:** Must
**Sprint:** 3

**Tasks:**

| # | Task | Owner |
|---|------|-------|
| 1 | Create `CompanyResponseService` with methods to add, edit, and delete a response (one per review constraint) | Backend |
| 2 | Ensure company responses bypass the moderation workflow and are published immediately | Backend |
| 3 | Add "Respond" button on the admin review view | Frontend |
| 4 | Build response form in the admin area with text input | Frontend |
| 5 | Display company response on the public tool page, clearly labelled as "Shelton Tool-Hire Response" | Frontend |
| 6 | Add edit and delete options for existing responses in the admin view | Frontend |
| 7 | Write unit tests for the one-response-per-review constraint and CRUD operations | Testing |

---

### US-2.6 – Review API endpoints

*[Technical — kept for reference]*

**As a** developer building the review features,
**I want** API endpoints for creating, fetching, and managing reviews and comments,
**so that** the front end can interact with review data cleanly.

**Acceptance Criteria:**
- `POST /api/tools/{toolId}/reviews` creates a new review (returns 201)
- `GET /api/tools/{toolId}/reviews` returns approved reviews for a tool (paginated)
- `POST /api/reviews/{reviewId}/comments` adds a comment to a review
- `GET /api/reviews/{reviewId}/comments` fetches comments for a review
- `POST /api/reviews/{reviewId}/response` allows staff to add a company response
- Validation errors return 400 with meaningful messages
- Unit tests cover all review service logic and edge cases

**Story Points:** 8
**Priority:** Must
**Sprint:** 3

---

### US-2.7 – User registration and login

**As a** returning customer,
**I want to** create an account and log in,
**so that** my reviews are linked to my profile and I do not have to enter my details every time.

**Acceptance Criteria:**
- Registration requires: name, email, and password
- Password must be at least 8 characters with at least one number and one uppercase letter
- Login returns a JWT token that is stored on the client
- Logged-in users see their name in the header and can access "My Reviews"
- Logged-out users can still browse the catalogue but must log in or provide details to leave a review
- Authentication uses ASP.NET Identity with JWT bearer tokens

**Story Points:** 8
**Priority:** Must
**Sprint:** 2

**Tasks:**

| # | Task | Owner |
|---|------|-------|
| 1 | Configure ASP.NET Identity with JWT bearer token generation | Backend |
| 2 | Implement registration service with password policy enforcement (min 8 chars, one number, one uppercase) | Backend |
| 3 | Implement login service that validates credentials and returns a JWT token | Backend |
| 4 | Build registration form (name, email, password) with client-side validation | Frontend |
| 5 | Build login form with clear error handling (generic error message, no field-specific hints) | Frontend |
| 6 | Store JWT token on the client and display the user's name in the site header | Frontend |
| 7 | Conditionally show "My Reviews" link for authenticated users | Frontend |
| 8 | Ensure GDPR-compliant data handling — collect only necessary personal data, store passwords securely via Identity's hashing | Backend |
| 9 | Write unit tests for registration validation, login flow, and token generation | Testing |

---

### US-2.8 – My reviews page

**As a** logged-in customer,
**I want to** see a list of all the reviews I have submitted,
**so that** I can track which ones have been approved and check for any responses.

**Acceptance Criteria:**
- "My Reviews" page is accessible from the user menu when logged in
- Each review shows: tool name, date submitted, current status (Pending / Approved / Rejected), and a snippet of the review text
- Clicking a review takes the user to the tool page where it is displayed
- If a review was rejected, a brief reason is shown (set by the moderator)

**Story Points:** 3
**Priority:** Should
**Sprint:** 4

**Tasks:**

| # | Task | Owner |
|---|------|-------|
| 1 | Create `ReviewService` method to return all reviews by the authenticated user, including status and rejection reason | Backend |
| 2 | Build "My Reviews" page accessible from the user menu | Frontend |
| 3 | Display each review as a list item (tool name, date, status badge, text snippet) | Frontend |
| 4 | Link each review to the corresponding tool detail page | Frontend |
| 5 | Show the moderator's rejection reason for any rejected reviews | Frontend |
| 6 | Write unit tests for the user-specific review retrieval service method | Testing |

---

### US-2.9 – Review database schema

*[Technical — kept for reference]*

**As a** developer,
**I want** the database schema for reviews, comments, and ratings,
**so that** we can store and query review data efficiently.

**Acceptance Criteria:**
- `Reviews` table: Id, ToolId (FK), UserId (or ReviewerName/Email for anonymous), ReviewText, EquipmentRating, CustomerServiceRating, TechnicalSupportRating, AfterSalesRating, ValueForMoneyRating, OverallRating (computed), Status (Pending/Approved/Rejected), RejectionReason, CreatedDate
- `ReviewComments` table: Id, ReviewId (FK), CommenterName, CommentText, Status, CreatedDate
- `CompanyResponses` table: Id, ReviewId (FK), ResponseText, StaffUserId, CreatedDate, UpdatedDate
- Proper indexes on ToolId and Status columns for query performance
- EF Core migrations created and tested

**Story Points:** 5
**Priority:** Must
**Sprint:** 2

---

## Summary for Sprint Planning

| Sprint | Stories | Total Points |
|--------|---------|-------------|
| Sprint 2 | US-2.7, US-2.9 | 13 |
| Sprint 3 | US-2.1, US-2.2, US-2.3, US-2.4, US-2.5, US-2.6 | 36 |
| Sprint 4 | US-2.8 | 3 |
