-- Migration 015: Retention policy tracking (privacy law compliance).
--
-- Defines per-organization data retention rules for loyalty-related
-- data types and provides an RPC to apply retention (mark records
-- past their retention window for deletion/anonymization).

-- 1) Register the customer.manage_retention permission.
INSERT INTO permissions (name, description) VALUES
  ('customer.manage_retention', 'Manage customer data retention policies within an organization')
ON CONFLICT (name) DO NOTHING;

-- Assign to admin/owner roles.
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM (
  VALUES
    ('admin',  'customer.manage_retention'),
    ('owner',  'customer.manage_retention')
) AS m(role_name, perm_name)
JOIN roles r ON r.name = m.role_name
JOIN permissions p ON p.name = m.perm_name
ON CONFLICT (role_id, permission_id) DO NOTHING;

-- 2) retention_policy table: per-organization retention rules by data type.
CREATE TABLE IF NOT EXISTS retention_policy (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    data_type TEXT NOT NULL CHECK (data_type IN ('customer_profile', 'loyalty_transactions', 'consent_history', 'loyalty_accounts')),
    retention_period_days INTEGER NOT NULL CHECK (retention_period_days > 0),
    auto_delete BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (organization_id, data_type)
);

CREATE INDEX IF NOT EXISTS idx_retention_policy_org ON retention_policy(organization_id);
CREATE INDEX IF NOT EXISTS idx_retention_policy_data_type ON retention_policy(data_type);

-- 3) rpc_set_retention_policy: set or update a retention policy for a data type.
CREATE OR REPLACE FUNCTION rpc_set_retention_policy(
    p_user_id UUID,
    p_org_id UUID,
    p_data_type TEXT,
    p_retention_period_days INTEGER,
    p_auto_delete BOOLEAN DEFAULT FALSE
)
RETURNS TABLE (
    id UUID,
    organization_id UUID,
    data_type TEXT,
    retention_period_days INTEGER,
    auto_delete BOOLEAN,
    created_at TIMESTAMPTZ,
    updated_at TIMESTAMPTZ
)
LANGUAGE plpgsql AS $$
DECLARE
    v_now TIMESTAMPTZ := NOW();
    v_existing_id UUID;
BEGIN
    -- Permission: staff must hold customer.manage_retention in the organization.
    PERFORM require_permission(p_user_id, 'customer.manage_retention', p_org_id);

    -- Validate retention period.
    IF p_retention_period_days <= 0 THEN
        RAISE EXCEPTION 'retention_period_days must be positive, got %', p_retention_period_days;
    END IF;

    -- Check for existing policy.
    SELECT id INTO v_existing_id
    FROM retention_policy
    WHERE organization_id = p_org_id AND data_type = p_data_type;

    IF v_existing_id IS NOT NULL THEN
        -- Update existing policy.
        UPDATE retention_policy
        SET retention_period_days = p_retention_period_days,
            auto_delete = p_auto_delete,
            updated_at = v_now
        WHERE id = v_existing_id;

        RETURN QUERY
        SELECT id, organization_id, data_type, retention_period_days, auto_delete,
               created_at, updated_at
        FROM retention_policy WHERE id = v_existing_id;
    ELSE
        -- Insert new policy.
        RETURN QUERY INSERT INTO retention_policy (
            organization_id, data_type, retention_period_days, auto_delete,
            created_at, updated_at
        ) VALUES (
            p_org_id, p_data_type, p_retention_period_days, p_auto_delete,
            v_now, v_now
        )
        RETURNING id, organization_id, data_type, retention_period_days, auto_delete,
               created_at, updated_at;
    END IF;
END;
$$;

-- 4) rpc_apply_retention: apply retention policy, returning records past
--    their retention window that should be deleted or anonymized.
CREATE OR REPLACE FUNCTION rpc_apply_retention(
    p_user_id UUID,
    p_org_id UUID,
    p_data_type TEXT DEFAULT NULL
)
RETURNS TABLE (
    data_type TEXT,
    record_id UUID,
    days_past_retention INTEGER,
    action TEXT
)
LANGUAGE plpgsql AS $$
DECLARE
    v_now TIMESTAMPTZ := NOW();
BEGIN
    -- Permission: staff must hold customer.manage_retention in the organization.
    PERFORM require_permission(p_user_id, 'customer.manage_retention', p_org_id);

    -- Apply retention for each matching policy.
    RETURN QUERY
    WITH policy_rows AS (
        SELECT rp.id, rp.data_type, rp.retention_period_days, rp.auto_delete
        FROM retention_policy rp
        WHERE rp.organization_id = p_org_id
          AND (p_data_type IS NULL OR rp.data_type = p_data_type)
    ),
    expired_customers AS (
        SELECT 'customer_profile' AS data_type,
               c.id AS record_id,
               (v_now - (c.updated_at + (rp.retention_period_days || ' days')::interval))::INTEGER AS days_past_retention,
               CASE WHEN rp.auto_delete THEN 'delete' ELSE 'review' END AS action
        FROM customers c
        JOIN policy_rows rp ON rp.data_type = 'customer_profile'
        WHERE c.updated_at + (rp.retention_period_days || ' days')::interval < v_now
          AND c.id NOT IN (
              SELECT customer_id FROM loyalty_accounts WHERE status = 'active'
          )
    ),
    expired_transactions AS (
        SELECT 'loyalty_transactions' AS data_type,
               lt.id AS record_id,
               (v_now - (lt.updated_at + (rp.retention_period_days || ' days')::interval))::INTEGER AS days_past_retention,
               CASE WHEN rp.auto_delete THEN 'delete' ELSE 'review' END AS action
        FROM loyalty_transactions lt
        JOIN policy_rows rp ON rp.data_type = 'loyalty_transactions'
        WHERE lt.updated_at + (rp.retention_period_days || ' days')::interval < v_now
    ),
    expired_consent AS (
        SELECT 'consent_history' AS data_type,
               ch.id AS record_id,
               (v_now - (ch.created_at + (rp.retention_period_days || ' days')::interval))::INTEGER AS days_past_retention,
               CASE WHEN rp.auto_delete THEN 'delete' ELSE 'review' END AS action
        FROM consent_history ch
        JOIN policy_rows rp ON rp.data_type = 'consent_history'
        WHERE ch.created_at + (rp.retention_period_days || ' days')::interval < v_now
    ),
    expired_accounts AS (
        SELECT 'loyalty_accounts' AS data_type,
               la.id AS record_id,
               (v_now - (la.updated_at + (rp.retention_period_days || ' days')::interval))::INTEGER AS days_past_retention,
               CASE WHEN rp.auto_delete THEN 'delete' ELSE 'review' END AS action
        FROM loyalty_accounts la
        JOIN policy_rows rp ON rp.data_type = 'loyalty_accounts'
        WHERE la.updated_at + (rp.retention_period_days || ' days')::interval < v_now
          AND la.status IN ('disenrolled', 'closed', 'forfeited')
    )
    SELECT * FROM expired_customers
    UNION ALL
    SELECT * FROM expired_transactions
    UNION ALL
    SELECT * FROM expired_consent
    UNION ALL
    SELECT * FROM expired_accounts
    ORDER BY data_type, record_id;
END;
$$;

-- 5) Row-level security on retention_policy.
ALTER TABLE retention_policy ENABLE ROW LEVEL SECURITY;

CREATE POLICY retention_policy_org_isolation ON retention_policy
  FOR ALL USING (
    organization_id = (
      SELECT ur.organization_id
      FROM user_roles ur
      JOIN auth_users au ON au.id = ur.user_id
      WHERE ur.user_id = current_setting('app.current_user_id', true)::uuid
        AND ur.status = 'active'
        AND au.is_active = TRUE
      LIMIT 1
    )
  );

ALTER TABLE retention_policy FORCE ROW LEVEL SECURITY;
