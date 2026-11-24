-- Migration 013: Loyalty redemption RPCs (Task 6).
--
-- Replaces the rpc_loyalty_redeem stub (migration 010) with a full
-- quote/reserve/commit/release/expire redemption workflow.
--
-- Flow:
--   quote    - read-only eligibility + cost quote (no state change)
--   reserve  - FOR UPDATE lock on account, deduct points, create active
--              reservation (idempotent via idempotency_key)
--   commit   - FOR UPDATE lock on reservation + account, write redeem
--              ledger entry, mark reservation committed
--   release  - FOR UPDATE lock on reservation + account, refund points
--              (refund reversal), mark reservation released
--   expire   - FOR UPDATE lock on reservation + account, refund points,
--              mark reservation expired

-- 1) Drop the rpc_loyalty_redeem stub (migration 010).
DROP FUNCTION IF EXISTS rpc_loyalty_redeem(UUID, UUID, INTEGER, UUID);

-- 2) rpc_loyalty_redeem_quote: read-only eligibility + cost quote.
--    Returns reward details and projected balance_after. No state change.
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
    -- Permission: user must hold loyalty.redeem in the organization.
    PERFORM require_permission(p_user_id, 'loyalty.redeem', p_org_id);

    -- Branch-scoped membership check.
    IF p_branch_id IS NOT NULL AND NOT has_active_membership(p_user_id, p_org_id, p_branch_id) THEN
        RAISE EXCEPTION 'user % has no active membership in organization % branch %', p_user_id, p_org_id, p_branch_id;
    END IF;

    -- Customer must exist in the organization.
    IF NOT EXISTS (
        SELECT 1 FROM customers WHERE id = p_customer_id AND organization_id = p_org_id
    ) THEN
        RAISE EXCEPTION 'customer % does not exist in organization %', p_customer_id, p_org_id;
    END IF;

    -- Reward must exist, be active, and be within its validity window.
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

    -- Points must be positive.
    IF p_points <= 0 THEN
        RAISE EXCEPTION 'points must be positive, got %', p_points;
    END IF;

    -- Reward point cost must match requested points.
    SELECT r.point_cost INTO v_balance
    FROM loyalty_rewards r WHERE r.id = p_reward_id;
    IF v_balance IS NULL OR v_balance <> p_points THEN
        RAISE EXCEPTION 'reward % point cost (%) does not match requested points (%)', p_reward_id, v_balance, p_points;
    END IF;

    -- Account must exist and have sufficient balance.
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

-- 3) rpc_loyalty_redeem_reserve: lock points and create an active reservation.
--    Idempotent: same idempotency_key returns the existing reservation.
--    Row locking: FOR UPDATE on the loyalty_accounts row prevents concurrent
--    over-reservation. Points are deducted from balance immediately and
--    refunded on release/expire.
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
    -- Permission: user must hold loyalty.redeem in the organization.
    PERFORM require_permission(p_user_id, 'loyalty.redeem', p_org_id);

    -- Branch-scoped membership check.
    IF p_branch_id IS NOT NULL AND NOT has_active_membership(p_user_id, p_org_id, p_branch_id) THEN
        RAISE EXCEPTION 'user % has no active membership in organization % branch %', p_user_id, p_org_id, p_branch_id;
    END IF;

    -- Customer must exist in the organization.
    IF NOT EXISTS (
        SELECT 1 FROM customers WHERE id = p_customer_id AND organization_id = p_org_id
    ) THEN
        RAISE EXCEPTION 'customer % does not exist in organization %', p_customer_id, p_org_id;
    END IF;

    -- Points must be positive.
    IF p_points <= 0 THEN
        RAISE EXCEPTION 'points must be positive, got %', p_points;
    END IF;

    -- Idempotency key must be provided.
    IF p_idempotency_key IS NULL THEN
        RAISE EXCEPTION 'idempotency_key is required';
    END IF;

    -- Reward must exist, be active, and match requested points.
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

    -- Check sufficient balance.
    IF v_balance < p_points THEN
        RAISE EXCEPTION 'insufficient points: have %, need %', v_balance, p_points;
    END IF;

    -- Check no existing active reservation (defensive; partial index also enforces).
    IF EXISTS (
        SELECT 1 FROM loyalty_reservations
        WHERE loyalty_reservations.loyalty_account_id = v_account_id
          AND loyalty_reservations.status = 'active'
    ) THEN
        RAISE EXCEPTION 'active reservation already exists for account %', v_account_id;
    END IF;

    -- Deduct points from balance (refunded on release/expire).
    UPDATE loyalty_accounts
    SET points_balance = points_balance - p_points,
        updated_at = v_now
    WHERE id = v_account_id;

    -- Create the active reservation.
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

-- 4) rpc_loyalty_redeem_commit: finalize an active reservation.
--    Writes a redeem ledger entry and marks the reservation committed.
--    Points were already deducted at reserve time; balance_after reflects
--    the post-deduction balance.
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
    -- Lock the reservation row.
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

    -- Permission: user must hold loyalty.redeem in the reservation's organization.
    PERFORM require_permission(p_user_id, 'loyalty.redeem', v_org_id);

    -- Lock the account row and fetch customer_id for the ledger entry.
    SELECT loyalty_accounts.id, loyalty_accounts.points_balance, loyalty_accounts.customer_id
      INTO v_account_id, v_balance, v_customer_id
    FROM loyalty_accounts
    WHERE loyalty_accounts.id = v_account_id
    FOR UPDATE;

    -- Write the redeem ledger entry. Balance was already deducted at reserve,
    -- so balance_after is the current (post-deduction) balance.
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

    -- Mark the reservation committed.
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

-- 5) rpc_loyalty_redeem_release: release an active reservation and refund
--    the reserved points back to the account balance (refund reversal).
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
    -- Lock the reservation row.
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

    -- Permission: user must hold loyalty.redeem in the reservation's organization.
    PERFORM require_permission(p_user_id, 'loyalty.redeem', v_org_id);

    -- Lock the account row and refund the reserved points.
    UPDATE loyalty_accounts
    SET points_balance = points_balance + v_points,
        updated_at = v_now
    WHERE id = v_account_id;

    -- Mark the reservation released.
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

-- 6) rpc_loyalty_redeem_expire: expire an active reservation and refund
--    the reserved points back to the account balance (refund reversal).
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
    -- Lock the reservation row.
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

    -- Permission: user must hold loyalty.redeem in the reservation's organization.
    PERFORM require_permission(p_user_id, 'loyalty.redeem', v_org_id);

    -- Lock the account row and refund the reserved points.
    UPDATE loyalty_accounts
    SET points_balance = points_balance + v_points,
        updated_at = v_now
    WHERE id = v_account_id;

    -- Mark the reservation expired.
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