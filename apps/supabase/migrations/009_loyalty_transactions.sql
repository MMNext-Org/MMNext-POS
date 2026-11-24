-- Migration 009: Add loyalty_transactions table for tracking points earned/redeemed
CREATE TABLE IF NOT EXISTS loyalty_transactions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    loyalty_account_id UUID NOT NULL REFERENCES loyalty_accounts(id) ON DELETE CASCADE,
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    customer_id UUID NOT NULL REFERENCES customers(id) ON DELETE CASCADE,
    type TEXT NOT NULL CHECK (type IN ('earn', 'redeem', 'expire', 'adjust')),
    points INTEGER NOT NULL CHECK (points != 0),
    balance_after INTEGER NOT NULL CHECK (balance_after >= 0),
    reason TEXT,
    reference_id TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_loyalty_transactions_account ON loyalty_transactions(loyalty_account_id);
CREATE INDEX IF NOT EXISTS idx_loyalty_transactions_customer ON loyalty_transactions(customer_id);
CREATE INDEX IF NOT EXISTS idx_loyalty_transactions_org ON loyalty_transactions(organization_id);

-- Loyalty account lifecycle metadata (Task 3).
ALTER TABLE loyalty_accounts ADD COLUMN IF NOT EXISTS status TEXT NOT NULL DEFAULT 'active' CHECK (status IN ('active', 'disenrolled', 'forfeited', 'closed'));
ALTER TABLE loyalty_accounts ADD COLUMN IF NOT EXISTS enrolled_at TIMESTAMPTZ;
ALTER TABLE loyalty_accounts ADD COLUMN IF NOT EXISTS disenrolled_at TIMESTAMPTZ;
ALTER TABLE loyalty_accounts ADD COLUMN IF NOT EXISTS consent_version TEXT;
ALTER TABLE loyalty_accounts ADD COLUMN IF NOT EXISTS consented_at TIMESTAMPTZ;
ALTER TABLE loyalty_accounts ADD COLUMN IF NOT EXISTS version INTEGER NOT NULL DEFAULT 1;

-- Idempotency key on the append-only ledger (Task 3).
ALTER TABLE loyalty_transactions ADD COLUMN IF NOT EXISTS idempotency_key UUID;
CREATE UNIQUE INDEX IF NOT EXISTS idx_loyalty_transactions_idempotency
  ON loyalty_transactions(idempotency_key)
  WHERE idempotency_key IS NOT NULL;

-- Loyalty ledger integrity + opening-balance backfill (Task 3).
CREATE OR REPLACE FUNCTION loyalty_discrepancy_report()
RETURNS TABLE (
    loyalty_account_id UUID,
    customer_id UUID,
    organization_id UUID,
    projected_balance INTEGER,
    ledger_balance INTEGER,
    difference INTEGER
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT la.id,
           la.customer_id,
           la.organization_id,
           la.points_balance,
           COALESCE(SUM(lt.points), 0)::INTEGER,
           (la.points_balance - COALESCE(SUM(lt.points), 0))::INTEGER
    FROM loyalty_accounts la
    LEFT JOIN loyalty_transactions lt ON lt.loyalty_account_id = la.id
    GROUP BY la.id, la.customer_id, la.organization_id, la.points_balance
    HAVING la.points_balance <> COALESCE(SUM(lt.points), 0);
END;
$$;

CREATE OR REPLACE FUNCTION loyalty_backfill_opening_balances()
RETURNS INTEGER
LANGUAGE plpgsql AS $$
DECLARE inserted_count INTEGER := 0;
BEGIN
    INSERT INTO loyalty_transactions (
        loyalty_account_id, organization_id, customer_id,
        type, points, balance_after, reason, reference_id, idempotency_key
    )
    SELECT la.id, la.organization_id, la.customer_id,
           'adjust', la.points_balance, la.points_balance,
           'Opening balance backfill', 'backfill:' || la.id::text,
           uuid_generate_v4()
    FROM loyalty_accounts la
    LEFT JOIN loyalty_transactions lt ON lt.loyalty_account_id = la.id
    WHERE lt.id IS NULL;
    GET DIAGNOSTICS inserted_count = ROW_COUNT;
    RETURN inserted_count;
END;
$$;
