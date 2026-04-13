using Microsoft.AspNetCore.Mvc;
using ReviewPortal.API.Extensions;
using ReviewPortal.Application.DTOs.Reviews;
using ReviewPortal.Application.Interfaces;

namespace ReviewPortal.API.Controllers;

[ApiController]
[Route("api/reviews/{reviewId:int}/comments")]
public class ReviewCommentsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewCommentsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet]
    public async Task<IActionResult> GetApproved(int reviewId, CancellationToken cancellationToken)
    {
        var result = await _reviewService.GetApprovedCommentsAsync(reviewId, cancellationToken);
        return this.ToActionResult(result, Ok);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        int reviewId,
        [FromBody] CreateCommentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _reviewService.AddCommentAsync(reviewId, request, cancellationToken);
        return this.ToActionResult(
            result,
            comment => Created($"/api/reviews/{reviewId}/comments/{comment.Id}", comment));
    }
}
