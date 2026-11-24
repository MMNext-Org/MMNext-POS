# BREWFLOW POS - Todo List
## Repository & Tooling
- [x] Set up monorepo (apps/web, packages/domain, docs) directory structure.
- [ ] Initialize Git repo, add .gitignore, LICENSE.
- [x] Add ESLint, Prettier, TypeScript config, Vitest, Testing Library.
- [x] Add UI design tokens (color, spacing, typography) using CSS variables.
- [x] Set up Vite + React + TypeScript project in `apps/web`.
## Supabase Backend & Auth
- [x] Create Supabase project and enable Auth.
- [x] Define Supabase database schema for tenancy (organizations, branches, devices, users, customers, products, categories, modifiers, product_variants, recipes, ingredients, units, stock_locations, stock_movements, orders, order_items, payments, refunds, customers, loyalty_accounts, suppliers, purchase_orders, purchase_order_items, goods_receipts, goods_receipt_items, supplier_invoices, supplier_payments, registrars, register_shifts, cash_movements, customers, loyalty_accounts, suppliers, suppliers_payments).
- [x] Implement Row Level Security (RLS) policies on all tenant‑owned tables.
- [x] Define role/permission model and server‑side checks.
- [x] Configure Supabase Auth with tenant/role handling and token validation.
- [x] Configure Storage buckets by organization and branch.
- [x] Write pgTAP / supabase test db tests for RLS enforcement across tenants and branches.
## Database & Database Schema
- [x] Define UUID primary keys and composite uniqueness constraints (org_id + branch_code, org_id + device_code).
- [x] Index foreign keys used by RLS and Sync Rules (org_id, branch_id, product_id, etc.).
- [x] Add CHECK constraints for non‑negative quantities, valid state transitions, revenue and cost handling.
## PowerSync / Local‑First Sync
- [x] Select PowerSync as the local‑first sync layer (confirm compatibility, pricing, limits).
- [x] Set up local SQLite schema matching Supabase schema (reuse names or map).
- [x] Configure PowerSync client in the PWA (device, branch, organization).
- [x] Define tenant‑scoped Sync Rules (catalog vs orders, sync frequency, conflict handling).
- [x] Implement sync queue handling (pending, uploading, synced, rejected) on client and server.
- [x] Write PowerSync tests covering offline create, retry, conflict, duplicate, and reconnect flows.
## Core POS (Stage 1)
- [x] Build POS UI screens: menu/category view, product selection, order creation, quantity, modifier selection.
- [x] Implement checkout flow: item totals, discounts, taxes, service charge, final amount calculation.
- [x] Integrate cash payment (record cash tender, calculate change, enforce cash‑only limits).
- [x] Implement card/QR payments with provider API callbacks (pending, settled).
- [x] Add receipt printing and KDS (kitchen display system) UI and order routing.
- [x] Define order states (draft, placed, accepted, preparing, ready, served, completed, cancelled).
- [x] Define payment status and flow.
- [x] Provide UI for order status history, order details page, refund dialog.
- [x] Integrate inventory tracking: stock decrement, stock movement recording, local provisional stock.
- [x] Implement offline order creation and sync when back online (queue, retry, conflict handling).
- [x] Add UI for cash drawer handling (cash in/out, float management, shift close).
## Inventory & Costing
- [x] Implement ingredient catalog, units, and unit conversion logic.
- [x] Build recipes/recipe_items tables and link to products (variant‑specific).
- [x] Implement stock locations and multi‑branch stock tracking.
- [x] Implement stock movement ledger (append‑only) and stock count adjustments.
- [x] Define low‑stock alerts, reorder points, expiry/batch handling.
- [x] Integrate inventory tracking with recipes (theoretical consumption) and manual adjustments.
- [x] Link recipe costing to product cost calculations (product cost, gross profit).
## Inventory Purchasing
- [x] Implement supplier directory, contact info, contract info.
- [x] Implement purchase order workflow and goods receipt handling.
- [x] Record goods receipt in inventory and reconcile with stock counts.
- [x] Implement return handling, discrepancy handling.
15. **Customer & Loyalty**
   - [x] Implement customer profile with contact info, preferences, birthday, consent.
   - [x] Implement loyalty points system, tiered rewards, coupon codes.
   - [x] Implement loyalty enrollment/disenrollment and redemption workflow.
   - [x] Provide export and deletion workflow per privacy law.
15. **Staff & Shift Management**
   - [x] Implement employee profiles, role assignments, PIN / device login.
   - [x] Design shift scheduling UI (optional initial release).
   - [x] Manage register open/close, cash float, cash in/out, close, counted cash, variance.
   - [ ] Implement shift handover, manager approval for close, tips allocation.
15. **Financial & Reporting**
   - [x] Implement daily cash reconciliation, sales summary, tax reports.
   - [x] Generate profit report with revenue, cost, tax, service charge.
   - [ ] Enable reports export (CSV/PDF) with tenant‑role restrictions.
   - [x] Provide analytics dashboard for owners.
15. **SaaS & Commercialization**
    - [x] Implement SaaS console: tenant directory, billing & subscription management.
    - [x] Provide SaaS usage reporting (active branches, orders, storage).
    - [x] Implement tenant suspension/reactivation workflows and audit logs.
    - [x] Provide platform admin functions: SaaS tenant provisioning, role management, pricing plans, service health monitoring.
15. **Offline-First Considerations**
    - [x] Ensure offline cash payment is recorded locally and stays pending until provider confirmation.
    - [x] Ensure offline order entries persist locally and queue for sync when back online.
    - [x] Implement sync retry logic and conflict resolution for orders, inventory, cash movements.
    - [x] Ensure that offline device with conflicting stock operations triggers defined conflict path.
    - [x] Ensure last-sync time and pending count are displayed.
16. **Security & Compliance**
    - [x] Implement RLS policies for every tenant-owned table.
    - [ ] Conduct security audit for data leakage, secret handling, cross‑tenant access.
    - [ ] Verify that no service-role/secret keys are bundled in the browser.
    - [x] Implement audit log for refunds, voids, discounts, stock adjustments, cash movements.
    - [x] Verify that RLS policies are enforced at every write and read path.
16. **Testing & QA**
    - [x] Write unit tests (Vitest) for domain logic.
    - [x] Write component tests (Testing Library) for UI.
    - [x] Write end‑to‑end tests (Playwright) covering POS flow, offline mode, sync, checkout, KDS.
    - [x] Write database/RLS tests (pgTAP) for each table, constraint, policy.
    - [x] Write sync-specific tests: offline -> online, disconnect, duplicate, retry, conflict.
    - [ ] Set up CI pipeline that runs lint, type‑check, unit, component, E2E, database tests.
17. **Documentation & Deliverables**
    - [x] Write PRD (product requirements doc) and this specification.
    - [x] Document architecture, database schema, sync protocol, RLS policies, security model, and deployment steps.
    - [x] Write API docs for each function/endpoint (tenant‑provision, sync‑upload, checkout, refund, etc.).
    - [x] Write developer onboarding guide (setup, dev environment, testing, CI/CD).
    - [ ] Create user documentation: admin guide, POS user guide, staff handbook, support.
## Risk Mitigation & Validation
- [x] Run PowerSync proof of concept (Stage 0) before broad feature development.
- [ ] Validate RLS + Sync Rules across two tenants and two branches.
- [x] Ensure offline cash payment remains pending until provider confirmation.
- [x] Verify offline order persistence, device re‑connect, and conflict resolution.
- [ ] Test printer and cash‑drawer hardware combinations with target devices.
- [ ] Perform load testing and performance profiling on POS UI and sync services.
- [ ] Conduct compliance review for privacy regulations (Thai, Burmese, GDPR‑like data handling).
---
*All tasks should be tracked in the repository issue tracker and linked to appropriate stages (e.g., Stage 0, Stage 1, Stage 2, etc.) as work progresses.*
