# Shelton Tool-Hire Review Portal – Product Backlog

## Project Overview

Shelton Tool-Hire requires a web-based review portal that allows customers to browse the hire catalogue, check rental prices, calculate costs for specific hire periods, and leave honest reviews about the tools/services and the service they received. On the business side, staff need the ability to manage equipment/service listings, adjust pricing, upload images, and moderate reviews before they are published.

As a team, we discussed what the company actually needs compared to what would simply be nice to have. We reviewed competitor sites such as HSS Hire, Speedy Hire, Jewson Tool Hire, and Brandon Hire Station to understand how the hire industry typically handles categories and pricing. Most competitors organise their catalogues by trade or job type, so we have followed a similar approach.

---

## User Personas

We defined three personas to keep user needs at the centre of development:

**Dave the DIY Customer** — A 35-year-old homeowner who browses at weekends looking for the right tool/service for a home project. He wants clear pricing, honest reviews, and a straightforward way to work out what it will cost. He gets frustrated by hidden charges and tools that arrive in poor condition.

**Sarah the Site Manager** — A 42-year-old who manages a small construction firm. She uses the portal daily to compare hire rates and check equipment reliability. She needs fast search, accurate pricing across different hire periods, and values detailed reviews about technical support quality.

**Mark the Moderator** — A 28-year-old member of Shelton's customer service team. He is responsible for keeping the review section clean, responding to customer concerns, and updating equipment listings when prices change. He needs an efficient moderation queue and straightforward content management tools.

---

## Our Epics

We have settled on three core epics. Each one represents a meaningful portion of value that can be delivered and demonstrated at sprint reviews.

| # | Epic | Who Benefits | Summary |
|---|------|-------------|---------|
| 1 | Tool/Service Catalogue & Rental Calculator | Customers | Browse, search, and calculate the cost of tool/service hire |
| 2 | Reviews, Ratings & Community Interaction | Customers & the business | Leave reviews, comment, respond, and rate the service |
| 3 | Back-Office Management & Moderation | Shelton staff | Manage listings, pricing, images, and moderate content |

---

## Sprint Plan (4 Sprints × 2 Weeks Each)

| Sprint | Dates (Approx.) | Focus |
|--------|-----------------|-------|
| Sprint 1 | Weeks 1–2 | Project setup, database schema, authentication, basic catalogue browsing |
| Sprint 2 | Weeks 3–4 | Rental calculator, search and filtering, category pages |
| Sprint 3 | Weeks 5–6 | Reviews and ratings system, commenting, moderation queue |
| Sprint 4 | Weeks 7–8 | Admin dashboard, polish, testing, CI/CD, final integration |

---

## Definitions and Design Decisions

### Tool/Service Categories

After reviewing competitor offerings and considering what Shelton's customers would realistically search for, we agreed on the following categories:

- **Building & Construction** – cement mixers, scaffolding, concrete saws, etc.
- **Cleaning & Maintenance** – pressure washers, industrial vacuums, floor scrubbers
- **Painting & Decorating** – paint sprayers, wallpaper steamers, sanding machines
- **Garden & Landscaping** – hedge trimmers, rotavators, chippers, turf cutters
- **Electrical & Heating** – fan heaters, dehumidifiers, cable detectors, PAT testers
- **Plumbing & Drainage** – pipe freezing kits, drain rods, pipe cutters
- **Access & Lifting** – cherry pickers, platform ladders, hoists
- **Breaking & Drilling** – breakers, core drills, SDS drills, diamond blades
- **Services** – equipment delivery, trained operator hire, PAT testing, site surveys

---

### Review Categories

When a customer writes a review, they rate their experience across these five areas (each out of 5 stars):

1. **Equipment Performance** – Did the tool/service do the job? Was it in good condition?
2. **Booking & Customer Service** – How easy was it to book? Were the staff helpful?
3. **Technical Support & Guidance** – Did they explain how to use it properly? Was the advice useful?
4. **After-Sales & Breakdown Support** – What happened when something went wrong? Was out-of-hours help available?
5. **Value for Money** – Was the price fair for what was provided?

The overall rating is an average of all five categories. We considered weighting them differently but decided to keep it straightforward for the prototype.

**Note on the 5th category:** The project brief suggested "Miscellaneous" as the fifth rating category. We discussed this as a team and decided to replace it with "Value for Money" for the following reasons:
- A "Miscellaneous" rating produces vague, difficult-to-interpret scores — customers would not know what they are rating, and the data would not be useful to the business.
- Established review platforms (Trustpilot, Google Reviews for hire companies such as HSS and Speedy) consistently include "Value for Money" as one of the most common and most valued rating dimensions.
- "Value for Money" gives Shelton a specific, measurable metric that directly influences customer purchasing decisions and can inform pricing strategy.
- This change was unanimously agreed by the team after researching competitor review structures.

---

### Service Handling

The project brief refers to "tools and services". After discussion, we decided to model services (such as equipment delivery, operator hire, or PAT testing) as tools within the catalogue rather than creating a separate entity. Services share the same attributes as tools — name, description, hire rates, images, and reviews — so a separate schema would duplicate structure without adding value. Where Shelton offers services, they are listed under a "Services" category alongside the physical equipment categories. This keeps the data model clean while covering the full range of offerings.

---

### Rating Aggregation

The rating model is defined as follows:

- Each review stores five category scores: Equipment Performance, Booking & Customer Service, Technical Support & Guidance, After-Sales & Breakdown Support, and Value for Money.
- Review-level overall rating is calculated as `(Equipment + Customer + Technical + AfterSales + Value) / 5`.
- Tool/service-level overall rating is calculated from approved reviews only.
- Cached `OverallRating` and `ReviewCount` values are stored on the `Tool` record so catalogue and detail pages can load rating summaries without recalculating every request.
- If a tool/service has fewer than 2 approved reviews, the frontend may display "Not enough reviews to rate" instead of a numeric score.

### Moderation Rules

The moderation workflow is defined as follows:

- All customer reviews are created with `Pending` status and require moderator approval before public display.
- All customer comments are created with `Pending` status and require moderator approval before public display.
- Moderators/admins can approve or reject reviews and comments.
- Rejections should include a short reason, especially for cases such as offensive, irrelevant, or spam content.
- Company responses are official staff content, so they bypass moderation and publish immediately.

### Pricing Logic

The rental calculator follows these rules:

- Every tool/service stores hourly, daily, and weekly hire rates.
- The customer supplies a start date/time and an end date/time.
- The end date/time must be later than the start date/time; otherwise the calculator returns a validation error.
- The calculator chooses the cheapest valid combination of hourly, daily, and weekly rates for the requested hire period.
- The response includes a cost breakdown showing the rate tiers used plus the final total.

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

We assigned MoSCoW priorities to all user stories to make scope decisions clear:

| Priority | Meaning | Stories |
|----------|---------|--------|
| **Must** | Core requirements — the system does not meet the brief without these | US-1.1, US-1.2, US-1.3, US-1.4, US-1.5, US-1.8, US-1.9, US-2.1, US-2.2, US-2.3, US-2.4, US-2.5, US-2.6, US-2.7, US-2.9, US-3.1, US-3.2, US-3.3, US-3.4, US-3.6, US-3.9 |
| **Should** | Important for a complete product but the prototype could work without them | US-1.6, US-1.7, US-2.8, US-3.5, US-3.7, US-3.10 |
| **Could** | Adds polish but can be dropped if time is limited | US-1.10 (CI pipeline), US-3.8 (dashboard) |
| **Won't** (this phase) | Explicitly out of scope for this prototype | Real payment integration, booking system, stock/availability tracking, multi-language support |

---

## Definition of Done

A story is considered done when:
- Code is written and peer-reviewed through a pull request
- Unit tests pass
- The feature works as described in the acceptance criteria
- No obvious bugs are found during a quick manual check
- Code is merged into the develop branch
