# BREWFLOW POS — Security Model: Loyalty & Privacy Workflows

This document describes the security controls for loyalty enrollment/disenrollment/redemption and customer privacy export/erasure. It covers authorization, tenant isolation, secret handling, and operational boundaries.

---

## 1. Authorization Model

### Authentication ≠ Authorization

Authentication (Supabase Auth) establishes identity. Authorization (RLS + RPC checks) establishes what that identity may do. Every loyalty/privacy RPC receives `p_user_id` and `p_org_id` and enforces organization membership server-side.

### Scoped Permissions

| Operation | Required Context | Enforced By |
|-----------|-----------------|-------------|
| Enroll customer | Authenticated user in org | `rpc_loyalty_enroll` + RLS |
| Disenroll customer | Authenticated user in org | `rpc_loyalty_disenroll` + RLS |
| Earn points | Authenticated user in org | RLS on `loyalty_transactions`/`loyalty_accounts` |
| Redeem points | Authenticated user in org | `rpc_loyalty_redeem_reserve/commit` + RLS |
| Export customer data | Authenticated user in org | Client-side org scoping + org-scoped queries |
| Record consent | Authenticated user in org | `rpc_record_consent` + RLS |
| Set retention policy | Authenticated user in org | `rpc_set_retention_policy` + RLS |
| Apply retention | Authenticated user in org | `rpc_apply_retention` + RLS |

### Role-Based Access

| Role | Loyalty Export | Loyalty Erasure | Loyalty Redeem at Checkout |
|------|---------------|----------------|----------------------------|
| Cashier | Yes (own org) | Yes (own org) | Yes |
| Shift Lead | Yes (own org) | Yes (own org) | Yes |
| Branch Manager | Yes (own org) | Yes (own org) | Yes |

Cross-tenant reads/writes are denied. Revoked or suspended memberships lose access immediately because RLS resolves organization via `current_setting('app.current_user_id', true)`.

---

## 2. Tenant Isolation

- Every tenant-owned table includes `organization_id`.
- RLS policies reference session locals set by `set_app_context(app.current_user_id, ...)`.
- The frontend never queries across organizations; all Supabase queries include `.eq('organization_id', organizationId)`.
- Loyalty accounts, transactions, and customers are strictly scoped to the calling user's organization.
- Export and erasure operate only within the caller's organization — no cross-tenant data is ever included.

---

## 3. Secret Handling

### No Secrets in the Browser

- The frontend uses only the Supabase **anon/public key**.
- **Service-role or secret keys are never bundled in client code.**
- All privileged operations (enroll, disenroll, redeem, consent recording, retention) run via Supabase RPCs evaluated server-side with RLS.
- Environment variables containing secrets (`SUPABASE_SERVICE_ROLE_KEY`, etc.) must be set only in server/Edge Function environments, never in `.env` files committed to the client.

### `.env` Placeholder

The repository `.env` contains placeholder Supabase credentials. The frontend is not live-testable against a real Supabase project without valid credentials; type-checking (`tsc`) and pgTAP database tests are the primary verification.

---

## 4. Data Protection

### Export

- Versioned JSON (`version: "1.0"`) generated client-side from org-scoped query results.
- Download filename includes version and date: `brewflow-privacy-export-v1.0-<YYYY-MM-DD>.json`.
- No credentials, secret configuration, internal authorization data, or other-tenant/customer records are included.
- Exports are not stored server-side in the current implementation; the JSON is held in browser memory and downloaded via a blob URL.

### Erasure / Disenrollment

- Disenrollment writes a forfeiture ledger entry reducing available points to zero.
- Historical ledger rows are preserved (append-only ledger); account status becomes `disenrolled`.
- Personal identifiers are not hard-deleted from financial/audit records; they remain as anonymized references where legally required.
- Active redemption reservations block disenrollment until resolved.

### Consent History

- Consent decisions are recorded with purpose, consent type, version, timestamp, and source.
- Consent history is immutable/append-only; withdrawals create a new record rather than editing the original.

---

## 5. Idempotency & Concurrency

- All mutation RPCs accept an idempotency key; duplicate submissions with the same key return the existing result without side effects.
- Account rows are locked during balance mutations (`SELECT ... FOR UPDATE` equivalent via RPC) to prevent concurrent double-spend.
- Two-phase redemption (reserve → commit) ensures points cannot be double-spent between reservation and commit.

---

## 6. Operational Boundaries & Limitations

| Item | Status |
|------|--------|
| Legal-compliance certification | Not claimed; jurisdictional review required |
| Backup erasure / third-party processor deletion | Documented as follow-up obligation, not implemented |
| Identity verification for privacy requests | Not implemented; requires legal review |
| Response deadlines for privacy requests | Not implemented; requires legal review |
| CSV/PDF export | Out of scope; JSON only |
| Offline loyalty redemption | Out of scope; redemption requires online server confirmation |

---

## 7. Checklist for Production Enable

- [ ] Replace `.env` placeholder credentials with real Supabase anon key (client) and service-role key (server only).
- [ ] Obtain jurisdiction-specific legal review for retention periods, erasure behavior, and consent requirements.
- [ ] Configure backup erasure and third-party processor deletion procedures.
- [ ] Verify RLS policies across two tenants and two branches with pgTAP.
- [ ] Confirm no service-role keys are bundled in browser bundles (build audit).
- [ ] Review the field-level retention/anonymization matrix with legal counsel.
