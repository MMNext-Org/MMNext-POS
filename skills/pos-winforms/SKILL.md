---
name: pos-winforms
description: MMNext POS WinForms UI patterns — forms, list pages, navigation, DevExpress controls, disposal.
---

# POS WinForms UI Skill

This skill covers the presentation layer of MMNext POS: WinForms forms, list pages, navigation, DevExpress controls, and disposal patterns.

## Key Files

- `src/MMNextPOS.WinForms/MainForm.cs` — Main shell, wires list pages and functional areas.
- `src/MMNextPOS.WinForms/NewSaleForm.cs` — Sales entry form.
- `src/MMNextPOS.WinForms/SalesListPage.cs` — Sales listing page.
- List pages and edit forms exist for: customers, suppliers, products, purchases, inventory, stock transfers, expenses, payments, settings, reports, backups, migrations, SuperAdmin.

## UI Patterns

- `AsyncFormBase` — base class for async forms with cancellation and progress.
- Navigation composition via a dedicated service/factory (not duplicated in MainForm).
- Standardized error handling, confirmation dialogs, and loading states.
- Forms must support cancellation tokens and not block the UI thread.
- Proper `Dispose(bool)` and disposal of controls/resources.

## DevExpress

- WinForms uses DevExpress controls (grid, edit, print).
- Confirm DevExpress package feed/license setup before building.
- Evaluation notices must not appear in production artifacts.

## Keyboard/Barcode

- Barcode scanner input flows through product search.
- Keyboard navigation should work across all forms.
- Duplicate scan handling for sales lines.

## Localization

- Myanmar Unicode/Zawgyi support for text, grids, reports, receipts, PDF/XLS export.
- `MyanmarFontConverter.cs` for font conversion boundaries.
- Test print preview, physical print, PDF, and XLS outputs.

## When to Use

- Creating or modifying WinForms forms and list pages.
- Adding navigation items to MainForm.
- Implementing barcode, keyboard, or print flows.
- Reviewing UI for disposal, cancellation, and error handling.

## Verification

- `dotnet build MMNextPOS.slnx --configuration Release` — build gate.
- `dotnet format --verify-no-changes` — format gate.
- UI smoke tests for navigation and open/close cycles.