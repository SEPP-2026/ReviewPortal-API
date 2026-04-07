using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Users;
using ReviewPortal.Application.Interfaces;
using ReviewPortal.Domain.Entities;
using ReviewPortal.Domain.Enums;
using ReviewPortal.Domain.Interfaces;
using System.Net.Mail;

namespace ReviewPortal.Application.Services;

public class AuthService : IAuthService
{
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

    private static string? ValidateRegistrationRequest(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return "Name is required.";
        }

        if (request.Name.Trim().Length > 100)
        {
            return "Name must be 100 characters or fewer.";
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return "Email is required.";
        }

        if (!IsValidEmail(request.Email))
        {
            return "A valid email address is required.";
        }

        if (request.Email.Trim().Length > 256)
        {
            return "Email must be 256 characters or fewer.";
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return "Password is required.";
        }

        if (request.Password.Length < 8)
        {
            return "Password must be at least 8 characters long.";
        }

        if (!request.Password.Any(char.IsUpper))
        {
            return "Password must contain at least one uppercase letter.";
        }

        if (!request.Password.Any(char.IsDigit))
        {
            return "Password must contain at least one number.";
        }

        return null;
    }

    private static bool IsValidEmail(string email)
    {
        return MailAddress.TryCreate(email.Trim(), out _);
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
