using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Categories;
using ReviewPortal.Application.DTOs.Tools;
using ReviewPortal.Domain.Entities;
using ReviewPortal.Infrastructure.Data;

namespace ReviewPortal.IntegrationTests.Api;

[Collection(ReviewPortalApiCollection.CollectionName)]
public class CatalogueApiIntegrationTests : IAsyncLifetime
{
    private readonly ReviewPortalApiFactory _factory;

    public CatalogueApiIntegrationTests(ReviewPortalApiFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync()
    {
        return _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task PublicCatalogueEndpoints_ReturnSeededCategoriesToolsSearchDetailAndRentalCalculation()
    {
        using var client = _factory.CreateHttpsClient();

        var categories = await client.GetFromJsonAsync<CategoryDto[]>("/api/categories");
        Assert.NotNull(categories);
        Assert.Contains(categories!, category => category.Id == _factory.AccessCategoryId && category.Name == "Access & Lifting");

        var categoryTools = await client.GetFromJsonAsync<PagedList<ToolSummaryDto>>(
            $"/api/categories/{_factory.AccessCategoryId}/tools?page=1&pageSize=10&sortBy=price&sortOrder=asc");
        Assert.NotNull(categoryTools);
        Assert.Equal(1, categoryTools!.TotalCount);
        Assert.Equal("Tower Scaffold", Assert.Single(categoryTools.Items).Name);

        var searchResults = await client.GetFromJsonAsync<PagedList<ToolSummaryDto>>(
            "/api/tools/search?q=tower&page=1&pageSize=10");
        Assert.NotNull(searchResults);
        Assert.Contains(searchResults!.Items, tool => tool.Id == _factory.TowerScaffoldToolId && tool.Name == "Tower Scaffold");

        var detail = await client.GetFromJsonAsync<ToolDto>($"/api/tools/{_factory.TowerScaffoldToolId}");
        Assert.NotNull(detail);
        Assert.Equal("Tower Scaffold", detail!.Name);
        Assert.True(detail.HasEnoughReviewsToRate);
        Assert.Equal(2, detail.ReviewCount);
        Assert.NotNull(detail.OverallRating);
        Assert.Equal("/uploads/tools/tower-scaffold.jpg", Assert.Single(detail.Images).ImageUrl);

        var start = new DateTime(2026, 5, 5, 9, 0, 0, DateTimeKind.Utc);
        var rentalResponse = await client.PostAsJsonAsync(
            $"/api/tools/{_factory.TowerScaffoldToolId}/rental-calculation",
            new RentalCalculationRequest(start, start.AddDays(1)));

        Assert.Equal(HttpStatusCode.OK, rentalResponse.StatusCode);
        var rental = await rentalResponse.Content.ReadFromJsonAsync<RentalCalculationResponse>();
        Assert.NotNull(rental);
        Assert.Equal("Tower Scaffold", rental!.ToolName);
        Assert.Equal(45m, rental.TotalCost);
        Assert.Contains("1 day x 45.00/day", rental.Breakdown);
    }

    [Fact]
    public async Task FeaturedCategories_ReturnHomepageContractWithSeededCategoryMetadata()
    {
        using var client = _factory.CreateHttpsClient();

        var featured = await client.GetFromJsonAsync<CategoryDto[]>("/api/categories/featured");

        Assert.NotNull(featured);
        Assert.NotEmpty(featured!);
        Assert.Equal(featured.OrderBy(category => category.Name).Select(category => category.Name), featured.Select(category => category.Name));
        Assert.Contains(featured, category =>
            category.Id == _factory.AccessCategoryId &&
            category.Name == "Access & Lifting" &&
            category.Description == "Access towers, platforms, and lifting equipment." &&
            category.ImageUrl == "/images/categories/access.jpg" &&
            category.ToolCount >= 1);
        Assert.Contains(featured, category =>
            category.Id == _factory.BreakingCategoryId &&
            category.Name == "Breaking & Drilling" &&
            category.ImageUrl == "/images/categories/breaking.jpg");
    }

    [Fact]
    public async Task CategoryBrowsing_ReturnsPaginationSortingAndExcludesInactiveTools()
    {
        using var client = _factory.CreateHttpsClient();
        await SeedAdditionalAccessToolsAsync();

        var pageTwo = await client.GetFromJsonAsync<PagedList<ToolSummaryDto>>(
            $"/api/categories/{_factory.AccessCategoryId}/tools?page=2&pageSize=2&sortBy=name");

        Assert.NotNull(pageTwo);
        Assert.Equal(2, pageTwo!.Page);
        Assert.Equal(2, pageTwo.PageSize);
        Assert.Equal(3, pageTwo.TotalCount);
        Assert.Equal(2, pageTwo.TotalPages);
        Assert.True(pageTwo.HasPreviousPage);
        Assert.False(pageTwo.HasNextPage);
        Assert.Equal(["Tower Scaffold"], pageTwo.Items.Select(tool => tool.Name).ToArray());

        var nameSorted = await client.GetFromJsonAsync<PagedList<ToolSummaryDto>>(
            $"/api/categories/{_factory.AccessCategoryId}/tools?page=1&pageSize=10&sortBy=name");
        Assert.NotNull(nameSorted);
        Assert.Equal(["Boom Lift", "Budget Ladder", "Tower Scaffold"], nameSorted!.Items.Select(tool => tool.Name).ToArray());

        var priceAscending = await client.GetFromJsonAsync<PagedList<ToolSummaryDto>>(
            $"/api/categories/{_factory.AccessCategoryId}/tools?page=1&pageSize=10&sortBy=price&sortOrder=asc");
        Assert.NotNull(priceAscending);
        Assert.Equal(["Budget Ladder", "Tower Scaffold", "Boom Lift"], priceAscending!.Items.Select(tool => tool.Name).ToArray());
        Assert.Equal([4m, 10m, 25m], priceAscending.Items.Select(tool => tool.StartingPrice).ToArray());

        var priceDescending = await client.GetFromJsonAsync<PagedList<ToolSummaryDto>>(
            $"/api/categories/{_factory.AccessCategoryId}/tools?page=1&pageSize=10&sortBy=price&sortOrder=desc");
        Assert.NotNull(priceDescending);
        Assert.Equal(["Boom Lift", "Tower Scaffold", "Budget Ladder"], priceDescending!.Items.Select(tool => tool.Name).ToArray());

        Assert.DoesNotContain(nameSorted.Items, tool => tool.Id == _factory.RetiredLadderToolId || tool.Name == "Retired Ladder");
    }

    [Fact]
    public async Task PriceFilter_ReturnsDailyRateMatchesValidationErrorsAndEmptyResults()
    {
        using var client = _factory.CreateHttpsClient();
        await SeedAdditionalAccessToolsAsync();

        var filtered = await client.GetFromJsonAsync<PagedList<ToolSummaryDto>>(
            $"/api/categories/{_factory.AccessCategoryId}/tools?minPrice=20&maxPrice=45&page=1&pageSize=10&sortBy=price&sortOrder=asc");

        Assert.NotNull(filtered);
        Assert.Equal(2, filtered!.TotalCount);
        Assert.Equal(["Budget Ladder", "Tower Scaffold"], filtered.Items.Select(tool => tool.Name).ToArray());
        Assert.All(filtered.Items, tool => Assert.InRange(tool.DailyRate, 20m, 45m));

        var negativeRange = await client.GetAsync($"/api/categories/{_factory.AccessCategoryId}/tools?minPrice=-1&maxPrice=50");
        Assert.Equal(HttpStatusCode.BadRequest, negativeRange.StatusCode);

        var reversedRange = await client.GetAsync($"/api/categories/{_factory.AccessCategoryId}/tools?minPrice=100&maxPrice=50");
        Assert.Equal(HttpStatusCode.BadRequest, reversedRange.StatusCode);

        var noMatches = await client.GetFromJsonAsync<PagedList<ToolSummaryDto>>(
            $"/api/categories/{_factory.AccessCategoryId}/tools?minPrice=999&maxPrice=1000&page=1&pageSize=10");

        Assert.NotNull(noMatches);
        Assert.Equal(0, noMatches!.TotalCount);
        Assert.Empty(noMatches.Items);
        Assert.Equal(0, noMatches.TotalPages);
        Assert.False(noMatches.HasNextPage);
    }

    [Fact]
    public async Task Search_ReturnsCaseInsensitiveNameDescriptionCategoryEmptyAndSpecialCharacterContracts()
    {
        using var client = _factory.CreateHttpsClient();
        await SeedOperatorServiceToolAsync();

        var nameMatch = await client.GetFromJsonAsync<PagedList<ToolSummaryDto>>(
            "/api/tools/search?q=ToWeR&page=1&pageSize=10");
        Assert.NotNull(nameMatch);
        Assert.Contains(nameMatch!.Items, tool => tool.Id == _factory.TowerScaffoldToolId && tool.Name == "Tower Scaffold");

        var descriptionMatch = await client.GetFromJsonAsync<PagedList<ToolSummaryDto>>(
            "/api/tools/search?q=masonry&page=1&pageSize=10");
        Assert.NotNull(descriptionMatch);
        Assert.Contains(descriptionMatch!.Items, tool => tool.Id == _factory.RotaryHammerToolId && tool.Name == "Rotary Hammer");

        var categoryMatch = await client.GetFromJsonAsync<PagedList<ToolSummaryDto>>(
            $"/api/tools/search?q={Uri.EscapeDataString("operator services")}&page=1&pageSize=10");
        Assert.NotNull(categoryMatch);
        Assert.Single(categoryMatch!.Items);
        Assert.Equal("Weekend Delivery", categoryMatch.Items[0].Name);
        Assert.Equal("Operator Services", categoryMatch.Items[0].CategoryName);

        var noResult = await client.GetFromJsonAsync<PagedList<ToolSummaryDto>>(
            "/api/tools/search?q=no-such-hire-item&page=1&pageSize=10");
        Assert.NotNull(noResult);
        Assert.Equal(0, noResult!.TotalCount);
        Assert.Empty(noResult.Items);

        var emptyQuery = await client.GetAsync("/api/tools/search?q=");
        Assert.Equal(HttpStatusCode.BadRequest, emptyQuery.StatusCode);

        var specialCharacters = await client.GetFromJsonAsync<PagedList<ToolSummaryDto>>(
            $"/api/tools/search?q={Uri.EscapeDataString("!@#$%^&*()")}&page=1&pageSize=10");
        Assert.NotNull(specialCharacters);
        Assert.Equal(0, specialCharacters!.TotalCount);
        Assert.Empty(specialCharacters.Items);
    }

    [Fact]
    public async Task ToolDetail_ReturnsFullPublicContractAndNotFoundForInactiveOrMissingTools()
    {
        using var client = _factory.CreateHttpsClient();
        await AddSecondTowerImageAsync();

        var detail = await client.GetFromJsonAsync<ToolDto>($"/api/tools/{_factory.TowerScaffoldToolId}");

        Assert.NotNull(detail);
        Assert.Equal(_factory.TowerScaffoldToolId, detail!.Id);
        Assert.Equal(_factory.AccessCategoryId, detail.CategoryId);
        Assert.Equal("Access & Lifting", detail.CategoryName);
        Assert.Equal("Tower Scaffold", detail.Name);
        Assert.Equal("Mobile scaffold tower for safe access at height.", detail.Description);
        Assert.Equal(10m, detail.HourlyRate);
        Assert.Equal(45m, detail.DailyRate);
        Assert.Equal(180m, detail.WeeklyRate);
        Assert.Equal("Guard rails included.", detail.SpecialNotes);
        Assert.True(detail.DepositRequired);
        Assert.Equal(75m, detail.DepositAmount);
        Assert.True(detail.IsActive);
        Assert.True(detail.HasEnoughReviewsToRate);
        Assert.Null(detail.RatingMessage);
        Assert.Equal(2, detail.ReviewCount);
        Assert.Equal(4.4m, detail.OverallRating);
        Assert.Equal(
            ["/uploads/tools/tower-scaffold.jpg", "/uploads/tools/tower-scaffold-side.jpg"],
            detail.Images.Select(image => image.ImageUrl).ToArray());

        var inactive = await client.GetAsync($"/api/tools/{_factory.RetiredLadderToolId}");
        Assert.Equal(HttpStatusCode.NotFound, inactive.StatusCode);

        var missing = await client.GetAsync("/api/tools/999999");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [Fact]
    public async Task RentalCalculator_ReturnsCheapestBreakdownsAndValidationContracts()
    {
        using var client = _factory.CreateHttpsClient();
        var start = new DateTime(2026, 5, 5, 9, 0, 0, DateTimeKind.Utc);

        await AssertRentalCalculationAsync(client, start, start.AddHours(4), 40m, "4 hours x 10.00/hour");
        await AssertRentalCalculationAsync(client, start, start.AddDays(2), 90m, "2 days x 45.00/day");
        await AssertRentalCalculationAsync(client, start, start.AddDays(6), 180m, "1 week x 180.00/week");
        await AssertRentalCalculationAsync(client, start, start.AddDays(7).AddHours(2), 200m, "1 week x 180.00/week + 2 hours x 10.00/hour");

        var invalidDateRange = await client.PostAsJsonAsync(
            $"/api/tools/{_factory.TowerScaffoldToolId}/rental-calculation",
            new RentalCalculationRequest(start, start));
        Assert.Equal(HttpStatusCode.BadRequest, invalidDateRange.StatusCode);

        var missingTool = await client.PostAsJsonAsync(
            "/api/tools/999999/rental-calculation",
            new RentalCalculationRequest(start, start.AddHours(4)));
        Assert.Equal(HttpStatusCode.NotFound, missingTool.StatusCode);

        var inactiveTool = await client.PostAsJsonAsync(
            $"/api/tools/{_factory.RetiredLadderToolId}/rental-calculation",
            new RentalCalculationRequest(start, start.AddHours(4)));
        Assert.Equal(HttpStatusCode.NotFound, inactiveTool.StatusCode);
    }

    [Fact]
    public async Task ToolsRootEndpoint_DoesNotExistForPublicCatalogueContract()
    {
        using var client = _factory.CreateHttpsClient();

        var toolsRoot = await client.GetAsync("/api/tools");
        var categoryTools = await client.GetAsync($"/api/categories/{_factory.AccessCategoryId}/tools?page=1&pageSize=10");

        Assert.Contains(toolsRoot.StatusCode, new[] { HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed });
        Assert.Equal(HttpStatusCode.OK, categoryTools.StatusCode);
    }

    private async Task AssertRentalCalculationAsync(
        HttpClient client,
        DateTime start,
        DateTime end,
        decimal expectedTotalCost,
        string expectedBreakdownPart)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/tools/{_factory.TowerScaffoldToolId}/rental-calculation",
            new RentalCalculationRequest(start, end));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var calculation = await response.Content.ReadFromJsonAsync<RentalCalculationResponse>();
        Assert.NotNull(calculation);
        Assert.Equal("Tower Scaffold", calculation!.ToolName);
        Assert.Equal(start, calculation.StartDateTime);
        Assert.Equal(end, calculation.EndDateTime);
        Assert.Equal(expectedTotalCost, calculation.TotalCost);
        Assert.Contains(expectedBreakdownPart, calculation.Breakdown);
    }

    private async Task SeedAdditionalAccessToolsAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var alreadySeeded = await context.Tools.AnyAsync(tool => tool.Name == "Budget Ladder");
        if (alreadySeeded)
        {
            return;
        }

        var timestamp = new DateTime(2026, 4, 4, 9, 0, 0, DateTimeKind.Utc);
        var budgetLadder = CreateTool(
            _factory.AccessCategoryId,
            "Budget Ladder",
            "Low-cost ladder for short indoor access jobs.",
            hourlyRate: 4m,
            dailyRate: 20m,
            weeklyRate: 75m,
            imageUrl: "/uploads/tools/budget-ladder.jpg",
            timestamp);
        var boomLift = CreateTool(
            _factory.AccessCategoryId,
            "Boom Lift",
            "Powered boom lift for high reach access work.",
            hourlyRate: 25m,
            dailyRate: 120m,
            weeklyRate: 480m,
            imageUrl: "/uploads/tools/boom-lift.jpg",
            timestamp);

        context.Tools.AddRange(budgetLadder, boomLift);
        await context.SaveChangesAsync();
    }

    private async Task SeedOperatorServiceToolAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var alreadySeeded = await context.Categories.AnyAsync(category => category.Name == "Operator Services");
        if (alreadySeeded)
        {
            return;
        }

        var timestamp = new DateTime(2026, 4, 4, 9, 30, 0, DateTimeKind.Utc);
        var serviceCategory = new Category
        {
            Name = "Operator Services",
            Description = "Staff-assisted hire support services.",
            ImageUrl = "/images/categories/operator-services.jpg"
        };
        var serviceTool = CreateTool(
            0,
            "Weekend Delivery",
            "Delivery for hired equipment during weekend bookings.",
            hourlyRate: 18m,
            dailyRate: 70m,
            weeklyRate: 250m,
            imageUrl: "/uploads/tools/weekend-delivery.jpg",
            timestamp);
        serviceTool.Category = serviceCategory;

        context.Categories.Add(serviceCategory);
        context.Tools.Add(serviceTool);
        await context.SaveChangesAsync();
    }

    private async Task AddSecondTowerImageAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var alreadyAdded = await context.ToolImages.AnyAsync(image =>
            image.ToolId == _factory.TowerScaffoldToolId &&
            image.ImageUrl == "/uploads/tools/tower-scaffold-side.jpg");
        if (alreadyAdded)
        {
            return;
        }

        context.ToolImages.Add(new ToolImage
        {
            ToolId = _factory.TowerScaffoldToolId,
            ImageUrl = "/uploads/tools/tower-scaffold-side.jpg",
            DisplayOrder = 2,
            UploadedDate = new DateTime(2026, 4, 4, 10, 0, 0, DateTimeKind.Utc)
        });
        await context.SaveChangesAsync();
    }

    private static Tool CreateTool(
        int categoryId,
        string name,
        string description,
        decimal hourlyRate,
        decimal dailyRate,
        decimal weeklyRate,
        string imageUrl,
        DateTime timestamp)
    {
        var tool = new Tool
        {
            CategoryId = categoryId,
            Name = name,
            Description = description,
            HourlyRate = hourlyRate,
            DailyRate = dailyRate,
            WeeklyRate = weeklyRate,
            DepositRequired = false,
            IsActive = true,
            CreatedDate = timestamp,
            UpdatedDate = timestamp
        };

        tool.Images.Add(new ToolImage
        {
            Tool = tool,
            ImageUrl = imageUrl,
            DisplayOrder = 1,
            UploadedDate = timestamp
        });

        return tool;
    }
}
