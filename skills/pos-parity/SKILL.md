---
name: pos-parity
description: Legacy FusionPOS parity matrix — trace each screen, field, report, voucher, and permission to MMNext POS implementation status.
---

# POS Parity Skill

This skill covers the parity matrix between legacy FusionPOS and MMNext POS. Use it to verify that legacy features have replacements with documented status and test coverage.

## Parity Statuses

| Status | Meaning |
|--------|---------|
| `Verified` | New implementation exists and has been tested against intended behavior. |
| `Implemented—needs QA` | Code exists but lacks documented scenario coverage or parity evidence. |
| `Partial` | Main path exists, but workflows, validations, permissions, or print/export paths are missing. |
| `Missing` | No usable replacement exists. |
| `Not in scope` | Explicitly excluded with owner-approved reason. |

## Key Files

- `docs/parity/PARITY-MATRIX.md` — Master parity matrix.
- `docs/parity/Sales-Hardening-Parity.md` — Sales parity checklist.
- `docs/parity/Phase3-Purchasing-Contacts-Payments-Expenses-Parity.md` — Phase 3 parity.
- `docs/parity/Entity-Schema-Mapping.md` — Legacy-to-new entity mapping.
- `plan.md` — Implementation roadmap with gap matrix.
- `Legacy-Module-Inventory.md` — Reference scope of legacy screens.

## Scope Areas

Sales, Contacts, Inventory, Purchases, Outstanding/Payments, Expenses, Warehouse/Stock Transfer, Starman multi-site workflows, Dashboard, Reports, Print Vouchers, Settings, SuperAdmin, License, Menus, API Controller, database utilities.

## When to Use

- Before starting a feature, check its parity status.
- After implementing a feature, update the parity matrix with status, test scenario, and evidence.
- During QA, verify each completed legacy feature has a status, test scenario, evidence artifact, and accepted-difference note.

## Verification

- Parity matrix maintained under `docs/parity/` and linked from `plan.md`.
- Each feature has a documented status and acceptance criteria.
- Output comparisons against legacy PDFs/images for reports and vouchers.