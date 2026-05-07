using Microsoft.Extensions.Options;
using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Tools;
using ReviewPortal.Application.Interfaces;
using ReviewPortal.Domain.Entities;
using ReviewPortal.Domain.Interfaces;

namespace ReviewPortal.Infrastructure.Services;

public class ImageService : IImageService
{
    private const int BufferSize = 81920;
    private const string DefaultRequestPath = "/uploads/tools";
    private const string FirstImagePrefix = "first";

    private readonly IToolRepository _toolRepository;
    private readonly IRepository<ToolImage> _toolImageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ImageStorageOptions _options;

    public ImageService(
        IToolRepository toolRepository,
        IRepository<ToolImage> toolImageRepository,
        IUnitOfWork unitOfWork,
        IOptions<ImageStorageOptions> options)
    {
        _toolRepository = toolRepository;
        _toolImageRepository = toolImageRepository;
        _unitOfWork = unitOfWork;
        _options = options.Value;
    }

    public async Task<Result<ToolImageDto>> UploadImageAsync(
        int toolId,
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var tool = await _toolRepository.GetByIdWithDetailsAsync(toolId, cancellationToken);
        if (tool is null)
        {
            return Result<ToolImageDto>.NotFound($"Tool with ID {toolId} not found.");
        }

        var storeResult = await StoreImageFileInternalAsync(toolId.ToString(), fileStream, fileName, cancellationToken);
        if (!storeResult.IsSuccess)
        {
            return Result<ToolImageDto>.Failure(storeResult.Error!);
        }

        var imageUrl = storeResult.Value!;
        var displayOrder = tool.Images.Count == 0
            ? 1
            : tool.Images.Max(image => image.DisplayOrder) + 1;

        var toolImage = new ToolImage
        {
            ToolId = toolId,
            ImageUrl = imageUrl,
            DisplayOrder = displayOrder,
            UploadedDate = DateTime.UtcNow
        };

        try
        {
            await _toolImageRepository.AddAsync(toolImage, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            DeleteLocalImageFile(imageUrl);
            throw;
        }

        return Result<ToolImageDto>.Success(new ToolImageDto(
            toolImage.Id,
            toolImage.ImageUrl,
            toolImage.DisplayOrder));
    }

    public Task<Result<string>> StoreImageFileAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        return StoreImageFileInternalAsync(FirstImagePrefix, fileStream, fileName, cancellationToken);
    }

    public Task DeleteStoredImageAsync(
        string imageUrl,
        CancellationToken cancellationToken = default)
    {
        DeleteLocalImageFile(imageUrl);
        return Task.CompletedTask;
    }

    public async Task<Result<bool>> DeleteImageAsync(
        int toolId,
        int imageId,
        CancellationToken cancellationToken = default)
    {
        var tool = await _toolRepository.GetByIdWithDetailsAsync(toolId, cancellationToken);
        if (tool is null)
        {
            return Result<bool>.NotFound($"Tool with ID {toolId} not found.");
        }

        var image = await _toolImageRepository.GetByIdAsync(imageId, cancellationToken);
        if (image is null || image.ToolId != toolId)
        {
            return Result<bool>.NotFound($"Image with ID {imageId} not found for tool {toolId}.");
        }

        if (tool.Images.Count <= 1)
        {
            return Result<bool>.Failure("Cannot delete the last image");
        }

        DeleteLocalImageFile(image.ImageUrl);

        await _toolImageRepository.DeleteAsync(image, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    private bool IsAllowedExtension(string extension)
    {
        return _options.AllowedExtensions
            .Select(NormalizeExtension)
            .Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    private async Task<Result<string>> StoreImageFileInternalAsync(
        string fileNamePrefix,
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(fileName) || fileStream is null || !fileStream.CanRead)
        {
            return Result<string>.Failure("Image file is required.");
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!IsAllowedExtension(extension))
        {
            return Result<string>.Failure("Only .jpg, .jpeg, .png, and .webp image files are allowed.");
        }

        if (fileStream.CanSeek)
        {
            if (fileStream.Length == 0)
            {
                return Result<string>.Failure("Image file is required.");
            }

            if (fileStream.Length > _options.MaxFileSizeBytes)
            {
                return Result<string>.Failure(GetFileSizeError());
            }

            fileStream.Position = 0;
        }

        var rootPath = GetStorageRootPath();
        Directory.CreateDirectory(rootPath);

        var storedFileName = $"{fileNamePrefix}-{Guid.NewGuid():N}{extension}";
        var storedFilePath = Path.Combine(rootPath, storedFileName);
        var writeResult = await WriteFileAsync(fileStream, storedFilePath, cancellationToken);

        if (!writeResult.IsSuccess)
        {
            DeleteFileIfExists(storedFilePath);
            return Result<string>.Failure(writeResult.Error!);
        }

        return Result<string>.Success($"{NormalizeRequestPath(_options.RequestPath)}/{storedFileName}");
    }

    private async Task<FileWriteResult> WriteFileAsync(
        Stream fileStream,
        string storedFilePath,
        CancellationToken cancellationToken)
    {
        var totalBytes = 0L;
        var buffer = new byte[BufferSize];

        await using var output = File.Create(storedFilePath);
        int bytesRead;
        while ((bytesRead = await fileStream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
        {
            totalBytes += bytesRead;
            if (totalBytes > _options.MaxFileSizeBytes)
            {
                return FileWriteResult.Failure(GetFileSizeError());
            }

            await output.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
        }

        return totalBytes == 0
            ? FileWriteResult.Failure("Image file is required.")
            : FileWriteResult.Success();
    }

    private void DeleteLocalImageFile(string imageUrl)
    {
        var requestPath = NormalizeRequestPath(_options.RequestPath);
        if (!imageUrl.StartsWith($"{requestPath}/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var storedFileName = imageUrl[(requestPath.Length + 1)..];
        if (storedFileName.Length == 0 || storedFileName != Path.GetFileName(storedFileName))
        {
            return;
        }

        var rootPath = EnsureTrailingDirectorySeparator(GetStorageRootPath());
        var filePath = Path.GetFullPath(Path.Combine(rootPath, storedFileName));
        if (!filePath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        DeleteFileIfExists(filePath);
    }

    private string GetStorageRootPath()
    {
        var rootPath = string.IsNullOrWhiteSpace(_options.RootPath)
            ? Path.Combine("uploads", "tools")
            : _options.RootPath;

        return Path.GetFullPath(Path.IsPathRooted(rootPath)
            ? rootPath
            : Path.Combine(Directory.GetCurrentDirectory(), rootPath));
    }

    private static string NormalizeRequestPath(string requestPath)
    {
        if (string.IsNullOrWhiteSpace(requestPath))
        {
            return DefaultRequestPath;
        }

        var normalized = requestPath.Replace('\\', '/').TrimEnd('/');
        return normalized.StartsWith('/')
            ? normalized
            : $"/{normalized}";
    }

    private static string NormalizeExtension(string extension)
    {
        return extension.StartsWith('.')
            ? extension.ToLowerInvariant()
            : $".{extension.ToLowerInvariant()}";
    }

    private static string EnsureTrailingDirectorySeparator(string path)
    {
        return path.EndsWith(Path.DirectorySeparatorChar)
            ? path
            : $"{path}{Path.DirectorySeparatorChar}";
    }

    private static void DeleteFileIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private string GetFileSizeError()
    {
        var maxMegabytes = _options.MaxFileSizeBytes / 1024m / 1024m;
        return $"Image file must be {maxMegabytes:0.#}MB or smaller.";
    }

    private sealed record FileWriteResult(bool IsSuccess, string? Error)
    {
        public static FileWriteResult Success() => new(true, null);

        public static FileWriteResult Failure(string error) => new(false, error);
    }
}
