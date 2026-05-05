using ReviewPortal.Application.Services;
using ReviewPortal.Domain.Entities;
using ReviewPortal.Domain.Enums;
using ReviewPortal.UnitTests.TestDoubles;

namespace ReviewPortal.UnitTests.Services;

public class DashboardServiceTests
{
    [Fact]
    public async Task GetDashboardStatsAsync_WhenNoDataExists_ReturnsZeroStats()
    {
        var service = CreateService();

        var result = await service.GetDashboardStatsAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.TotalActiveTools);
        Assert.Equal(0, result.Value.TotalInactiveTools);
        Assert.Equal(0, result.Value.PendingModerationCount);
        Assert.Equal(0, result.Value.ReviewsPublishedThisMonth);
        Assert.Empty(result.Value.TopRatedTools);
        Assert.Empty(result.Value.MostReviewedTools);
    }

    [Fact]
    public async Task GetDashboardStatsAsync_WhenMixedDataExists_ReturnsOverviewCounts()
    {
        var monthStart = GetCurrentUtcMonthStart();
        var category = new Category { Id = 1, Name = "Access & Lifting" };
        var activeTool = CreateTool(1, category, "Tower Scaffold", isActive: true, overallRating: 4.8m, reviewCount: 7);
        var secondActiveTool = CreateTool(2, category, "Material Hoist", isActive: true, overallRating: 4.1m, reviewCount: 3);
        var inactiveTool = CreateTool(3, category, "Retired Platform", isActive: false, overallRating: 4.9m, reviewCount: 8);
        var reviews =
            new[]
            {
                CreateReview(1, activeTool, ReviewStatus.Approved, monthStart.AddDays(1)),
                CreateReview(2, activeTool, ReviewStatus.Approved, monthStart.AddDays(-1)),
                CreateReview(3, secondActiveTool, ReviewStatus.Pending, monthStart.AddDays(2)),
                CreateReview(4, secondActiveTool, ReviewStatus.Rejected, monthStart.AddDays(2))
            };
        var comments =
            new[]
            {
                CreateComment(1, reviews[0], ReviewStatus.Pending),
                CreateComment(2, reviews[0], ReviewStatus.Pending),
                CreateComment(3, reviews[0], ReviewStatus.Approved)
            };
        var service = CreateService(
            tools: [activeTool, secondActiveTool, inactiveTool],
            reviews: reviews,
            comments: comments);

        var result = await service.GetDashboardStatsAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.TotalActiveTools);
        Assert.Equal(1, result.Value.TotalInactiveTools);
        Assert.Equal(3, result.Value.PendingModerationCount);
        Assert.Equal(1, result.Value.ReviewsPublishedThisMonth);
        Assert.Equal(["Retired Platform", "Tower Scaffold", "Material Hoist"], result.Value.TopRatedTools.Select(tool => tool.ToolName).ToArray());
        Assert.Equal(["Retired Platform", "Tower Scaffold", "Material Hoist"], result.Value.MostReviewedTools.Select(tool => tool.ToolName).ToArray());
    }

    [Fact]
    public async Task GetDashboardStatsAsync_TopRatedTools_ReturnsTopFiveOrderedByRating()
    {
        var category = new Category { Id = 1, Name = "Cleaning & Maintenance" };
        var tools = new[]
        {
            CreateTool(1, category, "Tool 1", overallRating: 4.1m, reviewCount: 5),
            CreateTool(2, category, "Tool 2", overallRating: 4.9m, reviewCount: 5),
            CreateTool(3, category, "Tool 3", overallRating: 4.2m, reviewCount: 5),
            CreateTool(4, category, "Tool 4", overallRating: 4.8m, reviewCount: 5),
            CreateTool(5, category, "Tool 5", overallRating: 4.4m, reviewCount: 5),
            CreateTool(6, category, "Tool 6", overallRating: 4.6m, reviewCount: 5),
            CreateTool(7, category, "Tool 7", overallRating: 5.0m, reviewCount: 1)
        };
        var service = CreateService(tools: tools);

        var result = await service.GetDashboardStatsAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(["Tool 2", "Tool 4", "Tool 6", "Tool 5", "Tool 3"], result.Value!.TopRatedTools.Select(tool => tool.ToolName).ToArray());
        Assert.All(result.Value.TopRatedTools, tool => Assert.True(tool.ReviewCount >= 2));
    }

    [Fact]
    public async Task GetDashboardStatsAsync_MostReviewedTools_ReturnsTopFiveOrderedByReviewCount()
    {
        var category = new Category { Id = 1, Name = "Breaking & Drilling" };
        var tools = new[]
        {
            CreateTool(1, category, "Tool 1", overallRating: 4.1m, reviewCount: 2),
            CreateTool(2, category, "Tool 2", overallRating: 4.2m, reviewCount: 8),
            CreateTool(3, category, "Tool 3", overallRating: 4.3m, reviewCount: 5),
            CreateTool(4, category, "Tool 4", overallRating: 4.4m, reviewCount: 11),
            CreateTool(5, category, "Tool 5", overallRating: 4.5m, reviewCount: 4),
            CreateTool(6, category, "Tool 6", overallRating: 4.6m, reviewCount: 1),
            CreateTool(7, category, "Tool 7", overallRating: null, reviewCount: 0)
        };
        var service = CreateService(tools: tools);

        var result = await service.GetDashboardStatsAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(["Tool 4", "Tool 2", "Tool 3", "Tool 5", "Tool 1"], result.Value!.MostReviewedTools.Select(tool => tool.ToolName).ToArray());
        Assert.All(result.Value.MostReviewedTools, tool => Assert.True(tool.ReviewCount > 0));
    }

    [Fact]
    public async Task GetDashboardStatsAsync_ReviewsPublishedThisMonth_UsesUtcMonthBoundary()
    {
        var currentMonthStart = GetCurrentUtcMonthStart();
        var nextMonthStart = currentMonthStart.AddMonths(1);
        var category = new Category { Id = 1, Name = "Garden & Landscaping" };
        var tool = CreateTool(1, category, "Rotavator");
        var reviews =
            new[]
            {
                CreateReview(1, tool, ReviewStatus.Approved, currentMonthStart.AddTicks(-1)),
                CreateReview(2, tool, ReviewStatus.Approved, currentMonthStart),
                CreateReview(3, tool, ReviewStatus.Approved, currentMonthStart.AddDays(1)),
                CreateReview(4, tool, ReviewStatus.Approved, nextMonthStart),
                CreateReview(5, tool, ReviewStatus.Pending, currentMonthStart.AddDays(1))
            };
        var service = CreateService(tools: [tool], reviews: reviews);

        var result = await service.GetDashboardStatsAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.ReviewsPublishedThisMonth);
    }

    private static DashboardService CreateService(
        IEnumerable<Tool>? tools = null,
        IEnumerable<Review>? reviews = null,
        IEnumerable<ReviewComment>? comments = null)
    {
        return new DashboardService(
            new InMemoryToolRepository(tools),
            new InMemoryReviewRepository(reviews),
            new InMemoryRepository<ReviewComment>(comments));
    }

    private static Tool CreateTool(
        int id,
        Category category,
        string name,
        bool isActive = true,
        decimal? overallRating = null,
        int reviewCount = 0)
    {
        return new Tool
        {
            Id = id,
            CategoryId = category.Id,
            Category = category,
            Name = name,
            Description = $"{name} description",
            HourlyRate = 10m,
            DailyRate = 30m,
            WeeklyRate = 100m,
            IsActive = isActive,
            OverallRating = overallRating,
            ReviewCount = reviewCount,
            Images = []
        };
    }

    private static Review CreateReview(
        int id,
        Tool tool,
        ReviewStatus status,
        DateTime createdDate)
    {
        var review = new Review
        {
            Id = id,
            ToolId = tool.Id,
            Tool = tool,
            ReviewerName = $"Reviewer {id}",
            ReviewerEmail = $"reviewer{id}@example.com",
            ReviewText = $"Reviewer {id} left detailed and useful feedback for this hire.",
            EquipmentRating = 4,
            CustomerServiceRating = 4,
            TechnicalSupportRating = 4,
            AfterSalesRating = 4,
            ValueForMoneyRating = 4,
            Status = status,
            CreatedDate = createdDate,
            Comments = []
        };

        review.CalculateOverallRating();
        return review;
    }

    private static ReviewComment CreateComment(int id, Review review, ReviewStatus status)
    {
        return new ReviewComment
        {
            Id = id,
            ReviewId = review.Id,
            Review = review,
            CommenterName = $"Commenter {id}",
            CommentText = $"Commenter {id} left a useful follow-up for moderation.",
            Status = status,
            CreatedDate = review.CreatedDate.AddHours(1)
        };
    }

    private static DateTime GetCurrentUtcMonthStart()
    {
        var utcNow = DateTime.UtcNow;
        return new DateTime(utcNow.Year, utcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
    }
}
