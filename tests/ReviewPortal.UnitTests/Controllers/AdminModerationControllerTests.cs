using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReviewPortal.API.Controllers.Admin;
using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Reviews;
using ReviewPortal.Application.Interfaces;

namespace ReviewPortal.UnitTests.Controllers;

public class AdminModerationControllerTests
{
    [Fact]
    public void Controller_HasAuthorizeAttributeForAdminAndModeratorRoles()
    {
        var attribute = typeof(AdminModerationController).GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(attribute);
        Assert.Equal("Admin,Moderator", attribute!.Roles);
    }

    [Fact]
    public void Controller_WhenNoTokenIsProvided_IsProtectedByAuthorizeAttribute()
    {
        var attribute = typeof(AdminModerationController).GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(attribute);
    }

    [Fact]
    public void Controller_WhenCustomerRoleIsUsed_IsExcludedFromAllowedRoles()
    {
        var attribute = typeof(AdminModerationController).GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(attribute);
        Assert.DoesNotContain("Customer", attribute!.Roles!.Split(','));
    }

    [Fact]
    public async Task GetPending_WhenReviewsExist_ReturnsOkAndPassesPaging()
    {
        var response = new PagedList<ModerationQueueItemDto>([CreateModerationQueueItemDto("Review")], 2, 5, 12);
        var reviewService = new FakeReviewService
        {
            PendingReviewsResult = Result<PagedList<ModerationQueueItemDto>>.Success(response)
        };
        var controller = new AdminModerationController(reviewService);

        var result = await controller.GetPending(page: 2, pageSize: 5, cancellationToken: CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(response, okResult.Value);
        Assert.Equal(2, reviewService.LastPendingPage);
        Assert.Equal(5, reviewService.LastPendingPageSize);
    }

    [Fact]
    public async Task GetPending_WhenDefaultsAreUsed_PassesDefaultPaging()
    {
        var reviewService = new FakeReviewService();
        var controller = new AdminModerationController(reviewService);

        await controller.GetPending(cancellationToken: CancellationToken.None);

        Assert.Equal(1, reviewService.LastPendingPage);
        Assert.Equal(20, reviewService.LastPendingPageSize);
    }

    [Fact]
    public async Task GetPending_WhenValidationFails_ReturnsBadRequestProblem()
    {
        var reviewService = new FakeReviewService
        {
            PendingReviewsResult = Result<PagedList<ModerationQueueItemDto>>.Failure("Page must be greater than or equal to 1.")
        };
        var controller = new AdminModerationController(reviewService);

        var result = await controller.GetPending(page: 0, pageSize: 20, cancellationToken: CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problemResult.StatusCode);
    }

    [Fact]
    public async Task ModerateReview_WhenReviewIsApproved_ReturnsOkAndPassesRequest()
    {
        var reviewService = new FakeReviewService
        {
            ModerateReviewResult = Result<bool>.Success(true)
        };
        var controller = new AdminModerationController(reviewService);
        var request = new ModerateReviewRequest(true, null);

        var result = await controller.ModerateReview(7, request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.True(Assert.IsType<bool>(okResult.Value));
        Assert.Equal(7, reviewService.LastModerateReviewId);
        Assert.Equal(request, reviewService.LastModerateReviewRequest);
    }

    [Fact]
    public async Task ModerateReview_WhenReviewIsRejected_ReturnsOkAndPassesReason()
    {
        var reviewService = new FakeReviewService
        {
            ModerateReviewResult = Result<bool>.Success(true)
        };
        var controller = new AdminModerationController(reviewService);
        var request = new ModerateReviewRequest(false, "Offensive content.");

        var result = await controller.ModerateReview(7, request, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(7, reviewService.LastModerateReviewId);
        Assert.Equal(request, reviewService.LastModerateReviewRequest);
    }

    [Fact]
    public async Task ModerateReview_WhenReviewDoesNotExist_ReturnsNotFoundProblem()
    {
        var reviewService = new FakeReviewService
        {
            ModerateReviewResult = Result<bool>.NotFound("Review with ID 404 not found.")
        };
        var controller = new AdminModerationController(reviewService);

        var result = await controller.ModerateReview(404, new ModerateReviewRequest(true, null), CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemResult.StatusCode);
    }

    [Fact]
    public async Task ModerateComment_WhenCommentIsApproved_ReturnsOkAndPassesRequest()
    {
        var reviewService = new FakeReviewService
        {
            ModerateCommentResult = Result<bool>.Success(true)
        };
        var controller = new AdminModerationController(reviewService);
        var request = new ModerateReviewRequest(true, null);

        var result = await controller.ModerateComment(11, request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.True(Assert.IsType<bool>(okResult.Value));
        Assert.Equal(11, reviewService.LastModerateCommentId);
        Assert.Equal(request, reviewService.LastModerateCommentRequest);
    }

    [Fact]
    public async Task ModerateComment_WhenCommentIsRejected_ReturnsOkAndPassesReason()
    {
        var reviewService = new FakeReviewService
        {
            ModerateCommentResult = Result<bool>.Success(true)
        };
        var controller = new AdminModerationController(reviewService);
        var request = new ModerateReviewRequest(false, "Spam.");

        var result = await controller.ModerateComment(11, request, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(11, reviewService.LastModerateCommentId);
        Assert.Equal(request, reviewService.LastModerateCommentRequest);
    }

    private static ModerationQueueItemDto CreateModerationQueueItemDto(string itemType)
    {
        return new ModerationQueueItemDto(
            itemType,
            7,
            7,
            15,
            "Pressure Washer",
            "Sam",
            "Excellent tool condition and a really helpful pickup handover.",
            new DateTime(2026, 4, 12, 10, 0, 0, DateTimeKind.Utc),
            "Pending",
            5,
            4,
            4,
            5,
            4,
            4.4m);
    }

    private sealed class FakeReviewService : IReviewService
    {
        public int? LastPendingPage { get; private set; }

        public int? LastPendingPageSize { get; private set; }

        public int? LastModerateReviewId { get; private set; }

        public ModerateReviewRequest? LastModerateReviewRequest { get; private set; }

        public int? LastModerateCommentId { get; private set; }

        public ModerateReviewRequest? LastModerateCommentRequest { get; private set; }

        public Result<PagedList<ModerationQueueItemDto>> PendingReviewsResult { get; set; } =
            Result<PagedList<ModerationQueueItemDto>>.Success(new PagedList<ModerationQueueItemDto>([], 1, 20, 0));

        public Result<bool> ModerateReviewResult { get; set; } = Result<bool>.Success(true);

        public Result<bool> ModerateCommentResult { get; set; } = Result<bool>.Success(true);

        public Task<Result<ReviewDto>> CreateReviewAsync(int toolId, CreateReviewRequest request, int? userId = null, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Result<ToolReviewsDto>> GetApprovedReviewsAsync(int toolId, int page, int pageSize, string? sortBy = null, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Result<ReviewCommentDto>> AddCommentAsync(int reviewId, CreateCommentRequest request, int? userId = null, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Result<IReadOnlyList<ReviewCommentDto>>> GetApprovedCommentsAsync(int reviewId, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Result<CompanyResponseDto>> AddCompanyResponseAsync(int reviewId, CreateCompanyResponseRequest request, int staffUserId, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Result<CompanyResponseDto>> UpdateCompanyResponseAsync(int reviewId, CreateCompanyResponseRequest request, int staffUserId, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Result<bool>> DeleteCompanyResponseAsync(int reviewId, int staffUserId, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Result<PagedList<ReviewSummaryDto>>> GetUserReviewsAsync(int userId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Result<PagedList<ModerationQueueItemDto>>> GetPendingReviewsAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            LastPendingPage = page;
            LastPendingPageSize = pageSize;
            return Task.FromResult(PendingReviewsResult);
        }

        public Task<Result<bool>> ModerateReviewAsync(int reviewId, ModerateReviewRequest request, CancellationToken cancellationToken = default)
        {
            LastModerateReviewId = reviewId;
            LastModerateReviewRequest = request;
            return Task.FromResult(ModerateReviewResult);
        }

        public Task<Result<bool>> ModerateCommentAsync(int commentId, ModerateReviewRequest request, CancellationToken cancellationToken = default)
        {
            LastModerateCommentId = commentId;
            LastModerateCommentRequest = request;
            return Task.FromResult(ModerateCommentResult);
        }
    }
}
