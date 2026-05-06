using System.Net;
using System.Net.Http.Json;
using ReviewPortal.Application.DTOs.Categories;

namespace ReviewPortal.IntegrationTests.Api;

[Collection(ReviewPortalApiCollection.CollectionName)]
public class AdminCategoriesApiIntegrationTests : IAsyncLifetime
{
    private readonly ReviewPortalApiFactory _factory;

    public AdminCategoriesApiIntegrationTests(ReviewPortalApiFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync()
    {
        return _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task AdminCategoryFlow_CreateDuplicateUpdateDeleteAndConflict_ReturnsExpectedStatusCodes()
    {
        using var adminClient = await _factory.CreateAuthenticatedClientAsync(
            ReviewPortalApiFactory.AdminEmail,
            ReviewPortalApiFactory.AdminPassword);
        using var publicClient = _factory.CreateHttpsClient();

        var createResponse = await adminClient.PostAsJsonAsync(
            "/api/admin/categories",
            new CreateCategoryRequest(
                "Compaction & Ground Prep",
                "Compactors and ground preparation equipment.",
                "/images/categories/compaction.jpg"));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<CategoryDto>();
        Assert.NotNull(created);
        Assert.Equal("Compaction & Ground Prep", created!.Name);
        Assert.Equal(0, created.ToolCount);

        var duplicateResponse = await adminClient.PostAsJsonAsync(
            "/api/admin/categories",
            new CreateCategoryRequest(
                "Compaction & Ground Prep",
                "Duplicate category attempt.",
                "/images/categories/duplicate.jpg"));
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);

        var updateResponse = await adminClient.PutAsJsonAsync(
            $"/api/admin/categories/{created.Id}",
            new UpdateCategoryRequest(
                "Compaction Equipment",
                "Updated category description.",
                "/images/categories/compaction-updated.jpg"));

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<CategoryDto>();
        Assert.NotNull(updated);
        Assert.Equal("Compaction Equipment", updated!.Name);
        Assert.Equal("Updated category description.", updated.Description);

        var deleteAssignedCategoryResponse = await adminClient.DeleteAsync($"/api/admin/categories/{_factory.AccessCategoryId}");
        Assert.Equal(HttpStatusCode.Conflict, deleteAssignedCategoryResponse.StatusCode);

        var deleteEmptyCategoryResponse = await adminClient.DeleteAsync($"/api/admin/categories/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteEmptyCategoryResponse.StatusCode);

        var deletedPublicDetail = await publicClient.GetAsync($"/api/categories/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, deletedPublicDetail.StatusCode);
    }

    [Fact]
    public async Task AdminCategoryEndpoints_WhenRequestInvalidOrCategoryMissing_ReturnExpectedStatusCodes()
    {
        using var adminClient = await _factory.CreateAuthenticatedClientAsync(
            ReviewPortalApiFactory.AdminEmail,
            ReviewPortalApiFactory.AdminPassword);

        var invalidCreate = await adminClient.PostAsJsonAsync(
            "/api/admin/categories",
            new CreateCategoryRequest(string.Empty, null, null));
        Assert.Equal(HttpStatusCode.BadRequest, invalidCreate.StatusCode);

        var missingUpdate = await adminClient.PutAsJsonAsync(
            "/api/admin/categories/999999",
            new UpdateCategoryRequest(
                "Missing Category",
                "Valid update body for a missing category.",
                "/images/categories/missing.jpg"));
        Assert.Equal(HttpStatusCode.NotFound, missingUpdate.StatusCode);

        var missingDelete = await adminClient.DeleteAsync("/api/admin/categories/999999");
        Assert.Equal(HttpStatusCode.NotFound, missingDelete.StatusCode);
    }
}
