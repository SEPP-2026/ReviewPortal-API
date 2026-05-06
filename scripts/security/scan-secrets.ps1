$ErrorActionPreference = "Stop"

$repoRoot = (git rev-parse --show-toplevel).Trim()
if ([string]::IsNullOrWhiteSpace($repoRoot)) {
    throw "Run this script from inside the git repository."
}

$findings = New-Object System.Collections.Generic.List[string]
$trackedFiles = git -C $repoRoot ls-files

$binaryExtensions = @(
    ".dll", ".exe", ".pdb", ".png", ".jpg", ".jpeg", ".webp", ".gif", ".ico",
    ".zip", ".7z", ".nupkg", ".snk", ".pfx", ".pdf", ".xlsx", ".pptx"
)

$patterns = @(
    @{
        Name = "Known leaked Azure SQL password"
        Regex = "Chamarairesh@2026"
    },
    @{
        Name = "Connection string contains a literal password"
        Regex = "(?i)Password=(?!<|\$\{|\$\(|%|__)[^;`"'\s]{8,}"
    },
    @{
        Name = "JSON JWT secret contains a literal value"
        Regex = '(?i)"Secret"\s*:\s*"(?!\s*"|<|__|\$\{)[^"]{16,}"'
    }
)

foreach ($relativePath in $trackedFiles) {
    $fullPath = Join-Path $repoRoot $relativePath
    $extension = [System.IO.Path]::GetExtension($fullPath)

    if ($binaryExtensions -contains $extension.ToLowerInvariant()) {
        continue
    }

    if (-not (Test-Path $fullPath)) {
        continue
    }

    $lineNumber = 0
    foreach ($line in Get-Content -Path $fullPath) {
        $lineNumber++

        foreach ($pattern in $patterns) {
            if ($line -match $pattern.Regex) {
                $findings.Add("${relativePath}:$lineNumber [$($pattern.Name)]")
            }
        }
    }
}

if ($findings.Count -gt 0) {
    Write-Host "Potential committed secrets found:" -ForegroundColor Red
    $findings | ForEach-Object { Write-Host " - $_" -ForegroundColor Red }
    exit 1
}

Write-Host "Secret scan passed: no high-risk committed secrets matched." -ForegroundColor Green
