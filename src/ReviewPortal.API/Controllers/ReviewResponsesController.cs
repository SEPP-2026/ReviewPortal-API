using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReviewPortal.API.Extensions;
using ReviewPortal.Application.DTOs.Reviews;
using ReviewPortal.Application.Interfaces;

namespace ReviewPortal.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Moderator")]
[Route("api/reviews/{reviewId:int}/response")]
public class ReviewResponsesController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewResponsesController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        int reviewId,
        [FromBody] CreateCompanyResponseRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedUserId(out var staffUserId, out var unauthorizedResult))
        {
            return unauthorizedResult!;
        }

        var result = await _reviewService.AddCompanyResponseAsync(reviewId, request, staffUserId, cancellationToken);
        return this.ToActionResult(
            result,
            response => Created($"/api/reviews/{reviewId}/response", response));
    }

    [HttpPut]
    public async Task<IActionResult> Update(
        int reviewId,
        [FromBody] CreateCompanyResponseRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedUserId(out var staffUserId, out var unauthorizedResult))
        {
            return unauthorizedResult!;
        }

        var result = await _reviewService.UpdateCompanyResponseAsync(reviewId, request, staffUserId, cancellationToken);
        return this.ToActionResult(result, Ok);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(int reviewId, CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedUserId(out var staffUserId, out var unauthorizedResult))
        {
            return unauthorizedResult!;
        }

        var result = await _reviewService.DeleteCompanyResponseAsync(reviewId, staffUserId, cancellationToken);
        return this.ToActionResult(result, _ => NoContent());
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
