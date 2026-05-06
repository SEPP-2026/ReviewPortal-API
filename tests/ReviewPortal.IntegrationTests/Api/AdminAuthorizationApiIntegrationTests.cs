using System.Net;
using System.Net.Http.Json;
using ReviewPortal.Application.DTOs.Reviews;
using ReviewPortal.Application.DTOs.Tools;

namespace ReviewPortal.IntegrationTests.Api;

[Collection(ReviewPortalApiCollection.CollectionName)]
public class AdminAuthorizationApiIntegrationTests : IAsyncLifetime
{
    private readonly ReviewPortalApiFactory _factory;

    public AdminAuthorizationApiIntegrationTests(ReviewPortalApiFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync()
    {
        return _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Theory]
    [MemberData(nameof(AdminRouteRequests))]
    public async Task AdminRoutes_WithoutToken_ReturnUnauthorized(AdminRouteRequest request)
    {
        using var client = _factory.CreateHttpsClient();

        var response = await request.SendAsync(client, _factory);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AdminRouteRequests))]
    public async Task AdminRoutes_WithCustomerToken_ReturnForbidden(AdminRouteRequest request)
    {
        using var client = await _factory.CreateAuthenticatedClientAsync(
            ReviewPortalApiFactory.CustomerEmail,
            ReviewPortalApiFactory.CustomerPassword);

        var response = await request.SendAsync(client, _factory);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    public static IEnumerable<object[]> AdminRouteRequests()
    {
        yield return
        [
            new AdminRouteRequest(
                "admin tool list",
                (client, _) => client.GetAsync("/api/admin/tools"))
        ];
        yield return
        [
            new AdminRouteRequest(
                "admin tool detail",
                (client, _) => client.GetAsync("/api/admin/tools/1"))
        ];
        yield return
        [
            new AdminRouteRequest(
                "admin tools",
                (client, factory) => client.PostAsJsonAsync("/api/admin/tools", CreateToolRequest(factory.BreakingCategoryId)))
        ];
        yield return
        [
            new AdminRouteRequest(
                "moderation",
                (client, _) => client.PutAsJsonAsync("/api/admin/moderation/reviews/1", new ModerateReviewRequest(true, null)))
        ];
        yield return
        [
            new AdminRouteRequest(
                "dashboard",
                (client, _) => client.GetAsync("/api/admin/dashboard/stats"))
        ];
    }

    private static CreateToolRequest CreateToolRequest(int categoryId)
    {
        return new CreateToolRequest(
            categoryId,
            "Admin Coverage Drill",
            "A valid request body used to exercise admin authorization filters.",
            9m,
            45m,
            180m,
            null,
            false,
            null,
            "/uploads/tools/admin-coverage-drill.jpg");
    }

    public sealed record AdminRouteRequest(
        string Name,
        Func<HttpClient, ReviewPortalApiFactory, Task<HttpResponseMessage>> SendAsync)
    {
        public override string ToString() => Name;
    }
}
