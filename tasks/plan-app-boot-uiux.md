# Implementation Plan: App Boot Fix, Build Health, and UI/UX Audit

## Overview

The loyalty enrollment/disenrollment and redemption work is implemented and committed (commit `6f0311a`), but the web app is not usable yet: it renders a **white screen**, and `npm run build` fails with **287 TypeScript errors**. This plan first makes the app boot, then makes the build green, then audits and fixes the UI/UX. It supersedes the (now complete) loyalty plan in `tasks/plan.md`.

Verified root causes (evidence from dev server module-graph crawl + `npx tsc -b`):

1. **White screen**: `src/App.tsx` calls `useColorMode()` in the `App()` body, but `ChakraProvider`/`ColorModeProvider` are only rendered *inside* App's JSX — the hook runs outside the provider and throws on mount. (`main.tsx` wraps App only in `AuthProvider`.)
2. **Broken PowerSync init**: `src/utils/powersyncClient.ts` imports non-existent `better-sqlite3` (Node-only, not installed) and uses placeholder code; requesting the module on the dev server returns **500** ("Failed to resolve import better-sqlite3"). Earlier this produced the `createClient` export error in the browser.
3. **Build failures**: `tsconfig.app.json` has no `paths` mapping for `@/*` (Vite has it, tsc does not) → 76 × TS2307; test files compiled without vitest globals → TS2593/TS2594; ~13 unused duplicate/scratch files are compiled (10 × `OrderContext_*.tsx`, `test_import_context.tsx`, `test-types.ts`, `src/auth/App.tsx`) adding ~60 errors; 83 × TS6133 unused-variable errors.

## Architecture Decisions

- Keep the provider composition in `main.tsx` (single source of provider nesting); remove duplicated providers from `App.tsx`.
- `powersyncClient.ts` must be **browser-safe**: no Node-only imports at module top level; PowerSync web SDK (`powersync`, already installed) loaded lazily inside `initializePowerSync()` with try/catch so init failures never break app boot. Existing exports (`initializePowerSync`, `getLocalDatabase`, `getLocalDatabasePath`) stay stable for callers.
- Tests are excluded from the app type-check build (vitest handles them); the app build covers `src` only.
- No behavior/feature changes in Phases 0–1 — this is repair work. Feature work stays out of scope.

## Task List

### Phase 0: Boot the App

#### Task 1: Fix provider composition
**Description:** Move `ChakraProvider`/`ColorModeProvider` above `App` so `useColorMode()` is called inside a provider; drop the duplicated `AuthProvider` nesting in `App.tsx`.

**Acceptance criteria:**
- [ ] `main.tsx` composes: StrictMode > ColorModeProvider > ChakraProvider > AuthProvider > App
- [ ] `App.tsx` no longer renders AuthProvider/ChakraProvider itself; `useColorMode()` works
- [ ] Visiting `/` renders the Home page, not a blank document

**Verification:**
- [ ] Crawl of dev-server module graph returns no failing modules from `/src/main.tsx`
- [ ] Page render check (browser console has no "must be used inside" provider error) — browser tool if approved, else module-graph + code review

**Dependencies:** None
**Files touched:** `src/main.tsx`, `src/App.tsx`
**Estimated scope:** S (2 files)

#### Task 2: Rewrite `powersyncClient.ts` (browser-safe)
**Description:** Remove the `better-sqlite3` import and placeholder implementation; initialize PowerSync via the installed `powersync` web SDK lazily, with graceful failure handling and the same export surface.

**Acceptance criteria:**
- [ ] No Node-only/non-existent imports at module top level
- [ ] `GET /src/utils/powersyncClient.ts` on the dev server returns 200 (no 500)
- [ ] `initializePowerSync()` resolves without throwing when the PowerSync service is unavailable (dev has no backend)

**Verification:**
- [ ] Dev-server module check returns 200 for the module
- [ ] No unhandled error in console from `App`'s init effect

**Dependencies:** None (independent of Task 1)
**Files touched:** `src/utils/powersyncClient.ts`, delete `src/utils/powersyncClient.js` (scratch copy)
**Estimated scope:** S (1–2 files)

#### Checkpoint: Boot
- [ ] App renders on `/` and primary routes; no provider/import errors

### Phase 1: Green Build

#### Task 3: tsconfig fixes
**Description:** Add `baseUrl`/`paths` for `@/*` → `src/*`, add vitest globals types, exclude tests and scratch files from the app build.

**Acceptance criteria:**
- [ ] All `@/...` imports type-check (no TS2307 from path aliases)
- [ ] Test files (`*.test.ts(x)`) are excluded from `tsconfig.app.json` compilation
- [ ] `npx tsc -b` error count strictly drops versus the 287 baseline

**Verification:**
- [ ] `npx tsc -b` runs; before/after error counts recorded
**Dependencies:** None
**Files touched:** `tsconfig.app.json` (possibly `tsconfig.json`)
**Estimated scope:** XS (1 file)

#### Task 4: Remove unused duplicate/scratch files
**Description:** Delete the 10 `OrderContext_*` copies (verified: zero importers), `src/test_import_context.tsx`, `src/test-types.ts`, `src/auth/App.tsx` (verify importer-free first), and repo-root scratch files (`test.ts`, `test2.js`, `test_simple.js`, `test_vite.js`, `fix_dependency_issue.js`, `crawl_dev_modules.cjs`); add `.gitignore` entries for `local.db*`.

**Acceptance criteria:**
- [ ] Each deleted file has no importers (grep verified before deletion)
- [ ] `npx tsc -b` error count drops by ~60+ versus baseline
- [ ] Dev server still serves `/` with no missing-module errors

**Verification:**
- [ ] grep for each deleted filename returns no matches
- [ ] `npx tsc -b` error count after < before
**Dependencies:** None (independent of Task 3; do after Task 3's tsconfig changes for accurate counting)
**Files touched:** deletions + `.gitignore`
**Estimated scope:** S (many deletions, no logic)

#### Task 5: Fix remaining real type errors
**Description:** Repair genuine type errors in live source files, working bottom-up: contexts (`AuthContext`, `OrderContext`, `InventoryContext`, `ProductsContext`, `TenantContext`, `recipeService`, `LoyaltyContext`) → pages (`Inventory`, `Orders`, `Products`, `Reports`, `Loyalty`, `Shifts`, `KitchenDisplay`, `OrderCreation`, `SignUp`, `Privacy`, `Dashboard`) → utils/services (`decimal`, `offlineSync`, `inventoryService`, `purchaseOrderService`, `powersyncClient` leftovers).

**Acceptance criteria:**
- [ ] `npx tsc -b` exits 0
- [ ] No `@ts-ignore`/`any`-blanket suppressions added (fix types properly; `unknown` + narrowing where needed)

**Verification:**
- [ ] `npx tsc -b` exit code 0
**Dependencies:** Task 3, Task 4
**Files touched:** ~25 source files
**Estimated scope:** L — break into per-directory sub-steps, checkpoint after each group

#### Checkpoint: Build
- [ ] `npx tsc -b` exits 0
- [ ] `npm run build` succeeds

### Phase 2: UI/UX Audit

#### Task 6: Audit routes/pages against the design system
**Description:** Inventory all routes (/, /dashboard, /orders, /shifts, /loyalty, /privacy, /reports, /signin, /signup, /tenant-test + KDS) and compare against design tokens in `src/index.css`/Chakra theme; produce an issue list (layout, spacing, consistency, loading/error/empty states, forms, feedback, basic a11y).

**Acceptance criteria:**
- [ ] Issue list produced with severity per route
- [ ] Each issue mapped to a file

**Verification:**
- [ ] Issue list reviewed and stored in `tasks/ui-audit.md`
**Dependencies:** Checkpoint: Build (build must be green to rely on pages)
**Files touched:** read-only audit + `tasks/ui-audit.md`
**Estimated scope:** M (audit artifact)

#### Task 7: Fix top UI/UX issues
**Description:** Fix high-severity findings from Task 6 first (broken layout, missing loading/error/empty states, inconsistent components), then medium ones as time allows.

**Acceptance criteria:**
- [ ] High-severity issues fixed
- [ ] Changes reuse existing design tokens/components; no ad-hoc inline styles introduced

**Verification:**
- [ ] Affected routes render without console errors
- [ ] Before/after screenshots (browser tool if approved) or DOM-level verification
**Dependencies:** Task 6
**Files touched:** depends on findings (pages/components)
**Estimated scope:** L — fix per route, checkpoint every 2–3 routes

#### Task 8: Route smoke test
**Description:** Verify every route renders cleanly on the dev server: no module failures in the graph, no console errors, expected landing UI per route.

**Acceptance criteria:**
- [ ] All routes return 200 for their module graph; no failing modules
- [ ] No uncaught errors on primary flows (home → orders → loyalty → privacy)

**Verification:**
- [ ] Automated module-graph crawl from `/src/main.tsx` passes (0 failures)
**Dependencies:** Task 7
**Files touched:** none (verification only)
**Estimated scope:** XS

#### Checkpoint: Complete
- [ ] All routes render; build green; no console errors on primary flows
- [ ] Audit findings triaged (fixed or documented as follow-ups)

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| 287 type errors hide design-level mismatches needing judgement | High | Fix file-by-file with per-group checkpoints; ask user when a type change implies behavior change |
| PowerSync web SDK (`powersync` v0.4) vs `@powersync/node` API mismatch | Medium | Lazy, failure-tolerant init; keep export surface stable; pin behavior with a smoke check |
| Deleting "duplicate" files that were actually referenced dynamically | Medium | grep every filename + dev-server module-graph check before/after deletion |
| UI fixes without a real browser (browser approval denied) | Medium | Use dev-server module crawl + DOM/console checks via approved browser tool when available |
| Loyalty/privacy code touched incidentally while fixing types | Low | Keep changes type-only; re-run loyalty tests (`tests/loyalty_redemption_test.sql` unchanged) |

## Open Questions

- Browser approval for the UI verification steps (currently denied; fallback is module-graph + code verification)
- Should PowerSync auto-init in dev without a backend, or stay behind an env flag?
- Whether the deleted duplicate `OrderContext_*` files should be archived anywhere first (currently: no importers, git history preserves them)