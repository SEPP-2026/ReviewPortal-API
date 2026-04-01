# Shelton Tool-Hire Review Portal – Product Backlog

## Project Overview

Shelton Tool-Hire wants a web-based review portal so customers can browse the hire catalogue, check rental prices, calculate costs for specific periods, and leave honest reviews about the tools and the service they received. On the business side, staff need a way to manage equipment listings, adjust pricing, upload images, and moderate the reviews before they go live.

We sat down as a team and talked through what the company actually needs versus what would be nice to have. We looked at sites like HSS Hire, Speedy Hire, Jewson Tool Hire, and Brandon Hire Station to get a feel for how the industry handles categories and pricing. Most of them break things down by trade or job type, so we have gone with something similar.

---

## User Personas

We defined three personas to keep user needs front and centre during development:

**Dave the DIY Customer** — 35-year-old homeowner. Browses at weekends looking for the right tool for a home project. Wants clear pricing, honest reviews, and an easy way to work out what it will cost him. Gets frustrated by hidden charges and tools that turn up in poor condition.

**Sarah the Site Manager** — 42, manages a small construction firm. Uses the portal daily to compare hire rates and check equipment reliability. Needs fast search, accurate pricing across different hire periods, and values detailed reviews about technical support quality.

**Mark the Moderator** — 28, works in Shelton's customer service team. Responsible for keeping the review section clean, responding to customer concerns, and updating equipment listings when prices change. Needs an efficient moderation queue and straightforward content management tools.

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

**Note on the 5th category:** The brief suggested "Miscellaneous" as the fifth rating category. We discussed this as a team and decided to replace it with "Value for Money" for the following reasons:
- A "Miscellaneous" rating produces vague, hard-to-interpret scores — customers do not know what they are rating, and the data is not actionable for the business.
- Looking at established review platforms (Trustpilot, Google Reviews for hire companies like HSS and Speedy), "Value for Money" is consistently one of the most common and most valued rating dimensions.
- Value for Money gives Shelton a specific, measurable metric that directly influences customer purchase decisions and can inform pricing strategy.
- This change was unanimously agreed by the team after researching competitor review structures.

---

## Tools and Services

The brief refers to "tools and services". After discussion, we decided to model services (such as equipment delivery, operator hire, or PAT testing) as tools within the catalogue rather than creating a separate entity. Services share the same attributes as tools — name, description, hire rates, images, and reviews — so a separate schema would duplicate structure without adding value. Where Shelton offers services, they are listed under a "Services" category alongside the physical equipment categories. This keeps the data model clean while covering the full range of offerings.

---

## Tech Stack

- **Backend:** ASP.NET Core Web API (.NET 8)
- **Frontend:** Next.js (React)
- **Database:** Microsoft SQL Server
- **Testing:** xUnit (unit tests), Playwright (end-to-end)
- **CI/CD:** GitHub Actions
- **Auth:** ASP.NET Identity with JWT tokens

---

## MoSCoW Prioritisation

We have assigned MoSCoW priorities to all user stories to make scope decisions clear:

| Priority | Meaning | Stories |
|----------|---------|--------|
| **Must** | Core requirements — the system does not meet the brief without these | US-1.1, US-1.2, US-1.3, US-1.4, US-1.5, US-1.8, US-1.9, US-2.1, US-2.2, US-2.3, US-2.4, US-2.5, US-2.6, US-2.7, US-2.9, US-3.1, US-3.2, US-3.3, US-3.4, US-3.6, US-3.9 |
| **Should** | Important for a complete product but the prototype could function without them | US-1.6, US-1.7, US-2.8, US-3.5, US-3.7, US-3.10 |
| **Could** | Adds polish but can be dropped if time is short | US-1.10 (CI pipeline), US-3.8 (dashboard) |
| **Won't** (this phase) | Explicitly out of scope for this prototype | Real payment integration, booking system, stock/availability tracking, multi-language support |

---

## Definition of Done

A story is done when:
- Code is written and peer-reviewed via pull request
- Unit tests pass
- Feature works as described in acceptance criteria
- No obvious bugs on a quick manual check
- Merged into the develop branch
