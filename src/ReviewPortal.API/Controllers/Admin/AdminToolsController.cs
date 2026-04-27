using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewPortal.API.Extensions;
using ReviewPortal.Application.DTOs.Tools;
using ReviewPortal.Application.Interfaces;

namespace ReviewPortal.API.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/tools")]
public class AdminToolsController : ControllerBase
{
    private readonly IToolService _toolService;

    public AdminToolsController(IToolService toolService)
    {
        _toolService = toolService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateToolRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _toolService.CreateToolAsync(request, cancellationToken);
        return this.ToActionResult(result, tool => Created($"/api/tools/{tool.Id}", tool));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateToolRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _toolService.UpdateToolAsync(id, request, cancellationToken);
        return this.ToActionResult(result, value => Ok(value));
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> SetStatus(
        int id,
        [FromBody] SetToolStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _toolService.SetToolStatusAsync(id, request.IsActive, cancellationToken);
        return this.ToActionResult(result, value => Ok(value));
    }
}
