using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Tools;
using ReviewPortal.Application.Interfaces;
using ReviewPortal.Domain.Entities;
using ReviewPortal.Domain.Interfaces;

namespace ReviewPortal.Application.Services;

public class ToolService : IToolService
{
    private const int MaxPageSize = 100;

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

        var toolList = sortedTools.ToList();
        var items = toolList
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapSummary)
            .ToList();

        return Result<PagedList<ToolSummaryDto>>.Success(new PagedList<ToolSummaryDto>(items, page, pageSize, toolList.Count));
    }

    public Task<Result<ToolDto>> GetToolByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<ToolDto>.Failure("Tool detail retrieval is not implemented in this slice."));
    }

    public Task<Result<PagedList<ToolSummaryDto>>> SearchToolsAsync(string query, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<PagedList<ToolSummaryDto>>.Failure("Tool search is not implemented in this slice."));
    }

    public Task<Result<PagedList<ToolSummaryDto>>> FilterByPriceRangeAsync(int categoryId, decimal? minPrice, decimal? maxPrice, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<PagedList<ToolSummaryDto>>.Failure("Price-range filtering is not implemented in this slice."));
    }

    public Task<Result<RentalCalculationResponse>> CalculateRentalCostAsync(int toolId, RentalCalculationRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<RentalCalculationResponse>.Failure("Rental calculation is not implemented in this slice."));
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
}
