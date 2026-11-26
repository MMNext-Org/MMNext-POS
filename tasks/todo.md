# Task List: App Boot Fix, Build Health, UI/UX Audit

## Phase 0: Boot the App
- [x] Task 1: Fix provider composition (useColorMode outside ChakraProvider) — main.tsx + App.tsx
- [x] Task 2: Rewrite src/utils/powersyncClient.ts browser-safe (remove better-sqlite3 import)

### Checkpoint: Boot
- [x] App renders (no white screen), no provider/import errors on /

## Phase 1: Green Build
- [x] Task 3: tsconfig.app.json — add @/* paths, vitest globals, exclude tests/junk
- [x] Task 4: Delete unused duplicate/scratch source files (10× OrderContext_*, test_*, src/auth/App.tsx)
- [x] Task 5: Fix remaining real type errors (contexts → pages → utils/services)

### Checkpoint: Build
- [x] `npx tsc -b` exits 0
- [x] `npm run build` succeeds

## Phase 2: UI/UX Audit
- [x] Task 6: Audit all routes/pages vs design tokens; produce issue list (tasks/ui-audit.md)
- [x] Task 7: Fix top UI/UX issues (layout, loading/error/empty states, consistency, a11y basics)
- [x] Task 8: Verify every route renders cleanly (no console errors)

### Checkpoint: Complete
- [ ] All routes render; build green; no console errors on primary flows