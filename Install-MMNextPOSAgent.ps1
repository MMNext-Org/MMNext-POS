[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [string]$ProjectRoot = 'J:\Project 1\MMNext POS',
    [string]$PackageRoot = $PSScriptRoot
)

$ErrorActionPreference = 'Stop'
if (-not (Test-Path -LiteralPath $ProjectRoot)) {
    throw "Project folder not found: $ProjectRoot"
}

$agentsSource = Join-Path $PackageRoot '.github\agents'
$agentsTarget = Join-Path $ProjectRoot '.github\agents'
$toolsSource = Join-Path $PackageRoot 'tools'
$toolsTarget = Join-Path $ProjectRoot 'tools\mmnextpos-agent'
$configSource = Join-Path $PackageRoot 'config\nvidia.env.example'
$configTarget = Join-Path $ProjectRoot 'config\nvidia.env.example'

foreach ($directory in @($agentsTarget, $toolsTarget, (Split-Path $configTarget -Parent))) {
    New-Item -ItemType Directory -Force -Path $directory | Out-Null
}

Copy-Item -Path (Join-Path $agentsSource '*') -Destination $agentsTarget -Force
Copy-Item -Path (Join-Path $toolsSource '*') -Destination $toolsTarget -Force
Copy-Item -Path $configSource -Destination $configTarget -Force

Write-Host "Installed MMNextPOS custom agents in $agentsTarget"
Write-Host "Installed advisory router in $toolsTarget"
Write-Host "Next step: review the generated files, set NVIDIA_API_KEY as a user environment variable, then open the repository in VS Code/Copilot."
Write-Host "No code changes, builds, tests, database operations, or API calls were performed by this installer."
