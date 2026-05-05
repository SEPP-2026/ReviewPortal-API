using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Users;
using ReviewPortal.Application.Interfaces;
using FluentValidation;
using ReviewPortal.Domain.Entities;
using ReviewPortal.Domain.Enums;
using ReviewPortal.Domain.Interfaces;
using ReviewPortal.Application.Validators;
using ReviewPortal.Application.Validators.Users;
using System.Security.Cryptography;
using System.Text;

namespace ReviewPortal.Application.Services;

public class AuthService : IAuthService
{
    private static readonly TimeSpan PasswordResetTokenLifetime = TimeSpan.FromMinutes(30);
    private const string ForgotPasswordMessage = "If an account exists for that email, a reset token has been generated. Because email delivery is not configured, use the returned token to complete the reset.";

    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtProvider _jwtProvider;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<RegisterRequest> _registerRequestValidator;
    private readonly IValidator<LoginRequest> _loginRequestValidator;
    private readonly IValidator<ChangePasswordRequest> _changePasswordRequestValidator;
    private readonly IValidator<ForgotPasswordRequest> _forgotPasswordRequestValidator;
    private readonly IValidator<ResetPasswordRequest> _resetPasswordRequestValidator;

    public AuthService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IJwtProvider jwtProvider,
        IPasswordHasher passwordHasher,
        IValidator<RegisterRequest>? registerRequestValidator = null,
        IValidator<LoginRequest>? loginRequestValidator = null,
        IValidator<ChangePasswordRequest>? changePasswordRequestValidator = null,
        IValidator<ForgotPasswordRequest>? forgotPasswordRequestValidator = null,
        IValidator<ResetPasswordRequest>? resetPasswordRequestValidator = null)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _jwtProvider = jwtProvider;
        _passwordHasher = passwordHasher;
        _registerRequestValidator = registerRequestValidator ?? new RegisterRequestValidator();
        _loginRequestValidator = loginRequestValidator ?? new LoginRequestValidator();
        _changePasswordRequestValidator = changePasswordRequestValidator ?? new ChangePasswordRequestValidator();
        _forgotPasswordRequestValidator = forgotPasswordRequestValidator ?? new ForgotPasswordRequestValidator();
        _resetPasswordRequestValidator = resetPasswordRequestValidator ?? new ResetPasswordRequestValidator();
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var validationError = RequestValidatorRunner.Validate(_registerRequestValidator, request, "Registration request is required.");
        if (validationError is not null)
        {
            return Result<AuthResponse>.Failure(validationError);
        }

        var normalizedEmail = NormalizeEmail(request.Email);
        if (await _userRepository.ExistsByEmailAsync(normalizedEmail, cancellationToken))
        {
            return Result<AuthResponse>.Conflict("Email is already taken.");
        }

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = UserRole.Customer,
            CreatedDate = DateTime.UtcNow
        };

        var createdUser = await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var token = _jwtProvider.Generate(createdUser);

        return Result<AuthResponse>.Success(new AuthResponse(
            token.Value,
            createdUser.Name,
            createdUser.Email,
            createdUser.Role.ToString(),
            token.ExpiresAtUtc
        ));
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var validationError = RequestValidatorRunner.Validate(_loginRequestValidator, request, "Login request is required.");
        if (validationError is not null)
        {
            return Result<AuthResponse>.Failure(validationError);
        }

        var user = await _userRepository.GetByEmailAsync(NormalizeEmail(request.Email), cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Result<AuthResponse>.Unauthorized("Invalid email or password.");
        }

        var token = _jwtProvider.Generate(user);

        return Result<AuthResponse>.Success(new AuthResponse(
            token.Value,
            user.Name,
            user.Email,
            user.Role.ToString(),
            token.ExpiresAtUtc
        ));
    }

    public async Task<Result<UserDto>> GetCurrentUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result<UserDto>.NotFound("User not found.");
        }

        return Result<UserDto>.Success(new UserDto(
            user.Id,
            user.Name,
            user.Email,
            user.Role.ToString(),
            user.CreatedDate
        ));
    }

    public async Task<Result<PasswordActionResponse>> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var validationError = RequestValidatorRunner.Validate(_changePasswordRequestValidator, request, "Change password request is required.");
        if (validationError is not null)
        {
            return Result<PasswordActionResponse>.Failure(validationError);
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result<PasswordActionResponse>.NotFound("User not found.");
        }

        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return Result<PasswordActionResponse>.Unauthorized("Current password is incorrect.");
        }

        if (request.CurrentPassword == request.NewPassword)
        {
            return Result<PasswordActionResponse>.Failure("New password must be different from the current password.");
        }

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        ClearPasswordResetState(user);

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PasswordActionResponse>.Success(new PasswordActionResponse("Password changed successfully."));
    }

    public async Task<Result<ForgotPasswordResponse>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var validationError = RequestValidatorRunner.Validate(_forgotPasswordRequestValidator, request, "Forgot password request is required.");
        if (validationError is not null)
        {
            return Result<ForgotPasswordResponse>.Failure(validationError);
        }

        var normalizedEmail = NormalizeEmail(request.Email);
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (user is null)
        {
            return Result<ForgotPasswordResponse>.Success(new ForgotPasswordResponse(ForgotPasswordMessage, null, null));
        }

        var resetToken = GenerateResetToken();
        user.PasswordResetTokenHash = HashToken(resetToken);
        user.PasswordResetTokenExpiryUtc = DateTime.UtcNow.Add(PasswordResetTokenLifetime);

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ForgotPasswordResponse>.Success(new ForgotPasswordResponse(
            ForgotPasswordMessage,
            resetToken,
            user.PasswordResetTokenExpiryUtc));
    }

    public async Task<Result<PasswordActionResponse>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var validationError = RequestValidatorRunner.Validate(_resetPasswordRequestValidator, request, "Reset password request is required.");
        if (validationError is not null)
        {
            return Result<PasswordActionResponse>.Failure(validationError);
        }

        var user = await _userRepository.GetByEmailAsync(NormalizeEmail(request.Email), cancellationToken);
        if (user is null || !IsValidResetToken(user, request.ResetToken))
        {
            return Result<PasswordActionResponse>.Failure("Reset token is invalid or has expired.");
        }

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        ClearPasswordResetState(user);

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PasswordActionResponse>.Success(new PasswordActionResponse("Password has been reset successfully."));
    }

    private static string GenerateResetToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }

    private static bool IsValidResetToken(User user, string resetToken)
    {
        if (string.IsNullOrWhiteSpace(user.PasswordResetTokenHash) || !user.PasswordResetTokenExpiryUtc.HasValue)
        {
            return false;
        }

        if (user.PasswordResetTokenExpiryUtc.Value < DateTime.UtcNow)
        {
            return false;
        }

        var providedTokenHashBytes = Convert.FromBase64String(HashToken(resetToken));
        var storedTokenHashBytes = Convert.FromBase64String(user.PasswordResetTokenHash);

        return CryptographicOperations.FixedTimeEquals(providedTokenHashBytes, storedTokenHashBytes);
    }

    private static void ClearPasswordResetState(User user)
    {
        user.PasswordResetTokenHash = null;
        user.PasswordResetTokenExpiryUtc = null;
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
