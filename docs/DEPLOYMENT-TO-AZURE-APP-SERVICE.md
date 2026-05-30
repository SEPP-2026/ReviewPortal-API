# Deploying To Azure App Service With GitHub Actions

This repository includes a GitHub Actions workflow at `.github/workflows/ci.yml` that:

1. runs unit tests first
2. runs integration tests on Windows with SQL Server LocalDB
3. publishes and deploys only on pushes to `main`
4. deploys the API to Azure App Service `reviewportal-api`

Production URL:

`https://reviewportal-api-escdb3f2epg8eeha.southeastasia-01.azurewebsites.net`

## How the workflow behaves

- Pull requests into `development` or `main`: run unit tests and integration tests
- Pushes to `development`: run unit tests and integration tests only
- Pushes to `main`: run unit tests and integration tests, publish the API, deploy to Azure App Service through the `production` GitHub environment, then trigger the Playwright QA automation workflow

## Recommended setup

This workflow is configured to use a publish profile secret named `AZURE_WEBAPP_PUBLISH_PROFILE`.

That is the simplest setup for your current app service, because your Azure portal already exposes the `Download publish profile` option.

## Azure setup steps

### 1. Confirm publish-profile access is enabled

In Azure Portal:

1. Open `reviewportal-api`
2. Go to `Settings -> Configuration -> General settings`
3. Make sure `SCM Basic Auth Publishing Credentials` is `On`
4. Save if you changed it

If this setting is off, Azure disables the publish-profile download button.

### 2. Download the publish profile

In Azure Portal:

1. Open `reviewportal-api`
2. Go to `Overview`
3. Click `Download publish profile`
4. Open the downloaded `.PublishSettings` file in a text editor
5. Copy the full file contents

## GitHub setup steps

### Option A: Environment secret and approval gate used by this project

In GitHub:

1. Open your repository
2. Go to `Settings -> Environments`
3. Create an environment named `production`
4. Under `Deployment protection rules`, enable `Required reviewers`
5. Add yourself or the people who are allowed to approve production deployments
6. Recommended: turn on `Prevent self-review` if you want a second person to approve production releases
7. Under deployment branches, restrict the environment to branch `main`
8. In the `production` environment, add a secret named `AZURE_WEBAPP_PUBLISH_PROFILE`
9. Paste the full contents of the downloaded publish profile

This is the setup used by this project.

When the workflow reaches the `Deploy To Azure App Service` job for `main`, GitHub will pause the run and wait for production approval before deployment starts. The `development` branch runs tests only and does not publish or deploy to Azure.

### Option B: Simpler fallback - repository secret

If you do not want to use GitHub environments:

1. Open your repository
2. Go to `Settings -> Secrets and variables -> Actions`
3. Add a repository secret named `AZURE_WEBAPP_PUBLISH_PROFILE`
4. Paste the full contents of the downloaded publish profile

The workflow already restricts deployment to `main`, so this still works.

## Azure application settings you must add

Deployment only publishes code. Your Azure App Service still needs runtime configuration.

In Azure Portal, go to:

`reviewportal-api -> Settings -> Environment variables`

or `Configuration`, depending on the Azure portal view.

Add these application settings:

- `ASPNETCORE_ENVIRONMENT = Production`
- `Jwt__Secret = <strong-random-secret>`
- `Jwt__Issuer = ReviewPortalAPI`
- `Jwt__Audience = ReviewPortalClient`
- `Jwt__ExpiryMinutes = 60`
- `APPLICATIONINSIGHTS_CONNECTION_STRING = <connection-string-from-reviewportal-api-application-insights>`
- `ImageStorage__ServiceUri = https://<storage-account-name>.blob.core.windows.net`
- `ImageStorage__ContainerName = tool-images`
- `ImageStorage__PublicBaseUrl = https://<storage-account-name>.blob.core.windows.net/tool-images`
- `ImageStorage__BlobNamePrefix = tools`

For the database, recommended:

- `ConnectionStrings__DefaultConnection = Server=tcp:<azure-sql-server>.database.windows.net,1433;Initial Catalog=<database-name>;Persist Security Info=False;User ID=<azure-sql-user>;Password=<rotated-password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;`

Alternative:

- Add a connection string named `DefaultConnection` in the `Connection strings` section.

Optional if you have a frontend calling this API:

- `Cors__AllowedOrigins__0 = https://reviewportal-frontend-dccvarataff4a8hg.southeastasia-01.azurewebsites.net`
- `Cors__AllowedOrigins__1 = http://localhost:3000`

After saving configuration, restart the App Service.

Application Insights telemetry is enabled only when `APPLICATIONINSIGHTS_CONNECTION_STRING` or `ApplicationInsights:ConnectionString` is present. Do not commit the real connection string to `appsettings.json`; keep it in Azure App Service settings for production and in .NET user secrets or ignored `appsettings.Local.json` for local development.

Uploaded tool/service images are stored in Azure Blob Storage. Prefer App Service managed identity with the `Storage Blob Data Contributor` role on the storage account; use a storage connection string only as a local development fallback. See [`azure-blob-storage/README.md`](azure-blob-storage/README.md) for the Azure Portal steps.

## Local Azure SQL migrations without committing secrets

For local migration testing against Azure SQL:

1. Copy `src/ReviewPortal.API/appsettings.Local.example.json` to `src/ReviewPortal.API/appsettings.Local.json`
2. Fill the local file with the rotated Azure SQL password and local JWT secret
3. Run:

```powershell
.\scripts\local\Update-AzureDatabase.ps1
```

To run the API locally against the same configured database:

```powershell
.\scripts\local\Run-ApiLocal.ps1
```

`appsettings.Local.json` is ignored by git. Do not paste Azure SQL passwords, publish profiles, or JWT secrets into checked-in appsettings or documentation files.

Before committing, run:

```powershell
.\scripts\security\scan-secrets.ps1
```

## First deployment steps

1. Commit and push this workflow to GitHub
2. Create the `production` environment in GitHub
3. Add required reviewers to the `production` environment
4. Add the `AZURE_WEBAPP_PUBLISH_PROFILE` environment secret, or use one repository secret
5. Add Azure application settings and connection string
6. Open a PR into `development` and confirm unit and integration tests pass
7. Merge or push to `development`; this runs unit and integration tests again, but does not deploy
8. Open a PR from `development` to `main`
9. Merge to `main` only after PR approval and passing unit and integration tests
10. When the `main` workflow reaches `Deploy To Azure App Service`, approve the production deployment
11. After deployment completes, the QA automation workflow is triggered against the deployed Azure app
12. After deployment completes, browse:
   `https://reviewportal-api-escdb3f2epg8eeha.southeastasia-01.azurewebsites.net`

## What to expect in GitHub Actions

The workflow has three jobs:

- `Unit Tests`
- `Integration Tests`
- `Publish API`
- `Deploy To Azure App Service`

`Deploy To Azure App Service` will only run if the unit and integration test jobs succeed.

Integration tests run on `windows-latest` because the integration test suite uses SQL Server LocalDB.

If required reviewers are configured on the `production` environment, the deploy job will sit in a waiting state until someone approves it.

## GitHub approval note

GitHub's current documentation says environment protection rules and required reviewers are available for all current plans on public repositories, but for private repositories they depend on plan level. If your repository is private and the required-reviewer option is missing, check your GitHub plan first.

## Common issues

### Deployment fails with publish profile or authorization errors

Check:

- `SCM Basic Auth Publishing Credentials` is on
- the full publish profile XML was copied into the GitHub secret
- the secret name is exactly `AZURE_WEBAPP_PUBLISH_PROFILE`

### App deploys but does not start

Check Azure configuration values:

- `Jwt__Secret`
- `Jwt__Issuer`
- `Jwt__Audience`
- `DefaultConnection` or `ConnectionStrings__DefaultConnection`

### API starts but frontend calls fail

Check your production CORS settings and add your frontend domain as `Cors__AllowedOrigins__0`, `Cors__AllowedOrigins__1`, and so on.

### Deploy job does not wait for approval

Check:

- the deploy job still references the `production` environment in `.github/workflows/ci.yml`
- the `production` environment exists in GitHub
- `Required reviewers` is enabled on that environment
- your repository plan supports environment protection rules for its visibility level

## Security note

This setup uses a publish profile because it is the quickest way to get your App Service deployed from GitHub Actions.

For a stronger production setup, the next step would be switching this workflow to Azure OpenID Connect or a service principal so you do not rely on publish-profile basic authentication.
