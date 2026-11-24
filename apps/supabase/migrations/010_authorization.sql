-- Migration 010: Scoped authorization for loyalty and customer privacy operations
--
-- Registers the scoped permission keys (loyalty.manage, loyalty.redeem,
-- customer.export, customer.erase), assigns them to roles, tracks membership
-- status, and enforces organization/branch-scoped membership + permission
-- checks in the database for the privileged RPCs. Authorization is enforced
-- in PostgreSQL, not UI-only; browser code holds no privileged secret.

-- 1) Register the scoped permission keys.
INSERT INTO permissions (name, description) VALUES
  ('loyalty.manage', 'Manage loyalty program configuration and accounts within an organization'),
  ('loyalty.redeem', 'Redeem loyalty rewards within an organization/branch'),
  ('customer.export', 'Export customer personal data within an organization'),
  ('customer.erase', 'Erase or anonymize customer personal data within an organization')
ON CONFLICT (name) DO NOTHING;

-- 2) Track membership status on user_roles (active / revoked / suspended).
ALTER TABLE user_roles
ADD COLUMN IF NOT EXISTS status TEXT NOT NULL DEFAULT 'active'
CHECK (status IN ('active', 'revoked', 'suspended'));

-- Optional branch scoping for a membership. NULL = org-wide access.
ALTER TABLE user_roles
ADD COLUMN IF NOT EXISTS branch_id UUID REFERENCES branches(id) ON DELETE SET NULL;

-- 3) Assign scoped permissions to roles (admin/owner get all; staff gets
--    loyalty.redeem only; cashier gets none of these privileged keys).
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM (
  VALUES
    ('admin',  'loyalty.manage'),
    ('admin',  'loyalty.redeem'),
    ('admin',  'customer.export'),
    ('admin',  'customer.erase'),
    ('owner',  'loyalty.manage'),
    ('owner',  'loyalty.redeem'),
    ('owner',  'customer.export'),
    ('owner',  'customer.erase'),
    ('staff',  'loyalty.redeem')
) AS m(role_name, perm_name)
JOIN roles r ON r.name = m.role_name
JOIN permissions p ON p.name = m.perm_name
ON CONFLICT (role_id, permission_id) DO NOTHING;

-- 4) Authorization functions enforced in the database.

-- True when the user has an active, non-revoked/suspended membership in the
-- organization. When p_branch_id is given, the membership must cover that
-- branch (org-wide memberships, branch_id IS NULL, cover every branch).
CREATE OR REPLACE FUNCTION has_active_membership(
    p_user_id UUID,
    p_org_id UUID,
    p_branch_id UUID DEFAULT NULL
)
RETURNS BOOLEAN
LANGUAGE plpgsql STABLE AS $$
BEGIN
  RETURN EXISTS (
    SELECT 1
    FROM user_roles ur
    JOIN auth_users au ON au.id = ur.user_id
    WHERE ur.user_id = p_user_id
      AND ur.organization_id = p_org_id
      AND ur.status = 'active'
      AND au.is_active = TRUE
      AND (p_branch_id IS NULL OR ur.branch_id IS NULL OR ur.branch_id = p_branch_id)
  );
END;
$$;

-- True when the user holds the named permission in the organization through
-- an active membership.
CREATE OR REPLACE FUNCTION user_has_permission(
    p_user_id UUID,
    p_permission TEXT,
    p_org_id UUID
)
RETURNS BOOLEAN
LANGUAGE plpgsql STABLE AS $$
BEGIN
  RETURN EXISTS (
    SELECT 1
    FROM user_roles ur
    JOIN auth_users au ON au.id = ur.user_id
    JOIN role_permissions rp ON rp.role_id = ur.role_id
    JOIN permissions perm ON perm.id = rp.permission_id
    WHERE ur.user_id = p_user_id
      AND ur.organization_id = p_org_id
      AND ur.status = 'active'
      AND au.is_active = TRUE
      AND perm.name = p_permission
  );
END;
$$;

-- Require the named permission for the user in the organization; raise if
-- the user has no active membership or lacks the permission.
CREATE OR REPLACE FUNCTION require_permission(
    p_user_id UUID,
    p_permission TEXT,
    p_org_id UUID
)
RETURNS VOID
LANGUAGE plpgsql STABLE AS $$
BEGIN
  IF NOT user_has_permission(p_user_id, p_permission, p_org_id) THEN
    IF NOT has_active_membership(p_user_id, p_org_id) THEN
      RAISE EXCEPTION 'user % has no active membership in organization %', p_user_id, p_org_id;
    ELSE
      RAISE EXCEPTION 'user % lacks permission % in organization %', p_user_id, p_permission, p_org_id;
    END IF;
  END IF;
END;
$$;

-- 5) Privileged RPC stubs. Each enforces organization/branch-scoped
--    membership and permission before doing any work. The real loyalty and
--    privacy implementations (Tasks 3-9) will replace these bodies.
CREATE OR REPLACE FUNCTION rpc_loyalty_manage(
    p_user_id UUID,
    p_org_id UUID,
    p_action TEXT,
    p_branch_id UUID DEFAULT NULL
)
RETURNS TEXT
LANGUAGE plpgsql STABLE AS $$
BEGIN
  PERFORM require_permission(p_user_id, 'loyalty.manage', p_org_id);
  IF p_branch_id IS NOT NULL AND NOT has_active_membership(p_user_id, p_org_id, p_branch_id) THEN
    RAISE EXCEPTION 'user % has no active membership in organization % branch %', p_user_id, p_org_id, p_branch_id;
  END IF;
  RETURN format('loyalty.manage executed for org %s branch %s action %s', p_org_id, COALESCE(p_branch_id::text, 'org-wide'), p_action);
END;
$$;

CREATE OR REPLACE FUNCTION rpc_loyalty_redeem(
    p_user_id UUID,
    p_org_id UUID,
    p_points INTEGER,
    p_branch_id UUID DEFAULT NULL
)
RETURNS TEXT
LANGUAGE plpgsql STABLE AS $$
BEGIN
  PERFORM require_permission(p_user_id, 'loyalty.redeem', p_org_id);
  IF p_branch_id IS NOT NULL AND NOT has_active_membership(p_user_id, p_org_id, p_branch_id) THEN
    RAISE EXCEPTION 'user % has no active membership in organization % branch %', p_user_id, p_org_id, p_branch_id;
  END IF;
  RETURN format('loyalty.redeem executed for org %s branch %s points %s', p_org_id, COALESCE(p_branch_id::text, 'org-wide'), p_points);
END;
$$;

CREATE OR REPLACE FUNCTION rpc_customer_export(
    p_user_id UUID,
    p_org_id UUID,
    p_branch_id UUID DEFAULT NULL
)
RETURNS TEXT
LANGUAGE plpgsql STABLE AS $$
BEGIN
  PERFORM require_permission(p_user_id, 'customer.export', p_org_id);
  IF p_branch_id IS NOT NULL AND NOT has_active_membership(p_user_id, p_org_id, p_branch_id) THEN
    RAISE EXCEPTION 'user % has no active membership in organization % branch %', p_user_id, p_org_id, p_branch_id;
  END IF;
  RETURN format('customer.export executed for org %s branch %s', p_org_id, COALESCE(p_branch_id::text, 'org-wide'));
END;
$$;

CREATE OR REPLACE FUNCTION rpc_customer_erase(
    p_user_id UUID,
    p_org_id UUID,
    p_branch_id UUID DEFAULT NULL
)
RETURNS TEXT
LANGUAGE plpgsql STABLE AS $$
BEGIN
  PERFORM require_permission(p_user_id, 'customer.erase', p_org_id);
  IF p_branch_id IS NOT NULL AND NOT has_active_membership(p_user_id, p_org_id, p_branch_id) THEN
    RAISE EXCEPTION 'user % has no active membership in organization % branch %', p_user_id, p_org_id, p_branch_id;
  END IF;
  RETURN format('customer.erase executed for org %s branch %s', p_org_id, COALESCE(p_branch_id::text, 'org-wide'));
END;
$$;

-- 6) Row-level security: tenant-owned tables are isolated to the authenticated
--    user's organization. The application server sets app.current_user_id
--    (from the authenticated session) before issuing queries; browser code
--    never sets it and holds no privileged secret.
ALTER TABLE customers ENABLE ROW LEVEL SECURITY;
ALTER TABLE loyalty_accounts ENABLE ROW LEVEL SECURITY;
ALTER TABLE loyalty_transactions ENABLE ROW LEVEL SECURITY;
ALTER TABLE orders ENABLE ROW LEVEL SECURITY;

CREATE POLICY customers_org_isolation ON customers
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

CREATE POLICY loyalty_accounts_org_isolation ON loyalty_accounts
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

CREATE POLICY loyalty_transactions_org_isolation ON loyalty_transactions
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

CREATE POLICY orders_org_isolation ON orders
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

-- Force RLS to apply to the table owner as well (defense in depth).
ALTER TABLE customers FORCE ROW LEVEL SECURITY;
ALTER TABLE loyalty_accounts FORCE ROW LEVEL SECURITY;
ALTER TABLE loyalty_transactions FORCE ROW LEVEL SECURITY;
ALTER TABLE orders FORCE ROW LEVEL SECURITY;
