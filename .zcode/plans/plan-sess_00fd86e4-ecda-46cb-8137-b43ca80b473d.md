# Priority 1: Database Schema & Migration Idempotence — Implementation Plan

## Understanding of Current State
- **ORM**: Dapper + MySqlConnector (no EF Core)
- **Schema creation**: `DatabaseInitializer.InitializeAsync()` runs at app startup via singleton registration
- **Idempotence**: `CREATE TABLE IF NOT EXISTS` + `INSERT IGNORE` + `EnsureSalesColumnsAsync()` for 2 known columns
- **Migration tracking**: NONE — no version table, no ordered scripts, no rollback

## Implementation Tasks

### 1. Add Schema Version Table (`SchemaVersions`)
Create a new table to track applied schema migrations:
```sql
CREATE TABLE IF NOT EXISTS SchemaVersions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Version VARCHAR(50) NOT NULL UNIQUE,      -- e.g., "2026.09.06.01"
    Description VARCHAR(500) NOT NULL,
    AppliedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    AppliedBy VARCHAR(100) NOT NULL,          -- "DatabaseInitializer" or migration tool
    Checksum VARCHAR(64) NULL,                -- SHA256 of migration script for drift detection
    Success BOOLEAN DEFAULT 1,
    ErrorMessage VARCHAR(2000) NULL
) ENGINE=InnoDB;
```

### 2. Refactor DatabaseInitializer into Ordered Migration Scripts
Split the monolithic SQL into numbered migration files:
- `Migrations/001_InitialSchema.sql` — all CREATE TABLE statements
- `Migrations/002_SeedDefaultData.sql` — INSERT IGNORE seed data
- `Migrations/003_AddSalesColumns.sql` — Status, LocationId columns (from EnsureSalesColumnsAsync)
- `Migrations/004_AddIndexes.sql` — missing indexes (see below)
- `Migrations/005_AddMissingFKs.sql` — missing foreign keys

### 3. Implement Migration Runner
Create `IMigrationRunner` / `MigrationRunner` service that:
- Reads `SchemaVersions` to find pending migrations
- Executes migrations in version order within a transaction
- Records success/failure with checksum
- Supports idempotent re-run (skips already-applied versions)
- Exposes `GetCurrentVersionAsync()`, `GetPendingMigrationsAsync()`

### 4. Add Missing Indexes & Foreign Keys (Schema Audit)
From code review, add these missing indexes and FKs:
- **Indexes**: `SaleTemps.Status`, `SaleTemps.LocationId`, `Sales.LocationId`, `StockMovements.MovementDate`, `Payments.PaymentDate`, `CustomerOutstandings.CustomerId+Status`, `SupplierOutstandings.SupplierId+Status`
- **FKs**: Several tables reference `Users` for CreatedBy/UpdatedBy without FK constraints
- **Unique constraints**: Verify all `Code` fields have UNIQUE indexes

### 5. Add Column Migration Framework
Generalize `EnsureSalesColumnsAsync()` into `EnsureColumnsAsync(tableName, columnDefinitions)` that:
- Reads required columns from a manifest (JSON or attributes on entities)
- Compares against `INFORMATION_SCHEMA.COLUMNS`
- Generates `ALTER TABLE ADD COLUMN` for missing columns
- Records in `SchemaVersions` as a migration step

### 6. Verify Transaction Boundaries (IUnitOfWork)
- Confirm `MySqlUnitOfWork` correctly shares connection/transaction across repositories
- Add integration test that simulates failure mid-transaction and verifies full rollback
- Verify `RepositoryBase` properly passes `Transaction` to Dapper

### 7. Idempotence Tests
Add tests in `MMNextPOS.Infrastructure.Tests`:
- `DatabaseInitializer_InitializeAsync_Twice_ShouldNotThrow` — runs initializer twice on same DB
- `MigrationRunner_ReRunAppliedMigrations_ShouldSkip` — verifies version table prevents re-execution
- `MigrationRunner_FailedMigration_ShouldRecordFailureAndAllowRetry` — verifies failed state tracking
- `ColumnMigration_AddMissingColumns_ShouldBeIdempotent` — verifies ALTER TABLE only runs once

## Files to Create/Modify

| File | Action |
|------|--------|
| `src/MMNextPOS.Infrastructure/Migrations/SchemaVersions.sql` | New migration table DDL |
| `src/MMNextPOS.Infrastructure/Migrations/001_InitialSchema.sql` | Extracted from DatabaseInitializer |
| `src/MMNextPOS.Infrastructure/Migrations/002_SeedDefaultData.sql` | Extracted seed data |
| `src/MMNextPOS.Infrastructure/Migrations/003_AddSalesColumns.sql` | Status, LocationId for Sales |
| `src/MMNextPOS.Infrastructure/Migrations/004_AddIndexes.sql` | Performance indexes |
| `src/MMNextPOS.Infrastructure/Migrations/005_AddMissingFKs.sql` | FK constraints for audit fields |
| `src/MMNextPOS.Infrastructure/IMigrationRunner.cs` | New interface |
| `src/MMNextPOS.Infrastructure/MigrationRunner.cs` | New implementation |
| `src/MMNextPOS.Infrastructure/DatabaseInitializer.cs` | Refactor to use MigrationRunner |
| `src/MMNextPOS.Application/DependencyInjection.cs` | Register MigrationRunner, remove DatabaseInitializer singleton |
| `tests/MMNextPOS.Infrastructure.Tests/MigrationIdempotenceTests.cs` | New test class |

## Acceptance Criteria
1. ✅ Clean Release build (`dotnet build --configuration Release`)
2. ✅ All existing tests pass + new idempotence tests pass
3. ✅ `DatabaseInitializer` runs idempotently (multiple startups = no errors)
4. ✅ `SchemaVersions` table tracks every applied migration with checksum
5. ✅ Failed migration can be retried after fix
6. ✅ Missing indexes/FKs added and verified in integration tests
7. ✅ Column migration framework handles future schema additions

## Risks & Mitigations
| Risk | Mitigation |
|------|------------|
| Breaking existing deployments | MigrationRunner checks version table first; existing DBs get version "0.0.0" baseline |
| Data loss during migration | All migrations run in transaction; rollback on failure |
| CI pipeline changes | Add migration test to CI; verify on clean and pre-populated DB |

## Next Steps
1. **Clarifying Question**: Should we keep `DatabaseInitializer` as the entry point (calling MigrationRunner), or replace it entirely with a new `MigrationRunner` hosted service?
2. **Clarifying Question**: For the baseline version — should existing databases be stamped with a "baseline" version (e.g., "2026.09.06.00") representing current state, or start fresh from version 1?