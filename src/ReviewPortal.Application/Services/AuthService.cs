using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Users;
using ReviewPortal.Application.Interfaces;
using ReviewPortal.Domain.Entities;
using ReviewPortal.Domain.Enums;
using ReviewPortal.Domain.Interfaces;
using System.Net.Mail;
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

    public AuthService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IJwtProvider jwtProvider,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _jwtProvider = jwtProvider;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var validationError = ValidateRegistrationRequest(request);
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
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Result<AuthResponse>.Failure("Email and password are required.");
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
        var validationError = ValidateChangePasswordRequest(request);
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
        var validationError = ValidateEmail(request.Email);
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
        var validationError = ValidateResetPasswordRequest(request);
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

    private static string? ValidateRegistrationRequest(RegisterRequest request)
    {
        return ValidateName(request.Name)
            ?? ValidateEmail(request.Email)
            ?? ValidatePassword(request.Password, "Password");
    }

    private static string? ValidateChangePasswordRequest(ChangePasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
        {
            return "Current password is required.";
        }

        return ValidatePassword(request.NewPassword, "New password");
    }

    private static string? ValidateResetPasswordRequest(ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ResetToken))
        {
            return "Reset token is required.";
        }

        return ValidateEmail(request.Email)
            ?? ValidatePassword(request.NewPassword, "New password");
    }

    private static string? ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Name is required.";
        }

        if (name.Trim().Length > 100)
        {
            return "Name must be 100 characters or fewer.";
        }

        return null;
    }

    private static string? ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return "Email is required.";
        }

        if (!IsValidEmail(email))
        {
            return "A valid email address is required.";
        }

        if (email.Trim().Length > 256)
        {
            return "Email must be 256 characters or fewer.";
        }

        return null;
    }

    private static string? ValidatePassword(string password, string fieldLabel)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return $"{fieldLabel} is required.";
        }

        if (password.Length < 8)
        {
            return $"{fieldLabel} must be at least 8 characters long.";
        }

        if (!password.Any(char.IsUpper))
        {
            return $"{fieldLabel} must contain at least one uppercase letter.";
        }

        if (!password.Any(char.IsDigit))
        {
            return $"{fieldLabel} must contain at least one number.";
        }

        return null;
    }

    private static bool IsValidEmail(string email)
    {
        return MailAddress.TryCreate(email.Trim(), out _);
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
