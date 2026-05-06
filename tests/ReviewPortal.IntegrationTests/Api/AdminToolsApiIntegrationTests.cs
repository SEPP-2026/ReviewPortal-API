using System.Net;
using System.Net.Http.Json;
using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Tools;

namespace ReviewPortal.IntegrationTests.Api;

[Collection(ReviewPortalApiCollection.CollectionName)]
public class AdminToolsApiIntegrationTests : IAsyncLifetime
{
    private readonly ReviewPortalApiFactory _factory;

    public AdminToolsApiIntegrationTests(ReviewPortalApiFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync()
    {
        return _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task AdminToolReadEndpoints_ReturnInactiveToolsAndEditDetail()
    {
        using var adminClient = await _factory.CreateAuthenticatedClientAsync(
            ReviewPortalApiFactory.AdminEmail,
            ReviewPortalApiFactory.AdminPassword);
        using var publicClient = _factory.CreateHttpsClient();

        var list = await adminClient.GetFromJsonAsync<PagedList<AdminToolSummaryDto>>(
            $"/api/admin/tools?status=inactive&categoryId={_factory.AccessCategoryId}&searchTerm=retired&page=1&pageSize=10&sortBy=name");

        Assert.NotNull(list);
        Assert.Equal(1, list!.TotalCount);
        var inactiveTool = Assert.Single(list.Items);
        Assert.Equal(_factory.RetiredLadderToolId, inactiveTool.Id);
        Assert.Equal("Retired Ladder", inactiveTool.Name);
        Assert.False(inactiveTool.IsActive);
        Assert.Equal(_factory.AccessCategoryId, inactiveTool.CategoryId);

        var adminDetail = await adminClient.GetFromJsonAsync<ToolDto>($"/api/admin/tools/{_factory.RetiredLadderToolId}");
        Assert.NotNull(adminDetail);
        Assert.Equal("Retired Ladder", adminDetail!.Name);
        Assert.False(adminDetail.IsActive);

        var publicDetail = await publicClient.GetAsync($"/api/tools/{_factory.RetiredLadderToolId}");
        Assert.Equal(HttpStatusCode.NotFound, publicDetail.StatusCode);
    }

    [Fact]
    public async Task AdminToolReadEndpoints_WhenCustomerTokenIsUsed_ReturnForbidden()
    {
        using var customerClient = await _factory.CreateAuthenticatedClientAsync(
            ReviewPortalApiFactory.CustomerEmail,
            ReviewPortalApiFactory.CustomerPassword);

        var listResponse = await customerClient.GetAsync("/api/admin/tools");
        var detailResponse = await customerClient.GetAsync($"/api/admin/tools/{_factory.RetiredLadderToolId}");

        Assert.Equal(HttpStatusCode.Forbidden, listResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, detailResponse.StatusCode);
    }
}
