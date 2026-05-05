namespace ReviewPortal.Infrastructure.Services;

public sealed class ImageStorageOptions
{
    public const string SectionName = "ImageStorage";

    public string RootPath { get; set; } = Path.Combine("uploads", "tools");

    public string RequestPath { get; set; } = "/uploads/tools";

    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;

    public string[] AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".webp"];
}
