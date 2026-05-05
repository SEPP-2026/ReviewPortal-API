using Microsoft.AspNetCore.Mvc;
using ReviewPortal.API.Extensions;
using ReviewPortal.Application.Interfaces;

namespace ReviewPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly IToolService _toolService;

    public CategoriesController(ICategoryService categoryService, IToolService toolService)
    {
        _categoryService = categoryService;
        _toolService = toolService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetAllCategoriesAsync(cancellationToken);
        return this.ToActionResult(result, Ok);
    }

    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured(CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetFeaturedCategoriesAsync(cancellationToken);
        return this.ToActionResult(result, Ok);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetCategoryByIdAsync(id, cancellationToken);
        return this.ToActionResult(result, Ok);
    }

    [HttpGet("{id:int}/tools")]
    public async Task<IActionResult> GetTools(
        int id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        CancellationToken cancellationToken = default)
    {
        var effectiveSort = BuildSortValue(sortBy, sortOrder);
        var result = minPrice.HasValue || maxPrice.HasValue
            ? await _toolService.FilterByPriceRangeAsync(id, minPrice, maxPrice, page, pageSize, effectiveSort, cancellationToken)
            : await _toolService.GetToolsByCategoryAsync(id, page, pageSize, effectiveSort, cancellationToken);

        return this.ToActionResult(result, Ok);
    }

    private static string? BuildSortValue(string? sortBy, string? sortOrder)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(sortOrder))
        {
            return sortBy;
        }

        return $"{sortBy}_{sortOrder}";
    }
}
