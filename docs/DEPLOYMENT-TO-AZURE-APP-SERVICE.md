# Deploying To Azure App Service With GitHub Actions

This repository includes a GitHub Actions workflow at `.github/workflows/ci.yml` that:

1. runs unit tests first
2. only publishes and deploys on pushes to `main`
3. deploys the API to Azure App Service `reviewportal-api`

Production URL:

`https://reviewportal-api-escdb3f2epg8eeha.southeastasia-01.azurewebsites.net`

## How the workflow behaves

- Pull requests into `main`: run unit tests only
- Pushes to `main`: run unit tests, publish the API, then deploy to Azure App Service

## Recommended setup

This workflow is configured to use a publish profile secret named `AZURE_WEBAPP_PUBLISH_PROFILE`.

That is the simplest setup for your current app service, because your Azure portal already exposes the **Download publish profile** option.

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

### Option A: Recommended if available - environment secret

In GitHub:

1. Open your repository
2. Go to `Settings -> Environments`
3. Create an environment named `production`
4. In that environment, add a secret named `AZURE_WEBAPP_PUBLISH_PROFILE`
5. Paste the full contents of the downloaded publish profile
6. Under deployment branches, restrict the environment to branch `main`

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

`reviewportal-api -> Settings -> Configuration`

Add these values:

- `Jwt__Secret`
- `Jwt__Issuer`
- `Jwt__Audience`

For the database, use one of these approaches:

- Add a connection string named `DefaultConnection` in the `Connection strings` tab
- Or add an application setting named `ConnectionStrings__DefaultConnection`

Optional if you have a frontend calling this API:

- `Cors__AllowedOrigins__0`
- `Cors__AllowedOrigins__1`

Example values:

- `Jwt__Issuer = ReviewPortalAPI`
- `Jwt__Audience = ReviewPortalClient`
- `Cors__AllowedOrigins__0 = https://your-frontend-domain`

After saving configuration, restart the app service.

## First deployment steps

1. Commit and push this workflow to GitHub
2. Add the publish profile secret
3. Add Azure application settings and connection string
4. Push a commit to `main`
5. Open the `Actions` tab in GitHub and watch the `CI-CD` workflow
6. After deployment completes, browse:
   `https://reviewportal-api-escdb3f2epg8eeha.southeastasia-01.azurewebsites.net`

## What to expect in GitHub Actions

The workflow has three jobs:

- `Unit Tests`
- `Publish API`
- `Deploy To Azure App Service`

`Deploy To Azure App Service` will only run if the earlier jobs succeed.

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

## Security note

This setup uses a publish profile because it is the quickest way to get your App Service deployed from GitHub Actions.

For a stronger production setup, the next step would be switching this workflow to Azure OpenID Connect or a service principal so you do not rely on publish-profile basic authentication.
