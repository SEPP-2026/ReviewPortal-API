using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReviewPortal.API.Extensions;
using ReviewPortal.Application.DTOs.Users;
using ReviewPortal.Application.Interfaces;
using System.Security.Claims;

namespace ReviewPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);
        return this.ToActionResult(result, Ok);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        return this.ToActionResult(result, Ok);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedUserId(out var userId, out var unauthorizedResult))
        {
            return unauthorizedResult!;
        }

        var result = await _authService.GetCurrentUserAsync(userId, cancellationToken);
        return this.ToActionResult(result, Ok);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedUserId(out var userId, out var unauthorizedResult))
        {
            return unauthorizedResult!;
        }

        var result = await _authService.ChangePasswordAsync(userId, request, cancellationToken);
        return this.ToActionResult(result, Ok);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.ForgotPasswordAsync(request, cancellationToken);
        return this.ToActionResult(result, Ok);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.ResetPasswordAsync(request, cancellationToken);
        return this.ToActionResult(result, Ok);
    }

    private bool TryGetAuthenticatedUserId(out int userId, out IActionResult? unauthorizedResult)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (int.TryParse(userIdStr, out userId))
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
