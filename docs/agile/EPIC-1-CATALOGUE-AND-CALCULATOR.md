# Epic 1 – Tool Catalogue & Rental Calculator

**As a customer, I want to browse Shelton's full range of hire equipment, find what I need quickly, and work out exactly how much it will cost before I commit to booking.**

This epic covers everything a customer interacts with when they land on the site and start looking for tools. It includes the homepage, category pages, individual tool detail pages, search functionality, and the rental cost calculator.

---

## User Stories

### US-1.1 – Homepage with featured categories

**As a** first-time visitor to the site,
**I want to** see the main tool categories laid out clearly on the homepage,
**so that** I can quickly find the type of equipment I am looking for without having to search around.

**Acceptance Criteria:**
- Homepage displays all tool categories with a relevant image and name for each
- Clicking a category takes the user to a filtered catalogue page
- The page loads within 3 seconds on a reasonable internet connection
- Works properly on mobile screens as well as desktop

**Story Points:** 5
**Priority:** Must
**Sprint:** 1

**Tasks:**

| # | Task | Owner |
|---|------|-------|
| 1 | Create `CategoryService` method to retrieve all categories with images | Backend |
| 2 | Build homepage layout with category grid (responsive card-based design) | Frontend |
| 3 | Create reusable `CategoryCard` component displaying image and name | Frontend |
| 4 | Implement navigation from each category card to the category browsing page | Frontend |
| 5 | Ensure the page meets basic accessibility standards (alt text on images, keyboard navigation) | Frontend |
| 6 | Write unit tests for `CategoryService` retrieval logic | Testing |

---

### US-1.2 – Category browsing page

**As a** customer looking for a specific type of tool,
**I want to** see all available tools within a chosen category,
**so that** I can compare options and decide which one suits my job.

**Acceptance Criteria:**
- Each category page shows a list of tools with thumbnail image, name, and starting price
- Tools can be sorted by name or price (low to high, high to low)
- Pagination or infinite scroll is used if there are more than 12 items
- An empty state message is shown if a category has no tools listed yet

**Story Points:** 5
**Priority:** Must
**Sprint:** 1

**Tasks:**

| # | Task | Owner |
|---|------|-------|
| 1 | Create `ToolService` method to return paginated tools filtered by category | Backend |
| 2 | Implement sorting logic (by name, price ascending, price descending) in the service layer | Backend |
| 3 | Build category page layout with tool listing grid | Frontend |
| 4 | Create reusable `ToolCard` component (thumbnail, name, starting price) | Frontend |
| 5 | Add sort controls (dropdown or toggle buttons) to the page header | Frontend |
| 6 | Implement pagination component for results exceeding 12 items | Frontend |
| 7 | Handle and display empty state message when a category has no tools | Frontend |
| 8 | Write unit tests for sorting and pagination logic | Testing |

---

### US-1.3 – Tool detail page

**As a** customer who has found a tool that looks right,
**I want to** see full details including description, images, and hire rates,
**so that** I can make an informed decision about whether to hire it.

**Acceptance Criteria:**
- Page shows tool name, full description, multiple images (with the ability to click through them), and the category it belongs to
- Hire rates are displayed clearly: hourly rate, daily rate, and weekly rate
- Any special notes or requirements (e.g. "requires a deposit" or "needs a trained operator") are shown
- The page includes a link or section for the rental calculator (see US-1.5)
- Average customer rating is visible if reviews exist (links to Epic 2)

**Story Points:** 5
**Priority:** Must
**Sprint:** 1

**Tasks:**

| # | Task | Owner |
|---|------|-------|
| 1 | Create `ToolService` method to return full tool details including images and hire rates | Backend |
| 2 | Build tool detail page layout with all required sections | Frontend |
| 3 | Implement image gallery component with click-through navigation | Frontend |
| 4 | Display hire rates in a clear, tabular format (hourly, daily, weekly) | Frontend |
| 5 | Show special notes and deposit information where applicable | Frontend |
| 6 | Add a placeholder section for the rental calculator (to be completed in US-1.5) | Frontend |
| 7 | Display average customer rating if reviews exist (placeholder until Epic 2 is built) | Frontend |
| 8 | Write unit tests for the tool detail service method | Testing |

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

**Tasks:**

| # | Task | Owner |
|---|------|-------|
| 1 | Implement search logic in `ToolService` with case-insensitive matching across name, description, and category | Backend |
| 2 | Add the search bar component to the site-wide header/navigation | Frontend |
| 3 | Build search results page reusing the `ToolCard` component | Frontend |
| 4 | Display a friendly "no results" message with suggested actions when no matches are found | Frontend |
| 5 | Ensure search bar is usable on mobile viewports | Frontend |
| 6 | Write unit tests for search service logic, including edge cases (empty query, special characters) | Testing |

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
- If the selected period is zero or negative, a sensible error is shown

**Story Points:** 8
**Priority:** Must
**Sprint:** 2

**Tasks:**

| # | Task | Owner |
|---|------|-------|
| 1 | Implement rental cost calculation service that determines the cheapest combination of hourly, daily, and weekly rates | Backend |
| 2 | Add date range validation logic (prevent negative or zero-length periods) | Backend |
| 3 | Create a calculator API endpoint that accepts start/end date-times and returns cost breakdown | Backend |
| 4 | Build date-time picker component for start and end inputs on the tool detail page | Frontend |
| 5 | Display cost breakdown in a clear format (e.g. "2 days × £45/day + 3 hours × £8/hour = £99") | Frontend |
| 6 | Show validation errors for invalid date selections | Frontend |
| 7 | Write unit tests for the calculation logic covering multiple scenarios (hours only, days only, mixed, weekly thresholds) | Testing |

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

**Tasks:**

| # | Task | Owner |
|---|------|-------|
| 1 | Add price range filter parameters (min/max daily rate) to the tool listing service method | Backend |
| 2 | Build price range filter control (min/max inputs or a slider) on the category page | Frontend |
| 3 | Update the tool listing to refresh dynamically without a full page reload when the filter is applied | Frontend |
| 4 | Add a "clear filter" option that resets results to the full listing | Frontend |
| 5 | Write unit tests for the price filter logic in the service layer | Testing |

---

### US-1.7 – Responsive mobile layout for catalogue

**As a** customer browsing on my phone,
**I want** the catalogue and tool pages to work properly on a small screen,
**so that** I can look things up on site without needing a laptop.

**Acceptance Criteria:**
- All catalogue pages (homepage, category, detail, search results) are usable on screens 375px wide and above
- Images resize appropriately
- The search bar is accessible on mobile
- Navigation does not break or overlap on smaller screens

**Story Points:** 5
**Priority:** Should
**Sprint:** 2

**Tasks:**

| # | Task | Owner |
|---|------|-------|
| 1 | Audit all catalogue pages (homepage, category, detail, search results) for 375px+ compatibility | Frontend |
| 2 | Implement responsive image resizing and appropriate aspect ratios | Frontend |
| 3 | Ensure the search bar is fully accessible and usable on mobile viewports | Frontend |
| 4 | Build a mobile-friendly navigation pattern (e.g. hamburger menu) that avoids overlapping elements | Frontend |
| 5 | Ensure touch targets meet minimum size guidelines for accessibility | Frontend |
| 6 | Test layout across common mobile viewport sizes (375px, 390px, 414px) | Testing |

---

### US-1.8 – API endpoints for tools and categories

*[Technical — kept for reference]*

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

*[Technical — kept for reference]*

**As a** developer,
**I want** a properly structured database with tables for categories, tools, and hire rates,
**so that** we have a solid foundation to build on.

**Acceptance Criteria:**
- `Categories` table with Id, Name, Description, ImageUrl
- `Tools` table with Id, Name, Description, CategoryId (FK), HourlyRate, DailyRate, WeeklyRate, SpecialNotes, DepositRequired, DepositAmount, IsActive, OverallRating, ReviewCount, CreatedDate, UpdatedDate
- `ToolImages` table with Id, ToolId (FK), ImageUrl, DisplayOrder, UploadedDate — images are stored in a separate table rather than a JSON column, allowing proper indexing and individual image management
- Seed data includes at least 3 categories with 4–5 tools each
- EF Core migrations are set up and working
- Database can be recreated from migrations on any team member's machine

**Story Points:** 5
**Priority:** Must
**Sprint:** 1

---

### US-1.10 – Project scaffolding and CI pipeline

*[Technical — kept for reference]*

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

## Implementation Status

Updated: 2026-04-10

Backend implementation for Epic 1 is complete for the catalogue and rental calculator API slice.

Implemented API endpoints:
- `GET /api/categories` for homepage/category listing data
- `GET /api/categories/featured` for featured homepage categories
- `GET /api/categories/{id}` for category details
- `GET /api/categories/{id}/tools?page=&pageSize=&sortBy=&sortOrder=&minPrice=&maxPrice=` for category browsing, sorting, pagination, and daily-rate price filtering
- `GET /api/tools/search?q=&page=&pageSize=` for case-insensitive tool search across tool name, description, and category
- `GET /api/tools/{id}` for full tool detail data including category, images, hire rates, special notes, deposit data, and rating summary
- `POST /api/tools/{id}/rental-calculation` for rental cost calculation using the cheapest hourly, daily, and weekly rate combination

Implemented backend components:
- `CategoryService.GetAllCategoriesAsync` and `GetFeaturedCategoriesAsync` support US-1.1 homepage category data.
- `ToolService.GetToolsByCategoryAsync` supports US-1.2 category browsing with active tools, thumbnail mapping, starting price, sort options, empty results, and pagination.
- `ToolService.GetToolByIdAsync` supports US-1.3 detail page data.
- `ToolService.SearchToolsAsync` supports US-1.4 search.
- `ToolService.CalculateRentalCostAsync` supports US-1.5 date validation and rental breakdown calculation.
- `ToolService.FilterByPriceRangeAsync` supports US-1.6 daily-rate min/max filtering.

Implemented data support:
- EF Core migration `20260409234000_SeedEpic1CatalogueData` seeds Epic 1 categories, tools, and tool images.
- `SeedFullTestData.sql` includes linked test data across all current tables and includes at least three categories with four tools each.

Implemented tests and verification:
- Unit tests cover controller success and error paths for Epic 1 endpoints.
- Unit tests cover category and tool service logic for sorting, pagination, search, price filtering, detail lookup, rental calculation, and validation.
- Seed tests verify the full seed script and Epic 1 seed migration include the expected catalogue data.
- Latest verified result: 88 unit tests passing.
- Filtered Epic 1 service coverage for `CategoryService` and `ToolService`: 100% line coverage and 100% branch coverage.
- Full solution build verified with `dotnet build ReviewPortal.slnx --no-restore --disable-build-servers -p:MSBuildEnableWorkloadResolver=false -m:1 -v:minimal`.

Frontend implementation tasks for layout, navigation, responsive pages, and dynamic UI controls remain tracked in the frontend-owned tasks above.

## Summary for Sprint Planning

| Sprint | Stories | Total Points |
|--------|---------|-------------|
| Sprint 1 | US-1.1, US-1.2, US-1.3, US-1.8, US-1.9, US-1.10 | 33 |
| Sprint 2 | US-1.4, US-1.5, US-1.6, US-1.7 | 21 |
