using ReviewPortal.Application.Common;
using ReviewPortal.Application.Services;
using ReviewPortal.Domain.Entities;
using ReviewPortal.UnitTests.TestDoubles;

namespace ReviewPortal.UnitTests.Services;

public class ToolServiceTests
{
    [Fact]
    public async Task GetToolsByCategoryAsync_WhenCategoryDoesNotExist_ReturnsNotFound()
    {
        var service = CreateService();

        var result = await service.GetToolsByCategoryAsync(404, 1, 12);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.FailureType);
    }

    [Fact]
    public async Task GetToolsByCategoryAsync_WhenCategoryHasNoTools_ReturnsEmptyPagedResult()
    {
        var category = new Category { Id = 1, Name = "Access" };
        var service = CreateService(categories: [category]);

        var result = await service.GetToolsByCategoryAsync(1, 1, 12);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Items);
        Assert.Equal(0, result.Value.TotalCount);
    }

    [Fact]
    public async Task GetToolsByCategoryAsync_DefaultSort_ReturnsAlphabeticalPageOfActiveTools()
    {
        var category = new Category { Id = 1, Name = "Access" };
        var service = CreateService(
            categories: [category],
            tools:
            [
                CreateTool(1, category, "Tower Scaffold", hourlyRate: 20m, isActive: true),
                CreateTool(2, category, "Access Platform", hourlyRate: 15m, isActive: true),
                CreateTool(3, category, "Boom Lift", hourlyRate: 18m, isActive: false)
            ]);

        var result = await service.GetToolsByCategoryAsync(1, 1, 12);

        Assert.True(result.IsSuccess);
        Assert.Equal(["Access Platform", "Tower Scaffold"], result.Value!.Items.Select(item => item.Name).ToArray());
    }

    [Fact]
    public async Task GetToolsByCategoryAsync_PriceSortDescending_ReturnsHighestStartingPriceFirst()
    {
        var category = new Category { Id = 1, Name = "Breaking & Drilling" };
        var service = CreateService(
            categories: [category],
            tools:
            [
                CreateTool(1, category, "SDS Drill", hourlyRate: 12m, dailyRate: 42m, weeklyRate: 100m),
                CreateTool(2, category, "Hydraulic Breaker", hourlyRate: 30m, dailyRate: 96m, weeklyRate: 240m),
                CreateTool(3, category, "Diamond Core Drill", hourlyRate: 18m, dailyRate: 70m, weeklyRate: 180m)
            ]);

        var result = await service.GetToolsByCategoryAsync(1, 1, 12, "price_desc");

        Assert.True(result.IsSuccess);
        Assert.Equal(
            ["Hydraulic Breaker", "Diamond Core Drill", "SDS Drill"],
            result.Value!.Items.Select(item => item.Name).ToArray());
    }

    [Fact]
    public async Task GetToolsByCategoryAsync_Pagination_ReturnsSecondPageWithTotalCount()
    {
        var category = new Category { Id = 1, Name = "Cleaning & Maintenance" };
        var service = CreateService(
            categories: [category],
            tools: Enumerable.Range(1, 14)
                .Select(index => CreateTool(index, category, $"Tool {index:00}", hourlyRate: index))
                .ToArray());

        var result = await service.GetToolsByCategoryAsync(1, 2, 12);

        Assert.True(result.IsSuccess);
        Assert.Equal(14, result.Value!.TotalCount);
        Assert.Equal(2, result.Value.TotalPages);
        Assert.Equal(["Tool 13", "Tool 14"], result.Value.Items.Select(item => item.Name).ToArray());
    }

    [Fact]
    public async Task GetToolsByCategoryAsync_MapsThumbnailAndStartingPrice()
    {
        var category = new Category { Id = 1, Name = "Garden & Landscaping" };
        var tool = CreateTool(1, category, "Rotavator", hourlyRate: 14m, dailyRate: 55m, weeklyRate: 180m);
        tool.Images =
        [
            new ToolImage { Id = 2, ToolId = 1, ImageUrl = "later.jpg", DisplayOrder = 2 },
            new ToolImage { Id = 1, ToolId = 1, ImageUrl = "first.jpg", DisplayOrder = 1 }
        ];

        var service = CreateService(categories: [category], tools: [tool]);

        var result = await service.GetToolsByCategoryAsync(1, 1, 12);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value!.Items);
        Assert.Equal(14m, item.StartingPrice);
        Assert.Equal("hour", item.StartingPriceUnit);
        Assert.Equal("first.jpg", item.ThumbnailUrl);
    }

    [Fact]
    public async Task GetToolsByCategoryAsync_InvalidSortValue_ReturnsValidationFailure()
    {
        var category = new Category { Id = 1, Name = "Painting & Decorating" };
        var service = CreateService(categories: [category]);

        var result = await service.GetToolsByCategoryAsync(1, 1, 12, "popularity");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
    }

    [Fact]
    public async Task GetToolByIdAsync_WhenToolExists_ReturnsFullDetailWithOrderedImages()
    {
        var category = new Category { Id = 1, Name = "Electrical & Heating" };
        var tool = CreateTool(7, category, "50L Dehumidifier", hourlyRate: 11m, dailyRate: 40m, weeklyRate: 135m);
        tool.SpecialNotes = "Keep clear of walls for best airflow.";
        tool.DepositRequired = true;
        tool.DepositAmount = 60m;
        tool.OverallRating = 4.6m;
        tool.ReviewCount = 1;
        tool.Images =
        [
            new ToolImage { Id = 2, ToolId = 7, ImageUrl = "second.jpg", DisplayOrder = 2 },
            new ToolImage { Id = 1, ToolId = 7, ImageUrl = "first.jpg", DisplayOrder = 1 }
        ];

        var service = CreateService(categories: [category], tools: [tool]);

        var result = await service.GetToolByIdAsync(7);

        Assert.True(result.IsSuccess);
        Assert.Equal("Electrical & Heating", result.Value!.CategoryName);
        Assert.Equal(11m, result.Value.HourlyRate);
        Assert.Equal(40m, result.Value.DailyRate);
        Assert.Equal(135m, result.Value.WeeklyRate);
        Assert.Equal("Keep clear of walls for best airflow.", result.Value.SpecialNotes);
        Assert.True(result.Value.DepositRequired);
        Assert.Equal(60m, result.Value.DepositAmount);
        Assert.Equal(4.6m, result.Value.OverallRating);
        Assert.Equal(["first.jpg", "second.jpg"], result.Value.Images.Select(image => image.ImageUrl).ToArray());
    }

    [Fact]
    public async Task GetToolByIdAsync_WhenToolIsInactive_ReturnsNotFound()
    {
        var category = new Category { Id = 1, Name = "Access & Lifting" };
        var tool = CreateTool(8, category, "Material Hoist", hourlyRate: 20m, isActive: false);
        var service = CreateService(categories: [category], tools: [tool]);

        var result = await service.GetToolByIdAsync(8);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.FailureType);
    }

    [Fact]
    public async Task GetToolByIdAsync_WhenToolDoesNotExist_ReturnsNotFound()
    {
        var service = CreateService();

        var result = await service.GetToolByIdAsync(999);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.FailureType);
    }

    [Fact]
    public async Task SearchToolsAsync_WhenQueryMatchesName_ReturnsMatchingToolSummary()
    {
        var category = new Category { Id = 1, Name = "Breaking & Drilling" };
        var matchingTool = CreateTool(1, category, "SDS Max Drill", hourlyRate: 15m);
        matchingTool.Images =
        [
            new ToolImage { Id = 1, ToolId = 1, ImageUrl = "sds-drill.jpg", DisplayOrder = 1 }
        ];

        var service = CreateService(
            categories: [category],
            tools:
            [
                matchingTool,
                CreateTool(2, category, "Hydraulic Breaker", hourlyRate: 22m)
            ]);

        var result = await service.SearchToolsAsync("sds", 1, 12);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value!.Items);
        Assert.Equal("SDS Max Drill", item.Name);
        Assert.Equal("Breaking & Drilling", item.CategoryName);
        Assert.Equal(15m, item.StartingPrice);
        Assert.Equal("sds-drill.jpg", item.ThumbnailUrl);
    }

    [Fact]
    public async Task SearchToolsAsync_WhenQueryMatchesDescriptionOrCategory_ReturnsCaseInsensitiveMatches()
    {
        var cleaning = new Category { Id = 1, Name = "Cleaning & Maintenance" };
        var heating = new Category { Id = 2, Name = "Electrical & Heating" };
        var pressureWasher = CreateTool(1, cleaning, "Petrol Pressure Washer", hourlyRate: 14m);
        pressureWasher.Description = "High-output machine for paving and site vehicles.";
        var dehumidifier = CreateTool(2, heating, "50L Dehumidifier", hourlyRate: 11m);
        var rotavator = CreateTool(3, new Category { Id = 3, Name = "Garden & Landscaping" }, "Rotavator", hourlyRate: 16m);

        var service = CreateService(
            categories: [cleaning, heating, rotavator.Category],
            tools: [pressureWasher, dehumidifier, rotavator]);

        var descriptionResult = await service.SearchToolsAsync("PAVING", 1, 12);
        var categoryResult = await service.SearchToolsAsync("heating", 1, 12);

        Assert.True(descriptionResult.IsSuccess);
        Assert.Equal(["Petrol Pressure Washer"], descriptionResult.Value!.Items.Select(item => item.Name).ToArray());
        Assert.True(categoryResult.IsSuccess);
        Assert.Equal(["50L Dehumidifier"], categoryResult.Value!.Items.Select(item => item.Name).ToArray());
    }

    [Fact]
    public async Task SearchToolsAsync_WhenToolIsInactive_ExcludesTool()
    {
        var category = new Category { Id = 1, Name = "Access & Lifting" };
        var service = CreateService(
            categories: [category],
            tools:
            [
                CreateTool(1, category, "Platform Ladder", hourlyRate: 10m, isActive: true),
                CreateTool(2, category, "Retired Platform Lift", hourlyRate: 18m, isActive: false)
            ]);

        var result = await service.SearchToolsAsync("platform", 1, 12);

        Assert.True(result.IsSuccess);
        Assert.Equal(["Platform Ladder"], result.Value!.Items.Select(item => item.Name).ToArray());
    }

    [Fact]
    public async Task SearchToolsAsync_WhenNoToolsMatch_ReturnsEmptyPagedResult()
    {
        var category = new Category { Id = 1, Name = "Access & Lifting" };
        var service = CreateService(
            categories: [category],
            tools: [CreateTool(1, category, "Platform Ladder", hourlyRate: 10m)]);

        var result = await service.SearchToolsAsync("excavator", 1, 12);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Items);
        Assert.Equal(0, result.Value.TotalCount);
    }

    [Fact]
    public async Task SearchToolsAsync_WhenQueryIsBlank_ReturnsValidationFailure()
    {
        var service = CreateService();

        var result = await service.SearchToolsAsync("   ", 1, 12);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal("Search query is required.", result.Error);
    }

    [Fact]
    public async Task CalculateRentalCostAsync_WhenHoursOnlyIsCheapest_ReturnsHourlyCost()
    {
        var category = new Category { Id = 1, Name = "Breaking & Drilling" };
        var tool = CreateTool(1, category, "SDS Max Drill", hourlyRate: 8m, dailyRate: 45m, weeklyRate: 250m);
        var service = CreateService(categories: [category], tools: [tool]);
        var start = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);
        var end = start.AddHours(3);

        var result = await service.CalculateRentalCostAsync(1, new(start, end));

        Assert.True(result.IsSuccess);
        Assert.Equal("SDS Max Drill", result.Value!.ToolName);
        Assert.Equal(24m, result.Value.TotalCost);
        Assert.Equal("3 hours x 8.00/hour = 24.00", result.Value.Breakdown);
    }

    [Fact]
    public async Task CalculateRentalCostAsync_WhenDailyRateIsCheaper_ReturnsDailyCost()
    {
        var category = new Category { Id = 1, Name = "Cleaning & Maintenance" };
        var tool = CreateTool(1, category, "Petrol Pressure Washer", hourlyRate: 10m, dailyRate: 50m, weeklyRate: 300m);
        var service = CreateService(categories: [category], tools: [tool]);
        var start = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);
        var end = start.AddHours(8);

        var result = await service.CalculateRentalCostAsync(1, new(start, end));

        Assert.True(result.IsSuccess);
        Assert.Equal(50m, result.Value!.TotalCost);
        Assert.Equal("1 day x 50.00/day = 50.00", result.Value.Breakdown);
    }

    [Fact]
    public async Task CalculateRentalCostAsync_WhenPeriodIsMixed_ReturnsDailyAndHourlyCost()
    {
        var category = new Category { Id = 1, Name = "Electrical & Heating" };
        var tool = CreateTool(1, category, "50L Dehumidifier", hourlyRate: 8m, dailyRate: 45m, weeklyRate: 250m);
        var service = CreateService(categories: [category], tools: [tool]);
        var start = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);
        var end = start.AddDays(1).AddHours(3);

        var result = await service.CalculateRentalCostAsync(1, new(start, end));

        Assert.True(result.IsSuccess);
        Assert.Equal(69m, result.Value!.TotalCost);
        Assert.Equal("1 day x 45.00/day + 3 hours x 8.00/hour = 69.00", result.Value.Breakdown);
    }

    [Fact]
    public async Task CalculateRentalCostAsync_WhenWeeklyRateIsCheaperForThreeDays_ReturnsWeeklyCost()
    {
        var category = new Category { Id = 1, Name = "Access & Lifting" };
        var tool = CreateTool(1, category, "Tower Scaffold", hourlyRate: 10m, dailyRate: 45m, weeklyRate: 100m);
        var service = CreateService(categories: [category], tools: [tool]);
        var start = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);
        var end = start.AddDays(3);

        var result = await service.CalculateRentalCostAsync(1, new(start, end));

        Assert.True(result.IsSuccess);
        Assert.Equal(100m, result.Value!.TotalCost);
        Assert.Equal("1 week x 100.00/week = 100.00", result.Value.Breakdown);
    }

    [Fact]
    public async Task CalculateRentalCostAsync_WhenEndDateTimeIsNotAfterStartDateTime_ReturnsValidationFailure()
    {
        var service = CreateService();
        var start = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);

        var result = await service.CalculateRentalCostAsync(1, new(start, start));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal("End date/time must be after the start date/time.", result.Error);
    }

    private static ToolService CreateService(
        IEnumerable<Category>? categories = null,
        IEnumerable<Tool>? tools = null)
    {
        return new ToolService(
            new InMemoryToolRepository(tools),
            new InMemoryCategoryRepository(categories));
    }

    private static Tool CreateTool(
        int id,
        Category category,
        string name,
        decimal hourlyRate,
        decimal? dailyRate = null,
        decimal? weeklyRate = null,
        bool isActive = true)
    {
        return new Tool
        {
            Id = id,
            CategoryId = category.Id,
            Category = category,
            Name = name,
            Description = $"{name} description",
            HourlyRate = hourlyRate,
            DailyRate = dailyRate ?? (hourlyRate * 3),
            WeeklyRate = weeklyRate ?? (hourlyRate * 10),
            IsActive = isActive,
            Images = []
        };
    }
}
