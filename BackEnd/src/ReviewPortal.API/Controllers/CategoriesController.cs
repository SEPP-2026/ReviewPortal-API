using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewPortal.Application.DTOs.Categories;
using ReviewPortal.Application.Interfaces;

namespace ReviewPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetAllCategoriesAsync(cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { Error = result.Error });
        }
        return Ok(result.Value);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetCategoryByIdAsync(id, cancellationToken);
        if (!result.IsSuccess)
        {
            return NotFound(new { Error = result.Error });
        }
        return Ok(result.Value);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _categoryService.CreateCategoryAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { Error = result.Error });
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Value?.Id }, result.Value);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _categoryService.UpdateCategoryAsync(id, request, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { Error = result.Error });
        }
        return Ok(result.Value);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.DeleteCategoryAsync(id, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { Error = result.Error });
        }
        return NoContent();
    }
}
