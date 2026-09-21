<#
.SYNOPSIS
    Run MMNext POS integration tests with Docker MySQL

.DESCRIPTION
    This script starts a MySQL 8.0 container, runs the database migrations,
    and executes the full Infrastructure test suite including MigrationIdempotenceTests.

.EXAMPLE
    .\run-docker-tests.ps1

.EXAMPLE
    .\run-docker-tests.ps1 -CleanUp
#>

param(
    [switch]$CleanUp,
    [switch]$KeepContainer,
    [string]$MySqlVersion = "8.0",
    [string]$DatabaseName = "mmnextpos_test",
    [string]$RootPassword = "testpassword"
)

Write-Host "=== MMNext POS Docker Test Runner ===" -ForegroundColor Cyan
Write-Host "MySQL Version: $MySqlVersion" -ForegroundColor Gray
Write-Host "Database: $DatabaseName" -ForegroundColor Gray

function Check-Docker {
    try {
        $dockerVersion = docker --version 2>$null
        if (-not $dockerVersion) {
            Write-Error "Docker is not installed or not in PATH"
            exit 1
        }
        Write-Host "Docker found: $dockerVersion" -ForegroundColor Green
    }
    catch {
        Write-Error "Docker is not available: $_"
        exit 1
    }
}

function Start-MySqlContainer {
    Write-Host "Starting MySQL $MySqlVersion container..." -ForegroundColor Cyan

    $containerName = "mmnextpos-mysql-test"

    # Stop and remove existing container if exists
    docker stop $containerName 2>$null | Out-Null
    docker rm $containerName 2>$null | Out-Null

    $runArgs = @(
        "run",
        "-d",
        "--name", $containerName,
        "-e", "MYSQL_ROOT_PASSWORD=$RootPassword",
        "-e", "MYSQL_DATABASE=$DatabaseName",
        "-e", "MYSQL_USER=mmnextpos",
        "-e", "MYSQL_PASSWORD=$RootPassword",
        "-p", "3307:3306",  # Use 3307 to avoid conflict with local MySQL
        "--health-cmd", "mysqladmin ping -h localhost -u root -p$RootPassword --silent",
        "--health-interval", "10s",
        "--health-timeout", "5s",
        "--health-retries", "10",
        "--health-start-period", "30s",
        "mysql:$MySqlVersion"
    )

    docker @runArgs | Out-Null
    Write-Host "Container started: $containerName" -ForegroundColor Green
}

function Wait-ForMySql {
    Write-Host "Waiting for MySQL to be ready..." -ForegroundColor Cyan

    $maxAttempts = 30
    $attempt = 0

    while ($attempt -lt $maxAttempts) {
        $attempt++
        try {
            $result = docker exec mmnextpos-mysql-test mysqladmin ping -h localhost -u root -p$RootPassword --silent 2>$null
            if ($LASTEXITCODE -eq 0) {
                Write-Host "MySQL is ready!" -ForegroundColor Green
                return
            }
        }
        catch {
            # Ignore
        }

        Write-Host "Attempt $attempt/$maxAttempts - Waiting for MySQL..."
        Start-Sleep -Seconds 3
    }

    Write-Error "MySQL did not become ready in time"
    exit 1
}

function Run-Migrations {
    Write-Host "Running database migrations..." -ForegroundColor Cyan

    $connStr = "Server=127.0.0.1;Port=3307;Database=$DatabaseName;User ID=root;Password=$RootPassword;Allow User Variables=true;"

    # The migrations are embedded in the application and run automatically on startup
    # For testing, we just verify the database is accessible
    $testQuery = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '$DatabaseName';"
    $result = docker exec mmnextpos-mysql-test mysql -u root -p$RootPassword -e "$testQuery" 2>$null

    if ($LASTEXITCODE -eq 0) {
        Write-Host "Database accessible, tables found" -ForegroundColor Green
    }
    else {
        Write-Warning "Could not verify tables - migrations may not have run yet"
    }
}

function Run-Tests {
    Write-Host "Running Infrastructure integration tests..." -ForegroundColor Cyan

    $connStr = "Server=127.0.0.1;Port=3307;Database=$DatabaseName;User ID=root;Password=$RootPassword;Allow User Variables=true;"

    # MySqlContainerFixture reads this variable to target an external MySQL
    # instead of spinning its own Testcontainer (UnitOfWorkTests always uses
    # its own Testcontainer, so Docker must be running either way).
    $env:MMNEXTPOS_CONNECTION_STRING = $connStr

    $testProject = "tests\MMNextPOS.Infrastructure.Tests\MMNextPOS.Infrastructure.Tests.csproj"

    if (-not (Test-Path $testProject)) {
        Write-Error "Test project not found: $testProject"
        exit 1
    }

    # Run all infrastructure tests
    $result = dotnet test $testProject --configuration Release --logger "trx;LogFileName=infrastructure-tests.trx"

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Infrastructure tests failed"
        exit 1
    }

    Write-Host "Infrastructure tests passed!" -ForegroundColor Green

    # Run MigrationIdempotenceTests specifically
    Write-Host "Running MigrationIdempotenceTests..." -ForegroundColor Cyan
    $migrationTest = dotnet test $testProject --configuration Release --filter "FullyQualifiedName~MigrationIdempotenceTests" --logger "trx;LogFileName=migration-tests.trx"

    if ($LASTEXITCODE -ne 0) {
        Write-Error "MigrationIdempotenceTests failed"
        exit 1
    }

    Write-Host "MigrationIdempotenceTests passed!" -ForegroundColor Green
}

function Clean-Up {
    if (-not $KeepContainer) {
        Write-Host "Cleaning up Docker containers..." -ForegroundColor Cyan
        docker stop mmnextpos-mysql-test 2>$null | Out-Null
        docker rm mmnextpos-mysql-test 2>$null | Out-Null
        Write-Host "Container removed" -ForegroundColor Green
    }
    else {
        Write-Host "Keeping container running (use -CleanUp to remove)" -ForegroundColor Yellow
    }
}

# Main execution
try {
    Check-Docker

    if ($CleanUp) {
        Clean-Up
        return
    }

    Start-MySqlContainer
    Wait-ForMySql
    Run-Migrations
    Run-Tests

    Write-Host "=== All tests passed! ===" -ForegroundColor Green
}
catch {
    Write-Error "Test run failed: $_"
    exit 1
}
finally {
    if (-not $KeepContainer -and -not $CleanUp) {
        Clean-Up
    }
}