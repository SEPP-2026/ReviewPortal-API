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
        Assert.Equal(7, result.Value.Reviews.Items.First().HelpfulCount);
        Assert.Equal(10, olderReview.HelpfulCount);
        var comment = Assert.Single(olderReview.Comments);
        Assert.Equal("Helpful commenter", comment.CommenterName);
        Assert.NotNull(olderReview.CompanyResponse);
        Assert.Equal("Shelton Team", olderReview.CompanyResponse!.StaffName);
    }

    [Fact]
    public async Task GetApprovedReviewsAsync_WhenSortedByHelpful_ReturnsMostHelpfulFirst()
    {
        var category = new Category { Id = 1, Name = "Cleaning & Maintenance" };
        var tool = CreateTool(7, category, isActive: true);
        var newerHighRating = CreateReview(
            1,
            tool,
            "Newest Rating",
            ReviewStatus.Approved,
            new DateTime(2026, 4, 12, 8, 0, 0, DateTimeKind.Utc),
            5,
            5,
            5,
            5,
            5);
        newerHighRating.Comments =
        [
            new ReviewComment
            {
                Id = 10,
                ReviewId = newerHighRating.Id,
                CommenterName = "Pending Reply",
                CommentText = "Pending replies should not count as helpful engagement.",
                Status = ReviewStatus.Pending,
                CreatedDate = new DateTime(2026, 4, 13, 8, 0, 0, DateTimeKind.Utc)
            }
        ];

        var olderWithApprovedReplies = CreateReview(
            2,
            tool,
            "Most Helpful",
            ReviewStatus.Approved,
            new DateTime(2026, 4, 10, 8, 0, 0, DateTimeKind.Utc),
            4,
            4,
            4,
            4,
            4);
        olderWithApprovedReplies.Comments =
        [
            CreateApprovedComment(21, olderWithApprovedReplies.Id),
            CreateApprovedComment(22, olderWithApprovedReplies.Id),
            CreateApprovedComment(23, olderWithApprovedReplies.Id)
        ];

        var leastHelpful = CreateReview(
            3,
            tool,
            "Least Helpful",
            ReviewStatus.Approved,
            new DateTime(2026, 4, 11, 8, 0, 0, DateTimeKind.Utc),
            3,
            3,
            3,
            3,
            3);

        var service = CreateService(reviews: [newerHighRating, olderWithApprovedReplies, leastHelpful], tools: [tool]);

        var result = await service.GetApprovedReviewsAsync(tool.Id, page: 1, pageSize: 2, sortBy: "helpful");

        Assert.True(result.IsSuccess);
        Assert.Equal(["Most Helpful", "Newest Rating"], result.Value!.Reviews.Items.Select(review => review.ReviewerName).ToArray());
        Assert.Equal([11, 10], result.Value.Reviews.Items.Select(review => review.HelpfulCount).ToArray());
        Assert.Equal(3, result.Value.Reviews.TotalCount);
        Assert.True(result.Value.Reviews.HasNextPage);
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
    public async Task GetAllApprovedReviewsAsync_ReturnsApprovedReviewsAcrossAllTools()
    {
        var category = new Category { Id = 1, Name = "Garden & Landscaping" };
        var toolA = CreateTool(1, category, isActive: true);
        var toolB = CreateTool(2, category, isActive: true);
        var baseDate = new DateTime(2026, 4, 1, 8, 0, 0, DateTimeKind.Utc);
        var reviews = new[]
        {
            CreateReview(1, toolA, "Ava", ReviewStatus.Approved, baseDate.AddDays(1), 5, 5, 5, 5, 5),
            CreateReview(2, toolB, "Ben", ReviewStatus.Approved, baseDate.AddDays(2), 4, 4, 4, 4, 4),
            CreateReview(3, toolA, "Cara", ReviewStatus.Pending, baseDate.AddDays(3), 3, 3, 3, 3, 3),
            CreateReview(4, toolB, "Dan", ReviewStatus.Approved, baseDate.AddDays(4), 5, 4, 5, 4, 5),
        };
        var service = CreateService(reviews: reviews, tools: [toolA, toolB]);

        var result = await service.GetAllApprovedReviewsAsync(page: 1, pageSize: 10);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.TotalCount);
        Assert.Equal(3, result.Value.Items.Count);
        // Newest approved first; the pending review is excluded.
        Assert.Equal(["Dan", "Ben", "Ava"], result.Value.Items.Select(review => review.ReviewerName).ToArray());
    }

    [Fact]
    public async Task GetAllApprovedReviewsAsync_PaginatesAcrossTools()
    {
        var category = new Category { Id = 1, Name = "Access & Lifting" };
        var tool = CreateTool(9, category, isActive: true);
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

        var result = await service.GetAllApprovedReviewsAsync(page: 2, pageSize: 10);

        Assert.True(result.IsSuccess);
        Assert.Equal(12, result.Value!.TotalCount);
        Assert.Equal(2, result.Value.TotalPages);
        Assert.Equal(2, result.Value.Items.Count);
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
    public async Task GetApprovedReviewsAsync_WhenSortIsInvalid_ReturnsValidationFailure()
    {
        var category = new Category { Id = 1, Name = "Electrical & Heating" };
        var tool = CreateTool(6, category, isActive: true);
        var service = CreateService(tools: [tool]);

        var result = await service.GetApprovedReviewsAsync(tool.Id, page: 1, pageSize: 10, sortBy: "popularity");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal("Invalid sortBy value. Supported values are recent, helpful, rating, and rating_asc.", result.Error);
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
    public async Task GetUserReviewsAsync_WhenAuthenticatedUserCannotBeResolved_ReturnsUnauthorizedFailure()
    {
        var service = CreateService();

        var result = await service.GetUserReviewsAsync(userId: 999, page: 1, pageSize: 10);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unauthorized, result.FailureType);
        Assert.Equal("Authenticated user account could not be found.", result.Error);
    }

    [Fact]
    public async Task GetUserReviewsAsync_WhenPageIsInvalid_ReturnsValidationFailure()
    {
        var user = CreateUser(42, UserRole.Customer, "Jordan Member");
        var service = CreateService(users: [user]);

        var result = await service.GetUserReviewsAsync(user.Id, page: 0, pageSize: 10);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal("Page must be greater than or equal to 1.", result.Error);
    }

    [Fact]
    public async Task GetUserReviewsAsync_WhenUserHasNoReviews_ReturnsEmptyPagedList()
    {
        var user = CreateUser(42, UserRole.Customer, "Jordan Member");
        var service = CreateService(users: [user]);

        var result = await service.GetUserReviewsAsync(user.Id, page: 1, pageSize: 10);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Items);
        Assert.Equal(0, result.Value.TotalCount);
        Assert.Equal(1, result.Value.Page);
        Assert.Equal(10, result.Value.PageSize);
    }

    [Fact]
    public async Task GetUserReviewsAsync_ReturnsCurrentUsersReviewsSortedByMostRecentFirst()
    {
        var category = new Category { Id = 1, Name = "Compaction" };
        var firstTool = CreateTool(7, category, isActive: true);
        var secondTool = CreateTool(8, category, isActive: true);
        var currentUser = CreateUser(42, UserRole.Customer, "Jordan Member");
        var otherUser = CreateUser(43, UserRole.Customer, "Avery Member");

        var rejectedReview = CreateReview(
            1,
            firstTool,
            "Jordan Member",
            ReviewStatus.Rejected,
            new DateTime(2026, 4, 4, 9, 0, 0, DateTimeKind.Utc),
            2,
            3,
            2,
            3,
            2);
        rejectedReview.UserId = currentUser.Id;
        rejectedReview.RejectionReason = "Please remove personal contact details from the review.";
        rejectedReview.ReviewText = new string('A', 150);

        var approvedReview = CreateReview(
            2,
            secondTool,
            "Jordan Member",
            ReviewStatus.Approved,
            new DateTime(2026, 4, 11, 12, 0, 0, DateTimeKind.Utc),
            5,
            4,
            4,
            5,
            4);
        approvedReview.UserId = currentUser.Id;
        approvedReview.CompanyResponse = new CompanyResponse
        {
            Id = 12,
            ReviewId = approvedReview.Id,
            StaffUserId = 80,
            ResponseText = "Thanks for the great feedback.",
            CreatedDate = new DateTime(2026, 4, 12, 8, 0, 0, DateTimeKind.Utc),
            UpdatedDate = new DateTime(2026, 4, 12, 8, 0, 0, DateTimeKind.Utc)
        };

        var otherUsersReview = CreateReview(
            3,
            firstTool,
            "Avery Member",
            ReviewStatus.Pending,
            new DateTime(2026, 4, 12, 8, 0, 0, DateTimeKind.Utc),
            4,
            4,
            4,
            4,
            4);
        otherUsersReview.UserId = otherUser.Id;

        var service = CreateService(
            reviews: [rejectedReview, approvedReview, otherUsersReview],
            tools: [firstTool, secondTool],
            users: [currentUser, otherUser]);

        var result = await service.GetUserReviewsAsync(currentUser.Id, page: 1, pageSize: 10);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.TotalCount);
        Assert.Equal([approvedReview.Id, rejectedReview.Id], result.Value.Items.Select(review => review.Id).ToArray());

        var latestReview = result.Value.Items.First();
        Assert.Equal(secondTool.Id, latestReview.ToolId);
        Assert.Equal(secondTool.Name, latestReview.ToolName);
        Assert.Equal("Approved", latestReview.Status);
        Assert.True(latestReview.HasCompanyResponse);

        var rejectedSummary = result.Value.Items.Last();
        Assert.Equal("Rejected", rejectedSummary.Status);
        Assert.Equal("Please remove personal contact details from the review.", rejectedSummary.RejectionReason);
        Assert.False(rejectedSummary.HasCompanyResponse);
        Assert.EndsWith("...", rejectedSummary.ReviewTextSnippet);
        Assert.True(rejectedSummary.ReviewTextSnippet.Length <= 140);
    }

    [Fact]
    public async Task GetUserReviewsAsync_PaginatesResults()
    {
        var category = new Category { Id = 1, Name = "Cleaning & Maintenance" };
        var tool = CreateTool(9, category, isActive: true);
        var user = CreateUser(42, UserRole.Customer, "Jordan Member");
        var reviews = Enumerable.Range(1, 12)
            .Select(index =>
            {
                var review = CreateReview(
                    index,
                    tool,
                    "Jordan Member",
                    ReviewStatus.Approved,
                    new DateTime(2026, 4, 1, 8, 0, 0, DateTimeKind.Utc).AddDays(index),
                    4,
                    4,
                    4,
                    4,
                    4);
                review.UserId = user.Id;
                return review;
            })
            .ToArray();
        var service = CreateService(reviews: reviews, tools: [tool], users: [user]);

        var result = await service.GetUserReviewsAsync(user.Id, page: 2, pageSize: 10);

        Assert.True(result.IsSuccess);
        Assert.Equal(12, result.Value!.TotalCount);
        Assert.Equal(2, result.Value.TotalPages);
        Assert.Equal([2, 1], result.Value.Items.Select(review => review.Id).ToArray());
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
        Assert.Equal("Pending", result.Value.Status);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);

        var savedComment = Assert.Single(commentRepository.Items);
        Assert.Equal(review.Id, savedComment.ReviewId);
        Assert.Equal(ReviewStatus.Pending, savedComment.Status);
        Assert.Equal("Sam Commenter", savedComment.CommenterName);
    }

    [Fact]
    public async Task AddCommentAsync_WhenAuthenticatedUserSubmits_UsesStoredUserDetailsAndLinksAccount()
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
        var user = new User
        {
            Id = 42,
            Name = "Jordan Member",
            Email = "jordan@example.com",
            PasswordHash = "hash"
        };
        var commentRepository = new InMemoryRepository<ReviewComment>();
        var service = CreateService(
            reviewRepository: new InMemoryReviewRepository([review]),
            commentRepository: commentRepository,
            unitOfWork: new FakeUnitOfWork(),
            tools: [tool],
            users: [user]);

        var request = new CreateCommentRequest(
            string.Empty,
            "Really useful review and our experience matched it very closely.");

        var result = await service.AddCommentAsync(review.Id, request, user.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal("Jordan Member", result.Value!.CommenterName);
        Assert.Equal("Pending", result.Value.Status);

        var savedComment = Assert.Single(commentRepository.Items);
        Assert.Equal(user.Id, savedComment.UserId);
        Assert.Equal("Jordan Member", savedComment.CommenterName);
    }

    [Fact]
    public async Task AddCommentAsync_WhenStaffMemberSubmits_AutoApprovesComment()
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
        var staff = new User
        {
            Id = 7,
            Name = "Morgan Moderator",
            Email = "morgan@example.com",
            PasswordHash = "hash",
            Role = UserRole.Moderator
        };
        var commentRepository = new InMemoryRepository<ReviewComment>();
        var service = CreateService(
            reviewRepository: new InMemoryReviewRepository([review]),
            commentRepository: commentRepository,
            unitOfWork: new FakeUnitOfWork(),
            tools: [tool],
            users: [staff]);

        var request = new CreateCommentRequest(
            string.Empty,
            "Thanks for the detailed feedback - glad the kit worked well for you.");

        var result = await service.AddCommentAsync(review.Id, request, staff.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal("Approved", result.Value!.Status);

        var savedComment = Assert.Single(commentRepository.Items);
        Assert.Equal(ReviewStatus.Approved, savedComment.Status);
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
    public async Task AddCommentAsync_WhenAnonymousNameIsMissing_ReturnsValidationFailure()
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

        var result = await service.AddCommentAsync(review.Id, ValidCommentRequest() with { CommenterName = "  " });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal("Commenter name is required.", result.Error);
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
    public async Task AddCommentAsync_WhenAuthenticatedUserCannotBeResolved_ReturnsUnauthorizedFailure()
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

        var result = await service.AddCommentAsync(review.Id, ValidCommentRequest() with { CommenterName = string.Empty }, userId: 999);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unauthorized, result.FailureType);
        Assert.Equal("Authenticated user account could not be found.", result.Error);
    }

    [Fact]
    public async Task AddCompanyResponseAsync_WhenRequestIsValid_SavesImmediateResponse()
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
        var staffUser = CreateStaffUser(42, UserRole.Admin, "Shelton Team");
        var companyResponseRepository = new InMemoryRepository<CompanyResponse>();
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(
            reviewRepository: new InMemoryReviewRepository([review]),
            companyResponseRepository: companyResponseRepository,
            unitOfWork: unitOfWork,
            tools: [tool],
            users: [staffUser]);

        var result = await service.AddCompanyResponseAsync(
            review.Id,
            new CreateCompanyResponseRequest("  Thanks for the thoughtful feedback.  "),
            staffUser.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal("Thanks for the thoughtful feedback.", result.Value!.ResponseText);
        Assert.Equal("Shelton Team", result.Value.StaffName);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);

        var savedResponse = Assert.Single(companyResponseRepository.Items);
        Assert.Equal(review.Id, savedResponse.ReviewId);
        Assert.Equal(staffUser.Id, savedResponse.StaffUserId);
        Assert.Equal("Thanks for the thoughtful feedback.", savedResponse.ResponseText);
        Assert.Same(savedResponse, review.CompanyResponse);
    }

    [Fact]
    public async Task AddCompanyResponseAsync_WhenReviewIsPending_ReturnsValidationFailure()
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
        var staffUser = CreateStaffUser(42, UserRole.Admin, "Shelton Team");
        var companyResponseRepository = new InMemoryRepository<CompanyResponse>();
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(
            reviewRepository: new InMemoryReviewRepository([review]),
            companyResponseRepository: companyResponseRepository,
            unitOfWork: unitOfWork,
            tools: [tool],
            users: [staffUser]);

        var result = await service.AddCompanyResponseAsync(review.Id, ValidCompanyResponseRequest(), staffUser.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal("Company responses can only be added to approved reviews.", result.Error);
        Assert.Empty(companyResponseRepository.Items);
        Assert.Null(review.CompanyResponse);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task AddCompanyResponseAsync_WhenReviewIsRejected_ReturnsValidationFailure()
    {
        var category = new Category { Id = 1, Name = "Access & Lifting" };
        var tool = CreateTool(9, category, isActive: true);
        var review = CreateReview(
            1,
            tool,
            "Ava",
            ReviewStatus.Rejected,
            new DateTime(2026, 4, 1, 8, 0, 0, DateTimeKind.Utc),
            5,
            4,
            4,
            5,
            5);
        review.RejectionReason = "Contains personal details.";
        var staffUser = CreateStaffUser(42, UserRole.Admin, "Shelton Team");
        var companyResponseRepository = new InMemoryRepository<CompanyResponse>();
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(
            reviewRepository: new InMemoryReviewRepository([review]),
            companyResponseRepository: companyResponseRepository,
            unitOfWork: unitOfWork,
            tools: [tool],
            users: [staffUser]);

        var result = await service.AddCompanyResponseAsync(review.Id, ValidCompanyResponseRequest(), staffUser.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal("Company responses can only be added to approved reviews.", result.Error);
        Assert.Empty(companyResponseRepository.Items);
        Assert.Null(review.CompanyResponse);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task AddCompanyResponseAsync_WhenReviewAlreadyHasResponse_ReturnsConflict()
    {
        var category = new Category { Id = 1, Name = "Access & Lifting" };
        var tool = CreateTool(9, category, isActive: true);
        var staffUser = CreateStaffUser(42, UserRole.Admin, "Shelton Team");
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
        review.CompanyResponse = new CompanyResponse
        {
            Id = 11,
            ReviewId = review.Id,
            StaffUserId = staffUser.Id,
            StaffUser = staffUser,
            ResponseText = "Existing response",
            CreatedDate = new DateTime(2026, 4, 2, 8, 0, 0, DateTimeKind.Utc),
            UpdatedDate = new DateTime(2026, 4, 2, 8, 0, 0, DateTimeKind.Utc)
        };
        var service = CreateService(reviews: [review], tools: [tool], users: [staffUser]);

        var result = await service.AddCompanyResponseAsync(review.Id, ValidCompanyResponseRequest(), staffUser.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.FailureType);
        Assert.Equal("A company response already exists for review with ID 1.", result.Error);
    }

    [Fact]
    public async Task AddCompanyResponseAsync_WhenStaffUserIsNotStaff_ReturnsForbidden()
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
        var customerUser = CreateStaffUser(42, UserRole.Customer, "Regular Customer");
        var service = CreateService(reviews: [review], tools: [tool], users: [customerUser]);

        var result = await service.AddCompanyResponseAsync(review.Id, ValidCompanyResponseRequest(), customerUser.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, result.FailureType);
        Assert.Equal("Only staff users can manage company responses.", result.Error);
    }

    [Fact]
    public async Task UpdateCompanyResponseAsync_WhenResponseExists_UpdatesTextAndStaffUser()
    {
        var category = new Category { Id = 1, Name = "Access & Lifting" };
        var tool = CreateTool(9, category, isActive: true);
        var originalStaffUser = CreateStaffUser(42, UserRole.Moderator, "Initial Staff");
        var updatedStaffUser = CreateStaffUser(43, UserRole.Admin, "Updated Staff");
        var existingResponse = new CompanyResponse
        {
            Id = 11,
            ReviewId = 1,
            StaffUserId = originalStaffUser.Id,
            StaffUser = originalStaffUser,
            ResponseText = "Original response",
            CreatedDate = new DateTime(2026, 4, 2, 8, 0, 0, DateTimeKind.Utc),
            UpdatedDate = new DateTime(2026, 4, 2, 8, 0, 0, DateTimeKind.Utc)
        };
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
        review.CompanyResponse = existingResponse;
        var companyResponseRepository = new InMemoryRepository<CompanyResponse>([existingResponse]);
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(
            reviewRepository: new InMemoryReviewRepository([review]),
            companyResponseRepository: companyResponseRepository,
            unitOfWork: unitOfWork,
            tools: [tool],
            users: [originalStaffUser, updatedStaffUser]);

        var result = await service.UpdateCompanyResponseAsync(
            review.Id,
            new CreateCompanyResponseRequest("Updated response from the Shelton team."),
            updatedStaffUser.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal("Updated response from the Shelton team.", result.Value!.ResponseText);
        Assert.Equal("Updated Staff", result.Value.StaffName);
        Assert.Equal(updatedStaffUser.Id, existingResponse.StaffUserId);
        Assert.Equal("Updated response from the Shelton team.", existingResponse.ResponseText);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task UpdateCompanyResponseAsync_WhenResponseDoesNotExist_ReturnsNotFound()
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
        var staffUser = CreateStaffUser(42, UserRole.Admin, "Shelton Team");
        var service = CreateService(reviews: [review], tools: [tool], users: [staffUser]);

        var result = await service.UpdateCompanyResponseAsync(review.Id, ValidCompanyResponseRequest(), staffUser.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.FailureType);
        Assert.Equal("Company response for review with ID 1 not found.", result.Error);
    }

    [Fact]
    public async Task DeleteCompanyResponseAsync_WhenResponseExists_RemovesResponse()
    {
        var category = new Category { Id = 1, Name = "Access & Lifting" };
        var tool = CreateTool(9, category, isActive: true);
        var staffUser = CreateStaffUser(42, UserRole.Admin, "Shelton Team");
        var existingResponse = new CompanyResponse
        {
            Id = 11,
            ReviewId = 1,
            StaffUserId = staffUser.Id,
            StaffUser = staffUser,
            ResponseText = "Existing response",
            CreatedDate = new DateTime(2026, 4, 2, 8, 0, 0, DateTimeKind.Utc),
            UpdatedDate = new DateTime(2026, 4, 2, 8, 0, 0, DateTimeKind.Utc)
        };
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
        review.CompanyResponse = existingResponse;
        var companyResponseRepository = new InMemoryRepository<CompanyResponse>([existingResponse]);
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(
            reviewRepository: new InMemoryReviewRepository([review]),
            companyResponseRepository: companyResponseRepository,
            unitOfWork: unitOfWork,
            tools: [tool],
            users: [staffUser]);

        var result = await service.DeleteCompanyResponseAsync(review.Id, staffUser.Id);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        Assert.Empty(companyResponseRepository.Items);
        Assert.Null(review.CompanyResponse);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
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
        Assert.All(result.Value!, comment => Assert.Equal("Approved", comment.Status));
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

    [Fact]
    public async Task GetPendingReviewsAsync_WhenQueueIsEmpty_ReturnsEmptyPagedList()
    {
        var service = CreateService();

        var result = await service.GetPendingReviewsAsync(page: 1, pageSize: 10);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Items);
        Assert.Equal(0, result.Value.TotalCount);
        Assert.Equal(1, result.Value.Page);
        Assert.Equal(10, result.Value.PageSize);
    }

    [Fact]
    public async Task GetPendingReviewsAsync_WhenPendingItemsExist_ReturnsItemLevelQueueOldestFirst()
    {
        var category = new Category { Id = 1, Name = "Breaking & Drilling" };
        var tool = CreateTool(12, category, isActive: true);
        var newerPendingReview = CreateReview(
            2,
            tool,
            "Newer Reviewer",
            ReviewStatus.Pending,
            new DateTime(2026, 4, 12, 8, 0, 0, DateTimeKind.Utc),
            4,
            4,
            4,
            4,
            4);
        var olderPendingReview = CreateReview(
            1,
            tool,
            "Older Reviewer",
            ReviewStatus.Pending,
            new DateTime(2026, 4, 10, 8, 0, 0, DateTimeKind.Utc),
            5,
            4,
            4,
            5,
            4);
        olderPendingReview.Comments =
        [
            new ReviewComment
            {
                Id = 3,
                ReviewId = olderPendingReview.Id,
                CommenterName = "Already approved",
                CommentText = "This approved comment should not appear in the moderation queue.",
                Status = ReviewStatus.Approved,
                CreatedDate = new DateTime(2026, 4, 11, 8, 0, 0, DateTimeKind.Utc)
            },
            new ReviewComment
            {
                Id = 4,
                ReviewId = olderPendingReview.Id,
                CommenterName = "Pending commenter",
                CommentText = "This pending comment should appear for moderators.",
                Status = ReviewStatus.Pending,
                CreatedDate = new DateTime(2026, 4, 11, 9, 0, 0, DateTimeKind.Utc)
            }
        ];
        var approvedReview = CreateReview(
            3,
            tool,
            "Approved Reviewer",
            ReviewStatus.Approved,
            new DateTime(2026, 4, 9, 8, 0, 0, DateTimeKind.Utc),
            5,
            5,
            5,
            5,
            5);
        var service = CreateService(reviews: [newerPendingReview, olderPendingReview, approvedReview], tools: [tool]);

        var result = await service.GetPendingReviewsAsync(page: 1, pageSize: 10);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.TotalCount);
        Assert.Equal(["Review", "Comment", "Review"], result.Value.Items.Select(item => item.ItemType).ToArray());
        Assert.Equal(["Older Reviewer", "Pending commenter", "Newer Reviewer"], result.Value.Items.Select(item => item.AuthorName).ToArray());
        Assert.Equal([olderPendingReview.Id, 4, newerPendingReview.Id], result.Value.Items.Select(item => item.ItemId).ToArray());
        Assert.All(result.Value.Items, item => Assert.Equal("Pending", item.Status));
        var reviewItem = result.Value.Items.First();
        Assert.Equal(tool.Id, reviewItem.ToolId);
        Assert.Equal("Tool 12", reviewItem.ToolName);
        Assert.Equal(5, reviewItem.EquipmentRating);
        var commentItem = result.Value.Items[1];
        Assert.Equal(olderPendingReview.Id, commentItem.ReviewId);
        Assert.Null(commentItem.EquipmentRating);
        Assert.Null(commentItem.OverallRating);
    }

    [Fact]
    public async Task GetPendingReviewsAsync_WhenDatabaseTimestampHasNoKind_ReturnsUtcSubmittedDate()
    {
        var category = new Category { Id = 1, Name = "Breaking & Drilling" };
        var tool = CreateTool(12, category, isActive: true);
        var databaseTimestamp = new DateTime(2026, 6, 13, 12, 30, 0, DateTimeKind.Unspecified);
        var pendingReview = CreateReview(
            1,
            tool,
            "Recent Reviewer",
            ReviewStatus.Pending,
            databaseTimestamp,
            5,
            4,
            4,
            5,
            4);
        var service = CreateService(reviews: [pendingReview], tools: [tool]);

        var result = await service.GetPendingReviewsAsync(page: 1, pageSize: 10);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value!.Items);
        Assert.Equal(DateTimeKind.Utc, item.SubmittedDate.Kind);
        Assert.Equal(databaseTimestamp.Ticks, item.SubmittedDate.Ticks);
    }

    [Fact]
    public async Task GetPendingReviewsAsync_WhenApprovedReviewHasPendingComment_ReturnsCommentItem()
    {
        var category = new Category { Id = 1, Name = "Cleaning & Maintenance" };
        var tool = CreateTool(13, category, isActive: true);
        var pendingReview = CreateReview(
            1,
            tool,
            "Pending Review Author",
            ReviewStatus.Pending,
            new DateTime(2026, 4, 10, 8, 0, 0, DateTimeKind.Utc),
            4,
            4,
            4,
            4,
            4);
        var approvedReviewWithPendingComment = CreateReview(
            2,
            tool,
            "Approved Review Author",
            ReviewStatus.Approved,
            new DateTime(2026, 4, 8, 8, 0, 0, DateTimeKind.Utc),
            5,
            5,
            5,
            4,
            4);
        approvedReviewWithPendingComment.Comments =
        [
            new ReviewComment
            {
                Id = 8,
                ReviewId = approvedReviewWithPendingComment.Id,
                CommenterName = "Approved commenter",
                CommentText = "This approved comment should stay out of the moderation queue.",
                Status = ReviewStatus.Approved,
                CreatedDate = new DateTime(2026, 4, 9, 8, 0, 0, DateTimeKind.Utc)
            },
            new ReviewComment
            {
                Id = 9,
                ReviewId = approvedReviewWithPendingComment.Id,
                CommenterName = "Pending commenter",
                CommentText = "Please double-check whether the public guidance needs updating for this hire.",
                Status = ReviewStatus.Pending,
                CreatedDate = new DateTime(2026, 4, 11, 9, 0, 0, DateTimeKind.Utc)
            }
        ];
        var service = CreateService(reviews: [approvedReviewWithPendingComment, pendingReview], tools: [tool]);

        var result = await service.GetPendingReviewsAsync(page: 1, pageSize: 10);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.TotalCount);
        Assert.Equal(["Review", "Comment"], result.Value.Items.Select(item => item.ItemType).ToArray());
        Assert.Equal(["Pending Review Author", "Pending commenter"], result.Value.Items.Select(item => item.AuthorName).ToArray());

        var pendingComment = Assert.Single(result.Value.Items.Where(item => item.ItemType == "Comment"));
        Assert.Equal(approvedReviewWithPendingComment.Id, pendingComment.ReviewId);
        Assert.Equal(9, pendingComment.ItemId);
        Assert.Equal("Pending", pendingComment.Status);
        Assert.Null(pendingComment.EquipmentRating);
    }

    [Fact]
    public async Task GetPendingReviewsAsync_WhenReviewHasMultiplePendingComments_CountsEachCommentSeparately()
    {
        var category = new Category { Id = 1, Name = "Cleaning & Maintenance" };
        var tool = CreateTool(15, category, isActive: true);
        var approvedReview = CreateReview(
            1,
            tool,
            "Approved Review Author",
            ReviewStatus.Approved,
            new DateTime(2026, 4, 8, 8, 0, 0, DateTimeKind.Utc),
            5,
            5,
            5,
            4,
            4);
        approvedReview.Comments =
        [
            new ReviewComment
            {
                Id = 10,
                ReviewId = approvedReview.Id,
                CommenterName = "First pending commenter",
                CommentText = "First pending comment for exact count coverage.",
                Status = ReviewStatus.Pending,
                CreatedDate = new DateTime(2026, 4, 11, 9, 0, 0, DateTimeKind.Utc)
            },
            new ReviewComment
            {
                Id = 11,
                ReviewId = approvedReview.Id,
                CommenterName = "Second pending commenter",
                CommentText = "Second pending comment for exact count coverage.",
                Status = ReviewStatus.Pending,
                CreatedDate = new DateTime(2026, 4, 11, 10, 0, 0, DateTimeKind.Utc)
            }
        ];
        var service = CreateService(reviews: [approvedReview], tools: [tool]);

        var result = await service.GetPendingReviewsAsync(page: 1, pageSize: 10);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.TotalCount);
        Assert.Equal(["First pending commenter", "Second pending commenter"], result.Value.Items.Select(item => item.AuthorName).ToArray());
        Assert.All(result.Value.Items, item => Assert.Equal("Comment", item.ItemType));
    }

    [Fact]
    public async Task GetPendingReviewsAsync_WhenPageIsRequested_ReturnsRequestedSlice()
    {
        var category = new Category { Id = 1, Name = "Cleaning & Maintenance" };
        var tool = CreateTool(14, category, isActive: true);
        var reviews = Enumerable.Range(1, 12)
            .Select(index => CreateReview(
                index,
                tool,
                $"Pending Reviewer {index}",
                ReviewStatus.Pending,
                new DateTime(2026, 4, 1, 8, 0, 0, DateTimeKind.Utc).AddDays(index),
                4,
                4,
                4,
                4,
                4))
            .ToArray();
        var service = CreateService(reviews: reviews, tools: [tool]);

        var result = await service.GetPendingReviewsAsync(page: 2, pageSize: 5);

        Assert.True(result.IsSuccess);
        Assert.Equal(12, result.Value!.TotalCount);
        Assert.Equal(3, result.Value.TotalPages);
        Assert.Equal(["Pending Reviewer 6", "Pending Reviewer 7", "Pending Reviewer 8", "Pending Reviewer 9", "Pending Reviewer 10"], result.Value.Items.Select(item => item.AuthorName).ToArray());
        Assert.All(result.Value.Items, item => Assert.Equal("Review", item.ItemType));
    }

    [Fact]
    public async Task ModerateReviewAsync_WhenReviewDoesNotExist_ReturnsNotFound()
    {
        var service = CreateService();

        var result = await service.ModerateReviewAsync(404, new ModerateReviewRequest(true, null));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.FailureType);
        Assert.Equal("Review with ID 404 not found.", result.Error);
    }

    [Fact]
    public async Task ModerateReviewAsync_WhenApprovingPendingReview_UpdatesStatusAndToolRating()
    {
        var category = new Category { Id = 1, Name = "Garden & Landscaping" };
        var tool = CreateTool(16, category, isActive: true);
        tool.ReviewCount = 1;
        tool.OverallRating = 3.0m;
        var pendingReview = CreateReview(
            1,
            tool,
            "Pending Reviewer",
            ReviewStatus.Pending,
            new DateTime(2026, 4, 10, 8, 0, 0, DateTimeKind.Utc),
            5,
            4,
            4,
            5,
            4);
        pendingReview.RejectionReason = "Old reason";
        var approvedReview = CreateReview(
            2,
            tool,
            "Approved Reviewer",
            ReviewStatus.Approved,
            new DateTime(2026, 4, 9, 8, 0, 0, DateTimeKind.Utc),
            3,
            3,
            3,
            3,
            3);
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(reviews: [pendingReview, approvedReview], tools: [tool], unitOfWork: unitOfWork);

        var result = await service.ModerateReviewAsync(pendingReview.Id, new ModerateReviewRequest(true, null));

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        Assert.Equal(ReviewStatus.Approved, pendingReview.Status);
        Assert.Null(pendingReview.RejectionReason);
        Assert.Equal(2, tool.ReviewCount);
        Assert.Equal(3.70m, tool.OverallRating);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ModerateReviewAsync_WhenRejectingPendingReview_StoresTrimmedReason()
    {
        var category = new Category { Id = 1, Name = "Access & Lifting" };
        var tool = CreateTool(17, category, isActive: true);
        var review = CreateReview(
            1,
            tool,
            "Pending Reviewer",
            ReviewStatus.Pending,
            new DateTime(2026, 4, 10, 8, 0, 0, DateTimeKind.Utc),
            4,
            4,
            4,
            4,
            4);
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(reviews: [review], tools: [tool], unitOfWork: unitOfWork);

        var result = await service.ModerateReviewAsync(review.Id, new ModerateReviewRequest(false, "  Contains personal details.  "));

        Assert.True(result.IsSuccess);
        Assert.Equal(ReviewStatus.Rejected, review.Status);
        Assert.Equal("Contains personal details.", review.RejectionReason);
        Assert.Equal(0, tool.ReviewCount);
        Assert.Null(tool.OverallRating);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ModerateReviewAsync_WhenRejectingApprovedReview_RecalculatesToolRating()
    {
        var category = new Category { Id = 1, Name = "Building & Construction" };
        var tool = CreateTool(18, category, isActive: true);
        tool.ReviewCount = 2;
        tool.OverallRating = 4.00m;
        var highReview = CreateReview(
            1,
            tool,
            "High Reviewer",
            ReviewStatus.Approved,
            new DateTime(2026, 4, 10, 8, 0, 0, DateTimeKind.Utc),
            5,
            5,
            5,
            5,
            5);
        var lowerReview = CreateReview(
            2,
            tool,
            "Lower Reviewer",
            ReviewStatus.Approved,
            new DateTime(2026, 4, 9, 8, 0, 0, DateTimeKind.Utc),
            3,
            3,
            3,
            3,
            3);
        var service = CreateService(reviews: [highReview, lowerReview], tools: [tool]);

        var result = await service.ModerateReviewAsync(highReview.Id, new ModerateReviewRequest(false, "No longer meets publication rules."));

        Assert.True(result.IsSuccess);
        Assert.Equal(ReviewStatus.Rejected, highReview.Status);
        Assert.Equal(1, tool.ReviewCount);
        Assert.Equal(3.00m, tool.OverallRating);
    }

    [Fact]
    public async Task ModerateReviewAsync_WhenRejectingWithoutReason_ReturnsValidationFailure()
    {
        var category = new Category { Id = 1, Name = "Building & Construction" };
        var tool = CreateTool(19, category, isActive: true);
        var review = CreateReview(
            1,
            tool,
            "Pending Reviewer",
            ReviewStatus.Pending,
            new DateTime(2026, 4, 10, 8, 0, 0, DateTimeKind.Utc),
            4,
            4,
            4,
            4,
            4);
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(reviews: [review], tools: [tool], unitOfWork: unitOfWork);

        var result = await service.ModerateReviewAsync(review.Id, new ModerateReviewRequest(false, "  "));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal("Rejection reason is required when rejecting.", result.Error);
        Assert.Equal(ReviewStatus.Pending, review.Status);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ModerateCommentAsync_WhenCommentDoesNotExist_ReturnsNotFound()
    {
        var service = CreateService();

        var result = await service.ModerateCommentAsync(404, new ModerateReviewRequest(true, null));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.FailureType);
        Assert.Equal("Comment with ID 404 not found.", result.Error);
    }

    [Fact]
    public async Task ModerateCommentAsync_WhenApprovingComment_UpdatesStatusAndClearsReason()
    {
        var comment = new ReviewComment
        {
            Id = 7,
            ReviewId = 1,
            CommenterName = "Sam",
            CommentText = "Helpful review and we had the same experience.",
            Status = ReviewStatus.Pending,
            RejectionReason = "Old reason"
        };
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(commentRepository: new InMemoryRepository<ReviewComment>([comment]), unitOfWork: unitOfWork);

        var result = await service.ModerateCommentAsync(comment.Id, new ModerateReviewRequest(true, null));

        Assert.True(result.IsSuccess);
        Assert.Equal(ReviewStatus.Approved, comment.Status);
        Assert.Null(comment.RejectionReason);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ModerateCommentAsync_WhenRejectingComment_StoresTrimmedReason()
    {
        var comment = new ReviewComment
        {
            Id = 7,
            ReviewId = 1,
            CommenterName = "Sam",
            CommentText = "Helpful review and we had the same experience.",
            Status = ReviewStatus.Pending
        };
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(commentRepository: new InMemoryRepository<ReviewComment>([comment]), unitOfWork: unitOfWork);

        var result = await service.ModerateCommentAsync(comment.Id, new ModerateReviewRequest(false, "  Off-topic comment.  "));

        Assert.True(result.IsSuccess);
        Assert.Equal(ReviewStatus.Rejected, comment.Status);
        Assert.Equal("Off-topic comment.", comment.RejectionReason);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    private static ReviewService CreateService(
        InMemoryReviewRepository? reviewRepository = null,
        InMemoryRepository<CompanyResponse>? companyResponseRepository = null,
        InMemoryRepository<ReviewComment>? commentRepository = null,
        FakeUnitOfWork? unitOfWork = null,
        IEnumerable<Review>? reviews = null,
        IEnumerable<Tool>? tools = null,
        IEnumerable<User>? users = null)
    {
        return new ReviewService(
            reviewRepository ?? new InMemoryReviewRepository(reviews),
            companyResponseRepository ?? new InMemoryRepository<CompanyResponse>(),
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

    private static CreateCompanyResponseRequest ValidCompanyResponseRequest()
    {
        return new CreateCompanyResponseRequest("Thanks for taking the time to leave this review.");
    }

    private static ReviewComment CreateApprovedComment(int id, int reviewId)
    {
        return new ReviewComment
        {
            Id = id,
            ReviewId = reviewId,
            CommenterName = $"Commenter {id}",
            CommentText = "Approved public reply that contributes to helpful sorting.",
            Status = ReviewStatus.Approved,
            CreatedDate = new DateTime(2026, 4, 12, 8, 0, 0, DateTimeKind.Utc).AddMinutes(id)
        };
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

    private static User CreateUser(int id, UserRole role, string name)
    {
        return new User
        {
            Id = id,
            Name = name,
            Email = $"{name.Replace(" ", string.Empty).ToLowerInvariant()}@example.com",
            PasswordHash = "hash",
            Role = role
        };
    }

    private static User CreateStaffUser(int id, UserRole role, string name)
    {
        return CreateUser(id, role, name);
    }
}
