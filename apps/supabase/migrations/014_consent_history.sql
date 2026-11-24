-- Migration 014: Consent history tracking (privacy law compliance).
--
-- Records when customers give or withdraw consent for loyalty program
-- data processing, marketing, and terms of service. Provides an
-- append-only audit trail required by privacy regulations (GDPR, CCPA, etc.).

-- 1) Register the customer.manage_consent permission.
INSERT INTO permissions (name, description) VALUES
  ('customer.manage_consent', 'Record and manage customer consent for data processing within an organization')
ON CONFLICT (name) DO NOTHING;

-- Assign to admin/owner roles.
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM (
  VALUES
    ('admin',  'customer.manage_consent'),
    ('owner',  'customer.manage_consent')
) AS m(role_name, perm_name)
JOIN roles r ON r.name = m.role_name
JOIN permissions p ON p.name = m.perm_name
ON CONFLICT (role_id, permission_id) DO NOTHING;

-- 2) consent_history table: append-only audit trail of consent events.
CREATE TABLE IF NOT EXISTS consent_history (
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

CREATE INDEX IF NOT EXISTS idx_consent_history_customer ON consent_history(customer_id);
CREATE INDEX IF NOT EXISTS idx_consent_history_org ON consent_history(organization_id);
CREATE INDEX IF NOT EXISTS idx_consent_history_customer_type ON consent_history(customer_id, consent_type);
CREATE INDEX IF NOT EXISTS idx_consent_history_consented_at ON consent_history(consented_at);

-- 3) rpc_record_consent: record a consent event (give or withdraw).
CREATE OR REPLACE FUNCTION rpc_record_consent(
    p_user_id UUID,
    p_customer_id UUID,
    p_org_id UUID,
    p_consent_type TEXT,
    p_consented BOOLEAN,
    p_consent_version TEXT,
    p_ip_address TEXT DEFAULT NULL,
    p_user_agent TEXT DEFAULT NULL
)
RETURNS TABLE (
    id UUID,
    customer_id UUID,
    organization_id UUID,
    consent_type TEXT,
    consented BOOLEAN,
    consent_version TEXT,
    consented_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ
)
LANGUAGE plpgsql AS $$
DECLARE
    v_now TIMESTAMPTZ := NOW();
BEGIN
    -- Permission: staff must hold customer.manage_consent in the organization.
    PERFORM require_permission(p_user_id, 'customer.manage_consent', p_org_id);

    -- Customer must exist in the organization.
    IF NOT EXISTS (
        SELECT 1 FROM customers WHERE id = p_customer_id AND organization_id = p_org_id
    ) THEN
        RAISE EXCEPTION 'customer % does not exist in organization %', p_customer_id, p_org_id;
    END IF;

    -- Record the consent event.
    RETURN QUERY INSERT INTO consent_history (
        customer_id, organization_id, consent_type, consented,
        consent_version, consented_at, ip_address, user_agent,
        created_at, updated_at
    ) VALUES (
        p_customer_id, p_org_id, p_consent_type, p_consented,
        p_consent_version, v_now, p_ip_address, p_user_agent,
        v_now, v_now
    )
    RETURNING id, customer_id, organization_id, consent_type, consented,
           consent_version, consented_at, created_at;
END;
$$;

-- 4) Row-level security on consent_history.
ALTER TABLE consent_history ENABLE ROW LEVEL SECURITY;

CREATE POLICY consent_history_org_isolation ON consent_history
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

ALTER TABLE consent_history FORCE ROW LEVEL SECURITY;
