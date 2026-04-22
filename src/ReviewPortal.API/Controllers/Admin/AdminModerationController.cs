using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewPortal.API.Extensions;
using ReviewPortal.Application.DTOs.Reviews;
using ReviewPortal.Application.Interfaces;

namespace ReviewPortal.API.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin,Moderator")]
[Route("api/admin/moderation")]
public class AdminModerationController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public AdminModerationController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _reviewService.GetPendingReviewsAsync(page, pageSize, cancellationToken);
        return this.ToActionResult(result, Ok);
    }

    [HttpPut("reviews/{id:int}")]
    public async Task<IActionResult> ModerateReview(
        int id,
        [FromBody] ModerateReviewRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _reviewService.ModerateReviewAsync(id, request, cancellationToken);
        return this.ToActionResult(result, value => Ok(value));
    }

    [HttpPut("comments/{id:int}")]
    public async Task<IActionResult> ModerateComment(
        int id,
        [FromBody] ModerateReviewRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _reviewService.ModerateCommentAsync(id, request, cancellationToken);
        return this.ToActionResult(result, value => Ok(value));
    }
}
