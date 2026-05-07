using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ReviewPortal.Application.DTOs.Users;

namespace ReviewPortal.IntegrationTests.Api;

[Collection(ReviewPortalApiCollection.CollectionName)]
public class AuthApiIntegrationTests : IAsyncLifetime
{
    private readonly ReviewPortalApiFactory _factory;

    public AuthApiIntegrationTests(ReviewPortalApiFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync()
    {
        return _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task RegisterLoginMeAndChangePassword_UseRealJwtPipeline()
    {
        using var client = _factory.CreateHttpsClient();
        var register = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest("New API User", "new.api.user@example.com", "Register123"));

        Assert.Equal(HttpStatusCode.OK, register.StatusCode);
        var registration = await register.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(registration);
        Assert.Equal("new.api.user@example.com", registration!.Email);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", registration.Token);
        var me = await client.GetFromJsonAsync<UserDto>("/api/auth/me");
        Assert.NotNull(me);
        Assert.Equal("New API User", me!.Name);
        Assert.Equal("Customer", me.Role);

        var changePassword = await client.PostAsJsonAsync(
            "/api/auth/change-password",
            new ChangePasswordRequest("Register123", "Changed123"));
        Assert.Equal(HttpStatusCode.OK, changePassword.StatusCode);

        client.DefaultRequestHeaders.Authorization = null;
        var oldLogin = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("new.api.user@example.com", "Register123"));
        Assert.Equal(HttpStatusCode.Unauthorized, oldLogin.StatusCode);

        var newLogin = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("new.api.user@example.com", "Changed123"));
        Assert.Equal(HttpStatusCode.OK, newLogin.StatusCode);
    }

    [Fact]
    public async Task ForgotAndResetPassword_WithReturnedToken_AllowsLoginWithNewPassword()
    {
        using var client = _factory.CreateHttpsClient();

        var forgot = await client.PostAsJsonAsync(
            "/api/auth/forgot-password",
            new ForgotPasswordRequest(ReviewPortalApiFactory.CustomerEmail));

        Assert.Equal(HttpStatusCode.OK, forgot.StatusCode);
        var forgotBody = await forgot.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
        Assert.NotNull(forgotBody);
        Assert.False(string.IsNullOrWhiteSpace(forgotBody!.ResetToken));
        Assert.NotNull(forgotBody.ResetTokenExpiresAtUtc);

        var reset = await client.PostAsJsonAsync(
            "/api/auth/reset-password",
            new ResetPasswordRequest(ReviewPortalApiFactory.CustomerEmail, forgotBody.ResetToken!, "Reset1234"));

        Assert.Equal(HttpStatusCode.OK, reset.StatusCode);

        var login = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(ReviewPortalApiFactory.CustomerEmail, "Reset1234"));
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }
}
