namespace ReviewPortal.Infrastructure.Services;

public interface IBlobImageStorage
{
    Task<string> UploadAsync(
        string blobName,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string imageUrl,
        CancellationToken cancellationToken = default);
}
