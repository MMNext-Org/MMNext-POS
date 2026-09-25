# =====================================================================
# MMNextPOS Installer Build Script
# =====================================================================
# Publishes the WinForms app (self-contained win-x64), stages fonts and
# (optionally) the portable MySQL engine, then compiles the Inno Setup
# installer via ISCC.exe.
#
# Usage:
#   scripts\Build-Installer.ps1                          # app-only installer
#   scripts\Build-Installer.ps1 -SingleFile              # single-file publish
#   scripts\Build-Installer.ps1 -IncludeDatabase         # bundle portable MySQL
#
# Output:
#   artifacts\installer\app\       staged publish output
#   artifacts\installer\fonts\     staged Pyidaungsu fonts
#   artifacts\installer\MMNextPOS_Setup_v<Version>.exe
# =====================================================================
[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$AppVersion = "2.0.0",
    [switch]$SingleFile,
    [switch]$IncludeDatabase,
    [string]$InnoCompiler = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
    [string]$MySqlSourceDir = "J:\Project 1\FusionPOS\MMNextPOS\MySQL_Database"
)

$ErrorActionPreference = 'Stop'

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$Root      = Split-Path -Parent $ScriptDir
$Proj      = Join-Path $Root "src\MMNextPOS.WinForms\MMNextPOS.WinForms.csproj"
$Stage     = Join-Path $Root "artifacts\installer"
$AppStage  = Join-Path $Stage "app"
$FontStage = Join-Path $Stage "fonts"
$DbStage   = Join-Path $Stage "database"
$Iss       = Join-Path $Root "installer\MMNextPOS_Setup.iss"

if (-not (Test-Path $InnoCompiler)) { throw "Inno Setup not found at '$InnoCompiler'. Install Inno Setup 6 first." }
if (-not (Test-Path $Iss)) { throw "Inno script not found: $Iss" }

Write-Host "== [1/5] Publish ($Configuration / $Runtime / self-contained) ==" -ForegroundColor Cyan
$publishArgs = @(
    "publish", $Proj,
    "-c", $Configuration,
    "-r", $Runtime,
    "--self-contained", "true",
    "-o", $AppStage
)
if ($SingleFile) {
    $publishArgs += @("-p:PublishSingleFile=true", "-p:IncludeNativeLibrariesForSelfExtract=true")
}
dotnet @publishArgs
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed (exit $LASTEXITCODE)" }

$appExe = Join-Path $AppStage "MMNextPOS.WinForms.exe"
if (-not (Test-Path $appExe)) { throw "Publish output missing expected exe: $appExe" }

Write-Host "== [2/5] Stage Pyidaungsu fonts ==" -ForegroundColor Cyan
if (Test-Path $FontStage) { Remove-Item $FontStage -Recurse -Force }
New-Item -ItemType Directory -Path $FontStage -Force | Out-Null
Copy-Item (Join-Path $Root "Fonts\*.ttf") $FontStage -Force
Get-ChildItem $FontStage | ForEach-Object { Write-Host ("  + " + $_.Name) }

Write-Host "== [3/5] Optional portable MySQL staging ==" -ForegroundColor Cyan
if ($IncludeDatabase) {
    if (-not (Test-Path $MySqlSourceDir)) { throw "MySQL source not found: $MySqlSourceDir" }
    if (Test-Path $DbStage) { Remove-Item $DbStage -Recurse -Force }
    New-Item -ItemType Directory -Path $DbStage -Force | Out-Null
    Get-ChildItem $MySqlSourceDir -Exclude "*.zip" | ForEach-Object {
        Copy-Item $_.FullName $DbStage -Recurse -Force
    }
    Write-Host "  + portable MySQL staged from $MySqlSourceDir"
} else {
    Write-Host "  (skipped - pass -IncludeDatabase to bundle)"
}

Write-Host "== [4/5] Compile installer (Inno Setup) ==" -ForegroundColor Cyan
$isccArgs = @("/DAppVersion=$AppVersion", "/O`"$Stage`"")
if ($IncludeDatabase) { $isccArgs += "/DBundleMySQL" }
Push-Location (Join-Path $Root "installer")
try {
    & $InnoCompiler @isccArgs $Iss
    if ($LASTEXITCODE -ne 0) { throw "ISCC failed (exit $LASTEXITCODE)" }
} finally {
    Pop-Location
}

Write-Host "== [5/5] Verify artifact ==" -ForegroundColor Cyan
$setupExe = Join-Path $Stage "MMNextPOS_Setup_v$AppVersion.exe"
if (-not (Test-Path $setupExe)) { throw "Installer was not produced: $setupExe" }

$sizeMB = [Math]::Round((Get-Item $setupExe).Length / 1MB, 1)
Write-Host ""
Write-Host "SUCCESS: $setupExe ($sizeMB MB)" -ForegroundColor Green
