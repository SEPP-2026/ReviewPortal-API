namespace ReviewPortal.Application.DTOs.Dashboard;

public record DashboardStatsDto(
    int TotalActiveTools,
    int TotalInactiveTools,
    int PendingModerationCount,
    int ReviewsPublishedThisMonth,
    IReadOnlyList<ToolRankingDto> TopRatedTools,
    IReadOnlyList<ToolRankingDto> MostReviewedTools
);

public record ToolRankingDto(
    int ToolId,
    string ToolName,
    decimal? OverallRating,
    int ReviewCount
);
