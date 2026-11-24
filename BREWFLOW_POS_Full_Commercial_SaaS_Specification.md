# BREWFLOW POS — Full Commercial SaaS Product Specification

**Document type:** Product Requirements + Technical Architecture + Implementation Blueprint  
**Product:** Multi-tenant Coffee Shop POS and Operations SaaS  
**Platform:** Responsive Web Application / Installable PWA (Desktop, Tablet, Mobile)  
**Cloud:** Supabase (PostgreSQL, Auth, RLS, Storage, Edge Functions)  
**Local-first sync:** PowerSync with local SQLite  
**Frontend:** React + TypeScript + Vite  
**Status:** Developer-ready baseline specification; implementation decisions must be validated during technical discovery.

---

## 1. Product Vision

Build a commercial-grade, multi-tenant SaaS platform for independent coffee shops and multi-branch café operators. It must manage the full operating lifecycle: point of sale, orders, barista workflow, recipes, stock, purchasing, suppliers, customers, loyalty, staff, shifts, cash, expenses, reporting, branches, and SaaS subscriptions.

The POS must be **offline-first**. Cashiers should continue to create and manage permitted transactions when internet access is interrupted. The local database is the immediate operational data source for the POS. Supabase PostgreSQL is the central cloud source of truth after successful synchronization and server-side validation.

### Product objectives

1. Complete common checkout flows quickly with minimal interaction.
2. Continue essential POS workflows during temporary internet outages.
3. Keep tenant and branch data strictly isolated.
4. Calculate recipe consumption and product cost consistently.
5. Give owners useful operational and financial visibility.
6. Support one shop initially and scale to multiple branches and many SaaS tenants.
7. Provide a fast, accessible interface on desktop, tablet, and mobile browsers.

### Non-goals for the first release

- Building a custom payment processor.
- Storing card numbers, CVV, or other payment-card credentials.
- Guaranteeing offline authorization for online payment gateways.
- Replacing a full statutory accounting package in every country.
- Providing native iOS/Android applications in the first release; ship an installable PWA first.

---

## 2. Confirmed Product Decisions

| Area | Decision |
|---|---|
| Delivery | Responsive web app and installable PWA |
| Devices | Desktop, tablet, mobile |
| Business model | Multi-tenant SaaS sold to independent coffee shops |
| Scope | Full commercial product |
| Cloud database | Supabase PostgreSQL |
| Local database | SQLite in the PowerSync client SDK |
| Sync | PowerSync cloud/self-hosted sync service connected to Supabase |
| Authentication | Supabase Auth |
| Tenant isolation | `organization_id` + PostgreSQL Row Level Security |
| Frontend | React, TypeScript, Vite |
| Visual design | Lightweight, clean, fast, touch-friendly UI |

### Important architecture decision

Use **PowerSync as the local-first synchronization layer**, subject to a short technical proof of concept and commercial pricing review. Supabase's partner documentation describes PowerSync as a Postgres-to-SQLite sync layer: data is replicated into an in-app local SQLite database, local writes are queued for upload, Sync Rules control data distribution, and backend conflict handling is configurable.

Do not implement a second custom synchronization engine on top of PowerSync. Keep business rules and validation in shared domain logic and secure server-side handlers.

Fallback option if PowerSync is rejected after the proof of concept: Dexie.js + IndexedDB with a custom outbox/inbox sync engine. This is a larger engineering responsibility and must include retries, idempotency, conflict resolution, migrations, and sync observability. Do not mix both strategies in the same production data path.

---

## 3. Architecture Overview

```text
Desktop / Tablet / Mobile Browser (PWA)
                  |
          React + TypeScript UI
                  |
       Application / Domain Layer
                  |
       PowerSync Client SDK
           /           \
 Local SQLite          Sync stream / upload queue
           \           /
          PowerSync Service
                  |
      Supabase PostgreSQL
       /       |       \
    Auth      RLS     Storage
                  |
         Supabase Edge Functions
                  |
   Payment providers / billing / webhooks
```

### Responsibilities

**PWA client**
- Render the UI and manage ephemeral view state.
- Query operational data from local SQLite.
- Apply validated local-first commands in SQLite transactions.
- Display online/offline/sync/conflict states.
- Cache the application shell and static assets.
- Never contain Supabase service-role or secret keys.

**PowerSync**
- Maintain the local SQLite replica.
- Download only data allowed by Sync Rules.
- Queue client writes for upload.
- Expose synchronization status and upload errors.
- Provide a path to custom upload and conflict-resolution logic.

**Supabase**
- PostgreSQL: canonical cloud data store.
- Auth: identity and sessions.
- RLS: tenant, branch, and membership isolation.
- Storage: product images, logos, attachments, exports.
- Edge Functions: trusted commands, tenant provisioning, billing integration, payment callbacks, privileged operations.
- Realtime: selective transient events/presence where PowerSync replication is not the right mechanism.

### Technology recommendations

| Concern | Recommendation |
|---|---|
| Build tool | Vite |
| UI | React + TypeScript |
| Styling | Tailwind CSS |
| Primitive components | Radix UI; selectively use shadcn/ui components |
| Icons | Lucide, tree-shaken imports |
| Routing | TanStack Router or React Router; choose one |
| UI state | Zustand for ephemeral client state only |
| Forms | React Hook Form + Zod |
| Local data | PowerSync SDK / SQLite |
| Cloud auth & storage | Supabase JS client |
| Date handling | date-fns or Day.js; standardize timezone rules |
| Testing | Vitest, Testing Library, Playwright |
| Database tests | pgTAP / `supabase test db` |
| Monitoring | Sentry or equivalent error monitoring + structured server logs |

Do not use a heavyweight global state store as a second source of business data. Business records should be read from the local database via reactive queries; Zustand is for UI-only state such as open dialogs, selected category, and temporary search text.

---

## 4. SaaS Tenancy and Organization Model

### Hierarchy

```text
Platform
  ├── Tenant / Organization A
  │     ├── Branch A1
  │     │     ├── POS Device A1-01
  │     │     └── POS Device A1-02
  │     └── Branch A2
  └── Tenant / Organization B
        └── Branch B1
```

- `organization` represents one SaaS customer/business account.
- `branch` represents one physical shop.
- A user may belong to one or more organizations.
- A user may have different roles in different branches.
- A device is registered to an organization and, where applicable, a branch.
- A platform-level super administrator is distinct from a tenant owner.

### Tenant data rule

Every tenant-owned table must have a non-null `organization_id`. Tables containing branch-specific records should also have a non-null `branch_id`. Organization-level catalog records can have a null `branch_id` or use an explicit scope field.

The frontend may add `organization_id` filters for query efficiency, but filters are **not** a security control. PostgreSQL RLS must enforce tenant isolation on every exposed table. RLS must also validate writes (`WITH CHECK`) so a user cannot insert or update a row into another tenant.

### Organization lifecycle

`trial` → `active` → `past_due` / `suspended` → `cancelled` / `archived`.

Suspension must be policy-driven. Define which read-only access remains available to tenant owners and how pending offline transactions are handled before suspending a tenant. Never silently discard queued POS transactions.

---

## 5. User Roles and Permission Model

Use permission keys, not hard-coded role names throughout the UI. Roles are bundles of permissions. The same permission may be granted differently by tenant and branch.

| Role | Main capabilities | Restrictions |
|---|---|---|
| Platform Super Admin | SaaS tenants, plans, support, platform health | No routine access to tenant financial data; audited break-glass access only |
| Tenant Owner | All tenant branches, users, reports, settings, billing | Cannot manage platform internals |
| Tenant Admin | Catalog, users, branches, operations | Billing / ownership transfer may be restricted |
| Branch Manager | Branch sales, shifts, stock, staff, approvals | Cannot access other branches unless explicitly granted |
| Cashier | POS, order lookup, permitted payment and receipt actions | Refunds, voids, large discounts require approval |
| Barista | KDS, item preparation status, order notes | No financial reports or refunds |
| Inventory Clerk | Stock counts, receiving, transfers, wastage | No payment or employee payroll access |
| Accountant | Sales summaries, expenses, tax and financial reports | No menu or order editing by default |
| Auditor | Read-only approved records and audit logs | No writes |

### Permission examples

`pos.sell`, `pos.discount.apply`, `pos.discount.override`, `orders.view`, `orders.void`, `orders.refund`, `kds.update`, `catalog.view`, `catalog.manage`, `inventory.view`, `inventory.adjust`, `inventory.count`, `purchasing.manage`, `staff.manage`, `shifts.open`, `shifts.close`, `cash.adjust`, `reports.sales`, `reports.finance`, `branches.manage`, `users.manage`, `billing.manage`, `audit.view`.

Every privileged action must be checked in the domain/server layer as well as reflected in the UI. Hiding a button is not authorization.

---

## 6. Functional Module Requirements

### 6.1 Platform Admin / SaaS Console
- Tenant directory and tenant profile.
- Trial, plan, subscription, and billing status.
- Usage metrics: branches, active users, registered devices, orders, storage.
- Feature entitlements by plan.
- Tenant suspension/reactivation with audit trail.
- Support notes and support access workflow.
- Platform health, sync backlog, error rates, and service status.
- No direct unrestricted tenant-data browsing in normal support workflows.

### 6.2 Tenant Onboarding
- Create organization and first owner.
- Business name, address, country, timezone, base currency, locale.
- Create first branch and register first device.
- Choose tax behavior and receipt configuration.
- Optional starter menu import/template.
- Create default roles and permissions.
- Show onboarding checklist and test-print flow.

### 6.3 Dashboard
- Gross sales, net sales, order count, average order value.
- Discounts, refunds, tax, service charge, tips.
- Product/category sales mix.
- Gross profit and recipe cost estimates.
- Low-stock and expiry alerts.
- Wastage and stock adjustments.
- Current shifts and cash variance.
- Branch comparisons for authorized users.
- Date/branch/channel filters.
- Clearly distinguish realized sales, tax collected, gross profit, and net profit.

### 6.4 POS Terminal
- Product grid with category tabs and fast search.
- Barcode scanner support as keyboard input.
- Variants: size, temperature, roast, etc.
- Modifier groups: milk, sugar, ice, toppings, extra shot.
- Per-item special instructions.
- Dine-in, takeaway, pickup, delivery, QR table order.
- Table number and guest count when needed.
- Hold, resume, merge, and split orders with strict permissions.
- Item discount, order discount, coupon, service charge, tax.
- Split tender and partial payment.
- Cash tender, change due, payment reference.
- Reprint receipt and kitchen ticket.
- Void, cancel, and refund reason with approval rules.
- Keyboard shortcuts and touch-optimized large controls.

**Checkout calculation order (configurable by jurisdiction):**

1. Sum line item prices and modifiers.
2. Apply item-level discounts.
3. Apply eligible order-level discounts/coupons.
4. Calculate service charge according to configuration.
5. Calculate taxable base and tax according to inclusive/exclusive tax settings.
6. Calculate final amount due and payment allocation.

Store a snapshot of applied product names, SKU, price, modifiers, discounts, tax rates, and currency on each order item. Editing a current product must not alter historical receipts.

### 6.5 Order Management
Separate **order lifecycle** from **payment lifecycle**.

Suggested order states: `draft`, `placed`, `accepted`, `preparing`, `ready`, `served`, `completed`, `cancelled`.

Suggested payment states: `unpaid`, `partially_paid`, `pending`, `paid`, `partially_refunded`, `refunded`, `failed`.

- Order timeline and status history.
- Channel, device, cashier, branch, and table context.
- Search by receipt number, local device code, customer, or time range.
- Partial item cancellation and refund rules.
- Full/partial refund records; never delete the original payment/order history.

### 6.6 Kitchen Display System (KDS)
- Live incoming tickets.
- Separate stations: espresso, cold drinks, bakery, kitchen.
- Per-item modifiers and special notes.
- Prep timer, priority, delayed-order warning.
- Assign/claim ticket and mark items ready.
- Order ready and completed transitions.
- Optional audio notification with mute control.
- Offline queue badge and last-sync indicator.

### 6.7 Product and Menu Management
- Categories, products, variants, modifier groups/options.
- Product images, SKU, barcode, description.
- Price lists and branch-specific prices.
- Tax category and revenue category.
- Availability schedule and sold-out state.
- Combo/set menu and add-on rules.
- Scheduled promotions and happy-hour pricing.
- Bulk CSV import/export with validation.

### 6.8 Recipes and Costing
- Recipe/BOM per product variant.
- Ingredient quantities and units.
- Yield, waste factor, and preparation loss.
- Ingredient price history.
- Recipe cost calculated from ingredient cost layers or configured costing method.
- Theoretical stock usage compared with actual stock usage.
- Gross profit and gross margin per item.
- Recipe revision history; orders keep a recipe/cost snapshot where needed.

### 6.9 Inventory
- Ingredients, units, unit conversions, stock locations.
- Current stock by branch/location.
- Stock-in, stock-out, waste, damage, adjustment.
- Stock count and variance approval.
- Minimum stock and reorder threshold.
- Expiry/batch/lot tracking where required.
- Stock movement ledger.
- Transfer request, dispatch, receive, and discrepancy flow.
- Recipe-driven theoretical consumption.
- Manual consumption and waste reason.

**Inventory rule:** Use an append-only stock movement ledger. Derive or maintain balances from validated movements. Do not let routine users directly overwrite stock quantity without an adjustment record and reason.

### 6.10 Purchasing and Suppliers
- Supplier directory and contacts.
- Supplier-specific item codes and price history.
- Purchase request and purchase order.
- Partial goods receipt.
- Supplier invoice reference and payment terms.
- Purchase return and discrepancy.
- Purchase cost update workflow.

### 6.11 Customer CRM and Loyalty
- Customer profile, consent, contact details, birthday (optional).
- Order history and preferences.
- Loyalty account and points ledger.
- Earn/redeem rules, tiers, rewards, coupons.
- Referral and campaign support as later integrations.
- Customer export/deletion workflows according to privacy law.
- Minimize customer data replicated to POS devices; sync only what that device needs.

### 6.12 Staff, Shifts, and Cash Drawer
- Employee profiles and organization/branch membership.
- PIN or secure device login option.
- Clock-in/out and attendance.
- Shift scheduling (optional initial release).
- Register open, opening float, cash in/out, close, counted cash.
- Expected vs actual cash variance.
- Shift sales and tender summary.
- Tips and service charge allocation rules.
- Shift handover and manager approval.

### 6.13 Finance and Expenses
- Expense categories and recurring expenses.
- Supplier payable references (not a full accounting ledger unless explicitly scoped).
- Daily cash summary.
- Sales/tax/refund/discount reconciliation.
- Operating expense summary.
- Profit report with clearly stated assumptions.
- Export CSV/PDF.

### 6.14 Reports and Analytics
- Sales by date, hour, branch, channel, cashier, product, category.
- Average order value and customer return rate.
- Best/worst sellers.
- Recipe cost and gross margin.
- Stock valuation, turnover, expiry, waste, and variance.
- Purchase trends and supplier pricing.
- Shift and cash reconciliation.
- Customer lifetime value (where enough data exists).
- Branch comparison and target tracking.

### 6.15 Settings and Integrations
- Business profile, timezone, locale, currency, tax, receipt footer.
- Branch hours, order channels, service charges.
- Printers and kitchen stations.
- Payment provider configuration.
- Email/SMS/notification provider.
- Delivery/accounting integrations where supported.
- Data export, retention, and backup controls.

---

## 7. Offline-First Data and Synchronization Design

### 7.1 Data classes

| Data class | Offline behavior | Sync direction |
|---|---|---|
| Menu, categories, modifiers | Read locally; cloud-managed updates | Cloud → local |
| Branch settings, tax config | Read locally; changes require manager approval | Cloud → local |
| Orders | Create/read/update via local transaction and event log | Bidirectional |
| Cash payments | Can be recorded locally as cash tender | Local → cloud |
| Online QR/card payments | Require provider confirmation; do not mark paid offline | Cloud/provider → local |
| KDS state | Local state changes allowed per station | Bidirectional |
| Stock movements | Record local provisional movements | Bidirectional |
| Customer details | Limited cached profile data | Selective |
| Reports | Cached summaries only; label as last synced | Cloud → local |
| SaaS billing/subscriptions | Online-only | Cloud → platform UI |

### 7.2 Local write rules

- Every local mutation gets a UUID generated on the client.
- Include `organization_id`, `branch_id`, `device_id`, `actor_user_id`, `created_at_client`, and a monotonic per-device sequence where useful.
- Use an idempotency key for every command that can be retried.
- Write related records in one SQLite transaction (e.g. order + lines + cash payment + local stock movement).
- Store an operation status: `pending`, `uploading`, `synced`, `rejected`, `needs_review`.
- Keep rejected items visible in a manager-facing sync issue queue; never silently drop them.

### 7.3 Conflict policy

Do not apply generic last-write-wins to all business records.

| Data | Conflict policy |
|---|---|
| Menu/settings | Server-authoritative version; clients refresh to latest version |
| Orders | Immutable event history; idempotent command processing |
| Payments/refunds | Append-only transaction records; provider/server confirmation authoritative |
| Stock movement | Append-only movement ledger; reconcile against server stock rules |
| Cash drawer | Append-only cash movement records and shift reconciliation |
| Customer profile | Field-level or explicit manager review for conflicting edits |
| Shift close / stock count | Conflict creates a review task; no silent overwrite |

### 7.4 Critical offline constraints

- Cash payments may be recorded offline. A later sync rejection must remain visible and auditable.
- A QR/card transfer cannot be considered paid merely because a cashier selected that method while offline. Keep it pending until confirmed by the provider/server.
- A payment provider's own offline card mode, if any, is a separate certified integration and must be tested with that provider.
- Two disconnected devices cannot guarantee globally correct shared stock simultaneously. Use provisional local availability, reserved branch buffers, or restrict high-risk stock-sensitive operations while offline.
- Do not assume a browser PWA will run background synchronization after it is closed. Sync guarantees depend on the browser, OS, service worker behavior, and SDK. Display last successful sync time.
- Ask the browser for persistent storage where supported, but still plan for local cache eviction and device loss.

### 7.5 Sync UX

Show a persistent, non-blocking sync status:
- `Online · Synced`
- `Online · Syncing 4 changes`
- `Offline · 4 changes queued`
- `Needs attention · 1 rejected change`
- `Last synced 10:42 AM`

Allow authorized managers to inspect/retry a failed sync item. Do not show misleading green status while unresolved uploads exist.

### 7.6 Device identity and order numbering

Do not rely on a cloud-generated sequential ID for offline receipt numbering. Use UUID primary keys plus a readable local receipt code constructed from branch code, device code, business date, and a per-device counter. Enforce uniqueness on a suitable composite key. A cloud-assigned canonical order number may be attached after synchronization.

---

## 8. Payment, Cash, and Financial Integrity

- Never store full card PAN, CVV, or payment secrets.
- Store provider transaction identifiers and masked display metadata only when allowed by the provider.
- Keep payment status separate from order status.
- Use idempotency keys on checkout, capture, refund, and webhook handling.
- Record payment attempts and provider callbacks as immutable events.
- Refunds must reference the original payment and require reason/permission.
- Do not rewrite past receipts after tax, product price, or discount configuration changes.
- Store monetary values in integer minor units (e.g. satang for THB) with ISO currency code; do not use floating-point arithmetic for money.
- Currency exponent must be configurable by currency; do not assume every currency has two decimal places.
- Use explicit tax modes: inclusive, exclusive, exempt, zero-rated, and jurisdiction-specific rules. Validate with a local accounting/tax specialist before production.

---

## 9. Database Domain Model / ERD Outline

All tenant tables include `organization_id`, and most operational tables include `branch_id`. Use UUID primary keys. Include `created_at`, `updated_at`, `created_by`, `updated_by`, and `version` where appropriate. Financial and stock ledgers should be append-only; use reversal records rather than destructive edits.

### Platform and tenancy
- `organizations`
- `organization_memberships`
- `branches`
- `devices`
- `roles`
- `permissions`
- `role_permissions`
- `membership_roles`
- `plans`
- `subscriptions`
- `subscription_events`

### Catalog and pricing
- `categories`
- `products`
- `product_variants`
- `modifier_groups`
- `modifier_options`
- `product_modifier_groups`
- `price_lists`
- `branch_product_prices`
- `tax_categories`
- `tax_rules`

### Recipes and inventory
- `ingredients`
- `units`
- `unit_conversions`
- `recipes`
- `recipe_items`
- `stock_locations`
- `stock_movements`
- `stock_balances` (derived/materialized balance)
- `stock_counts`
- `stock_count_items`
- `stock_transfers`
- `stock_transfer_items`
- `waste_records`

### Purchasing
- `suppliers`
- `purchase_orders`
- `purchase_order_items`
- `goods_receipts`
- `goods_receipt_items`
- `supplier_invoices`
- `supplier_payments`

### POS and payments
- `registers`
- `register_shifts`
- `cash_movements`
- `orders`
- `order_items`
- `order_item_modifiers`
- `order_events`
- `payments`
- `payment_events`
- `refunds`
- `idempotency_records`

### Customers and loyalty
- `customers`
- `loyalty_accounts`
- `loyalty_transactions`
- `promotions`
- `coupons`
- `coupon_redemptions`

### Staff and finance
- `employees`
- `attendance_records`
- `staff_schedules`
- `expense_categories`
- `expenses`
- `tips_allocations`

### Auditing and system
- `audit_logs`
- `notifications`
- `integration_connections`
- `sync_issue_records`
- `export_jobs`

### Key relationships

```text
organizations 1---N branches
organizations 1---N organization_memberships N---1 auth.users
branches 1---N devices
branches 1---N orders
orders 1---N order_items
order_items N---N modifier_options (via order_item_modifiers)
products 1---N product_variants
product_variants 1---N recipes (versioned)
recipes 1---N recipe_items N---1 ingredients
ingredients 1---N stock_movements
branches 1---N stock_locations
suppliers 1---N purchase_orders 1---N purchase_order_items
purchase_orders 1---N goods_receipts
orders 1---N payments 1---N payment_events
payments 1---N refunds (through references/allocations)
customers 1---N orders
customers 1---1 loyalty_accounts 1---N loyalty_transactions
registers 1---N register_shifts 1---N cash_movements
```

### Essential constraints and indexes
- Composite uniqueness for `(organization_id, branch_code)` and `(organization_id, device_code)`.
- Unique `idempotency_key` within its command scope.
- Index tenant and branch foreign keys used by RLS and Sync Rules.
- Index `(branch_id, created_at)` for orders and stock movements.
- Index `(organization_id, sku)` or `(organization_id, barcode)` where unique by tenant.
- Check constraints for non-negative quantities where appropriate, valid state transitions in command logic, and valid currency amounts.
- Foreign keys should generally prevent orphaned financial or inventory records.

---

## 10. Initial SQL Foundation Example

The following is a **starter foundation**, not the complete production migration for all modules. Extend it with role/permission enforcement, branch-level authorization, timestamps, audit triggers, and pgTAP tests before production.

```sql
create schema if not exists private;

create table public.organizations (
  id uuid primary key default gen_random_uuid(),
  name text not null,
  slug text not null unique,
  status text not null default 'trial'
    check (status in ('trial', 'active', 'past_due', 'suspended', 'cancelled', 'archived')),
  default_currency char(3) not null,
  default_timezone text not null,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);

create table public.organization_memberships (
  id uuid primary key default gen_random_uuid(),
  organization_id uuid not null references public.organizations(id),
  user_id uuid not null references auth.users(id) on delete cascade,
  membership_status text not null default 'active'
    check (membership_status in ('invited', 'active', 'suspended', 'removed')),
  created_at timestamptz not null default now(),
  unique (organization_id, user_id)
);

create table public.branches (
  id uuid primary key default gen_random_uuid(),
  organization_id uuid not null references public.organizations(id),
  branch_code text not null,
  name text not null,
  timezone text not null,
  status text not null default 'active'
    check (status in ('active', 'inactive', 'archived')),
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now(),
  unique (organization_id, branch_code),
  unique (organization_id, id)
);

create table public.devices (
  id uuid primary key default gen_random_uuid(),
  organization_id uuid not null references public.organizations(id),
  branch_id uuid not null,
  device_code text not null,
  display_name text not null,
  device_status text not null default 'active'
    check (device_status in ('active', 'revoked', 'maintenance')),
  last_seen_at timestamptz,
  created_at timestamptz not null default now(),
  foreign key (organization_id, branch_id)
    references public.branches (organization_id, id),
  unique (organization_id, device_code)
);

create index organization_memberships_user_org_idx
  on public.organization_memberships (user_id, organization_id)
  where membership_status = 'active';

create index branches_org_idx on public.branches (organization_id);
create index devices_org_branch_idx on public.devices (organization_id, branch_id);

-- Membership helper. Keep it in a non-exposed schema and grant execute explicitly.
create or replace function private.is_active_org_member(target_org uuid)
returns boolean
language sql
stable
security definer
set search_path = ''
as $$
  select exists (
    select 1
    from public.organization_memberships m
    where m.organization_id = target_org
      and m.user_id = (select auth.uid())
      and m.membership_status = 'active'
  );
$$;

revoke all on function private.is_active_org_member(uuid) from public, anon;
grant usage on schema private to authenticated;
grant execute on function private.is_active_org_member(uuid) to authenticated;

alter table public.organizations enable row level security;
alter table public.organization_memberships enable row level security;
alter table public.branches enable row level security;
alter table public.devices enable row level security;

revoke all on public.organizations, public.organization_memberships,
  public.branches, public.devices from anon, authenticated;

grant select on public.organizations to authenticated;
grant select on public.organization_memberships to authenticated;
grant select, insert, update on public.branches to authenticated;
grant select, insert, update on public.devices to authenticated;

create policy org_member_can_read_organization
on public.organizations for select to authenticated
using (private.is_active_org_member(id));

create policy user_can_read_own_memberships
on public.organization_memberships for select to authenticated
using (user_id = (select auth.uid()));

create policy org_members_can_read_branches
on public.branches for select to authenticated
using (private.is_active_org_member(organization_id));

create policy org_members_can_create_branches
on public.branches for insert to authenticated
with check (private.is_active_org_member(organization_id));

create policy org_members_can_update_branches
on public.branches for update to authenticated
using (private.is_active_org_member(organization_id))
with check (private.is_active_org_member(organization_id));

create policy org_members_can_read_devices
on public.devices for select to authenticated
using (private.is_active_org_member(organization_id));
```

**Production hardening required:** membership alone is not sufficient permission for every write. Add role/permission checks to insert/update/delete policies and trusted commands (for example, only owners/admins may create branches or register devices). Review function ownership and grants in the target Supabase project. Add pgTAP tests for allowed and denied access, including cross-tenant insert/update attempts. Do not copy this example as a complete security policy set without those tests.

### PowerSync Sync Rules planning

Sync only data required by a user's role and assigned branches:
- Tenant-level catalog/settings: organization-scoped and branch overrides.
- POS device data: active product catalog, modifiers, tax config, open/recent orders, register shift, required stock availability, and minimum customer lookup fields.
- KDS device data: orders routed to its assigned preparation station/branch.
- Inventory device data: ingredient and stock data for authorized locations.
- Reports: preferably cloud-generated summaries; do not sync raw financial history to every cashier device.
- SaaS billing and platform administration: online-only; never replicate to tenant POS devices.

Design Sync Rules and RLS together. A row must not be synchronized merely because the client requests it. Re-check the current PowerSync configuration syntax against its official documentation during Stage 0.

---

## 11. API / Command Contract

Use Supabase Data API for safe RLS-protected CRUD where suitable. Use Edge Functions or a trusted backend command handler for operations requiring authorization, validation, idempotency, provider callbacks, or multiple-record transactional behavior. Do not create a custom server that merely proxies every CRUD request.

Suggested functions/routes:

| Function | Purpose |
|---|---|
| `tenant-provision` | Create tenant, owner membership, default branch and settings |
| `sync-upload` | Validate and apply queued local commands in batches |
| `checkout-order` | Validate checkout, finalize server-side records, idempotently process cash/order command |
| `refund-payment` | Validate permission, reason, amount, and original transaction |
| `close-register-shift` | Validate shift totals and finalize close event |
| `approve-stock-adjustment` | Approve and post inventory adjustment |
| `receive-purchase-order` | Validate partial/full goods receipt and stock movement |
| `payment-webhook` | Verify provider signature and process callback idempotently |
| `subscription-webhook` | Update SaaS plan/subscription entitlements |
| `export-report` | Generate authorized report export |

### Command envelope example

```json
{
  "command_id": "uuid",
  "command_type": "order.create",
  "organization_id": "uuid",
  "branch_id": "uuid",
  "device_id": "uuid",
  "actor_user_id": "uuid",
  "idempotency_key": "stable-unique-key",
  "client_created_at": "ISO-8601 timestamp",
  "schema_version": 1,
  "payload": {}
}
```

Server response should return `accepted`, `rejected`, or `needs_review`, plus canonical IDs, server timestamps, validation errors, and a retryable/non-retryable classification. Replaying the same idempotency key must not create a duplicate order/payment/stock movement.

---

## 12. Security and Privacy Requirements

1. Enable RLS on every tenant-exposed table.
2. Test SELECT, INSERT, UPDATE, and DELETE policies for every table where those operations are granted.
3. Revoke broad grants not needed by `anon` and `authenticated`; policies do not remove table grants automatically.
4. Use server-managed authorization claims or membership checks. Never trust user-editable `user_metadata` for roles or tenant access.
5. Never ship Supabase service-role/secret keys in browser bundles.
6. Restrict Storage buckets and object paths by organization/branch.
7. Validate all Edge Function input with schemas; enforce role and tenant membership server-side.
8. Add audit events for refunds, voids, discounts above threshold, stock adjustments, cash movements, permission changes, and billing actions.
9. Rate-limit authentication, sensitive actions, exports, and public endpoints.
10. Use secure cookies/session practices and MFA for owners/admins where feasible.
11. Minimize local replication of customer personal data.
12. Protect shared devices with PIN re-authentication, auto-lock, and device revocation.
13. Do not claim the browser local database is encrypted at rest unless the chosen SDK/runtime and deployment actually provide that guarantee. If stronger local encryption is required, evaluate a native wrapper and secure key storage.
14. Maintain retention, export, deletion, and incident-response procedures appropriate to customer jurisdictions.

### RLS policy approach

Use a shared-schema multi-tenant model with `organization_id` on tenant tables and a membership/role check in RLS. For users belonging to multiple organizations, make the active organization a server-verified context and still validate membership. Do not trust a client-supplied `organization_id` without checking membership. Create pgTAP tests proving Tenant A cannot read or write Tenant B rows.

---

## 13. UI/UX Design System

### Visual direction

A calm, premium café operations interface: light surfaces, generous whitespace, restrained coffee-inspired accent colors, clear typography, and fast feedback. Avoid oversized gradients, excessive shadows, glassmorphism, decorative animation, and dense dashboard clutter.

Suggested token direction (adjust during design review):
- Background: warm off-white.
- Main surfaces: white / very light neutral.
- Primary text: dark charcoal.
- Primary accent: deep coffee brown or muted evergreen.
- Success: restrained green.
- Warning: amber.
- Danger: red.
- Borders: subtle neutral gray.

Use CSS variables/design tokens so tenant branding and future dark mode do not require component rewrites.

### Layout by device

**Desktop:** collapsible left navigation, compact top bar, wide tables, keyboard shortcuts. POS may run in distraction-free full-screen mode.

**Tablet:** optimized POS two-panel layout; category/product grid on one side and order summary on the other. Large touch targets and persistent checkout action.

**Mobile:** responsive stacked POS layout, thumb-friendly actions, bottom navigation for operational pages, compact order summary. Avoid squeezing desktop tables into a narrow viewport; use cards or horizontally scrollable data grids only when appropriate.

### POS interaction goals
- Add a common product to an order in one tap/click.
- Complete a straightforward cash sale in as few steps as practical.
- Keep category, search, cart, total, and checkout consistently visible.
- Use clear feedback for sold-out items, required modifiers, invalid discounts, and offline status.
- Confirm destructive or financial actions; do not add confirmation to every harmless tap.

### Performance rules
- Lazy-load non-POS routes and large charts.
- Keep POS critical JavaScript bundle small; analyze bundle output in CI.
- Use route-level code splitting and virtualize large lists.
- Resize/compress product images and lazy-load thumbnails.
- Avoid unnecessary polling and duplicate subscriptions.
- Query local SQLite with indexed filters and bounded result sets.
- Keep motion minimal and respect reduced-motion settings.
- Provide skeletons only where data is genuinely loading; local cached POS data should render immediately.

### Localization
Support configurable locale, timezone, currency, date/number format, and tax labels per tenant/branch. Design the UI for English initially and keep an i18n layer ready for Thai and Burmese. Use fonts with correct glyph coverage; test line height and wrapping for each supported script.

---

## 14. Screen Map

### Public / SaaS
- Marketing landing page (optional separate lightweight site)
- Pricing
- Sign in / reset password / invite acceptance
- Tenant onboarding wizard
- Subscription and billing portal

### Tenant application
- Dashboard
- POS Terminal
- Orders / Order detail / Refund dialog
- KDS / Station view
- Products / Categories / Variants / Modifiers
- Recipes / Recipe cost details
- Inventory overview / Ingredients / Stock ledger / Counts / Transfers / Wastage
- Purchasing / Suppliers / Purchase orders / Receiving
- Customers / Customer profile / Loyalty
- Staff / Roles / Attendance / Shift schedule
- Register / Shift open / Shift close / Cash movement
- Expenses
- Reports / Export center
- Branches / Devices
- Integrations
- Settings / Tax / Receipts / Printers / Business profile
- Sync health / Failed sync queue

### Platform console
- Tenant list/detail
- Plan and feature entitlements
- Subscription events
- Usage and service health
- Support/audit access log

---

## 15. Hardware and PWA Considerations

- Use a PWA manifest and service worker for installation and app-shell caching.
- Cache static assets only; avoid indiscriminately caching sensitive API responses.
- Test installation, storage persistence, device sleep, browser restart, and offline recovery on target browsers.
- Barcode scanners commonly operate as keyboard-wedge devices; support that input path first.
- Thermal printing must be validated per printer model and browser/OS. Browser printing may show a dialog; silent printing, cash-drawer kick, and reliable LAN printer control may require a local print bridge or supported hardware integration.
- Keep KDS and customer display as separate responsive routes/screens.
- Do not assume USB/Bluetooth browser APIs work consistently across desktop and mobile browsers.

---

## 16. Non-Functional Requirements

| Area | Requirement |
|---|---|
| POS startup | Render cached POS shell and core local data immediately after app unlock |
| Checkout | Local cash checkout must not wait for cloud round-trip |
| Sync | Retry transient failures; surface permanent rejections |
| Consistency | Idempotent commands and append-only payment/stock histories |
| Security | RLS and least privilege on all tenant tables |
| Availability | Graceful offline experience for explicitly supported workflows |
| Accessibility | Keyboard navigation, semantic controls, visible focus, adequate contrast |
| Responsiveness | Desktop, tablet landscape/portrait, mobile portrait |
| Observability | Client errors, sync backlog, upload rejects, Edge Function failures |
| Backup | Automated database backup and tested restore process |
| Audit | Record high-risk business and admin actions |
| Data portability | Tenant exports in documented formats |

Avoid promising a numeric uptime or response-time SLA until hosting, sync provider, and support plans are selected and measured.

---

## 17. Testing and Acceptance Criteria

### Core POS
- Cashier can open a shift and complete a cash sale while online.
- Cashier can complete an allowed cash sale while offline.
- Duplicate retries do not create duplicate orders or payments.
- Item price/modifier/tax snapshots remain unchanged after catalog edits.
- Partial/full refunds preserve the original order and payment history.

### Offline/sync
- App reloads with previously synced menu and branch configuration available offline.
- New offline orders are visible locally and queued.
- On reconnect, queued records synchronize once.
- Simulated disconnect during upload does not duplicate records.
- Rejected uploads appear in a manager sync-issue queue with actionable reason.
- Two offline devices with conflicting stock operations trigger the defined conflict path.
- Last-sync time and pending count are accurate.

### Tenant security
- A user in Tenant A cannot read, create, update, or delete Tenant B data.
- A branch-only user cannot access other branches unless permission allows.
- Storage object policies prevent cross-tenant access.
- Service-role/secret keys are absent from browser bundles.
- RLS automated tests run in CI.

### Inventory and costing
- Recipe usage creates the correct ingredient movements.
- Unit conversions produce correct quantities.
- Stock count creates an auditable adjustment.
- Product cost and margin calculations use the expected recipe version and currency precision.

### UX and compatibility
- POS workflows are tested on at least one representative desktop browser, tablet browser, and mobile browser.
- Touch targets are usable and critical actions remain visible.
- Long product lists remain responsive.
- Thermal printer and cash-drawer combinations are explicitly tested before certification for a device profile.

### Automated test stack
- Unit: Vitest.
- Component: Testing Library.
- Browser E2E: Playwright.
- Database and RLS: pgTAP / `supabase test db`.
- Sync tests: repeat, reorder, disconnect, reconnect, duplicate, and conflict scenarios.

---

## 18. Suggested Repository Structure

```text
brewflow-pos/
├── apps/
│   ├── web/                    # Vite React PWA
│   └── platform-admin/         # optional separate admin app
├── packages/
│   ├── ui/                     # shared design system
│   ├── domain/                 # business rules and state machines
│   ├── validation/             # Zod schemas
│   ├── database/               # generated types and query helpers
│   ├── powersync/              # local DB schema, connectors, sync hooks
│   ├── permissions/            # permission keys and checks
│   ├── reporting/              # report calculations and DTOs
│   └── config/                 # eslint, tsconfig, build config
├── supabase/
│   ├── migrations/
│   ├── functions/
│   │   ├── tenant-provision/
│   │   ├── sync-upload/
│   │   ├── payment-webhook/
│   │   ├── subscription-webhook/
│   │   └── export-report/
│   ├── tests/                  # pgTAP and RLS tests
│   └── seed.sql
├── docs/
│   ├── PRD.md
│   ├── ARCHITECTURE.md
│   ├── DATABASE.md
│   ├── SYNC_PROTOCOL.md
│   ├── SECURITY.md
│   ├── API.md
│   └── UI_SPEC.md
├── e2e/
├── scripts/
├── .env.example
├── package.json
└── README.md
```

Use a monorepo only if there is a real need to share packages between web app and admin app. Keep the dependency graph simple. Commit database migrations and generate TypeScript database types in CI or a controlled development workflow.

---

## 19. Delivery Plan

### Stage 0 — Technical proof of concept
- Validate PowerSync + Supabase Auth integration.
- Test local SQLite persistence in target desktop/tablet/mobile browsers.
- Test upload queue and custom upload handler.
- Test RLS + Sync Rules across two tenants and two branches.
- Test offline cash order, reconnect, duplicate retry, and conflict handling.
- Validate printer approach with actual target hardware.

**Exit gate:** Do not start full POS implementation until offline order persistence and tenant-scoped synchronization are proven.

### Stage 1 — SaaS foundation
- Organizations, branches, memberships, roles, devices.
- Supabase migrations, RLS, Storage rules, Auth.
- Tenant onboarding and platform admin baseline.
- PWA shell, navigation, design system, sync status.

### Stage 2 — Core POS
- Catalog, variants, modifiers, order, checkout, cash payment, receipt, shift.
- KDS and order status.
- Offline-first order creation and retry safety.

### Stage 3 — Inventory and purchasing
- Ingredients, recipes, stock movement ledger, costing.
- Suppliers, purchase orders, receiving, transfers, stock count, wastage.

### Stage 4 — Customer and workforce
- CRM, loyalty, promotions.
- Employee profiles, attendance, tips, role-based approvals.

### Stage 5 — Finance and analytics
- Expenses, reconciliation, reports, exports, branch comparisons.

### Stage 6 — Commercialization
- Plans, billing, feature entitlements, usage limits, support workflows.
- Monitoring, backup/restore drills, security review, load testing, documentation.

---

## 20. Risks and Mitigations

| Risk | Mitigation |
|---|---|
| Sync complexity | PowerSync proof of concept before feature development |
| Cross-tenant data exposure | RLS on every table, membership checks, pgTAP tests |
| Duplicate transactions | Idempotency keys and immutable transaction records |
| Offline stock oversell | Provisional stock, branch buffers, reconciliation workflow |
| Browser storage eviction | Persistent storage request, sync status, device policy, restore strategy |
| Payment misclassification offline | Digital payments remain pending until provider confirmation |
| Printing incompatibility | Certify tested printer/browser/device combinations and use bridge when needed |
| UI bundle bloat | Bundle budgets, route splitting, no unnecessary large UI libraries |
| Regulatory/tax differences | Tenant-specific jurisdiction config and local professional validation |
| SaaS cost growth | Meter usage and review Supabase/PowerSync pricing before launch |

---

## 21. AI Coding Agent Master Prompt

Use this prompt as the starting instruction for Codex or another repository-aware coding agent.

> You are the principal software architect and senior full-stack engineer responsible for building BREWFLOW POS, a commercial multi-tenant Coffee Shop SaaS platform.
>
> **Mandatory product decisions:** responsive React + TypeScript + Vite PWA; Supabase PostgreSQL/Auth/Storage/Edge Functions; PostgreSQL RLS; PowerSync with local SQLite for offline-first data; desktop/tablet/mobile; lightweight, fast, accessible UI. Do not substitute a different database or sync engine without explicitly documenting the trade-off and requesting approval.
>
> **Architecture rules:**
> 1. Every tenant-owned record has `organization_id`; branch-owned data also has `branch_id`.
> 2. RLS is the security boundary. Client-side filters are not security.
> 3. Never place service-role or secret keys in frontend code.
> 4. POS reads from the local database; do not block ordinary checkout on cloud round-trips.
> 5. All queued commands have UUIDs and idempotency keys.
> 6. Payment, refund, stock movement, cash movement, and audit histories are append-only or reversal-based.
> 7. Keep order status separate from payment status.
> 8. Cash may be recorded offline; online payment methods must not be marked paid without provider/server confirmation.
> 9. Do not use floating-point arithmetic for currency.
> 10. Do not silently discard failed sync records or apply last-write-wins to financial/stock records.
>
> **Engineering workflow:**
> - First inspect the repository and report its current state.
> - Read this specification and convert it into a task plan with dependencies.
> - Build Stage 0 proof of concept before broad feature development.
> - Implement one vertical slice at a time: migrations + RLS + domain logic + local sync + UI + tests.
> - Add or update tests for every business rule and permission.
> - Run type-check, lint, unit tests, database tests, and relevant E2E tests after each slice.
> - Keep modules small and use clear TypeScript types and Zod validation at boundaries.
> - Do not invent payment integrations, printer capabilities, or tax rules; isolate adapters and document what requires provider/country validation.
> - Update `docs/` and `.env.example` with every architectural change.
> - Never claim a feature is complete unless its tests pass and its acceptance criteria are verified.
>
> **Initial task:** create the monorepo/application skeleton, design tokens, Supabase migrations for tenancy/membership/branch/device, RLS policies and pgTAP tests, PowerSync local schema and tenant-scoped Sync Rules, sync status UI, and a minimal offline cash-order vertical slice. Do not build the entire platform in one unreviewable code generation step.

---

## 22. Reference Documentation

- Supabase — PowerSync integration overview: https://supabase.com/partners/catalog/powersync
- Supabase — Multi-tenant isolation guidance: https://supabase.com/kb/faqs/database/multi-tenant-isolation
- Supabase — Row Level Security: https://supabase.com/docs/guides/database/postgres/row-level-security
- Supabase — Securing your data: https://supabase.com/docs/guides/database/secure-data
- PowerSync — Documentation: https://docs.powersync.com/
- PowerSync — Supabase integration: https://docs.powersync.com/integration-guides/supabase
- Vite — Documentation: https://vite.dev/guide/
- React — Documentation: https://react.dev/learn

**Documentation note:** PowerSync is a third-party synchronization component. Confirm current product capabilities, web SDK support, Sync Rules, hosting model, pricing, and service limits during Stage 0. Supabase and PowerSync documentation may change after this specification is written.
