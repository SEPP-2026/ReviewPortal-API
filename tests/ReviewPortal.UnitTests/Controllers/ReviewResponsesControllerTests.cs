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

public class ReviewResponsesControllerTests
{
    [Fact]
    public void Controller_HasAuthorizeAttributeForStaffRoles()
    {
        var attribute = typeof(ReviewResponsesController).GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(attribute);
        Assert.Equal("Admin,Moderator", attribute!.Roles);
    }

    [Fact]
    public async Task Create_WhenUserIdClaimIsMissing_ReturnsUnauthorizedProblem()
    {
        var controller = new ReviewResponsesController(new FakeReviewService())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            }
        };

        var result = await controller.Create(7, new CreateCompanyResponseRequest("Thanks for your review."), CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemResult.StatusCode);
    }

    [Fact]
    public async Task Create_WhenResponseIsSubmitted_ReturnsCreatedResponse()
    {
        var response = CreateCompanyResponseDto();
        var reviewService = new FakeReviewService
        {
            AddCompanyResponseResult = Result<CompanyResponseDto>.Success(response)
        };
        var controller = CreateController(reviewService, userId: 42);
        var request = new CreateCompanyResponseRequest("Thanks for your review.");

        var result = await controller.Create(7, request, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedResult>(result);
        Assert.Equal("/api/reviews/7/response", createdResult.Location);
        Assert.Same(response, createdResult.Value);
        Assert.Equal(7, reviewService.LastCompanyResponseReviewId);
        Assert.Equal(42, reviewService.LastCompanyResponseStaffUserId);
        Assert.Equal(request, reviewService.LastCompanyResponseRequest);
    }

    [Fact]
    public async Task Create_WhenReviewAlreadyHasResponse_ReturnsConflictProblem()
    {
        var reviewService = new FakeReviewService
        {
            AddCompanyResponseResult = Result<CompanyResponseDto>.Conflict("A company response already exists for review with ID 7.")
        };
        var controller = CreateController(reviewService, userId: 42);

        var result = await controller.Create(7, new CreateCompanyResponseRequest("Thanks for your review."), CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status409Conflict, problemResult.StatusCode);
    }

    [Fact]
    public async Task Create_WhenReviewIsNotApproved_ReturnsBadRequestProblem()
    {
        var reviewService = new FakeReviewService
        {
            AddCompanyResponseResult = Result<CompanyResponseDto>.Failure("Company responses can only be added to approved reviews.")
        };
        var controller = CreateController(reviewService, userId: 42);

        var result = await controller.Create(7, new CreateCompanyResponseRequest("Thanks for your review."), CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problemResult.StatusCode);
    }

    [Fact]
    public async Task Update_WhenResponseIsUpdated_ReturnsOk()
    {
        var response = CreateCompanyResponseDto() with { ResponseText = "Updated response" };
        var reviewService = new FakeReviewService
        {
            UpdateCompanyResponseResult = Result<CompanyResponseDto>.Success(response)
        };
        var controller = CreateController(reviewService, userId: 43);
        var request = new CreateCompanyResponseRequest("Updated response");

        var result = await controller.Update(7, request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(response, okResult.Value);
        Assert.Equal(7, reviewService.LastUpdateCompanyResponseReviewId);
        Assert.Equal(43, reviewService.LastUpdateCompanyResponseStaffUserId);
        Assert.Equal(request, reviewService.LastUpdateCompanyResponseRequest);
    }

    [Fact]
    public async Task Delete_WhenResponseIsDeleted_ReturnsNoContent()
    {
        var reviewService = new FakeReviewService
        {
            DeleteCompanyResponseResult = Result<bool>.Success(true)
        };
        var controller = CreateController(reviewService, userId: 42);

        var result = await controller.Delete(7, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(7, reviewService.LastDeleteCompanyResponseReviewId);
        Assert.Equal(42, reviewService.LastDeleteCompanyResponseStaffUserId);
    }

    private static ReviewResponsesController CreateController(FakeReviewService reviewService, int userId)
    {
        return new ReviewResponsesController(reviewService)
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

    private static CompanyResponseDto CreateCompanyResponseDto()
    {
        return new CompanyResponseDto(
            11,
            "Thanks for your review.",
            "Shelton Team",
            new DateTime(2026, 4, 13, 10, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 13, 10, 0, 0, DateTimeKind.Utc));
    }

    private sealed class FakeReviewService : IReviewService
    {
        public int? LastCompanyResponseReviewId { get; private set; }

        public int? LastCompanyResponseStaffUserId { get; private set; }

        public CreateCompanyResponseRequest? LastCompanyResponseRequest { get; private set; }

        public int? LastUpdateCompanyResponseReviewId { get; private set; }

        public int? LastUpdateCompanyResponseStaffUserId { get; private set; }

        public CreateCompanyResponseRequest? LastUpdateCompanyResponseRequest { get; private set; }

        public int? LastDeleteCompanyResponseReviewId { get; private set; }

        public int? LastDeleteCompanyResponseStaffUserId { get; private set; }

        public Result<CompanyResponseDto> AddCompanyResponseResult { get; set; } =
            Result<CompanyResponseDto>.Success(CreateCompanyResponseDto());

        public Result<CompanyResponseDto> UpdateCompanyResponseResult { get; set; } =
            Result<CompanyResponseDto>.Success(CreateCompanyResponseDto());

        public Result<bool> DeleteCompanyResponseResult { get; set; } =
            Result<bool>.Success(true);

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
            LastCompanyResponseReviewId = reviewId;
            LastCompanyResponseStaffUserId = staffUserId;
            LastCompanyResponseRequest = request;
            return Task.FromResult(AddCompanyResponseResult);
        }

        public Task<Result<CompanyResponseDto>> UpdateCompanyResponseAsync(int reviewId, CreateCompanyResponseRequest request, int staffUserId, CancellationToken cancellationToken = default)
        {
            LastUpdateCompanyResponseReviewId = reviewId;
            LastUpdateCompanyResponseStaffUserId = staffUserId;
            LastUpdateCompanyResponseRequest = request;
            return Task.FromResult(UpdateCompanyResponseResult);
        }

        public Task<Result<bool>> DeleteCompanyResponseAsync(int reviewId, int staffUserId, CancellationToken cancellationToken = default)
        {
            LastDeleteCompanyResponseReviewId = reviewId;
            LastDeleteCompanyResponseStaffUserId = staffUserId;
            return Task.FromResult(DeleteCompanyResponseResult);
        }

        public Task<Result<PagedList<ReviewSummaryDto>>> GetUserReviewsAsync(int userId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
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
