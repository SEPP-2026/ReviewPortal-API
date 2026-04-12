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
    private readonly IReviewService _reviewService;

    public ToolReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
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
