# Test Plan - Black-Box, Dry Run, and Usability Testing

> Purpose: MSc submission test plan for the Shelton Tool-Hire Review Portal.
>
> Scope: Public catalogue, rental calculator, authentication, review workflow, moderation, admin catalogue management, database integrity, and user-facing usability checks.

---

## 1. Test Objectives

The test plan verifies that the system:

- satisfies the functional requirements in `docs/REQUIREMENTS-SPECIFICATION.md`
- protects database integrity through keys, indexes, constraints, and service validation
- allows customers to browse, search, calculate hire cost, and submit feedback
- supports staff moderation before reviews/comments become public
- enforces role-based access for admin and moderator operations
- is usable by representative customers and staff

---

## 2. Test Scope

### In Scope

| Area | Coverage |
|------|----------|
| Catalogue | Categories, category tools, search, sorting, filtering, pagination |
| Rental calculator | Date/time validation, cheapest hourly/daily/weekly cost calculation, cost breakdown |
| Authentication | Register, login, JWT claims, role-based access |
| Reviews | Submit review, pending status, approved review display, rating aggregation |
| Comments | Submit comment, pending status, approved comment display |
| Company responses | One response per approved review, staff-only access |
| Moderation | Pending queue, approve/reject reviews and comments, rejection reason |
| Admin tools | Create/update/status routes and Admin-only access |
| Database | Foreign keys, unique indexes, rating check constraints, status indexes |
| Usability | Key customer and staff tasks, navigation, clarity, error recovery |

### Out of Scope

| Area | Reason |
|------|--------|
| Payment processing | Not part of the prototype scope |
| Real booking/availability | Rental calculator estimates cost only |
| Production email delivery | Password reset token is generated but email delivery is not configured |
| Multi-language support | Not required for this prototype |

---

## 3. Test Environment

| Item | Test Environment |
|------|------------------|
| Backend | ASP.NET Core Web API (.NET 8) |
| Database | Local SQL Server or Azure SQL test database |
| API tools | Swagger, Postman, or HTTP client tests |
| Automated tests | xUnit unit and integration tests |
| Browser testing | Next.js frontend, or Swagger/API dry run where frontend is unavailable |
| Seed users | Customer, Admin, Moderator seeded test accounts |
| Seed data | Categories, tools/services, images, reviews, comments, company responses |

---

## 4. Entry and Exit Criteria

### Entry Criteria

| Criterion | Required Evidence |
|-----------|-------------------|
| Build completes successfully | `dotnet build ReviewPortal.slnx` passes |
| Test database exists | EF migrations applied successfully |
| Seed data available | Demo categories, tools/services, users, and reviews present |
| API can start | `/health` returns success |
| Test accounts available | Customer, Admin, and Moderator credentials known |

### Exit Criteria

| Criterion | Required Evidence |
|-----------|-------------------|
| Critical black-box tests pass | Test case results recorded |
| Dry run completed | Dry-run checklist signed off |
| Usability testing completed | Participant task results and observations recorded |
| Defects triaged | Severity, owner, and resolution decision recorded |
| No critical open defects | Critical and high defects fixed or formally accepted |

---

## 5. Black-Box Test Plan

Black-box testing validates behaviour from the user/API consumer perspective without relying on internal code knowledge.

### 5.1 Black-Box Test Cases

| ID | Requirement Area | Test Scenario | Input / Action | Expected Result |
|----|------------------|---------------|----------------|-----------------|
| BB-01 | Catalogue | View all categories | Open homepage or call `GET /api/categories` | Active categories are returned with name, description, and image |
| BB-02 | Category browsing | Browse tools/services in category | Select category or call `GET /api/categories/{id}/tools` | Tools/services for that category are returned with thumbnail, name, price, rating summary |
| BB-03 | Sorting | Sort category by price ascending | Use sort parameter/control | Results ordered from lowest to highest daily/starting price |
| BB-04 | Search | Search by keyword | Search for a known term such as "drill" | Matching tools/services are returned regardless of case |
| BB-05 | Empty search | Search with unmatched term | Search for random unmatched text | Friendly no-results response/message shown |
| BB-06 | Price filter | Filter by daily rate range | Apply min/max daily rate | Only tools/services inside range are returned |
| BB-07 | Tool detail | View a specific tool/service | Open detail page or call `GET /api/tools/{id}` | Description, images, rates, deposit data, rating summary returned |
| BB-08 | Rental calculator | Valid hire period | Start before end | Cost breakdown and total returned |
| BB-09 | Rental calculator validation | Invalid hire period | End date/time equal to or before start | HTTP 400 or validation message returned; no total calculated |
| BB-10 | Registration | Register new customer | Valid name, email, password | Account created and JWT returned |
| BB-11 | Registration validation | Weak password | Password missing uppercase or number | Validation error returned |
| BB-12 | Login | Valid login | Known user credentials | JWT returned with user details and role |
| BB-13 | Login failure | Invalid password | Wrong password | Generic invalid login response returned |
| BB-14 | Submit review | Valid review | Name/email, text over 20 chars, five ratings | Review created with `Pending` status and confirmation shown |
| BB-15 | Review validation | Missing rating | Submit review with one rating missing | Validation error returned |
| BB-16 | Public reviews | Approved reviews only | View tool reviews | Approved reviews shown; pending/rejected reviews hidden |
| BB-17 | Comment submission | Valid comment | Comment text over 10 chars | Comment saved as `Pending` |
| BB-18 | Company response | Add response to approved review | Staff submits response | Response saved and visible on public review |
| BB-19 | Company response guard | Add response to pending/rejected review | Staff submits response | Validation error returned |
| BB-20 | Moderation queue | View pending items | Moderator calls `GET /api/admin/moderation/pending` | Pending reviews/comments returned oldest first |
| BB-21 | Approve review | Moderator approves review | Approve pending review | Review becomes public and tool rating/count updates |
| BB-22 | Reject review | Moderator rejects with reason | Reject pending review | Review status becomes `Rejected` and reason stored |
| BB-23 | Admin auth | Admin route without token | Call `POST /api/admin/tools` without token | HTTP 401 returned |
| BB-24 | Admin role enforcement | Admin route with Customer token | Call admin route as Customer | HTTP 403 returned |
| BB-25 | Database constraint | Rating outside 1-5 | Attempt invalid review insert | Insert fails or API rejects invalid rating |

### 5.2 Black-Box Test Result Template

| Test ID | Date | Tester | Build/Version | Result | Evidence | Defect ID |
|---------|------|--------|---------------|--------|----------|-----------|
| BB-01 | | | | Pass / Fail | Screenshot, response body, or log | |

---

## 6. Dry Run Plan

Dry runs are full rehearsal checks before demonstration, submission, or deployment. They are designed to catch environment, data, and workflow problems.

### 6.1 Dry Run Checklist

| Step | Action | Expected Result | Evidence |
|------|--------|-----------------|----------|
| DR-01 | Pull latest branch and check working tree | Expected branch and files present | Git status screenshot/log |
| DR-02 | Restore packages | `dotnet restore ReviewPortal.slnx` succeeds | Terminal output |
| DR-03 | Build solution | `dotnet build ReviewPortal.slnx` succeeds | Terminal output |
| DR-04 | Apply migrations to local/test DB | `dotnet ef database update` succeeds | Migration output |
| DR-05 | Confirm latest migration | `__EFMigrationsHistory` contains latest migration | SQL query result |
| DR-06 | Start API | API starts without configuration error | Terminal output |
| DR-07 | Health check | `/health` returns success | Browser/API output |
| DR-08 | Swagger check | `/swagger` loads | Browser screenshot |
| DR-09 | Public catalogue smoke test | Categories/tools endpoints return data | API response |
| DR-10 | Rental calculator smoke test | Valid period returns cost breakdown | API response |
| DR-11 | Login smoke test | Customer/Admin/Moderator can log in | Token response |
| DR-12 | Review submission smoke test | Review saved as pending | API response/database check |
| DR-13 | Moderation smoke test | Moderator approves/rejects pending item | API response/database check |
| DR-14 | Public visibility check | Approved content visible, pending/rejected hidden | API response |
| DR-15 | Admin route security check | No token returns 401; Customer token returns 403 | API response |
| DR-16 | Final test run | `dotnet test ReviewPortal.slnx` passes | Terminal output |

### 6.2 Dry Run Defect Log

| Defect ID | Step | Description | Severity | Owner | Status | Resolution |
|-----------|------|-------------|----------|-------|--------|------------|
| DR-DEF-001 | | | Critical / High / Medium / Low | | Open / Fixed / Accepted | |

### 6.3 Dry Run Sign-Off

| Role | Name | Date | Decision |
|------|------|------|----------|
| Developer | | | Pass / Re-run required |
| Tester | | | Pass / Re-run required |
| Product reviewer | | | Pass / Re-run required |

---

## 7. Usability Testing Schema

Usability testing checks whether target users can complete core tasks efficiently and with acceptable clarity.

### 7.1 Participant Groups

| Group | Target Participants | Rationale |
|-------|---------------------|-----------|
| DIY customer | 2 to 3 participants | Represents casual catalogue browsing and price checking |
| Trade/site user | 1 to 2 participants | Represents faster search and comparison needs |
| Staff/admin user | 1 to 2 participants | Represents moderation and catalogue management tasks |

### 7.2 Usability Tasks

| Task ID | Participant Group | Task | Success Criteria |
|---------|-------------------|------|------------------|
| UT-01 | Customer | Find a tool/service from a category | Participant reaches correct detail page without assistance |
| UT-02 | Customer | Search for a known tool/service | Participant finds relevant result within 2 minutes |
| UT-03 | Customer | Calculate rental cost for a weekend hire | Participant enters valid dates and understands the total/breakdown |
| UT-04 | Customer | Submit a review | Participant completes review form and understands moderation message |
| UT-05 | Customer | Comment on an approved review | Participant submits a valid comment and understands pending status |
| UT-06 | Registered user | View My Reviews | Participant can find review status and rejection reason if present |
| UT-07 | Moderator | Open moderation queue and approve a review | Participant can approve without assistance |
| UT-08 | Moderator | Reject a review with a reason | Participant can enter reason and understand outcome |
| UT-09 | Admin | Add/edit a tool/service | Participant can locate required fields and save/update |
| UT-10 | Admin | Deactivate and reactivate a tool/service | Participant understands public visibility effect |

### 7.3 Usability Metrics

| Metric | Measurement Method | Target |
|--------|--------------------|--------|
| Task completion | Pass/fail per task | 80 percent or more completion without assistance |
| Time on task | Stopwatch or screen recording | Core customer tasks under 2 minutes |
| Error count | Count incorrect clicks, invalid submissions, abandoned steps | No critical task-blocking errors |
| Assistance required | Record prompts/help given | Minimal assistance for core tasks |
| Satisfaction | 1 to 5 post-task score | Average score 4 or above |
| Clarity of error messages | Participant explains what went wrong and how to fix it | Most participants understand validation messages |

### 7.4 Usability Observation Sheet

| Participant ID | Role | Task ID | Completed? | Time Taken | Errors | Assistance Given | Satisfaction 1-5 | Notes |
|----------------|------|---------|------------|------------|--------|------------------|-------------------|-------|
| P01 | | | Yes / No | | | | | |

### 7.5 Post-Test Questions

| Question | Response Type |
|----------|---------------|
| What was easiest to do? | Free text |
| What was hardest to do? | Free text |
| Did the rental cost breakdown make sense? | Yes/No plus comments |
| Did the review moderation message make sense? | Yes/No plus comments |
| Were any labels or buttons confusing? | Free text |
| How confident would you feel using this system without help? | 1 to 5 |

### 7.6 Usability Issue Log

| Issue ID | Task ID | Observation | Severity | Recommended Improvement | Status |
|----------|---------|-------------|----------|-------------------------|--------|
| UX-001 | | | Critical / High / Medium / Low | | Open / Fixed / Accepted |

---

## 8. Test Data

| Data Type | Required Examples |
|-----------|-------------------|
| Categories | Building & Construction, Cleaning & Maintenance, Garden & Landscaping, Services |
| Tools/services | At least one active item per key category, with hourly/daily/weekly rates |
| Reviews | Approved, Pending, and Rejected examples |
| Comments | Approved and Pending examples |
| Company responses | One response attached to an approved review |
| Users | Customer, Admin, Moderator |

---

## 9. Traceability to Submission Requirements

| Submission Area | Evidence in This Plan |
|-----------------|-----------------------|
| Black-box testing | Section 5 test cases and result template |
| Dry runs | Section 6 dry-run checklist, defect log, and sign-off |
| Usability testing schema | Section 7 participant groups, tasks, metrics, observation sheet, and issue log |
| Database schema validation | BB-25 and dry-run migration/database checks |
| Functional requirements coverage | Test cases map to catalogue, calculator, auth, reviews, moderation, and admin requirements |
