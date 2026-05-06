param(
    [string]$Migration = ""
)

$ErrorActionPreference = "Stop"
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$infrastructureProject = Join-Path $repoRoot "src\ReviewPortal.Infrastructure\ReviewPortal.Infrastructure.csproj"
$apiProject = Join-Path $repoRoot "src\ReviewPortal.API\ReviewPortal.API.csproj"
$localSettings = Join-Path $repoRoot "src\ReviewPortal.API\appsettings.Local.json"
$exampleSettings = Join-Path $repoRoot "src\ReviewPortal.API\appsettings.Local.example.json"

if (-not (Test-Path $localSettings)) {
    Copy-Item -Path $exampleSettings -Destination $localSettings
    throw "Created src\ReviewPortal.API\appsettings.Local.json. Fill it with rotated Azure SQL credentials, then run this script again."
}

$localJson = Get-Content -Path $localSettings -Raw
if ($localJson -match "<[^>]+>") {
    throw "src\ReviewPortal.API\appsettings.Local.json still contains placeholders. Replace them with rotated Azure SQL credentials before running migrations."
}

$env:ASPNETCORE_ENVIRONMENT = "Development"

$arguments = @(
    "ef",
    "database",
    "update"
)

if (-not [string]::IsNullOrWhiteSpace($Migration)) {
    $arguments += $Migration
}

$arguments += @(
    "--project",
    $infrastructureProject,
    "--startup-project",
    $apiProject
)

dotnet @arguments
