using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReviewPortal.API.Controllers;
using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Categories;
using ReviewPortal.Application.DTOs.Tools;
using ReviewPortal.Application.Interfaces;

namespace ReviewPortal.UnitTests.Controllers;

public class CatalogueControllerTests
{
    [Fact]
    public async Task CategoriesGetAll_ReturnsHomepageCategories()
    {
        var categories = new[]
        {
            new CategoryDto(1, "Access & Lifting", "Safe access platforms.", "access.jpg", 2)
        };
        var categoryService = new FakeCategoryService
        {
            AllCategoriesResult = Result<IReadOnlyList<CategoryDto>>.Success(categories)
        };
        var controller = new CategoriesController(categoryService, new FakeToolService());

        var result = await controller.GetAll(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(categories, okResult.Value);
    }

    [Fact]
    public async Task CategoriesGetFeatured_ReturnsFeaturedCategoriesForHomepage()
    {
        var categories = new[]
        {
            new CategoryDto(2, "Breaking & Drilling", "Core drilling tools.", "breaking.jpg", 4)
        };
        var categoryService = new FakeCategoryService
        {
            FeaturedCategoriesResult = Result<IReadOnlyList<CategoryDto>>.Success(categories)
        };
        var controller = new CategoriesController(categoryService, new FakeToolService());

        var result = await controller.GetFeatured(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(categories, okResult.Value);
    }

    [Fact]
    public async Task CategoriesGetById_WhenCategoryDoesNotExist_ReturnsNotFoundProblem()
    {
        var categoryService = new FakeCategoryService
        {
            CategoryByIdResult = Result<CategoryDto>.NotFound("Category with ID 404 not found.")
        };
        var controller = new CategoriesController(categoryService, new FakeToolService());

        var result = await controller.GetById(404, CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemResult.StatusCode);
    }

    [Fact]
    public async Task CategoriesGetTools_ReturnsPagedToolsAndPassesSortParameters()
    {
        var pagedTools = CreatePagedTools("SDS Max Drill");
        var toolService = new FakeToolService
        {
            CategoryToolsResult = Result<PagedList<ToolSummaryDto>>.Success(pagedTools)
        };
        var controller = new CategoriesController(new FakeCategoryService(), toolService);

        var result = await controller.GetTools(6, page: 2, pageSize: 12, sortBy: "price", sortOrder: "desc", cancellationToken: CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(pagedTools, okResult.Value);
        Assert.Equal(6, toolService.LastCategoryId);
        Assert.Equal(2, toolService.LastCategoryPage);
        Assert.Equal(12, toolService.LastCategoryPageSize);
        Assert.Equal("price_desc", toolService.LastCategorySortBy);
    }

    [Fact]
    public async Task CategoriesGetTools_WhenPriceRangeProvided_UsesDailyRateFilter()
    {
        var pagedTools = CreatePagedTools("Concrete Saw");
        var toolService = new FakeToolService
        {
            FilteredCategoryToolsResult = Result<PagedList<ToolSummaryDto>>.Success(pagedTools)
        };
        var controller = new CategoriesController(new FakeCategoryService(), toolService);

        var result = await controller.GetTools(
            6,
            page: 1,
            pageSize: 12,
            sortBy: "price",
            sortOrder: "asc",
            minPrice: 40m,
            maxPrice: 80m,
            cancellationToken: CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(pagedTools, okResult.Value);
        Assert.Equal(6, toolService.LastFilterCategoryId);
        Assert.Equal(40m, toolService.LastMinPrice);
        Assert.Equal(80m, toolService.LastMaxPrice);
        Assert.Equal(1, toolService.LastFilterPage);
        Assert.Equal(12, toolService.LastFilterPageSize);
        Assert.Equal("price_asc", toolService.LastFilterSortBy);
        Assert.Null(toolService.LastCategoryId);
    }

    [Fact]
    public async Task CategoriesGetTools_WhenPriceRangeIsInvalid_ReturnsBadRequestProblem()
    {
        var toolService = new FakeToolService
        {
            FilteredCategoryToolsResult = Result<PagedList<ToolSummaryDto>>.Failure("Minimum price must be less than or equal to maximum price.")
        };
        var controller = new CategoriesController(new FakeCategoryService(), toolService);

        var result = await controller.GetTools(
            6,
            minPrice: 90m,
            maxPrice: 40m,
            cancellationToken: CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problemResult.StatusCode);
    }

    [Fact]
    public async Task ToolsSearch_ReturnsPagedSearchResults()
    {
        var pagedTools = CreatePagedTools("Petrol Pressure Washer");
        var toolService = new FakeToolService
        {
            SearchToolsResult = Result<PagedList<ToolSummaryDto>>.Success(pagedTools)
        };
        var controller = new ToolsController(toolService);

        var result = await controller.Search("washer", page: 1, pageSize: 12, cancellationToken: CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(pagedTools, okResult.Value);
        Assert.Equal("washer", toolService.LastSearchQuery);
        Assert.Equal(1, toolService.LastSearchPage);
        Assert.Equal(12, toolService.LastSearchPageSize);
    }

    [Fact]
    public async Task ToolsSearch_WhenQueryIsNull_PassesEmptyQueryToService()
    {
        var toolService = new FakeToolService
        {
            SearchToolsResult = Result<PagedList<ToolSummaryDto>>.Failure("Search query is required.")
        };
        var controller = new ToolsController(toolService);

        var result = await controller.Search(null, cancellationToken: CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problemResult.StatusCode);
        Assert.Equal(string.Empty, toolService.LastSearchQuery);
    }

    [Fact]
    public async Task ToolsGetById_ReturnsToolDetail()
    {
        var toolDetail = CreateToolDetail();
        var toolService = new FakeToolService
        {
            ToolByIdResult = Result<ToolDto>.Success(toolDetail)
        };
        var controller = new ToolsController(toolService);

        var result = await controller.GetById(11, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(toolDetail, okResult.Value);
        Assert.Equal(11, toolService.LastToolId);
    }

    [Fact]
    public async Task ToolsCalculateRentalCost_ReturnsCalculationBreakdown()
    {
        var request = new RentalCalculationRequest(
            new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 1, 2, 12, 0, 0, DateTimeKind.Utc));
        var response = new RentalCalculationResponse(
            "50L Dehumidifier",
            request.StartDateTime,
            request.EndDateTime,
            "1 day x 45.00/day + 3 hours x 8.00/hour = 69.00",
            69m);
        var toolService = new FakeToolService
        {
            RentalCalculationResult = Result<RentalCalculationResponse>.Success(response)
        };
        var controller = new ToolsController(toolService);

        var result = await controller.CalculateRentalCost(7, request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(response, okResult.Value);
        Assert.Equal(7, toolService.LastRentalToolId);
        Assert.Equal(request, toolService.LastRentalCalculationRequest);
    }

    [Fact]
    public async Task ToolsCalculateRentalCost_WhenDatesAreInvalid_ReturnsBadRequestProblem()
    {
        var request = new RentalCalculationRequest(
            new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc));
        var toolService = new FakeToolService
        {
            RentalCalculationResult = Result<RentalCalculationResponse>.Failure("End date/time must be after the start date/time.")
        };
        var controller = new ToolsController(toolService);

        var result = await controller.CalculateRentalCost(7, request, CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problemResult.StatusCode);
    }

    private static PagedList<ToolSummaryDto> CreatePagedTools(string toolName)
    {
        return new PagedList<ToolSummaryDto>(
            [new ToolSummaryDto(1, toolName, "Breaking & Drilling", 15m, "hour", 54m, 4m, 1, "tool.jpg")],
            1,
            12,
            1);
    }

    private static ToolDto CreateToolDetail()
    {
        return new ToolDto(
            11,
            6,
            "Breaking & Drilling",
            "SDS Max Drill",
            "Heavy-duty SDS Max rotary hammer.",
            15m,
            54m,
            180m,
            "Bits are hired separately.",
            false,
            null,
            true,
            4m,
            1,
            [new ToolImageDto(1, "sds-max-drill.jpg", 1)],
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc));
    }

    private sealed class FakeCategoryService : ICategoryService
    {
        public Result<IReadOnlyList<CategoryDto>> AllCategoriesResult { get; set; } =
            Result<IReadOnlyList<CategoryDto>>.Success([]);

        public Result<IReadOnlyList<CategoryDto>> FeaturedCategoriesResult { get; set; } =
            Result<IReadOnlyList<CategoryDto>>.Success([]);

        public Result<CategoryDto> CategoryByIdResult { get; set; } =
            Result<CategoryDto>.Success(new CategoryDto(1, "Access & Lifting", null, null, 0));

        public Task<Result<IReadOnlyList<CategoryDto>>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(AllCategoriesResult);
        }

        public Task<Result<IReadOnlyList<CategoryDto>>> GetFeaturedCategoriesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(FeaturedCategoriesResult);
        }

        public Task<Result<CategoryDto>> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CategoryByIdResult);
        }

        public Task<Result<CategoryDto>> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result<CategoryDto>.Failure("Not used by these tests."));
        }

        public Task<Result<CategoryDto>> UpdateCategoryAsync(int id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result<CategoryDto>.Failure("Not used by these tests."));
        }

        public Task<Result<bool>> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result<bool>.Failure("Not used by these tests."));
        }
    }

    private sealed class FakeToolService : IToolService
    {
        public int? LastCategoryId { get; private set; }

        public int? LastCategoryPage { get; private set; }

        public int? LastCategoryPageSize { get; private set; }

        public string? LastCategorySortBy { get; private set; }

        public int? LastFilterCategoryId { get; private set; }

        public decimal? LastMinPrice { get; private set; }

        public decimal? LastMaxPrice { get; private set; }

        public int? LastFilterPage { get; private set; }

        public int? LastFilterPageSize { get; private set; }

        public string? LastFilterSortBy { get; private set; }

        public string? LastSearchQuery { get; private set; }

        public int? LastSearchPage { get; private set; }

        public int? LastSearchPageSize { get; private set; }

        public int? LastToolId { get; private set; }

        public int? LastRentalToolId { get; private set; }

        public RentalCalculationRequest? LastRentalCalculationRequest { get; private set; }

        public Result<PagedList<ToolSummaryDto>> CategoryToolsResult { get; set; } =
            Result<PagedList<ToolSummaryDto>>.Success(CreatePagedTools("Default Tool"));

        public Result<PagedList<ToolSummaryDto>> SearchToolsResult { get; set; } =
            Result<PagedList<ToolSummaryDto>>.Success(CreatePagedTools("Default Tool"));

        public Result<PagedList<ToolSummaryDto>> FilteredCategoryToolsResult { get; set; } =
            Result<PagedList<ToolSummaryDto>>.Success(CreatePagedTools("Default Tool"));

        public Result<ToolDto> ToolByIdResult { get; set; } =
            Result<ToolDto>.Success(CreateToolDetail());

        public Result<RentalCalculationResponse> RentalCalculationResult { get; set; } =
            Result<RentalCalculationResponse>.Success(new RentalCalculationResponse("Default Tool", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), "1 hour x 1.00/hour = 1.00", 1m));

        public Task<Result<PagedList<ToolSummaryDto>>> GetToolsByCategoryAsync(int categoryId, int page, int pageSize, string? sortBy = null, CancellationToken cancellationToken = default)
        {
            LastCategoryId = categoryId;
            LastCategoryPage = page;
            LastCategoryPageSize = pageSize;
            LastCategorySortBy = sortBy;
            return Task.FromResult(CategoryToolsResult);
        }

        public Task<Result<ToolDto>> GetToolByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            LastToolId = id;
            return Task.FromResult(ToolByIdResult);
        }

        public Task<Result<PagedList<ToolSummaryDto>>> SearchToolsAsync(string query, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            LastSearchQuery = query;
            LastSearchPage = page;
            LastSearchPageSize = pageSize;
            return Task.FromResult(SearchToolsResult);
        }

        public Task<Result<PagedList<ToolSummaryDto>>> FilterByPriceRangeAsync(int categoryId, decimal? minPrice, decimal? maxPrice, int page, int pageSize, string? sortBy = null, CancellationToken cancellationToken = default)
        {
            LastFilterCategoryId = categoryId;
            LastMinPrice = minPrice;
            LastMaxPrice = maxPrice;
            LastFilterPage = page;
            LastFilterPageSize = pageSize;
            LastFilterSortBy = sortBy;
            return Task.FromResult(FilteredCategoryToolsResult);
        }

        public Task<Result<RentalCalculationResponse>> CalculateRentalCostAsync(int toolId, RentalCalculationRequest request, CancellationToken cancellationToken = default)
        {
            LastRentalToolId = toolId;
            LastRentalCalculationRequest = request;
            return Task.FromResult(RentalCalculationResult);
        }

        public Task<Result<ToolDto>> CreateToolAsync(CreateToolRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result<ToolDto>.Failure("Not used by these tests."));
        }

        public Task<Result<ToolDto>> UpdateToolAsync(int id, UpdateToolRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result<ToolDto>.Failure("Not used by these tests."));
        }

        public Task<Result<bool>> SetToolStatusAsync(int id, bool isActive, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result<bool>.Failure("Not used by these tests."));
        }
    }
}
