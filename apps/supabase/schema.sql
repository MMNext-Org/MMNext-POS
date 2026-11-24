-- BREWFLOW POS Supabase Schema
-- Define UUID primary keys and composite uniqueness constraints (org_id + branch_code, org_id + device_code)
-- Index foreign keys used by RLS and Sync Rules (org_id, branch_id, product_id, etc.)
-- Add CHECK constraints for non‑negative quantities, valid state transitions, revenue and cost handling.

-- Enable UUID extension if not already enabled
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Tenancy tables
CREATE TABLE organizations (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE branches (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    branch_code TEXT NOT NULL,
    name TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (organization_id, branch_code)
);

-- Auth and RLS related tables
-- NOTE: Enable Supabase Auth and configure JWT claims with organization_id for RLS policies.
-- Example: In your auth settings, ensure the token includes a claim `organization_id` that matches the user's organization.
CREATE TABLE auth_users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    email TEXT NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    organization_id UUID REFERENCES organizations(id) ON DELETE CASCADE,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE devices (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    branch_id UUID REFERENCES branches(id) ON DELETE SET NULL,
    device_code TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (organization_id, device_code)
);

-- Tenant-owned business tables
CREATE TABLE products (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    branch_id UUID REFERENCES branches(id) ON DELETE SET NULL,
    name TEXT NOT NULL,
    description TEXT,
    price DECIMAL(12,2) NOT NULL CHECK (price >= 0),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (organization_id, name)
);

CREATE TABLE product_variants (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    product_id UUID NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    sku TEXT NOT NULL,
    name TEXT NOT NULL,
    price DECIMAL(12,2) NOT NULL CHECK (price >= 0),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (organization_id, sku)
);

CREATE TABLE categories (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    name TEXT NOT NULL,
    parent_category_id UUID REFERENCES categories(id) ON DELETE SET NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE modifiers (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    name TEXT NOT NULL,
    price DECIMAL(12,2) NOT NULL CHECK (price >= 0),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE product_modifier_relations (
    product_variant_id UUID NOT NULL REFERENCES product_variants(id) ON DELETE CASCADE,
    modifier_id UUID NOT NULL REFERENCES modifiers(id) ON DELETE CASCADE,
    PRIMARY KEY (product_variant_id, modifier_id)
);

CREATE TABLE ingredients (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    name TEXT NOT NULL,
    unit TEXT NOT NULL,
    conversion_rate DECIMAL(10,5) NOT NULL CHECK (conversion_rate > 0),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (organization_id, name)
);

CREATE TABLE recipes (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    name TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE recipe_items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    recipe_id UUID NOT NULL REFERENCES recipes(id) ON DELETE CASCADE,
    ingredient_id UUID NOT NULL REFERENCES ingredients(id) ON DELETE CASCADE,
    quantity DECIMAL(12,5) NOT NULL CHECK (quantity >= 0),
    unit_id UUID NOT NULL REFERENCES ingredients(id) ON DELETE CASCADE
);

CREATE TABLE stock_locations (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    branch_id UUID REFERENCES branches(id) ON DELETE SET NULL,
    name TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (organization_id, branch_id, name)
);

CREATE TABLE stock_movements (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    branch_id UUID REFERENCES branches(id) ON DELETE SET NULL,
    stock_location_id UUID NOT NULL REFERENCES stock_locations(id) ON DELETE CASCADE,
    product_variant_id UUID NOT NULL REFERENCES product_variants(id) ON DELETE CASCADE,
    quantity DECIMAL(12,5) NOT NULL CHECK (quantity >= 0),
    movement_type TEXT NOT NULL CHECK (movement_type IN ('in', 'out', 'transfer', 'adjustment')),
    reference_number TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE orders (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    branch_id UUID REFERENCES branches(id) ON DELETE SET NULL,
    table_number TEXT,
    status TEXT NOT NULL CHECK (status IN ('draft', 'placed', 'accepted', 'preparing', 'ready', 'served', 'completed', 'cancelled')),
    total DECIMAL(12,2) NOT NULL CHECK (total >= 0),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE order_items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    order_id UUID NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    product_variant_id UUID NOT NULL REFERENCES product_variants(id) ON DELETE CASCADE,
    quantity INTEGER NOT NULL CHECK (quantity > 0),
    price DECIMAL(12,2) NOT NULL CHECK (price >= 0),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE payments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    order_id UUID NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    amount DECIMAL(12,2) NOT NULL CHECK (amount >= 0),
    method TEXT NOT NULL CHECK (method IN ('cash', 'card', 'qr', 'mobile')),
    status TEXT NOT NULL CHECK (status IN ('pending', 'completed', 'failed', 'refunded')),
    provider_transaction_id TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (order_id, provider_transaction_id)
);

CREATE TABLE refunds (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    order_id UUID NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    amount DECIMAL(12,2) NOT NULL CHECK (amount >= 0),
    reason TEXT,
    status TEXT NOT NULL CHECK (status IN ('pending', 'completed', 'failed')),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE customers (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    name TEXT NOT NULL,
    email TEXT,
    phone TEXT,
    loyalty_points INTEGER DEFAULT 0 CHECK (loyalty_points >= 0),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (organization_id, email)
);

CREATE TABLE loyalty_accounts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    customer_id UUID NOT NULL REFERENCES customers(id) ON DELETE CASCADE,
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    points_balance INTEGER NOT NULL CHECK (points_balance >= 0),
    tier TEXT,
    status TEXT NOT NULL DEFAULT 'active' CHECK (status IN ('active', 'disenrolled', 'forfeited', 'closed')),
    enrolled_at TIMESTAMPTZ,
    disenrolled_at TIMESTAMPTZ,
    consent_version TEXT,
    consented_at TIMESTAMPTZ,
    version INTEGER NOT NULL DEFAULT 1,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (organization_id, customer_id)
);

-- Consent history: append-only audit trail of customer consent events
-- (privacy law compliance — migration 014).
CREATE TABLE consent_history (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    customer_id UUID NOT NULL REFERENCES customers(id) ON DELETE CASCADE,
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    consent_type TEXT NOT NULL CHECK (consent_type IN ('loyalty_enrollment', 'marketing', 'data_processing', 'terms_of_service')),
    consented BOOLEAN NOT NULL DEFAULT TRUE,
    consent_version TEXT NOT NULL,
    consented_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    ip_address TEXT,
    user_agent TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_consent_history_customer ON consent_history(customer_id);
CREATE INDEX idx_consent_history_org ON consent_history(organization_id);
CREATE INDEX idx_consent_history_customer_type ON consent_history(customer_id, consent_type);
CREATE INDEX idx_consent_history_consented_at ON consent_history(consented_at);

-- Retention policy: per-organization data retention rules by data type
-- (privacy law compliance — migration 015).
CREATE TABLE retention_policy (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    data_type TEXT NOT NULL CHECK (data_type IN ('customer_profile', 'loyalty_transactions', 'consent_history', 'loyalty_accounts')),
    retention_period_days INTEGER NOT NULL CHECK (retention_period_days > 0),
    auto_delete BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (organization_id, data_type)
);

CREATE INDEX idx_retention_policy_org ON retention_policy(organization_id);
CREATE INDEX idx_retention_policy_data_type ON retention_policy(data_type);

CREATE TABLE suppliers (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    name TEXT NOT NULL,
    contact_info TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (organization_id, name)
);

CREATE TABLE purchase_orders (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    supplier_id UUID NOT NULL REFERENCES suppliers(id) ON DELETE CASCADE,
    status TEXT NOT NULL CHECK (status IN ('draft', 'pending', 'confirmed', 'received', 'cancelled')),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE purchase_order_items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    purchase_order_id UUID NOT NULL REFERENCES purchase_orders(id) ON DELETE CASCADE,
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    supplier_item_id UUID NOT NULL REFERENCES ingredients(id) ON DELETE CASCADE,
    quantity INTEGER NOT NULL CHECK (quantity > 0),
    unit_price DECIMAL(12,2) NOT NULL CHECK (unit_price >= 0),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE goods_receipts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    purchase_order_id UUID NOT NULL REFERENCES purchase_orders(id) ON DELETE CASCADE,
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    status TEXT NOT NULL CHECK (status IN ('pending', 'received', 'rejected')),
    received_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE goods_receipt_items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    goods_receipt_id UUID NOT NULL REFERENCES goods_receipts(id) ON DELETE CASCADE,
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    ingredient_id UUID NOT NULL REFERENCES ingredients(id) ON DELETE CASCADE,
    quantity INTEGER NOT NULL CHECK (quantity > 0),
    unit_price DECIMAL(12,2) NOT NULL CHECK (unit_price >= 0),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE registrars (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    name TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (organization_id, name)
);

CREATE TABLE register_shifts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    registrar_id UUID NOT NULL REFERENCES registrars(id) ON DELETE CASCADE,
    start_time TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    end_time TIMESTAMPTZ NOT NULL CHECK (end_time >= start_time),
    status TEXT NOT NULL DEFAULT 'open' CHECK (status IN ('open', 'pending_approval', 'closed', 'handed_over')),
    cash_in DECIMAL(12,2) NOT NULL DEFAULT 0 CHECK (cash_in >= 0),
    cash_out DECIMAL(12,2) NOT NULL DEFAULT 0 CHECK (cash_out >= 0),
    total DECIMAL(12,2) NOT NULL DEFAULT 0 CHECK (total >= 0 AND total = cash_in - cash_out),
    handed_over_to UUID REFERENCES auth_users(id) ON DELETE SET NULL,
    handed_over_by UUID REFERENCES auth_users(id) ON DELETE SET NULL,
    handed_over_at TIMESTAMPTZ,
    approval_status TEXT NOT NULL DEFAULT 'none' CHECK (approval_status IN ('none', 'pending', 'approved', 'rejected')),
    approved_by UUID REFERENCES auth_users(id) ON DELETE SET NULL,
    approved_at TIMESTAMPTZ,
    tips_total DECIMAL(12,2) NOT NULL DEFAULT 0 CHECK (tips_total >= 0),
    tips_allocated DECIMAL(12,2) NOT NULL DEFAULT 0 CHECK (tips_allocated >= 0),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE cash_movements (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    register_shift_id UUID NOT NULL REFERENCES register_shifts(id) ON DELETE CASCADE,
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    amount DECIMAL(12,2) NOT NULL CHECK (amount >= 0),
    type TEXT NOT NULL CHECK (type IN ('cash_in', 'cash_out', 'sale', 'refund')),
    description TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Shift handover records: tracks handing an open register shift from one staff member to another.
CREATE TABLE shift_handover (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    register_shift_id UUID NOT NULL REFERENCES register_shifts(id) ON DELETE CASCADE,
    from_user_id UUID NOT NULL REFERENCES auth_users(id) ON DELETE CASCADE,
    to_user_id UUID NOT NULL REFERENCES auth_users(id) ON DELETE CASCADE,
    status TEXT NOT NULL DEFAULT 'pending' CHECK (status IN ('pending', 'accepted', 'rejected')),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Tips allocation records: tracks distributing tip pool among staff for a shift.
CREATE TABLE tips_allocations (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    register_shift_id UUID NOT NULL REFERENCES register_shifts(id) ON DELETE CASCADE,
    allocated_by UUID NOT NULL REFERENCES auth_users(id) ON DELETE CASCADE,
    total_amount DECIMAL(12,2) NOT NULL CHECK (total_amount >= 0),
    status TEXT NOT NULL DEFAULT 'pending' CHECK (status IN ('pending', 'completed', 'reversed')),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Per-user tip amounts within a tips allocation.
CREATE TABLE tips_allocation_items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tips_allocation_id UUID NOT NULL REFERENCES tips_allocations(id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES auth_users(id) ON DELETE CASCADE,
    amount DECIMAL(12,2) NOT NULL CHECK (amount >= 0),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Loyalty transactions: tracks points earned and redeemed by customers.
CREATE TABLE loyalty_transactions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    loyalty_account_id UUID NOT NULL REFERENCES loyalty_accounts(id) ON DELETE CASCADE,
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    customer_id UUID NOT NULL REFERENCES customers(id) ON DELETE CASCADE,
    type TEXT NOT NULL CHECK (type IN ('earn', 'redeem', 'expire', 'adjust')),
    points INTEGER NOT NULL CHECK (points != 0),
    balance_after INTEGER NOT NULL CHECK (balance_after >= 0),
    reason TEXT,
    reference_id TEXT,
    idempotency_key UUID,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_loyalty_transactions_idempotency
  ON loyalty_transactions(idempotency_key)
  WHERE idempotency_key IS NOT NULL;

-- Loyalty ledger integrity: reports accounts whose cached points_balance diverges
-- from the append-only ledger sum (empty in a consistent state).
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

-- Backfill opening balances: seed one 'adjust' ledger row per account that has
-- no ledger rows yet, using points_balance as balance_after. Idempotent: safe
-- to re-run (only accounts with zero ledger rows get an entry).
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

-- Earn rules: organization-scoped configurable points-earning configuration.
CREATE TABLE loyalty_earn_rules (
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

-- Reward catalog: redeemable rewards with point cost and monetary value.
CREATE TABLE loyalty_rewards (
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

-- Payment events: append-only ledger linking payments to loyalty earn/reversal.
CREATE TABLE loyalty_payment_events (
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

-- Loyalty reservations: tracks active point reservations held during
-- redemption flows. Disenrollment rejects while an active reservation exists.
CREATE TABLE loyalty_reservations (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    loyalty_account_id UUID NOT NULL REFERENCES loyalty_accounts(id) ON DELETE CASCADE,
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    reward_id UUID REFERENCES loyalty_rewards(id) ON DELETE SET NULL,
    points_reserved INTEGER NOT NULL CHECK (points_reserved > 0),
    status TEXT NOT NULL DEFAULT 'active' CHECK (status IN ('active', 'committed', 'released', 'expired')),
    reserved_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    committed_at TIMESTAMPTZ,
    released_at TIMESTAMPTZ,
    expires_at TIMESTAMPTZ,
    idempotency_key UUID NOT NULL,
    reason TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_loyalty_reservations_idempotency
  ON loyalty_reservations(idempotency_key);
CREATE INDEX IF NOT EXISTS idx_loyalty_reservations_account_active
  ON loyalty_reservations(loyalty_account_id) WHERE status = 'active';
CREATE INDEX IF NOT EXISTS idx_loyalty_reservations_org ON loyalty_reservations(organization_id);

-- Helper: compute points earned from a payment amount using integer math.
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

-- Helper: round-half-up integer proration for partial refund restoration.
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

-- Earn rule lookup: active rule for an organization at a given time.
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

-- Reward lookup: active reward by name for an organization at a given time.
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

-- rpc_loyalty_enroll: enroll a customer in the loyalty program. Idempotent:
-- active accounts are returned unchanged; disenrolled/forfeited/closed
-- accounts are re-enrolled at zero balance (forfeited points not restored).
CREATE OR REPLACE FUNCTION rpc_loyalty_enroll(
    p_user_id UUID,
    p_customer_id UUID,
    p_org_id UUID,
    p_consent_version TEXT,
    p_branch_id UUID DEFAULT NULL
)
RETURNS TABLE (
    loyalty_account_id UUID,
    customer_id UUID,
    status TEXT,
    points_balance INTEGER,
    enrolled_at TIMESTAMPTZ,
    disenrolled_at TIMESTAMPTZ,
    action TEXT
)
LANGUAGE plpgsql AS $$
DECLARE
    v_existing_id UUID;
    v_existing_status TEXT;
    v_now TIMESTAMPTZ := NOW();
BEGIN
    PERFORM require_permission(p_user_id, 'loyalty.manage', p_org_id);

    IF p_branch_id IS NOT NULL AND NOT has_active_membership(p_user_id, p_org_id, p_branch_id) THEN
        RAISE EXCEPTION 'user % has no active membership in organization % branch %', p_user_id, p_org_id, p_branch_id;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM customers WHERE id = p_customer_id AND organization_id = p_org_id
    ) THEN
        RAISE EXCEPTION 'customer % does not exist in organization %', p_customer_id, p_org_id;
    END IF;

    SELECT loyalty_accounts.id, loyalty_accounts.status
      INTO v_existing_id, v_existing_status
    FROM loyalty_accounts
    WHERE loyalty_accounts.customer_id = p_customer_id
      AND loyalty_accounts.organization_id = p_org_id;

    IF v_existing_id IS NOT NULL THEN
        IF v_existing_status = 'active' THEN
            RETURN QUERY
            SELECT loyalty_accounts.id, loyalty_accounts.customer_id, loyalty_accounts.status,
                   loyalty_accounts.points_balance, loyalty_accounts.enrolled_at,
                   loyalty_accounts.disenrolled_at, 'already_enrolled'
            FROM loyalty_accounts WHERE id = v_existing_id;
            RETURN;
        END IF;

        UPDATE loyalty_accounts
        SET status = 'active',
            enrolled_at = v_now,
            disenrolled_at = NULL,
            consent_version = p_consent_version,
            consented_at = v_now,
            version = version + 1,
            points_balance = 0,
            updated_at = v_now
        WHERE id = v_existing_id;

        RETURN QUERY
        SELECT loyalty_accounts.id, loyalty_accounts.customer_id, loyalty_accounts.status,
               loyalty_accounts.points_balance, loyalty_accounts.enrolled_at,
               loyalty_accounts.disenrolled_at, 're_enrolled'
        FROM loyalty_accounts WHERE id = v_existing_id;
        RETURN;
    END IF;

    RETURN QUERY INSERT INTO loyalty_accounts (
        id, customer_id, organization_id, points_balance, tier,
        status, enrolled_at, consent_version, consented_at, version,
        created_at, updated_at
    ) VALUES (
        uuid_generate_v4(), p_customer_id, p_org_id, 0, 'bronze',
        'active', v_now, p_consent_version, v_now, 1, v_now, v_now
    )
    RETURNING loyalty_accounts.id, loyalty_accounts.customer_id, loyalty_accounts.status,
           loyalty_accounts.points_balance, loyalty_accounts.enrolled_at,
           loyalty_accounts.disenrolled_at, 'enrolled';
END;
$$;

-- rpc_loyalty_disenroll: disenroll a customer and forfeit available points.
-- Writes a forfeiture ledger entry reducing available points to zero.
-- Rejects disenrollment while an active redemption reservation exists.
CREATE OR REPLACE FUNCTION rpc_loyalty_disenroll(
    p_user_id UUID,
    p_customer_id UUID,
    p_org_id UUID,
    p_branch_id UUID DEFAULT NULL
)
RETURNS TABLE (
    loyalty_account_id UUID,
    customer_id UUID,
    status TEXT,
    points_balance INTEGER,
    forfeited_points INTEGER,
    disenrolled_at TIMESTAMPTZ,
    action TEXT
)
LANGUAGE plpgsql AS $$
DECLARE
    v_account_id UUID;
    v_account_status TEXT;
    v_points_balance INTEGER;
    v_forfeited INTEGER := 0;
    v_now TIMESTAMPTZ := NOW();
BEGIN
    PERFORM require_permission(p_user_id, 'loyalty.manage', p_org_id);

    IF p_branch_id IS NOT NULL AND NOT has_active_membership(p_user_id, p_org_id, p_branch_id) THEN
        RAISE EXCEPTION 'user % has no active membership in organization % branch %', p_user_id, p_org_id, p_branch_id;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM customers WHERE id = p_customer_id AND organization_id = p_org_id
    ) THEN
        RAISE EXCEPTION 'customer % does not exist in organization %', p_customer_id, p_org_id;
    END IF;

    SELECT loyalty_accounts.id, loyalty_accounts.status, loyalty_accounts.points_balance
      INTO v_account_id, v_account_status, v_points_balance
    FROM loyalty_accounts
    WHERE loyalty_accounts.customer_id = p_customer_id
      AND loyalty_accounts.organization_id = p_org_id;

    IF v_account_id IS NULL THEN
        RAISE EXCEPTION 'no loyalty account for customer % in organization %', p_customer_id, p_org_id;
    END IF;

    IF v_account_status IN ('disenrolled', 'closed') THEN
        RETURN QUERY
        SELECT loyalty_accounts.id, loyalty_accounts.customer_id, loyalty_accounts.status,
               loyalty_accounts.points_balance, 0, loyalty_accounts.disenrolled_at,
               'already_disenrolled'
        FROM loyalty_accounts WHERE id = v_account_id;
        RETURN;
    END IF;

    IF EXISTS (
        SELECT 1 FROM loyalty_reservations
        WHERE loyalty_reservations.loyalty_account_id = v_account_id
          AND loyalty_reservations.status = 'active'
    ) THEN
        RAISE EXCEPTION 'cannot disenroll customer %: active redemption reservation exists', p_customer_id;
    END IF;

    v_forfeited := v_points_balance;
    IF v_forfeited > 0 THEN
        INSERT INTO loyalty_transactions (
            loyalty_account_id, organization_id, customer_id,
            type, points, balance_after, reason, reference_id, idempotency_key
        ) VALUES (
            v_account_id, p_org_id, p_customer_id,
            'adjust', -v_forfeited, 0,
            'Forfeited on disenrollment', 'disenroll:' || v_account_id::text,
            uuid_generate_v4()
        );
    END IF;

    UPDATE loyalty_accounts
    SET status = 'disenrolled',
        disenrolled_at = v_now,
        points_balance = 0,
        version = version + 1,
        updated_at = v_now
    WHERE id = v_account_id;

    RETURN QUERY
    SELECT loyalty_accounts.id, loyalty_accounts.customer_id, loyalty_accounts.status,
           loyalty_accounts.points_balance, v_forfeited, loyalty_accounts.disenrolled_at,
           'disenrolled'
    FROM loyalty_accounts WHERE id = v_account_id;
END;
$$;

-- rpc_loyalty_redeem_quote: read-only eligibility + cost quote. No state change.
CREATE OR REPLACE FUNCTION rpc_loyalty_redeem_quote(
    p_user_id UUID,
    p_customer_id UUID,
    p_org_id UUID,
    p_points INTEGER,
    p_reward_id UUID,
    p_branch_id UUID DEFAULT NULL
)
RETURNS TABLE (
    reward_id UUID,
    reward_name TEXT,
    point_cost INTEGER,
    reward_value_minor_units INTEGER,
    reward_type TEXT,
    balance_before INTEGER,
    balance_after INTEGER,
    expires_at TIMESTAMPTZ
)
LANGUAGE plpgsql STABLE AS $$
DECLARE
    v_balance INTEGER;
    v_now TIMESTAMPTZ := NOW();
BEGIN
    PERFORM require_permission(p_user_id, 'loyalty.redeem', p_org_id);

    IF p_branch_id IS NOT NULL AND NOT has_active_membership(p_user_id, p_org_id, p_branch_id) THEN
        RAISE EXCEPTION 'user % has no active membership in organization % branch %', p_user_id, p_org_id, p_branch_id;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM customers WHERE id = p_customer_id AND organization_id = p_org_id
    ) THEN
        RAISE EXCEPTION 'customer % does not exist in organization %', p_customer_id, p_org_id;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM loyalty_rewards
        WHERE id = p_reward_id
          AND organization_id = p_org_id
          AND is_active = TRUE
          AND (active_until IS NULL OR active_until > v_now)
          AND active_from <= v_now
    ) THEN
        RAISE EXCEPTION 'reward % is not active in organization %', p_reward_id, p_org_id;
    END IF;

    IF p_points <= 0 THEN
        RAISE EXCEPTION 'points must be positive, got %', p_points;
    END IF;

    SELECT r.point_cost INTO v_balance
    FROM loyalty_rewards r WHERE r.id = p_reward_id;
    IF v_balance IS NULL OR v_balance <> p_points THEN
        RAISE EXCEPTION 'reward % point cost (%) does not match requested points (%)', p_reward_id, v_balance, p_points;
    END IF;

    SELECT loyalty_accounts.points_balance INTO v_balance
    FROM loyalty_accounts
    WHERE loyalty_accounts.customer_id = p_customer_id
      AND loyalty_accounts.organization_id = p_org_id
      AND loyalty_accounts.status = 'active';

    IF v_balance IS NULL THEN
        RAISE EXCEPTION 'no active loyalty account for customer % in organization %', p_customer_id, p_org_id;
    END IF;

    IF v_balance < p_points THEN
        RAISE EXCEPTION 'insufficient points: have %, need %', v_balance, p_points;
    END IF;

    RETURN QUERY
    SELECT r.id, r.name, r.point_cost, r.reward_value_minor_units, r.reward_type,
           v_balance, v_balance - p_points,
           (v_now + INTERVAL '30 minutes')
    FROM loyalty_rewards r WHERE r.id = p_reward_id;
END;
$$;

-- rpc_loyalty_redeem_reserve: lock points and create an active reservation.
-- Idempotent via idempotency_key. Row locks account with FOR UPDATE.
-- Points are deducted from balance and refunded on release/expire.
CREATE OR REPLACE FUNCTION rpc_loyalty_redeem_reserve(
    p_user_id UUID,
    p_customer_id UUID,
    p_org_id UUID,
    p_points INTEGER,
    p_reward_id UUID,
    p_branch_id UUID,
    p_idempotency_key UUID
)
RETURNS TABLE (
    reservation_id UUID,
    loyalty_account_id UUID,
    points_reserved INTEGER,
    status TEXT,
    reserved_at TIMESTAMPTZ,
    expires_at TIMESTAMPTZ
)
LANGUAGE plpgsql AS $$
DECLARE
    v_account_id UUID;
    v_balance INTEGER;
    v_account_status TEXT;
    v_reward_cost INTEGER;
    v_now TIMESTAMPTZ := NOW();
    v_expires_at TIMESTAMPTZ := NOW() + INTERVAL '30 minutes';
BEGIN
    PERFORM require_permission(p_user_id, 'loyalty.redeem', p_org_id);

    IF p_branch_id IS NOT NULL AND NOT has_active_membership(p_user_id, p_org_id, p_branch_id) THEN
        RAISE EXCEPTION 'user % has no active membership in organization % branch %', p_user_id, p_org_id, p_branch_id;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM customers WHERE id = p_customer_id AND organization_id = p_org_id
    ) THEN
        RAISE EXCEPTION 'customer % does not exist in organization %', p_customer_id, p_org_id;
    END IF;

    IF p_points <= 0 THEN
        RAISE EXCEPTION 'points must be positive, got %', p_points;
    END IF;

    IF p_idempotency_key IS NULL THEN
        RAISE EXCEPTION 'idempotency_key is required';
    END IF;

    SELECT r.point_cost INTO v_reward_cost
    FROM loyalty_rewards r
    WHERE r.id = p_reward_id
      AND r.organization_id = p_org_id
      AND r.is_active = TRUE
      AND (r.active_until IS NULL OR r.active_until > v_now)
      AND r.active_from <= v_now;

    IF v_reward_cost IS NULL THEN
        RAISE EXCEPTION 'reward % is not active in organization %', p_reward_id, p_org_id;
    END IF;

    IF v_reward_cost <> p_points THEN
        RAISE EXCEPTION 'reward % point cost (%) does not match requested points (%)', p_reward_id, v_reward_cost, p_points;
    END IF;

    -- Idempotency: return existing reservation if key already used.
    IF EXISTS (
        SELECT 1 FROM loyalty_reservations WHERE idempotency_key = p_idempotency_key
    ) THEN
        RETURN QUERY
        SELECT loyalty_reservations.id, loyalty_reservations.loyalty_account_id,
               loyalty_reservations.points_reserved, loyalty_reservations.status,
               loyalty_reservations.reserved_at, loyalty_reservations.expires_at
        FROM loyalty_reservations
        WHERE idempotency_key = p_idempotency_key;
        RETURN;
    END IF;

    -- Lock the account row to prevent concurrent over-reservation.
    SELECT loyalty_accounts.id, loyalty_accounts.points_balance, loyalty_accounts.status
      INTO v_account_id, v_balance, v_account_status
    FROM loyalty_accounts
    WHERE loyalty_accounts.customer_id = p_customer_id
      AND loyalty_accounts.organization_id = p_org_id
    FOR UPDATE;

    IF v_account_id IS NULL THEN
        RAISE EXCEPTION 'no loyalty account for customer % in organization %', p_customer_id, p_org_id;
    END IF;

    IF v_account_status <> 'active' THEN
        RAISE EXCEPTION 'loyalty account % is not active (status: %)', v_account_id, v_account_status;
    END IF;

    IF v_balance < p_points THEN
        RAISE EXCEPTION 'insufficient points: have %, need %', v_balance, p_points;
    END IF;

    IF EXISTS (
        SELECT 1 FROM loyalty_reservations
        WHERE loyalty_reservations.loyalty_account_id = v_account_id
          AND loyalty_reservations.status = 'active'
    ) THEN
        RAISE EXCEPTION 'active reservation already exists for account %', v_account_id;
    END IF;

    UPDATE loyalty_accounts
    SET points_balance = points_balance - p_points,
        updated_at = v_now
    WHERE id = v_account_id;

    RETURN QUERY INSERT INTO loyalty_reservations (
        id, loyalty_account_id, organization_id, reward_id,
        points_reserved, status, reserved_at, expires_at,
        idempotency_key, reason, created_at, updated_at
    ) VALUES (
        uuid_generate_v4(), v_account_id, p_org_id, p_reward_id,
        p_points, 'active', v_now, v_expires_at,
        p_idempotency_key,
        format('Redemption of %s points for reward %s', p_points, p_reward_id::text),
        v_now, v_now
    )
    RETURNING loyalty_reservations.id, loyalty_reservations.loyalty_account_id,
           loyalty_reservations.points_reserved, loyalty_reservations.status,
           loyalty_reservations.reserved_at, loyalty_reservations.expires_at;
END;
$$;

-- rpc_loyalty_redeem_commit: finalize an active reservation. Writes a redeem
-- ledger entry and marks the reservation committed.
CREATE OR REPLACE FUNCTION rpc_loyalty_redeem_commit(
    p_reservation_id UUID,
    p_user_id UUID
)
RETURNS TABLE (
    reservation_id UUID,
    loyalty_account_id UUID,
    points_redeemed INTEGER,
    balance_after INTEGER,
    transaction_id UUID,
    status TEXT,
    committed_at TIMESTAMPTZ
)
LANGUAGE plpgsql AS $$
DECLARE
    v_res_id UUID;
    v_account_id UUID;
    v_points INTEGER;
    v_res_status TEXT;
    v_org_id UUID;
    v_customer_id UUID;
    v_balance INTEGER;
    v_now TIMESTAMPTZ := NOW();
    v_txn_id UUID;
BEGIN
    SELECT loyalty_reservations.id, loyalty_reservations.loyalty_account_id,
           loyalty_reservations.points_reserved, loyalty_reservations.status,
           loyalty_reservations.organization_id
      INTO v_res_id, v_account_id, v_points, v_res_status, v_org_id
    FROM loyalty_reservations
    WHERE loyalty_reservations.id = p_reservation_id
    FOR UPDATE;

    IF v_res_id IS NULL THEN
        RAISE EXCEPTION 'reservation % does not exist', p_reservation_id;
    END IF;

    IF v_res_status <> 'active' THEN
        RAISE EXCEPTION 'reservation % is not active (status: %)', p_reservation_id, v_res_status;
    END IF;

    PERFORM require_permission(p_user_id, 'loyalty.redeem', v_org_id);

    SELECT loyalty_accounts.id, loyalty_accounts.points_balance, loyalty_accounts.customer_id
      INTO v_account_id, v_balance, v_customer_id
    FROM loyalty_accounts
    WHERE loyalty_accounts.id = v_account_id
    FOR UPDATE;

    v_txn_id := uuid_generate_v4();
    INSERT INTO loyalty_transactions (
        id, loyalty_account_id, organization_id, customer_id,
        type, points, balance_after, reason, reference_id, idempotency_key,
        created_at, updated_at
    ) VALUES (
        v_txn_id, v_account_id, v_org_id, v_customer_id,
        'redeem', -v_points, v_balance,
        format('Committed redemption of %s points', v_points),
        'redeem:' || v_res_id::text,
        uuid_generate_v4(),
        v_now, v_now
    );

    UPDATE loyalty_reservations
    SET status = 'committed',
        committed_at = v_now,
        updated_at = v_now
    WHERE id = p_reservation_id;

    RETURN QUERY
    SELECT loyalty_reservations.id, loyalty_reservations.loyalty_account_id,
           loyalty_reservations.points_reserved, v_balance,
           v_txn_id, loyalty_reservations.status, loyalty_reservations.committed_at
    FROM loyalty_reservations
    WHERE loyalty_reservations.id = p_reservation_id;
END;
$$;

-- rpc_loyalty_redeem_release: release an active reservation and refund
-- reserved points back to the account balance (refund reversal).
CREATE OR REPLACE FUNCTION rpc_loyalty_redeem_release(
    p_reservation_id UUID,
    p_user_id UUID
)
RETURNS TABLE (
    reservation_id UUID,
    loyalty_account_id UUID,
    points_refunded INTEGER,
    status TEXT,
    released_at TIMESTAMPTZ
)
LANGUAGE plpgsql AS $$
DECLARE
    v_res_id UUID;
    v_account_id UUID;
    v_points INTEGER;
    v_res_status TEXT;
    v_org_id UUID;
    v_now TIMESTAMPTZ := NOW();
BEGIN
    SELECT loyalty_reservations.id, loyalty_reservations.loyalty_account_id,
           loyalty_reservations.points_reserved, loyalty_reservations.status,
           loyalty_reservations.organization_id
      INTO v_res_id, v_account_id, v_points, v_res_status, v_org_id
    FROM loyalty_reservations
    WHERE loyalty_reservations.id = p_reservation_id
    FOR UPDATE;

    IF v_res_id IS NULL THEN
        RAISE EXCEPTION 'reservation % does not exist', p_reservation_id;
    END IF;

    IF v_res_status <> 'active' THEN
        RAISE EXCEPTION 'reservation % is not active (status: %)', p_reservation_id, v_res_status;
    END IF;

    PERFORM require_permission(p_user_id, 'loyalty.redeem', v_org_id);

    UPDATE loyalty_accounts
    SET points_balance = points_balance + v_points,
        updated_at = v_now
    WHERE id = v_account_id;

    UPDATE loyalty_reservations
    SET status = 'released',
        released_at = v_now,
        updated_at = v_now
    WHERE id = p_reservation_id;

    RETURN QUERY
    SELECT loyalty_reservations.id, loyalty_reservations.loyalty_account_id,
           loyalty_reservations.points_reserved, loyalty_reservations.status,
           loyalty_reservations.released_at
    FROM loyalty_reservations
    WHERE loyalty_reservations.id = p_reservation_id;
END;
$$;

-- rpc_loyalty_redeem_expire: expire an active reservation and refund
-- reserved points back to the account balance (refund reversal).
CREATE OR REPLACE FUNCTION rpc_loyalty_redeem_expire(
    p_reservation_id UUID,
    p_user_id UUID
)
RETURNS TABLE (
    reservation_id UUID,
    loyalty_account_id UUID,
    points_refunded INTEGER,
    status TEXT,
    expired_at TIMESTAMPTZ
)
LANGUAGE plpgsql AS $$
DECLARE
    v_res_id UUID;
    v_account_id UUID;
    v_points INTEGER;
    v_res_status TEXT;
    v_org_id UUID;
    v_now TIMESTAMPTZ := NOW();
BEGIN
    SELECT loyalty_reservations.id, loyalty_reservations.loyalty_account_id,
           loyalty_reservations.points_reserved, loyalty_reservations.status,
           loyalty_reservations.organization_id
      INTO v_res_id, v_account_id, v_points, v_res_status, v_org_id
    FROM loyalty_reservations
    WHERE loyalty_reservations.id = p_reservation_id
    FOR UPDATE;

    IF v_res_id IS NULL THEN
        RAISE EXCEPTION 'reservation % does not exist', p_reservation_id;
    END IF;

    IF v_res_status <> 'active' THEN
        RAISE EXCEPTION 'reservation % is not active (status: %)', p_reservation_id, v_res_status;
    END IF;

    PERFORM require_permission(p_user_id, 'loyalty.redeem', v_org_id);

    UPDATE loyalty_accounts
    SET points_balance = points_balance + v_points,
        updated_at = v_now
    WHERE id = v_account_id;

    UPDATE loyalty_reservations
    SET status = 'expired',
        updated_at = v_now
    WHERE id = p_reservation_id;

    RETURN QUERY
    SELECT loyalty_reservations.id, loyalty_reservations.loyalty_account_id,
           loyalty_reservations.points_reserved, loyalty_reservations.status,
           v_now
    FROM loyalty_reservations
    WHERE loyalty_reservations.id = p_reservation_id;
END;
$$;

-- Manager approvals required to finalize (close) a register shift.
CREATE TABLE shift_approvals (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    register_shift_id UUID NOT NULL REFERENCES register_shifts(id) ON DELETE CASCADE,
    approver_id UUID NOT NULL REFERENCES auth_users(id) ON DELETE CASCADE,
    status TEXT NOT NULL DEFAULT 'pending' CHECK (status IN ('pending', 'approved', 'rejected')),
    reason TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- RLS policies will be applied to all tenant-owned tables via ONGANIZATION_ID context