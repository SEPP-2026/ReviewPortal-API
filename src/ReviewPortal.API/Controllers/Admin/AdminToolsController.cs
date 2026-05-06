using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReviewPortal.API.Extensions;
using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Tools;
using ReviewPortal.Application.Interfaces;

namespace ReviewPortal.API.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/tools")]
public class AdminToolsController : ControllerBase
{
    private readonly IToolService _toolService;
    private readonly IImageService _imageService;

    public AdminToolsController(IToolService toolService, IImageService imageService)
    {
        _toolService = toolService;
        _imageService = imageService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] AdminToolQueryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _toolService.GetAdminToolsAsync(request, cancellationToken);
        return this.ToActionResult(result, Ok);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _toolService.GetAdminToolByIdAsync(id, cancellationToken);
        return this.ToActionResult(result, Ok);
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

    [HttpPost("{id:int}/images")]
    public async Task<IActionResult> UploadImage(
        int id,
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            var missingFileResult = Result<ToolImageDto>.Failure("Image file is required.");
            return this.ToActionResult(missingFileResult, image => Created($"/api/tools/{id}/images/{image.Id}", image));
        }

        await using var fileStream = file.OpenReadStream();
        var result = await _imageService.UploadImageAsync(id, fileStream, file.FileName, cancellationToken);

        return this.ToActionResult(result, image => Created($"/api/tools/{id}/images/{image.Id}", image));
    }

    [HttpDelete("{id:int}/images/{imageId:int}")]
    public async Task<IActionResult> DeleteImage(
        int id,
        int imageId,
        CancellationToken cancellationToken)
    {
        var result = await _imageService.DeleteImageAsync(id, imageId, cancellationToken);
        return this.ToActionResult(result, value => Ok(value));
    }
}
