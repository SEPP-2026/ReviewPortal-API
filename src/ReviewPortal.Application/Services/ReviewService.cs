using FluentValidation;
using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Reviews;
using ReviewPortal.Application.Interfaces;
using ReviewPortal.Application.Validators;
using ReviewPortal.Application.Validators.Reviews;
using ReviewPortal.Domain.Entities;
using ReviewPortal.Domain.Enums;
using ReviewPortal.Domain.Interfaces;

namespace ReviewPortal.Application.Services;

public class ReviewService : IReviewService
{
    private const int DefaultMaxPageSize = 100;
    private const string NoReviewsMessage = "No reviews yet - be the first to share your experience";
    private const int ReviewSnippetLength = 140;

    private readonly IReviewRepository _reviewRepository;
    private readonly IRepository<CompanyResponse> _companyResponseRepository;
    private readonly IRepository<ReviewComment> _reviewCommentRepository;
    private readonly IToolRepository _toolRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateReviewRequest> _createReviewRequestValidator;
    private readonly IValidator<CreateCommentRequest> _createCommentRequestValidator;
    private readonly IValidator<CreateCompanyResponseRequest> _createCompanyResponseRequestValidator;
    private readonly IValidator<ModerateReviewRequest> _moderateReviewRequestValidator;

    public ReviewService(
        IReviewRepository reviewRepository,
        IRepository<CompanyResponse> companyResponseRepository,
        IRepository<ReviewComment> reviewCommentRepository,
        IToolRepository toolRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateReviewRequest>? createReviewRequestValidator = null,
        IValidator<CreateCommentRequest>? createCommentRequestValidator = null,
        IValidator<CreateCompanyResponseRequest>? createCompanyResponseRequestValidator = null,
        IValidator<ModerateReviewRequest>? moderateReviewRequestValidator = null)
    {
        _reviewRepository = reviewRepository;
        _companyResponseRepository = companyResponseRepository;
        _reviewCommentRepository = reviewCommentRepository;
        _toolRepository = toolRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _createReviewRequestValidator = createReviewRequestValidator ?? new CreateReviewRequestValidator();
        _createCommentRequestValidator = createCommentRequestValidator ?? new CreateCommentRequestValidator();
        _createCompanyResponseRequestValidator = createCompanyResponseRequestValidator ?? new CreateCompanyResponseRequestValidator();
        _moderateReviewRequestValidator = moderateReviewRequestValidator ?? new ModerateReviewRequestValidator();
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

        var validationError = RequestValidatorRunner.Validate(_createReviewRequestValidator, request, "Review request is required.")
            ?? ValidateAnonymousReviewerDetails(request, userId);
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

        var totalApprovedReviews = await _reviewRepository.CountApprovedByToolIdAsync(toolId, cancellationToken);
        var averageOverallRating = totalApprovedReviews == 0
            ? null
            : await _reviewRepository.GetAverageOverallRatingByToolIdAsync(toolId, cancellationToken);
        var approvedReviews = totalApprovedReviews == 0
            ? Array.Empty<Review>()
            : await _reviewRepository.GetApprovedByToolIdWithDetailsAsync(toolId, page, pageSize, cancellationToken);
        var pagedReviews = new PagedList<ReviewDto>(
            approvedReviews
                .Select(review => MapReview(review, tool.Name))
                .ToList(),
            page,
            pageSize,
            totalApprovedReviews);

        if (averageOverallRating.HasValue)
        {
            averageOverallRating = Math.Round(averageOverallRating.Value, 2, MidpointRounding.AwayFromZero);
        }

        return Result<ToolReviewsDto>.Success(new ToolReviewsDto(
            toolId,
            averageOverallRating,
            totalApprovedReviews,
            totalApprovedReviews == 0 ? NoReviewsMessage : null,
            pagedReviews));
    }

    public async Task<Result<ReviewCommentDto>> AddCommentAsync(
        int reviewId,
        CreateCommentRequest request,
        int? userId = null,
        CancellationToken cancellationToken = default)
    {
        var validationError = RequestValidatorRunner.Validate(_createCommentRequestValidator, request, "Comment request is required.")
            ?? ValidateAnonymousCommenterName(request, userId);
        if (validationError is not null)
        {
            return Result<ReviewCommentDto>.Failure(validationError);
        }

        var review = await _reviewRepository.GetByIdAsync(reviewId, cancellationToken);
        if (review is null || review.Status != ReviewStatus.Approved)
        {
            return Result<ReviewCommentDto>.NotFound($"Review with ID {reviewId} not found.");
        }

        var commenterNameResult = await ResolveCommenterNameAsync(request, userId, cancellationToken);
        if (!commenterNameResult.IsSuccess)
        {
            return Result<ReviewCommentDto>.Failure(
                commenterNameResult.Error!,
                commenterNameResult.FailureType ?? ErrorType.Validation);
        }

        var comment = new ReviewComment
        {
            ReviewId = reviewId,
            UserId = userId,
            CommenterName = commenterNameResult.Value!,
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

    public async Task<Result<CompanyResponseDto>> AddCompanyResponseAsync(
        int reviewId,
        CreateCompanyResponseRequest request,
        int staffUserId,
        CancellationToken cancellationToken = default)
    {
        var validationError = RequestValidatorRunner.Validate(_createCompanyResponseRequestValidator, request, "Company response request is required.");
        if (validationError is not null)
        {
            return Result<CompanyResponseDto>.Failure(validationError);
        }

        var staffUserResult = await ResolveStaffUserAsync(staffUserId, cancellationToken);
        if (!staffUserResult.IsSuccess)
        {
            return Result<CompanyResponseDto>.Failure(
                staffUserResult.Error!,
                staffUserResult.FailureType ?? ErrorType.Validation);
        }

        var review = await _reviewRepository.GetByIdWithDetailsAsync(reviewId, cancellationToken);
        if (review is null)
        {
            return Result<CompanyResponseDto>.NotFound($"Review with ID {reviewId} not found.");
        }

        if (review.Status != ReviewStatus.Approved)
        {
            return Result<CompanyResponseDto>.Failure("Company responses can only be added to approved reviews.");
        }

        if (review.CompanyResponse is not null)
        {
            return Result<CompanyResponseDto>.Conflict($"A company response already exists for review with ID {reviewId}.");
        }

        var utcNow = DateTime.UtcNow;
        var companyResponse = new CompanyResponse
        {
            ReviewId = reviewId,
            StaffUserId = staffUserId,
            StaffUser = staffUserResult.Value!,
            ResponseText = request.ResponseText.Trim(),
            CreatedDate = utcNow,
            UpdatedDate = utcNow
        };

        await _companyResponseRepository.AddAsync(companyResponse, cancellationToken);
        review.CompanyResponse = companyResponse;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CompanyResponseDto>.Success(MapCompanyResponse(companyResponse));
    }

    public async Task<Result<CompanyResponseDto>> UpdateCompanyResponseAsync(
        int reviewId,
        CreateCompanyResponseRequest request,
        int staffUserId,
        CancellationToken cancellationToken = default)
    {
        var validationError = RequestValidatorRunner.Validate(_createCompanyResponseRequestValidator, request, "Company response request is required.");
        if (validationError is not null)
        {
            return Result<CompanyResponseDto>.Failure(validationError);
        }

        var staffUserResult = await ResolveStaffUserAsync(staffUserId, cancellationToken);
        if (!staffUserResult.IsSuccess)
        {
            return Result<CompanyResponseDto>.Failure(
                staffUserResult.Error!,
                staffUserResult.FailureType ?? ErrorType.Validation);
        }

        var review = await _reviewRepository.GetByIdWithDetailsAsync(reviewId, cancellationToken);
        if (review is null)
        {
            return Result<CompanyResponseDto>.NotFound($"Review with ID {reviewId} not found.");
        }

        if (review.CompanyResponse is null)
        {
            return Result<CompanyResponseDto>.NotFound($"Company response for review with ID {reviewId} not found.");
        }

        review.CompanyResponse.ResponseText = request.ResponseText.Trim();
        review.CompanyResponse.StaffUserId = staffUserId;
        review.CompanyResponse.StaffUser = staffUserResult.Value!;
        review.CompanyResponse.UpdatedDate = DateTime.UtcNow;

        await _companyResponseRepository.UpdateAsync(review.CompanyResponse, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CompanyResponseDto>.Success(MapCompanyResponse(review.CompanyResponse));
    }

    public async Task<Result<bool>> DeleteCompanyResponseAsync(
        int reviewId,
        int staffUserId,
        CancellationToken cancellationToken = default)
    {
        var staffUserResult = await ResolveStaffUserAsync(staffUserId, cancellationToken);
        if (!staffUserResult.IsSuccess)
        {
            return Result<bool>.Failure(
                staffUserResult.Error!,
                staffUserResult.FailureType ?? ErrorType.Validation);
        }

        var review = await _reviewRepository.GetByIdWithDetailsAsync(reviewId, cancellationToken);
        if (review is null)
        {
            return Result<bool>.NotFound($"Review with ID {reviewId} not found.");
        }

        if (review.CompanyResponse is null)
        {
            return Result<bool>.NotFound($"Company response for review with ID {reviewId} not found.");
        }

        await _companyResponseRepository.DeleteAsync(review.CompanyResponse, cancellationToken);
        review.CompanyResponse = null;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    public async Task<Result<PagedList<ReviewSummaryDto>>> GetUserReviewsAsync(
        int userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidatePaging(page, pageSize);
        if (validationError is not null)
        {
            return Result<PagedList<ReviewSummaryDto>>.Failure(validationError);
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result<PagedList<ReviewSummaryDto>>.Unauthorized("Authenticated user account could not be found.");
        }

        var totalReviews = await _reviewRepository.CountByUserIdAsync(userId, cancellationToken);
        var reviews = totalReviews == 0
            ? Array.Empty<Review>()
            : await _reviewRepository.GetByUserIdWithDetailsAsync(userId, page, pageSize, cancellationToken);

        var pagedReviews = new PagedList<ReviewSummaryDto>(
            reviews.Select(MapReviewSummary).ToList(),
            page,
            pageSize,
            totalReviews);

        return Result<PagedList<ReviewSummaryDto>>.Success(pagedReviews);
    }

    public async Task<Result<PagedList<ModerationQueueItemDto>>> GetPendingReviewsAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var validationError = ValidatePaging(page, pageSize);
        if (validationError is not null)
        {
            return Result<PagedList<ModerationQueueItemDto>>.Failure(validationError);
        }

        var pendingReviews = await _reviewRepository.GetPendingWithDetailsAsync(cancellationToken);
        var queueItems = pendingReviews
            .SelectMany(CreateModerationQueueItems)
            .OrderBy(item => item.SubmittedDate)
            .ThenBy(item => GetModerationItemTypeSort(item.ItemType))
            .ThenBy(item => item.ItemId)
            .ToList();

        var pagedQueueItems = new PagedList<ModerationQueueItemDto>(
            queueItems
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList(),
            page,
            pageSize,
            queueItems.Count);

        return Result<PagedList<ModerationQueueItemDto>>.Success(pagedQueueItems);
    }

    public async Task<Result<bool>> ModerateReviewAsync(int reviewId, ModerateReviewRequest request, CancellationToken cancellationToken = default)
    {
        var validationError = RequestValidatorRunner.Validate(_moderateReviewRequestValidator, request, "Moderation request is required.");
        if (validationError is not null)
        {
            return Result<bool>.Failure(validationError);
        }

        var review = await _reviewRepository.GetByIdAsync(reviewId, cancellationToken);
        if (review is null)
        {
            return Result<bool>.NotFound($"Review with ID {reviewId} not found.");
        }

        review.Status = request.Approved ? ReviewStatus.Approved : ReviewStatus.Rejected;
        review.RejectionReason = request.Approved ? null : request.RejectionReason!.Trim();

        var tool = await _toolRepository.GetByIdAsync(review.ToolId, cancellationToken);
        if (tool is null)
        {
            return Result<bool>.NotFound($"Tool with ID {review.ToolId} not found.");
        }

        await _reviewRepository.UpdateAsync(review, cancellationToken);
        await RecalculateToolRatingAsync(tool, cancellationToken);
        await _toolRepository.UpdateAsync(tool, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> ModerateCommentAsync(int commentId, ModerateReviewRequest request, CancellationToken cancellationToken = default)
    {
        var validationError = RequestValidatorRunner.Validate(_moderateReviewRequestValidator, request, "Moderation request is required.");
        if (validationError is not null)
        {
            return Result<bool>.Failure(validationError);
        }

        var comment = await _reviewCommentRepository.GetByIdAsync(commentId, cancellationToken);
        if (comment is null)
        {
            return Result<bool>.NotFound($"Comment with ID {commentId} not found.");
        }

        comment.Status = request.Approved ? ReviewStatus.Approved : ReviewStatus.Rejected;
        comment.RejectionReason = request.Approved ? null : request.RejectionReason!.Trim();

        await _reviewCommentRepository.UpdateAsync(comment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    private static string? ValidateAnonymousReviewerDetails(CreateReviewRequest request, int? userId)
    {
        if (userId.HasValue)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(request.ReviewerName))
        {
            return "Reviewer name is required when submitting anonymously.";
        }

        if (string.IsNullOrWhiteSpace(request.ReviewerEmail))
        {
            return "Reviewer email is required when submitting anonymously.";
        }

        return null;
    }

    private static string? ValidateAnonymousCommenterName(CreateCommentRequest request, int? userId)
    {
        if (userId.HasValue || !string.IsNullOrWhiteSpace(request.CommenterName))
        {
            return null;
        }

        return "Commenter name is required.";
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

    private async Task<Result<string>> ResolveCommenterNameAsync(
        CreateCommentRequest request,
        int? userId,
        CancellationToken cancellationToken)
    {
        if (!userId.HasValue)
        {
            return Result<string>.Success(request.CommenterName.Trim());
        }

        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
        if (user is null)
        {
            return Result<string>.Unauthorized("Authenticated user account could not be found.");
        }

        return Result<string>.Success(user.Name);
    }

    private async Task<Result<User>> ResolveStaffUserAsync(int staffUserId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(staffUserId, cancellationToken);
        if (user is null)
        {
            return Result<User>.Unauthorized("Authenticated user account could not be found.");
        }

        if (user.Role is not UserRole.Admin and not UserRole.Moderator)
        {
            return Result<User>.Forbidden("Only staff users can manage company responses.");
        }

        return Result<User>.Success(user);
    }

    private async Task RecalculateToolRatingAsync(Tool tool, CancellationToken cancellationToken)
    {
        var approvedReviews = (await _reviewRepository.GetByToolIdAsync(tool.Id, cancellationToken))
            .Where(review => review.Status == ReviewStatus.Approved)
            .ToList();

        tool.ReviewCount = approvedReviews.Count;
        tool.OverallRating = approvedReviews.Count == 0
            ? null
            : Math.Round(
                approvedReviews.Average(review => review.OverallRating),
                2,
                MidpointRounding.AwayFromZero);
    }

    private static IEnumerable<ModerationQueueItemDto> CreateModerationQueueItems(Review review)
    {
        if (review.Status == ReviewStatus.Pending)
        {
            yield return MapReviewModerationQueueItem(review);
        }

        foreach (var comment in review.Comments.Where(comment => comment.Status == ReviewStatus.Pending))
        {
            yield return MapCommentModerationQueueItem(review, comment);
        }
    }

    private static ModerationQueueItemDto MapReviewModerationQueueItem(Review review)
    {
        return new ModerationQueueItemDto(
            "Review",
            review.Id,
            review.Id,
            review.ToolId,
            review.Tool?.Name ?? string.Empty,
            review.ReviewerName,
            review.ReviewText,
            review.CreatedDate,
            review.Status.ToString(),
            review.EquipmentRating,
            review.CustomerServiceRating,
            review.TechnicalSupportRating,
            review.AfterSalesRating,
            review.ValueForMoneyRating,
            review.OverallRating);
    }

    private static ModerationQueueItemDto MapCommentModerationQueueItem(Review review, ReviewComment comment)
    {
        return new ModerationQueueItemDto(
            "Comment",
            comment.Id,
            review.Id,
            review.ToolId,
            review.Tool?.Name ?? string.Empty,
            comment.CommenterName,
            comment.CommentText,
            comment.CreatedDate,
            comment.Status.ToString(),
            null,
            null,
            null,
            null,
            null,
            null);
    }

    private static int GetModerationItemTypeSort(string itemType)
    {
        return itemType == "Review" ? 0 : 1;
    }

    private static ReviewDto MapReview(Review review, string toolName, ReviewStatus commentStatus = ReviewStatus.Approved)
    {
        var comments = review.Comments
            .Where(comment => comment.Status == commentStatus)
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
            comments,
            companyResponse);
    }

    private static ReviewSummaryDto MapReviewSummary(Review review)
    {
        return new ReviewSummaryDto(
            review.Id,
            review.ToolId,
            review.Tool.Name,
            BuildReviewSnippet(review.ReviewText),
            review.OverallRating,
            review.Status.ToString(),
            review.RejectionReason,
            review.CreatedDate,
            review.CompanyResponse is not null);
    }

    private static string BuildReviewSnippet(string reviewText)
    {
        var trimmedReviewText = reviewText.Trim();
        if (trimmedReviewText.Length <= ReviewSnippetLength)
        {
            return trimmedReviewText;
        }

        return $"{trimmedReviewText[..(ReviewSnippetLength - 3)].TrimEnd()}...";
    }

    private static ReviewCommentDto MapComment(ReviewComment comment)
    {
        return new ReviewCommentDto(
            comment.Id,
            comment.CommenterName,
            comment.CommentText,
            comment.Status.ToString(),
            comment.CreatedDate);
    }

    private static CompanyResponseDto MapCompanyResponse(CompanyResponse companyResponse)
    {
        return new CompanyResponseDto(
            companyResponse.Id,
            companyResponse.ResponseText,
            companyResponse.StaffUser?.Name ?? string.Empty,
            companyResponse.CreatedDate,
            companyResponse.UpdatedDate);
    }
}
