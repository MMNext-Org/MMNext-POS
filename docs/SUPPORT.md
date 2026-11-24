# BREWFLOW POS — Support Guide

## Overview

This Support Guide provides troubleshooting steps, contact information, and privacy rights information for BREWFLOW POS users and administrators.

---

## 1. Troubleshooting

### Login Issues

| Problem | Possible Cause | Solution |
|---------|---------------|----------|
| Cannot log in | Invalid email or password | Verify credentials and try again |
| Account locked | Too many failed attempts | Contact your administrator to unlock |
| Session expired | Inactivity timeout | Log in again |
| Page won't load | Network or browser issue | Refresh page; check internet connection; try a different browser |

### Shift Issues

| Problem | Possible Cause | Solution |
|---------|---------------|----------|
| Cannot open shift | Another shift may be open for your branch | Contact your Branch Manager |
| Shift close rejected | Branch Manager did not approve | Review rejection notes; correct issues; re-request close |
| Cannot hand over shift | No eligible colleagues available | Another staff member must be available to receive the handover |
| Cash drawer doesn't balance | Cash in/out not recorded correctly | Review cash transactions; contact Branch Manager |

### Tips Allocation Issues

| Problem | Possible Cause | Solution |
|---------|---------------|----------|
| Allocate button disabled | Allocated amounts don't equal total | Adjust item amounts so they sum exactly to the total tip amount |
| Cannot allocate tips | Shift not approved or not closed | Ensure shift is closed and Branch Manager has approved |

### Loyalty Issues

| Problem | Possible Cause | Solution |
|---------|---------------|----------|
| Cannot award points | Points value must be positive | Enter a positive integer for points |
| Cannot redeem points | Insufficient balance | Verify customer has enough points; enter a lower amount |
| Customer not found | Customer may not be enrolled | Enroll the customer first |

### Reports Issues

| Problem | Possible Cause | Solution |
|---------|---------------|----------|
| Reports link not visible | Your role is Cashier | Reports require Shift Lead or Branch Manager role |
| Access denied on /reports | Insufficient role | Contact administrator to update your role |
| Export button disabled | No data matches filters | Adjust date range or report type |
| PDF export blocked | Browser popup blocker | Allow popups for the site; use HTML fallback download |

### Privacy Issues

| Problem | Possible Cause | Solution |
|---------|---------------|----------|
| Cannot export data | No customer data available | Verify customer records exist |
| Cannot delete customer | Customer may have dependencies | Contact administrator |

---

## 2. Contact Information

For technical support, account issues, or feature requests:

- **System Administrator:** Contact your organization's BREWFLOW POS administrator
- **Security Issues:** Report immediately to your security team
- **General Support:** support@brewflow.example.com (replace with your organization's support email)

When contacting support, please include:
- Your name and role
- Organization and branch
- Description of the issue
- Steps to reproduce (if applicable)
- Screenshots or error messages (if available)

---

## 3. Privacy Rights

### Right to Access (Data Export)

You can request a copy of your personal data at any time:

1. Navigate to **Privacy**.
2. Click **Prepare Export** under Data Export.
3. Download the JSON file containing your customer profile, loyalty account, and transaction history.

### Right to Erasure (Data Deletion)

You can request permanent deletion of your customer data:

1. Navigate to **Privacy**.
2. Select your name from the customer dropdown.
3. Click **Delete Customer Data**.
4. Confirm permanent deletion.

**Warning:** Deletion is permanent and cannot be undone. All loyalty points, transactions, and account data will be removed.

---

## 4. Security

### Password Security

- Use a strong, unique password.
- Never share your password with anyone, including managers or IT staff.
- Log out when you finish your shift or step away from the register.

### Role-Based Access

BREWFLOW POS enforces role-based access control:
- **Cashier:** Orders, shifts, cash in/out, loyalty operations
- **Shift Lead:** All Cashier permissions + reports export
- **Branch Manager:** All Shift Lead permissions + shift close approval, tips oversight

Reports are restricted to Shift Lead and Branch Manager roles. Cashiers cannot access the Reports page.

### Data Handling

- Customer data is stored securely and scoped to your organization/branch.
- Export data is provided as JSON for your personal records.
- Deletion requests are processed permanently.

---

*BREWFLOW POS Support Guide — Version 1.0*