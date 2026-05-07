using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Tools;

namespace ReviewPortal.Application.Interfaces;

public interface IImageService
{
    Task<Result<string>> StoreImageFileAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default);

    Task DeleteStoredImageAsync(
        string imageUrl,
        CancellationToken cancellationToken = default);

    Task<Result<ToolImageDto>> UploadImageAsync(
        int toolId,
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default);

    Task<Result<bool>> DeleteImageAsync(
        int toolId,
        int imageId,
        CancellationToken cancellationToken = default);
}
