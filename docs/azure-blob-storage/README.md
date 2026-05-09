# Azure Blob Storage for Tool/Service Images

This README documents how to create Azure Blob Storage for ReviewPortal tool/service image uploads and how to configure the API to use it.

## What Changes in the API

- Admin upload endpoints keep the same routes:
  - `POST /api/admin/tools` for first image plus tool/service metadata.
  - `POST /api/admin/tools/{id}/images` for additional images.
  - `DELETE /api/admin/tools/{id}/images/{imageId}` for image deletion.
- Uploaded image bytes are stored in Azure Blob Storage instead of `uploads/tools` on the API host.
- `ToolImages.ImageUrl` continues to store a URL string, now pointing at the blob URL.
- No EF Core migration is required because the schema already stores `ImageUrl` as `nvarchar(500)`.
- Uploads are still limited to `.jpg`, `.jpeg`, `.png`, and `.webp` files up to 5 MB, with file-signature validation.

## Create Storage in the Azure Portal

1. Sign in to the [Azure Portal](https://portal.azure.com/).
2. Create or choose a Resource Group for the ReviewPortal environment.
3. Create a Storage Account:
   - Performance: `Standard`.
   - Redundancy: `LRS` for demo/dev or `ZRS`/`GRS` for production requirements.
   - Region: match the App Service region where possible.
   - Public network access: keep enabled unless the App Service has private endpoint/VNet access.
4. Open the Storage Account and create a Blob container:
   - Container name: `tool-images`.
   - Public access level:
     - Use `Blob` if the frontend should load stored image URLs directly.
     - Use `Private` only if you later add a signed URL or image-proxy flow; the current API stores a stable URL without SAS tokens.
5. Enable soft delete for blobs in `Data protection` so accidental deletes can be recovered during the retention window.
6. Keep storage account keys out of source control. Prefer managed identity plus Azure RBAC for production.

## Give the API Access

Recommended production setup:

1. Open the Azure App Service for `ReviewPortal.API`.
2. Enable a system-assigned managed identity.
3. Open the Storage Account, then `Access control (IAM)`.
4. Add a role assignment:
   - Role: `Storage Blob Data Contributor`.
   - Assign access to: `Managed identity`.
   - Member: the ReviewPortal API App Service managed identity.
5. Wait a few minutes for RBAC propagation before testing uploads.

Local development options:

- Use `DefaultAzureCredential` by signing in with Azure CLI or Visual Studio, then assign your user `Storage Blob Data Contributor` on the storage account.
- Or use a storage connection string in user secrets for local development only.

Microsoft's .NET Blob Storage guidance recommends `DefaultAzureCredential`/managed identity for passwordless access and warns against exposing account keys in insecure locations.

## API Configuration

Set these values through user secrets, environment variables, or App Service application settings.

```json
{
  "ImageStorage": {
    "ConnectionString": "",
    "ServiceUri": "https://<storage-account-name>.blob.core.windows.net",
    "ContainerName": "tool-images",
    "PublicBaseUrl": "https://<storage-account-name>.blob.core.windows.net/tool-images",
    "BlobNamePrefix": "tools",
    "MaxFileSizeBytes": 5242880,
    "AllowedExtensions": [ ".jpg", ".jpeg", ".png", ".webp" ]
  }
}
```

Use `ConnectionString` only when managed identity is not available. If both `ConnectionString` and `ServiceUri` are configured, the connection string is used.

Environment variable form:

```powershell
$env:ImageStorage__ServiceUri = "https://<storage-account-name>.blob.core.windows.net"
$env:ImageStorage__ContainerName = "tool-images"
$env:ImageStorage__PublicBaseUrl = "https://<storage-account-name>.blob.core.windows.net/tool-images"
$env:ImageStorage__BlobNamePrefix = "tools"
```

Local user secrets:

```powershell
dotnet user-secrets set "ImageStorage:ServiceUri" "https://<storage-account-name>.blob.core.windows.net" --project src/ReviewPortal.API
dotnet user-secrets set "ImageStorage:ContainerName" "tool-images" --project src/ReviewPortal.API
dotnet user-secrets set "ImageStorage:PublicBaseUrl" "https://<storage-account-name>.blob.core.windows.net/tool-images" --project src/ReviewPortal.API
```

Connection string fallback for local-only use:

```powershell
dotnet user-secrets set "ImageStorage:ConnectionString" "<storage-connection-string>" --project src/ReviewPortal.API
```

## Verify Uploads

1. Start the API with valid database, JWT, and image storage settings.
2. Sign in as an Admin user.
3. Create a tool/service with a valid JPG, PNG, or WebP first image.
4. Confirm the API response includes an `Images[0].ImageUrl` value under the configured blob base URL.
5. Confirm the blob exists in the `tool-images` container.
6. Upload an additional image through `POST /api/admin/tools/{id}/images`.
7. Delete a non-last image through `DELETE /api/admin/tools/{id}/images/{imageId}` and confirm the blob is removed or soft-deleted.

If the API response still returns an image URL beginning with `/uploads/tools/`, the App Service is still running the old local-file build. Deploy the latest API code, restart the App Service, and repeat the upload test. A successful Blob Storage upload returns a URL beginning with `https://<storage-account-name>.blob.core.windows.net/tool-images/`.

## Operational Notes

- Do not commit `appsettings.Local.json`, storage connection strings, account keys, SAS tokens, or `.env` files.
- Blob names are generated server-side and do not reuse the uploaded client filename.
- The API sets blob content type based on the validated extension.
- Existing seeded image URLs can remain as sample/static paths until migrated manually to real blob URLs.
- If the container is public, only blob read access is public; uploads and deletes still happen through authenticated Admin API endpoints.

## References

- [Azure Blob Storage client library for .NET](https://learn.microsoft.com/en-us/azure/storage/blobs/storage-quickstart-blobs-dotnet)
- [Create and manage Azure Blob Storage clients](https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blob-client-management)
- [Delete and restore blobs with .NET](https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blob-delete)
