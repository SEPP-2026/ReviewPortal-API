using Microsoft.Extensions.Options;
using ReviewPortal.Application.Common;
using ReviewPortal.Domain.Entities;
using ReviewPortal.Infrastructure.Services;
using ReviewPortal.UnitTests.TestDoubles;

namespace ReviewPortal.UnitTests.Services;

public class ImageServiceTests
{
    [Fact]
    public async Task UploadImageAsync_ValidImage_SavesFileAndCreatesToolImage()
    {
        var rootPath = CreateTempRootPath();
        var existingImage = CreateImage(id: 2, displayOrder: 3);
        var tool = CreateTool(images: [existingImage]);

        try
        {
            var service = CreateService(
                rootPath,
                [tool],
                [existingImage],
                out var imageRepository,
                out var unitOfWork);
            using var stream = new MemoryStream([1, 2, 3, 4]);

            var result = await service.UploadImageAsync(15, stream, "hammer.PNG", CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.StartsWith("/uploads/tools/15-", result.Value!.ImageUrl);
            Assert.EndsWith(".png", result.Value.ImageUrl);
            Assert.Equal(4, result.Value.DisplayOrder);
            Assert.Equal(2, imageRepository.Items.Count);
            Assert.Contains(imageRepository.Items, image =>
                image.ToolId == 15 &&
                image.ImageUrl == result.Value.ImageUrl &&
                image.DisplayOrder == 4);
            Assert.Single(Directory.GetFiles(rootPath));
            Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        }
        finally
        {
            DeleteTempRootPath(rootPath);
        }
    }

    [Fact]
    public async Task UploadImageAsync_InvalidExtension_ReturnsValidationFailure()
    {
        var rootPath = CreateTempRootPath();
        var tool = CreateTool();

        try
        {
            var service = CreateService(
                rootPath,
                [tool],
                [],
                out var imageRepository,
                out var unitOfWork);
            using var stream = new MemoryStream([1, 2, 3]);

            var result = await service.UploadImageAsync(15, stream, "hammer.gif", CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.FailureType);
            Assert.Empty(imageRepository.Items);
            Assert.Equal(0, unitOfWork.SaveChangesCallCount);
        }
        finally
        {
            DeleteTempRootPath(rootPath);
        }
    }

    [Fact]
    public async Task UploadImageAsync_FileTooLarge_ReturnsValidationFailure()
    {
        var rootPath = CreateTempRootPath();
        var tool = CreateTool();

        try
        {
            var service = CreateService(
                rootPath,
                [tool],
                [],
                out var imageRepository,
                out var unitOfWork,
                maxFileSizeBytes: 2);
            using var stream = new MemoryStream([1, 2, 3]);

            var result = await service.UploadImageAsync(15, stream, "hammer.jpg", CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.FailureType);
            Assert.Empty(imageRepository.Items);
            Assert.Equal(0, unitOfWork.SaveChangesCallCount);
        }
        finally
        {
            DeleteTempRootPath(rootPath);
        }
    }

    [Fact]
    public async Task UploadImageAsync_ToolDoesNotExist_ReturnsNotFound()
    {
        var rootPath = CreateTempRootPath();

        try
        {
            var service = CreateService(
                rootPath,
                [],
                [],
                out _,
                out var unitOfWork);
            using var stream = new MemoryStream([1, 2, 3]);

            var result = await service.UploadImageAsync(404, stream, "hammer.jpg", CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.FailureType);
            Assert.Equal(0, unitOfWork.SaveChangesCallCount);
        }
        finally
        {
            DeleteTempRootPath(rootPath);
        }
    }

    [Fact]
    public async Task DeleteImageAsync_ExistingImage_DeletesFileAndRecord()
    {
        var rootPath = CreateTempRootPath();
        var imageToDelete = CreateImage(id: 3, imageUrl: "/uploads/tools/delete-me.jpg", displayOrder: 1);
        var imageToKeep = CreateImage(id: 4, imageUrl: "/uploads/tools/keep-me.jpg", displayOrder: 2);
        var tool = CreateTool(images: [imageToDelete, imageToKeep]);

        try
        {
            Directory.CreateDirectory(rootPath);
            await File.WriteAllBytesAsync(Path.Combine(rootPath, "delete-me.jpg"), [1, 2, 3], CancellationToken.None);
            var service = CreateService(
                rootPath,
                [tool],
                [imageToDelete, imageToKeep],
                out var imageRepository,
                out var unitOfWork);

            var result = await service.DeleteImageAsync(15, 3, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value);
            Assert.DoesNotContain(imageRepository.Items, image => image.Id == 3);
            Assert.Contains(imageRepository.Items, image => image.Id == 4);
            Assert.False(File.Exists(Path.Combine(rootPath, "delete-me.jpg")));
            Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        }
        finally
        {
            DeleteTempRootPath(rootPath);
        }
    }

    [Fact]
    public async Task DeleteImageAsync_LastImage_ReturnsValidationFailure()
    {
        var rootPath = CreateTempRootPath();
        var image = CreateImage(id: 3, imageUrl: "/uploads/tools/last-image.jpg");
        var tool = CreateTool(images: [image]);

        try
        {
            Directory.CreateDirectory(rootPath);
            var filePath = Path.Combine(rootPath, "last-image.jpg");
            await File.WriteAllBytesAsync(filePath, [1, 2, 3], CancellationToken.None);
            var service = CreateService(
                rootPath,
                [tool],
                [image],
                out var imageRepository,
                out var unitOfWork);

            var result = await service.DeleteImageAsync(15, 3, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.FailureType);
            Assert.Contains(imageRepository.Items, repositoryImage => repositoryImage.Id == 3);
            Assert.True(File.Exists(filePath));
            Assert.Equal(0, unitOfWork.SaveChangesCallCount);
        }
        finally
        {
            DeleteTempRootPath(rootPath);
        }
    }

    [Fact]
    public async Task DeleteImageAsync_ImageDoesNotExist_ReturnsNotFound()
    {
        var rootPath = CreateTempRootPath();
        var image = CreateImage(id: 3);
        var tool = CreateTool(images: [image, CreateImage(id: 4, displayOrder: 2)]);

        try
        {
            var service = CreateService(
                rootPath,
                [tool],
                [image],
                out _,
                out var unitOfWork);

            var result = await service.DeleteImageAsync(15, 404, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.FailureType);
            Assert.Equal(0, unitOfWork.SaveChangesCallCount);
        }
        finally
        {
            DeleteTempRootPath(rootPath);
        }
    }

    private static ImageService CreateService(
        string rootPath,
        IReadOnlyList<Tool> tools,
        IReadOnlyList<ToolImage> images,
        out InMemoryRepository<ToolImage> imageRepository,
        out FakeUnitOfWork unitOfWork,
        long maxFileSizeBytes = 5 * 1024 * 1024)
    {
        var toolRepository = new InMemoryToolRepository(tools);
        imageRepository = new InMemoryRepository<ToolImage>(images);
        unitOfWork = new FakeUnitOfWork();

        return new ImageService(
            toolRepository,
            imageRepository,
            unitOfWork,
            Options.Create(new ImageStorageOptions
            {
                RootPath = rootPath,
                RequestPath = "/uploads/tools",
                MaxFileSizeBytes = maxFileSizeBytes,
                AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"]
            }));
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

    private static string CreateTempRootPath()
    {
        return Path.Combine(Path.GetTempPath(), $"reviewportal-images-{Guid.NewGuid():N}");
    }

    private static void DeleteTempRootPath(string rootPath)
    {
        if (Directory.Exists(rootPath))
        {
            Directory.Delete(rootPath, recursive: true);
        }
    }
}
