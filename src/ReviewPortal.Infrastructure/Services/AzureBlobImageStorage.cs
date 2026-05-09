using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace ReviewPortal.Infrastructure.Services;

public sealed class AzureBlobImageStorage : IBlobImageStorage
{
    private readonly BlobContainerClient _containerClient;
    private readonly ImageStorageOptions _options;

    public AzureBlobImageStorage(
        BlobContainerClient containerClient,
        IOptions<ImageStorageOptions> options)
    {
        _containerClient = containerClient;
        _options = options.Value;
    }

    public async Task<string> UploadAsync(
        string blobName,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        await _containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        content.Position = 0;
        var blobClient = _containerClient.GetBlobClient(blobName);
        await blobClient.UploadAsync(
            content,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                }
            },
            cancellationToken);

        return BuildImageUrl(blobClient.Uri, blobName);
    }

    public async Task DeleteAsync(
        string imageUrl,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetBlobName(imageUrl, out var blobName))
        {
            return;
        }

        await _containerClient
            .GetBlobClient(blobName)
            .DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
    }

    private string BuildImageUrl(Uri blobUri, string blobName)
    {
        var publicBaseUrl = NormalizeBaseUrl(_options.PublicBaseUrl);
        if (publicBaseUrl is null)
        {
            return blobUri.ToString();
        }

        return $"{publicBaseUrl}/{blobName}";
    }

    private bool TryGetBlobName(string imageUrl, out string blobName)
    {
        blobName = string.Empty;
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return false;
        }

        var normalizedImageUrl = imageUrl.Trim();
        var publicBaseUrl = NormalizeBaseUrl(_options.PublicBaseUrl);
        var containerBaseUrl = NormalizeBaseUrl(_containerClient.Uri.ToString());

        if (publicBaseUrl is not null &&
            TryRemoveBaseUrl(normalizedImageUrl, publicBaseUrl, out blobName))
        {
            return IsExpectedBlobName(blobName);
        }

        if (containerBaseUrl is not null &&
            TryRemoveBaseUrl(normalizedImageUrl, containerBaseUrl, out blobName))
        {
            return IsExpectedBlobName(blobName);
        }

        return false;
    }

    private bool IsExpectedBlobName(string blobName)
    {
        var normalizedPrefix = NormalizeBlobNameSegment(_options.BlobNamePrefix);
        if (string.IsNullOrWhiteSpace(normalizedPrefix))
        {
            return blobName.Length > 0;
        }

        return blobName.StartsWith($"{normalizedPrefix}/", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryRemoveBaseUrl(
        string imageUrl,
        string baseUrl,
        out string blobName)
    {
        blobName = string.Empty;
        var prefix = $"{baseUrl}/";
        if (!imageUrl.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        blobName = Uri.UnescapeDataString(imageUrl[prefix.Length..]);
        return blobName.Length > 0;
    }

    private static string? NormalizeBaseUrl(string? baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return null;
        }

        return baseUrl.Trim().TrimEnd('/');
    }

    public static string NormalizeBlobNameSegment(string? segment)
    {
        return string.IsNullOrWhiteSpace(segment)
            ? string.Empty
            : segment.Replace('\\', '/').Trim('/');
    }
}
