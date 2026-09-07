# MMNext POS — Parity Matrix Index

This directory holds the traceable parity matrix that maps every legacy FusionPOS screen, menu item, report, voucher, field, validation rule, and permission to the status of its replacement in MMNext POS.

## Legend

| Status | Meaning |
|---|---|
| `Verified` | New implementation exists and has been tested against the intended behavior. |
| `Implemented—needs QA` | Code exists but lacks documented scenario coverage or parity evidence. |
| `Partial` | Main path exists, but one or more important workflows, validations, permissions, or print/export paths are missing. |
| `Missing` | No usable replacement exists. |
| `Not in scope` | Explicitly excluded and recorded with an owner-approved reason. |

## Priority

| Priority | Meaning |
|---|---|
| `P0 — Critical` | Blocks financial or stock accuracy, data safety, or release readiness. |
| `P1 — High` | Required for cashier/operations daily work. |
| `P2 — Medium` | Required for a complete workflow or administration. |
| `P3 — Low` | Nice-to-have or deferred with approved reason. |

## Files

| File | Module | Description |
|---|---|---|
| [PARITY-MATRIX.md](PARITY-MATRIX.md) | All modules | Master traceability matrix (single source of truth). |

## Owner Abbreviations

| Owner | Role |
|---|---|
| App Lead | Application Layer Lead |
| Infra Lead | Infrastructure / DB Lead |
| WinForms Lead | Presentation Layer Lead |
| Reports Lead | Reports / Vouchers Lead |
| Localization Owner | Myanmar Unicode / Fonts |
| Security Owner | Authorization / Licensing |
| Ops Owner | Backup / Restore / Migration |
| QA Lead | Testing / Verification |
| Product Owner | Scope / Acceptance |

## Rules

1. A component may never be promoted to `Verified` on the basis of file existence alone; test scenario results, parity evidence artifacts, and an accepted-difference note (where applicable) are required.
2. Every `Missing` component must have a priority and an owner.
3. Any `Not in scope` decision must record an owner-approved reason.
4. The matrix is updated at the end of each sprint and before each milestone exit.