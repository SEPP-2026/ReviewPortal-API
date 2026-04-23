# Sprint Planning – All Sprints

This document provides a consolidated view of how we have distributed the work across all four sprints. Each sprint lasts two weeks.

---

## Sprint 1 – Foundation (Weeks 1–2)

**Sprint Goal:** Set up the project infrastructure and deliver a working, browsable tool/service catalogue with real data served by the API.

| Story ID | Story Title | Epic | Points | Owner |
|----------|-------------|------|--------|-------|
| US-1.10 | Project scaffolding and CI pipeline | 1 | 5 | TBD |
| US-1.9 | Database schema for tools and categories | 1 | 5 | TBD |
| US-1.8 | API endpoints for tools and categories | 1 | 8 | TBD |
| US-1.1 | Homepage with featured categories | 1 | 5 | TBD |
| US-1.2 | Category browsing page | 1 | 5 | TBD |
| US-1.3 | Tool/service detail page | 1 | 5 | TBD |

**Total Points:** 33

**What we should be able to demo at Sprint Review:**
- A running API that returns categories and tools/services from a SQL database
- A working frontend showing the homepage with categories, a category page with tool/service listings, and a tool/service detail page
- A GitHub Actions pipeline that builds and runs tests on every push

---

## Sprint 2 – Search, Calculator & Auth (Weeks 3–4)

**Sprint Goal:** Enable customers to find tools/services effectively and see how much hire will cost. Set up user registration and admin authentication in preparation for Sprint 3.

| Story ID | Story Title | Epic | Points | Owner |
|----------|-------------|------|--------|-------|
| US-1.4 | Search for tools/services | 1 | 5 | TBD |
| US-1.5 | Rental cost calculator | 1 | 8 | TBD |
| US-1.6 | Filter tools/services by price range | 1 | 3 | TBD |
| US-1.7 | Responsive mobile layout for catalogue | 1 | 5 | TBD |
| US-2.7 | User registration and login | 2 | 8 | TBD |
| US-2.9 | Review database schema | 2 | 5 | TBD |
| US-3.1 | Admin login and role-based access | 3 | 5 | TBD |

**Total Points:** 39

**What we should be able to demo at Sprint Review:**
- Search functionality that finds tools/services by keyword
- A rental calculator on each tool/service page that works out costs for a given date range
- Price filtering on category pages
- User registration and login working with JWT tokens
- Admin login with a separate access level
- A mobile-friendly layout across the catalogue

---

## Sprint 3 – Reviews & Moderation (Weeks 5–6)

**Sprint Goal:** Deliver the full review and rating system including moderation, so that customers can leave feedback and the business can manage it.

| Story ID | Story Title | Epic | Points | Owner |
|----------|-------------|------|--------|-------|
| US-2.1 | Submit a review for a tool/service | 2 | 8 | TBD |
| US-2.2 | Display approved reviews on the tool/service page | 2 | 5 | TBD |
| US-2.3 | Overall tool/service ranking based on ratings | 2 | 5 | TBD |
| US-2.4 | Comment on someone else's review | 2 | 5 | TBD |
| US-2.5 | Company response to a review | 2 | 5 | TBD |
| US-2.6 | Review API endpoints | 2 | 8 | TBD |
| US-3.6 | Review moderation queue | 3 | 8 | TBD |
| US-3.9 | Admin API endpoints (moderation part) | 3 | 6 | TBD |

**Total Points:** 50

**What we should be able to demo at Sprint Review:**
- Customers can submit a review with star ratings across five categories
- Reviews go into a moderation queue and are only visible after approval
- Comments on reviews, which are also moderated
- The company can respond to reviews from the admin side
- Tools/services display average ratings and can be sorted by rating
- A moderation queue in the admin area with approve and reject functionality

---

## Sprint 4 – Admin Tools, Polish & Testing (Weeks 7–8)

**Sprint Goal:** Complete the back-office features, address remaining items, polish the UI, and ensure the whole system is thoroughly tested and reliable.

| Story ID | Story Title | Epic | Points | Owner |
|----------|-------------|------|--------|-------|
| US-3.2 | Add new equipment or service to the catalogue | 3 | 5 | TBD |
| US-3.3 | Edit existing equipment/service details and pricing | 3 | 5 | TBD |
| US-3.4 | Manage tool/service images | 3 | 5 | TBD |
| US-3.5 | Deactivate or remove equipment/service | 3 | 3 | TBD |
| US-3.7 | Manage categories | 3 | 3 | TBD |
| US-3.8 | Admin dashboard with overview stats | 3 | 5 | TBD |
| US-3.9 | Admin API endpoints (remaining CRUD) | 3 | 7 | TBD |
| US-3.10 | Playwright end-to-end tests | 3 | 8 | TBD |
| US-2.8 | My reviews page | 2 | 3 | TBD |

**Total Points:** 44

**What we should be able to demo at Sprint Review:**
- A full admin dashboard with summary statistics
- Admins can add, edit, and deactivate equipment/services, as well as manage images
- Category management functionality
- A "My Reviews" page for logged-in customers
- Playwright tests covering the three main user journeys
- GitHub Actions pipeline running the full test suite
- A polished system ready for the final presentation

---

## Velocity Notes

We are estimating a team velocity of roughly 35–45 points per sprint. Sprint 3 is the heaviest at 50 points, but a significant portion of that work (the API endpoints) can be started towards the end of Sprint 2 if we finish early. We will reassess after Sprint 1 based on how much we actually completed.

If Sprint 3 looks likely to slip, we will move US-2.4 (comments on reviews) or US-2.5 (company responses) into Sprint 4, as they are not critical for the core review flow.
