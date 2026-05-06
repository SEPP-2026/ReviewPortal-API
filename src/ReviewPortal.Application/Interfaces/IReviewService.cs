using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Reviews;

namespace ReviewPortal.Application.Interfaces;

public interface IReviewService
{
    Task<Result<ReviewDto>> CreateReviewAsync(int toolId, CreateReviewRequest request, int? userId = null, CancellationToken cancellationToken = default);

    Task<Result<ToolReviewsDto>> GetApprovedReviewsAsync(int toolId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<Result<ReviewCommentDto>> AddCommentAsync(int reviewId, CreateCommentRequest request, int? userId = null, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<ReviewCommentDto>>> GetApprovedCommentsAsync(int reviewId, CancellationToken cancellationToken = default);

    Task<Result<CompanyResponseDto>> AddCompanyResponseAsync(int reviewId, CreateCompanyResponseRequest request, int staffUserId, CancellationToken cancellationToken = default);

    Task<Result<CompanyResponseDto>> UpdateCompanyResponseAsync(int reviewId, CreateCompanyResponseRequest request, int staffUserId, CancellationToken cancellationToken = default);

    Task<Result<bool>> DeleteCompanyResponseAsync(int reviewId, int staffUserId, CancellationToken cancellationToken = default);

    Task<Result<PagedList<ReviewSummaryDto>>> GetUserReviewsAsync(int userId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<Result<PagedList<ModerationQueueItemDto>>> GetPendingReviewsAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    Task<Result<bool>> ModerateReviewAsync(int reviewId, ModerateReviewRequest request, CancellationToken cancellationToken = default);

    Task<Result<bool>> ModerateCommentAsync(int commentId, ModerateReviewRequest request, CancellationToken cancellationToken = default);
}
