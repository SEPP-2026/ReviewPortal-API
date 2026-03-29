# Shelton Tool-Hire Review Portal – Product Backlog

## Project Overview

Shelton Tool-Hire wants a web-based review portal so customers can browse the hire catalogue, check rental prices, calculate costs for specific periods, and leave honest reviews about the tools and the service they received. On the business side, staff need a way to manage equipment listings, adjust pricing, upload images, and moderate the reviews before they go live.

We sat down as a team and talked through what the company actually needs versus what would be nice to have. We looked at sites like HSS Hire, Speedy Hire, Jewson Tool Hire, and Brandon Hire Station to get a feel for how the industry handles categories and pricing. Most of them break things down by trade or job type, so we have gone with something similar.

---

## Our Epics

We have settled on three core epics. Each one represents a chunk of value that we can deliver and demonstrate at sprint reviews.

| # | Epic | Who benefits | Quick summary |
|---|------|-------------|---------------|
| 1 | Tool Catalogue & Rental Calculator | Customers | Browse, search, and price up tool hire |
| 2 | Reviews, Ratings & Community Interaction | Customers & the business | Leave reviews, comment, respond, rate the service |
| 3 | Back-Office Management & Moderation | Shelton staff | Manage listings, pricing, images, and moderate content |

---

## Sprint Plan (4 Sprints × 2 weeks each)

| Sprint | Dates (approx.) | Focus |
|--------|-----------------|-------|
| Sprint 1 | Weeks 1–2 | Project setup, database schema, authentication, basic catalogue browsing |
| Sprint 2 | Weeks 3–4 | Rental calculator, search & filtering, category pages |
| Sprint 3 | Weeks 5–6 | Reviews & ratings system, commenting, moderation queue |
| Sprint 4 | Weeks 7–8 | Admin dashboard, polish, testing, CI/CD, final integration |

---

## Tool Categories

After looking at what competitors offer and thinking about what Shelton's customers would actually search for, we decided on these categories:

- **Building & Construction** – cement mixers, scaffolding, concrete saws, etc.
- **Cleaning & Maintenance** – pressure washers, industrial vacuums, floor scrubbers
- **Painting & Decorating** – paint sprayers, wallpaper steamers, sanding machines
- **Garden & Landscaping** – hedge trimmers, rotavators, chippers, turf cutters
- **Electrical & Heating** – fan heaters, dehumidifiers, cable detectors, PAT testers
- **Plumbing & Drainage** – pipe freezing kits, drain rods, pipe cutters
- **Access & Lifting** – cherry pickers, platform ladders, hoists
- **Breaking & Drilling** – breakers, core drills, SDS drills, diamond blades

---

## Review Categories

When a customer writes a review, they rate the experience across these areas (each out of 5 stars):

1. **Equipment Performance** – Did the tool do the job? Was it in good nick?
2. **Booking & Customer Service** – How easy was it to book? Were the staff helpful?
3. **Technical Support & Guidance** – Did they explain how to use it properly? Good advice?
4. **After-Sales & Breakdown Support** – What happened when something went wrong? Out-of-hours help?
5. **Value for Money** – Was the price fair for what you got?

The overall rating is an average of all five. We discussed weighting them differently but decided to keep it simple for the prototype.

---

## Tech Stack

- **Backend:** ASP.NET Core Web API (.NET 8)
- **Frontend:** Next.js (React)
- **Database:** Microsoft SQL Server
- **Testing:** xUnit (unit tests), Playwright (end-to-end)
- **CI/CD:** GitHub Actions
- **Auth:** ASP.NET Identity with JWT tokens

---

## Definition of Done

A story is done when:
- Code is written and peer-reviewed via pull request
- Unit tests pass
- Feature works as described in acceptance criteria
- No obvious bugs on a quick manual check
- Merged into the develop branch
