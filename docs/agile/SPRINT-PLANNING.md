# Sprint Planning – All Sprints

This document gives a consolidated view of how we have split the work across all four sprints. Each sprint is two weeks.

---

## Sprint 1 – Foundation (Weeks 1–2)

**Sprint Goal:** Get the project infrastructure in place and deliver a working, browsable tool catalogue with real data coming from the API.

| Story ID | Story Title | Epic | Points | Owner |
|----------|-------------|------|--------|-------|
| US-1.10 | Project scaffolding and CI pipeline | 1 | 5 | TBD |
| US-1.9 | Database schema for tools and categories | 1 | 5 | TBD |
| US-1.8 | API endpoints for tools and categories | 1 | 8 | TBD |
| US-1.1 | Homepage with featured categories | 1 | 5 | TBD |
| US-1.2 | Category browsing page | 1 | 5 | TBD |
| US-1.3 | Tool detail page | 1 | 5 | TBD |

**Total Points:** 33

**What we should be able to demo at Sprint Review:**
- Running API that returns categories and tools from a SQL database
- A working frontend that shows the homepage with categories, a category page with tool listings, and a tool detail page
- GitHub Actions pipeline that builds and runs tests on every push

---

## Sprint 2 – Search, Calculator & Auth (Weeks 3–4)

**Sprint Goal:** Let customers find tools effectively and see how much hire will cost. Set up user registration and admin authentication for Sprint 3.

| Story ID | Story Title | Epic | Points | Owner |
|----------|-------------|------|--------|-------|
| US-1.4 | Search for tools | 1 | 5 | TBD |
| US-1.5 | Rental cost calculator | 1 | 8 | TBD |
| US-1.6 | Filter tools by price range | 1 | 3 | TBD |
| US-1.7 | Responsive mobile layout for catalogue | 1 | 5 | TBD |
| US-2.7 | User registration and login | 2 | 8 | TBD |
| US-2.9 | Review database schema | 2 | 5 | TBD |
| US-3.1 | Admin login and role-based access | 3 | 5 | TBD |

**Total Points:** 39

**What we should be able to demo at Sprint Review:**
- Search that finds tools by keyword
- Rental calculator on each tool page that works out costs for a given date range
- Price filtering on category pages
- User registration and login working with JWT tokens
- Admin login with separate access level
- Mobile-friendly layout across the catalogue

---

## Sprint 3 – Reviews & Moderation (Weeks 5–6)

**Sprint Goal:** Deliver the full review and rating system including moderation, so customers can leave feedback and the business can manage it.

| Story ID | Story Title | Epic | Points | Owner |
|----------|-------------|------|--------|-------|
| US-2.1 | Submit a review for a tool | 2 | 8 | TBD |
| US-2.2 | Display approved reviews on tool page | 2 | 5 | TBD |
| US-2.3 | Overall tool ranking based on ratings | 2 | 5 | TBD |
| US-2.4 | Comment on someone else's review | 2 | 5 | TBD |
| US-2.5 | Company response to a review | 2 | 5 | TBD |
| US-2.6 | Review API endpoints | 2 | 8 | TBD |
| US-3.6 | Review moderation queue | 3 | 8 | TBD |
| US-3.9 | Admin API endpoints (moderation part) | 3 | 6 | TBD |

**Total Points:** 50

**What we should be able to demo at Sprint Review:**
- Customers can submit a review with star ratings across five categories
- Reviews go into a moderation queue and are only visible after approval
- Comments on reviews, also moderated
- Company can respond to reviews from the admin side
- Tools show average ratings and can be sorted by rating
- Moderation queue in the admin area with approve/reject functionality

---

## Sprint 4 – Admin Tools, Polish & Testing (Weeks 7–8)

**Sprint Goal:** Complete the back-office features, tie up loose ends, polish the UI, and make sure the whole system is tested and reliable.

| Story ID | Story Title | Epic | Points | Owner |
|----------|-------------|------|--------|-------|
| US-3.2 | Add new equipment to the catalogue | 3 | 5 | TBD |
| US-3.3 | Edit existing equipment details and pricing | 3 | 5 | TBD |
| US-3.4 | Manage tool images | 3 | 5 | TBD |
| US-3.5 | Deactivate or remove equipment | 3 | 3 | TBD |
| US-3.7 | Manage categories | 3 | 3 | TBD |
| US-3.8 | Admin dashboard with overview stats | 3 | 5 | TBD |
| US-3.9 | Admin API endpoints (remaining CRUD) | 3 | 7 | TBD |
| US-3.10 | Playwright end-to-end tests | 3 | 8 | TBD |
| US-2.8 | My reviews page | 2 | 3 | TBD |

**Total Points:** 44

**What we should be able to demo at Sprint Review:**
- Full admin dashboard with stats
- Admins can add, edit, deactivate equipment and manage images
- Category management
- "My Reviews" page for logged-in customers
- Playwright tests covering the three main user journeys
- GitHub Actions pipeline running full test suite
- Everything polished and ready for final presentation

---

## Velocity Notes

We are estimating a team velocity of roughly 35–45 points per sprint. Sprint 3 is the heaviest at 50 points, but a good chunk of that (the API endpoints) can be started towards the end of Sprint 2 if we finish early. We will reassess after Sprint 1 based on how much we actually got through.

If Sprint 3 looks like it is going to slip, we will move US-2.4 (comments on reviews) or US-2.5 (company responses) into Sprint 4 as they are not critical for the core review flow.
