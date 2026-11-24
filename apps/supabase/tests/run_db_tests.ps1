<#
.SYNOPSIS
  Runs the pgTAP authorization test suite against a clean local PostgreSQL database.

  This is the backend for `npm run test:db`. It:
    1. Creates a clean test database (drop + create).
    2. Loads pgTAP inline (pure PL/pgSQL, no CREATE EXTENSION needed).
    3. Applies schema.sql + role migrations + authorization migration.
    4. Loads two-organization/two-branch fixtures.
    5. Runs the authorization pgTAP suite.
  Exits nonzero if any SQL errors occur or any pgTAP assertion fails.
#>
param(
  [string]$PgHost = 'localhost',
  [int]$PgPort    = 5432,
  [string]$PgUser = 'postgres',
  [string]$PgDb   = 'pos_test',
  [string]$Psql   = 'C:\Program Files\PostgreSQL\17\bin\psql.exe'
)

$ErrorActionPreference = 'Stop'
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$SupabaseDir = Split-Path -Parent $ScriptDir          # apps\supabase
$AppsDir     = Split-Path -Parent $SupabaseDir        # apps
$RepoRoot    = Split-Path -Parent $AppsDir            # repository root
$Pgtap       = Join-Path $RepoRoot '.dbtest\scratch\pgtap\generated\pgtap.sql'

if (-not (Test-Path $Psql)) {
  Write-Error "psql not found at $Psql (set Psql param or PSQL_PATH env)"
  exit 1
}
if (-not (Test-Path $Pgtap)) {
  Write-Error "pgTAP SQL not found at $Pgtap"
  exit 1
}

$baseArgs = @("-h", $PgHost, "-p", ([string]$PgPort), "-U", $PgUser, "-v", "ON_ERROR_STOP=1")

function Run-Psql($database, [string[]]$files) {
  $args = @($baseArgs) + @("-d", $database)
  foreach ($f in $files) { $args += @("-f", $f) }
  # Temporarily allow non-terminating errors so PostgreSQL NOTICEs on stderr
  # (e.g. "relation already exists, skipping") do not abort the run.
  $oldPref = $ErrorActionPreference
  $ErrorActionPreference = 'Continue'
  $out = & $Psql @args 2>&1
  $ErrorActionPreference = $oldPref
  return $out
}

Write-Host "Creating clean test database $PgDb ..."
$oldPref = $ErrorActionPreference
$ErrorActionPreference = 'Continue'
$out = & $Psql @baseArgs @("-d", "postgres",
  "-c", "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '$PgDb' AND pid <> pg_backend_pid();",
  "-c", "DROP DATABASE IF EXISTS ""$PgDb"";",
  "-c", "CREATE DATABASE ""$PgDb"";") 2>&1
$ErrorActionPreference = $oldPref
if ($LASTEXITCODE -ne 0) {
  $outStr = if ($out -is [array]) { $out -join "`n" } else { [string]$out }
  Write-Error "Failed to create test database:`n$outStr"
  exit 1
}

$setupFiles = @(
  $Pgtap                                 # pgTAP library: loaded once into the DB
  Join-Path $SupabaseDir 'schema.sql'
  Join-Path $SupabaseDir 'migrations\002_roles_schema.sql'
  Join-Path $SupabaseDir 'migrations\010_authorization.sql'
  Join-Path $SupabaseDir 'migrations\011_loyalty_earn_rewards.sql'
  Join-Path $SupabaseDir 'migrations\012_loyalty_enroll_disenroll.sql'
  Join-Path $SupabaseDir 'migrations\013_loyalty_redemption.sql'
  Join-Path $ScriptDir 'fixtures.sql'
)
# Each test file declares its own plan(); pgTAP allows only one plan per psql
# session, so suites run in separate psql sessions (pgTAP functions persist
# in the DB after setup, so they need not be reloaded per suite).
$testFiles = @(
  Join-Path $ScriptDir 'authorization_test.sql'
  Join-Path $ScriptDir 'loyalty_schema_test.sql'
  Join-Path $ScriptDir 'loyalty_earn_test.sql'
  Join-Path $ScriptDir 'loyalty_lifecycle_test.sql'
  Join-Path $ScriptDir 'loyalty_redemption_test.sql'
)

Write-Host "Applying schema + migrations + fixtures ..."
$setupOut = Run-Psql $PgDb $setupFiles 2>&1
$setupExit = $LASTEXITCODE
if ($setupExit -ne 0) {
  Write-Error "Setup (schema/migrations/fixtures) failed:`n$setupOut"
  exit 1
}

Write-Host "Running pgTAP test suites ..."
# Run each test file in its own psql session so every suite can declare its own
# plan() (pgTAP allows only one plan per session). Keeps suites independent and
# scalable as more test files are added.
$failed = $false
$allOut = @()
foreach ($tf in $testFiles) {
  Write-Host "  -> $(Split-Path $tf -Leaf)"
  $out = Run-Psql $PgDb @($tf) 2>&1
  $exit = $LASTEXITCODE
  $allOut += $out
  if ($exit -ne 0) { $failed = $true }
  if ($out -match 'not ok' -or $out -match 'looks like FAILED') { $failed = $true }
}

$testOut = $allOut -join "`n"
Write-Host $testOut
if ($failed) {
  Write-Host "DB TESTS FAILED" -ForegroundColor Red
  exit 1
}

Write-Host "DB TESTS PASSED" -ForegroundColor Green
exit 0
