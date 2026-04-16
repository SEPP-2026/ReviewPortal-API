using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReviewPortal.API.Controllers;
using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Reviews;
using ReviewPortal.Application.Interfaces;

namespace ReviewPortal.UnitTests.Controllers;

public class UserReviewsControllerTests
{
    [Fact]
    public void Controller_HasAuthorizeAttribute()
    {
        var attribute = typeof(UserReviewsController).GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(attribute);
    }

    [Fact]
    public async Task GetMine_WhenUserIdClaimIsMissing_ReturnsUnauthorizedProblem()
    {
        var controller = new UserReviewsController(new FakeReviewService())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            }
        };

        var result = await controller.GetMine(cancellationToken: CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemResult.StatusCode);
    }

    [Fact]
    public async Task GetMine_WhenReviewsAreReturned_ReturnsOkAndPassesPaging()
    {
        var reviews = new PagedList<ReviewSummaryDto>(
        [
            new ReviewSummaryDto(
                5,
                11,
                "Tracked Dumper",
                "Helpful snippet",
                4.2m,
                "Approved",
                null,
                new DateTime(2026, 4, 12, 10, 0, 0, DateTimeKind.Utc),
                true)
        ],
            page: 2,
            pageSize: 5,
            totalCount: 11);
        var reviewService = new FakeReviewService
        {
            GetUserReviewsResult = Result<PagedList<ReviewSummaryDto>>.Success(reviews)
        };
        var controller = CreateController(reviewService, userId: 42);

        var result = await controller.GetMine(page: 2, pageSize: 5, cancellationToken: CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(reviews, okResult.Value);
        Assert.Equal(42, reviewService.LastUserReviewsUserId);
        Assert.Equal(2, reviewService.LastUserReviewsPage);
        Assert.Equal(5, reviewService.LastUserReviewsPageSize);
    }

    [Fact]
    public async Task GetMine_WhenServiceReturnsValidationFailure_ReturnsBadRequestProblem()
    {
        var reviewService = new FakeReviewService
        {
            GetUserReviewsResult = Result<PagedList<ReviewSummaryDto>>.Failure("Page must be greater than or equal to 1.")
        };
        var controller = CreateController(reviewService, userId: 42);

        var result = await controller.GetMine(page: 0, pageSize: 10, cancellationToken: CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problemResult.StatusCode);
    }

    private static UserReviewsController CreateController(FakeReviewService reviewService, int userId)
    {
        return new UserReviewsController(reviewService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                    ], "TestAuth"))
                }
            }
        };
    }

    private sealed class FakeReviewService : IReviewService
    {
        public int? LastUserReviewsUserId { get; private set; }

        public int? LastUserReviewsPage { get; private set; }

        public int? LastUserReviewsPageSize { get; private set; }

        public Result<PagedList<ReviewSummaryDto>> GetUserReviewsResult { get; set; } =
            Result<PagedList<ReviewSummaryDto>>.Success(new PagedList<ReviewSummaryDto>([], 1, 10, 0));

        public Task<Result<ReviewDto>> CreateReviewAsync(int toolId, CreateReviewRequest request, int? userId = null, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Result<ToolReviewsDto>> GetApprovedReviewsAsync(int toolId, int page, int pageSize, CancellationToken cancellationToken = default)
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
            LastUserReviewsUserId = userId;
            LastUserReviewsPage = page;
            LastUserReviewsPageSize = pageSize;
            return Task.FromResult(GetUserReviewsResult);
        }

        public Task<Result<PagedList<ReviewDto>>> GetPendingReviewsAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Result<bool>> ModerateReviewAsync(int reviewId, ModerateReviewRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Result<bool>> ModerateCommentAsync(int commentId, ModerateReviewRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
