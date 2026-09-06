[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Task,
    [string]$Context = "",
    [string]$Python = "python"
)

$ErrorActionPreference = "Stop"
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$router = Join-Path $scriptRoot "nvidia_router.py"

if (-not $env:NVIDIA_API_KEY -and -not $env:NGC_API_KEY) {
    throw "Set NVIDIA_API_KEY or NGC_API_KEY in the user environment. Do not place the key in this script."
}

& $Python $router $Task --context $Context
if ($LASTEXITCODE -ne 0) {
    throw "The NVIDIA advisory router failed with exit code $LASTEXITCODE."
}
