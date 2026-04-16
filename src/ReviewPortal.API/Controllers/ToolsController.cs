using Microsoft.AspNetCore.Mvc;
using ReviewPortal.API.Extensions;
using ReviewPortal.Application.DTOs.Tools;
using ReviewPortal.Application.Interfaces;

namespace ReviewPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ToolsController : ControllerBase
{
    private readonly IToolService _toolService;

    public ToolsController(IToolService toolService)
    {
        _toolService = toolService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery(Name = "q")] string? query,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken cancellationToken = default)
    {
        var result = await _toolService.SearchToolsAsync(query ?? string.Empty, page, pageSize, cancellationToken);
        return this.ToActionResult(result, Ok);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _toolService.GetToolByIdAsync(id, cancellationToken);
        return this.ToActionResult(result, Ok);
    }

    [HttpPost("{id:int}/rental-calculation")]
    public async Task<IActionResult> CalculateRentalCost(
        int id,
        [FromBody] RentalCalculationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _toolService.CalculateRentalCostAsync(id, request, cancellationToken);
        return this.ToActionResult(result, Ok);
    }
}
