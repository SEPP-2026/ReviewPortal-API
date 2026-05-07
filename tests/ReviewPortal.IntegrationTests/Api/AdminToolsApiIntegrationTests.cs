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

        using var createContent = CreateToolMultipartContent(
            _factory.BreakingCategoryId,
            "Admin Flow Plate Compactor");
        var createResponse = await adminClient.PostAsync("/api/admin/tools", createContent);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ToolDto>();
        Assert.NotNull(created);
        Assert.Equal("Admin Flow Plate Compactor", created!.Name);
        Assert.True(created.IsActive);
        Assert.Equal(_factory.BreakingCategoryId, created.CategoryId);
        Assert.Single(created.Images);
        Assert.StartsWith("/uploads/tools/first-", created.Images[0].ImageUrl);
        Assert.EndsWith(".jpg", created.Images[0].ImageUrl);

        var storedFileName = created.Images[0].ImageUrl.Split('/').Last();
        var storedFilePath = Path.Combine(_factory.UploadRootPath, storedFileName);
        Assert.True(File.Exists(storedFilePath), $"Expected first image file at {storedFilePath}.");

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

        using var invalidCreateContent = CreateToolMultipartContent(
            0,
            string.Empty,
            description: string.Empty,
            hourlyRate: "0",
            dailyRate: "0",
            weeklyRate: "0",
            depositRequired: "true",
            depositAmount: null);
        var invalidCreate = await adminClient.PostAsync("/api/admin/tools", invalidCreateContent);
        Assert.Equal(HttpStatusCode.BadRequest, invalidCreate.StatusCode);

        using var missingImageContent = CreateToolMultipartContent(
            _factory.BreakingCategoryId,
            "Missing First Image Tool",
            includeFile: false);
        var missingImageCreate = await adminClient.PostAsync("/api/admin/tools", missingImageContent);
        Assert.Equal(HttpStatusCode.BadRequest, missingImageCreate.StatusCode);

        using var invalidFileContent = CreateToolMultipartContent(
            _factory.BreakingCategoryId,
            "Invalid First Image Tool",
            fileName: "not-an-image.txt",
            fileBytes: [1, 2, 3],
            mediaType: "text/plain");
        var invalidFileCreate = await adminClient.PostAsync("/api/admin/tools", invalidFileContent);
        Assert.Equal(HttpStatusCode.BadRequest, invalidFileCreate.StatusCode);

        var oversizedBytes = new byte[(5 * 1024 * 1024) + 1];
        oversizedBytes[0] = 0xFF;
        oversizedBytes[1] = 0xD8;
        using var tooLargeContent = CreateToolMultipartContent(
            _factory.BreakingCategoryId,
            "Oversized First Image Tool",
            fileName: "too-large.jpg",
            fileBytes: oversizedBytes);
        var tooLargeCreate = await adminClient.PostAsync("/api/admin/tools", tooLargeContent);
        Assert.Equal(HttpStatusCode.BadRequest, tooLargeCreate.StatusCode);

        using var missingCategoryContent = CreateToolMultipartContent(
            999999,
            "Missing Category Tool");
        var missingCategoryCreate = await adminClient.PostAsync("/api/admin/tools", missingCategoryContent);
        Assert.Equal(HttpStatusCode.NotFound, missingCategoryCreate.StatusCode);
        Assert.True(
            !Directory.Exists(_factory.UploadRootPath) || Directory.GetFiles(_factory.UploadRootPath).Length == 0,
            "Expected failed create attempts to clean up any stored first-image files.");

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

    private static MultipartFormDataContent CreateToolMultipartContent(
        int categoryId,
        string name,
        string description = "A valid admin-created equipment item for integration coverage.",
        string hourlyRate = "11",
        string dailyRate = "55",
        string weeklyRate = "220",
        string? specialNotes = "Includes operator guidance.",
        string depositRequired = "false",
        string? depositAmount = null,
        bool includeFile = true,
        byte[]? fileBytes = null,
        string fileName = "admin-flow-tool.jpg",
        string mediaType = "image/jpeg")
    {
        var content = new MultipartFormDataContent();
        AddString(content, "CategoryId", categoryId.ToString());
        AddString(content, "Name", name);
        AddString(content, "Description", description);
        AddString(content, "HourlyRate", hourlyRate);
        AddString(content, "DailyRate", dailyRate);
        AddString(content, "WeeklyRate", weeklyRate);
        if (!string.IsNullOrWhiteSpace(specialNotes))
        {
            AddString(content, "SpecialNotes", specialNotes);
        }

        AddString(content, "DepositRequired", depositRequired);
        if (depositAmount is not null)
        {
            AddString(content, "DepositAmount", depositAmount);
        }

        if (includeFile)
        {
            var fileContent = new ByteArrayContent(fileBytes ?? [0xFF, 0xD8, 0xFF, 0xD9]);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mediaType);
            content.Add(fileContent, "file", fileName);
        }

        return content;
    }

    private static void AddString(MultipartFormDataContent content, string name, string value)
    {
        content.Add(new StringContent(value), name);
    }
}
