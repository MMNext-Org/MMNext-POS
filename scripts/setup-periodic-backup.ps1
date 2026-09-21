# MMNext POS - Automatic Backup Task Scheduler Setup
# Run this as Administrator to create a Windows Task Scheduler job for daily backups

param(
    [string]$BackupDir = "C:\MMNextPOS\Backups",
    [string]$AppPath = "J:\Project 1\MMNext POS\src\MMNextPOS.WinForms\bin\Release\net8.0-windows\MMNextPOS.WinForms.exe",
    [string]$DatabaseConnectionString = "",
    [switch]$RegisterTask,
    [switch]$UnregisterTask
)

# Settings
$TaskName = "MMNextPOS Daily Backup"
$TaskDescription = "Automated daily backup of MMNext POS database and application files"
$BackupScriptPath = Join-Path $PSScriptRoot "run-backup.ps1"

# Logging function
function Write-Log
{
    param([string]$Message, [string]$Level = "INFO")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "[$timestamp] $Level : $Message"
    Write-Host "$timestamp | $Level | $Message" -OutFile "$BackupDir\backup.log" -Append
}

# If just showing status, exit early
if (-not $RegisterTask -and -not $UnregisterTask)
{
    Write-Host "=== MMNext POS Schedule Setup ===" -ForegroundColor Cyan
    Write-Host "Use -RegisterTask to create the backup task" -ForegroundColor Yellow
    Write-Host "Use -UnregisterTask to remove the backup task" -ForegroundColor Yellow
    Write-Host "Use -DatabaseConnectionString for custom connection"
    exit 0
}

# Unregister existing task
if ($UnregisterTask)
{
    Write-Log "Unregistering existing task..."
    schtasks.exe /Delete /TN $TaskName /F 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0)
    {
        Write-Log "SUCCESS: Task '$TaskName' removed successfully" "PASS"
    }
    else
    {
        Write-Log "Note: Task might not exist or removal failed (code $LASTEXITCODE)" "WARN"
    }
    exit 0
}

# Register task
if ($RegisterTask)
{
    Write-Log "Starting task registration..."

    # Validate inputs
    if (-not (Test-Path $AppPath))
    {
        Write-Log "ERROR: Application path not found: $AppPath" "ERROR"
        exit 1
    }

    if (-not (Test-Path $BackupDir))
    {
        Write-Log "Creating backup directory: $BackupDir"
        New-Item -ItemType Directory -Path $BackupDir -Force | Out-Null
    }

    # Build run command
    $runCommand = "`"$AppPath`" --backup --output `"$BackupDir`""
    
    Write-Log "Task command: $runCommand"

    # Create backup script wrapper
    $wrapperScript = @"
@echo off
powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "%~dp0backup-monitor.ps1"
"@
    Set-Content -Path $BackupScriptPath -Value $wrapperScript

    # Register with schtasks
    $action = "`"$AppPath`" --backup --output `"$BackupDir`""

    $schArgs = @(
        "/Create",
        "/TN", $TaskName,
        "/SC", "DAILY",
        "/ST", "02:00",
        "/RL", "HIGHEST",
        "/RA", "Administrator",
        "/DU", "SYSTEM",
        "/TR", $runCommand,
        "/F"
    )

    Write-Log "Running: schtasks.exe $schArgs"
    $result = & schtasks.exe $schArgs 2>&1

    if ($LASTEXITCODE -eq 0 -or $result -match "SUCCESS")
    {
        Write-Log "SUCCESS: Task '$TaskName' registered successfully" "PASS"
        Write-Log "Backup directory: $BackupDir"
        Write-Log "Scheduled time: Daily at 02:00"
        Write-Log "Next run: Tomorrow at 02:00"
        Write-Log "To edit: Get-Service '$TaskName' | Set-ScheduledTask -Trigger"
    }
    else
    {
        Write-Log "FAILED: Could not register task (exit code $LASTEXITCODE)" "ERROR"
        Write-Log $result
        exit 1
    }
}
