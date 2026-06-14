using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReviewPortal.API.Controllers;
using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Reviews;
using ReviewPortal.Application.Interfaces;

namespace ReviewPortal.UnitTests.Controllers;

public class ToolReviewsControllerTests
{
    [Fact]
    public async Task GetApproved_WhenReviewsExist_ReturnsOkAndPassesPaging()
    {
        var response = new ToolReviewsDto(
            7,
            4.2m,
            2,
            null,
            new PagedList<ReviewDto>([CreateReviewDto()], 2, 5, 7));
        var reviewService = new FakeReviewService
        {
            GetApprovedReviewsResult = Result<ToolReviewsDto>.Success(response)
        };
        var controller = new ToolReviewsController(reviewService);

        var result = await controller.GetApproved(7, page: 2, pageSize: 5, sortBy: "helpful", cancellationToken: CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(response, okResult.Value);
        Assert.Equal(7, reviewService.LastGetToolId);
        Assert.Equal(2, reviewService.LastGetPage);
        Assert.Equal(5, reviewService.LastGetPageSize);
        Assert.Equal("helpful", reviewService.LastGetSortBy);
    }

    [Fact]
    public async Task GetApproved_WhenDefaultsAreUsed_PassesDefaultPageSize()
    {
        var reviewService = new FakeReviewService
        {
            GetApprovedReviewsResult = Result<ToolReviewsDto>.Success(
                new ToolReviewsDto(7, null, 0, "No reviews yet - be the first to share your experience", new PagedList<ReviewDto>([], 1, 10, 0)))
        };
        var controller = new ToolReviewsController(reviewService);

        await controller.GetApproved(7, cancellationToken: CancellationToken.None);

        Assert.Equal(1, reviewService.LastGetPage);
        Assert.Equal(10, reviewService.LastGetPageSize);
    }

    [Fact]
    public async Task GetApproved_WhenValidationFails_ReturnsBadRequestProblem()
    {
        var reviewService = new FakeReviewService
        {
            GetApprovedReviewsResult = Result<ToolReviewsDto>.Failure("Page must be greater than or equal to 1.")
        };
        var controller = new ToolReviewsController(reviewService);

        var result = await controller.GetApproved(7, page: 0, pageSize: 10, cancellationToken: CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problemResult.StatusCode);
    }

    [Fact]
    public async Task GetApproved_WhenToolDoesNotExist_ReturnsNotFoundProblem()
    {
        var reviewService = new FakeReviewService
        {
            GetApprovedReviewsResult = Result<ToolReviewsDto>.NotFound("Tool with ID 404 not found.")
        };
        var controller = new ToolReviewsController(reviewService);

        var result = await controller.GetApproved(404, page: 1, pageSize: 10, cancellationToken: CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemResult.StatusCode);
    }

    [Fact]
    public async Task Create_WhenReviewIsSubmitted_ReturnsCreatedResponse()
    {
        var review = CreateReviewDto();
        var reviewService = new FakeReviewService
        {
            CreateReviewResult = Result<ReviewDto>.Success(review)
        };
        var controller = new ToolReviewsController(reviewService);
        var request = CreateRequest();

        var result = await controller.Create(7, request, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedResult>(result);
        Assert.Equal("/api/tools/7/reviews/15", createdResult.Location);
        Assert.Same(review, createdResult.Value);
        Assert.Equal(7, reviewService.LastToolId);
        Assert.Equal(request, reviewService.LastRequest);
        Assert.Null(reviewService.LastUserId);
    }

    [Fact]
    public async Task Create_WhenUserIsAuthenticated_PassesUserIdToService()
    {
        var reviewService = new FakeReviewService
        {
            CreateReviewResult = Result<ReviewDto>.Success(CreateReviewDto())
        };
        var controller = new ToolReviewsController(reviewService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.NameIdentifier, "42")
                    ], "TestAuth"))
                }
            }
        };

        await controller.Create(7, CreateRequest(), CancellationToken.None);

        Assert.Equal(42, reviewService.LastUserId);
    }

    [Fact]
    public async Task Create_WhenSubClaimIsPresent_PassesUserIdToService()
    {
        var reviewService = new FakeReviewService
        {
            CreateReviewResult = Result<ReviewDto>.Success(CreateReviewDto())
        };
        var controller = new ToolReviewsController(reviewService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim("sub", "84")
                    ], "TestAuth"))
                }
            }
        };

        await controller.Create(7, CreateRequest(), CancellationToken.None);

        Assert.Equal(84, reviewService.LastUserId);
    }

    [Fact]
    public async Task Create_WhenValidationFails_ReturnsBadRequestProblem()
    {
        var reviewService = new FakeReviewService
        {
            CreateReviewResult = Result<ReviewDto>.Failure("Review text must be at least 20 characters.")
        };
        var controller = new ToolReviewsController(reviewService);

        var result = await controller.Create(7, CreateRequest(), CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problemResult.StatusCode);
    }

    private static CreateReviewRequest CreateRequest()
    {
        return new CreateReviewRequest(
            "Sam",
            "sam@example.com",
            "Excellent tool condition and a really helpful pickup handover.",
            5,
            4,
            4,
            5,
            4);
    }

    private static ReviewDto CreateReviewDto()
    {
        return new ReviewDto(
            15,
            7,
            "Pressure Washer",
            "Sam",
            "Excellent tool condition and a really helpful pickup handover.",
            5,
            4,
            4,
            5,
            4,
            4.4m,
            "Pending",
            null,
            new DateTime(2026, 4, 12, 10, 0, 0, DateTimeKind.Utc),
            9,
            [],
            null);
    }

    private sealed class FakeReviewService : IReviewService
    {
        public int? LastToolId { get; private set; }

        public int? LastGetToolId { get; private set; }

        public int? LastGetPage { get; private set; }

        public int? LastGetPageSize { get; private set; }

        public string? LastGetSortBy { get; private set; }

        public CreateReviewRequest? LastRequest { get; private set; }

        public int? LastUserId { get; private set; }

        public Result<ReviewDto> CreateReviewResult { get; set; } = Result<ReviewDto>.Success(CreateReviewDto());

        public Result<ToolReviewsDto> GetApprovedReviewsResult { get; set; } =
            Result<ToolReviewsDto>.Success(
                new ToolReviewsDto(7, null, 0, "No reviews yet - be the first to share your experience", new PagedList<ReviewDto>([], 1, 10, 0)));

        public Task<Result<ReviewDto>> CreateReviewAsync(int toolId, CreateReviewRequest request, int? userId = null, CancellationToken cancellationToken = default)
        {
            LastToolId = toolId;
            LastRequest = request;
            LastUserId = userId;
            return Task.FromResult(CreateReviewResult);
        }

        public Task<Result<PagedList<ReviewDto>>> GetAllApprovedReviewsAsync(int page, int pageSize, string? sortBy = null, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Result<ToolReviewsDto>> GetApprovedReviewsAsync(int toolId, int page, int pageSize, string? sortBy = null, CancellationToken cancellationToken = default)
        {
            LastGetToolId = toolId;
            LastGetPage = page;
            LastGetPageSize = pageSize;
            LastGetSortBy = sortBy;
            return Task.FromResult(GetApprovedReviewsResult);
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
