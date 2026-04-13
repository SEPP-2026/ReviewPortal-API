using System.ComponentModel.DataAnnotations;
using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Reviews;
using ReviewPortal.Application.Interfaces;
using ReviewPortal.Domain.Entities;
using ReviewPortal.Domain.Enums;
using ReviewPortal.Domain.Interfaces;

namespace ReviewPortal.Application.Services;

public class ReviewService : IReviewService
{
    private const int DefaultMaxPageSize = 100;
    private const string NoReviewsMessage = "No reviews yet - be the first to share your experience";
    private const int MinCommentLength = 10;
    private const int MaxCommentLength = 1000;
    private const int MinReviewLength = 20;
    private const int MaxReviewLength = 2000;

    private readonly IReviewRepository _reviewRepository;
    private readonly IRepository<ReviewComment> _reviewCommentRepository;
    private readonly IToolRepository _toolRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReviewService(
        IReviewRepository reviewRepository,
        IRepository<ReviewComment> reviewCommentRepository,
        IToolRepository toolRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _reviewRepository = reviewRepository;
        _reviewCommentRepository = reviewCommentRepository;
        _toolRepository = toolRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ReviewDto>> CreateReviewAsync(
        int toolId,
        CreateReviewRequest request,
        int? userId = null,
        CancellationToken cancellationToken = default)
    {
        var tool = await _toolRepository.GetByIdAsync(toolId, cancellationToken);
        if (tool is null || !tool.IsActive)
        {
            return Result<ReviewDto>.NotFound($"Tool with ID {toolId} not found.");
        }

        var validationError = ValidateReviewRequest(request, userId);
        if (validationError is not null)
        {
            return Result<ReviewDto>.Failure(validationError);
        }

        var reviewerDetails = await ResolveReviewerDetailsAsync(request, userId, cancellationToken);
        if (!reviewerDetails.IsSuccess)
        {
            return Result<ReviewDto>.Failure(reviewerDetails.Error!, reviewerDetails.FailureType ?? ErrorType.Validation);
        }

        var review = new Review
        {
            ToolId = toolId,
            UserId = userId,
            ReviewerName = reviewerDetails.Value!.ReviewerName,
            ReviewerEmail = reviewerDetails.Value.ReviewerEmail,
            ReviewText = request.ReviewText.Trim(),
            EquipmentRating = request.EquipmentRating,
            CustomerServiceRating = request.CustomerServiceRating,
            TechnicalSupportRating = request.TechnicalSupportRating,
            AfterSalesRating = request.AfterSalesRating,
            ValueForMoneyRating = request.ValueForMoneyRating,
            Status = ReviewStatus.Pending
        };

        review.CalculateOverallRating();

        await _reviewRepository.AddAsync(review, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ReviewDto>.Success(MapReview(review, tool.Name));
    }

    public async Task<Result<ToolReviewsDto>> GetApprovedReviewsAsync(int toolId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var validationError = ValidatePaging(page, pageSize);
        if (validationError is not null)
        {
            return Result<ToolReviewsDto>.Failure(validationError);
        }

        var tool = await _toolRepository.GetByIdAsync(toolId, cancellationToken);
        if (tool is null || !tool.IsActive)
        {
            return Result<ToolReviewsDto>.NotFound($"Tool with ID {toolId} not found.");
        }

        var approvedReviews = await _reviewRepository.GetApprovedByToolIdWithDetailsAsync(toolId, cancellationToken);
        var mappedReviews = approvedReviews
            .Select(review => MapReview(review, tool.Name))
            .ToList();

        var pagedReviews = new PagedList<ReviewDto>(
            mappedReviews
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList(),
            page,
            pageSize,
            mappedReviews.Count);

        decimal? averageOverallRating = mappedReviews.Count == 0
            ? null
            : Math.Round(mappedReviews.Average(review => review.OverallRating), 2, MidpointRounding.AwayFromZero);

        return Result<ToolReviewsDto>.Success(new ToolReviewsDto(
            toolId,
            averageOverallRating,
            mappedReviews.Count,
            mappedReviews.Count == 0 ? NoReviewsMessage : null,
            pagedReviews));
    }

    public async Task<Result<ReviewCommentDto>> AddCommentAsync(int reviewId, CreateCommentRequest request, CancellationToken cancellationToken = default)
    {
        var validationError = ValidateCommentRequest(request);
        if (validationError is not null)
        {
            return Result<ReviewCommentDto>.Failure(validationError);
        }

        var review = await _reviewRepository.GetByIdAsync(reviewId, cancellationToken);
        if (review is null || review.Status != ReviewStatus.Approved)
        {
            return Result<ReviewCommentDto>.NotFound($"Review with ID {reviewId} not found.");
        }

        var comment = new ReviewComment
        {
            ReviewId = reviewId,
            CommenterName = request.CommenterName.Trim(),
            CommentText = request.CommentText.Trim(),
            Status = ReviewStatus.Pending
        };

        await _reviewCommentRepository.AddAsync(comment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ReviewCommentDto>.Success(MapComment(comment));
    }

    public async Task<Result<IReadOnlyList<ReviewCommentDto>>> GetApprovedCommentsAsync(int reviewId, CancellationToken cancellationToken = default)
    {
        var review = await _reviewRepository.GetByIdWithDetailsAsync(reviewId, cancellationToken);
        if (review is null || review.Status != ReviewStatus.Approved)
        {
            return Result<IReadOnlyList<ReviewCommentDto>>.NotFound($"Review with ID {reviewId} not found.");
        }

        var approvedComments = review.Comments
            .Where(comment => comment.Status == ReviewStatus.Approved)
            .OrderBy(comment => comment.CreatedDate)
            .ThenBy(comment => comment.Id)
            .Select(MapComment)
            .ToList();

        return Result<IReadOnlyList<ReviewCommentDto>>.Success(approvedComments);
    }

    public Task<Result<CompanyResponseDto>> AddCompanyResponseAsync(int reviewId, CreateCompanyResponseRequest request, int staffUserId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<CompanyResponseDto>.Failure("Company responses are not implemented in this slice."));
    }

    public Task<Result<PagedList<ReviewSummaryDto>>> GetUserReviewsAsync(int userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<PagedList<ReviewSummaryDto>>.Failure("User review retrieval is not implemented in this slice."));
    }

    public Task<Result<PagedList<ReviewDto>>> GetPendingReviewsAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<PagedList<ReviewDto>>.Failure("Pending review retrieval is not implemented in this slice."));
    }

    public Task<Result<bool>> ModerateReviewAsync(int reviewId, ModerateReviewRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<bool>.Failure("Review moderation is not implemented in this slice."));
    }

    public Task<Result<bool>> ModerateCommentAsync(int commentId, ModerateReviewRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<bool>.Failure("Comment moderation is not implemented in this slice."));
    }

    private static string? ValidateReviewRequest(CreateReviewRequest request, int? userId)
    {
        if (request is null)
        {
            return "Review request is required.";
        }

        if (!userId.HasValue)
        {
            if (string.IsNullOrWhiteSpace(request.ReviewerName))
            {
                return "Reviewer name is required when submitting anonymously.";
            }

            if (request.ReviewerName.Trim().Length > 100)
            {
                return "Reviewer name must be 100 characters or fewer.";
            }

            if (string.IsNullOrWhiteSpace(request.ReviewerEmail))
            {
                return "Reviewer email is required when submitting anonymously.";
            }

            if (request.ReviewerEmail.Trim().Length > 256)
            {
                return "Reviewer email must be 256 characters or fewer.";
            }

            var emailAttribute = new EmailAddressAttribute();
            if (!emailAttribute.IsValid(request.ReviewerEmail.Trim()))
            {
                return "Reviewer email must be a valid email address.";
            }
        }

        if (string.IsNullOrWhiteSpace(request.ReviewText))
        {
            return "Review text is required.";
        }

        var reviewText = request.ReviewText.Trim();
        if (reviewText.Length < MinReviewLength)
        {
            return $"Review text must be at least {MinReviewLength} characters.";
        }

        if (reviewText.Length > MaxReviewLength)
        {
            return $"Review text must be {MaxReviewLength} characters or fewer.";
        }

        return ValidateRating(request.EquipmentRating, "Equipment rating")
            ?? ValidateRating(request.CustomerServiceRating, "Customer service rating")
            ?? ValidateRating(request.TechnicalSupportRating, "Technical support rating")
            ?? ValidateRating(request.AfterSalesRating, "After-sales rating")
            ?? ValidateRating(request.ValueForMoneyRating, "Value for money rating");
    }

    private static string? ValidateCommentRequest(CreateCommentRequest request)
    {
        if (request is null)
        {
            return "Comment request is required.";
        }

        if (string.IsNullOrWhiteSpace(request.CommenterName))
        {
            return "Commenter name is required.";
        }

        if (request.CommenterName.Trim().Length > 100)
        {
            return "Commenter name must be 100 characters or fewer.";
        }

        if (string.IsNullOrWhiteSpace(request.CommentText))
        {
            return "Comment text is required.";
        }

        var commentText = request.CommentText.Trim();
        if (commentText.Length < MinCommentLength)
        {
            return $"Comment text must be at least {MinCommentLength} characters.";
        }

        if (commentText.Length > MaxCommentLength)
        {
            return $"Comment text must be {MaxCommentLength} characters or fewer.";
        }

        return null;
    }

    private static string? ValidateRating(int rating, string fieldName)
    {
        if (rating is < 1 or > 5)
        {
            return $"{fieldName} must be between 1 and 5.";
        }

        return null;
    }

    private static string? ValidatePaging(int page, int pageSize)
    {
        if (page < 1)
        {
            return "Page must be greater than or equal to 1.";
        }

        if (pageSize < 1)
        {
            return "Page size must be greater than or equal to 1.";
        }

        if (pageSize > DefaultMaxPageSize)
        {
            return $"Page size must not exceed {DefaultMaxPageSize}.";
        }

        return null;
    }

    private async Task<Result<(string ReviewerName, string ReviewerEmail)>> ResolveReviewerDetailsAsync(
        CreateReviewRequest request,
        int? userId,
        CancellationToken cancellationToken)
    {
        if (!userId.HasValue)
        {
            return Result<(string ReviewerName, string ReviewerEmail)>.Success(
                (request.ReviewerName.Trim(), request.ReviewerEmail.Trim()));
        }

        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
        if (user is null)
        {
            return Result<(string ReviewerName, string ReviewerEmail)>.Unauthorized("Authenticated user account could not be found.");
        }

        return Result<(string ReviewerName, string ReviewerEmail)>.Success((user.Name, user.Email));
    }

    private static ReviewDto MapReview(Review review, string toolName)
    {
        var approvedComments = review.Comments
            .Where(comment => comment.Status == ReviewStatus.Approved)
            .OrderBy(comment => comment.CreatedDate)
            .ThenBy(comment => comment.Id)
            .Select(MapComment)
            .ToList();

        var companyResponse = review.CompanyResponse is null
            ? null
            : new CompanyResponseDto(
                review.CompanyResponse.Id,
                review.CompanyResponse.ResponseText,
                review.CompanyResponse.StaffUser?.Name ?? string.Empty,
                review.CompanyResponse.CreatedDate,
                review.CompanyResponse.UpdatedDate);

        return new ReviewDto(
            review.Id,
            review.ToolId,
            toolName,
            review.ReviewerName,
            review.ReviewText,
            review.EquipmentRating,
            review.CustomerServiceRating,
            review.TechnicalSupportRating,
            review.AfterSalesRating,
            review.ValueForMoneyRating,
            review.OverallRating,
            review.Status.ToString(),
            review.RejectionReason,
            review.CreatedDate,
            approvedComments,
            companyResponse);
    }

    private static ReviewCommentDto MapComment(ReviewComment comment)
    {
        return new ReviewCommentDto(
            comment.Id,
            comment.CommenterName,
            comment.CommentText,
            comment.CreatedDate);
    }
}
