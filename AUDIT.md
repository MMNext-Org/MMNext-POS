# BREWFLOW POS — Project Audit Report

**Date:** 2026-11-27  
**Scope:** Correctness, security, performance, and maintainability across `src/`, `apps/supabase/`, build config, and dependencies.  
**Stack:** React 19, Vite 8, TypeScript 5.5, @supabase/supabase-js, @chakra-ui/react v3, framer-motion, oxlint, vitest (unused).

---

## Executive Summary

The codebase is structurally sound but carries several **high-severity security and correctness issues** that must be addressed before any production deployment:

1. **Service role key placeholders** exist in server-side Supabase config files with hardcoded fallbacks (`'your-service-role-key'`, `'public-anon-key'`). These are legal/infrastructure items flagged in SECURITY.md §7 as outside this task's scope, but they must not ship to production.
2. **Two inconsistent Supabase client initializations** — `src/utils/supabase.ts` (VITE env vars) and `src/context/AuthContext.tsx` (REACT_APP env vars with localhost fallbacks). Auth logic depends on the latter.
3. **RLS is enabled on only 4 of ~20 tenant-owned tables** (`customers`, `loyalty_accounts`, `loyalty_transactions`, `orders`). Payments, refunds, and other tables lack RLS policies.
4. **No `.env` file exists** and placeholder credentials are committed in `apps/supabase/*.js`.
5. **No Tailwind config exists**, so all `text-coffee-brown`, `bg-coffee-brown`, etc. utility classes are inert — pages render unstyled.
6. **No `@/` path aliases in `tsconfig.app.json`** (only in `vite.config.ts`), causing pre-existing TypeScript resolution errors.
7. **Stale duplicate `src/auth/App.tsx`** diverges from `src/App.tsx` (missing providers, wrong Navigation signature).
8. **Dummy API directories** (`api_old`, `api_temp`, `rename_api`) contain placeholder content and should be removed.

Performance work from the prior pass is already in place: `Loyalty.tsx` and `Privacy.tsx` use O(1) `Map` lookups and `useMemo`. No remaining expensive queries, blocking sync calls, or large-bundle imports were identified as "worst" issues.

---

## Findings

### Security — High

#### S1. Service role key placeholders committed to repo
**Files:** `apps/supabase/auth_config.js`, `apps/supabase/config.js`, `apps/supabase/storage.js`  
**Evidence:**
```js
// auth_config.js
process.env.SUPABASE_SERVICE_ROLE_KEY || 'your-service-role-key'
// config.js
serviceRoleKey: 'YOUR_SERVICE_ROLE_KEY'
// storage.js
process.env.SUPABASE_SERVICE_ROLE_KEY || 'your-service-role-key'
```
**Impact:** If deployed, these fallbacks could expose service-role credentials. Flagged in SECURITY.md §7 as out of scope for this task (legal/infrastructure), but must be remediated before production.  
**Recommendation:** Remove all hardcoded key fallbacks; require env vars at runtime; add `.env.example` (no real keys) and ensure `.env` is gitignored.

#### S2. No `.env` file; placeholder credentials present
**Impact:** Developers have no local env template; risk of committing real keys.  
**Recommendation:** Add `.env.example` with `VITE_SUPABASE_URL` and `VITE_SUPABASE_ANON_KEY` placeholders only.

#### S3. Inconsistent Supabase client initialization
**Files:** `src/utils/supabase.ts` vs `src/context/AuthContext.tsx`  
**Evidence:** `AuthContext.tsx` uses `process.env.REACT_APP_SUPABASE_URL` with fallback `'http://localhost:54321'` / `'public-anon-key'`; `src/utils/supabase.ts` uses `import.meta.env.VITE_*`. Auth flows depend on the inconsistent client.  
**Recommendation:** Consolidate to a single client (prefer `src/utils/supabase.ts` with VITE env vars) and remove the fallback defaults.

#### S4. RLS covers only 4 tables
**File:** `apps/supabase/migrations/010_authorization.sql`  
**Evidence:** `ALTER TABLE ... ENABLE ROW LEVEL SECURITY` + policies exist only for `customers`, `loyalty_accounts`, `loyalty_transactions`, `orders`. `payments`, `refunds`, `loyalty_reservations`, etc. lack RLS.  
**Recommendation:** Extend RLS policies to all tenant-owned tables, matching the `app.current_user_id` / `organization_id` pattern already established.

#### S5. Hardcoded org/branch IDs in OrderCreation
**File:** `src/pages/OrderCreation.tsx`  
**Evidence:** `organizationId: 'org-1'`, `branchId: 'branch-1'` with `// TODO: use real org` comments.  
**Impact:** Orders are created with fake tenant IDs, bypassing RLS tenant isolation.  
**Recommendation:** Derive `organizationId`/`branchId` from the authenticated user's app context (already available via `useAuth` / `setAppContext`).

---

### Performance — Medium (mostly already addressed)

#### P1. Unnecessary Supabase round-trip on order submit
**File:** `src/pages/OrderCreation.tsx`  
**Evidence:** Inside `handleSubmit`:
```ts
const organizationId = (await supabase.auth.getUser()).data.user?.user_metadata?.organization_id || '';
```
This makes an extra network call on every order submission even though `useAuth()` already provides `user`.  
**Recommendation:** Use `user?.user_metadata?.organization_id` from `useAuth()` instead. (Not blocking render — it's inside an async handler — but wasteful.)

#### P2. Tailwind utility classes are inert (no Tailwind config)
**Evidence:** No `tailwind.config.js`/`tailwind.config.ts` exists. Classes like `text-coffee-brown`, `bg-coffee-brown` produce no CSS.  
**Impact:** Pages render unstyled — a functional correctness/UX issue, not a runtime performance issue.  
**Recommendation:** Add Tailwind config or replace utility classes with a working styling approach. (Prior UI/UX work unified on Tailwind-style classes; config is the missing piece.)

#### P3. Duplicate App.tsx (src/auth/App.tsx)
**Evidence:** `src/auth/App.tsx` is a stale copy missing PowerSync/Order/Shift providers and calls `Navigation` without the `user` prop.  
**Recommendation:** Remove `src/auth/App.tsx`; keep `src/App.tsx` as the single app root.

#### P4. Dummy API directories
**Dirs:** `src/api_old`, `src/api_temp`, `src/rename_api` — each contains a single placeholder `# Dummy API route` file.  
**Recommendation:** Delete these directories; they add bundle noise and confusion.

---

### Correctness — Medium

#### C1. @/ path aliases missing from tsconfig.app.json
**Evidence:** `vite.config.ts` defines `@/context`, `@/utils`, etc.; `tsconfig.app.json` has no `paths` section.  
**Impact:** Pre-existing `tsc` resolution errors for `@/…` imports; IDE intellisense broken.  
**Recommendation:** Mirror the `resolve.alias` entries into `tsconfig.app.json` `compilerOptions.paths`.

#### C2. Orders.tsx default vs named export mismatch
**File:** `src/pages/OrderCreation.tsx` imports `{ Order } from '@/types'` — pre-existing export-style inconsistency flagged in prior tsc run.  
**Recommendation:** Align export style across `src/types/index.ts`.

#### C3. exportToPdf uses window.open + document.write
**File:** `src/utils/export.ts`  
**Impact:** Fragile under popup blockers; may fail silently.  
**Recommendation:** Replace with a Blob-based download (e.g., `jsPDF` or a hidden iframe) consistent with the CSV export path.

---

### Maintainability — Low

#### M1. No page-level tests
**Evidence:** vitest is installed but test files exist only for `syncPowerSync`, `ShiftContext`, `KitchenDisplay`, `ConnectionStatus`. No tests for `Orders`, `Loyalty`, `Privacy`, `Reports`, `Shifts`, `Dashboard`, `Home`.  
**Recommendation:** Add vitest + React Testing Library tests for core pages (Orders, Loyalty, Privacy) as features change.

#### M2. PowerSync is a browser no-op
**Evidence:** `@powersync/node` is Node-only; no browser-capable PowerSync SDK installed. Offline orders use `localStorage` directly.  
**Recommendation:** Either install `@powersync/web` (or equivalent) for real offline sync, or document `localStorage` as the intentional offline strategy.

---

## Items Already Addressed (prior work, committed 02dd0bf)

| Area | Item | Status |
|------|------|--------|
| Loyalty.tsx | `useMemo` for `searchResults`, `customerTransactions`, `accountsMap` | Done |
| Loyalty.tsx | Inline enrollment validation feedback | Done |
| Loyalty.tsx | aria labels on interactive elements | Done |
| Privacy.tsx | `customersMap` O(1) lookup replacing `.find()` per row | Done |
| Privacy.tsx | File download button added | Done |
| Profile.tsx | Fixed structural bug (unreachable code) | Done |
| App.tsx | Navigation shell, skip-to-content, LoadingSpinner, main landmark | Done |
| Pages | Consistent loading states (LoadingSpinner component) | Done |
| Pages | Consistent error states (ErrorDisplay component) | Done |
| Pages | Empty states for list views (EmptyState component) | Done |
| Orders.tsx | Show product names not IDs | Done |
| Shifts.tsx | $ prefix on cash displays | Done |
| Reports.tsx | Pagination controls added | Done |
| SignIn | Unified with Tailwind classes | Done |
| A11y | Skip-to-content link, aria labels on key elements (partial; Loyalty/Privacy/Orders/Dashboard/Home still need remaining work) | Partial |
| UI_AUDIT.md | Created documenting UI/UX findings | Done |

Remaining a11y work (Loyalty/Privacy/Orders/Dashboard/Home) and responsive breakpoints are tracked in the prior UI/UX task list, not this audit.

---

## Recommended Action Plan

### Before production (blocking)
1. Remove service role key fallbacks from `apps/supabase/*.js`; require env vars.
2. Add `.env.example`; ensure `.env` is gitignored.
3. Consolidate Supabase client init to `src/utils/supabase.ts`; remove `AuthContext.tsx` localhost fallbacks.
4. Derive real `organizationId`/`branchId` in `OrderCreation.tsx` from auth context; remove hardcoded `'org-1'`/`'branch-1'`.
5. Extend RLS policies to all tenant-owned tables (payments, refunds, etc.).

### Before next release (high)
6. Add Tailwind config (or working styling) so utility classes render.
7. Mirror `@/` aliases from `vite.config.ts` into `tsconfig.app.json`.
8. Remove `src/auth/App.tsx` (stale duplicate).
9. Delete `src/api_old`, `src/api_temp`, `src/rename_api`.
10. Replace `window.open`+`document.write` PDF export with Blob-based download.

### Ongoing (medium/low)
11. Add vitest + RTL tests for core pages (Orders, Loyalty, Privacy).
12. Complete remaining a11y items (Loyalty/Privacy/Orders/Dashboard/Home) and responsive breakpoints.
13. Resolve PowerSync browser strategy (install web SDK or document localStorage approach).

---

## Conclusion

The codebase has a solid domain model (RLS-ready schema, scoped permissions, idempotent loyalty RPCs, earn/redeem ledger) and the prior performance pass already optimized the two heaviest client-side pages (Loyalty, Privacy). The remaining gaps are primarily **security** (committed placeholder keys, incomplete RLS, inconsistent auth client) and **correctness** (missing Tailwind config, missing tsconfig aliases, stale duplicate App, hardcoded tenant IDs). None of these are performance regressions, but all are blocking for a production deployment.
