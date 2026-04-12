using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Reviews;
using ReviewPortal.Application.Services;
using ReviewPortal.Domain.Entities;
using ReviewPortal.Domain.Enums;
using ReviewPortal.UnitTests.TestDoubles;

namespace ReviewPortal.UnitTests.Services;

public class ReviewServiceTests
{
    [Fact]
    public async Task CreateReviewAsync_WhenAnonymousRequestIsValid_SavesPendingReview()
    {
        var category = new Category { Id = 1, Name = "Cleaning & Maintenance" };
        var tool = CreateTool(7, category, isActive: true);
        var reviewRepository = new InMemoryRepository<Review>();
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(reviewRepository, unitOfWork, tools: [tool]);

        var request = new CreateReviewRequest(
            "  Sam Customer  ",
            "  sam@example.com  ",
            "  Excellent kit, easy collection, and very clear support from the team.  ",
            5,
            4,
            4,
            5,
            4);

        var result = await service.CreateReviewAsync(tool.Id, request);

        Assert.True(result.IsSuccess);
        Assert.Equal("Pending", result.Value!.Status);
        Assert.Equal(4.4m, result.Value.OverallRating);
        Assert.Equal("Sam Customer", result.Value.ReviewerName);
        Assert.Equal("Excellent kit, easy collection, and very clear support from the team.", result.Value.ReviewText);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);

        var savedReview = Assert.Single(reviewRepository.Items);
        Assert.Equal(ReviewStatus.Pending, savedReview.Status);
        Assert.Equal("sam@example.com", savedReview.ReviewerEmail);
        Assert.Equal(tool.Id, savedReview.ToolId);
    }

    [Fact]
    public async Task CreateReviewAsync_WhenAuthenticatedUserSubmits_UsesStoredUserDetails()
    {
        var category = new Category { Id = 1, Name = "Access & Lifting" };
        var tool = CreateTool(9, category, isActive: true);
        var user = new User
        {
            Id = 42,
            Name = "Jordan Member",
            Email = "jordan@example.com",
            PasswordHash = "hash"
        };

        var reviewRepository = new InMemoryRepository<Review>();
        var service = CreateService(reviewRepository, new FakeUnitOfWork(), tools: [tool], users: [user]);

        var request = new CreateReviewRequest(
            "",
            "",
            "Really smooth rental experience with dependable equipment and fast support.",
            4,
            4,
            5,
            4,
            5);

        var result = await service.CreateReviewAsync(tool.Id, request, user.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal("Jordan Member", result.Value!.ReviewerName);

        var savedReview = Assert.Single(reviewRepository.Items);
        Assert.Equal(user.Id, savedReview.UserId);
        Assert.Equal("Jordan Member", savedReview.ReviewerName);
        Assert.Equal("jordan@example.com", savedReview.ReviewerEmail);
    }

    [Fact]
    public async Task CreateReviewAsync_WhenToolDoesNotExist_ReturnsNotFound()
    {
        var service = CreateService();

        var result = await service.CreateReviewAsync(
            404,
            ValidRequest());

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.FailureType);
        Assert.Equal("Tool with ID 404 not found.", result.Error);
    }

    [Fact]
    public async Task CreateReviewAsync_WhenAnonymousNameIsMissing_ReturnsValidationFailure()
    {
        var category = new Category { Id = 1, Name = "Garden & Landscaping" };
        var tool = CreateTool(3, category, isActive: true);
        var service = CreateService(tools: [tool]);

        var result = await service.CreateReviewAsync(
            tool.Id,
            ValidRequest() with { ReviewerName = "  " });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal("Reviewer name is required when submitting anonymously.", result.Error);
    }

    [Fact]
    public async Task CreateReviewAsync_WhenReviewTextIsTooShort_ReturnsValidationFailure()
    {
        var category = new Category { Id = 1, Name = "Building & Construction" };
        var tool = CreateTool(5, category, isActive: true);
        var service = CreateService(tools: [tool]);

        var result = await service.CreateReviewAsync(
            tool.Id,
            ValidRequest() with { ReviewText = "Too short for the rule"[..19] });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal("Review text must be at least 20 characters.", result.Error);
    }

    [Fact]
    public async Task CreateReviewAsync_WhenAnyRatingFallsOutsideRange_ReturnsValidationFailure()
    {
        var category = new Category { Id = 1, Name = "Electrical & Heating" };
        var tool = CreateTool(6, category, isActive: true);
        var service = CreateService(tools: [tool]);

        var result = await service.CreateReviewAsync(
            tool.Id,
            ValidRequest() with { TechnicalSupportRating = 0 });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal("Technical support rating must be between 1 and 5.", result.Error);
    }

    [Fact]
    public async Task CreateReviewAsync_WhenAuthenticatedUserCannotBeResolved_ReturnsUnauthorizedFailure()
    {
        var category = new Category { Id = 1, Name = "Breaking & Drilling" };
        var tool = CreateTool(10, category, isActive: true);
        var service = CreateService(tools: [tool]);

        var result = await service.CreateReviewAsync(tool.Id, ValidRequest(), userId: 999);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unauthorized, result.FailureType);
        Assert.Equal("Authenticated user account could not be found.", result.Error);
    }

    private static ReviewService CreateService(
        InMemoryRepository<Review>? reviewRepository = null,
        FakeUnitOfWork? unitOfWork = null,
        IEnumerable<Tool>? tools = null,
        IEnumerable<User>? users = null)
    {
        return new ReviewService(
            reviewRepository ?? new InMemoryRepository<Review>(),
            new InMemoryToolRepository(tools),
            new InMemoryUserRepository(users),
            unitOfWork ?? new FakeUnitOfWork());
    }

    private static CreateReviewRequest ValidRequest()
    {
        return new CreateReviewRequest(
            "Alex Reviewer",
            "alex@example.com",
            "Excellent hire experience with helpful staff and dependable equipment.",
            5,
            4,
            4,
            5,
            4);
    }

    private static Tool CreateTool(int id, Category category, bool isActive)
    {
        return new Tool
        {
            Id = id,
            CategoryId = category.Id,
            Category = category,
            Name = $"Tool {id}",
            Description = $"Tool {id} description",
            HourlyRate = 10m,
            DailyRate = 30m,
            WeeklyRate = 100m,
            IsActive = isActive,
            Images = []
        };
    }
}
