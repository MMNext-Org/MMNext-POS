# Product Requirements Document (PRD) - SaaS POS System

## 1. Overview
A multi-tenant, offline-first Point of Sale (POS) system with a SaaS administration console. Enables retailers to manage sales, inventory, customer loyalty, staff shifts, and financial reporting across multiple branches and organizations.

## 2. Goals
- Enable seamless offline operation with automatic sync and conflict resolution.
- Provide a unified SaaS console for tenant, subscription, and billing management.
- Support multi-branch inventory tracking with stock locations and movements.
- Deliver comprehensive reporting and analytics for financial and operational insights.
- Ensure robust security via Row Level Security (RLS) and role-based access control.

## 3. Scope
### In Scope
- **Core POS**: Checkout, payment processing (cash, card, offline), cash drawer handling, shift management.
- **Inventory**: Stock tracking, locations, movements, low-stock alerts, reorder points, batch/expiry handling.
- **SaaS Console**: Tenant directory, subscription billing, usage reporting, admin controls.
- **Integration**: Sync with existing ERP/Supabase backend via PowerSync.
- **Security**: RLS policies, JWT-based authentication, tenant isolation.

### Out of Scope (Future Phases)
- Mobile app development (beyond web PWA).
- Advanced analytics dashboards (beyond basic reports).
- Multi-currency support (initial launch supports single currency).

## 4. Functional Requirements

### 4.1 POS Frontend
- **Checkout**: Add items, apply discounts, process cash payments, record pending offline payments.
- **Cash Drawer**: Record cash in/out, manage float, close shift with counted cash and variance.
- **Staff Management**: Employee profiles, PIN/device login, shift scheduling, handover, approvals.
- **Inventory**: Real-time stock levels, stock movements (receipts, adjustments), multi-location tracking, low-stock alerts.
- **Offline Sync**: Local create, retry logic, conflict detection (last-write-wins), status indicators.

### 4.2 SaaS Administration Console
- **Tenant Management**: Create/activate/ suspend tenants, view audit logs, manage organization settings.
- **Subscription Management**: Plan selection, billing cycle, proration, renewal, cancellation.
- **Reporting**: Daily cash reconciliation, sales summary, tax reports, profit & loss, inventory levels.
- **User Management**: Role assignments, permissions, staff onboarding/offboarding.

### 4.3 Sync Protocol
- **Streams**: Tenant-scoped streams for orders, inventory, stock movements, catalog updates.
- **Conflict Policy**: Last-write-wins for most resources; merge policy for catalog updates.
- **Retry Logic**: Exponential backoff for failed sync attempts; manual retry option in UI.
- **Conflict Resolution**: UI to review and resolve duplicate or conflicting records.

### 4.4 Security & Compliance
- **RLS**: Enforce tenant isolation on all mutable tables (orders, payments, inventory, etc.).
- **Authentication**: JWT with `organization_id`, `role`, `tenant` claims; 7-day access token, 30-day refresh.
- **Audit Logging**: Record all critical actions (inventory adjustments, cash movements, user changes) with user, timestamp, and before/after data.
- **Data Privacy**: Support GDPR/CCPA data export and deletion workflows.

## 5. Non-Functional Requirements
- **Performance**: UI responses < 2s under normal load; sync operations < 10s.
- **Scalability**: Support up to 1000 concurrent users and 10,000+ daily transactions.
- **Reliability**: 99.9% uptime for SaaS console; offline mode continues functioning during network outages.
- **Maintainability**: Modular codebase, comprehensive test coverage (unit, integration, E2E).

## 6. Technical Requirements
### 6.1 Architecture
- **Frontend**: React + Vite PWA, TypeScript, React Router, component library (e.g., Tailwind UI).
- **Backend**: Supabase (PostgreSQL) with RLS, PowerSync for offline sync, Node.js/Express for custom API endpoints.
- **Database**: PostgreSQL with UUID primary keys, CHECK constraints for statuses, `is_offline`/`sync_status` columns.
- **Sync**: PowerSync with MongoDB storage, tenant-scoped queries using `organization_id` and `branch_code`.

### 6.2 Database Schema (Key Tables)
- `organizations`, `branches`, `tenants` – Tenancy hierarchy.
- `products`, `categories`, `units` – Catalog management.
- `stock_locations`, `stock_movements` – Inventory tracking with offline support.
- `orders`, `order_items`, `payments`, `refunds` – Sales and financial transactions with offline flags.
- `users`, `roles`, `permissions` – Authentication and authorization.
- `audit_logs` – Immutable log of critical changes.

### 6.3 Security Model
- **Authentication**: Supabase Auth with custom JWT claims containing `organization_id`, `role`, `tenant`.
- **Authorization**: RLS policies combined with server-side checks; role hierarchy (admin > owner > staff > cashier).
- **Data Protection**: Encrypted fields (if required), at-rest encryption for sensitive data.

### 6.4 Deployment
- **SaaS Console**: Hosted on Vercel/Netlify (static build) + Supabase backend.
- **CI/CD**: GitHub Actions for lint, test, build, and deploy pipelines.
- **Monitoring**: Log aggregation (e.g., Sentry), health checks, alerting.

## 7. Assumptions & Constraints
- **Network**: Intermittent connectivity expected; system must function offline.
- **Concurrency**: Multiple devices per branch; conflict resolution must be deterministic.
- **Regulatory**: Compliance with local tax reporting and financial data handling laws.
- **Data Model**: UUID generation via `uuid_generate_v4()` (PostgreSQL) or equivalent in SQLite.

## 8. Open Issues & Risks
- **Conflict Resolution UI**: Design and implementation of conflict resolution screens.
- **RLS Performance**: Ensuring RLS policies do not degrade write/read performance at scale.
- **Testing Coverage**: Comprehensive end-to-end testing for offline scenarios still in progress.

## 9. Deliverables
1. PRD (this document) – Completed.
2. Architecture Specification – Draft to be created.
3. Database Schema & Migration Scripts – Complete.
4. API Documentation – In progress.
5. Developer Onboarding Guide – To be written.
6. User Documentation – To be written.

## 10. Appendices
- **Appendix A**: Glossary of terms.
- **Appendix B**: Glossary of acronyms (e.g., POS, SaaS, RLS, PowerSync).
- **Appendix C**: Wireframes/Mockups (to be added later).

*Signed-off by:*  
[Leo Smitt / MMNext Team]  
Date: 2026-11-24