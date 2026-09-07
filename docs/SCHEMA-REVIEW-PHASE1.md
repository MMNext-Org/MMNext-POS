# Phase 1 — DatabaseInitializer vs Repositories vs Models: Schema Consistency Review

**Date:** 2026-09-07
**Phase:** Phase 1 — Transaction and Data Safety (plan.md §4)
**Scope:** Review `DatabaseInitializer` and the migration pipeline against repository queries and domain model fields.

---

## 1. Executive Summary

The schema-initialization pipeline is **sound in design**: `DatabaseInitializer` delegates all DDL to a versioned `MigrationRunner`, validates schema state before and after, and is idempotent. The current source correctly loads **8 migrations** (`000`–`007`) in version order. After all migrations apply, every domain entity table has the `EntityBase` audit columns (`CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`, `IsDeleted`) that the reflection-based `GenericRepository` relies on.

Three data-safety concerns were identified that should be addressed before the M1 milestone:

| # | Finding | Severity | Risk ID |
|---|---------|----------|---------|
| 1 | `GenericRepository` reflection-based `INSERT`/`UPDATE` writes every scalar property, including `CreatedAt`, unconditionally | High | R4 |
| 2 | `SaleRepository.CreateSaleWithDetailsAsync` uses explicit column lists that omit audit columns, diverging from `GenericRepository.AddAsync` behavior | Medium | R4 |
| 3 | Stale compiled binaries can mask current schema code during `--no-build` test runs | Medium | R3 |

---

## 2. DatabaseInitializer Review

`DatabaseInitializer.InitializeAsync()` does the following, correctly:

1. Validates schema before migrations (`ValidateSchemaAsync`).
2. Runs pending migrations (`RunMigrationsAsync`).
3. Throws if any migration fails.
4. Validates schema after migrations and throws on drift.
5. Wraps everything in structured logging.

**Verdict:** Correct and idempotent. No change required.

`DatabaseInitializer` itself creates **no tables directly** — all DDL lives in embedded SQL migrations under `src/MMNextPOS.Infrastructure/Migrations/`.

---

## 3. Migration Pipeline Review

### 3.1 Migration inventory

| Version | File | Purpose |
|---------|------|---------|
| 000 | `000_BaselineSchemaVersions.sql` | `SchemaVersions` tracking table |
| 001 | `001_InitialSchema.sql` | Core master, sales, inventory, purchase, payment, license, Starman, settings, audit tables |
| 002 | `002_SeedDefaultData.sql` | Default roles, admin user, menu/report permissions |
| 003 | `003_AddSalesColumns.sql` | `Sales.Status`, `Sales.LocationId` + FK |
| 004 | `004_AddIndexes.sql` | Performance indexes (idempotent, `INFORMATION_SCHEMA` guarded) |
| 005 | `005_MissingEntityTables.sql` | `Registrations`, `RemoteWarehouses`, `Subscriptions`, `DashboardWidgets` |
| 006 | `006_AlignSchemaWithEntities.sql` | Adds `CreatedBy`/`UpdatedBy`/`CreatedAt`/`UpdatedAt` where missing; rebuilds mismatched tables |
| 007 | `007_AddMissingFKs.sql` | `CreatedBy`/`UpdatedBy` → `Users(Id)` FKs |

### 3.2 Confirmed correct in current source

`MigrationRunner.LoadMigrations()` currently enumerates all 8 files above in version order. This is verified against the `Migrations/` directory contents and the embedded-resource glob `Migrations\*.sql` in `MMNextPOS.Infrastructure.csproj`.

> **Note:** An earlier integration-test run reported `Loaded 6 migrations: 000, 001, 002, 003, 004, 005`. That output came from a `dotnet test --no-build` invocation against **stale binaries**. The source is correct; a clean rebuild removes the discrepancy.

---

## 4. Schema vs. Model Fields

`EntityBase` defines the contract every entity table must satisfy:

```csharp
public int Id { get; set; }
public DateTime CreatedAt { get; set; }        // NOT NULL, default UtcNow
public DateTime? UpdatedAt { get; set; }
public int? CreatedBy { get; set; }
public int? UpdatedBy { get; set; }
public bool IsDeleted { get; set; }
```

### 4.1 Verified alignment (representative entities)

| Entity | Table | Audit columns present after migration 006/007 | Notes |
|--------|-------|-----------------------------------------------|-------|
| `Sale` | `Sales` | CreatedAt/UpdatedAt/CreatedBy/UpdatedBy added by 006; IsDeleted in 001 | `CustomerName` is `[NotMapped]`, populated via LEFT JOIN |
| `SaleDetail` | `SaleDetails` | CreatedAt/UpdatedAt added by 006; IsDeleted in 001 | — |
| `Product` | `Products` | CreatedBy/UpdatedBy already in 001; CreatedAt/UpdatedAt in 001 | `IsActive` is entity-specific, present in table |
| `Customer` | `Customers` | CreatedBy/UpdatedBy already in 001; CreatedAt/UpdatedAt in 001 | — |

### 4.2 Migration 006 rebuild coverage

Migration 006 rebuilds (drop/recreate) `SystemSettings`, `BackupSettings`, `SuperAdminLogs`, `PaymentVouchers`, `SaleReceipts`/`SaleReceiptDetails`, `PurchaseReceipts`/`PurchaseReceiptDetails` to align them with their entity contracts. The migration header documents this as safe because "every repository INSERT referenced columns the old tables did not have".

**Flag:** Rebuilds are guarded by the migration being versioned and idempotent, but they `DROP` the old table shape. Any accepted business writes in those tables before migration 006 runs would be lost. Verify these tables have no write path prior to migration 006 (see §6 action items).

---

## 5. Schema vs. Repository Queries

### 5.1 `GenericRepository<T>` (reflection-based)

- `AddAsync` builds `INSERT INTO {table} SET {all scalar props except Id}`.
- `UpdateAsync` builds `UPDATE {table} SET {all scalar props except Id} WHERE Id = @Id`.
- `DeleteAsync` soft-deletes (`IsDeleted = 1`) when the model has an `IsDeleted` property; otherwise hard-deletes.
- `GetAllAsync`/`GetPageAsync`/`GetByIdAsync` filter `IsDeleted = 0` when present.

**Finding 1 (High):** `UpdateAsync` writes `CreatedAt` unconditionally. If a service constructs an entity in memory (rather than loading it first), `CreatedAt` defaults to `DateTime.UtcNow` and an update silently rewrites the creation timestamp. This is a data-integrity hazard, not a schema mismatch.

**Recommendation:** Exclude `CreatedAt` (and optionally `CreatedBy`) from the `UPDATE` column set; let `UpdatedAt` be maintained by the model or by `ON UPDATE CURRENT_TIMESTAMP`.

### 5.2 `SaleRepository` (explicit SQL)

- `CreateSaleWithDetailsAsync` inserts `Sales (CustomerId, SaleDate, TotalAmount, Status, LocationId)` and `SaleDetails (SaleId, ProductId, Quantity, UnitPrice)` — all columns exist post-migration. It correctly requires an active transaction and throws otherwise.
- `GetRecentAsync` uses `SELECT s.*, c.Name AS CustomerName ... LEFT JOIN Customers` — maps to `Sale.CustomerName` via `[NotMapped]`.

**Finding 2 (Medium):** `CreateSaleWithDetailsAsync` omits `CreatedBy`/`UpdatedBy`/`IsDeleted`/`CreatedAt`/`UpdatedAt`, relying on DB defaults. This is internally consistent but diverges from `GenericRepository.AddAsync`, which writes those columns explicitly. Standardize on one convention to avoid audit-timestamp drift between code paths.

### 5.3 `SaleDetailRepository`

- `GetBySaleIdAsync` uses `SELECT * ... WHERE SaleId = @SaleId AND IsDeleted = 0` — consistent with schema.

---

## 6. Action Items (before M1 Data Safety exit)

| # | Action | Owner | Priority |
|---|--------|-------|----------|
| 1 | Fix `GenericRepository.UpdateAsync` to exclude `CreatedAt` (and `CreatedBy`) from the SET clause; preserve creation timestamp on update | App Lead / Infra Lead | P0 |
| 2 | Standardize sale insert path: either explicit columns in `SaleRepository` include audit fields, or `GenericRepository` consistently relies on DB defaults | App Lead | P1 |
| 3 | Rebuild cleanly and re-run infrastructure tests **without** `--no-build` to confirm 8 migrations apply and schema validates `Current == 007` | QA Lead | P0 |
| 4 | Confirm tables rebuilt by migration 006 (`SystemSettings`, `BackupSettings`, receipts, vouchers) have no pre-006 write path; document in migration reconciliation report | Infra Lead | P1 |
| 5 | Add a migration integration test that asserts `GetMigrationHistoryAsync` returns exactly 8 successful entries and `ValidateSchemaAsync().ExpectedVersion == "007"` | QA Lead | P1 |

---

## 7. References

- `src/MMNextPOS.Infrastructure/DatabaseInitializer.cs`
- `src/MMNextPOS.Infrastructure/MigrationRunner.cs`
- `src/MMNextPOS.Infrastructure/Migrations/*.sql` (8 files)
- `src/MMNextPOS.Infrastructure/Repositories/GenericRepository.cs`
- `src/MMNextPOS.Infrastructure/Repositories/SaleRepository.cs`
- `src/MMNextPOS.Infrastructure/Repositories/SaleDetailRepository.cs`
- `src/MMNextPOS.Domain/Models/EntityBase.cs`
- `src/MMNextPOS.Domain/Models/Sale.cs`, `Product.cs`, `Customer.cs`