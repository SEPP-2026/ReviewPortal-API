namespace ReviewPortal.Infrastructure.Services;

public sealed class ImageStorageOptions
{
    public const string SectionName = "ImageStorage";

    public string ConnectionString { get; set; } = string.Empty;

    public string ServiceUri { get; set; } = string.Empty;

    public string ContainerName { get; set; } = "tool-images";

    public string PublicBaseUrl { get; set; } = string.Empty;

    public string BlobNamePrefix { get; set; } = "tools";

    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;

    public string[] AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".webp"];
}
