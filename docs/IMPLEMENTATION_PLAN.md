# BREWFLOW POS Implementation Plan

## Stage 0: Technical Proof of Concept

### Current Status
✅ Completed:
- Fixed TypeScript configuration to enable proper build
- Implemented robust idempotency in order creation to prevent duplicate orders
- Ensured decimal arithmetic for financial accuracy (prevents floating-point errors)
- Maintained existing database schema with UNIQUE constraint for idempotency checks
- Added audit logging for financial operations

### Next Steps
1. ✅ Implement full authentication flow using Supabase/Auth
   - ✅ Login flow with Supabase Auth signInWithPassword
   - ✅ Register flow with Supabase Auth signUp
   - ✅ Extract organization_id, branch_id, role_id, role_name from user.app_metadata
   - ✅ Implement set_app_context RPC call with 4 parameters
   - ✅ Set session locals for RLS evaluation
   - ✅ Implement logout functionality

2. ✅ Add local SQLite persistence and PowerSync integration
   - ✅ Install PowerSync SDK
   - ✅ Configure local SQLite adapter
   - ✅ Implement sync callbacks for orders and other resources
   - ✅ Test offline order creation and local storage
   - ✅ Verify automatic sync after reconnection

3. ✅ Create offline cash order creation workflow
   - ✅ Build order creation UI with connection status indicator
   - ✅ Implement local-first transaction processing
   - ✅ Queue operations for upload when online

4. ✅ Verify connection and synchronization status indicators
   - ✅ Display online/offline/syncing/error status in UI
   - ✅ Implement retry logic for failed syncs

5. ✅ Write unit and integration tests for the idempotent order endpoint
   - ✅ Test idempotency key generation and validation
   - ✅ Verify existing order detection with matching parameters
   - ✅ Test item comparison serialization
   - ✅ Validate decimal arithmetic accuracy in financial calculations

6. ⏳ Ensure tenant isolation through RLS policies in PostgreSQL (future migration)
   - ✅ Review schema.sql for organization_id and branch_id columns
   - ✅ Verify set_app_context function properly sets session locals
   - ⏳ Create and test RLS policies for tenant isolation
   - ⏳ Verify no cross-tenant data access possible

### Technical Architecture
- **Frontend**: React + Vite + TypeScript
- **Backend**: Node.js with SQLite database
- **Database**: PostgreSQL via Supabase Cloud with RLS policies
- **Authentication**: Supabase Auth
- **Sync**: PowerSync Web SDK with local SQLite
- **Persistence**: Service Worker + Local Storage for PWA
- **Data Flow**: 
  - Offline-first operations using local SQLite
  - PowerSync handles synchronization between local and cloud
  - Tenant isolation enforced via organization_id and branch_id scoping
  - Idempotent transaction handling to prevent duplicates

### Files Modified
- `src/tsconfig.node.json`: Updated to commonjs module system and es2020 target
- `backend/order creation endpoint`: Enhanced with idempotency protection
- `src/utils/audit.ts`: Added audit logging for financial operations

### Testing Plan
- Unit tests for idempotency logic
- Integration tests for order creation flow
- Tests for decimal arithmetic accuracy
- Connection status indicator tests