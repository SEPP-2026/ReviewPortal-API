using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ReviewPortal.API.Extensions;
using ReviewPortal.Application.DTOs.Reviews;
using ReviewPortal.Application.Interfaces;

namespace ReviewPortal.API.Controllers;

[ApiController]
[Route("api/tools/{toolId:int}/reviews")]
public class ToolReviewsController : ControllerBase
{
    private const int DefaultPageSize = 10;

    private readonly IReviewService _reviewService;

    public ToolReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet]
    public async Task<IActionResult> GetApproved(
        int toolId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _reviewService.GetApprovedReviewsAsync(toolId, page, pageSize, cancellationToken);
        return this.ToActionResult(result, Ok);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        int toolId,
        [FromBody] CreateReviewRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _reviewService.CreateReviewAsync(
            toolId,
            request,
            GetAuthenticatedUserId(),
            cancellationToken);

        return this.ToActionResult(
            result,
            review => Created($"/api/tools/{toolId}/reviews/{review.Id}", review));
    }

    private int? GetAuthenticatedUserId()
    {
        var claimValue = User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claimValue, out var userId) ? userId : null;
    }
}
