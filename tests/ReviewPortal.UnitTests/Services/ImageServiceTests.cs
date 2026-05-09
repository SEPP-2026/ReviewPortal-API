using Microsoft.Extensions.Options;
using ReviewPortal.Application.Common;
using ReviewPortal.Domain.Entities;
using ReviewPortal.Infrastructure.Services;
using ReviewPortal.UnitTests.TestDoubles;

namespace ReviewPortal.UnitTests.Services;

public class ImageServiceTests
{
    private static readonly byte[] JpegBytes = [0xFF, 0xD8, 0xFF, 0xD9];
    private static readonly byte[] PngBytes = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2];

    [Fact]
    public async Task UploadImageAsync_ValidImage_UploadsBlobAndCreatesToolImage()
    {
        var existingImage = CreateImage(id: 2, displayOrder: 3);
        var tool = CreateTool(images: [existingImage]);
        var service = CreateService(
            [tool],
            [existingImage],
            out var imageRepository,
            out var unitOfWork,
            out var blobStorage);
        using var stream = new MemoryStream(PngBytes);

        var result = await service.UploadImageAsync(15, stream, "hammer.PNG", CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.StartsWith($"{FakeBlobImageStorage.PublicBaseUrl}/tools/15/", result.Value!.ImageUrl);
        Assert.EndsWith(".png", result.Value.ImageUrl);
        Assert.Equal(4, result.Value.DisplayOrder);
        Assert.Equal("image/png", blobStorage.GetContentType(result.Value.ImageUrl));
        Assert.Equal(PngBytes, blobStorage.GetBytes(result.Value.ImageUrl));
        Assert.Equal(2, imageRepository.Items.Count);
        Assert.Contains(imageRepository.Items, image =>
            image.ToolId == 15 &&
            image.ImageUrl == result.Value.ImageUrl &&
            image.DisplayOrder == 4);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task StoreImageFileAsync_ValidImage_UploadsBlobAndReturnsFirstImageUrl()
    {
        var service = CreateService(
            [],
            [],
            out var imageRepository,
            out var unitOfWork,
            out var blobStorage);
        using var stream = new MemoryStream(JpegBytes);

        var result = await service.StoreImageFileAsync(stream, "first-image.JPG", CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.StartsWith($"{FakeBlobImageStorage.PublicBaseUrl}/tools/first/", result.Value!);
        Assert.EndsWith(".jpg", result.Value);
        Assert.True(blobStorage.ContainsUrl(result.Value));
        Assert.Equal("image/jpeg", blobStorage.GetContentType(result.Value));
        Assert.Empty(imageRepository.Items);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task StoreImageFileAsync_InvalidExtension_ReturnsValidationFailureWithoutUploadingBlob()
    {
        var service = CreateService(
            [],
            [],
            out _,
            out _,
            out var blobStorage);
        using var stream = new MemoryStream([1, 2, 3]);

        var result = await service.StoreImageFileAsync(stream, "first-image.gif", CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal(0, blobStorage.BlobCount);
    }

    [Fact]
    public async Task StoreImageFileAsync_FileContentDoesNotMatchExtension_ReturnsValidationFailure()
    {
        var service = CreateService(
            [],
            [],
            out _,
            out _,
            out var blobStorage);
        using var stream = new MemoryStream([1, 2, 3]);

        var result = await service.StoreImageFileAsync(stream, "first-image.jpg", CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal("Image content does not match the file extension.", result.Error);
        Assert.Equal(0, blobStorage.BlobCount);
    }

    [Fact]
    public async Task StoreImageFileAsync_FileTooLarge_ReturnsValidationFailureWithoutUploadingBlob()
    {
        var service = CreateService(
            [],
            [],
            out _,
            out _,
            out var blobStorage,
            maxFileSizeBytes: 2);
        using var stream = new MemoryStream(JpegBytes);

        var result = await service.StoreImageFileAsync(stream, "first-image.jpg", CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal(0, blobStorage.BlobCount);
    }

    [Fact]
    public async Task DeleteStoredImageAsync_StoredBlob_DeletesBlob()
    {
        var service = CreateService(
            [],
            [],
            out _,
            out _,
            out var blobStorage);
        var imageUrl = blobStorage.AddBlob("tools/first/delete-me.jpg", JpegBytes, "image/jpeg");

        await service.DeleteStoredImageAsync(imageUrl, CancellationToken.None);

        Assert.False(blobStorage.ContainsUrl(imageUrl));
    }

    [Fact]
    public async Task UploadImageAsync_InvalidExtension_ReturnsValidationFailure()
    {
        var tool = CreateTool();
        var service = CreateService(
            [tool],
            [],
            out var imageRepository,
            out var unitOfWork,
            out var blobStorage);
        using var stream = new MemoryStream([1, 2, 3]);

        var result = await service.UploadImageAsync(15, stream, "hammer.gif", CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Empty(imageRepository.Items);
        Assert.Equal(0, blobStorage.BlobCount);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task UploadImageAsync_FileTooLarge_ReturnsValidationFailure()
    {
        var tool = CreateTool();
        var service = CreateService(
            [tool],
            [],
            out var imageRepository,
            out var unitOfWork,
            out var blobStorage,
            maxFileSizeBytes: 2);
        using var stream = new MemoryStream(JpegBytes);

        var result = await service.UploadImageAsync(15, stream, "hammer.jpg", CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Empty(imageRepository.Items);
        Assert.Equal(0, blobStorage.BlobCount);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task UploadImageAsync_ToolDoesNotExist_ReturnsNotFound()
    {
        var service = CreateService(
            [],
            [],
            out _,
            out var unitOfWork,
            out var blobStorage);
        using var stream = new MemoryStream(JpegBytes);

        var result = await service.UploadImageAsync(404, stream, "hammer.jpg", CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.FailureType);
        Assert.Equal(0, blobStorage.BlobCount);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task DeleteImageAsync_ExistingImage_DeletesBlobAndRecord()
    {
        var blobStorage = new FakeBlobImageStorage();
        var imageUrl = blobStorage.AddBlob("tools/15/delete-me.jpg", JpegBytes, "image/jpeg");
        var imageToDelete = CreateImage(id: 3, imageUrl: imageUrl, displayOrder: 1);
        var imageToKeep = CreateImage(id: 4, imageUrl: "/uploads/tools/keep-me.jpg", displayOrder: 2);
        var tool = CreateTool(images: [imageToDelete, imageToKeep]);
        var service = CreateService(
            [tool],
            [imageToDelete, imageToKeep],
            out var imageRepository,
            out var unitOfWork,
            out _,
            blobStorageOverride: blobStorage);

        var result = await service.DeleteImageAsync(15, 3, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        Assert.DoesNotContain(imageRepository.Items, image => image.Id == 3);
        Assert.Contains(imageRepository.Items, image => image.Id == 4);
        Assert.False(blobStorage.ContainsUrl(imageUrl));
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task DeleteImageAsync_LastImage_ReturnsValidationFailure()
    {
        var blobStorage = new FakeBlobImageStorage();
        var imageUrl = blobStorage.AddBlob("tools/15/last-image.jpg", JpegBytes, "image/jpeg");
        var image = CreateImage(id: 3, imageUrl: imageUrl);
        var tool = CreateTool(images: [image]);
        var service = CreateService(
            [tool],
            [image],
            out var imageRepository,
            out var unitOfWork,
            out _,
            blobStorageOverride: blobStorage);

        var result = await service.DeleteImageAsync(15, 3, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Contains(imageRepository.Items, repositoryImage => repositoryImage.Id == 3);
        Assert.True(blobStorage.ContainsUrl(imageUrl));
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task DeleteImageAsync_ImageDoesNotExist_ReturnsNotFound()
    {
        var image = CreateImage(id: 3);
        var tool = CreateTool(images: [image, CreateImage(id: 4, displayOrder: 2)]);
        var service = CreateService(
            [tool],
            [image],
            out _,
            out var unitOfWork,
            out _);

        var result = await service.DeleteImageAsync(15, 404, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.FailureType);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    private static ImageService CreateService(
        IReadOnlyList<Tool> tools,
        IReadOnlyList<ToolImage> images,
        out InMemoryRepository<ToolImage> imageRepository,
        out FakeUnitOfWork unitOfWork,
        out FakeBlobImageStorage blobStorage,
        long maxFileSizeBytes = 5 * 1024 * 1024,
        FakeBlobImageStorage? blobStorageOverride = null)
    {
        var toolRepository = new InMemoryToolRepository(tools);
        imageRepository = new InMemoryRepository<ToolImage>(images);
        unitOfWork = new FakeUnitOfWork();
        blobStorage = blobStorageOverride ?? new FakeBlobImageStorage();

        return new ImageService(
            toolRepository,
            imageRepository,
            unitOfWork,
            Options.Create(new ImageStorageOptions
            {
                ContainerName = "tool-images",
                PublicBaseUrl = FakeBlobImageStorage.PublicBaseUrl,
                BlobNamePrefix = "tools",
                MaxFileSizeBytes = maxFileSizeBytes,
                AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"]
            }),
            blobStorage);
    }

    private static Tool CreateTool(params ToolImage[] images)
    {
        return new Tool
        {
            Id = 15,
            CategoryId = 1,
            Name = "Rotary Hammer",
            Description = "Heavy-duty hammer drill for masonry and concrete work.",
            HourlyRate = 8.50m,
            DailyRate = 45.00m,
            WeeklyRate = 180.00m,
            Images = images.ToList()
        };
    }

    private static ToolImage CreateImage(
        int id,
        string imageUrl = "/uploads/tools/existing.jpg",
        int displayOrder = 1)
    {
        return new ToolImage
        {
            Id = id,
            ToolId = 15,
            ImageUrl = imageUrl,
            DisplayOrder = displayOrder,
            UploadedDate = new DateTime(2026, 4, 20, 9, 0, 0, DateTimeKind.Utc)
        };
    }

    private sealed class FakeBlobImageStorage : IBlobImageStorage
    {
        public const string PublicBaseUrl = "https://reviewportaltest.blob.core.windows.net/tool-images";

        private readonly Dictionary<string, StoredBlob> _blobs = new(StringComparer.OrdinalIgnoreCase);

        public int BlobCount => _blobs.Count;

        public async Task<string> UploadAsync(
            string blobName,
            Stream content,
            string contentType,
            CancellationToken cancellationToken = default)
        {
            using var memoryStream = new MemoryStream();
            content.Position = 0;
            await content.CopyToAsync(memoryStream, cancellationToken);
            _blobs[blobName] = new StoredBlob(memoryStream.ToArray(), contentType);
            return $"{PublicBaseUrl}/{blobName}";
        }

        public Task DeleteAsync(
            string imageUrl,
            CancellationToken cancellationToken = default)
        {
            _blobs.Remove(GetBlobName(imageUrl));
            return Task.CompletedTask;
        }

        public string AddBlob(string blobName, byte[] bytes, string contentType)
        {
            _blobs[blobName] = new StoredBlob(bytes, contentType);
            return $"{PublicBaseUrl}/{blobName}";
        }

        public bool ContainsUrl(string imageUrl)
        {
            return _blobs.ContainsKey(GetBlobName(imageUrl));
        }

        public byte[] GetBytes(string imageUrl)
        {
            return _blobs[GetBlobName(imageUrl)].Bytes;
        }

        public string GetContentType(string imageUrl)
        {
            return _blobs[GetBlobName(imageUrl)].ContentType;
        }

        private static string GetBlobName(string imageUrl)
        {
            return imageUrl[(PublicBaseUrl.Length + 1)..];
        }

        private sealed record StoredBlob(byte[] Bytes, string ContentType);
    }
}
