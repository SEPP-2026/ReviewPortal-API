using System.Net;
using System.Net.Http.Json;
using ReviewPortal.Application.DTOs.Categories;
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
                "admin tool create",
                (client, factory) => client.PostAsJsonAsync("/api/admin/tools", CreateToolRequest(factory.BreakingCategoryId)))
        ];
        yield return
        [
            new AdminRouteRequest(
                "admin tool update",
                (client, factory) => client.PutAsJsonAsync("/api/admin/tools/1", CreateUpdateToolRequest(factory.BreakingCategoryId)))
        ];
        yield return
        [
            new AdminRouteRequest(
                "admin tool status",
                (client, _) => client.PatchAsJsonAsync("/api/admin/tools/1/status", new SetToolStatusRequest(false)))
        ];
        yield return
        [
            new AdminRouteRequest(
                "admin tool image upload",
                (client, _) => client.PostAsync("/api/admin/tools/1/images", CreateImageContent()))
        ];
        yield return
        [
            new AdminRouteRequest(
                "admin tool image delete",
                (client, _) => client.DeleteAsync("/api/admin/tools/1/images/1"))
        ];
        yield return
        [
            new AdminRouteRequest(
                "admin category create",
                (client, _) => client.PostAsJsonAsync(
                    "/api/admin/categories",
                    new CreateCategoryRequest("Admin Auth Category", "Authorization route coverage.", null)))
        ];
        yield return
        [
            new AdminRouteRequest(
                "admin category update",
                (client, _) => client.PutAsJsonAsync(
                    "/api/admin/categories/1",
                    new UpdateCategoryRequest("Admin Auth Category Updated", "Authorization route coverage.", null)))
        ];
        yield return
        [
            new AdminRouteRequest(
                "admin category delete",
                (client, _) => client.DeleteAsync("/api/admin/categories/1"))
        ];
        yield return
        [
            new AdminRouteRequest(
                "review moderation",
                (client, _) => client.PutAsJsonAsync("/api/admin/moderation/reviews/1", new ModerateReviewRequest(true, null)))
        ];
        yield return
        [
            new AdminRouteRequest(
                "comment moderation",
                (client, _) => client.PutAsJsonAsync("/api/admin/moderation/comments/1", new ModerateReviewRequest(true, null)))
        ];
        yield return
        [
            new AdminRouteRequest(
                "pending moderation",
                (client, _) => client.GetAsync("/api/admin/moderation/pending"))
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

    private static UpdateToolRequest CreateUpdateToolRequest(int categoryId)
    {
        return new UpdateToolRequest(
            categoryId,
            "Admin Coverage Drill Updated",
            "A valid update request body used to exercise admin authorization filters.",
            10m,
            50m,
            200m,
            null,
            false,
            null);
    }

    private static MultipartFormDataContent CreateImageContent()
    {
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent([0xFF, 0xD8, 0xFF, 0xD9]);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "auth-coverage.jpg");
        return content;
    }

    public sealed record AdminRouteRequest(
        string Name,
        Func<HttpClient, ReviewPortalApiFactory, Task<HttpResponseMessage>> SendAsync)
    {
        public override string ToString() => Name;
    }
}
