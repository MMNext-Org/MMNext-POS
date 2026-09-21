# POS System User Acceptance Test (UAT) Script

## Test Information

| Field | Value |
|-------|-------|
| **Project** | MMNext POS - Point of Sale System |
| **Version** | v1.0 (UAT) |
| **Tester Name** | ___________________ |
| **Test Date** | ___________________ |
| **Test Environment** | ☐ Development ☐ Staging ☐ Production |
| **Tester Role** | ☐ Cashier ☐ Manager ☐ Admin |

---

## Instructions for Tester

1. Read each test step carefully before starting
2. Follow the steps exactly as written
3. Check the "Result" box and record actual outcome
4. Be honest: if something doesn't work as expected, mark it "Failed"
5. Add comments when something is confusing or unexpected
6. Save and submit the completed form after each test session

---

## Pre-Test Setup Checklist

Before starting tests, verify:

- [ ] Application is running and logged in
- [ ] Test data is loaded (or system has default data)
- [ ] Printer is connected and working (for print tests)
- [ ] Barcode scanner is connected (if applicable)
- [ ] You have the necessary permissions for your role

---

## Test Suite 1: User Login & Permissions

### TC-001: Login with Valid Credentials

| Step | Action | Expected Result | Actual Result | Pass/Fail |
|------|--------|-----------------|---------------|-----------|
| 1 | Open the application | Login screen appears | | ☐ Pass ☐ Fail |
| 2 | Enter username: `admin` | Username field accepts input | | ☐ Pass ☐ Fail |
| 3 | Enter password: `admin123` | Password field accepts input (masked) | | ☐ Pass ☐ Fail |
| 4 | Click Login button | Dashboard/Main screen appears | | ☐ Pass ☐ Fail |

**Comments:**

---

### TC-002: Role-Based Menu Access

| Step | Action | Expected Result | Actual Result | Pass/Fail |
|------|--------|-----------------|---------------|-----------|
| 1 | Login as Cashier | Main menu shows relevant items | | ☐ Pass ☐ Fail |
| 2 | Try to access Users/Roles | Access denied or menu not visible | | ☐ Pass ☐ Fail |
| 3 | Login as Admin | Main menu shows ALL items including Users/Roles | | ☐ Pass ☐ Fail |
| 4 | Access Users/Roles as Admin | List page opens showing user list | | ☐ Pass ☐ Fail |

**Comments:**

---

## Test Suite 2: Sale Entry (Core Cashier Workflow)

### TC-003: Create New Casual Sale (Walk-in Customer)

| Step | Action | Expected Result | Actual Result | Pass/Fail |
|------|--------|-----------------|---------------|-----------|
| 1 | Click "New Sale" | New Sale form opens | | ☐ Pass ☐ Fail |
| 2 | Add product: scan or search `SKU001` | Product added to cart with name, price | | ☐ Pass ☐ Fail |
| 3 | Add another product: `SKU002` | Both products show in cart | | ☐ Pass ☐ Fail |
| 4 | Change quantity to 3 on first item | Line total updates to Price × 3 | | ☐ Pass ☐ Fail |
| 5 | Add discount of 5 to one item | Discount applied, showing in line | | ☐ Pass ☐ Fail |
| 6 | Add another product with 8 quantity | Sale Total = Sum of all lines | | ☐ Pass ☐ Fail |
| 7 | Click "Complete" (F10 or complete button) | Receipt prints (or shows preview) | | ☐ Pass ☐ Fail |
| 8 | Stock of items reduced after sale | Product list shows reduced quantity | | ☐ Pass ☐ Fail |

**Expected Stock Calculation:**
- Product A: Initial stock: 10 → Sold: 3 → Remaining: 7
- Product B: Initial stock: 5 → Sold: 1 → Remaining: 4

**Comments:**

---

### TC-004: Barcode Scanner Input

| Step | Action | Expected Result | Actual Result | Pass/Fail |
|------|--------|-----------------|---------------|-----------|
| 1 | Open New Sale form | Form ready for input | | ☐ Pass ☐ Fail |
| 2 | Scan barcode of Product 1 | Product auto-adds to cart | | ☐ Pass ☐ Fail |
| 3 | Scan barcode of Product 2 | Second product auto-adds | | ☐ Pass ☐ Fail |
| 4 | Scan same product again | Quantity increases (no duplicate row) | | ☐ Pass ☐ Fail |
| 5 | Scan expired/invalid barcode | Error message shown, nothing added | | ☐ Pass ☐ Fail |

**Comments:**

---

### TC-005: Price Discounts

| Step | Action | Expected Result | Actual Result | Pass/Fail |
|------|--------|-----------------|---------------|-----------|
| 1 | Open New Sale, add products | Products in cart | | ☐ Pass ☐ Fail |
| 2 | Apply percentage discount (10%) | 10% off the total | | ☐ Pass ☐ Fail |
| 3 | Apply fixed amount discount ($5) | $5 off the total | | ☐ Pass ☐ Fail |
| 4 | Try discount > 100% | System rejects with error message | | ☐ Pass ☐ Fail |
| 5 | Apply negative discount | System allows (increases price) or shows confirmation | | ☐ Pass ☐ Fail |

**Comments:**

---

## Test Suite 3: Hold & Resume Sales

### TC-006: Hold an Active Sale

| Step | Action | Expected Result | Actual Result | Pass/Fail |
|------|--------|-----------------|---------------|-----------|
| 1 | Start New Sale, add 5 items | Items in cart | | ☐ Pass ☐ Fail |
| 2 | Click "Hold" (F5 or Hold button) | Sale marked as "On Hold" status | | ☐ Pass ☐ Fail |
| 3 | Close New Sale form | Form closes without errors | | ☐ Pass ☐ Fail |
| 4 | Go to Sale Drafts list | Held sale appears in list | | ☐ Pass ☐ Fail |
| 5 | Resume the held sale | Items restored exactly as before | | ☐ Pass ☐ Fail |
| 6 | Complete the sale | Sale completes successfully | | ☐ Pass ☐ Fail |

**Comments:**

---

### TC-007: Resume from Draft (After Shutdown)

| Step | Action | Expected Result | Actual Result | Pass/Fail |
|------|--------|-----------------|---------------|-----------|
| 1 | Start sale, add items | Items in cart | | ☐ Pass ☐ Fail |
| 2 | Hold the sale | Sale marked "On Hold" | | ☐ Pass ☐ Fail |
| 3 | Close application completely | Application closes | | ☐ Pass ☐ Fail |
| 4 | Restart application, login | Application opens normally | | ☐ Pass ☐ Fail |
| 5 | Go to Sale Drafts | Your draft is listed | | ☐ Pass ☐ Fail |
| 6 | Resume the draft | All items, quantities, prices match exactly | | ☐ Pass ☐ Fail |
| 7 | Complete the sale | Sale completes, receipt prints | | ☐ Pass ☐ Fail |

**Comments:**

---

## Test Suite 4: Sale Cancellation & Void

### TC-008: Void Completed Sale

| Step | Action | Expected Result | Actual Result | Pass/Fail |
|------|--------|-----------------|---------------|-----------|
| 1 | Create and complete a sale | Sale marked Complete, receipt printed | | ☐ Pass ☐ Fail |
| 2 | Go to Sales List | Completed sale appears in history | | ☐ Pass ☐ Fail |
| 3 | Select the sale, click "Void" | Confirmation dialog appears | | ☐ Pass ☐ Fail |
| 4 | Enter void reason: "Customer changed mind" | Reason saved | | ☐ Pass ☐ Fail |
| 5 | Confirm void | Sale status changes to "Voided" | | ☐ Pass ☐ Fail |
| 6 | Check stock levels | Stock restored to original counts | | ☐ Pass ☐ Fail |
| 7 | Check invoice/tracking | Void appears in audit log | | ☐ Pass ☐ Fail |

**Comments:**

---

## Test Suite 5: Returns & Exchanges

### TC-009: Process Return/Refund

| Step | Action | Expected Result | Actual Result | Pass/Fail |
|------|--------|-----------------|---------------|-----------|
| 1 | Complete a sale first | Sale completed with products | | ☐ Pass ☐ Fail |
| 2 | Go to Returns/Exchanges | Returns list accessible | | ☐ Pass ☐ Fail |
| 3 | Select the completed sale | Sale details load | | ☐ Pass ☐ Fail |
| 4 | Select items to return (1 item) | Selected item highlighted | | ☐ Pass ☐ Fail |
| 5 | Process return | Return receipt generated | | ☐ Pass ☐ Fail |
| 6 | Check stock of returned item | Stock increased by returned quantity | | ☐ Pass ☐ Fail |
| 7 | Check customer account | Credit appears for returned amount | | ☐ Pass ☐ Fail |

**Comments:**

---

### TC-010: Exchange (Return + New Sale)

| Step | Action | Expected Result | Actual Result | Pass/Fail |
|------|--------|-----------------|---------------|-----------|
| 1 | Customer brings item back | Item can be identified | | ☐ Pass ☐ Fail |
| 2 | Choose new item at same value | Exchanged item added | | ☐ Pass ☐ Fail |
| 3 | Complete exchange | Credit equal to item applied | | ☐ Pass ☐ Fail |
| 4 | No payment required | New sale allowed without payment | | ☐ Pass ☐ Fail |
| 5 | Check stock | Original stock restored, new item deducted | | ☐ Pass ☐ Fail |

**Comments:**

---

## Test Suite 6: Stock Management

### TC-011: Stock Adjustment (Increase)

| Step | Action | Expected Result | Actual Result | Pass/Fail |
|------|--------|-----------------|---------------|-----------|
| 1 | Go to Stock Adjustment | Form opens | | ☐ Pass ☐ Fail |
| 2 | Select product `SKU001` | Product selected | | ☐ Pass ☐ Fail |
| 3 | Enter quantity: 20 | Quantity entered | | ☐ Pass ☐ Fail |
| 4 | Enter reason: "Restocking" | Reason saved | | ☐ Pass ☐ Fail |
| 5 | Save adjustment | Stock quantity updated | | ☐ Pass ☐ Fail |
| 6 | Check stock | Initial + 20 added | | ☐ Pass ☐ Fail |

**Comments:**

---

### TC-012: Stock Transfer Between Locations

| Step | Action | Expected Result | Actual Result | Pass/Fail |
|------|--------|-----------------|---------------|-----------|
| 1 | Go to Stock Transfers | List page opens | | ☐ Pass ☐ Fail |
| 2 | Click "New Transfer" | Edit form opens | | ☐ Pass ☐ Fail |
| 3 | Set From Location: "Main Store" | Selected | | ☐ Pass ☐ Fail |
| 4 | Set To Location: "Branch 2" | Selected | | ☐ Pass ☐ Fail |
| 5 | Add product with quantity 10 | Product added to list | | ☐ Pass ☐ Fail |
| 6 | Save transfer | Transfer created with Draft status | | ☐ Pass ☐ Fail |

**Comments:**

---

### TC-013: Serial Number Tracking

| Step | Action | Expected Result | Actual Result | Pass/Fail |
|------|--------|-----------------|---------------|-----------|
| 1 | Receive product with serials | Serial numbers entered | | ☐ Pass ☐ Fail |
| 2 | Sell product with serial number | Serial tracked in sale | | ☐ Pass ☐ Fail |
| 3 | Sell another unit | Next serial auto-increments | | ☐ Pass ☐ Fail |
| 4 | Return item with serial | Serial status updated to Returned | | ☐ Pass ☐ Fail |
| 5 | Check serial history | All movements logged | | ☐ Pass ☐ Fail |

**Comments:**

---

## Test Suite 7: Payments & Outstanding

### TC-014: Cash Payment

| Step | Action | Expected Result | Actual Result | Pass/Fail |
|------|--------|-----------------|---------------|-----------|
| 1 | Complete sale with cash payment $100 | Sale processes | | ☐ Pass ☐ Fail |
| 2 | Tender Cash: $100 | Drawer opens or transaction completes | | ☐ Pass ☐ Fail |
| 3 | Change calculation shows $20 | Correct change shown | | ☐ Pass ☐ Fail |
| 4 | Receipt shows correct amount | Payment method: Cash, change: $20 | | ☐ Pass ☐ Fail |

**Comments:**

---

### TC-015: Card Payment

| Step | Action | Expected Result | Actual Result | Pass/Fail |
|------|--------|-----------------|---------------|-----------|
| 1 | Complete sale, select Card payment | Card option selected | | ☐ Pass ☐ Fail |
| 2 | Enter card details (last 4: 1234) | Details accepted | | ☐ Pass ☐ Fail |
| 3 | Complete transaction | Payment processed | | ☐ Pass ☐ Fail |
| 4 | Check payment record | Card payment recorded with details | | ☐ Pass ☐ Fail |

**Comments:**

---

### TC-016: Customer Credit & Outstanding

| Step | Action | Expected Result | Actual Result | Pass/Fail |
|------|--------|-----------------|---------------|-----------|
| 1 | Create sale on credit (no payment) | Sale marked as Credit | | ☐ Pass ☐ Fail |
| 2 | Check Outstanding list | Customer shows owing balance | | ☐ Pass ☐ Fail |
| 3 | Record partial payment $50 | Balance reduces by $50 | | ☐ Pass ☐ Fail |
| 4 | Record overpayment | Overpayment becomes credit | | ☐ Pass ☐ Fail |
| 5 | Clear all outstanding | Balance becomes zero, status "Cleared" | | ☐ Pass ☐ Fail |

**Comments:**

---

## Test Suite 8: Reporting

### TC-017: Daily Sales Summary

| Step | Action | Expected Result | Actual Result | Pass/Fail |
|------|--------|-----------------|---------------|-----------|
| 1 | Open Reports > Daily Summary | Report form opens | | ☐ Pass ☐ Fail |
| 2 | Select today's date | Today's date auto-selected | | ☐ Pass ☐ Fail |
| 3 | Generate report | Today's totals shown (sales count, total, cash, card, credit) | | ☐ Pass ☐ Fail |
| 4 | Verify totals match manual count | System and manual amounts agree | | ☐ Pass ☐ Fail |
| 5 | Print/Save report | PDF or printed output generated | | ☐ Pass ☐ Fail |

**Comments:**

---

### TC-018: Stock Report

| Step | Action | Expected Result | Actual Result | Pass/Fail |
|------|--------|-----------------|---------------|-----------|
| 1 | Open Stock Movement Report | Report form opens | | ☐ Pass ☐ Fail |
| 2 | Set date range: Today | Date range set | | ☐ Pass ☐ Fail |
| 3 | Generate report | All stock movements for today listed | | ☐ Pass ☐ Fail |
| 4 | Filter by type: Sale | Only Sale movements shown | | ☐ Pass ☐ Fail |
| 5 | Filter by type: Return | Only Return movements shown | | ☐ Pass ☐ Fail |

**Comments:**

---

## Test Suite 9: Purchasing & Supplier

### TC-019: Create Purchase Order

| Step | Action | Expected Result | Actual Result | Pass/Fail |
|------|--------|-----------------|---------------|-----------|
| 1 | Go to Purchases > New | Purchase form opens | | ☐ Pass ☐ Fail |
| 2 | Select supplier | Supplier selected | | ☐ Pass ☐ Fail |
| 3 | Add items to purchase | Items in cart | | ☐ Pass ☐ Fail |
| 4 | Set expected quantities and prices | Line totals calculated | | ☐ Pass ☐ Fail |
| 5 | Save as draft | Draft saved for later | | ☐ Pass ☐ Fail |
| 6 | Complete purchase | Stock increased accordingly | | ☐ Pass ☐ Fail |

**Comments:**

---

## Test Suite 10: General Application

### TC-020: Navigation & Menu Structure

| Step | Action | Expected Result | Actual Result | Pass/Fail |
|------|--------|-----------------|---------------|-----------|
| 1 | Open Main Form | All main menus visible | | ☐ Pass ☐ Fail |
| 2 | Verify sub-menus exist for your role | Role-appropriate menus shown | | ☐ Pass ☐ Fail |
| 3 | Open each main menu | No errors on open | | ☐ Pass ☐ Fail |
| 4 | Switch between pages quickly | No crashes or lag | | ☐ Pass ☐ Fail |
| 5 | Reset and reopen application | Preferences remembered | | ☐ Pass ☐ Fail |

**Comments:**

---

### TC-021: Data Entry Validation

| Step | Action | Expected Result | Actual Result | Pass/Fail |
|------|--------|-----------------|---------------|-----------|
| 1 | Try to create customer without name | Error message shown | | ☐ Pass ☐ Fail |
| 2 | Try negative price on product | Rejected or auto-corrected | | ☐ Pass ☐ Fail |
| 3 | Try invalid email format | Validation shows "invalid email" | | ☐ Pass ☐ Fail |
| 4 | Try special characters in name | Progresses or clear error | | ☐ Pass ☐ Fail |
| 5 | Try extremely long input | Limited by field max length | | ☐ Pass ☐ Fail |

**Comments:**

---

## Sign-Off

### Test Completion Status

| Summary | Count |
|---------|-------|
| Total Tests Executed | _____ |
| Tests Passed | _____ |
| Tests Failed | _____ |
| Pass Rate | _____% |

### Critical Issues Found

1. __________________________________________________
2. __________________________________________________
3. __________________________________________________

### Tester Sign-Off

I certify that I have executed all assigned test cases and recorded the results accurately.

**Tester Name:** ___________________________ **Date:** ___________ **Signature:** ___________________

**Reviewed By:** ___________________________ **Date:** ___________ **Approval:** ☐ Approved ☐ Rejected (Reason: ___________)

---

## Appendix A: Test Data Requirements

These prerequisites are needed before starting tests:

1. **Products:** At least 5 products with different SKUs, prices, and stock levels
2. **Customers:** At least 3 customers including walk-in
3. **Suppliers:** At least 1 supplier for purchases
4. **Payment Methods:** Cash and Card configured
5. **Locations:** At least 2 locations if testing transfers

---

## Appendix B: Common Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| F1 | Help |
| F10 | Complete Sale |
| F5 | Hold Sale |
| Esc | Cancel/Close |
| Ctrl+N | New Sale |
| Ctrl+S | Save |

**Note:** Do not memorize shortcuts; use the UI buttons primarily. Shortcuts are aids, not requirements.

---

## Appendix C: Contact Support

**For technical issues:**
- Document the exact error message or unexpected behavior
- Note the step where it occurred
- Save a screenshot if possible
- Share the completed form with the QA team

**Environment Details for Issues:**
- OS Version: Windows 10/11
- App Version: v1.0
- Database: MySQL
- Report issues via GitHub Issues or Slack #uat-final

---

*End of UAT Test Script — MMNext POS v1.0*