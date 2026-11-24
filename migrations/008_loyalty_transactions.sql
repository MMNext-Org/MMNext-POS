-- Migration 008: Add loyalty_transactions table for tracking points earned/redeemed
CREATE TABLE IF NOT EXISTS loyalty_transactions (
    id TEXT NOT NULL PRIMARY KEY,
    loyalty_account_id TEXT NOT NULL REFERENCES loyalty_accounts(id) ON DELETE CASCADE,
    organization_id TEXT NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    customer_id TEXT NOT NULL REFERENCES customers(id) ON DELETE CASCADE,
    type TEXT NOT NULL CHECK (type IN ('earn', 'redeem', 'expire', 'adjust')),
    points INTEGER NOT NULL CHECK (points != 0),
    balance_after INTEGER NOT NULL CHECK (balance_after >= 0),
    reason TEXT,
    reference_id TEXT,
    created_at TEXT NOT NULL DEFAULT (datetime('now')),
    updated_at TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE INDEX IF NOT EXISTS idx_loyalty_transactions_account ON loyalty_transactions(loyalty_account_id);
CREATE INDEX IF NOT EXISTS idx_loyalty_transactions_customer ON loyalty_transactions(customer_id);
CREATE INDEX IF NOT EXISTS idx_loyalty_transactions_org ON loyalty_transactions(organization_id);
