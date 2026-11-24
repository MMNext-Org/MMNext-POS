-- Migration 011: Configurable earn rules and rewards (Task 4).
--
-- Adds organization-scoped earn configuration, a reward catalog, and an
-- append-only payment-events ledger for idempotent earn/reversal. All point
-- math is integer-based with deterministic round-half-up rounding.

-- 1) Earn rules: how many points a customer earns per eligible payment.
CREATE TABLE IF NOT EXISTS loyalty_earn_rules (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    points_per_currency_unit INTEGER NOT NULL CHECK (points_per_currency_unit > 0),
    currency_minor_unit INTEGER NOT NULL DEFAULT 100 CHECK (currency_minor_unit > 0),
    min_spend_minor_units INTEGER NOT NULL DEFAULT 0 CHECK (min_spend_minor_units >= 0),
    max_points_per_transaction INTEGER CHECK (max_points_per_transaction > 0),
    eligible_tiers TEXT[] DEFAULT NULL,
    eligible_channels TEXT[] DEFAULT NULL,
    active_from TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    active_until TIMESTAMPTZ,
    version INTEGER NOT NULL DEFAULT 1,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (organization_id, version)
);

CREATE INDEX IF NOT EXISTS idx_loyalty_earn_rules_org ON loyalty_earn_rules(organization_id);
CREATE INDEX IF NOT EXISTS idx_loyalty_earn_rules_active ON loyalty_earn_rules(organization_id, is_active, active_from, active_until);

-- 2) Reward catalog: redeemable rewards with point cost and monetary value.
CREATE TABLE IF NOT EXISTS loyalty_rewards (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    name TEXT NOT NULL,
    description TEXT,
    point_cost INTEGER NOT NULL CHECK (point_cost > 0),
    reward_value_minor_units INTEGER NOT NULL DEFAULT 0 CHECK (reward_value_minor_units >= 0),
    reward_type TEXT NOT NULL DEFAULT 'discount' CHECK (reward_type IN ('discount', 'free_item', 'voucher', 'credit')),
    eligible_tiers TEXT[] DEFAULT NULL,
    active_from TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    active_until TIMESTAMPTZ,
    version INTEGER NOT NULL DEFAULT 1,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (organization_id, name, version)
);

CREATE INDEX IF NOT EXISTS idx_loyalty_rewards_org ON loyalty_rewards(organization_id);
CREATE INDEX IF NOT EXISTS idx_loyalty_rewards_active ON loyalty_rewards(organization_id, is_active, active_from, active_until);

-- 3) Payment events: append-only ledger linking payments to loyalty earn/reversal.
--    Idempotency key prevents duplicate earn for the same payment event.
CREATE TABLE IF NOT EXISTS loyalty_payment_events (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    loyalty_account_id UUID NOT NULL REFERENCES loyalty_accounts(id) ON DELETE CASCADE,
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    customer_id UUID NOT NULL REFERENCES customers(id) ON DELETE CASCADE,
    payment_id UUID REFERENCES payments(id) ON DELETE SET NULL,
    order_id UUID REFERENCES orders(id) ON DELETE SET NULL,
    event_type TEXT NOT NULL CHECK (event_type IN ('payment_completed', 'payment_refunded', 'payment_voided')),
    points_earned INTEGER NOT NULL CHECK (points_earned != 0),
    balance_after INTEGER NOT NULL CHECK (balance_after >= 0),
    idempotency_key UUID NOT NULL,
    reason TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_loyalty_payment_events_idempotency
  ON loyalty_payment_events(idempotency_key);
CREATE INDEX IF NOT EXISTS idx_loyalty_payment_events_account ON loyalty_payment_events(loyalty_account_id);
CREATE INDEX IF NOT EXISTS idx_loyalty_payment_events_payment ON loyalty_payment_events(payment_id);
CREATE INDEX IF NOT EXISTS idx_loyalty_payment_events_org ON loyalty_payment_events(organization_id);

-- 4) Helper: compute points earned from a payment amount using integer math.
--    points = floor(amount_minor_units * points_per_currency_unit / currency_minor_unit)
CREATE OR REPLACE FUNCTION loyalty_points_earned(
    p_amount_minor_units INTEGER,
    p_points_per_currency_unit INTEGER,
    p_currency_minor_unit INTEGER
)
RETURNS INTEGER
LANGUAGE plpgsql IMMUTABLE AS $$
DECLARE
    v_raw NUMERIC;
BEGIN
    v_raw := (p_amount_minor_units::NUMERIC * p_points_per_currency_unit) / p_currency_minor_unit::NUMERIC;
    RETURN floor(v_raw)::INTEGER;
END;
$$;

-- 5) Helper: round-half-up integer proration for partial refund restoration.
--    Returns floor((numerator * denominator_scale + denominator/2) / denominator)
--    using integer-only arithmetic to avoid float drift.
CREATE OR REPLACE FUNCTION loyalty_prorate_points(
    p_total_points INTEGER,
    p_refund_minor_units INTEGER,
    p_original_minor_units INTEGER
)
RETURNS INTEGER
LANGUAGE plpgsql IMMUTABLE AS $$
BEGIN
    IF p_original_minor_units <= 0 THEN
        RETURN 0;
    END IF;
    RETURN ((p_total_points::NUMERIC * p_refund_minor_units) / p_original_minor_units::NUMERIC)::INTEGER;
END;
$$;

-- 6) Earn rule lookup: active rule for an organization at a given time.
CREATE OR REPLACE FUNCTION loyalty_active_earn_rule(
    p_org_id UUID,
    p_at TIMESTAMPTZ DEFAULT NOW()
)
RETURNS TABLE (
    id UUID,
    organization_id UUID,
    points_per_currency_unit INTEGER,
    currency_minor_unit INTEGER,
    min_spend_minor_units INTEGER,
    max_points_per_transaction INTEGER,
    eligible_tiers TEXT[],
    eligible_channels TEXT[],
    version INTEGER
)
LANGUAGE plpgsql STABLE AS $$
BEGIN
    RETURN QUERY
    SELECT r.id, r.organization_id, r.points_per_currency_unit, r.currency_minor_unit,
           r.min_spend_minor_units, r.max_points_per_transaction,
           r.eligible_tiers, r.eligible_channels, r.version
    FROM loyalty_earn_rules r
    WHERE r.organization_id = p_org_id
      AND r.is_active = TRUE
      AND r.active_from <= p_at
      AND (r.active_until IS NULL OR r.active_until > p_at)
    ORDER BY r.version DESC
    LIMIT 1;
END;
$$;

-- 7) Reward lookup: active reward by name for an organization at a given time.
CREATE OR REPLACE FUNCTION loyalty_active_reward(
    p_org_id UUID,
    p_name TEXT,
    p_at TIMESTAMPTZ DEFAULT NOW()
)
RETURNS TABLE (
    id UUID,
    organization_id UUID,
    name TEXT,
    point_cost INTEGER,
    reward_value_minor_units INTEGER,
    reward_type TEXT,
    eligible_tiers TEXT[],
    version INTEGER
)
LANGUAGE plpgsql STABLE AS $$
BEGIN
    RETURN QUERY
    SELECT r.id, r.organization_id, r.name, r.point_cost, r.reward_value_minor_units,
           r.reward_type, r.eligible_tiers, r.version
    FROM loyalty_rewards r
    WHERE r.organization_id = p_org_id
      AND r.name = p_name
      AND r.is_active = TRUE
      AND r.active_from <= p_at
      AND (r.active_until IS NULL OR r.active_until > p_at)
    ORDER BY r.version DESC
    LIMIT 1;
END;
$$;
