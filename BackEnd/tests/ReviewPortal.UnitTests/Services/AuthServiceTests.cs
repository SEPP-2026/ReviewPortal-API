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
