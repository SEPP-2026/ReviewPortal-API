# Testing Strategy — Shelton Tool-Hire Review Portal

## 1. Overview

This document outlines our approach to testing the Shelton Tool-Hire Review Portal. We follow a **test pyramid** strategy, writing the majority of tests at the unit level, a smaller number of integration tests, and a focused set of end-to-end tests covering critical user journeys.

The goal is not 100% code coverage for the sake of it, but **meaningful test coverage** that gives the team confidence to ship without regressions.

The detailed MSc submission test plan, including black-box tests, dry-run checks, and usability testing schema, is maintained in [TEST-PLAN.md](TEST-PLAN.md).

---

## 2. Test Pyramid

```
         /  E2E  \          ← 3 critical user journeys (Playwright)
        /----------\
       / Integration \      ← API endpoints, DB queries (WebApplicationFactory)
      /----------------\
     /    Unit Tests     \  ← Services, validators, domain logic (xUnit)
    /____________________\
```

| Level | What We Test | Tools | Approximate Count |
|-------|-------------|-------|-------------------|
| **Unit** | Application services, domain logic, validators, calculator algorithm | xUnit, Moq, FluentAssertions, Bogus | 60–80 tests |
| **Integration** | API controllers with real DB, EF Core queries, auth pipeline | xUnit, WebApplicationFactory, TestContainers or in-memory DB | 25–35 tests |
| **End-to-End** | Full user journeys through the browser | Playwright (.NET) | 3–5 scenarios |

---

## 3. Unit Tests

**Project:** `tests/ReviewPortal.UnitTests/`
**References:** Domain, Application

### What unit tests cover
- **Application Services:** Each public method in `ToolService`, `ReviewService`, `CategoryService`, `AuthService` is tested with mocked repository dependencies
- **Domain Logic:** `Review.CalculateOverallRating()` method — verifies correct averaging of five star ratings
- **Validators:** FluentValidation validators for all request DTOs (CreateReviewRequest, CreateToolRequest, etc.)
- **Rental Calculator:** The cost calculation algorithm — this is the most heavily tested component because it has complex business logic
- **Result Pattern:** Verify success and failure paths through the `Result<T>` type

### Rental Calculator Test Cases
The brief gives a specific example (*"9:00 am 8th Sept – 3pm 10th September"*), so this must be tested thoroughly:

| Test Case | Input | Expected Behaviour |
|-----------|-------|-------------------|
| Exact hours only | 9:00 AM – 2:00 PM same day (5 hours) | 5 × hourly rate |
| Full day | 9:00 AM – 9:00 AM next day (24 hours) | 1 × daily rate (should be cheaper than 24 × hourly) |
| Multi-day | 9:00 AM Mon – 9:00 AM Thu (3 days) | 3 × daily rate |
| Week boundary | 7 days exactly | 1 × weekly rate (should be cheaper than 7 × daily) |
| Mixed period | 9:00 AM 8th Sept – 3:00 PM 10th Sept | Cheapest combination: 2 × daily + 6 × hourly |
| Sub-hour | 9:00 AM – 9:30 AM (30 mins) | 1 × hourly (minimum billable unit) |
| End before start | End date before start date | Validation error |
| Zero duration | Same start and end | Validation error |
| Long period | 3 weeks + 2 days + 4 hours | 3 × weekly + 2 × daily + 4 × hourly |

### Naming Convention
```
MethodName_StateUnderTest_ExpectedBehaviour
```
Examples:
- `CalculateRentalCost_TwoDaysAndSixHours_ReturnsCheapestCombination`
- `CreateReview_AllRatingsMissing_ReturnsValidationError`
- `GetToolById_ToolDoesNotExist_ReturnsFailureResult`

### Test Data
We use **Bogus** (faker library) to generate realistic test data:
```csharp
var faker = new Faker<Tool>()
    .RuleFor(t => t.Name, f => f.Commerce.ProductName())
    .RuleFor(t => t.HourlyRate, f => f.Random.Decimal(5, 25))
    .RuleFor(t => t.DailyRate, f => f.Random.Decimal(30, 100))
    .RuleFor(t => t.WeeklyRate, f => f.Random.Decimal(150, 400));
```

---

## 4. Integration Tests

**Project:** `tests/ReviewPortal.IntegrationTests/`
**References:** All projects

### What integration tests cover
- **API Endpoints:** Send HTTP requests to the real API pipeline and verify responses, status codes, and JSON structure
- **Database Queries:** Verify that EF Core configurations produce correct SQL and that complex queries (search, filtering, pagination) work correctly
- **Authentication & Authorisation:** Verify that admin endpoints return 401 for unauthenticated requests and 403 for unauthorised roles
- **Moderation Workflow:** Submit a review → verify it's Pending → approve it → verify it appears in approved list

### Test Infrastructure
We use `WebApplicationFactory<Program>` to spin up the API in-process with a test SQL Server database:

```csharp
public class ReviewPortalApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace production DB with test database
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            services.Remove(descriptor!);
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer("Server=...;Database=ReviewPortal_Test;..."));
        });
    }
}
```

### Key Integration Test Scenarios

| Scenario | Endpoints | Assertions |
|----------|-----------|------------|
| Browse catalogue | `GET /api/categories`, `GET /api/categories/{id}/tools` | Returns categories with tool counts, tools with correct data |
| Search tools | `GET /api/tools/search?q=cement` | Returns matching tools, case-insensitive |
| Submit review | `POST /api/tools/{id}/reviews` | Returns 201, review saved with Pending status |
| Moderation flow | `POST review` → `GET pending` → `PUT approve` → `GET approved` | Full lifecycle verified |
| Admin auth | `POST /api/admin/tools` without token | Returns 401 |
| Role enforcement | `POST /api/admin/tools` with Customer role | Returns 403 |

---

## 5. End-to-End Tests

**Tool:** Playwright (.NET)
**Sprint:** 4 (US-3.10)

### Critical User Journeys

#### Journey 1: Browse → Detail → Calculate
1. Navigate to homepage
2. Click a category
3. Click a tool
4. Enter start date/time and end date/time in the calculator
5. Verify the cost breakdown is displayed correctly

#### Journey 2: Submit Review → Moderate → Publish
1. Navigate to a tool detail page
2. Click "Write a Review"
3. Fill in name, email, review text, and all five star ratings
4. Submit and verify confirmation message
5. Log in as moderator
6. Navigate to moderation queue
7. Approve the review
8. Navigate back to the tool page and verify the review is now visible

#### Journey 3: Admin → Add Tool → Verify
1. Log in as admin
2. Navigate to "Add Equipment"
3. Fill in all required fields and upload an image
4. Save the tool
5. Navigate to the public catalogue
6. Verify the new tool appears in the correct category

---

## 6. Acceptance Criteria Traceability

Every user story has acceptance criteria. Each acceptance criterion maps to at least one test:

| Test Level | What It Verifies |
|-----------|-----------------|
| Unit test | Business logic, validation rules, calculation correctness |
| Integration test | API behaviour, database persistence, auth enforcement |
| E2E test | Full user flow through the UI works end-to-end |

A story is only considered **Done** when:
- ✅ All acceptance criteria have corresponding passing tests
- ✅ Unit tests pass
- ✅ Integration tests pass
- ✅ Manual smoke test confirms no obvious issues
- ✅ Code reviewed via pull request

---

## 7. Code Coverage Targets

We do not chase arbitrary coverage numbers. Our targets are based on what matters:

| Area | Target | Rationale |
|------|--------|-----------|
| Application Services | 80%+ | Core business logic — high risk of regressions |
| Domain Entities | 90%+ | Simple but critical — rating calculations must be correct |
| Validators | 100% | Every validation rule must be tested |
| Controllers | 60%+ | Thin controllers — mostly tested via integration tests |
| Infrastructure | 40%+ | Repositories are generic — tested indirectly via integration tests |

---

## 8. CI/CD Integration

All tests run automatically in the GitHub Actions pipeline:

```yaml
- name: Run Unit Tests
  run: dotnet test tests/ReviewPortal.UnitTests --no-build

- name: Run Integration Tests
  run: dotnet test tests/ReviewPortal.IntegrationTests --no-build

- name: Run E2E Tests
  run: npx playwright test
```

Tests must pass on every pull request before merging to `develop`.
