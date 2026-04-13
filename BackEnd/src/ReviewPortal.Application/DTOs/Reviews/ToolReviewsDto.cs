using ReviewPortal.Application.Common;

namespace ReviewPortal.Application.DTOs.Reviews;

public record ToolReviewsDto(
    int ToolId,
    decimal? AverageOverallRating,
    int TotalApprovedReviews,
    string? EmptyStateMessage,
    PagedList<ReviewDto> Reviews
);
