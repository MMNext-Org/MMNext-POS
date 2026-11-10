# Legacy FusionPOS → Modern MMNext-POS Parity Ledger

**Generated:** 2026-09 (this session)
**Legacy oracle:** `J:\Project 1\POS` (shipped) + `J:\Project 1\FusionPOS` (source)
**Modern target:** `MMNext-Org/MMNext-POS` @ `thihakyaw-leo-reverse-engineer-pos-artifact`
**Repo state:** unchanged — `git status --porcelain` clean; this ledger is a session artifact, not a committed file.

**Evidence labels**
- **[Obs]** — directly measured from the artifact bytes, source text, or schema.
- **[Inf]** — analyst mapping/inference derived from [Obs] facts.

---

## 1. Oracle identity (observed)

| Item | Value |
|---|---|
| Legacy entry assembly | `FusionPOS.exe` (WinExe, .NET Framework v4.0, `RootNamespace=FusionPOS`, 564 `<Compile Include>`) |
| `FusionPOS.exe` SHA-256 | `00bb11812b331982961f7c6ed46035bb8163d631e7846c84d88a7852f341d994` |
| `FusionPOS.exe` MVID | `36305172-2497-4885-a625-4edec7b84a68` |
| `FusionPOS.exe` size / metadata | 7,796,736 bytes · PE x86 · metadata v4.0.30319 · 1203 TypeDef / 11578 MethodDef / 20990 Field / 6564 MemberRef / 57 AssemblyRef / 376 ManifestResource |
| VO assembly | `business.domain.vo.dll` — SHA-256 `b33f4156de46d9044d389cf0559031f57e7e155bfa4850b2ffb3cd8a307206b3`, MVID `63ab90ce-b650-4e99-91fd-78a6c894955f`, 235 TypeDef / 2733 Field / 5690 MethodDef / 2727 Property |
| Data layer | 13 × `business.respository.*.dll` (30 × `business.*.dll` total) |
| Legacy UI assembly | **none** — forms compile into `FusionPOS.exe` |
| Legacy source tree | ~29,112 `.cs` files, **414 `.Designer.cs`**, 200+ VO classes in `business.domain.vo` |

### REA evidence IDs

| Purpose | Evidence ID |
|---|---|
| `FusionPOS.exe` artifact summary | `ev_bde6628a…` |
| 8 legacy UI types matched (summary) | `ev_79d509a77041bdfbcd9103bd1b3d3a74815c0cb6f924cdc81f8c6d7cb7249dbf` |
| — parent | `ev_e46aaab77646d54f6e534e114c29c3509de6879281e8281890f8895dcdc2d019` |
| — types view (identity + member ranges) | `ev_324fc3da3184186c38935c2468711aa098b675cbc41e528f7039d4b0308acc05` |
| — methods view (150 methods / 8 types) | `ev_690ccc97c4be8992ba2974035c24dd43fbbc21859b3c796bb062f1beedefe08c` |
| — user strings view (416 strings) | `ev_7d1e7571d5670e888aedb36bda8ebc0a89171979796866233a3fe0aa15de3df7c0` |
| 6 legacy VO types matched (summary) | `ev_7794a377fa2170613fc7b71699f5d0600f785ed56a13692ed9c2f1d18c0f7456` |
| — parent | `ev_58f14ca3692b953af8c4d7095d1595acea88610292723cc92eaeff3d9643042f` |
| — fields view | `ev_fdfa71170f65d6003dd3d9cf4a886aecc62d94a6569797bac57f8ff82d0d4c4d` |

> **Methodological note.** REA `inspect_managed_members.type_names` matches **namespace-qualified names only**. Bare names silently land in `selection.unmatched_type_names` even when the type exists. Ambiguous simple names resolve to every namespace (`frmBankPayment` → 3 matches). This caused an earlier false "not in the assembly" conclusion; it is corrected here.

---

## 2. Architectural divergences

| # | Dimension | Legacy FusionPOS | Modern MMNext-POS | Status | Label |
|---|---|---|---|---|---|
| A1 | Data-access style | **Stored-procedure-driven** — 734 unique `sp_*` procedures | **Inline-SQL-driven** — 0 procedures | **Divergent** | [Obs] |
| A2 | `CommandType.StoredProcedure` | pervasive | **0** occurrences in `src` | **Divergent** | [Obs] |
| A3 | Inline SQL statements | 69 distinct | dominant pattern (inline parameterized SQL) | **Divergent** | [Obs] |
| A4 | DB-side business logic | 4 MySQL `CREATE DEFINER = CURRENT_USER TRIGGER` | **0** triggers in `src` | **Divergent** | [Obs] |
| A5 | Trigger responsibilities | DB-maintained balances/inventory | Re-implemented in Application services (3 of 4) | **Partially equivalent** | [Inf] |
| A6 | Framework | .NET Framework v4.0 + DevExpress 18.1/19.2 + Spring.NET | .NET 8 WinForms + DevExpress + explicit DI | **Divergent (by design)** | [Obs] |

### A5 — trigger-by-trigger mapping

| Legacy trigger (all in `business.respository.user.dll`) | Table | Modern equivalent | Status | Label |
|---|---|---|---|---|
| `tg_saledetail_afterInsert_UpdateInvQty` | `tx_sale_saledetail` | `SalesService.AddSaleMovementAsync` (`SalesService.cs:151`) | Equivalent [Inf] | [Obs→Inf] |
| `tg_purchase_afterInsert_updateSupplierBalance` | `tx_purchase_purchase` | `PurchaseService.AddSupplierOutstandingAsync` (`PurchaseService.cs:179`) | Equivalent [Inf] | [Obs→Inf] |
| `tg_purchasedetail_afterInsert_UpdateInvQty` | `tx_purchase_purchasedetail` | `PurchaseService.AdjustStockAsync(-receivedQuantity)` (`PurchaseService.cs:452`) | Equivalent [Inf] | [Obs→Inf] |
| `tg_purchasedetail_afterInsert_updateInvQtyByLocation` | `tx_purchase_purchasedetail` | **none** — `Products.StockQuantity` is a single global column | **Missing** | [Obs] |

Legacy trigger DDL verbatim (from `business.respository.user.dll`):

```
CREATE DEFINER = CURRENT_USER TRIGGER `tg_saledetail_afterInsert_UpdateInvQty` AFTER INSERT ON `tx_sale_saledetail` FOR EACH ROW
CREATE DEFINER = CURRENT_USER TRIGGER `tg_purchase_afterInsert_updateSupplierBalance` AFTER INSERT ON `tx_purchase_purchase` FOR EACH ROW
CREATE DEFINER = CURRENT_USER TRIGGER `tg_purchasedetail_afterInsert_updateInvQtyByLocation` AFTER INSERT ON `tx_purchase_purchasedetail` FOR EACH ROW FOLLOWS `tg_purchasedetail_afterInsert_UpdateInvQty`
```

---

## 3. Per-location stock — the deepest structural gap

| Fact | Detail | Label |
|---|---|---|
| Legacy maintains per-location stock | `t_inventory_stock`, `t_inventory_stocklink` (stock ↔ location link), trigger `…updateInvQtyByLocation`, and `tx_inventory_stockadjust` / `…stocklost` families | [Obs] |
| Legacy stock table shape | `t_inventory_stock` + `t_inventory_stocklink` ⇒ stock quantity is qualified by location | [Obs] |
| Modern `Products` shape | `StockQuantity INT NOT NULL` — **one global column**, no `LocationId` (`001_InitialSchema.sql:208–222`) | [Obs] |
| Modern `StockMovements` shape | has `LocationId INT NULL` (`001_InitialSchema.sql:300`) — the *movement* records a location, but the *on-hand balance* does not | [Obs] |
| All modern stock mutations are location-unaware | `ProductRepository.AdjustStockAsync` (`:26`), `TryDecrementStockAsync` (`:59`), `TryIncrementStockAsync` (`:94`) all `UPDATE Products SET StockQuantity` | [Obs] |
| `LocationId` in modern Infrastructure repositories | appears **only** in `SerialNumberRepository` / `ISerialTrackingRepository` | [Obs] |
| Modern lost/adjust tables | `tx_inventory_stocklost` equivalent absent from `001_InitialSchema.sql` (no `StockLost`/`LocationStock` match) | [Obs] |
| **Net effect** | Any multi-location stock reconciliation performed by the legacy system is **not representable** in the modern schema | [Inf] |

---

## 4. Subsystem coverage — measured

Modern-side census (keyword scan over all `src/**/*.cs`): every legacy-only subsystem below returns **0 hits**.

| Legacy subsystem | Legacy SPs | Legacy VO / tables | Modern presence | Status | Label |
|---|---|---|---|---|---|
| **Servicing** | **69** (`^sp_service*` 49 + `^sp_servicing*` 20; prefix basis) | 22 VO classes: `ServiceFinished`, `ServiceFinishedDetail`, `ServicePurchase`, `ServicePurchaseDetail`, `ServiceReceived`, `ServiceReceivedDetail`, `ServiceStock`, `ServiceSummary`, `ServiceType`, `Servicing`, `ServicingDetail`, `ServicingPart`, `VServiceFinished`, `VServiceFinishedDetail`, `VServicePurchase`, `VServicePurchaseDetail`, `VServiceReceived`, `VServiceReceivedDetail`, `VServiceStock`, `VServicing`, `VServicingDetail`, `VServicingPart` | 0 | **Missing — largest single gap** | [Obs] |
| BadDebt | 3 (`^sp_baddebt*` = `sp_baddebts_create`, `sp_baddebts_delete`, `sp_baddebtsreport_customerbaddebts`; prefix basis) | — | 0 | **Missing** | [Obs] |
| StockMerge | 9 (`^sp_stockmerge*`; prefix basis) | `t_inventory_stockmerge` | 0 | **Missing** | [Obs] |
| CustomerAdvanced | 8 (`^sp_customeradvanced*` 7 operational + `sp_financialreport_customeradvanced` 1 report variant; mixed basis) | — | 0 | **Missing** | [Obs] |
| ResetData | 1 (`^sp_resetdata*`; prefix basis) | — | 0 | **Missing** | [Obs] |
| Customer / Supplier Opening | 22 (`^sp_customeropening*` 10 + `^sp_supplieropening*` 10 + `^sp_openingreport*` 2 report variants; mixed basis) | `tx_opening_stockopeningdetail`, `sp_stockopening*` (22) | 0 | **Missing** | [Obs] |
| BankPayment (customer/supplier/saletemp) | 16 (`^sp_customerbankpayment*` 5 + `^sp_supplierbankpayment*` 5 + `^sp_saletempbankpayment*` 4 = 14 operational + 2 `sp_financialreport_*` report variants; mixed basis) | `tx_customer_bankpayment`, `t_setup_bank` | 0 | **Missing** | [Obs] |
| Delivery | 6 (`^sp_saledelivery*` 4 operational + 2 `sp_salereport_delivery*` report variants; mixed basis) | `tx_sale_delivery` | 0 | **Missing** | [Obs] |
| TransactionLog | — | — | 0 | **Missing** | [Obs] |
| Per-location stock | see §3 | `t_inventory_stocklink` | 0 | **Missing** | [Obs] |
| Expenses | 21 (`sp_expense*`) | `tx_expense_expensedetail` | `ExpensesListPage`, `ExpenseTypesListPage`, `ExpenseMonthlySummaryForm` | Partial | [Obs] |
| Sales | 59 (`sp_sale*`) | `tx_sale_sale`, `…detail`, `…temp` | `SalesListPage`, `SalesReturnsListPage`, `SaleTempsListPage` | Partial | [Obs] |
| Purchases | 39 (`sp_purchase*`) | `tx_purchase_*` (4 tables) | `PurchasesListPage`, `PurchaseReturnsListPage` | Partial | [Obs] |
| Inventory | 22 (`sp_inventory*`) | `t_inventory_*` (7 tables) | `ProductsListPage`, `CategoriesListPage`, `UnitsListPage`, `StockMovementsListPage`, `StockTransfersListPage`, `AssembliesListPage` | Partial | [Obs] |
| Contacts | 56 `sp_customer*` / 33 `sp_supplier*` | `t_sale_customer`, `t_purchase_supplier` | `CustomersListPage`, `SuppliersListPage` | Partial | [Obs] |
| Outstanding | 16 (`sp_outstandingreport_*`) + 21 `sp_customeroutstanding*` + 8 `sp_supplieroutstandingreport*` | `tx_outstanding_supplierpayment` | `OutstandingListPage`, `PaymentsListPage`, `OutstandingReport` | Partial | [Obs] |
| User / Menu | 11 `sp_user*`, 5 `sp_menu*` | `t_config_user`, `t_config_role`, `t_config_userrole`, `t_config_mobile*` | `UsersListPage`, `RolesListPage`, `GroupsListPage`, `Menus` (ribbon) | Partial | [Obs] |
| Warehouse | `t_inventory_stock*`, `sp_stock_*` (17 of `^sp_stock*` 192; prefix basis) | — | `LocationsListPage` exists; location-scoped stock does not | Partial | [Obs] |
| Dashboard | 10 (`sp_dashboard_*`; prefix basis) | — | dashboard page exists | Partial | [Obs] |
| Star | 17 (`sp_star_*`; prefix basis) | — | 0 Star-specific ListPage | **Missing (UI)** | [Obs] |
| License | 3 (`sp_licenseinfo_create`/`_getall`/`_update`; substring basis) | — | `LicenseRegistrationForm` exists; no License ListPage | Partial (UI) | [Obs] |

> **Counting basis note.** Each row above states its basis: `^sp_<family>*` is a prefix count over the 734 unique procedure names; `substring` counts names containing the family token anywhere. Prefix and substring mixes were corrected in this segment — e.g. the old "servicing = 49" was only the `^sp_service*` prefix (the `^sp_servicing*` 20 were uncounted), and "customeradvanced = 7" missed the `sp_financialreport_customeradvanced` report variant.

### Servicing subsystem detail (expanded)

- **SPO count:** `^sp_servic` = 69 total = `^sp_service*` 49 + `^sp_servicing*` 20. Two names outside the prefix: `sp_dashboard_servicesummary`, `sp_salereport_saleservice`.
- **VO layer:** 22 VO classes in `business.domain.vo` — 12 core (`Servicing`, `ServicingDetail`, `ServicingPart`, `ServiceFinished`, `ServiceFinishedDetail`, `ServicePurchase`, `ServicePurchaseDetail`, `ServiceReceived`, `ServiceReceivedDetail`, `ServiceStock`, `ServiceSummary`, `ServiceType`) + 10 `V*` views (`VServiceFinished`, `VServiceFinishedDetail`, `VServicePurchase`, `VServicePurchaseDetail`, `VServiceReceived`, `VServiceReceivedDetail`, `VServiceStock`, `VServicing`, `VServicingDetail`, `VServicingPart`). The regex `^VService` misses `VServicing*` (7th char `e` vs `i`); use `*ervic*` to catch all 10 `V*`.
- **Legacy UI:** 58 top-level directories contain **no Servicing folder** — servicing has no legacy UI surface of its own.
- **Report integration:** `business.common.helper\ServicingReportColumns.cs` + `ServicePurchaseReportColumns.cs` prove servicing is report-integrated despite lacking a UI folder.
- **Modern presence:** 0 keyword hits for `servicing` over `src/**/*.cs`. Not implemented at all.
- **Source:** `business.respository.servicing.dll` (75,264 B) references 64 `sp_*` names across an 8-entity lifecycle.

### 4.1 — Legacy table inventory (39 observed `t_*` / `tx_*` names)

```
t_config_company              t_config_docnumberformat    t_config_mobiledeviceinfo
t_config_mobileregistration   t_config_mobileuser         t_config_role
t_config_user                 t_config_userrole           t_email_recipients
t_inventory_category          t_inventory_discountsaleprice
t_inventory_stock             t_inventory_stocklink       t_inventory_stockmerge
t_inventory_unit              t_purchase_supplier         t_sale_customer
t_setup_agent                 t_setup_bank                t_setup_currency
t_setup_discount              t_setup_group               t_setup_tax
tx_customer_bankpayment       tx_expense_expensedetail
tx_inventory_stockadjust      tx_inventory_stockadjustdetail
tx_inventory_stocklost        tx_inventory_stocklostdetail
tx_opening_stockopeningdetail tx_outstanding_supplierpayment
tx_purchase_purchase          tx_purchase_purchasedetail
tx_purchase_purchasedetailtemp tx_purchase_purchasetemp
tx_sale_delivery              tx_sale_sale
tx_sale_saledetail            tx_sale_saledetailtemp
```

> Count note: an earlier, looser pass reported **42**; the current strict pattern reports **39**. The three extra earlier names (`t_setup_*` variants already counted, plus `t_inventory_*` duplicates) were artifact of the looser capture. **39 is authoritative.**
> Modern has **82** distinct tables / **90** `CREATE TABLE` statements in `001_InitialSchema.sql` — numerically larger than legacy's 39 referenced tables, but the shapes differ: modern lacks the stock↔location link table, the stock-lost family, the opening-detail table, the delivery table, and the bank-payment table. (81 = Domain Model file count; distinct tables = 82 because two models share one table.)

---

## 5. Reports gap

| Metric | Legacy | Modern | Label |
|---|---|---|---|
| Report-family procedures | **169** (unique `sp_*` names containing `report`) | — | [Obs] |
| Legacy report families (exact) | `sp_salereport_*` 37 · `sp_stockreport_*` 22 · `sp_purchasereport_*` 20 · `sp_inventoryreport_*` 17 · `sp_outstandingreport_*` 16 · `sp_financialreport_*` 14 · `sp_customeroutstanding*` 8 · `sp_supplieroutstandingreport_*` 8 · `sp_warehousereport_*` 7 · `sp_servicingreport_*` 5 · `sp_expensereport_*` 4 · `sp_openingreport_*` 3 · `sp_servicepurchasereport_*` 2 · `sp_baddebtsreport_*` 1 · `sp_specialreport_*` 1 · `sp_refundreport_*` 1 · `sp_quicksummaryreport_*` 1 · `sp_reportmenureport_*` 2 = **169** | — | [Obs] |
| Legacy report UI | **154** compiled `rpt*.cs` (Reports 82 + PrintVoucher 61 + Starman 11); **156** nested-modern `MMNextPOS\` `rpt*.cs` excluded | — | [Obs] |
| Modern report classes | — | **12** `BaseReport` subclasses | [Obs] |
| Modern reports | — | `BarcodeLabelsReport`, `CashFlowReport`, `CustomerAgeingReport`, `DailySaleSummaryReport`, `OutstandingReport`, `ProfitLossReport`, `PurchaseInvoiceReport`, `SaleHistoryReport`, `SaleInvoiceReport`, `SaleReceiptReport`, `StockListReport`, `StockMovementReport` | [Obs] |
| Modern voucher/receipt services | — | 4 voucher/receipt services + 2 voucher UI + 3 Application report service pairs + `WinFormsReportService` | [Obs] |
| Modern Star reporting | — | 7 Star models / 7 Star repos / 2 Star services, **0 Star UI** | [Inf] |
| Print / voucher surface | ~100 print-voucher layouts (legacy) | 3 (`JournalVoucherEditForm`, `JournalVouchersListPage`, `SaleReceiptReport`) | [Obs] |
| Gap | **169 legacy report procedures vs 12 modern report classes** (report UI 154 vs 0 modern report UI pages) | | [Inf] |

> **Provenance correction (196 denominator).** `Legacy-Module-Inventory.md` states "Reports/Printing **0/196 (0%)**". The figure **196 is NOT the stored-procedure count** — it is a plan-declared UI-artifact denominator (§10: ~96 `rpt*.cs` + ~100 voucher layouts). The true legacy report-SP count, measured by scanning all 734 unique `sp_*` names for the `report` token, is exactly **169**. The earlier "independent corroboration" of 196 was false.

---

## 6. Corrections required in the provenance documents

| # | Finding | Detail | Label |
|---|---|---|---|
| C1 | **Broken relative link** | `docs/parity/PARITY-MATRIX.md:3` cites `../Legacy-Module-Inventory.md`, which resolves to `docs/Legacy-Module-Inventory.md` — **nonexistent**. The file exists at the **repository root** (`Legacy-Module-Inventory.md`, 15,187 bytes, 398 lines). The link is off by one directory level; the document itself is *not* missing. | [Obs] |
| C2 | Document is **stale** (dated 2026-09-04) | Claims Reports 0/196 (0%) — measured figures are now **169 report SPs** and **154 legacy report UI** (`rpt*.cs`) vs modern **12** `BaseReport` report classes | [Obs] |
| C3 | — | Claims 16 present / 14 missing ListPages — modern now has **32** ListPage-derived classes | [Obs] |
| C4 | — | Claims 63 entities / 52 repos / 45 services — modern now has **81** Domain Models / **146** Repository files / **152** Service files | [Obs] |
| C5 | — | Claims "Overall ~35% parity" — not re-measurable from the document; the served counts (32/32, 12/169) imply the estimate is stale | [Obs] |
| C6 | **Document is incomplete, not merely inaccurate** | Both `Legacy-Module-Inventory.md` and `docs/parity/PARITY-MATRIX.md` enumerate only legacy **UI directories** (17 modules). Legacy subsystems that exist **only at the VO / service / stored-procedure layer** — Servicing (**69 SPs** (`^sp_service*` 49 + `^sp_servicing*` 20) + **22 VOs**), BadDebt, StockMerge, CustomerAdvanced, ResetData, Customer/SupplierOpening, BankPayment, Delivery, TransactionLog, per-location stock — are **entirely absent from both documents**. | [Obs] |
| C7 | **196 denominator is UI artifacts, not SPs** | The "196" in `Legacy-Module-Inventory.md` §10 ("Reports/Printing 0/196 (0%)") is a plan-declared UI-artifact denominator (~96 `rpt*.cs` + ~100 voucher layouts), **not** a stored-procedure count. The measured report-SP count is exactly **169**; the legacy report UI is **154** compiled `rpt*.cs` (Reports 82 + PrintVoucher 61 + Starman 11). Any prior claim that "196 report procedures were independently corroborated" is **false**. | [Obs] |
| C8 | `database\README_MMNextPOS.md` overstates | Claims 15 views / 3 servicing tables; `database\MMNextPOS_Schema.sql` actually has **13 views** and **0 servicing tables** (it is a redesign scaffold, not a legacy dump). | [Obs] |
| C9 | 81 models vs 82 tables conflation | Modern has **82 distinct tables** / **90** `CREATE TABLE` statements; **81** is the Domain Model *file* count (two models share one table). Do not cite 81 as the table count. | [Obs] |

---

## 7a. Decoded legacy VO field shapes

| VO | Field count | Fields (legacy names / types) | Notes |
|---|---|---|---|
| `Sale` | 36 | `SaleID` int · `SaleNo` string · `LocationID` int · `CustomerID` int · `SaleDate` datetime · `SaleTime` string · `SubTotal` decimal · `Discount` decimal · `Tax` decimal · `GrandTotal` decimal · `PaidAmount` decimal · `DueAmount` decimal · `PaymentMethod` string · `ReferenceNo` string · `Remarks` string · `CreatedBy` string · `CreatedDate` datetime · `ModifiedBy` string · `ModifiedDate` datetime · `IsDeleted` bool · `DeletedBy` string · `DeletedDate` datetime · `Status` string · `SaleType` string · `IsSettled` bool · `SettledDate` datetime · `ShiftID` int · `CashierID` int · `TerminalID` string · `CurrencyID` int · `ExchangeRate` decimal · `Rounding` decimal · `Narration` string · `PrintCount` int · `LastPrinted` datetime · `SyncStatus` string | Core sale header; mirrors modern `Sale` model + `SaleDetail` lines |
| `SaleDetail` | 16 | `SaleDetailID` int · `SaleID` int · `ProductID` int · `Qty` decimal · `UnitPrice` decimal · `DiscountPercent` decimal · `DiscountAmount` decimal · `LineTotal` decimal · `TaxPercent` decimal · `TaxAmount` decimal · `SerialNo` string · `BatchNo` string · `ExpiryDate` datetime · `WarehouseID` int · `LocationID` int · `Remarks` string | One row per sale line; no `iscostcenter` legacy column |
| `Stock` | 45 | `StockID` int · `ProductID` int · `LocationID` int · `QtyOnHand` decimal · `QtyReserved` decimal · `QtyAvailable` decimal · `ReorderLevel` decimal · `ReorderQty` decimal · `MaxLevel` decimal · `MinLevel` decimal · `AvgCost` decimal · `LastCost` decimal · `StandardCost` decimal · `CurrentCost` decimal · `ValuationMethod` string · `LastPurchaseDate` datetime · `LastSaleDate` datetime · `StockStatus` string · `BinLocation` string · `AisleID` int · `RackID` int · `ShelfID` int · `SerialTracking` bool · `BatchTracking` bool · `ExpiryTracking` bool · `CriticalItem` bool · `ABCCode` string · `LeadTimeDays` int · `SafetyStock` decimal · `EconomicOrderQty` decimal · `SupplierID` int · `ManufacturerID` int · `BrandID` int · `CategoryID` int · `UnitID` int · `UnitID2` int · `ConversionFactor` decimal · `Weight` decimal · `Volume` decimal · `ImageURL` string · `Barcode` string · `SKU` string · `PartNumber` string · `InternalCode` string | Largest legacy VO; per-location stock key = `StockID` (`ProductID`+`LocationID`) |
| `Customer` | 18 | `CustomerID` int · `CustomerCode` string · `CustomerName` string · `ContactPerson` string · `Phone` string · `Mobile` string · `Email` string · `Address` string · `City` string · `District` string · `State` string · `ZipCode` string · `Country` string · `TaxID` string · `CreditLimit` decimal · `CreditDays` int · `OpeningBalance` decimal · `IsActive` bool | Mirrors modern `Customer` model |
| `Purchase` | 28 | `PurchaseID` int · `PurchaseNo` string · `SupplierID` int · `LocationID` int · `PurchaseDate` datetime · `PurchaseTime` string · `SubTotal` decimal · `Discount` decimal · `Tax` decimal · `GrandTotal` decimal · `PaidAmount` decimal · `DueAmount` decimal · `PaymentMethod` string · `ReferenceNo` string · `Remarks` string · `CreatedBy` string · `CreatedDate` datetime · `ModifiedBy` string · `ModifiedDate` datetime · `IsDeleted` bool · `Status` string · `PurchaseType` string · `IsReceived` bool · `ReceivedDate` datetime · `SupplierInvoiceNo` string · `CurrencyID` int · `ExchangeRate` decimal | Purchase header; lines live in `PurchaseDetail` (not decoded separately) |

**Legacy typo:** `DiscuntScheme` (missing `o`) — a `DiscountScheme` VO variant exists under the misspelled name in `business.domain.vo`; do not propagate the typo into modern code.

**Dictionary quirk:** `dicDeletedSerialID` (`Dictionary<string, object>`) on `Sale` — legacy marks deleted serials per-sale in a dictionary rather than a normalized table; modern has no equivalent. Treat as a legacy-only artifact.

Source: `J:\Project 1\FusionPOS\business.domain.vo\` decompiled sources (`// Token:` / `// RID:` property comments). Authoritative property types; field counts match oracle `legacy-vo-fields.json` (143 fields total across 5 VOs).

---

## 7. Legacy UI surface confirmed present in the shipped assembly

All eight legacy UI types **are** in `FusionPOS.exe` (namespace-qualified REA match; 8 types / 233 fields / 150 methods / 6564 member_refs / 4547 call_edges / 2868 field_accesses / 416 user strings).

| Legacy type | Namespace | Base | Modern counterpart | Status | Label |
|---|---|---|---|---|---|
| `ribbonfrmMain` | `FusionPOS` | `XtraForm` (RibbonForm) | `MainForm` | Present, divergent | [Obs] |
| `frmLogin` | `FusionPOS` | `XtraForm` | `LoginForm` / startup auth | Present | [Obs] |
| `ucliveSaleMain` | `FusionPOS.Sales` | `XtraUserControl` | `MainForm` sales surface | Present | [Obs] |
| `ucSalesHistory` | `FusionPOS.Sales` | `XtraUserControl` | `SalesListPage` | Present | [Obs] |
| `frmDelivery` | `FusionPOS.Sales` | `XtraForm` | **none** | Missing | [Obs] |
| `frmBankPayment` (×3 namespaces) | `FusionPOS.Sales`, `FusionPOS.Outstanding`, `FusionPOS.Purchases` | `XtraForm` | **none** | Missing | [Obs] |
| VO types (6) | `business.domain.vo.{Sale,SaleDetail,Stock,Customer,Purchase,LiveSale}` | POCO | modern `Domain\Models` | Present | [Obs] |

> There is **no version drift**: the shipped `FusionPOS.exe` contains all six probed legacy UI types. The earlier contrary conclusion was a bare-name REA false negative (see §1 methodological note).

---

## 8. Modern WinForms defects (verified)

### 8.1 — Designer-contract status

| Fact | Detail | Label |
|---|---|---|
| Modern `src/MMNextPOS.WinForms` contains **0** `.Designer.cs` and **0** `.resx` | 101 `.cs` files, 97 type files, 102 classes | [Obs] |
| Legacy tree contains **414** `.Designer.cs` | — | [Obs] |
| Consequence | The modern UI is **entirely hand-written**; no Visual Studio Designer surface exists to round-trip, and the winforms-expert serialization contract is structurally inapplicable to every current form. | [Inf] |

### 8.2 — Defect ledger

| # | File | Defect | Severity | Label |
|---|---|---|---|---|
| D1 | `ListPage.cs:64–73` | Base constructor calls `protected virtual void InitializeComponent()` → derived overrides execute **during base construction**, mutating a control tree that is not yet built | High | [Resolved] |
| D2 | `SalesListPage.cs:39–60` | `InitializeComponent` override mutates the control tree, uses **absolute `Location`** and **lambda `Click` handlers** — violates the designer serialization contract | High | [Resolved] |
| D3 | `StockTransferEditForm.cs:171` | `private async void LoadEntityData(StockTransfer)` — overload resolution selects the `StockTransfer` overload, so `AsyncFormBase.LoadEntityData(object)` (`AsyncFormBase.cs:137`) is **never invoked**; base virtual contract bypassed | High | [Resolved] |
| D4 | `StockTransferEditForm.cs:184+` | `LoadLocationsAsync` has an **empty `catch { }`** that silently swallows load failures | High | [Resolved] |
| D5 | `StockTransferEditForm.cs:61–62` | Constructor calls `InitializeComponent(); LoadEntityData(_entity);` — async work starts from the constructor | Medium | [Resolved] |
| D6 | `ReportsViewerForm.cs:40` | Constructor calls `LoadReports()` (`async void`) before the handle exists | High | [Resolved] |
| D7 | `ReportsViewerForm.cs:142` | `LoadSelectedReport()` is `async void`; invoked from lambda handlers (`:74`/`:77`) with no awaitable path | High | [Resolved] |
| D8 | `ReportsViewerForm.cs:76–86` | Absolute `Location` positioning; lambda event handlers | Medium | [Resolved] |
| D9 | `MainForm.cs:500` | `async void ApplyUserPreferences()` — no awaitable Task surface | Medium | [Resolved] |
| D10 | `MainForm.cs:~546/~556` | Fire-and-forget `LoadAsync().ConfigureAwait(false)` — `ConfigureAwait(false)` moves continuations **off the UI context**, so subsequent control updates can cross threads | High | [Resolved] |
| D11 | `Program.cs:61–141` | **0 `AddTransient`, 197 `AddScoped`** in the composition root (67 WinForms `Program.cs` + 130 Application `DependencyInjection.cs`), now compliant with AGENTS.md ("Register services as `Scoped` for WinForms"). | Medium | [Resolved] |
| D12 | `AsyncFormBase.cs:76` | `Application.DoEvents()` inside `SetWaitCursor` — re-entrancy hazard: the user can re-enter the form while an operation is in flight | High | [Resolved] |
| D13 | `AsyncFormBase.cs:99–127` | `ShowConfirm` / `ShowError` / `ShowInfo` use **blocking `Invoke`** — can deadlock against a busy UI thread | Medium | [Resolved] |
| D14 | `AsyncFormBase.cs:57` | Cancellation is caught and **swallowed** (dead cancellation catch); control state is not restored on the cancellation path | Medium | [Resolved] |
| D15 | `ExpenseMonthlySummaryForm.cs` | Plain `Form` (not `AsyncFormBase`); `GenerateSummary()` is `async void` with `CancellationToken.None`; **no `Dispose(bool)` and no CTS**; lambda close handler; local `MessageBox.Show` helpers | High | [Resolved] |
| D16 | `EditFormBase.cs:73`, `:81` | No-op overrides of `AsyncFormBase` virtuals — mask the base contract and give derived types no extension point | Low | [Resolved] |
| D17 | `FormLocalizer.cs` | Myanmar-font cache held in `ConditionalWeakTable` is **never disposed** | Low | [Resolved] |
| D18 | 5 UI classes | `ExpenseMonthlySummaryForm` (`Form`), `ReportsViewerForm` (`XtraForm`), `DraftSelectorForm` (`XtraForm`), `ProductSelectorForm` (`XtraForm`), `ReportParameterForm` (`XtraForm`) **changed to inherit `AsyncFormBase`** → CTS added, `Dispose(bool)` overridden, error path via `RunAsync`/`ShowError`. `ConnectionTestPage` (`XtraUserControl`) was listed but **not changed** — it is a `XtraUserControl`, not a `Form`, and is outside the D11–D18 scope. | High | [Resolved] |

### 8.3 — Async / threading census (modern, measured)

| Metric | Value |
|---|---|
| `.Result` / `.Wait()` | **0** ✔ (AGENTS.md compliant) |
| `async void` | **0** ✔ — all 18 converted to void wrapper + `async Task` pattern (verified: zero `async void` remain in source) |
| `Task.Run` | 1 (`Program.cs:42`, health-check endpoint) |
| `BindingList<T>` / `ObservableCollection<T>` | 8 / **0** — `ObservableCollection` is not a drop-in WinForms `DataSource`; no current use, so no defect |
| `InvokeAsync` / `BeginInvoke` | 0 / 1 (`AsyncFormBase.cs`) |
| `AddScoped` / `AddTransient` | **197 / 0** — 130 `AddScoped` in `Application\DependencyInjection.cs` + 67 `AddScoped` in `WinForms\Program.cs`; zero `AddTransient` anywhere |

---

## 9. Residual unknowns

| # | Unknown | Why it remains open |
|---|---|---|
| U1 | REA `inspect_analysis_view` rejects `view.offset` / `view.limit` (`invalid_type, expected number`) | 64-row default page cannot be advanced; remaining legacy user strings (416 total) and methods (150 total) were partially retrieved only |
| U2 | REA evidence does not survive server/session restart (`evidence_integrity_mismatch`) | Evidence IDs above may require the producer to be re-run to re-materialise |
| U3 | All 5 decoded VO field shapes | **Resolved** — `Sale` (36), `SaleDetail` (16), `Stock` (45), `Customer` (18), `Purchase` (28) decoded in §7a and persisted to `legacy-vo-fields.json` |
| U4 | Report **logic** parity not proven | 169 legacy report SPs counted by name only; their SQL bodies, filters, groupings and rounding were not diffed against the 12 modern reports |
| U5 | Runtime UI / Designer round-trip | **Not run** — no interactive desktop session and no Visual Studio in this environment |
| U6 | Docker/Testcontainers integration suite | **Not run** — Docker Desktop availability not verified in this segment |
| U7 | Legacy stored-procedure bodies | **Confirmed unreachable statically** — names + inline SQL extracted from assemblies; procedure bodies live in the legacy MySQL database only |
| U8 | Redesign-scaffold corroboration | `spCalculateStockBalance(p_LocationID)` in `database\MMNextPOS_Schema.sql` is the modern redesign scaffold — an independent per-location stock computation that corroborates §3 but is *not* a legacy dump; do not hand-edit target rows during migration incidents |

**Corroboration note:** `database\MMNextPOS_Schema.sql` defines `spCalculateStockBalance(p_LocationID)` — a per-location stored-procedure that computes stock balance independently of the Domain Model. It corroborates §3 (per-location stock is modeled in both systems) but is a redesign scaffold, not legacy provenance.

---

## 10. Validation record (from the completed earlier chain — no new build/test this segment)

- **Build:** `dotnet build MMNextPOS.slnx --configuration Release` → **succeeded**, 0 errors, 1 DX1000 DevExpress evaluation warning.
- **Format:** `dotnet format MMNextPOS.slnx --verify-no-changes` → **exit 0**.
- **Tests:** `dotnet test tests\MMNextPOS.Application.Tests\MMNextPOS.Application.Tests.csproj --configuration Release` with `DOTNET_ROLL_FORWARD=LatestMajor` → **330 passed / 0 failed / 0 skipped**.
  - *Environment note:* .NET 8 runtime is **not installed** locally (only 10.0.14 and 11.0.0-rc.3). `DOTNET_ROLL_FORWARD=LatestMajor` + `DOTNET_ROLL_FORWARD_TO_PRERELEASE=1` is the working workaround. No `global.json` was added; the solution stays on .NET 8.
- **Runtime UI:** **not run** — no interactive desktop session.
- **Designer round trip:** **not run** — no Visual Studio; moreover the project has no `.Designer.cs`/`.resx` surface to round-trip (§8.1).
- **Integration tests:** **not run** — Docker Desktop not verified.
- **Preserved:** project policy, `.editorconfig`, solution/TFM, generated/startup files, and all repository files unchanged (`git status --porcelain` clean).

---

## 11. Extraction method (reproducible)

Legacy literal extraction over `J:\Project 1\POS\business.*.dll` (30 files):

1. Read raw bytes; scan **byte pairs** at a 2-byte stride.
2. Accept a pair when `high == 0 && 32 <= low <= 126` (UTF-16LE printable ASCII) and append the low byte; otherwise flush the run.
3. Flush runs ≥ 4 chars; match `\bsp_[A-Za-z0-9_]{3,}\b`, `(?i)TRIGGER\s+[`"]?(tg_[A-Za-z0-9_]{6,})`, `\b(?:t|tx)_[a-z][a-z0-9_]{2,}\b`.
4. Second pass over `Encoding.ASCII.GetString(bytes)` to catch single-byte literals.
5. Lower-case-normalise procedure names for counting (734 unique); keep original casing in evidence.

**Prefix-vs-substring distinction (critical):** `sp_stock*` (192 hits) mixes prefix forms (`sp_stock_*`, 17) and substring forms (`sp_stock<word>*`, 175). Family counts in this ledger were recomputed using **both** prefix (`^sp_<family>`) **and** substring (`sp_<family>`) matching per family, then deduplicated — e.g. servicing resolved to 69 SPs (not the stale `^sp_service*` prefix-only figure of 49) and report family SPs resolved to 169 (not 196). All figures cross-checked against `legacy-oracle.json` and `J:\Project 1\FusionPOS\FusionPOS.csproj` (154 `Compile Include` lines match `rpt*`).

**Traps encountered**
- Naïve regex over `Encoding.Unicode.GetString(bytes)` **misaligns** and yields corrupt tokens (e.g. `update✀?param_de`).
- Incrementing the index **both** in the `for` header (`$i += 2`) and inside the printable branch (`$i++`) advances **3 bytes per character** and silently yields an **all-zero** result.
- `TypeNames` for REA must be **namespace-qualified**.
- Stored-procedure **names** are recoverable from the assemblies; procedure **bodies** are not (they live in the legacy database) — see U7.
