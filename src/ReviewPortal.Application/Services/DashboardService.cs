using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Dashboard;
using ReviewPortal.Application.Interfaces;
using ReviewPortal.Domain.Entities;
using ReviewPortal.Domain.Enums;
using ReviewPortal.Domain.Interfaces;

namespace ReviewPortal.Application.Services;

public class DashboardService : IDashboardService
{
    private const int RankingLimit = 5;
    private const int MinimumReviewsRequiredForRating = 2;

    private readonly IToolRepository _toolRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IRepository<ReviewComment> _reviewCommentRepository;

    public DashboardService(
        IToolRepository toolRepository,
        IReviewRepository reviewRepository,
        IRepository<ReviewComment> reviewCommentRepository)
    {
        _toolRepository = toolRepository;
        _reviewRepository = reviewRepository;
        _reviewCommentRepository = reviewCommentRepository;
    }

    public async Task<Result<DashboardStatsDto>> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        var tools = await _toolRepository.GetAllAsync(cancellationToken);
        var reviews = await _reviewRepository.GetAllAsync(cancellationToken);
        var comments = await _reviewCommentRepository.GetAllAsync(cancellationToken);

        var totalActiveTools = tools.Count(tool => tool.IsActive);
        var totalInactiveTools = tools.Count(tool => !tool.IsActive);
        var pendingModerationCount =
            reviews.Count(review => review.Status == ReviewStatus.Pending) +
            comments.Count(comment => comment.Status == ReviewStatus.Pending);

        var currentMonthStart = GetCurrentUtcMonthStart();
        var nextMonthStart = currentMonthStart.AddMonths(1);
        var reviewsPublishedThisMonth = reviews.Count(review =>
            review.Status == ReviewStatus.Approved &&
            review.CreatedDate >= currentMonthStart &&
            review.CreatedDate < nextMonthStart);

        return Result<DashboardStatsDto>.Success(new DashboardStatsDto(
            totalActiveTools,
            totalInactiveTools,
            pendingModerationCount,
            reviewsPublishedThisMonth,
            GetTopRatedTools(tools),
            GetMostReviewedTools(tools)));
    }

    private static IReadOnlyList<ToolRankingDto> GetTopRatedTools(IEnumerable<Tool> tools)
    {
        return tools
            .Where(tool => tool.ReviewCount >= MinimumReviewsRequiredForRating && tool.OverallRating.HasValue)
            .OrderByDescending(tool => tool.OverallRating)
            .ThenByDescending(tool => tool.ReviewCount)
            .ThenBy(tool => tool.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(tool => tool.Id)
            .Take(RankingLimit)
            .Select(MapRanking)
            .ToList();
    }

    private static IReadOnlyList<ToolRankingDto> GetMostReviewedTools(IEnumerable<Tool> tools)
    {
        return tools
            .Where(tool => tool.ReviewCount > 0)
            .OrderByDescending(tool => tool.ReviewCount)
            .ThenByDescending(tool => tool.OverallRating ?? 0m)
            .ThenBy(tool => tool.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(tool => tool.Id)
            .Take(RankingLimit)
            .Select(MapRanking)
            .ToList();
    }

    private static ToolRankingDto MapRanking(Tool tool)
    {
        return new ToolRankingDto(
            tool.Id,
            tool.Name,
            tool.OverallRating,
            tool.ReviewCount);
    }

    private static DateTime GetCurrentUtcMonthStart()
    {
        var utcNow = DateTime.UtcNow;
        return new DateTime(utcNow.Year, utcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
    }
}
