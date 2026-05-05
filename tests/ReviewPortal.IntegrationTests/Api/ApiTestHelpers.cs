using System.Net.Http.Headers;
using System.Net.Http.Json;
using ReviewPortal.Application.DTOs.Users;

namespace ReviewPortal.IntegrationTests.Api;

internal static class ApiTestHelpers
{
    public static async Task<HttpClient> CreateAuthenticatedClientAsync(
        this ReviewPortalApiFactory factory,
        string email,
        string password)
    {
        var client = factory.CreateHttpsClient();
        var token = await client.LoginForTokenAsync(email, password);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public static async Task<string> LoginForTokenAsync(this HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        response.EnsureSuccessStatusCode();

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        Assert.False(string.IsNullOrWhiteSpace(auth!.Token));

        return auth.Token;
    }
}
