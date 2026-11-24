# BREWFLOW POS Architecture Overview

## 1. System Architecture

```
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

## 2. Component Responsibilities

**PWA Client**
- Render UI and manage view state
- Query operational data from local SQLite
- Apply validated local-first commands in SQLite transactions
- Display online/offline/sync/conflict states
- Cache application shell and static assets
- Never contain Supabase service-role or secret keys

**PowerSync**
- Maintain local SQLite replica
- Download only data allowed by Sync Rules
- Queue client writes for upload
- Expose synchronization status and upload errors
- Provide path to custom upload and conflict-resolution logic

**Supabase**
- PostgreSQL: canonical cloud data store
- Auth: identity and sessions
- RLS: tenant, branch, and membership isolation
- Storage: product images, logos, attachments, exports
- Edge Functions: trusted commands, tenant provisioning, billing integration, payment callbacks, privileged operations
- Realtime: selective transient events/presence where PowerSync replication is not appropriate

## 3. Data Flow

1. **User Interaction**: Cashier creates an order in the PWA UI (local SQLite)
2. **Local Validation**: Form validation with Zod, business rule checks
3. **Local Transaction**: Write to SQLite within a transaction
4. **Sync Queue**: PowerSync automatically detects the write and queues it
5. **Sync Execution**: When online, PowerSync uploads the queued write
6. **Cloud Processing**: Supabase Edge Function validates the write server-side
6. **Conflict Resolution**: If conflicts detected, edge function returns resolution strategy
7. **Status Update**: UI shows sync status; idempotent operations ensure no duplicate processing

## 4. Security Model

- **RLS Policies**: Enforce organization_id and branch_id scoping on all tenant tables
- **Session Context**: `set_app_context` sets session locals for RLS evaluation
- **No Service Role Keys**: Frontend only uses anon/public keys; secrets stored in environment
- **Financial Calculations**: Use Decimal type throughout to avoid floating-point errors
- **Idempotency**: All transaction endpoints require unique idempotency keys and check for existing records

## 5. Tenant Isolation

- **Organization Scope**: Every tenant-owned table includes `organization_id`
- **Branch Scope**: Tables with branch-specific records also include `branch_id`
- **RLS**: Policies reference session locals (current_organization_id, current_branch_id)
- **Device Registration**: Devices belong to organization and optionally branch
- **Audit Trails**: All financial operations log user, timestamp, and before/after state

## 6. Offline Support

- **Local Database**: SQLite is the primary operational store
- **Sync Rules**: Define what data is synchronized and when
- **Conflict Handling**: Automatic conflict detection with configurable resolution strategy
- **Status Indicators**: UI shows online/offline/syncing/error states
- **Retry Logic**: Automatic retries with exponential backoff for failed syncs

## 7. Authentication Flow

1. User enters credentials
2. Supabase Auth signInWithPassword
3. On success, retrieve user.app_metadata (organization_id, branch_id, role_id, role_name)
4. Call `set_app_context` with 4 parameters
5. Session locals are set for RLS evaluation
6. Redirect to appropriate dashboard

## 8. Environment Configuration

- `.env.example` provides required environment variables
- Development: Local SQLite file + local Supabase instance (if available)
- Production: Supabase cloud services only (PowerSync handles local replication)

This architecture provides a solid foundation for Stage 0 proof-of-concept while maintaining the requirements for production readiness.