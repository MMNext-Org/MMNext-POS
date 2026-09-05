# MMNext POS — Next Steps (post-merge with origin/main)

## Context
The branch was fast-forwarded onto origin/main, which brought a large body of already-merged work that pre-empted the Phase-D gap-fill plan. Current state:

| Layer | Count | Status vs. roadmap |
|---|---|---|
| Domain models | 77 | Phase B essentially complete (~150 VOs target; masters + sales + purchases + inventory + warehouse + starman + license DTOs present) |
| Repositories | 137 | Phase C complete (generic base + per-domain, paging, soft-delete) |
| Service interfaces | 54 | Phase D complete (Inventory, Purchase, Outstanding, Expense, Setting, License, Backup, Report, Migration, SuperAdmin, etc.) |
| WinForms UI | 60+ screens | Phase E-H largely done (ListPage base, MainForm nav, ~40 ListPages/EditForms, LiveSale, ReportsViewer, Stock/Purchase/Outstanding/Expense forms) |
| Tests | 13 service suites + infra | M1 unit-test portion green (113 passing); integration needs Docker |

## Remaining gaps (verified against code, not the stale plan)

1. **Login / authentication screen** — absent. `IUserService` exists but no `LoginForm`; Program.cs launches MainForm directly.
2. **License / Registration UI** — absent. `ILicenseInfoService`/`LicenseInfoService` and models (`LicenseInfo`, `DeviceInfo`, `Registration`, `Subscription`) exist, but no UI screen or device-binding flow.
3. **Theme / Language / Myanmar-font converter** — absent entirely (no `ILanguageService`/`IThemeService`, no font converter, no `UITheme` class).
4. **Backup / Restore / Data-migration UI** — `IBackupService` (256-line impl) and `IMigrationService` (218-line impl) exist, but no UI screens.
5. **SuperAdmin UI** — `ISuperAdminService` (297-line impl) exists, but no screens (deleted-views, change-date log viewer, JSON log history, script executor).
6. **Reports & vouchers (Phase I)** — `IReportService`/`ReportService` (164 lines) + `ReportsViewerForm` + Star* report DTOs exist, but the ~96 `rpt*` definitions and ~100 print vouchers from FusionPOS are **not enumerated or implemented**. This is the largest remaining body of work.
7. **Dashboard** — `DashboardWidget` model exists; no `DashboardForm`/`ucQuickSummary` UI.

## Recommended next steps (priority order)

### 1. Authentication & app shell (highest value, unblocks everything)
- `LoginForm` (async login via `IUserService`, role resolution via `IUserRoleService`/`IRoleService`).
- `Program.cs`: show LoginForm -> on success launch MainForm with current `User` + `Role` context.
- `IMainNavigationService` already exists; wire role-based menu visibility (currently MainForm hardcodes pages).

### 2. License & device binding
- `LicenseInfoService`/`LicenseRegistrationForm`: enter key -> validate via existing offline `ILicenseInfoService`; bind `DeviceInfo` fingerprint (CPU/HDD/MAC).
- Enforce expiry on startup (route to registration screen when `ExpiryDate` passed).

### 3. Theme / Language / Myanmar fonts (cross-cutting)
- Add `ILanguageService` + `IThemeService` (change events); `Language`/`Theme` models + repos already exist in the model set.
- Port FusionPOS `ucMyanmarFontConverter` (Zawgyi<->Unicode) — high-risk area, do early.

### 4. Reports & vouchers (Phase I) — largest, do incrementally
- Enumerate the ~96 `rpt*` + ~100 voucher classes from `J:\Project 1\POS\FusionPOS` (do NOT guess).
- Prioritise top-10 (Sale receipt, Sale invoice, Purchase invoice, Stock list, P&L, Cash Flow, Outstanding, barcode labels).
- Reimplement each as `XtraReport` fed by `IReportService`; wire into `ReportsViewerForm` with a parameter form.

### 5. Backup / Restore / Migration / SuperAdmin UI (Phase J)
- `BackupRestoreForm` over `IBackupService` + `IMigrationService`.
- `SuperAdminForm`: change-date log viewer, deleted/invoice history views, JSON log viewer, script executor.

### 6. Parity QA & packaging (Phase K/L)
- Side-by-side parity checklist (already drafted in a prior turn).
- 10k-row perf tests; memory-leak audit; `dotnet publish -r win-x64 --self-contained`; WiX MSI; migration CLI; tag `v1.0.0`.

## Validation gate per step (Engineering rule 5)
- `dotnet build MMNextPOS.slnx` must stay 0 warnings / 0 errors (`TreatWarningsAsErrors=True` already on in all 4 projects).
- New suites added to `tests/MMNextPOS.Application.Tests`; run `dotnet test` (skip Docker-dependent infra tests locally, or rely on CI where Docker is present).
- Audit-log row per write action (`IAuditService` / `ChangeDateLogs` already implemented upstream).
