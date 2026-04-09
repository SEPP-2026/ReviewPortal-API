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
