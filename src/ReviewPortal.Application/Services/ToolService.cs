using System.Globalization;
using FluentValidation;
using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Tools;
using ReviewPortal.Application.Interfaces;
using ReviewPortal.Application.Validators;
using ReviewPortal.Application.Validators.Tools;
using ReviewPortal.Domain.Entities;
using ReviewPortal.Domain.Enums;
using ReviewPortal.Domain.Interfaces;

namespace ReviewPortal.Application.Services;

public class ToolService : IToolService
{
    private const int MaxPageSize = 100;
    private const int HoursInDay = 24;
    private const int HoursInWeek = 168;
    private const int MinimumReviewsRequiredForRating = 2;
    private const string NotEnoughReviewsMessage = "Not enough reviews to rate";

    private readonly IToolRepository _toolRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<RentalCalculationRequest> _rentalCalculationRequestValidator;
    private readonly IValidator<CreateToolRequest> _createToolRequestValidator;
    private readonly IValidator<UpdateToolRequest> _updateToolRequestValidator;

    public ToolService(
        IToolRepository toolRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        IValidator<RentalCalculationRequest>? rentalCalculationRequestValidator = null,
        IValidator<CreateToolRequest>? createToolRequestValidator = null,
        IValidator<UpdateToolRequest>? updateToolRequestValidator = null)
    {
        _toolRepository = toolRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _rentalCalculationRequestValidator = rentalCalculationRequestValidator ?? new RentalCalculationRequestValidator();
        _createToolRequestValidator = createToolRequestValidator ?? new CreateToolRequestValidator();
        _updateToolRequestValidator = updateToolRequestValidator ?? new UpdateToolRequestValidator();
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
        var rankedTools = ApplyRatingMetrics(tools);
        var sortedTools = ApplySort(rankedTools, sortBy, out validationError);
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

    public async Task<Result<PagedList<AdminToolSummaryDto>>> GetAdminToolsAsync(
        AdminToolQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateAdminToolQuery(request);
        if (validationError is not null)
        {
            return Result<PagedList<AdminToolSummaryDto>>.Failure(validationError);
        }

        var activeFilter = GetAdminActiveFilter(request.Status, out validationError);
        if (validationError is not null)
        {
            return Result<PagedList<AdminToolSummaryDto>>.Failure(validationError);
        }

        var tools = ApplyRatingMetrics(await _toolRepository.GetAllWithDetailsAsync(cancellationToken));
        var filteredTools = ApplyAdminFilters(tools, request.SearchTerm, request.CategoryId, activeFilter);
        var sortedTools = ApplyAdminSort(filteredTools, request.SortBy, out validationError);
        if (validationError is not null)
        {
            return Result<PagedList<AdminToolSummaryDto>>.Failure(validationError);
        }

        return Result<PagedList<AdminToolSummaryDto>>.Success(CreatePagedAdminSummary(sortedTools, request.Page, request.PageSize));
    }

    public async Task<Result<ToolDto>> GetAdminToolByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var tool = await _toolRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        if (tool is null)
        {
            return Result<ToolDto>.NotFound($"Tool with ID {id} not found.");
        }

        return Result<ToolDto>.Success(MapToolDetail(tool));
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
        var rankedTools = ApplyRatingMetrics(tools);
        var matchingTools = rankedTools
            .Where(tool =>
                ContainsIgnoreCase(tool.Name, normalizedQuery) ||
                ContainsIgnoreCase(tool.Description, normalizedQuery) ||
                ContainsIgnoreCase(tool.Category.Name, normalizedQuery))
            .OrderBy(tool => tool.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(tool => tool.Id);

        return Result<PagedList<ToolSummaryDto>>.Success(CreatePagedSummary(matchingTools, page, pageSize));
    }

    public async Task<Result<PagedList<ToolSummaryDto>>> FilterByPriceRangeAsync(
        int categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        int page,
        int pageSize,
        string? sortBy = null,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidatePaging(page, pageSize) ?? ValidatePriceRange(minPrice, maxPrice);
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
        var rankedTools = ApplyRatingMetrics(tools);
        var filteredTools = rankedTools.Where(tool =>
            (!minPrice.HasValue || tool.DailyRate >= minPrice.Value) &&
            (!maxPrice.HasValue || tool.DailyRate <= maxPrice.Value));

        var sortedTools = ApplySort(filteredTools, sortBy, out validationError);
        if (validationError is not null)
        {
            return Result<PagedList<ToolSummaryDto>>.Failure(validationError);
        }

        return Result<PagedList<ToolSummaryDto>>.Success(CreatePagedSummary(sortedTools, page, pageSize));
    }

    public async Task<Result<RentalCalculationResponse>> CalculateRentalCostAsync(int toolId, RentalCalculationRequest request, CancellationToken cancellationToken = default)
    {
        var validationError = RequestValidatorRunner.Validate(_rentalCalculationRequestValidator, request, "Rental calculation request is required.");
        if (validationError is not null)
        {
            return Result<RentalCalculationResponse>.Failure(validationError);
        }

        var tool = await _toolRepository.GetByIdWithDetailsAsync(toolId, cancellationToken);
        if (tool is null || !tool.IsActive)
        {
            return Result<RentalCalculationResponse>.NotFound($"Tool with ID {toolId} not found.");
        }

        var rentalPeriod = request.EndDateTime - request.StartDateTime;
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

    public async Task<Result<ToolDto>> CreateToolAsync(CreateToolRequest request, CancellationToken cancellationToken = default)
    {
        var validationError = RequestValidatorRunner.Validate(_createToolRequestValidator, request, "Tool request is required.");
        if (validationError is not null)
        {
            return Result<ToolDto>.Failure(validationError);
        }

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
        {
            return Result<ToolDto>.NotFound($"Category with ID {request.CategoryId} not found.");
        }

        var utcNow = DateTime.UtcNow;
        var tool = new Tool
        {
            CategoryId = request.CategoryId,
            Category = category,
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            HourlyRate = request.HourlyRate,
            DailyRate = request.DailyRate,
            WeeklyRate = request.WeeklyRate,
            SpecialNotes = TrimOptional(request.SpecialNotes),
            DepositRequired = request.DepositRequired,
            DepositAmount = request.DepositRequired ? request.DepositAmount : null,
            IsActive = true,
            OverallRating = null,
            ReviewCount = 0,
            CreatedDate = utcNow,
            UpdatedDate = utcNow
        };

        tool.Images.Add(new ToolImage
        {
            Tool = tool,
            ImageUrl = request.ImageUrl.Trim(),
            DisplayOrder = 1,
            UploadedDate = utcNow
        });

        await _toolRepository.AddAsync(tool, cancellationToken);
        foreach (var image in tool.Images)
        {
            image.ToolId = tool.Id;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ToolDto>.Success(MapToolDetail(tool));
    }

    public async Task<Result<ToolDto>> UpdateToolAsync(int id, UpdateToolRequest request, CancellationToken cancellationToken = default)
    {
        var validationError = RequestValidatorRunner.Validate(_updateToolRequestValidator, request, "Tool request is required.");
        if (validationError is not null)
        {
            return Result<ToolDto>.Failure(validationError);
        }

        var tool = await _toolRepository.GetByIdAsync(id, cancellationToken);
        if (tool is null)
        {
            return Result<ToolDto>.NotFound($"Tool with ID {id} not found.");
        }

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
        {
            return Result<ToolDto>.NotFound($"Category with ID {request.CategoryId} not found.");
        }

        tool.CategoryId = request.CategoryId;
        tool.Category = category;
        tool.Name = request.Name.Trim();
        tool.Description = request.Description.Trim();
        tool.HourlyRate = request.HourlyRate;
        tool.DailyRate = request.DailyRate;
        tool.WeeklyRate = request.WeeklyRate;
        tool.SpecialNotes = TrimOptional(request.SpecialNotes);
        tool.DepositRequired = request.DepositRequired;
        tool.DepositAmount = request.DepositRequired ? request.DepositAmount : null;

        await _toolRepository.UpdateAsync(tool, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var refreshedTool = await _toolRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        return Result<ToolDto>.Success(MapToolDetail(refreshedTool ?? tool));
    }

    public async Task<Result<bool>> SetToolStatusAsync(int id, bool isActive, CancellationToken cancellationToken = default)
    {
        var tool = await _toolRepository.GetByIdAsync(id, cancellationToken);
        if (tool is null)
        {
            return Result<bool>.NotFound($"Tool with ID {id} not found.");
        }

        tool.IsActive = isActive;

        await _toolRepository.UpdateAsync(tool, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
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

    private static string? ValidatePriceRange(decimal? minPrice, decimal? maxPrice)
    {
        if (minPrice is < 0m || maxPrice is < 0m)
        {
            return "Price values must be greater than or equal to 0.";
        }

        if (minPrice.HasValue && maxPrice.HasValue && minPrice.Value > maxPrice.Value)
        {
            return "Minimum price must be less than or equal to maximum price.";
        }

        return null;
    }

    private static string? ValidateAdminToolQuery(AdminToolQueryRequest? request)
    {
        if (request is null)
        {
            return "Admin tool query request is required.";
        }

        var pagingError = ValidatePaging(request.Page, request.PageSize);
        if (pagingError is not null)
        {
            return pagingError;
        }

        if (request.CategoryId is <= 0)
        {
            return "Category ID must be greater than 0.";
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm) && request.SearchTerm.Trim().Length > 200)
        {
            return "Search term must be 200 characters or fewer.";
        }

        return null;
    }

    private static bool? GetAdminActiveFilter(string? status, out string? validationError)
    {
        validationError = null;

        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        return status.Trim().ToLowerInvariant() switch
        {
            "all" => null,
            "active" => true,
            "inactive" => false,
            _ => SetInvalidStatus(out validationError)
        };
    }

    private static bool? SetInvalidStatus(out string validationError)
    {
        validationError = "Invalid status value. Supported values are all, active, and inactive.";
        return null;
    }

    private static IEnumerable<Tool> ApplyAdminFilters(
        IEnumerable<Tool> tools,
        string? searchTerm,
        int? categoryId,
        bool? activeFilter)
    {
        var filteredTools = tools;

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalizedSearchTerm = searchTerm.Trim();
            filteredTools = filteredTools.Where(tool =>
                ContainsIgnoreCase(tool.Name, normalizedSearchTerm) ||
                ContainsIgnoreCase(tool.Description, normalizedSearchTerm) ||
                ContainsIgnoreCase(tool.Category.Name, normalizedSearchTerm));
        }

        if (categoryId.HasValue)
        {
            filteredTools = filteredTools.Where(tool => tool.CategoryId == categoryId.Value);
        }

        if (activeFilter.HasValue)
        {
            filteredTools = filteredTools.Where(tool => tool.IsActive == activeFilter.Value);
        }

        return filteredTools;
    }

    private static IEnumerable<Tool> ApplyAdminSort(IEnumerable<Tool> tools, string? sortBy, out string? validationError)
    {
        validationError = null;

        var normalizedSort = NormalizeSort(sortBy);
        if (normalizedSort is not ("name" or "name_desc" or "category" or "category_desc" or "status" or "status_desc" or "updated" or "updated_desc" or "price" or "price_asc" or "starting_price" or "price_desc" or "starting_price_desc" or "rating" or "rating_desc" or "rating_asc"))
        {
            validationError = "Invalid sortBy value. Supported values are name, category, status, updated, price, price_desc, rating, and rating_asc.";
            return tools;
        }

        return normalizedSort switch
        {
            "category" => tools
                .OrderBy(tool => tool.Category.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(tool => tool.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(tool => tool.Id),
            "category_desc" => tools
                .OrderByDescending(tool => tool.Category.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(tool => tool.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(tool => tool.Id),
            "status" => tools
                .OrderByDescending(tool => tool.IsActive)
                .ThenBy(tool => tool.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(tool => tool.Id),
            "status_desc" => tools
                .OrderBy(tool => tool.IsActive)
                .ThenBy(tool => tool.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(tool => tool.Id),
            "updated" => tools
                .OrderBy(tool => tool.UpdatedDate)
                .ThenBy(tool => tool.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(tool => tool.Id),
            "updated_desc" => tools
                .OrderByDescending(tool => tool.UpdatedDate)
                .ThenBy(tool => tool.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(tool => tool.Id),
            _ => ApplySort(tools, sortBy, out validationError)
        };
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

    private static IReadOnlyList<Tool> ApplyRatingMetrics(IEnumerable<Tool> tools)
    {
        var toolList = tools.ToList();

        foreach (var tool in toolList)
        {
            ApplyRatingMetrics(tool);
        }

        return toolList;
    }

    private static void ApplyRatingMetrics(Tool tool)
    {
        var approvedReviews = tool.Reviews
            .Where(review => review.Status == ReviewStatus.Approved)
            .ToList();

        tool.ReviewCount = approvedReviews.Count;
        tool.OverallRating = approvedReviews.Count < MinimumReviewsRequiredForRating
            ? null
            : CalculateOverallRating(approvedReviews);
    }

    private static decimal CalculateOverallRating(IReadOnlyCollection<Review> approvedReviews)
    {
        var averageEquipment = approvedReviews.Average(review => (decimal)review.EquipmentRating);
        var averageCustomerService = approvedReviews.Average(review => (decimal)review.CustomerServiceRating);
        var averageTechnicalSupport = approvedReviews.Average(review => (decimal)review.TechnicalSupportRating);
        var averageAfterSales = approvedReviews.Average(review => (decimal)review.AfterSalesRating);
        var averageValueForMoney = approvedReviews.Average(review => (decimal)review.ValueForMoneyRating);

        return Math.Round(
            (averageEquipment + averageCustomerService + averageTechnicalSupport + averageAfterSales + averageValueForMoney) / 5m,
            2,
            MidpointRounding.AwayFromZero);
    }

    private static ToolSummaryDto MapSummary(Tool tool)
    {
        var (startingPrice, startingPriceUnit) = GetStartingPrice(tool);
        var (hasEnoughReviewsToRate, ratingMessage) = GetRatingDisplayState(tool);

        return new ToolSummaryDto(
            tool.Id,
            tool.Name,
            tool.Category.Name,
            startingPrice,
            startingPriceUnit,
            tool.DailyRate,
            tool.OverallRating,
            tool.ReviewCount,
            hasEnoughReviewsToRate,
            ratingMessage,
            GetThumbnailUrl(tool));
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

    private static AdminToolSummaryDto MapAdminSummary(Tool tool)
    {
        var (hasEnoughReviewsToRate, ratingMessage) = GetRatingDisplayState(tool);

        return new AdminToolSummaryDto(
            tool.Id,
            tool.CategoryId,
            tool.Category.Name,
            tool.Name,
            tool.HourlyRate,
            tool.DailyRate,
            tool.WeeklyRate,
            tool.IsActive,
            tool.OverallRating,
            tool.ReviewCount,
            hasEnoughReviewsToRate,
            ratingMessage,
            GetThumbnailUrl(tool),
            tool.UpdatedDate);
    }

    private static PagedList<AdminToolSummaryDto> CreatePagedAdminSummary(IEnumerable<Tool> tools, int page, int pageSize)
    {
        var toolList = tools.ToList();
        var items = toolList
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapAdminSummary)
            .ToList();

        return new PagedList<AdminToolSummaryDto>(items, page, pageSize, toolList.Count);
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

        return Result<ToolDto>.Success(MapToolDetail(tool));
    }

    private static ToolDto MapToolDetail(Tool tool)
    {
        ApplyRatingMetrics(tool);
        var (hasEnoughReviewsToRate, ratingMessage) = GetRatingDisplayState(tool);
        return new ToolDto(
            tool.Id,
            tool.CategoryId,
            tool.Category?.Name ?? string.Empty,
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
            hasEnoughReviewsToRate,
            ratingMessage,
            tool.Images
                .OrderBy(image => image.DisplayOrder)
                .ThenBy(image => image.Id)
                .Select(image => new ToolImageDto(image.Id, image.ImageUrl, image.DisplayOrder))
                .ToList(),
            tool.CreatedDate,
            tool.UpdatedDate);
    }

    private static (bool HasEnoughReviewsToRate, string? RatingMessage) GetRatingDisplayState(Tool tool)
    {
        var hasEnoughReviewsToRate = tool.ReviewCount >= MinimumReviewsRequiredForRating;

        return (
            hasEnoughReviewsToRate,
            hasEnoughReviewsToRate ? null : NotEnoughReviewsMessage);
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

    private static string? GetThumbnailUrl(Tool tool)
    {
        return tool.Images
            .OrderBy(image => image.DisplayOrder)
            .ThenBy(image => image.Id)
            .Select(image => image.ImageUrl)
            .FirstOrDefault();
    }

    private sealed record RentalCombination(int Weeks, int Days, int Hours, int CoveredHours, decimal TotalCost)
    {
        public int UnitCount => Weeks + Days + Hours;
    }

    private static string? TrimOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
