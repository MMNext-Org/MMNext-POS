# UI/UX Audit — All Routes

## Overview
Audited every routed page and major component against design-system tokens, layout consistency, loading/error/empty states, forms, feedback, and basic a11y. Build is green (`npm run build` exits 0). Issues are ranked Critical → High → Medium → Low.

## Cross-Cutting Issues (affect all routes)

| # | Severity | Issue | Affected | Files |
|---|----------|-------|----------|-------|
| X1 | Critical | **No global navigation shell** — `Navigation` component exists but is never rendered in `App.tsx`. Users can't move between pages except via links inside individual pages. | All routes | `src/App.tsx`, `src/components/Navigation.tsx` |
| X2 | High | **Inconsistent styling approaches** — mix of Tailwind utility classes (`src/pages/*`), inline styles (`OrderCreationWithSummary`, `OrderNew`, `ConnectionTest`), and Chakra UI (`SignUp`). No unified design token usage. | All pages | `src/pages/*`, `src/components/*` |
| X3 | High | **No consistent loading states** — Home/Dashboard/Profile show `<div className="loading">Loading...</div>`; Shifts/Loyalty/Orders/Reports have no loading skeleton; Inventory shows inline "Loading products..." text. | Home, Dashboard, Profile, Shifts, Loyalty, Orders, Reports, Inventory | `src/pages/*` |
| X4 | High | **No consistent error states** — Error display varies per page: raw text (`{error && <p>...}`), red text (`text-red-500`), no error boundary, no retry mechanism. | Orders, Loyalty, Inventory, Reports, SignIn, SignUp | `src/pages/*` |
| X5 | High | **No empty states on list views** — Pages that can show empty lists lack consistent empty-state components. | Orders (order items), Reports (data preview), Loyalty (customers table) | `src/pages/*` |
| X6 | Medium | **No toast/notification system** — Success/error feedback uses inline text or `alert()` browser dialogs. | Loyalty, Privacy, Orders, Shifts | `src/pages/*` |
| X7 | Medium | **No confirmation dialogs** — Destructive actions (disenroll, delete customer) use `window.confirm()` instead of custom modals. | Loyalty, Privacy | `src/pages/Loyalty.tsx`, `src/pages/Privacy.tsx` |
| X8 | Medium | **No accessibility (a11y)** — Missing `aria-label`, `aria-describedby`, `role`, focus management, skip links, and keyboard navigation patterns across all pages. | All routes | `src/pages/*`, `src/components/*` |
| X9 | Medium | **No responsive design** — Pages use fixed containers (`max-w-4xl`, `p-6`) with no mobile breakpoints or adaptive layouts. | All routes | `src/pages/*` |
| X10 | Low | **No skip-to-content link** — Keyboard users must tab through entire nav/header on every page. | All routes | `src/App.tsx`, `src/pages/*` |

---

## Per-Route Issues

### Home (`/`)
| # | Severity | Issue | Files |
|---|----------|-------|-------|
| H1 | Medium | **Minimal landing page** — No hero section, CTA, or visual hierarchy. Just plain text links. | `src/pages/Home.tsx` |
| H2 | Low | **No page title/SEO meta** — No `<title>` or meta description. | `src/pages/Home.tsx` |

### Dashboard (`/dashboard`)
| # | Severity | Issue | Files |
|---|----------|-------|-------|
| D1 | High | **No real dashboard data** — Just a welcome message and static links. `SyncQueue` is rendered but no summary widgets (total sales, active shifts, low stock alerts). | `src/pages/Dashboard.tsx` |
| D2 | Medium | **No unauthenticated redirect** — Shows "Please sign in" text instead of redirecting to `/signin`. | `src/pages/Dashboard.tsx` |
| D3 | Medium | **No loading skeleton** — Shows raw `Loading...` div while auth loads. | `src/pages/Dashboard.tsx` |

### Orders (`/orders`)
| # | Severity | Issue | Files |
|---|----------|-------|-------|
| O1 | High | **Order items show `productId` instead of product name** — UI displays raw IDs, not human-readable names. | `src/pages/Orders.tsx` |
| O2 | High | **Order items reset on navigation** — `orderItems` is local state only; no persistence or sync with OrderContext. | `src/pages/Orders.tsx` |
| O3 | Medium | **No validation on quantity input** — No max bound, no step indicators. | `src/pages/Orders.tsx` |
| O4 | Medium | **No order history/list view** — Only shows current session items, no past orders. | `src/pages/Orders.tsx` |
| O5 | Low | **Product select shows "Loading products..." option** — Should use a proper loading state instead of a disabled option. | `src/pages/Orders.tsx` |

### Profile (`/profile`)
| # | Severity | Issue | Files |
|---|----------|-------|-------|
| P1 | Critical | **Structural bug** — Extra `;` after `if (!user) { return ...; };` makes the following `return` block unreachable code. Authenticated users never see their profile. | `src/pages/Profile.tsx` |
| P2 | High | **No user profile data** — Only shows name, no email, role, avatar, or editable fields. | `src/pages/Profile.tsx` |
| P3 | Medium | **No logout confirmation** — Logout button fires immediately without confirmation. | `src/pages/Profile.tsx` |
| P4 | Medium | **No loading state styling** — Raw `Loading...` div, no spinner or skeleton. | `src/pages/Profile.tsx` |

### Shifts (`/shifts`)
| # | Severity | Issue | Files |
|---|----------|-------|-------|
| S1 | High | **Inconsistent button styling** — Mix of `Button` component (Chakra-like) and raw `<button>` elements with inline Tailwind classes. | `src/pages/Shifts.tsx` |
| S2 | Medium | **Cash input has no currency formatting** — Plain number input, no `$` prefix or 2-decimal enforcement. | `src/pages/Shifts.tsx` |
| S3 | Medium | **No confirmation for shift close** — `requestClose()` fires immediately; no "are you sure" dialog. | `src/pages/Shifts.tsx` |
| S4 | Low | **Shift status badge uses inline style** — Conditional class string inline instead of a reusable status badge component. | `src/pages/Shifts.tsx` |

### Loyalty (`/loyalty`)
| # | Severity | Issue | Files |
|---|----------|-------|-------|
| L1 | Medium | **Uses `window.confirm()` for disenroll** — Browser-native dialog instead of custom modal. | `src/pages/Loyalty.tsx` |
| L2 | Medium | **No customer creation validation feedback** — Errors from `enrollCustomer` surface only via try/catch, no inline form errors. | `src/pages/Loyalty.tsx` |
| L3 | Low | **Earn/redeem inputs lack min/max visual cues** — Only enforced in JS, not in HTML attributes consistently. | `src/pages/Loyalty.tsx` |

### Privacy (`/privacy`)
| # | Severity | Issue | Files |
|---|----------|-------|-------|
| PR1 | High | **Complex form with no section grouping** — Consent, export, deletion, retention all in one flat form without fieldsets or section headers. | `src/pages/Privacy.tsx` |
| PR2 | Medium | **No file download for export** — `exportJson` state holds JSON string but no download button/link. | `src/pages/Privacy.tsx` |
| PR3 | Medium | **Uses `window.confirm()` for deletion** — Should be custom modal. | `src/pages/Privacy.tsx` |
| PR4 | Low | **No success/error toast** — Uses `statusMessage` state text only. | `src/pages/Privacy.tsx` |

### Reports (`/reports`)
| # | Severity | Issue | Files |
|---|----------|-------|-------|
| R1 | Medium | **No pagination on data preview** — Shows first 50 rows with a note; no page navigation for larger datasets. | `src/pages/Reports.tsx` |
| R2 | Medium | **Export fires immediately on button click** — No confirmation dialog for potentially large exports. | `src/pages/Reports.tsx` |
| R3 | Low | **Headers derived from first row only** — If first row is missing keys, headers are incomplete. | `src/pages/Reports.tsx` |

### SignIn (`/signin`)
| # | Severity | Issue | Files |
|---|----------|-------|-------|
| SI1 | High | **Inconsistent with SignUp** — SignIn uses plain HTML + Tailwind; SignUp uses Chakra UI. Different visual design entirely. | `src/pages/SignIn.tsx` |
| SI2 | Medium | **No password visibility toggle** — Password field is plain `type="password"` with no show/hide option. | `src/pages/SignIn.tsx` |
| SI3 | Medium | **No "remember me" or "forgot password"** — Missing common auth patterns. | `src/pages/SignIn.tsx` |
| SI4 | Low | **No autofill attributes** — Missing `autocomplete="email"` / `autocomplete="current-password"`. | `src/pages/SignIn.tsx` |

### SignUp (`/signup`)
| # | Severity | Issue | Files |
|---|----------|-------|-------|
| SU1 | High | **Inconsistent with SignIn** — Chakra UI vs plain HTML/Tailwind. | `src/pages/SignUp.tsx` |
| SU2 | Medium | **No password strength indicator** — No feedback on password complexity. | `src/pages/SignUp.tsx` |
| SU3 | Medium | **Name marked optional but no explanation** — Placeholder says optional but no helper text. | `src/pages/SignUp.tsx` |

### TenantTest (`/tenant-test`)
| # | Severity | Issue | Files |
|---|----------|-------|-------|
| TT1 | Low | **Debug page in production nav** — Navigation links to `/tenant-test` which is a developer test page. | `src/components/Navigation.tsx`, `src/pages/TenantTest.tsx` |

---

## Pages Not Routed (in `src/pages/` but missing from `App.tsx` routes)

| Page | Status | Notes |
|------|--------|-------|
| `OrderNew.tsx` | Not routed | Uses inline styles + `OrderCreationWithSummary` (mock data, not connected to real API) |
| `OrderCreation.tsx` | Not routed | Connected to real APIs but not linked from any route |
| `KitchenDisplay.tsx` | Not routed | CSS classes (`status-pending`, etc.) referenced but no CSS defined; action buttons are no-ops |
| `Inventory.tsx` | Not routed | Works but uses raw HTML buttons, modal has inline styles |
| `ConnectionTest.tsx` | Not routed | Very basic, just renders `ConnectionStatus` |
| `Products.tsx` | Not routed | Grid layout works; "Add to Order" just shows `alert()` |
| `ReceiptView.tsx` | Component only | Not a route; used as a sub-component |

---

## Top Priority Fixes (High Severity)

1. **Fix Profile.tsx structural bug** — Remove stray `;` so authenticated users see their profile.
2. **Add global Navigation shell** — Render `Navigation` in `App.tsx` so users can navigate between routes.
3. **Unify styling approach** — Pick one approach (Tailwind classes used by most pages) and migrate inline-style/Chakra pages.
4. **Add consistent loading states** — Replace raw `Loading...` divs with a reusable loading spinner/skeleton component.
5. **Add consistent error states** — Reusable error display component with retry option.
6. **Fix Orders product name display** — Show product names instead of raw IDs.
7. **Add empty states** — For Orders, Reports, Loyalty list views.

## Recommended Approach

- **Phase 2a**: Fix Critical/High issues (Profile bug, Navigation shell, loading/error states, styling unification)
- **Phase 2b**: Fix Medium issues (empty states, a11y basics, responsive improvements, button consistency)
- **Phase 2c**: Low issues and polish (skip links, pagination, download buttons, etc.)
