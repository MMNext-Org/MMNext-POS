# BREWFLOW POS

A commercial multi-tenant Coffee Shop SaaS platform built with:

- **Frontend**: React + TypeScript + Vite + Tailwind CSS
- **Backend**: Supabase (PostgreSQL + Auth + Storage) + PowerSync (local-first sync with SQLite)
- **Architecture**: Multi-tenant (organization_id, branch_id), Row Level Security (RLS), offline-first data sync
- **Features**: Product catalog, POS ordering, inventory & costing, offline cash payment, tenant management, SaaS billing & subscription console

## Repository Structure

```
apps/
├── web/                     # Vite React PWA (main POS UI)
└── platform-admin/          # optional admin app

packages/
├── domain/                  # business rules and state machines
├── validation/              # Zod schemas
├── database/                # generated types and query helpers
├── powersync/               # local DB schema, connectors, sync hooks
├── permissions/             # permission keys and checks
├── reporting/               # report calculations and DTOs
└── config/                  # eslint, tsconfig, build config

supabase/
├── migrations/              # Supabase migrations
├── functions/               # tenant-provision, sync-upload, payment-webhook, etc.
├── tests/                   # pgTAP and RLS tests
└── seed.sql                 # seed data

docs/
├── PRD.md                   # product requirements doc
├── ARCHITECTURE.md          # system architecture overview
├── DATABASE.md              # DB schema details
├── SYNC_PROTOCOL.md         # sync rules and protocol
├── SECURITY.md              # RLS, auth, data protection
├── API.md                   # API contracts
└── UI_SPEC.md               # UI/UX specifications

e2e/                         # end-to-end tests (Playwright)
scripts/                     # automation scripts
.env.example                   # environment variables template
package.json
package-lock.json
tsconfig.json
tsconfig.app.json
tsconfig.node.json
```

## Getting Started

```bash
# Install dependencies
npm install

# Start development server (with hot reload)
npm run dev

# Build for production
npm run build
```

## Next Steps (Phase 0)

1. ✅ Set up monorepo directory structure
2. ✅ Initialize Git repo, add .gitignore, LICENSE
3. ✅ Add ESLint, Prettier, TypeScript config, Vitest, Testing Library
4. ✅ Add UI design tokens (color, spacing, typography) using CSS variables
5. ✅ Set up Vite + React + TypeScript project in `apps/web`

Next step is to initialize the web project and add initial files.
