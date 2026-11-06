---
name: pos-database
description: MMNext POS database schema, migrations, Dapper queries, and repository patterns.
---

# POS Database Skill

This skill covers the data layer of MMNext POS: MySQL schema, migrations, Dapper queries, and repository patterns.

## Key Files

- `src/MMNextPOS.Infrastructure/DatabaseInitializer.cs` — Creates master, sales, inventory, warehouse tables.
- `src/MMNextPOS.Infrastructure/Repositories/GenericRepository.cs` — Generic repository with paging and soft-delete.
- `src/MMNextPOS.Infrastructure/DependencyInjection.cs` — Registers repositories and services.
- Migrations: `000_BaselineSchemaVersions` through `009_AddInvoiceNoToSales` (ordered, idempotent, checksum-verified).
- `src/MMNextPOS.Application/Services/IUnitOfWork.cs` — Transaction boundary for business operations.

## Migration Rules

- Migrations are idempotent and version-tracked (`IMigrationRunner`).
- Run migrations only against a backup or staging copy first.
- Verify row counts, totals, stock, payments, and invoice numbers after migration.
- Never manually edit or delete target rows during a migration incident.

## Repository Patterns

- Use `GenericRepository` for standard CRUD with paging and soft-delete.
- All database access is async with cancellation tokens.
- Use parameterized Dapper queries (no string interpolation in SQL).
- `IProductRepository.TryDecrementStockAsync` — guarded atomic stock decrement.

## Transaction Safety

- One `IUnitOfWork` transaction boundary per business operation.
- Write audit records within the same transaction when appropriate.
- Test commit, rollback, timeout, cancellation, and repository exceptions.

## When to Use

- Writing or modifying repository queries.
- Creating or modifying migrations.
- Troubleshooting schema drift or transaction issues.
- Verifying database initialization.

## Verification

- `dotnet test` — run `MigrationIdempotenceTests` and repository integration tests.
- `dotnet build MMNextPOS.slnx --configuration Release` — build gate.