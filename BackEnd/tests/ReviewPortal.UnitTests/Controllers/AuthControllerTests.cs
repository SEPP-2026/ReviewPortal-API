using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReviewPortal.API.Controllers;
using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Users;
using ReviewPortal.Application.Interfaces;

namespace ReviewPortal.UnitTests.Controllers;

public class AuthControllerTests
{
    [Fact]
    public async Task Register_ReturnsAuthResponse()
    {
        var request = new RegisterRequest("Jane Doe", "jane@example.com", "Secure123");
        var response = new AuthResponse("token", "Jane Doe", "jane@example.com", "Customer", DateTime.UtcNow.AddHours(1));
        var authService = new FakeAuthService
        {
            RegisterResult = Result<AuthResponse>.Success(response)
        };
        var controller = new AuthController(authService);

        var result = await controller.Register(request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(response, okResult.Value);
        Assert.Equal(request, authService.LastRegisterRequest);
    }

    [Fact]
    public async Task ChangePassword_WhenUserIdClaimIsMissing_ReturnsUnauthorizedProblem()
    {
        var controller = new AuthController(new FakeAuthService())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            }
        };

        var result = await controller.ChangePassword(new ChangePasswordRequest("Current123", "NewSecure123"), CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemResult.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_WithAuthenticatedUser_PassesUserIdToService()
    {
        var request = new ChangePasswordRequest("Current123", "NewSecure123");
        var response = new PasswordActionResponse("Password changed successfully.");
        var authService = new FakeAuthService
        {
            ChangePasswordResult = Result<PasswordActionResponse>.Success(response)
        };
        var controller = new AuthController(authService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.NameIdentifier, "42")
                    ], "TestAuth"))
                }
            }
        };

        var result = await controller.ChangePassword(request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(response, okResult.Value);
        Assert.Equal(42, authService.LastChangePasswordUserId);
        Assert.Equal(request, authService.LastChangePasswordRequest);
    }

    [Fact]
    public async Task ForgotPassword_ReturnsResetTokenResponse()
    {
        var request = new ForgotPasswordRequest("jane@example.com");
        var response = new ForgotPasswordResponse("Reset requested.", "token-value", DateTime.UtcNow.AddMinutes(30));
        var authService = new FakeAuthService
        {
            ForgotPasswordResult = Result<ForgotPasswordResponse>.Success(response)
        };
        var controller = new AuthController(authService);

        var result = await controller.ForgotPassword(request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(response, okResult.Value);
        Assert.Equal(request, authService.LastForgotPasswordRequest);
    }

    [Fact]
    public async Task ResetPassword_WhenTokenIsInvalid_ReturnsBadRequestProblem()
    {
        var request = new ResetPasswordRequest("jane@example.com", "bad-token", "NewSecure123");
        var authService = new FakeAuthService
        {
            ResetPasswordResult = Result<PasswordActionResponse>.Failure("Reset token is invalid or has expired.")
        };
        var controller = new AuthController(authService);

        var result = await controller.ResetPassword(request, CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problemResult.StatusCode);
        Assert.Equal(request, authService.LastResetPasswordRequest);
    }

    private sealed class FakeAuthService : IAuthService
    {
        public RegisterRequest? LastRegisterRequest { get; private set; }

        public int? LastChangePasswordUserId { get; private set; }

        public ChangePasswordRequest? LastChangePasswordRequest { get; private set; }

        public ForgotPasswordRequest? LastForgotPasswordRequest { get; private set; }

        public ResetPasswordRequest? LastResetPasswordRequest { get; private set; }

        public Result<AuthResponse> RegisterResult { get; set; } =
            Result<AuthResponse>.Success(new AuthResponse("token", "Default User", "default@example.com", "Customer", DateTime.UtcNow.AddHours(1)));

        public Result<AuthResponse> LoginResult { get; set; } =
            Result<AuthResponse>.Success(new AuthResponse("token", "Default User", "default@example.com", "Customer", DateTime.UtcNow.AddHours(1)));

        public Result<UserDto> CurrentUserResult { get; set; } =
            Result<UserDto>.Success(new UserDto(1, "Default User", "default@example.com", "Customer", DateTime.UtcNow));

        public Result<PasswordActionResponse> ChangePasswordResult { get; set; } =
            Result<PasswordActionResponse>.Success(new PasswordActionResponse("Password changed successfully."));

        public Result<ForgotPasswordResponse> ForgotPasswordResult { get; set; } =
            Result<ForgotPasswordResponse>.Success(new ForgotPasswordResponse("Reset requested.", null, null));

        public Result<PasswordActionResponse> ResetPasswordResult { get; set; } =
            Result<PasswordActionResponse>.Success(new PasswordActionResponse("Password has been reset successfully."));

        public Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            LastRegisterRequest = request;
            return Task.FromResult(RegisterResult);
        }

        public Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(LoginResult);
        }

        public Task<Result<UserDto>> GetCurrentUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CurrentUserResult);
        }

        public Task<Result<PasswordActionResponse>> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
        {
            LastChangePasswordUserId = userId;
            LastChangePasswordRequest = request;
            return Task.FromResult(ChangePasswordResult);
        }

        public Task<Result<ForgotPasswordResponse>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
        {
            LastForgotPasswordRequest = request;
            return Task.FromResult(ForgotPasswordResult);
        }

        public Task<Result<PasswordActionResponse>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
        {
            LastResetPasswordRequest = request;
            return Task.FromResult(ResetPasswordResult);
        }
    }
}
