using System.Net;
using System.Net.Http.Json;
using ReviewPortal.Application.DTOs.Tools;

namespace ReviewPortal.IntegrationTests.Api;

[Collection(ReviewPortalApiCollection.CollectionName)]
public class AdminToolImagesApiIntegrationTests : IAsyncLifetime
{
    private readonly ReviewPortalApiFactory _factory;

    public AdminToolImagesApiIntegrationTests(ReviewPortalApiFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync()
    {
        return _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task AdminToolImageUploadAndDelete_WhenValidFilePosted_StoresAndRemovesBlob()
    {
        using var adminClient = await _factory.CreateAuthenticatedClientAsync(
            ReviewPortalApiFactory.AdminEmail,
            ReviewPortalApiFactory.AdminPassword);

        using var uploadContent = CreateImageContent([0xFF, 0xD8, 0xFF, 0xD9], "tower-extra.jpg");
        var uploadResponse = await adminClient.PostAsync(
            $"/api/admin/tools/{_factory.TowerScaffoldToolId}/images",
            uploadContent);

        Assert.Equal(HttpStatusCode.Created, uploadResponse.StatusCode);
        var uploadedImage = await uploadResponse.Content.ReadFromJsonAsync<ToolImageDto>();
        Assert.NotNull(uploadedImage);
        Assert.Equal(2, uploadedImage!.DisplayOrder);
        Assert.StartsWith($"{TestBlobImageStorage.PublicBaseUrl}/tools/{_factory.TowerScaffoldToolId}/", uploadedImage.ImageUrl);
        Assert.Equal("image/jpeg", _factory.BlobStorage.GetContentType(uploadedImage.ImageUrl));
        Assert.True(_factory.BlobStorage.ContainsUrl(uploadedImage.ImageUrl));

        var deleteResponse = await adminClient.DeleteAsync(
            $"/api/admin/tools/{_factory.TowerScaffoldToolId}/images/{uploadedImage.Id}");

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        var deleted = await deleteResponse.Content.ReadFromJsonAsync<bool>();
        Assert.True(deleted);
        Assert.False(_factory.BlobStorage.ContainsUrl(uploadedImage.ImageUrl));
    }

    [Fact]
    public async Task AdminToolImageUpload_WhenFileMissingInvalidOrTooLarge_ReturnsBadRequest()
    {
        using var adminClient = await _factory.CreateAuthenticatedClientAsync(
            ReviewPortalApiFactory.AdminEmail,
            ReviewPortalApiFactory.AdminPassword);

        using var missingContent = new MultipartFormDataContent();
        var missingFileResponse = await adminClient.PostAsync(
            $"/api/admin/tools/{_factory.TowerScaffoldToolId}/images",
            missingContent);
        Assert.Equal(HttpStatusCode.BadRequest, missingFileResponse.StatusCode);

        using var invalidExtensionContent = CreateImageContent([1, 2, 3], "not-an-image.txt", "text/plain");
        var invalidExtensionResponse = await adminClient.PostAsync(
            $"/api/admin/tools/{_factory.TowerScaffoldToolId}/images",
            invalidExtensionContent);
        Assert.Equal(HttpStatusCode.BadRequest, invalidExtensionResponse.StatusCode);

        using var mismatchedContent = CreateImageContent([1, 2, 3], "not-an-image.jpg");
        var mismatchedResponse = await adminClient.PostAsync(
            $"/api/admin/tools/{_factory.TowerScaffoldToolId}/images",
            mismatchedContent);
        Assert.Equal(HttpStatusCode.BadRequest, mismatchedResponse.StatusCode);

        var oversizedBytes = new byte[(5 * 1024 * 1024) + 1];
        oversizedBytes[0] = 0xFF;
        oversizedBytes[1] = 0xD8;
        using var tooLargeContent = CreateImageContent(oversizedBytes, "too-large.jpg");
        var tooLargeResponse = await adminClient.PostAsync(
            $"/api/admin/tools/{_factory.TowerScaffoldToolId}/images",
            tooLargeContent);
        Assert.Equal(HttpStatusCode.BadRequest, tooLargeResponse.StatusCode);
    }

    [Fact]
    public async Task AdminToolImageDelete_WhenImageMissingOrLastImage_ReturnsExpectedStatusCodes()
    {
        using var adminClient = await _factory.CreateAuthenticatedClientAsync(
            ReviewPortalApiFactory.AdminEmail,
            ReviewPortalApiFactory.AdminPassword);

        var missingImageResponse = await adminClient.DeleteAsync(
            $"/api/admin/tools/{_factory.TowerScaffoldToolId}/images/999999");
        Assert.Equal(HttpStatusCode.NotFound, missingImageResponse.StatusCode);

        var missingToolResponse = await adminClient.DeleteAsync("/api/admin/tools/999999/images/1");
        Assert.Equal(HttpStatusCode.NotFound, missingToolResponse.StatusCode);

        var tool = await adminClient.GetFromJsonAsync<ToolDto>($"/api/admin/tools/{_factory.TowerScaffoldToolId}");
        Assert.NotNull(tool);
        var onlyImage = Assert.Single(tool!.Images);

        var deleteLastImageResponse = await adminClient.DeleteAsync(
            $"/api/admin/tools/{_factory.TowerScaffoldToolId}/images/{onlyImage.Id}");
        Assert.Equal(HttpStatusCode.BadRequest, deleteLastImageResponse.StatusCode);
    }

    private static MultipartFormDataContent CreateImageContent(
        byte[] bytes,
        string fileName,
        string mediaType = "image/jpeg")
    {
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mediaType);
        content.Add(fileContent, "file", fileName);
        return content;
    }
}
