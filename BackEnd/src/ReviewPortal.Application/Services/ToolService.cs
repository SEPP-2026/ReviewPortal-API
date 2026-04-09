using System.Globalization;
using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Tools;
using ReviewPortal.Application.Interfaces;
using ReviewPortal.Domain.Entities;
using ReviewPortal.Domain.Interfaces;

namespace ReviewPortal.Application.Services;

public class ToolService : IToolService
{
    private const int MaxPageSize = 100;
    private const int HoursInDay = 24;
    private const int HoursInWeek = 168;

    private readonly IToolRepository _toolRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ToolService(IToolRepository toolRepository, ICategoryRepository categoryRepository)
    {
        _toolRepository = toolRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<PagedList<ToolSummaryDto>>> GetToolsByCategoryAsync(
        int categoryId,
        int page,
        int pageSize,
        string? sortBy = null,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidatePaging(page, pageSize);
        if (validationError is not null)
        {
            return Result<PagedList<ToolSummaryDto>>.Failure(validationError);
        }

        var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
        if (category is null)
        {
            return Result<PagedList<ToolSummaryDto>>.NotFound($"Category with ID {categoryId} not found.");
        }

        var tools = await _toolRepository.GetActiveByCategoryAsync(categoryId, cancellationToken);
        var sortedTools = ApplySort(tools, sortBy, out validationError);
        if (validationError is not null)
        {
            return Result<PagedList<ToolSummaryDto>>.Failure(validationError);
        }

        return Result<PagedList<ToolSummaryDto>>.Success(CreatePagedSummary(sortedTools, page, pageSize));
    }

    public Task<Result<ToolDto>> GetToolByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return GetToolByIdInternalAsync(id, cancellationToken);
    }

    public async Task<Result<PagedList<ToolSummaryDto>>> SearchToolsAsync(string query, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var validationError = ValidatePaging(page, pageSize) ?? ValidateSearchQuery(query);
        if (validationError is not null)
        {
            return Result<PagedList<ToolSummaryDto>>.Failure(validationError);
        }

        var normalizedQuery = query.Trim();
        var tools = await _toolRepository.GetAllActiveWithDetailsAsync(cancellationToken);
        var matchingTools = tools
            .Where(tool =>
                ContainsIgnoreCase(tool.Name, normalizedQuery) ||
                ContainsIgnoreCase(tool.Description, normalizedQuery) ||
                ContainsIgnoreCase(tool.Category.Name, normalizedQuery))
            .OrderBy(tool => tool.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(tool => tool.Id);

        return Result<PagedList<ToolSummaryDto>>.Success(CreatePagedSummary(matchingTools, page, pageSize));
    }

    public Task<Result<PagedList<ToolSummaryDto>>> FilterByPriceRangeAsync(int categoryId, decimal? minPrice, decimal? maxPrice, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<PagedList<ToolSummaryDto>>.Failure("Price-range filtering is not implemented in this slice."));
    }

    public async Task<Result<RentalCalculationResponse>> CalculateRentalCostAsync(int toolId, RentalCalculationRequest request, CancellationToken cancellationToken = default)
    {
        if (request.EndDateTime <= request.StartDateTime)
        {
            return Result<RentalCalculationResponse>.Failure("End date/time must be after the start date/time.");
        }

        var tool = await _toolRepository.GetByIdWithDetailsAsync(toolId, cancellationToken);
        if (tool is null || !tool.IsActive)
        {
            return Result<RentalCalculationResponse>.NotFound($"Tool with ID {toolId} not found.");
        }

        var rentalPeriod = request.EndDateTime - request.StartDateTime;
        if (rentalPeriod.TotalHours > int.MaxValue)
        {
            return Result<RentalCalculationResponse>.Failure("Rental period is too long.");
        }

        var billableHours = (int)Math.Ceiling(rentalPeriod.TotalHours);
        var cheapestCombination = FindCheapestRentalCombination(
            billableHours,
            tool.HourlyRate,
            tool.DailyRate,
            tool.WeeklyRate);

        return Result<RentalCalculationResponse>.Success(new RentalCalculationResponse(
            tool.Name,
            request.StartDateTime,
            request.EndDateTime,
            BuildRentalBreakdown(cheapestCombination, tool),
            cheapestCombination.TotalCost));
    }

    public Task<Result<ToolDto>> CreateToolAsync(CreateToolRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<ToolDto>.Failure("Tool creation is not implemented in this slice."));
    }

    public Task<Result<ToolDto>> UpdateToolAsync(int id, UpdateToolRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<ToolDto>.Failure("Tool updates are not implemented in this slice."));
    }

    public Task<Result<bool>> SetToolStatusAsync(int id, bool isActive, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<bool>.Failure("Tool status changes are not implemented in this slice."));
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

        if (pageSize > MaxPageSize)
        {
            return $"Page size must not exceed {MaxPageSize}.";
        }

        return null;
    }

    private static string? ValidateSearchQuery(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return "Search query is required.";
        }

        if (query.Trim().Length > 200)
        {
            return "Search query must be 200 characters or fewer.";
        }

        return null;
    }

    private static IEnumerable<Tool> ApplySort(IEnumerable<Tool> tools, string? sortBy, out string? validationError)
    {
        validationError = null;

        var normalizedSort = NormalizeSort(sortBy);

        if (normalizedSort is not ("name" or "name_desc" or "price" or "price_asc" or "starting_price" or "price_desc" or "starting_price_desc" or "rating" or "rating_desc" or "rating_asc"))
        {
            validationError = "Invalid sortBy value. Supported values are name, price, price_desc, rating, and rating_asc.";
            return tools;
        }

        return normalizedSort switch
        {
            "name" => tools
                .OrderBy(tool => tool.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(tool => tool.Id),
            "name_desc" => tools
                .OrderByDescending(tool => tool.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(tool => tool.Id),
            "price" or "price_asc" or "starting_price" => tools
                .OrderBy(tool => GetStartingPrice(tool).Price)
                .ThenBy(tool => tool.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(tool => tool.Id),
            "price_desc" or "starting_price_desc" => tools
                .OrderByDescending(tool => GetStartingPrice(tool).Price)
                .ThenBy(tool => tool.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(tool => tool.Id),
            "rating" or "rating_desc" => tools
                .OrderByDescending(tool => tool.OverallRating ?? 0m)
                .ThenBy(tool => tool.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(tool => tool.Id),
            _ => tools
                .OrderBy(tool => tool.OverallRating ?? 0m)
                .ThenBy(tool => tool.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(tool => tool.Id)
        };
    }

    private static string NormalizeSort(string? sortBy)
    {
        return string.IsNullOrWhiteSpace(sortBy)
            ? "name"
            : sortBy.Trim().Replace("-", "_").ToLowerInvariant();
    }

    private static ToolSummaryDto MapSummary(Tool tool)
    {
        var (startingPrice, startingPriceUnit) = GetStartingPrice(tool);

        return new ToolSummaryDto(
            tool.Id,
            tool.Name,
            tool.Category.Name,
            startingPrice,
            startingPriceUnit,
            tool.DailyRate,
            tool.OverallRating,
            tool.ReviewCount,
            tool.Images
                .OrderBy(image => image.DisplayOrder)
                .ThenBy(image => image.Id)
                .Select(image => image.ImageUrl)
                .FirstOrDefault());
    }

    private static PagedList<ToolSummaryDto> CreatePagedSummary(IEnumerable<Tool> tools, int page, int pageSize)
    {
        var toolList = tools.ToList();
        var items = toolList
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapSummary)
            .ToList();

        return new PagedList<ToolSummaryDto>(items, page, pageSize, toolList.Count);
    }

    private static bool ContainsIgnoreCase(string value, string query)
    {
        return value.Contains(query, StringComparison.OrdinalIgnoreCase);
    }

    private static RentalCombination FindCheapestRentalCombination(
        int requestedHours,
        decimal hourlyRate,
        decimal dailyRate,
        decimal weeklyRate)
    {
        RentalCombination? bestCombination = null;
        var maxWeeks = DivideAndRoundUp(requestedHours, HoursInWeek) + 1;

        for (var weeks = 0; weeks <= maxWeeks; weeks++)
        {
            var remainingHoursAfterWeeks = requestedHours - (weeks * HoursInWeek);
            var daysNeeded = remainingHoursAfterWeeks > 0
                ? DivideAndRoundUp(remainingHoursAfterWeeks, HoursInDay)
                : 0;
            var maxDays = daysNeeded + 1;

            for (var days = 0; days <= maxDays; days++)
            {
                var hoursCoveredByLargerUnits = (weeks * HoursInWeek) + (days * HoursInDay);
                var hours = Math.Max(0, requestedHours - hoursCoveredByLargerUnits);
                var coveredHours = hoursCoveredByLargerUnits + hours;
                var totalCost = (weeks * weeklyRate) + (days * dailyRate) + (hours * hourlyRate);
                var candidate = new RentalCombination(weeks, days, hours, coveredHours, totalCost);

                if (bestCombination is null || IsBetterRentalCombination(candidate, bestCombination))
                {
                    bestCombination = candidate;
                }
            }
        }

        return bestCombination!;
    }

    private static bool IsBetterRentalCombination(RentalCombination candidate, RentalCombination currentBest)
    {
        if (candidate.TotalCost != currentBest.TotalCost)
        {
            return candidate.TotalCost < currentBest.TotalCost;
        }

        if (candidate.CoveredHours != currentBest.CoveredHours)
        {
            return candidate.CoveredHours < currentBest.CoveredHours;
        }

        return candidate.UnitCount < currentBest.UnitCount;
    }

    private static int DivideAndRoundUp(int value, int divisor)
    {
        return (value + divisor - 1) / divisor;
    }

    private static string BuildRentalBreakdown(RentalCombination combination, Tool tool)
    {
        var parts = new List<string>();

        AddBreakdownPart(parts, combination.Weeks, "week", tool.WeeklyRate);
        AddBreakdownPart(parts, combination.Days, "day", tool.DailyRate);
        AddBreakdownPart(parts, combination.Hours, "hour", tool.HourlyRate);

        return $"{string.Join(" + ", parts)} = {FormatCost(combination.TotalCost)}";
    }

    private static void AddBreakdownPart(List<string> parts, int quantity, string unit, decimal rate)
    {
        if (quantity <= 0)
        {
            return;
        }

        parts.Add($"{quantity} {unit}{Pluralize(quantity)} x {FormatCost(rate)}/{unit}");
    }

    private static string Pluralize(int quantity)
    {
        return quantity == 1 ? string.Empty : "s";
    }

    private static string FormatCost(decimal value)
    {
        return value.ToString("0.00", CultureInfo.InvariantCulture);
    }

    private async Task<Result<ToolDto>> GetToolByIdInternalAsync(int id, CancellationToken cancellationToken)
    {
        var tool = await _toolRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        if (tool is null || !tool.IsActive)
        {
            return Result<ToolDto>.NotFound($"Tool with ID {id} not found.");
        }

        return Result<ToolDto>.Success(new ToolDto(
            tool.Id,
            tool.CategoryId,
            tool.Category.Name,
            tool.Name,
            tool.Description,
            tool.HourlyRate,
            tool.DailyRate,
            tool.WeeklyRate,
            tool.SpecialNotes,
            tool.DepositRequired,
            tool.DepositAmount,
            tool.IsActive,
            tool.OverallRating,
            tool.ReviewCount,
            tool.Images
                .OrderBy(image => image.DisplayOrder)
                .ThenBy(image => image.Id)
                .Select(image => new ToolImageDto(image.Id, image.ImageUrl, image.DisplayOrder))
                .ToList(),
            tool.CreatedDate,
            tool.UpdatedDate));
    }

    private static (decimal Price, string Unit) GetStartingPrice(Tool tool)
    {
        return new[]
        {
            (tool.HourlyRate, "hour"),
            (tool.DailyRate, "day"),
            (tool.WeeklyRate, "week")
        }
        .OrderBy(rate => rate.Item1)
        .First() switch
        {
            var rate => (rate.Item1, rate.Item2)
        };
    }

    private sealed record RentalCombination(int Weeks, int Days, int Hours, int CoveredHours, decimal TotalCost)
    {
        public int UnitCount => Weeks + Days + Hours;
    }
}
