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
    public async Task AdminToolMutationFlow_CreateUpdateDeactivate_ReturnsExpectedResponses()
    {
        using var adminClient = await _factory.CreateAuthenticatedClientAsync(
            ReviewPortalApiFactory.AdminEmail,
            ReviewPortalApiFactory.AdminPassword);
        using var publicClient = _factory.CreateHttpsClient();

        var createResponse = await adminClient.PostAsJsonAsync(
            "/api/admin/tools",
            CreateToolRequest(_factory.BreakingCategoryId, "Admin Flow Plate Compactor"));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ToolDto>();
        Assert.NotNull(created);
        Assert.Equal("Admin Flow Plate Compactor", created!.Name);
        Assert.True(created.IsActive);
        Assert.Equal(_factory.BreakingCategoryId, created.CategoryId);
        Assert.Single(created.Images);
        Assert.Equal("/uploads/tools/admin-flow-plate-compactor.jpg", created.Images[0].ImageUrl);

        var updateResponse = await adminClient.PutAsJsonAsync(
            $"/api/admin/tools/{created.Id}",
            new UpdateToolRequest(
                _factory.AccessCategoryId,
                "Admin Flow Plate Compactor Updated",
                "Updated compacting equipment description for the admin integration flow.",
                13m,
                65m,
                260m,
                "Updated safety note.",
                true,
                120m));

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<ToolDto>();
        Assert.NotNull(updated);
        Assert.Equal("Admin Flow Plate Compactor Updated", updated!.Name);
        Assert.Equal(_factory.AccessCategoryId, updated.CategoryId);
        Assert.True(updated.DepositRequired);
        Assert.Equal(120m, updated.DepositAmount);

        var deactivateResponse = await adminClient.PatchAsJsonAsync(
            $"/api/admin/tools/{created.Id}/status",
            new SetToolStatusRequest(false));

        Assert.Equal(HttpStatusCode.OK, deactivateResponse.StatusCode);
        var statusUpdated = await deactivateResponse.Content.ReadFromJsonAsync<bool>();
        Assert.True(statusUpdated);

        var adminDetail = await adminClient.GetFromJsonAsync<ToolDto>($"/api/admin/tools/{created.Id}");
        Assert.NotNull(adminDetail);
        Assert.False(adminDetail!.IsActive);

        var inactiveList = await adminClient.GetFromJsonAsync<PagedList<AdminToolSummaryDto>>(
            "/api/admin/tools?status=inactive&searchTerm=compactor&page=1&pageSize=10&sortBy=name");

        Assert.NotNull(inactiveList);
        Assert.Contains(inactiveList!.Items, tool => tool.Id == created.Id && !tool.IsActive);

        var publicDetail = await publicClient.GetAsync($"/api/tools/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, publicDetail.StatusCode);
    }

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
    public async Task AdminToolEndpoints_WhenRequestInvalidOrToolMissing_ReturnExpectedStatusCodes()
    {
        using var adminClient = await _factory.CreateAuthenticatedClientAsync(
            ReviewPortalApiFactory.AdminEmail,
            ReviewPortalApiFactory.AdminPassword);

        var invalidCreate = await adminClient.PostAsJsonAsync(
            "/api/admin/tools",
            new CreateToolRequest(
                0,
                string.Empty,
                string.Empty,
                0m,
                0m,
                0m,
                null,
                true,
                null,
                string.Empty));
        Assert.Equal(HttpStatusCode.BadRequest, invalidCreate.StatusCode);

        var missingDetail = await adminClient.GetAsync("/api/admin/tools/999999");
        Assert.Equal(HttpStatusCode.NotFound, missingDetail.StatusCode);

        var missingUpdate = await adminClient.PutAsJsonAsync(
            "/api/admin/tools/999999",
            new UpdateToolRequest(
                _factory.AccessCategoryId,
                "Missing Tool Update",
                "Valid update body for a missing tool.",
                9m,
                45m,
                180m,
                null,
                false,
                null));
        Assert.Equal(HttpStatusCode.NotFound, missingUpdate.StatusCode);

        var missingStatus = await adminClient.PatchAsJsonAsync(
            "/api/admin/tools/999999/status",
            new SetToolStatusRequest(false));
        Assert.Equal(HttpStatusCode.NotFound, missingStatus.StatusCode);
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

    private static CreateToolRequest CreateToolRequest(int categoryId, string name)
    {
        var slug = name
            .ToLowerInvariant()
            .Replace(" ", "-");

        return new CreateToolRequest(
            categoryId,
            name,
            "A valid admin-created equipment item for integration coverage.",
            11m,
            55m,
            220m,
            "Includes operator guidance.",
            false,
            null,
            $"/uploads/tools/{slug}.jpg");
    }
}
