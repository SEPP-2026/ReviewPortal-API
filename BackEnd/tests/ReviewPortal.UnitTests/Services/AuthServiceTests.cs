using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Users;
using ReviewPortal.Application.Services;
using ReviewPortal.Domain.Entities;
using ReviewPortal.Domain.Enums;
using ReviewPortal.UnitTests.TestDoubles;

namespace ReviewPortal.UnitTests.Services;

public class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_WithWeakPassword_ReturnsValidationFailure()
    {
        var service = CreateService();

        var result = await service.RegisterAsync(new RegisterRequest("Jane", "jane@example.com", "weak"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal("Password must be at least 8 characters long.", result.Error);
    }

    [Fact]
    public async Task RegisterAsync_WithValidRequest_CreatesCustomerAndReturnsToken()
    {
        var userRepository = new InMemoryUserRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(userRepository, unitOfWork);

        var result = await service.RegisterAsync(new RegisterRequest(" Jane Doe ", "Jane@Example.com ", "Secure123"));

        Assert.True(result.IsSuccess);
        Assert.Equal("Jane Doe", result.Value!.Name);
        Assert.Equal("jane@example.com", result.Value.Email);
        Assert.Equal("Customer", result.Value.Role);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);

        var createdUser = await userRepository.GetByEmailAsync("jane@example.com");
        Assert.NotNull(createdUser);
        Assert.Equal(UserRole.Customer, createdUser!.Role);
        Assert.Equal("hashed::Secure123", createdUser.PasswordHash);
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ReturnsConflict()
    {
        var service = CreateService(new InMemoryUserRepository(
        [
            new User
            {
                Id = 4,
                Name = "Existing User",
                Email = "existing@example.com",
                PasswordHash = "hashed::Secure123",
                Role = UserRole.Customer
            }
        ]));

        var result = await service.RegisterAsync(new RegisterRequest("New User", "Existing@Example.com", "Secure123"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.FailureType);
        Assert.Equal("Email is already taken.", result.Error);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ReturnsUnauthorized()
    {
        var service = CreateService(new InMemoryUserRepository(
        [
            new User
            {
                Id = 7,
                Name = "Chris",
                Email = "chris@example.com",
                PasswordHash = "hashed::Secure123",
                Role = UserRole.Customer
            }
        ]));

        var result = await service.LoginAsync(new LoginRequest("chris@example.com", "Wrong123"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unauthorized, result.FailureType);
        Assert.Equal("Invalid email or password.", result.Error);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsTokenAndUserDetails()
    {
        var service = CreateService(new InMemoryUserRepository(
        [
            new User
            {
                Id = 7,
                Name = "Chris",
                Email = "chris@example.com",
                PasswordHash = "hashed::Secure123",
                Role = UserRole.Customer
            }
        ]));

        var result = await service.LoginAsync(new LoginRequest(" chris@example.com ", "Secure123"));

        Assert.True(result.IsSuccess);
        Assert.Equal("token-for-7", result.Value!.Token);
        Assert.Equal("Chris", result.Value.Name);
        Assert.Equal("chris@example.com", result.Value.Email);
        Assert.Equal("Customer", result.Value.Role);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithIncorrectCurrentPassword_ReturnsUnauthorized()
    {
        var service = CreateService(new InMemoryUserRepository(
        [
            new User
            {
                Id = 9,
                Name = "Maya",
                Email = "maya@example.com",
                PasswordHash = "hashed::Secure123",
                Role = UserRole.Customer
            }
        ]));

        var result = await service.ChangePasswordAsync(9, new ChangePasswordRequest("Wrong123", "NewSecure123"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unauthorized, result.FailureType);
        Assert.Equal("Current password is incorrect.", result.Error);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithValidRequest_UpdatesPasswordAndClearsResetState()
    {
        var userRepository = new InMemoryUserRepository(
        [
            new User
            {
                Id = 9,
                Name = "Maya",
                Email = "maya@example.com",
                PasswordHash = "hashed::Secure123",
                PasswordResetTokenHash = "old-token-hash",
                PasswordResetTokenExpiryUtc = DateTime.UtcNow.AddMinutes(5),
                Role = UserRole.Customer
            }
        ]);
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(userRepository, unitOfWork);

        var result = await service.ChangePasswordAsync(9, new ChangePasswordRequest("Secure123", "NewSecure123"));

        Assert.True(result.IsSuccess);
        Assert.Equal("Password changed successfully.", result.Value!.Message);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);

        var updatedUser = await userRepository.GetByEmailAsync("maya@example.com");
        Assert.NotNull(updatedUser);
        Assert.Equal("hashed::NewSecure123", updatedUser!.PasswordHash);
        Assert.Null(updatedUser.PasswordResetTokenHash);
        Assert.Null(updatedUser.PasswordResetTokenExpiryUtc);
    }

    [Fact]
    public async Task ForgotPasswordAsync_WhenEmailIsUnknown_ReturnsGenericSuccessWithoutToken()
    {
        var service = CreateService();

        var result = await service.ForgotPasswordAsync(new ForgotPasswordRequest("unknown@example.com"));

        Assert.True(result.IsSuccess);
        Assert.Equal("If an account exists for that email, a reset token has been generated. Because email delivery is not configured, use the returned token to complete the reset.", result.Value!.Message);
        Assert.Null(result.Value.ResetToken);
        Assert.Null(result.Value.ResetTokenExpiresAtUtc);
    }

    [Fact]
    public async Task ForgotPasswordAsync_WhenEmailExists_ReturnsResetTokenAndStoresHash()
    {
        var userRepository = new InMemoryUserRepository(
        [
            new User
            {
                Id = 12,
                Name = "Nina",
                Email = "nina@example.com",
                PasswordHash = "hashed::Secure123",
                Role = UserRole.Customer
            }
        ]);
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(userRepository, unitOfWork);

        var result = await service.ForgotPasswordAsync(new ForgotPasswordRequest("nina@example.com"));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value!.ResetToken);
        Assert.NotNull(result.Value.ResetTokenExpiresAtUtc);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);

        var updatedUser = await userRepository.GetByEmailAsync("nina@example.com");
        Assert.NotNull(updatedUser);
        Assert.NotNull(updatedUser!.PasswordResetTokenHash);
        Assert.True(updatedUser.PasswordResetTokenExpiryUtc > DateTime.UtcNow);
        Assert.NotEqual(result.Value.ResetToken, updatedUser.PasswordResetTokenHash);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithExpiredToken_ReturnsValidationFailure()
    {
        var userRepository = new InMemoryUserRepository(
        [
            new User
            {
                Id = 13,
                Name = "Ava",
                Email = "ava@example.com",
                PasswordHash = "hashed::Secure123",
                PasswordResetTokenHash = "expired-token-hash",
                PasswordResetTokenExpiryUtc = DateTime.UtcNow.AddMinutes(-1),
                Role = UserRole.Customer
            }
        ]);
        var service = CreateService(userRepository);

        var result = await service.ResetPasswordAsync(new ResetPasswordRequest("ava@example.com", "expired-token", "FreshPass123"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal("Reset token is invalid or has expired.", result.Error);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithValidToken_UpdatesPasswordAndClearsResetState()
    {
        var userRepository = new InMemoryUserRepository(
        [
            new User
            {
                Id = 15,
                Name = "Luca",
                Email = "luca@example.com",
                PasswordHash = "hashed::Secure123",
                Role = UserRole.Customer
            }
        ]);
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(userRepository, unitOfWork);

        var forgotPasswordResult = await service.ForgotPasswordAsync(new ForgotPasswordRequest("luca@example.com"));
        var resetPasswordResult = await service.ResetPasswordAsync(new ResetPasswordRequest(
            "luca@example.com",
            forgotPasswordResult.Value!.ResetToken!,
            "FreshPass123"));

        Assert.True(resetPasswordResult.IsSuccess);
        Assert.Equal("Password has been reset successfully.", resetPasswordResult.Value!.Message);
        Assert.Equal(2, unitOfWork.SaveChangesCallCount);

        var updatedUser = await userRepository.GetByEmailAsync("luca@example.com");
        Assert.NotNull(updatedUser);
        Assert.Equal("hashed::FreshPass123", updatedUser!.PasswordHash);
        Assert.Null(updatedUser.PasswordResetTokenHash);
        Assert.Null(updatedUser.PasswordResetTokenExpiryUtc);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WhenUserIsMissing_ReturnsNotFound()
    {
        var service = CreateService();

        var result = await service.GetCurrentUserAsync(999);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.FailureType);
        Assert.Equal("User not found.", result.Error);
    }

    private static AuthService CreateService(
        InMemoryUserRepository? userRepository = null,
        FakeUnitOfWork? unitOfWork = null)
    {
        return new AuthService(
            userRepository ?? new InMemoryUserRepository(),
            unitOfWork ?? new FakeUnitOfWork(),
            new FakeJwtProvider(),
            new FakePasswordHasher());
    }
}
