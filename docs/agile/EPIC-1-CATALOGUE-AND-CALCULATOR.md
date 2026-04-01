# Epic 1 – Tool Catalogue & Rental Calculator

**As a customer, I want to browse Shelton's full range of hire equipment, find what I need quickly, and work out exactly how much it will cost me before I commit to booking.**

This epic covers everything a customer interacts with when they land on the site and start looking for tools. It includes the homepage, category pages, individual tool detail pages, search functionality, and the rental cost calculator.

---

## User Stories

### US-1.1 – Homepage with featured categories

**As a** first-time visitor to the site,
**I want to** see the main tool categories laid out clearly on the homepage,
**so that** I can quickly find the type of equipment I am looking for without having to dig around.

**Acceptance Criteria:**
- Homepage displays all tool categories with a relevant image and name for each
- Clicking a category takes the user to a filtered catalogue page
- The page loads within 3 seconds on a reasonable internet connection
- Works sensibly on mobile screens as well as desktop

**Story Points:** 5
**Priority:** Must
**Sprint:** 1

---

### US-1.2 – Category browsing page

**As a** customer looking for a specific type of tool,
**I want to** see all available tools within a chosen category,
**so that** I can compare options and decide which one suits my job.

**Acceptance Criteria:**
- Each category page shows a list of tools with thumbnail image, name, and starting price
- Tools can be sorted by name or price (low to high, high to low)
- Pagination or infinite scroll is used if there are more than 12 items
- An empty state message shows if a category has no tools listed yet

**Story Points:** 5
**Priority:** Must
**Sprint:** 1

---

### US-1.3 – Tool detail page

**As a** customer who has found a tool that looks right,
**I want to** see full details including description, images, and hire rates,
**so that** I can make an informed decision about whether to hire it.

**Acceptance Criteria:**
- Page shows tool name, full description, multiple images (with ability to click through them), and the category it belongs to
- Hire rates are displayed clearly: hourly rate, daily rate, and weekly rate
- Any special notes or requirements (e.g. "requires a deposit" or "needs a trained operator") are shown
- The page includes a link or section for the rental calculator (see US-1.5)
- Average customer rating is visible if reviews exist (links to Epic 2)

**Story Points:** 5
**Priority:** Must
**Sprint:** 1

---

### US-1.4 – Search for tools

**As a** customer who knows roughly what they need,
**I want to** type in a search term and get relevant results back,
**so that** I do not have to browse through every category manually.

**Acceptance Criteria:**
- A search bar is visible on every page (header/nav area)
- Searching returns results matching tool name, description, or category
- Results show thumbnail, name, category, and starting hire price
- If nothing matches, a helpful message is displayed (e.g. "No results found – try a different term or browse our categories")
- Search is not case-sensitive

**Story Points:** 5
**Priority:** Must
**Sprint:** 2

---

### US-1.5 – Rental cost calculator

**As a** customer who wants to hire a specific tool,
**I want to** enter the dates and times I need it for and see the total cost,
**so that** I can budget properly and avoid surprises.

**Acceptance Criteria:**
- Calculator is available on each tool detail page
- User picks a start date/time and an end date/time using a date-time picker
- The system calculates the cost based on the tool's stored hourly, daily, and weekly rates
- Calculation logic: the system should work out the cheapest combination (e.g. 3 days is cheaper as a weekly rate if the weekly rate is less than 3 × daily)
- A breakdown is shown: "2 days × £45/day + 3 hours × £8/hour = £99"
- Validation prevents selecting an end date before the start date
- If the selected period is zero or negative, show a sensible error

**Story Points:** 8
**Priority:** Must
**Sprint:** 2

---

### US-1.6 – Filter tools by price range

**As a** budget-conscious customer,
**I want to** filter the catalogue by a price range,
**so that** I only see tools that fit within what I can afford.

**Acceptance Criteria:**
- A price range filter is available on category pages (min/max input or slider)
- Filtering is based on the daily hire rate
- Results update without a full page reload
- The filter can be cleared to show all results again

**Story Points:** 3
**Priority:** Should
**Sprint:** 2

---

### US-1.7 – Responsive mobile layout for catalogue

**As a** customer browsing on my phone,
**I want** the catalogue and tool pages to work properly on a small screen,
**so that** I can look things up on site without needing a laptop.

**Acceptance Criteria:**
- All catalogue pages (homepage, category, detail, search results) are usable on screens 375px wide and up
- Images resize appropriately
- The search bar is accessible on mobile
- Navigation does not break or overlap on smaller screens

**Story Points:** 5
**Priority:** Should
**Sprint:** 2

---

### US-1.8 – API endpoints for tools and categories

**As a** developer working on the front end,
**I want** RESTful API endpoints for categories and tools,
**so that** we can fetch and display catalogue data from the database.

**Acceptance Criteria:**
- `GET /api/categories` returns all categories
- `GET /api/categories/{id}/tools` returns tools in a specific category
- `GET /api/tools/{id}` returns full detail for a single tool (including hire rates)
- `GET /api/tools/search?q={term}` returns search results
- All endpoints return proper HTTP status codes (200, 404, etc.)
- Responses are in JSON format
- Unit tests cover the controller and service logic

**Story Points:** 8
**Priority:** Must
**Sprint:** 1

---

### US-1.9 – Database schema for tools and categories

**As a** developer,
**I want** a properly structured database with tables for categories, tools, and hire rates,
**so that** we have a solid foundation to build on.

**Acceptance Criteria:**
- `Categories` table with Id, Name, Description, ImageUrl
- `Tools` table with Id, Name, Description, CategoryId (FK), HourlyRate, DailyRate, WeeklyRate, SpecialNotes, DepositRequired, DepositAmount, IsActive, OverallRating, ReviewCount, CreatedDate, UpdatedDate
- `ToolImages` table with Id, ToolId (FK), ImageUrl, DisplayOrder, UploadedDate — images are stored in a separate table rather than a JSON column, allowing proper indexing and individual image management
- Seed data includes at least 3 categories with 4-5 tools each
- EF Core migrations are set up and working
- Database can be recreated from migrations on any team member's machine

**Story Points:** 5
**Priority:** Must
**Sprint:** 1

---

### US-1.10 – Project scaffolding and CI pipeline

**As a** team member,
**I want** the project structure, build pipeline, and dev environment set up,
**so that** everyone can start contributing code from day one.

**Acceptance Criteria:**
- ASP.NET Core Web API project created with standard folder structure
- Next.js project initialised with basic routing
- GitHub Actions workflow runs on every push: builds both projects, runs unit tests
- README updated with setup instructions (how to run locally, connection string config, etc.)
- .gitignore covers all the usual suspects (bin, obj, node_modules, .env)

**Story Points:** 5
**Priority:** Could
**Sprint:** 1

---

## Summary for Sprint Planning

| Sprint | Stories | Total Points |
|--------|---------|-------------|
| Sprint 1 | US-1.1, US-1.2, US-1.3, US-1.8, US-1.9, US-1.10 | 33 |
| Sprint 2 | US-1.4, US-1.5, US-1.6, US-1.7 | 21 |
