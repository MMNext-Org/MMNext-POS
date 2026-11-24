# BREWFLOW POS — POS User Guide

## Overview

Welcome to the BREWFLOW POS User Guide. This document is for cashiers and front-line staff who use the POS system daily to process orders, manage shifts, handle cash, and operate the loyalty program.

---

## 1. Getting Started

### Logging In

1. Navigate to the BREWFLOW POS application URL.
2. Click **Sign In**.
3. Enter your email and password.
4. Click **Sign In**.

### Logging Out

Click your profile name in the top navigation bar, then click **Logout**.

---

## 2. Orders

### Placing an Order

1. Navigate to **Orders**.
2. Select items from the menu to add to the cart.
3. Adjust quantities as needed.
4. Select a payment method.
5. Click **Place Order**.

The order is recorded in the system and associated with your current open shift.

### Viewing Order History

All completed orders are listed in the Orders page. You can view order details including items, total amount, payment method, and timestamp.

---

## 3. Shift Management

### Opening Your Shift

Before you can process any sales, you must open a register shift:

1. Navigate to **Shifts**.
2. Select your name from the staff dropdown.
3. Enter your opening float amount (the cash you start with in the drawer).
4. Click **Open Shift**.

Your shift status changes to `open` and the cash drawer dashboard becomes active.

### Cash Drawer Dashboard

While your shift is open, the dashboard shows:

- **Cash In** — Cash added to the drawer
- **Cash Out** — Cash removed from the drawer
- **Net Total** — Opening float + Cash In − Cash Out

### Recording Cash In

To add cash to the drawer (e.g., for making change):

1. Click **Cash In**.
2. Enter the amount.
3. Click **Record**.

### Recording Cash Out

To remove cash from the drawer (e.g., for a safe drop):

1. Click **Cash Out**.
2. Enter the amount.
3. Click **Record**.

### Requesting Shift Close

When you are ready to end your shift:

1. Click **Request Shift Close**.
2. Your shift status changes to `pending_approval`.
3. A Branch Manager must review and approve the close before it is finalized.

### Shift Handover (Hand Shift to Colleague)

If you need to transfer your open shift to another staff member:

1. While your shift is open, click **Hand Over Shift**.
2. Select a colleague from the dropdown list.
3. Review your current shift holder name and cash float summary.
4. Click **Hand Over Shift**.

Your shift status changes to `handed_over` and your colleague takes over as shift holder.

---

## 4. Loyalty Program

### Enrolling a New Customer

1. Navigate to **Loyalty**.
2. Fill in the customer details:
   - Full name (required)
   - Email (optional)
   - Phone (optional)
3. Click **Enroll Customer**.

### Searching for a Customer

Use the search box to find customers by name, email, or phone number. Click a result to view their loyalty details.

### Awarding Points

To award points to a customer:

1. Open the customer's detail view.
2. Enter the number of points to award (must be a positive number).
3. Enter a reason (e.g., "Latte purchase").
4. Click **Award Points**.

### Redeeming Points

To deduct points from a customer's balance:

1. Open the customer's detail view.
2. Enter the points to redeem (cannot exceed available balance).
3. Enter a reason (e.g., "Free coffee reward").
4. Click **Redeem Points**.

Points are reserved and committed in a single server transaction. The customer's points balance decreases, and a redemption ledger entry is recorded.

### Redeeming Points at Checkout (Order Discount)

When placing an order, you can apply a customer's loyalty points as a discount:

1. Navigate to **Orders** and begin creating an order.
2. In the **Customer & Loyalty** section, search for and select a customer.
3. The customer's loyalty points balance and tier are displayed.
4. Enter the number of points to redeem (100 points = $1.00 discount). The discount and new total update automatically.
5. Click **Submit Order**.

The points are reserved and committed before the order is created. If the order fails, the reservation is released and the points are returned.

### Disenrolling a Customer

To permanently remove a customer from the loyalty program:

1. Navigate to **Loyalty** or **Privacy**.
2. Select the customer.
3. Click **Disenroll** / **Delete Customer Data**.
4. Confirm in the dialog.

**Warning:** Disenrollment is permanent and cannot be undone. A forfeiture entry reduces the available points to zero; historical ledger rows are preserved. Disenrolled accounts cannot earn, reserve, or redeem points. Re-enrollment starts at zero.

---

## 5. Privacy — Your Data Rights

### Exporting Your Data

You can export your customer data as a versioned JSON file:

1. Navigate to **Privacy**.
2. Click **Prepare Export** under Data Export.
3. The system generates a JSON file (version `1.0`) containing your profile, loyalty account, transactions, and consent history.
4. Click **Download JSON** to save your data.

The download filename includes the version and date: `brewflow-privacy-export-v1.0-<YYYY-MM-DD>.json`.

### Deleting Your Data

To permanently delete your customer account:

1. Navigate to **Privacy**.
2. Select your name from the customer dropdown.
3. Click **Delete Customer Data**.
4. Confirm permanent deletion.

**Warning:** Deletion is permanent and cannot be undone.

---

*BREWFLOW POS User Guide — Version 1.0*