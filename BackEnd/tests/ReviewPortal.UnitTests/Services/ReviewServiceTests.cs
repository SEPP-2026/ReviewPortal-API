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
        var reviewRepository = new InMemoryReviewRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(reviewRepository: reviewRepository, unitOfWork: unitOfWork, tools: [tool]);

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

        var reviewRepository = new InMemoryReviewRepository();
        var service = CreateService(reviewRepository: reviewRepository, unitOfWork: new FakeUnitOfWork(), tools: [tool], users: [user]);

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

    [Fact]
    public async Task GetApprovedReviewsAsync_WhenToolDoesNotExist_ReturnsNotFound()
    {
        var service = CreateService();

        var result = await service.GetApprovedReviewsAsync(404, page: 1, pageSize: 10);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.FailureType);
        Assert.Equal("Tool with ID 404 not found.", result.Error);
    }

    [Fact]
    public async Task GetApprovedReviewsAsync_WhenNoApprovedReviews_ReturnsEmptyMessageAndNoAverage()
    {
        var category = new Category { Id = 1, Name = "Cleaning & Maintenance" };
        var tool = CreateTool(7, category, isActive: true);
        var service = CreateService(tools: [tool]);

        var result = await service.GetApprovedReviewsAsync(tool.Id, page: 1, pageSize: 10);

        Assert.True(result.IsSuccess);
        Assert.Equal(tool.Id, result.Value!.ToolId);
        Assert.Null(result.Value.AverageOverallRating);
        Assert.Equal(0, result.Value.TotalApprovedReviews);
        Assert.Equal("No reviews yet - be the first to share your experience", result.Value.EmptyStateMessage);
        Assert.Empty(result.Value.Reviews.Items);
    }

    [Fact]
    public async Task GetApprovedReviewsAsync_ReturnsApprovedReviewsSortedByMostRecentFirst()
    {
        var category = new Category { Id = 1, Name = "Access & Lifting" };
        var tool = CreateTool(9, category, isActive: true);
        var firstApproved = CreateReview(
            1,
            tool,
            "Ava",
            ReviewStatus.Approved,
            new DateTime(2026, 4, 1, 8, 0, 0, DateTimeKind.Utc),
            5,
            4,
            4,
            5,
            5);
        firstApproved.Comments =
        [
            new ReviewComment
            {
                Id = 2,
                ReviewId = 1,
                CommenterName = "Pending commenter",
                CommentText = "This should not appear.",
                Status = ReviewStatus.Pending,
                CreatedDate = new DateTime(2026, 4, 2, 8, 0, 0, DateTimeKind.Utc)
            },
            new ReviewComment
            {
                Id = 1,
                ReviewId = 1,
                CommenterName = "Helpful commenter",
                CommentText = "We had the same experience.",
                Status = ReviewStatus.Approved,
                CreatedDate = new DateTime(2026, 4, 1, 9, 0, 0, DateTimeKind.Utc)
            }
        ];
        firstApproved.CompanyResponse = new CompanyResponse
        {
            Id = 11,
            ReviewId = 1,
            StaffUserId = 50,
            ResponseText = "Thanks for the feedback.",
            CreatedDate = new DateTime(2026, 4, 3, 8, 0, 0, DateTimeKind.Utc),
            UpdatedDate = new DateTime(2026, 4, 3, 9, 0, 0, DateTimeKind.Utc),
            StaffUser = new User
            {
                Id = 50,
                Name = "Shelton Team",
                Email = "team@example.com",
                PasswordHash = "hash"
            }
        };

        var secondApproved = CreateReview(
            2,
            tool,
            "Ben",
            ReviewStatus.Approved,
            new DateTime(2026, 4, 10, 8, 0, 0, DateTimeKind.Utc),
            4,
            4,
            3,
            4,
            2);
        var pendingReview = CreateReview(
            3,
            tool,
            "Pending Person",
            ReviewStatus.Pending,
            new DateTime(2026, 4, 12, 8, 0, 0, DateTimeKind.Utc),
            5,
            5,
            5,
            5,
            5);
        var otherTool = CreateTool(10, category, isActive: true);
        var otherToolApproved = CreateReview(
            4,
            otherTool,
            "Other Tool Reviewer",
            ReviewStatus.Approved,
            new DateTime(2026, 4, 11, 8, 0, 0, DateTimeKind.Utc),
            5,
            5,
            5,
            5,
            5);

        var service = CreateService(
            reviews: [firstApproved, secondApproved, pendingReview, otherToolApproved],
            tools: [tool, otherTool]);

        var result = await service.GetApprovedReviewsAsync(tool.Id, page: 1, pageSize: 10);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.TotalApprovedReviews);
        Assert.Equal(4.0m, result.Value.AverageOverallRating);
        Assert.Null(result.Value.EmptyStateMessage);
        Assert.Equal(["Ben", "Ava"], result.Value.Reviews.Items.Select(review => review.ReviewerName).ToArray());

        var olderReview = result.Value.Reviews.Items.Last();
        var comment = Assert.Single(olderReview.Comments);
        Assert.Equal("Helpful commenter", comment.CommenterName);
        Assert.NotNull(olderReview.CompanyResponse);
        Assert.Equal("Shelton Team", olderReview.CompanyResponse!.StaffName);
    }

    [Fact]
    public async Task GetApprovedReviewsAsync_PaginatesApprovedReviews()
    {
        var category = new Category { Id = 1, Name = "Garden & Landscaping" };
        var tool = CreateTool(4, category, isActive: true);
        var reviews = Enumerable.Range(1, 12)
            .Select(index => CreateReview(
                index,
                tool,
                $"Reviewer {index}",
                ReviewStatus.Approved,
                new DateTime(2026, 4, 1, 8, 0, 0, DateTimeKind.Utc).AddDays(index),
                4,
                4,
                4,
                4,
                4))
            .ToArray();
        var service = CreateService(reviews: reviews, tools: [tool]);

        var result = await service.GetApprovedReviewsAsync(tool.Id, page: 2, pageSize: 10);

        Assert.True(result.IsSuccess);
        Assert.Equal(12, result.Value!.Reviews.TotalCount);
        Assert.Equal(2, result.Value.Reviews.TotalPages);
        Assert.Equal(["Reviewer 2", "Reviewer 1"], result.Value.Reviews.Items.Select(review => review.ReviewerName).ToArray());
    }

    [Fact]
    public async Task GetApprovedReviewsAsync_WhenPageIsInvalid_ReturnsValidationFailure()
    {
        var category = new Category { Id = 1, Name = "Electrical & Heating" };
        var tool = CreateTool(6, category, isActive: true);
        var service = CreateService(tools: [tool]);

        var result = await service.GetApprovedReviewsAsync(tool.Id, page: 0, pageSize: 10);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal("Page must be greater than or equal to 1.", result.Error);
    }

    [Fact]
    public async Task GetApprovedReviewsAsync_WhenPageSizeExceedsMaximum_ReturnsValidationFailure()
    {
        var category = new Category { Id = 1, Name = "Electrical & Heating" };
        var tool = CreateTool(6, category, isActive: true);
        var service = CreateService(tools: [tool]);

        var result = await service.GetApprovedReviewsAsync(tool.Id, page: 1, pageSize: 101);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal("Page size must not exceed 100.", result.Error);
    }

    [Fact]
    public async Task GetApprovedReviewsAsync_RoundsAverageAcrossAllApprovedReviews()
    {
        var category = new Category { Id = 1, Name = "Cleaning & Maintenance" };
        var tool = CreateTool(12, category, isActive: true);
        var reviews =
            new[]
            {
                CreateReview(
                    1,
                    tool,
                    "Reviewer One",
                    ReviewStatus.Approved,
                    new DateTime(2026, 4, 1, 8, 0, 0, DateTimeKind.Utc),
                    5,
                    4,
                    4,
                    4,
                    4),
                CreateReview(
                    2,
                    tool,
                    "Reviewer Two",
                    ReviewStatus.Approved,
                    new DateTime(2026, 4, 2, 8, 0, 0, DateTimeKind.Utc),
                    4,
                    4,
                    4,
                    4,
                    4),
                CreateReview(
                    3,
                    tool,
                    "Reviewer Three",
                    ReviewStatus.Approved,
                    new DateTime(2026, 4, 3, 8, 0, 0, DateTimeKind.Utc),
                    4,
                    4,
                    4,
                    4,
                    4)
            };
        var service = CreateService(reviews: reviews, tools: [tool]);

        var result = await service.GetApprovedReviewsAsync(tool.Id, page: 1, pageSize: 2);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.TotalApprovedReviews);
        Assert.Equal(4.07m, result.Value.AverageOverallRating);
        Assert.Equal(2, result.Value.Reviews.Items.Count);
    }

    [Fact]
    public async Task AddCommentAsync_WhenRequestIsValid_SavesPendingComment()
    {
        var category = new Category { Id = 1, Name = "Access & Lifting" };
        var tool = CreateTool(9, category, isActive: true);
        var review = CreateReview(
            1,
            tool,
            "Ava",
            ReviewStatus.Approved,
            new DateTime(2026, 4, 1, 8, 0, 0, DateTimeKind.Utc),
            5,
            4,
            4,
            5,
            5);
        var commentRepository = new InMemoryRepository<ReviewComment>();
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(
            reviewRepository: new InMemoryReviewRepository([review]),
            commentRepository: commentRepository,
            unitOfWork: unitOfWork,
            tools: [tool]);

        var request = new CreateCommentRequest(
            "  Sam Commenter  ",
            "  We had a very similar experience and the team was brilliant.  ");

        var result = await service.AddCommentAsync(review.Id, request);

        Assert.True(result.IsSuccess);
        Assert.Equal("Sam Commenter", result.Value!.CommenterName);
        Assert.Equal("We had a very similar experience and the team was brilliant.", result.Value.CommentText);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);

        var savedComment = Assert.Single(commentRepository.Items);
        Assert.Equal(review.Id, savedComment.ReviewId);
        Assert.Equal(ReviewStatus.Pending, savedComment.Status);
        Assert.Equal("Sam Commenter", savedComment.CommenterName);
    }

    [Fact]
    public async Task AddCommentAsync_WhenReviewDoesNotExist_ReturnsNotFound()
    {
        var service = CreateService();

        var result = await service.AddCommentAsync(404, ValidCommentRequest());

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.FailureType);
        Assert.Equal("Review with ID 404 not found.", result.Error);
    }

    [Fact]
    public async Task AddCommentAsync_WhenReviewIsNotApproved_ReturnsNotFound()
    {
        var category = new Category { Id = 1, Name = "Access & Lifting" };
        var tool = CreateTool(9, category, isActive: true);
        var review = CreateReview(
            1,
            tool,
            "Ava",
            ReviewStatus.Pending,
            new DateTime(2026, 4, 1, 8, 0, 0, DateTimeKind.Utc),
            5,
            4,
            4,
            5,
            5);
        var service = CreateService(reviews: [review], tools: [tool]);

        var result = await service.AddCommentAsync(review.Id, ValidCommentRequest());

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.FailureType);
        Assert.Equal("Review with ID 1 not found.", result.Error);
    }

    [Fact]
    public async Task AddCommentAsync_WhenCommentTextIsTooShort_ReturnsValidationFailure()
    {
        var category = new Category { Id = 1, Name = "Access & Lifting" };
        var tool = CreateTool(9, category, isActive: true);
        var review = CreateReview(
            1,
            tool,
            "Ava",
            ReviewStatus.Approved,
            new DateTime(2026, 4, 1, 8, 0, 0, DateTimeKind.Utc),
            5,
            4,
            4,
            5,
            5);
        var service = CreateService(reviews: [review], tools: [tool]);

        var result = await service.AddCommentAsync(review.Id, ValidCommentRequest() with { CommentText = "too short" });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal("Comment text must be at least 10 characters.", result.Error);
    }

    [Fact]
    public async Task GetApprovedCommentsAsync_ReturnsApprovedCommentsOnlyInChronologicalOrder()
    {
        var category = new Category { Id = 1, Name = "Access & Lifting" };
        var tool = CreateTool(9, category, isActive: true);
        var review = CreateReview(
            1,
            tool,
            "Ava",
            ReviewStatus.Approved,
            new DateTime(2026, 4, 1, 8, 0, 0, DateTimeKind.Utc),
            5,
            4,
            4,
            5,
            5);
        review.Comments =
        [
            new ReviewComment
            {
                Id = 2,
                ReviewId = review.Id,
                CommenterName = "Pending commenter",
                CommentText = "This should stay hidden.",
                Status = ReviewStatus.Pending,
                CreatedDate = new DateTime(2026, 4, 2, 8, 0, 0, DateTimeKind.Utc)
            },
            new ReviewComment
            {
                Id = 3,
                ReviewId = review.Id,
                CommenterName = "Later commenter",
                CommentText = "We had the same result.",
                Status = ReviewStatus.Approved,
                CreatedDate = new DateTime(2026, 4, 3, 8, 0, 0, DateTimeKind.Utc)
            },
            new ReviewComment
            {
                Id = 1,
                ReviewId = review.Id,
                CommenterName = "Earlier commenter",
                CommentText = "Helpful review, thanks for posting.",
                Status = ReviewStatus.Approved,
                CreatedDate = new DateTime(2026, 4, 1, 9, 0, 0, DateTimeKind.Utc)
            }
        ];

        var service = CreateService(reviews: [review], tools: [tool]);

        var result = await service.GetApprovedCommentsAsync(review.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(["Earlier commenter", "Later commenter"], result.Value!.Select(comment => comment.CommenterName).ToArray());
    }

    [Fact]
    public async Task GetApprovedCommentsAsync_WhenReviewIsNotApproved_ReturnsNotFound()
    {
        var category = new Category { Id = 1, Name = "Access & Lifting" };
        var tool = CreateTool(9, category, isActive: true);
        var review = CreateReview(
            1,
            tool,
            "Ava",
            ReviewStatus.Pending,
            new DateTime(2026, 4, 1, 8, 0, 0, DateTimeKind.Utc),
            5,
            4,
            4,
            5,
            5);
        var service = CreateService(reviews: [review], tools: [tool]);

        var result = await service.GetApprovedCommentsAsync(review.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.FailureType);
        Assert.Equal("Review with ID 1 not found.", result.Error);
    }

    private static ReviewService CreateService(
        InMemoryReviewRepository? reviewRepository = null,
        InMemoryRepository<ReviewComment>? commentRepository = null,
        FakeUnitOfWork? unitOfWork = null,
        IEnumerable<Review>? reviews = null,
        IEnumerable<Tool>? tools = null,
        IEnumerable<User>? users = null)
    {
        return new ReviewService(
            reviewRepository ?? new InMemoryReviewRepository(reviews),
            commentRepository ?? new InMemoryRepository<ReviewComment>(),
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

    private static CreateCommentRequest ValidCommentRequest()
    {
        return new CreateCommentRequest(
            "Alex Commenter",
            "Helpful review and we found the same issue with the hire.");
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

    private static Review CreateReview(
        int id,
        Tool tool,
        string reviewerName,
        ReviewStatus status,
        DateTime createdDate,
        int equipmentRating,
        int customerServiceRating,
        int technicalSupportRating,
        int afterSalesRating,
        int valueForMoneyRating)
    {
        var review = new Review
        {
            Id = id,
            ToolId = tool.Id,
            Tool = tool,
            ReviewerName = reviewerName,
            ReviewerEmail = $"{reviewerName.Replace(" ", string.Empty).ToLowerInvariant()}@example.com",
            ReviewText = $"{reviewerName} left detailed and useful feedback for this hire.",
            EquipmentRating = equipmentRating,
            CustomerServiceRating = customerServiceRating,
            TechnicalSupportRating = technicalSupportRating,
            AfterSalesRating = afterSalesRating,
            ValueForMoneyRating = valueForMoneyRating,
            Status = status,
            CreatedDate = createdDate,
            Comments = []
        };

        review.CalculateOverallRating();
        return review;
    }
}
