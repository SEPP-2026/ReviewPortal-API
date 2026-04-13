using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReviewPortal.API.Controllers;
using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Reviews;
using ReviewPortal.Application.Interfaces;

namespace ReviewPortal.UnitTests.Controllers;

public class ReviewCommentsControllerTests
{
    [Fact]
    public async Task GetApproved_WhenCommentsExist_ReturnsOk()
    {
        var comments = new[]
        {
            new ReviewCommentDto(11, "Sam", "Helpful review and we saw the same thing.", new DateTime(2026, 4, 13, 10, 0, 0, DateTimeKind.Utc))
        };
        var reviewService = new FakeReviewService
        {
            ApprovedCommentsResult = Result<IReadOnlyList<ReviewCommentDto>>.Success(comments)
        };
        var controller = new ReviewCommentsController(reviewService);

        var result = await controller.GetApproved(7, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(comments, okResult.Value);
        Assert.Equal(7, reviewService.LastGetCommentsReviewId);
    }

    [Fact]
    public async Task GetApproved_WhenReviewDoesNotExist_ReturnsNotFoundProblem()
    {
        var reviewService = new FakeReviewService
        {
            ApprovedCommentsResult = Result<IReadOnlyList<ReviewCommentDto>>.NotFound("Review with ID 404 not found.")
        };
        var controller = new ReviewCommentsController(reviewService);

        var result = await controller.GetApproved(404, CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemResult.StatusCode);
    }

    [Fact]
    public async Task Create_WhenCommentIsSubmitted_ReturnsCreatedResponse()
    {
        var comment = new ReviewCommentDto(15, "Sam", "Helpful review and we saw the same thing.", new DateTime(2026, 4, 13, 10, 0, 0, DateTimeKind.Utc));
        var reviewService = new FakeReviewService
        {
            AddCommentResult = Result<ReviewCommentDto>.Success(comment)
        };
        var controller = new ReviewCommentsController(reviewService);
        var request = new CreateCommentRequest("Sam", "Helpful review and we saw the same thing.");

        var result = await controller.Create(7, request, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedResult>(result);
        Assert.Equal("/api/reviews/7/comments/15", createdResult.Location);
        Assert.Same(comment, createdResult.Value);
        Assert.Equal(7, reviewService.LastCommentReviewId);
        Assert.Equal(request, reviewService.LastCommentRequest);
    }

    [Fact]
    public async Task Create_WhenValidationFails_ReturnsBadRequestProblem()
    {
        var reviewService = new FakeReviewService
        {
            AddCommentResult = Result<ReviewCommentDto>.Failure("Comment text must be at least 10 characters.")
        };
        var controller = new ReviewCommentsController(reviewService);

        var result = await controller.Create(7, new CreateCommentRequest("Sam", "short"), CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problemResult.StatusCode);
    }

    private sealed class FakeReviewService : IReviewService
    {
        public int? LastCommentReviewId { get; private set; }

        public CreateCommentRequest? LastCommentRequest { get; private set; }

        public int? LastGetCommentsReviewId { get; private set; }

        public Result<ReviewCommentDto> AddCommentResult { get; set; } =
            Result<ReviewCommentDto>.Success(new ReviewCommentDto(1, "Default", "Default approved comment text.", DateTime.UtcNow));

        public Result<IReadOnlyList<ReviewCommentDto>> ApprovedCommentsResult { get; set; } =
            Result<IReadOnlyList<ReviewCommentDto>>.Success([]);

        public Task<Result<ReviewDto>> CreateReviewAsync(int toolId, CreateReviewRequest request, int? userId = null, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Result<ToolReviewsDto>> GetApprovedReviewsAsync(int toolId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Result<ReviewCommentDto>> AddCommentAsync(int reviewId, CreateCommentRequest request, CancellationToken cancellationToken = default)
        {
            LastCommentReviewId = reviewId;
            LastCommentRequest = request;
            return Task.FromResult(AddCommentResult);
        }

        public Task<Result<IReadOnlyList<ReviewCommentDto>>> GetApprovedCommentsAsync(int reviewId, CancellationToken cancellationToken = default)
        {
            LastGetCommentsReviewId = reviewId;
            return Task.FromResult(ApprovedCommentsResult);
        }

        public Task<Result<CompanyResponseDto>> AddCompanyResponseAsync(int reviewId, CreateCompanyResponseRequest request, int staffUserId, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
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
