param(
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$apiProject = Join-Path $repoRoot "src\ReviewPortal.API\ReviewPortal.API.csproj"
$localSettings = Join-Path $repoRoot "src\ReviewPortal.API\appsettings.Local.json"
$exampleSettings = Join-Path $repoRoot "src\ReviewPortal.API\appsettings.Local.example.json"

if (-not (Test-Path $localSettings)) {
    Copy-Item -Path $exampleSettings -Destination $localSettings
    throw "Created src\ReviewPortal.API\appsettings.Local.json. Fill it with local secrets, then run this script again."
}

$localJson = Get-Content -Path $localSettings -Raw
if ($localJson -match "<[^>]+>") {
    throw "src\ReviewPortal.API\appsettings.Local.json still contains placeholders. Replace them with rotated local/Azure values before running the API."
}

$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --configuration $Configuration --project $apiProject
