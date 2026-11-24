# BREWFLOW POS — Administrator Guide

## Overview

Welcome to the BREWFLOW POS Administrator Guide. This document is for system administrators responsible for configuring user roles, managing tenant access, generating reports, and overseeing day-to-day system operations.

BREWFLOW POS is a point-of-sale system supporting shift management, manager approval for shift close, tips allocation, loyalty enrollment/redemption, privacy data export/deletion, and role-based reports export (CSV/PDF).

---

## 1. User Roles and Permissions

BREWFLOW POS uses role-based access control (RBAC). The following roles are defined:

| Role | Permissions |
|------|-------------|
| **Cashier** | Open shifts, process orders, take cash in/out, view own shift data, participate in loyalty transactions |
| **Shift Lead** | All Cashier permissions + view and export reports (Orders, Shifts, Loyalty, Sales) |
| **Branch Manager** | All Shift Lead permissions + approve/reject shift closures, finalize shift close, tips allocation oversight |

### Reports Access

Reports are restricted to **Shift Lead** and **Branch Manager** roles only. Cashiers cannot see the Reports navigation link and will receive an access-denied message if they attempt to navigate to `/reports`.

---

## 2. Getting Started

### Prerequisites

- A modern web browser (Chrome, Firefox, Edge, or Safari)
- Network access to the BREWFLOW POS application
- Valid user credentials issued by your system administrator

### First Login

1. Navigate to the BREWFLOW POS application URL.
2. Click **Sign In**.
3. Enter your email and password.
4. Upon successful login, your organization and branch context will be loaded automatically from your user profile.

---

## 3. Navigation

The top navigation bar provides access to the following sections:

| Link | Description | Required Role |
|------|-------------|---------------|
| Home | Dashboard landing page | All |
| Dashboard | Business overview | All |
| Orders | Order management | All |
| Shifts | Shift open/close management | All |
| Loyalty | Customer loyalty program | All |
| Privacy | Data export and deletion | All |
| Reports | CSV/PDF report export | Shift Lead / Branch Manager |
| Profile | User profile settings | All |
| Logout | Sign out of the application | All |

**Note:** The Reports link only appears for users with Shift Lead or Branch Manager roles.

---

## 4. Shift Management

### Opening a Shift

Before processing any sales, a staff member must open a register shift:

1. Navigate to **Shifts**.
2. Select the staff member opening the shift from the dropdown.
3. Enter the opening float amount (cash starting in the drawer).
4. Click **Open Shift**.

The shift status will change to `open`, and the cash drawer dashboard will display cash in, cash out, and net total.

### Cash In / Cash Out

While a shift is open, you can record cash movements:

- **Cash In:** Add cash to the drawer (e.g., for making change).
- **Cash Out:** Remove cash from the drawer (e.g., for safe drop).

Enter the amount and click the corresponding button.

### Requesting Shift Close

When a staff member is ready to end their shift:

1. Click **Request Shift Close**.
2. The shift status changes to `pending_approval` with approval status `none`.
3. A Branch Manager must review and approve the close before it can be finalized.

### Manager Approval for Shift Close

Branch Managers can review pending shift closures:

1. Navigate to **Shifts** while a shift is in `pending_approval` state.
2. Click **Review Manager Approval** to open the approval modal.
3. Review the shift details:
   - Cash in / cash out / net total
   - Tips allocated (if any)
4. Enter an approval reason or notes.
5. Click **Approve & Close** or **Reject**.

- **Approved:** The shift moves to approved status and can be finalized.
- **Rejected:** The shift remains open with a rejection reason recorded.

### Shift Handover

If a staff member needs to transfer an open shift to a colleague:

1. While a shift is open, click **Hand Over Shift**.
2. Select an eligible colleague from the dropdown (all staff except the current shift holder).
3. Review the current shift holder and cash float summary.
4. Click **Hand Over Shift**.

The shift status changes to `handed_over` and the colleague becomes the new shift holder.

### Finalizing a Shift Close

Once a Branch Manager has approved a shift close:

1. Click **Finalize Close (approved)**.
2. The shift status changes to `closed`.
3. The shift is now eligible for tips allocation.

---

## 5. Tips Allocation

After a shift is closed and approved, tips can be distributed among staff:

1. Navigate to **Shifts**.
2. Click **Allocate Tips** on the closed shift.
3. Enter the total tip amount.
4. Add one or more recipients:
   - Select a staff member from the dropdown.
   - Enter the amount to allocate to that staff member.
   - Repeat for additional recipients.
5. Verify that allocated amounts equal the total tip amount.
6. Click **Allocate Tips**.

**Validation:** The system requires that all allocated item amounts sum exactly to the total tip amount. The allocate button is disabled until the amounts match.

---

## 6. Loyalty Program

### Enrolling a Customer

1. Navigate to **Loyalty**.
2. In the "Find or Enroll a Customer" section, fill in:
   - Full name (required)
   - Email (optional)
   - Phone (optional)
3. Click **Enroll Customer**.

The customer is created with a zero points balance and "Standard" tier. Enrollment consent (version 1.0) is recorded, and an opening-balance ledger entry is written.

Duplicate enrollment for an already-enrolled customer returns the existing account without creating duplicate accounts or ledger entries.

### Searching for Customers

Use the search box to find existing customers by name, email, or phone. Matching results appear as a list; click a customer to view their loyalty details.

### Earning Points

To award points to a customer:

1. Open the customer's detail view.
2. In the **Earn Points** section, enter:
   - Points to award (must be a positive integer)
   - Reason (e.g., "Coffee purchase")
3. Click **Award Points**.

The customer's points balance increases, and an append-only ledger transaction record is created.

### Redeeming Points

Redemption uses a two-phase reserve-and-commit process:

1. Open the customer's detail view.
2. In the **Redeem Points** section, enter:
   - Points to redeem (cannot exceed available balance)
   - Reason (e.g., "Free coffee reward")
3. Click **Redeem Points**.

Points are reserved, then committed in a single server transaction. The customer's points balance decreases and a redemption ledger entry is recorded. Redemption is idempotent: retrying with the same reference does not double-deduct.

At checkout (Order Creation), points can also be redeemed as a discount: 100 points = $1.00 off the order total. The reservation is released if the order is not completed.

### Disenrolling a Customer

To permanently remove a customer from the loyalty program:

1. Open the customer's detail view.
2. Click **Disenroll**.
3. Confirm the disenrollment in the dialog.

**Warning:** Disenrollment is permanent and cannot be undone.

- A forfeiture ledger entry is written, reducing the available points balance to zero. The forfeited amount is recorded.
- Historical ledger rows are preserved (append-only ledger); they are not deleted.
- The account status becomes `disenrolled`. Disenrolled accounts cannot earn, reserve, or redeem points.
- Disenrollment is rejected while a redemption reservation is active for the account.
- Re-enrollment starts at zero points; forfeited points are never restored.

---

## 7. Privacy & Data (Right to Access / Erasure)

BREWFLOW POS supports privacy law compliance with data export and deletion capabilities.

### Data Export

1. Navigate to **Privacy**.
2. In the **Data Export** section, click **Prepare Export**.
3. The system generates a versioned, machine-readable JSON file (version `1.0`) containing:
   - Customer profiles (name, email, phone, consent)
   - Loyalty accounts (points balance, tier)
   - Loyalty transactions (earn/redeem/adjust/expire history)
   - Consent history (type, given/withdrawn, version, timestamp)
   - Summary statistics (customer count, account count, transaction count, consent record count, total points)
4. Click **Download JSON** to save the file to your computer.

**Export rules:**
- The download filename includes the version and date: `brewflow-privacy-export-v1.0-<YYYY-MM-DD>.json`.
- Empty histories are included as empty arrays, not omitted.
- A customer with no loyalty account is represented with `null` for that section.
- No credentials, secret configuration, internal authorization data, or other-tenant/customer records are included.
- Exports are scoped to your organization only.

### Data Deletion (Disenrollment)

1. Navigate to **Privacy** or **Loyalty**.
2. Select a customer from the dropdown.
3. Click **Delete Customer Data** / **Disenroll**.
4. Confirm permanent disenrollment in the dialog.

**Warning:** Deletion/disenrollment is permanent and cannot be undone.

- A forfeiture ledger entry reduces the available points balance to zero; the forfeited amount is recorded.
- Historical loyalty ledger rows are preserved (append-only ledger) for audit purposes.
- The account status becomes `disenrolled`. Disenrolled accounts cannot earn, reserve, or redeem points.
- Disenrollment is blocked while a redemption reservation is active for the account.
- Re-enrollment starts at zero points; forfeited points are never restored.
- Financial and audit records are not silently hard-deleted; personal identifiers remain as anonymized references where legally required.

---

## 8. Reports Export (Shift Lead / Branch Manager Only)

### Generating a Report

1. Navigate to **Reports** (only visible to Shift Lead and Branch Manager roles).
2. Select report type:
   - **Orders** — Order records
   - **Shifts** — Shift records
   - **Loyalty Customers** — Customer loyalty records
   - **Sales Summary** — Sales data (reuses order preparation)
3. Select format:
   - **CSV** — Comma-separated values (opens in Excel, Google Sheets, etc.)
   - **PDF** — Printable document via browser print dialog
4. Optionally set a date range (Start Date and End Date).
5. Review the data preview table (shows up to 50 records).
6. Click **Export CSV** or **Export PDF**.

### CSV Export

- Downloads a `.csv` file with the report data.
- Nested objects and arrays are JSON-stringified within cells.
- Compatible with spreadsheet applications.

### PDF Export

- Opens the browser print dialog for a printable report.
- If the popup is blocked, an HTML fallback download is provided.

### Access Control

- Reports are restricted to **Shift Lead** and **Branch Manager** roles.
- Cashiers attempting to access `/reports` will see an access-denied page explaining the restriction.
- The Reports navigation link is hidden for non-manager roles.

---

## 9. Tenant / Organization Context

BREWFLOW POS supports multi-organization deployments. Each user is associated with:

- **Organization** — The business entity (e.g., "Central Café")
- **Branch** — The specific location (e.g., "Main Street")
- **Role** — The user's permission level (Cashier, Shift Lead, Branch Manager)

Your organization and branch are loaded automatically at login from your user profile metadata. Shift data, orders, and loyalty records are scoped to your organization context.

---

## 10. Troubleshooting

| Issue | Possible Cause | Resolution |
|-------|---------------|------------|
| Cannot see Reports link | Your role is Cashier | Reports require Shift Lead or Branch Manager role |
| Shift close rejected | Branch Manager rejected the close | Review rejection reason; adjust shift and re-request close |
| Tips allocation blocked | Allocated amounts don't equal total | Adjust item amounts so they sum exactly to the total |
| Cannot redeem points | Insufficient points balance | Verify customer has enough points; enter a lower amount |
| Export button disabled | No data available for selected filters | Adjust date range or report type; verify data exists |
| Login fails | Invalid credentials or session expired | Re-enter credentials; contact admin if locked out |

---

## 11. Support

For technical issues, account management, or feature requests:

- **Contact:** Your system administrator or BREWFLOW POS support team
- **Documentation:** This guide, plus the POS User Guide and Staff Handbook
- **Security Concerns:** Report security issues immediately to your security team

---

*BREWFLOW POS Administrator Guide — Version 1.0*