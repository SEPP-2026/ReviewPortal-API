namespace ReviewPortal.Application.DTOs.Tools;

public sealed class AdminToolQueryRequest
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;

    public string? SearchTerm { get; init; }

    public int? CategoryId { get; init; }

    public string? Status { get; init; }

    public string? SortBy { get; init; }
}

public record AdminToolSummaryDto(
    int Id,
    int CategoryId,
    string CategoryName,
    string Name,
    decimal HourlyRate,
    decimal DailyRate,
    decimal WeeklyRate,
    bool IsActive,
    decimal? OverallRating,
    int ReviewCount,
    bool HasEnoughReviewsToRate,
    string? RatingMessage,
    string? ThumbnailUrl,
    DateTime UpdatedDate
);
