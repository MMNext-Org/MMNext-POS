## Stage 0 Decisions

### 1. Offline Storage Strategy
Selected local SQLite as the primary offline storage mechanism, rejecting Dexie.js + IndexedDB due to complexity and the need for transactional integrity with PowerSync synchronization.

### 2. Idempotency Implementation
Implemented idempotency protection in order creation endpoints using:
- Composite keys: organizationId + branchId + status + total + serialized items
- Decimal arithmetic for financial accuracy
- Database-level uniqueness constraints
- Audit logging for all financial operations

### 3. Connection Status Indicators
Implemented using real browser online/offline events rather than simulated status, providing accurate network state detection.

### 4. Architecture Approach
Adopted tenant-scoped synchronization with PowerSync using organization_id and branch_code parameters, with local SQLite as the source of truth during offline periods.

### 5. Database Schema Design
Extended schema with:
- organization_id (tenant isolation)
- branch_id (branch ownership)
- order_status (enumerated states for order lifecycle)
- Financial records with decimal precision for accurate monetary calculations

These decisions were validated against the full BREWFLOW POS Full Commercial SaaS Specification and confirmed to meet all mandatory requirements.