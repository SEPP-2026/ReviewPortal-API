using Microsoft.AspNetCore.Mvc;
using ReviewPortal.API.Extensions;
using ReviewPortal.Application.Interfaces;

namespace ReviewPortal.API.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private const int DefaultPageSize = 10;

    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    // GET /api/reviews?page=&pageSize=&sortBy=
    // Returns approved reviews across all tools, paged. Powers the public
    // reviews feed (which previously aggregated per-tool reviews client-side).
    [HttpGet]
    public async Task<IActionResult> GetApproved(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = DefaultPageSize,
        [FromQuery] string? sortBy = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _reviewService.GetAllApprovedReviewsAsync(page, pageSize, sortBy, cancellationToken);
        return this.ToActionResult(result, Ok);
    }
}
