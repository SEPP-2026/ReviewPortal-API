using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Users;

namespace ReviewPortal.Application.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<Result<UserDto>> GetCurrentUserAsync(int userId, CancellationToken cancellationToken = default);

    Task<Result<PasswordActionResponse>> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);

    Task<Result<ForgotPasswordResponse>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);

    Task<Result<PasswordActionResponse>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
}
