-- Migration 007: Shift handover, manager approval for close, tips allocation
-- Extends register_shifts with lifecycle fields and adds supporting tables.

-- Shift lifecycle + approval + tips columns on register_shifts.
ALTER TABLE register_shifts ADD COLUMN organization_id TEXT REFERENCES organizations(id) ON DELETE CASCADE;
ALTER TABLE register_shifts ADD COLUMN status TEXT NOT NULL DEFAULT 'open' CHECK (status IN ('open', 'pending_approval', 'closed', 'handed_over'));
ALTER TABLE register_shifts ADD COLUMN handed_over_to TEXT REFERENCES auth_users(id) ON DELETE SET NULL;
ALTER TABLE register_shifts ADD COLUMN handed_over_by TEXT REFERENCES auth_users(id) ON DELETE SET NULL;
ALTER TABLE register_shifts ADD COLUMN handed_over_at TEXT;
ALTER TABLE register_shifts ADD COLUMN approval_status TEXT NOT NULL DEFAULT 'none' CHECK (approval_status IN ('none', 'pending', 'approved', 'rejected'));
ALTER TABLE register_shifts ADD COLUMN approved_by TEXT REFERENCES auth_users(id) ON DELETE SET NULL;
ALTER TABLE register_shifts ADD COLUMN approved_at TEXT;
ALTER TABLE register_shifts ADD COLUMN tips_total REAL NOT NULL DEFAULT 0 CHECK (tips_total >= 0);
ALTER TABLE register_shifts ADD COLUMN tips_allocated REAL NOT NULL DEFAULT 0 CHECK (tips_allocated >= 0);

-- Shift handover records.
CREATE TABLE IF NOT EXISTS shift_handover (
    id TEXT NOT NULL PRIMARY KEY,
    organization_id TEXT NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    register_shift_id TEXT NOT NULL REFERENCES register_shifts(id) ON DELETE CASCADE,
    from_user_id TEXT NOT NULL REFERENCES auth_users(id) ON DELETE CASCADE,
    to_user_id TEXT NOT NULL REFERENCES auth_users(id) ON DELETE CASCADE,
    status TEXT NOT NULL DEFAULT 'pending' CHECK (status IN ('pending', 'accepted', 'rejected')),
    created_at TEXT NOT NULL DEFAULT (datetime('now')),
    updated_at TEXT NOT NULL DEFAULT (datetime('now'))
);

-- Tips allocation records.
CREATE TABLE IF NOT EXISTS tips_allocations (
    id TEXT NOT NULL PRIMARY KEY,
    organization_id TEXT NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    register_shift_id TEXT NOT NULL REFERENCES register_shifts(id) ON DELETE CASCADE,
    allocated_by TEXT NOT NULL REFERENCES auth_users(id) ON DELETE CASCADE,
    total_amount REAL NOT NULL CHECK (total_amount >= 0),
    status TEXT NOT NULL DEFAULT 'pending' CHECK (status IN ('pending', 'completed', 'reversed')),
    created_at TEXT NOT NULL DEFAULT (datetime('now')),
    updated_at TEXT NOT NULL DEFAULT (datetime('now'))
);

-- Per-user tip amounts within a tips allocation.
CREATE TABLE IF NOT EXISTS tips_allocation_items (
    id TEXT NOT NULL PRIMARY KEY,
    tips_allocation_id TEXT NOT NULL REFERENCES tips_allocations(id) ON DELETE CASCADE,
    user_id TEXT NOT NULL REFERENCES auth_users(id) ON DELETE CASCADE,
    amount REAL NOT NULL CHECK (amount >= 0),
    created_at TEXT NOT NULL DEFAULT (datetime('now'))
);

-- Manager approvals required to finalize (close) a register shift.
CREATE TABLE IF NOT EXISTS shift_approvals (
    id TEXT NOT NULL PRIMARY KEY,
    organization_id TEXT NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    register_shift_id TEXT NOT NULL REFERENCES register_shifts(id) ON DELETE CASCADE,
    approver_id TEXT NOT NULL REFERENCES auth_users(id) ON DELETE CASCADE,
    status TEXT NOT NULL DEFAULT 'pending' CHECK (status IN ('pending', 'approved', 'rejected')),
    reason TEXT,
    created_at TEXT NOT NULL DEFAULT (datetime('now')),
    updated_at TEXT NOT NULL DEFAULT (datetime('now'))
);
