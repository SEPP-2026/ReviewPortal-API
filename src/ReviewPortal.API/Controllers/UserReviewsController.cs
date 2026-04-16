using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReviewPortal.API.Extensions;
using ReviewPortal.Application.Interfaces;

namespace ReviewPortal.API.Controllers;

[ApiController]
[Authorize]
[Route("api/users/me/reviews")]
public class UserReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public UserReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMine(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthenticatedUserId(out var userId, out var unauthorizedResult))
        {
            return unauthorizedResult!;
        }

        var result = await _reviewService.GetUserReviewsAsync(userId, page, pageSize, cancellationToken);
        return this.ToActionResult(result, Ok);
    }

    private bool TryGetAuthenticatedUserId(out int userId, out IActionResult? unauthorizedResult)
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (int.TryParse(userIdValue, out userId))
        {
            unauthorizedResult = null;
            return true;
        }

        unauthorizedResult = Problem(
            title: "Unauthorized",
            detail: "The access token does not contain a valid user identifier.",
            statusCode: StatusCodes.Status401Unauthorized);
        return false;
    }
}
