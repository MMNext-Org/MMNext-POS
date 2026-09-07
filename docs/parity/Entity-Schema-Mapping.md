# MMNext POS — Legacy VO → Entity → Table → Column Mapping (Phase 1 Parity Matrix)

**Purpose**: Document every legacy FusionPOS Value Object (VO) mapping to the new MMNext POS POCO entity, database table, and column set. Status fields indicate whether the mapping is complete, requires guard, or needs data migration.

## Legend
| Symbol | Meaning |
|--------|---------|
| ✅ | Full mapping complete: VO → POCO → table exists with all required columns, indexes, FKs. |
| ⚠️ | Mapping exists but requires guard/conditional logic (e.g., sentinel‑guarded rebuild, `[NotMapped]` attribute). |
| ❌ | Missing: table or column does not exist; requires migration (005‑007). |
| 📦 | Table rebuilt in migration 006 (DROP+CREATE IF NOT EXISTS); no production data migrated because inserts always failed under old schema. |

## Mapping Table

| Legacy VO / Entity | New POCO Entity | Database Table | Key Columns | Indexes / FKs | Status |
|-------------------|-----------------|----------------|-------------|---------------|--------|
| **saleVo** → Sale | `Sale` (Domain) | `Sales` | `Id`, `CustomerId`, `TotalAmount`, `Status`, `CreatedBy`, `UpdatedBy`, `CreatedAt`, `UpdatedAt`, `IsDeleted` | `PK_Sales_Id`, `IX_Sales_CustomerId`, `IX_Sales_Status` (guarded) | ✅ |
| **saleDetailVo** → SaleDetail | `SaleDetail` (Domain) | `SaleDetails` | `Id`, `SaleId`, `ProductId`, `Quantity`, `UnitPrice`, `LineTotal`, `UnitCost` | `PK_SaleDetails_Id`, `IX_SaleDetails_SaleId`, `IX_SaleDetails_ProductId` | ✅ |
| **saleTempVo** → SaleTemp | `SaleTemp` (Domain) | `SaleTemp` | `Id`, `SessionId`, `ProductId`, `Quantity`, `Price`, `Status` | `PK_SaleTemp_Id`, `IX_SaleTemp_SessionId` | ✅ |
| **customerVo** → Customer | `Customer` (Domain) | `Customers` | `Id`, `Name`, `Email`, `Phone`, `Address`, `Status`, `CreatedBy`, `UpdatedBy`, `CreatedAt`, `UpdatedAt`, `IsDeleted` | `PK_Customers_Id`, `IX_Customers_Email` (unique) | ✅ |
| **stockVo** → Product | `Product` (Domain) | `Products` | `Id`, `Name`, `Sku`, `Description`, `CategoryId`, `OpeningStock`, `CurrentStock`, `UnitCost`, `ReorderLevel`, `IsActive`, `LastAdjustment`, `CreatedBy`, `UpdatedBy`, `CreatedAt`, `UpdatedAt`, `IsDeleted` | `PK_Products_Id`, `IX_Products_CategoryId` (dropped — column missing in old schema; guarded in 004), `IX_Products_Sku` (unique) | ✅ (004/006) |
| **supplierVo** → Supplier | `Supplier` (Domain) | `Suppliers` | `Id`, `Name`, `ContactName`, `Email`, `Phone`, `Address`, `Status`, `CreatedBy`, `UpdatedBy`, `CreatedAt`, `UpdatedAt`, `IsDeleted` | `PK_Suppliers_Id`, `IX_Suppliers_Email` (unique) | ✅ |
| **purchaseVo** → Purchase | `Purchase` (Domain) | `Purchases` | `Id`, `SupplierId`, `TotalAmount`, `Status`, `CreatedBy`, `UpdatedBy`, `CreatedAt`, `UpdatedAt`, `IsDeleted` | `PK_Purchases_Id`, `IX_Purchases_SupplierId`, `IX_Purchases_Status` | ✅ |
| **purchaseDetailVo** → PurchaseDetail | `PurchaseDetail` (Domain) | `PurchaseDetails` | `Id`, `PurchaseId`, `ProductId`, `Quantity`, `UnitCost`, `LineTotal`, `ReceivedQuantity`, `Status` | `PK_PurchaseDetails_Id`, `IX_PurchaseDetails_PurchaseId`, `IX_PurchaseDetails_ProductId` | ✅ |
| **transactionLogVo** → TransactionLog | `TransactionLog` (Domain) | `TransactionLogs` | `Id`, `UserId`, `Action`, `EntityType`, `EntityId`, `OldValues`, `NewValues`, `Timestamp`, `CreatedBy`, `CreatedAt` | `PK_TransactionLogs_Id`, `IX_TransactionLogs_EntityType_EntityId` | ✅ |
| **userVo** → User | `User` (Domain) | `Users` | `Id`, `Username`, `PasswordHash`, `Role`, `Status`, `CreatedBy`, `UpdatedBy`, `CreatedAt`, `UpdatedAt`, `IsDeleted`, `FailedLoginCount`, `LastLoginAt` | `PK_Users_Id`, `IX_Users_Username` (unique), `FK_Users_CreatedBy_SystemUsers` (self‑FK, SET NULL) | ✅ |
| **Reporting / other VOs** | Various reporting POCOs | `DashboardWidgets`, `Registrations`, `RemoteWarehouses`, `Subscriptions` | See migration 005 for full column lists | See migration 005 for indexes/FKs | ✅ (005) |

## Tables Added in Migration 005 (MissingEntityTables)
| Table | POCO Entity | Primary Purpose | Key Columns | Indexes |
|-------|-------------|----------------|-------------|---------|
| `Registrations` | `Registration` | User/device registration tokens | `Id`, `Token`, `DeviceInfoId`, `Expiry`, `CreatedBy`, `CreatedAt`, `IsDeleted` | `PK_Registrations_Id`, `IX_Registrations_Token` (unique), `IX_Registrations_DeviceInfoId` |
| `RemoteWarehouses` | `RemoteWarehouse` | Multi‑site inventory sync | `Id`, `Name`, `Code`, `Address`, `Latitude`, `Longitude`, `Status`, `CreatedBy`, `CreatedAt`, `IsDeleted` | `PK_RemoteWarehouses_Id`, `IX_RemoteWarehouses_Code` (unique) |
| `Subscriptions` | `Subscription` | Subscription / license tracking | `Id`, `Name`, `Plan`, `Status`, `StartDate`, `EndDate`, `CreatedBy`, `CreatedAt`, `IsDeleted` | `PK_Subscriptions_Id`, `IX_Subscriptions_Plan` |
| `DashboardWidgets` | `DashboardWidget` | Widget configuration for UI | `Id`, `WidgetType`, `ConfigJson`, `Position`, `IsVisible`, `CreatedBy`, `CreatedAt`, `IsDeleted` | `PK_DashboardWidgets_Id`, `IX_DashboardWidgets_WidgetType` |

## Tables Rebuilt in Migration 006 (Sentinel‑Guarded)
| Old Table | Reason for Rebuild | New Columns Added | Status |
|-----------|-------------------|-------------------|--------|
| `SystemSettings` | Column‑model mismatch (many model‑required fields missing) | `Key` (PK, not‑null), `Value` (not‑null), `Description`, `LastUpdated`, `UpdatedBy`, `CreatedBy`, `CreatedAt`, `IsDeleted` | 📦 (rebuilt; no prod data) |
| `BackupSettings` | Same as SystemSettings | `Key`, `Value`, `Description`, `LastUpdated`, `UpdatedBy`, `CreatedBy`, `CreatedAt`, `IsDeleted` | 📦 |
| `SuperAdminLogs` | Same pattern | `Id` (bigint, auto), `Message`, `Level`, `CreatedAt`, `CreatedBy`, `IsDeleted` | 📦 |
| `PaymentVouchers` | Same pattern | `Id`, `VoucherNumber`, `Amount`, `Status`, `CreatedBy`, `CreatedAt`, `IsDeleted` | 📦 |
| `SaleReceipts` | Same pattern + Details | `Id`, `SaleId`, `ReceiptNumber`, `TotalAmount`, `Status`, `CreatedBy`, `CreatedAt`, `IsDeleted` + `SaleReceiptDetails` (1‑to‑many) | 📦 |
| `PurchaseReceipts` | Same pattern + Details | `Id`, `PurchaseId`, `ReceiptNumber`, `TotalAmount`, `Status`, `CreatedBy`, `CreatedAt`, `IsDeleted` + `PurchaseReceiptDetails` | 📦 |
| `StockMovements` | Added `ProductId`, `Quantity`, `IsActive`, `CreatedBy/UpdatedBy/CreatedAt/UpdatedAt/IsDeleted` | `IX_StockMovements_ProductId`, `IX_StockMovements_IsActive` | ✅ |
| `PurchaseDetails` | Added `ReceivedQuantity` (default 0), `CreatedBy/UpdatedBy/CreatedAt/UpdatedAt/IsDeleted` | `IX_PurchaseDetails_ReceivedQuantity` | ✅ |

## `[NotMapped]` Attributes (Prevent GenericRepository INSERT Errors)
| Entity | NotMapped Property | Reason |
|--------|-------------------|--------|
| `Sale` | `CustomerName` | Column `CustomerName` does not exist in `Sales` table; mapped via navigation `Customer` entity instead. |
| `SaleDetail` | (any navigation/calc props) | Filtered by `IsColumnProperty()` predicate in `GenericRepository`. |
| `PurchaseDetail` | (any navigation/calc props) | Same predicate. |

## Predicates / Helpers
| Component | Purpose |
|-----------|---------|
| `GenericRepository.IsColumnProperty()` | Filters `System.Reflection.PropertyInfo` to only those whose name matches a column in the target table; skips navigation properties, complex types, and `[NotMapped]` attributes. Applied in `AddAsync<T>` and `UpdateAsync<T>`. |
| `MySqlUnitOfWork` connection string | Appends `Allow User Variables=true` if not present; required for `SET @sql` blocks in migrations 003‑007 (MySQL 8 user‑variable support). |
| `MigrationRunner.knownMigrations` | Ordered list: `005_MissingEntityTables`, `006_AlignSchemaWithEntities`, `007_AddMissingFKs`. Removed dangling `005_Registration` entry. |

## Migration Version History
| Phase | Versions | Summary |
|-------|----------|---------|
| **Phase 1 — Schema Init** | 000‑007 | All legacy VO entity tables created/altered; audit columns added; missing tables (Registrations, RemoteWarehouses, Subscriptions, DashboardWidgets) created; FKs added; MySQL 8 syntax fixed; idempotent chain verified. |
| **Phase 2 — Transaction & Data Safety** *(to be started)* | — | Verify entity‑field mapping against every repository query, confirm idempotent upgrade path for existing installations, run full integration test suite with Docker. |

## Accountability
| Artifact | Owner | Date |
|----------|-------|------|
| Migration 003‑007 SQL files | Infrastructure team | 2026‑09‑07 |
| `MySqlUnitOfWork` connection‑string fix | Infrastructure team | 2026‑09‑07 |
| `GenericRepository.IsColumnProperty()` predicate | Infrastructure team | 2026‑09‑07 |
| `Sale.cs` `[NotMapped]` | Domain team | 2026‑09‑07 |
| `UsersListPage.cs` (build fix) | WinForms team | 2026‑09‑07 |
| Parity matrix documentation | QA / Architecture | 2026‑09‑07 |

---
*This matrix reflects the codebase state as of 2026‑09‑07 after completion of Phase 1 schema initialization. All migration scripts, model attributes, and `MigrationRunner` configuration have been updated to produce a consistent, idempotent schema across MySQL 8.0. Full parity validation requires Docker‑based integration test execution (Phase 2).*

---