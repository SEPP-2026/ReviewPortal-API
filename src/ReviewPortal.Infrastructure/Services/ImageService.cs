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
    private const string FirstImagePrefix = "first";

    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    private readonly IToolRepository _toolRepository;
    private readonly IRepository<ToolImage> _toolImageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ImageStorageOptions _options;
    private readonly IBlobImageStorage _blobImageStorage;

    public ImageService(
        IToolRepository toolRepository,
        IRepository<ToolImage> toolImageRepository,
        IUnitOfWork unitOfWork,
        IOptions<ImageStorageOptions> options,
        IBlobImageStorage blobImageStorage)
    {
        _toolRepository = toolRepository;
        _toolImageRepository = toolImageRepository;
        _unitOfWork = unitOfWork;
        _options = options.Value;
        _blobImageStorage = blobImageStorage;
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
            await TryDeleteStoredImageAsync(imageUrl, cancellationToken);
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
        return _blobImageStorage.DeleteAsync(imageUrl, cancellationToken);
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

        await _blobImageStorage.DeleteAsync(image.ImageUrl, cancellationToken);

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
        var preparedResult = await PrepareImageFileAsync(fileStream, fileName, cancellationToken);
        if (!preparedResult.IsSuccess)
        {
            return Result<string>.Failure(preparedResult.Error!);
        }

        await using var preparedImage = preparedResult.Value!.Content;
        var blobName = BuildBlobName(fileNamePrefix, preparedResult.Value.Extension);
        var imageUrl = await _blobImageStorage.UploadAsync(
            blobName,
            preparedImage,
            preparedResult.Value.ContentType,
            cancellationToken);

        return Result<string>.Success(imageUrl);
    }

    private async Task<Result<PreparedImage>> PrepareImageFileAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(fileName) || fileStream is null || !fileStream.CanRead)
        {
            return Result<PreparedImage>.Failure("Image file is required.");
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!IsAllowedExtension(extension))
        {
            return Result<PreparedImage>.Failure("Only .jpg, .jpeg, .png, and .webp image files are allowed.");
        }

        if (fileStream.CanSeek)
        {
            if (fileStream.Length == 0)
            {
                return Result<PreparedImage>.Failure("Image file is required.");
            }

            if (fileStream.Length > _options.MaxFileSizeBytes)
            {
                return Result<PreparedImage>.Failure(GetFileSizeError());
            }

            fileStream.Position = 0;
        }

        var contentResult = await CopyToMemoryStreamAsync(fileStream, cancellationToken);
        if (!contentResult.IsSuccess)
        {
            return Result<PreparedImage>.Failure(contentResult.Error!);
        }

        var content = contentResult.Value!;
        if (!HasExpectedSignature(content, extension))
        {
            content.Dispose();
            return Result<PreparedImage>.Failure("Image content does not match the file extension.");
        }

        content.Position = 0;
        return Result<PreparedImage>.Success(new PreparedImage(
            content,
            extension,
            GetContentType(extension)));
    }

    private async Task<Result<MemoryStream>> CopyToMemoryStreamAsync(
        Stream fileStream,
        CancellationToken cancellationToken)
    {
        var totalBytes = 0L;
        var buffer = new byte[BufferSize];
        var content = new MemoryStream();

        int bytesRead;
        while ((bytesRead = await fileStream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
        {
            totalBytes += bytesRead;
            if (totalBytes > _options.MaxFileSizeBytes)
            {
                content.Dispose();
                return Result<MemoryStream>.Failure(GetFileSizeError());
            }

            await content.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
        }

        if (totalBytes == 0)
        {
            content.Dispose();
            return Result<MemoryStream>.Failure("Image file is required.");
        }

        content.Position = 0;
        return Result<MemoryStream>.Success(content);
    }

    private string BuildBlobName(string fileNamePrefix, string extension)
    {
        var segments = new[]
            {
                AzureBlobImageStorage.NormalizeBlobNameSegment(_options.BlobNamePrefix),
                AzureBlobImageStorage.NormalizeBlobNameSegment(fileNamePrefix),
                $"{Guid.NewGuid():N}{extension}"
            }
            .Where(segment => !string.IsNullOrWhiteSpace(segment));

        return string.Join('/', segments);
    }

    private async Task TryDeleteStoredImageAsync(
        string imageUrl,
        CancellationToken cancellationToken)
    {
        try
        {
            await _blobImageStorage.DeleteAsync(imageUrl, cancellationToken);
        }
        catch
        {
            // Preserve the original database exception; cleanup can be retried manually from the blob name.
        }
    }

    private static bool HasExpectedSignature(MemoryStream content, string extension)
    {
        var bytes = content.ToArray();
        return extension switch
        {
            ".jpg" or ".jpeg" => bytes.Length >= 3 &&
                bytes[0] == 0xFF &&
                bytes[1] == 0xD8 &&
                bytes[2] == 0xFF,
            ".png" => bytes.AsSpan().StartsWith(PngSignature),
            ".webp" => bytes.Length >= 12 &&
                bytes[0] == 0x52 &&
                bytes[1] == 0x49 &&
                bytes[2] == 0x46 &&
                bytes[3] == 0x46 &&
                bytes[8] == 0x57 &&
                bytes[9] == 0x45 &&
                bytes[10] == 0x42 &&
                bytes[11] == 0x50,
            _ => false
        };
    }

    private static string GetContentType(string extension)
    {
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }

    private static string NormalizeExtension(string extension)
    {
        return extension.StartsWith('.')
            ? extension.ToLowerInvariant()
            : $".{extension.ToLowerInvariant()}";
    }

    private string GetFileSizeError()
    {
        var maxMegabytes = _options.MaxFileSizeBytes / 1024m / 1024m;
        return $"Image file must be {maxMegabytes:0.#}MB or smaller.";
    }

    private sealed record PreparedImage(
        MemoryStream Content,
        string Extension,
        string ContentType);
}
