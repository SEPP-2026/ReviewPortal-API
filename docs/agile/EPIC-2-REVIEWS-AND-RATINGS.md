# Epic 2 – Reviews, Ratings & Community Interaction

**As a customer who has hired a tool from Shelton, I want to share my honest experience and read what other people think, so that future customers (and the company) can benefit from real feedback.**

This epic deals with the whole review lifecycle — writing a review, rating different aspects of the service, reading other people's reviews, commenting on them, and the company being able to respond. It also covers the rating system that feeds into overall tool rankings.

---

## User Stories

### US-2.1 – Submit a review for a tool

**As a** customer who has recently hired a tool,
**I want to** write a review and rate my experience,
**so that** I can help other customers decide and give Shelton feedback.

**Acceptance Criteria:**
- A "Write a Review" button is visible on every tool detail page
- The user must provide their name and email (or be logged in) to submit
- The review form includes:
  - A text field for the written review (minimum 20 characters, max 2000)
  - Star ratings (1–5) for each review category: Equipment Performance, Booking & Customer Service, Technical Support, After-Sales Support, Value for Money
- All five ratings are required before the form can be submitted
- On submission, the user sees a confirmation message: "Thanks for your review – it will be visible once our team has checked it"
- The review is saved with a status of "Pending" (not visible to other customers until approved)

**Story Points:** 8
**Sprint:** 3

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
**Sprint:** 3

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
- If there are fewer than 2 reviews, show "Not enough reviews to rate" instead of a number

**Story Points:** 5
**Sprint:** 3

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
**Sprint:** 3

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
**Sprint:** 3

---

### US-2.6 – Review API endpoints

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
**Sprint:** 2

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
**Sprint:** 4

---

### US-2.9 – Review database schema

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
**Sprint:** 2

---

## Summary for Sprint Planning

| Sprint | Stories | Total Points |
|--------|---------|-------------|
| Sprint 2 | US-2.7, US-2.9 | 13 |
| Sprint 3 | US-2.1, US-2.2, US-2.3, US-2.4, US-2.5, US-2.6 | 36 |
| Sprint 4 | US-2.8 | 3 |
