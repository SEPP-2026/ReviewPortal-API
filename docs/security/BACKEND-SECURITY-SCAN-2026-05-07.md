# Backend Security Scan and Fix Backlog

Date: 2026-05-07
Scope: `ReviewPortal-API` backend only, including API, Application, Domain, Infrastructure, migrations/scripts, CI, and backend tests.

This is a comprehensive pre-scan review, but no security review can honestly prove "100% secure". Treat this file as the ordered backlog to harden the backend before the external security scan.

## Scan Commands Run

| Check | Result |
|------|--------|
| `./scripts/security/scan-secrets.ps1` | Failed because the scanner matches its own pattern definitions in `scripts/security/scan-secrets.ps1` lines 19 and 23. This is a scanner false positive, but it will still fail CI. |
| Tracked-file secret keyword search | No tracked appsettings file currently contains a real Azure SQL password or JWT secret. `src/ReviewPortal.API/appsettings.Local.json` is ignored by git and currently contains placeholders. |
| `dotnet list ReviewPortal.slnx package --vulnerable --include-transitive` | Blocked by private Azure Artifacts feed authentication: HTTP 401. This must be fixed for repeatable CI scanning. |
| `dotnet list ReviewPortal.slnx package --vulnerable --include-transitive --source https://api.nuget.org/v3/index.json` | Found vulnerable transitive packages. See SEC-03. |
| `dotnet list ReviewPortal.slnx package --deprecated --source https://api.nuget.org/v3/index.json` | Found deprecated packages. See SEC-04. |
| API/controller security review | Admin routes have role attributes. Public mutation routes need rate limiting and abuse protection. |
| Config/hosting review | Swagger is always enabled, HSTS/security headers are missing, `AllowedHosts` is `*`, and production CORS must be explicitly checked. |
| Upload review | Uploads use generated filenames and extension/size checks, but should add file signature/content scanning and security headers. |

## Current Good Controls

- Database and JWT secrets have been removed from tracked `appsettings.json` and `appsettings.Development.json`.
- `appsettings.Local.json` is ignored by git.
- Startup fails when required connection string/JWT settings are empty or placeholder values.
- SQL access is through EF Core repositories; no `FromSqlRaw` or `ExecuteSqlRaw` usage found in application code.
- Passwords are hashed with ASP.NET Core `PasswordHasher<TUser>` compatible hashing.
- Admin endpoints are protected with `[Authorize(Roles = "Admin")]` or `[Authorize(Roles = "Admin,Moderator")]`.
- JWT validation checks issuer, audience, lifetime, and signing key.
- Uploaded image filenames are generated server-side; user file names are not used for storage paths.
- Review rating values have database check constraints.

## Priority Fix Backlog

### SEC-01: Rotate leaked/known Azure SQL credentials and verify production is clean

Priority: Critical
Area: Secrets, Azure SQL

The repo no longer has the real Azure SQL password in tracked appsettings, but previous docs/tasks show that a real password was exposed before. Rotation is still marked as required in `docs/agile/IMPLEMENTATION-TASKS.md`.

Tasks:
- Rotate the Azure SQL password in Azure.
- Update Azure App Service connection string with the rotated value.
- Update local ignored `appsettings.Local.json` or user secrets with the rotated value only if needed for migrations.
- Confirm the old password no longer authenticates.
- Ensure Azure SQL firewall only allows required IP ranges/App Service access.

Acceptance:
- `TASK-24.2` can be marked complete.
- Local and Azure migrations still run with the rotated secret.
- No tracked file contains the old secret or any real password.

### SEC-02: Fix committed-secret scanner false positives before CI security scan

Priority: Critical
Area: CI, secret scanning

`scripts/security/scan-secrets.ps1` currently detects its own signature definitions:

- `scripts/security/scan-secrets.ps1:19`
- `scripts/security/scan-secrets.ps1:23`

This means the CI "Scan for committed secrets" step fails even when app config is clean.

Tasks:
- Refactor scanner patterns so literal blocked secrets are encoded or loaded without matching themselves.
- Add an allowlist/exclusion only for pattern definition lines, not for app/config/docs.
- Keep scanning tracked files via `git ls-files`.
- Add a small test or documented dry run proving a sample secret fails and placeholders pass.

Acceptance:
- `./scripts/security/scan-secrets.ps1` exits `0` on the current repo.
- A temporary test file containing a literal password still makes the scanner fail.
- CI secret scan passes before restore/build/test.

### SEC-03: Update vulnerable NuGet transitive packages

Priority: High
Area: Dependencies

NuGet.org vulnerability scan found:

| Project | Package | Version | Severity |
|--------|---------|---------|----------|
| `ReviewPortal.API` | `Microsoft.IdentityModel.JsonWebTokens` | `7.0.3` | Moderate |
| `ReviewPortal.API` | `System.IdentityModel.Tokens.Jwt` | `7.0.3` | Moderate |
| `ReviewPortal.IntegrationTests` | `Microsoft.IdentityModel.JsonWebTokens` | `7.0.3` | Moderate |
| `ReviewPortal.IntegrationTests` | `System.IdentityModel.Tokens.Jwt` | `7.0.3` | Moderate |
| `ReviewPortal.IntegrationTests` | `System.Net.Http` | `4.3.0` | High |
| `ReviewPortal.IntegrationTests` | `System.Text.RegularExpressions` | `4.3.0` | High |
| `ReviewPortal.UnitTests` | `System.Net.Http` | `4.3.0` | High |
| `ReviewPortal.UnitTests` | `System.Text.RegularExpressions` | `4.3.0` | High |

Likely root causes:
- `src/ReviewPortal.API/ReviewPortal.API.csproj` pins `Microsoft.AspNetCore.Authentication.JwtBearer` to `8.0.0`, pulling older IdentityModel packages.
- Older test packages pull legacy `NETStandard.Library` transitive packages.

Tasks:
- Update `Microsoft.AspNetCore.Authentication.JwtBearer` to the same patched .NET 8 servicing level used by the rest of the stack.
- Update `Microsoft.EntityFrameworkCore.*` packages together to the selected patched 8.0.x level.
- Update test packages: `Microsoft.NET.Test.Sdk`, `coverlet.collector`, `xunit`, and `xunit.runner.visualstudio`.
- Re-run vulnerability scan using NuGet.org and the normal repo NuGet config.

Acceptance:
- `dotnet list ReviewPortal.slnx package --vulnerable --include-transitive --source https://api.nuget.org/v3/index.json` reports no vulnerable packages.
- `dotnet build ReviewPortal.slnx` passes.
- `dotnet test ReviewPortal.slnx` passes.

### SEC-04: Remove or isolate deprecated packages

Priority: Medium
Area: Dependencies

Deprecated scan found:

- `FluentValidation.AspNetCore 11.3.1` is marked legacy.
- `xunit 2.5.3` is marked legacy with `xunit.v3` as an alternative.

Tasks:
- Replace `FluentValidation.AspNetCore` auto-validation with a supported FluentValidation integration approach or explicit validation pipeline.
- Decide whether to upgrade to xUnit v3 now or defer until after the MSc submission if package churn is too risky.

Acceptance:
- Deprecated package scan is clean, or a documented exception exists for xUnit until after submission.

### SEC-05: Remove production risk from seeded demo users and known passwords

Priority: High
Area: Authentication, database seed data

The project seeds predictable demo users and documents their passwords in `README.md` and SQL scripts:

- Customer test user
- Admin test user
- Moderator test user

This is acceptable for local demos only, but dangerous if migrations/seed scripts are applied to Azure production.

Tasks:
- Decide whether production Azure should contain seeded customer/admin/moderator users.
- If not, create a migration or production cleanup script to remove seeded users from production.
- Move demo/test user seed data behind a development/testing-only path.
- Remove documented reusable admin/moderator passwords from production-facing docs, or clearly mark as local demo only.
- Create an admin bootstrap process using one-time secret or manual Azure SQL/admin process.

Acceptance:
- Production database has no known-password admin/moderator accounts.
- Local/integration tests still have deterministic test users.
- Docs clearly separate local demo credentials from production setup.

### SEC-06: Stop returning password reset tokens from public API in production

Priority: High
Area: Authentication

`ForgotPasswordAsync` returns the reset token in the response because email delivery is not configured. This is useful for development, but not safe for production.

Tasks:
- Add email delivery or a mockable notification service.
- In Production, never return `ResetToken` in `ForgotPasswordResponse`.
- Return a generic response for both existing and non-existing email addresses.
- Keep token return enabled only in Testing/Development if needed for tests.
- Add integration tests for production behavior.

Acceptance:
- Production forgot-password response never contains a reset token.
- Reset flow still works through the configured notification path.
- Tests cover Development/Testing and Production behavior separately.

### SEC-07: Add rate limiting and brute-force protection

Priority: High
Area: Auth, public API abuse protection

Public mutation endpoints can currently be called repeatedly without rate limits:

- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/forgot-password`
- `POST /api/auth/reset-password`
- `POST /api/tools/{toolId}/reviews`
- `POST /api/reviews/{reviewId}/comments`
- `POST /api/tools/{id}/rental-calculation`

Tasks:
- Add ASP.NET Core rate limiting policies.
- Use stricter limits for auth endpoints than catalogue reads.
- Add failed-login tracking or account lockout in the custom auth flow.
- Consider CAPTCHA or moderation-abuse throttling for anonymous reviews/comments.
- Add integration tests for `429 Too Many Requests`.

Acceptance:
- Repeated login/reset attempts are throttled.
- Review/comment spam is throttled.
- Normal integration tests are not flaky.

### SEC-08: Gate Swagger UI outside Development

Priority: High
Area: API exposure

`Program.cs` currently enables Swagger and Swagger UI unconditionally.

Tasks:
- Enable Swagger only in Development/Testing, or protect it behind Admin auth in production.
- Confirm Azure App Service production environment does not expose Swagger.
- Add a lightweight integration or smoke check for environment-specific Swagger behavior.

Acceptance:
- Production `/swagger` is unavailable or requires admin authorization.
- Development Swagger still works locally.

### SEC-09: Add production security headers and HSTS

Priority: Medium
Area: HTTP hardening

The API uses HTTPS redirection but does not currently add HSTS or common security headers.

Tasks:
- Add `app.UseHsts()` outside Development.
- Add headers such as:
  - `X-Content-Type-Options: nosniff`
  - `X-Frame-Options: DENY`
  - `Referrer-Policy: no-referrer`
  - `Permissions-Policy` with no unnecessary browser permissions
  - a minimal Content Security Policy for Swagger/static content if Swagger remains exposed
- Add tests or documented verification using `curl -I`.

Acceptance:
- Production responses include the expected headers.
- Uploaded/static images are still served correctly.

### SEC-10: Lock down hosts and CORS for Azure/Next.js

Priority: Medium
Area: Hosting configuration

`AllowedHosts` is currently `*`. CORS defaults to localhost only, but Azure production must be configured explicitly for the deployed Next.js app.

Tasks:
- Set production `AllowedHosts` to the API host names.
- Configure Azure `Cors__AllowedOrigins__0` etc. to exact Next.js frontend origins.
- Keep localhost origins only for Development.
- Add a deployment checklist item to verify preflight requests.

Acceptance:
- Production does not allow arbitrary Host headers.
- Production CORS allows only the approved frontend origin(s).
- Local development still works with `localhost:3000`.

### SEC-11: Harden image upload validation

Priority: Medium
Area: File upload

The image service checks extension and size, and stores generated filenames. It should also validate file content.

Status update (2026-05-09): TASK-31 added JPEG/PNG/WebP file-signature validation before blob upload and tests extension/content mismatch handling.

Tasks:
- Verify JPEG/PNG/WEBP file signatures, not only extension.
- Check `IFormFile.ContentType` as a weak signal, but do not trust it alone.
- Consider malware scanning or Azure Blob Storage with Defender scanning for production.
- Ensure uploaded files are served with safe content types and `nosniff`.
- Add tests for extension/content mismatch.

Acceptance:
- A `.jpg` file with non-image content is rejected.
- Real small JPG/PNG/WEBP files still upload.
- Delete and last-image protections still pass.

### SEC-12: Add dependency/security scans to CI properly

Priority: Medium
Area: CI/CD

Current CI has restore/build/unit tests/publish/deploy. It does not run dependency vulnerability checks, integration tests, CodeQL, or a working secret scan.

Tasks:
- Fix secret scan first.
- Add `dotnet list package --vulnerable --include-transitive`.
- Fix private Azure Artifacts feed authentication or ensure CI uses the intended NuGet source.
- Add CodeQL or equivalent static analysis.
- Publish test results and optionally coverage.
- Consider failing PRs on High/Critical package advisories.

Acceptance:
- PR CI runs secret scan, vulnerability scan, build, unit tests, and preferably integration tests.
- Security scan result is reproducible locally and in CI.

### SEC-13: Add security-focused integration tests

Priority: Medium
Area: Test automation

TASK-28 expanded admin API integration coverage, but the project still needs security-specific tests.

Tasks:
- Unauthenticated admin endpoints return `401`.
- Customer token on admin endpoints returns `403`.
- Swagger production behavior is tested.
- Rate limiting returns `429`.
- Forgot-password production response does not include token.
- Security headers are present in production-like environment.
- CORS preflight allows approved origin and rejects unapproved origin.

Acceptance:
- New security tests pass locally and in CI.

### SEC-14: Review JWT lifecycle and revocation behavior

Priority: Medium
Area: Authentication

JWTs are stateless and remain valid until expiry. Password change/reset does not invalidate already-issued tokens.

Tasks:
- Add a user `SecurityStamp` or `TokenVersion`.
- Include it in JWT claims.
- Reject tokens when the stored version changes after password reset/change.
- Consider shorter access-token lifetime and refresh tokens only if the frontend needs long sessions.

Acceptance:
- Old tokens stop working after password change/reset.
- Existing login flow remains compatible with Next.js.

### SEC-15: Move production file storage to Azure Blob Storage

Priority: Low/Medium
Area: File storage, operations

Local/App Service filesystem uploads can work for demos, but production is safer with Blob Storage.

Status update (2026-05-09): TASK-31 replaced local filesystem upload/delete behavior with Azure Blob Storage and documented Azure Portal setup in `docs/azure-blob-storage/README.md`.

Tasks:
- Add an `IFileStorageService` abstraction.
- Implement local filesystem for Development/Testing.
- Implement Azure Blob Storage for Production.
- Store only public blob URLs or relative paths in `ToolImages`.
- Configure storage account access policy and content types.

Acceptance:
- Upload/delete tests pass for local implementation.
- Azure implementation is documented and configurable without committed secrets.

## Suggested Implementation Sequence

1. SEC-01: rotate Azure SQL credentials.
2. SEC-02: fix committed-secret scanner false positives.
3. SEC-03: update vulnerable packages.
4. SEC-05: remove production seeded known-password accounts.
5. SEC-06: stop returning reset tokens in production.
6. SEC-07: add rate limiting/account lockout.
7. SEC-08 and SEC-09: lock down Swagger and add security headers/HSTS.
8. SEC-10: production CORS/AllowedHosts hardening.
9. SEC-11: image upload content validation.
10. SEC-12 and SEC-13: CI security scans and security integration tests.
11. SEC-14 and SEC-15: token revocation and Blob Storage hardening.

## Notes For The Next Security Scan

- The scanner failure in the screenshot is real as a CI failure, but it is not proof that appsettings still contains the leaked password. It is matching the scanner's own pattern text.
- The normal `dotnet list ... --vulnerable` command currently fails because a private Azure Artifacts feed returns `401`. Use a source override temporarily, but fix the feed/config for CI.
- Do not run production migrations again until SEC-01 and SEC-05 are decided, because migrations/SQL scripts can seed known demo users.
- Keep the current custom JWT auth stack unless the brief explicitly requires full ASP.NET Identity. The security work above can be done without migrating the whole auth model.
