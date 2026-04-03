# Non-Functional Requirements — Shelton Tool-Hire Review Portal

This document captures the quality attributes and non-functional requirements that the system must satisfy beyond its functional capabilities. These are grouped by ISO 25010 quality characteristics where appropriate.

---

## 1. Performance

| ID | Requirement | Target | Verification |
|----|-------------|--------|-------------|
| NFR-01 | Homepage and category pages shall load within 3 seconds on a standard broadband connection | < 3s initial load | Lighthouse audit, manual testing |
| NFR-02 | API endpoints shall respond within 500ms for standard queries (tool detail, category listing) | < 500ms p95 | Integration test timing, logs |
| NFR-03 | Search results shall be returned within 1 second | < 1s | Integration test timing |
| NFR-04 | The rental calculator shall compute results client-side with no perceptible delay after the API returns rate data | < 100ms client-side | Manual testing |
| NFR-05 | Catalogue pages shall use pagination (max 12 items per page) to prevent excessive data transfer | Max 12 items/page | API response validation |

---

## 2. Security

| ID | Requirement | Target | Verification |
|----|-------------|--------|-------------|
| NFR-06 | All passwords shall be hashed using ASP.NET Identity's default PBKDF2 algorithm — never stored in plain text | Hashed storage | Code review, unit test |
| NFR-07 | Password policy shall enforce a minimum of 8 characters with at least one uppercase letter and one number | Policy enforced | Unit test, integration test |
| NFR-08 | Authentication tokens (JWT) shall expire after 60 minutes | 60-min expiry | Integration test |
| NFR-09 | Admin API endpoints shall require both authentication and the "Admin" or "Moderator" role | Role-based access | Integration test (401/403) |
| NFR-10 | Failed login attempts shall display a generic error message that does not reveal whether the email or password was incorrect | Generic message | Manual testing |
| NFR-11 | All user inputs shall be validated server-side using FluentValidation to prevent injection attacks | Input sanitised | Unit tests on validators |
| NFR-12 | HTTPS shall be enforced in production | TLS required | Configuration review |
| NFR-13 | CORS policy shall restrict allowed origins to the frontend domain in production | Restricted origins | Configuration review |

---

## 3. Usability

| ID | Requirement | Target | Verification |
|----|-------------|--------|-------------|
| NFR-14 | The portal shall be fully usable on screens from 375px wide (mobile) up to 1920px (desktop) | Responsive 375px+ | Manual testing, Playwright |
| NFR-15 | Navigation shall be consistent and accessible across all pages | Consistent nav | Manual testing |
| NFR-16 | Form validation errors shall be displayed inline next to the relevant field with clear, human-readable messages | Inline errors | Manual testing, E2E tests |
| NFR-17 | The admin back-office shall have its own layout and navigation, visually distinct from the public portal | Separate layout | Manual testing |
| NFR-18 | Empty states shall display helpful messages (e.g. "No reviews yet", "No tools in this category") rather than blank pages | Friendly empty states | Manual testing |

---

## 4. Accessibility

| ID | Requirement | Target | Verification |
|----|-------------|--------|-------------|
| NFR-19 | The portal shall target WCAG 2.1 Level AA compliance | AA compliant | Lighthouse accessibility audit |
| NFR-20 | All images shall have descriptive `alt` text | Alt text present | HTML validation |
| NFR-21 | Interactive elements shall be navigable via keyboard (tab order, enter to activate) | Keyboard accessible | Manual testing |
| NFR-22 | Colour contrast ratios shall meet WCAG AA minimum (4.5:1 for normal text, 3:1 for large text) | AA contrast | Lighthouse audit |
| NFR-23 | Star rating inputs shall be operable without a mouse (e.g. radio buttons or arrow keys) | Keyboard operable | Manual testing |

---

## 5. Reliability and Availability

| ID | Requirement | Target | Verification |
|----|-------------|--------|-------------|
| NFR-24 | The system shall handle unexpected errors gracefully using global exception handling middleware, returning structured ProblemDetails JSON | No raw stack traces | Integration test |
| NFR-25 | Database operations shall use transactions where multiple writes are involved (e.g. approving a review and updating the tool's cached rating) | Transactional integrity | Code review |
| NFR-26 | No data shall be hard-deleted — all deletions use soft-delete (IsActive flag) to preserve audit history and review linkage | Soft-delete only | Code review, integration test |

---

## 6. Scalability

| ID | Requirement | Target | Verification |
|----|-------------|--------|-------------|
| NFR-27 | Database queries shall use appropriate indexes on frequently queried columns (ToolId, CategoryId, Status, IsActive) | Indexed queries | EF migration review |
| NFR-28 | Tool overall ratings and review counts shall be cached as denormalised fields on the Tools table to avoid expensive aggregate queries | Denormalised ratings | ERD review, code review |
| NFR-29 | All list endpoints shall support pagination to prevent unbounded result sets | Paginated responses | API testing |

---

## 7. Maintainability

| ID | Requirement | Target | Verification |
|----|-------------|--------|-------------|
| NFR-30 | The codebase shall follow Clean Architecture with strict layer separation: Domain → Application → Infrastructure → API | No layer violations | Code review, project reference check |
| NFR-31 | All API responses shall use DTOs — domain entities shall never be exposed directly | DTOs only | Code review |
| NFR-32 | EF Core entity configurations shall use Fluent API exclusively (no data annotations on entities) | Fluent API only | Code review |
| NFR-33 | The CI/CD pipeline shall build the solution and run all tests on every push to any branch | Automated CI | GitHub Actions workflow |
| NFR-34 | Code shall follow consistent naming conventions (file-scoped namespaces, async suffix, I-prefix for interfaces) | Consistent style | Code review, CLAUDE.md |

---

## 8. Data Integrity

| ID | Requirement | Target | Verification |
|----|-------------|--------|-------------|
| NFR-35 | Foreign key relationships shall be enforced at the database level | FK constraints | Migration review |
| NFR-36 | Review ratings shall be constrained to the range 1–5 | Range validation | FluentValidation + DB check constraint |
| NFR-37 | Each review shall have exactly one overall rating calculated as the arithmetic mean of the five individual ratings | Correct calculation | Unit test |
| NFR-38 | The CompanyResponses table shall enforce a unique constraint on ReviewId (one response per review) | Unique constraint | Migration review |
| NFR-39 | Audit timestamps (CreatedDate, UpdatedDate) shall be set automatically by the DbContext, not by client code | Auto-timestamps | Code review, integration test |
