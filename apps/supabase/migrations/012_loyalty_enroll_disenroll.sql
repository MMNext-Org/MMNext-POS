-- Migration 012: Loyalty enrollment/disenrollment RPCs (Task 5).
--
-- Replaces the rpc_loyalty_manage stub with real enrollment/disenrollment
-- RPCs. Enrollment is idempotent; disenrollment forfeits available points
-- through an explicit ledger entry and rejects disenrollment while a
-- redemption reservation is active. Re-enrollment starts at zero and never
-- revives forfeited points.

-- 1) Loyalty reservations: tracks active point reservations held during
--    redemption flows (expanded in Task 6). Disenrollment checks for active
--    reservations and rejects while one exists.
CREATE TABLE IF NOT EXISTS loyalty_reservations (
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

-- 2) Drop the rpc_loyalty_manage stub (migration 010).
DROP FUNCTION IF EXISTS rpc_loyalty_manage(UUID, UUID, TEXT, UUID);

-- 3) rpc_loyalty_enroll: enroll a customer in the loyalty program.
--    Idempotent: if the account is already active, returns it unchanged.
--    If the account is disenrolled/forfeited/closed, re-enrolls at zero
--    balance (forfeited points are not restored).
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
    -- Permission: staff must hold loyalty.manage in the organization.
    PERFORM require_permission(p_user_id, 'loyalty.manage', p_org_id);

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

    -- Check for an existing loyalty account for this customer/org.
    SELECT loyalty_accounts.id, loyalty_accounts.status
      INTO v_existing_id, v_existing_status
    FROM loyalty_accounts
    WHERE loyalty_accounts.customer_id = p_customer_id
      AND loyalty_accounts.organization_id = p_org_id;

    IF v_existing_id IS NOT NULL THEN
        IF v_existing_status = 'active' THEN
            -- Already enrolled: idempotent, no duplicate ledger entry.
            RETURN QUERY
            SELECT loyalty_accounts.id, loyalty_accounts.customer_id, loyalty_accounts.status,
                   loyalty_accounts.points_balance, loyalty_accounts.enrolled_at,
                   loyalty_accounts.disenrolled_at, 'already_enrolled'
            FROM loyalty_accounts WHERE id = v_existing_id;
            RETURN;
        END IF;

        -- Re-enrollment: disenrolled/forfeited/closed -> active at zero.
        -- Forfeited points are not restored; balance resets to zero.
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

    -- New enrollment: create account with zero balance.
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

-- 4) rpc_loyalty_disenroll: disenroll a customer and forfeit available points.
--    Writes a forfeiture ledger entry reducing available points to zero.
--    Rejects disenrollment while an active redemption reservation exists.
--    Idempotent: disenrolling an already-disenrolled/closed account is a no-op.
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
    -- Permission: staff must hold loyalty.manage in the organization.
    PERFORM require_permission(p_user_id, 'loyalty.manage', p_org_id);

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

    -- Find the loyalty account.
    SELECT loyalty_accounts.id, loyalty_accounts.status, loyalty_accounts.points_balance
      INTO v_account_id, v_account_status, v_points_balance
    FROM loyalty_accounts
    WHERE loyalty_accounts.customer_id = p_customer_id
      AND loyalty_accounts.organization_id = p_org_id;

    IF v_account_id IS NULL THEN
        RAISE EXCEPTION 'no loyalty account for customer % in organization %', p_customer_id, p_org_id;
    END IF;

    -- Already disenrolled/closed: idempotent, no-op.
    IF v_account_status IN ('disenrolled', 'closed') THEN
        RETURN QUERY
        SELECT loyalty_accounts.id, loyalty_accounts.customer_id, loyalty_accounts.status,
               loyalty_accounts.points_balance, 0, loyalty_accounts.disenrolled_at,
               'already_disenrolled'
        FROM loyalty_accounts WHERE id = v_account_id;
        RETURN;
    END IF;

    -- Reject disenrollment while an active redemption reservation exists.
    IF EXISTS (
        SELECT 1 FROM loyalty_reservations
        WHERE loyalty_reservations.loyalty_account_id = v_account_id
          AND loyalty_reservations.status = 'active'
    ) THEN
        RAISE EXCEPTION 'cannot disenroll customer %: active redemption reservation exists', p_customer_id;
    END IF;

    -- Forfeit available points: write a negative adjust ledger entry.
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

    -- Update account: disenrolled, zero balance.
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