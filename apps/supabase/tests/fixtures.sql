-- Deterministic fixtures for two-organization, two-branch authorization testing.
-- All identifiers are fixed UUIDs so tests can reference them directly.
-- Organizations: org_a (Tenant A), org_b (Tenant B).

-- Clean slate (idempotent within a fresh test database).
TRUNCATE TABLE loyalty_transactions, loyalty_accounts, customers, orders,
         user_roles, role_permissions, roles, auth_users,
         branches, organizations RESTART IDENTITY CASCADE;

-- Organizations
INSERT INTO organizations (id, name, created_at, updated_at) VALUES
  ('11111111-1111-1111-1111-111111111001', 'Tenant A', NOW(), NOW()),
  ('22222222-2222-2222-2222-222222222002', 'Tenant B', NOW(), NOW());

-- Branches (two per organization)
INSERT INTO branches (id, organization_id, branch_code, name, created_at, updated_at) VALUES
  ('11111111-1111-1111-1111-111111111101', '11111111-1111-1111-1111-111111111001', 'A-01', 'Tenant A Branch 1', NOW(), NOW()),
  ('11111111-1111-1111-1111-111111111102', '11111111-1111-1111-1111-111111111001', 'A-02', 'Tenant A Branch 2', NOW(), NOW()),
  ('22222222-2222-2222-2222-222222222101', '22222222-2222-2222-2222-222222222002', 'B-01', 'Tenant B Branch 1', NOW(), NOW()),
  ('22222222-2222-2222-2222-222222222102', '22222222-2222-2222-2222-222222222002', 'B-02', 'Tenant B Branch 2', NOW(), NOW());

-- Roles (valid 32-hex UUIDs).
INSERT INTO roles (id, name, description, created_at, updated_at) VALUES
  ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'admin', 'Full access', NOW(), NOW()),
  ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'owner', 'Tenant settings', NOW(), NOW()),
  ('cccccccc-cccc-cccc-cccc-cccccccccccc', 'staff', 'Operations', NOW(), NOW()),
  ('dddddddd-dddd-dddd-dddd-dddddddddddd', 'cashier', 'Payments only', NOW(), NOW());

-- Permission keys are created by migration 010 (ids auto-generated). Fixtures
-- link roles to permissions by name below.
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

-- Auth users (one inactive to test revoked/suspended membership).
INSERT INTO auth_users (id, email, password_hash, organization_id, is_active, created_at, updated_at) VALUES
  ('10000000-0000-0000-0000-000000000001', 'admin.a@tenanta.test', 'x', '11111111-1111-1111-1111-111111111001', TRUE, NOW(), NOW()),
  ('10000000-0000-0000-0000-000000000002', 'staff.a@tenanta.test', 'x', '11111111-1111-1111-1111-111111111001', TRUE, NOW(), NOW()),
  ('10000000-0000-0000-0000-000000000003', 'cashier.a@tenanta.test', 'x', '11111111-1111-1111-1111-111111111001', TRUE, NOW(), NOW()),
  ('10000000-0000-0000-0000-000000000004', 'revoked.a@tenanta.test', 'x', '11111111-1111-1111-1111-111111111001', TRUE, NOW(), NOW()),
  ('10000000-0000-0000-0000-000000000005', 'suspended.a@tenanta.test', 'x', '11111111-1111-1111-1111-111111111001', TRUE, NOW(), NOW()),
  ('10000000-0000-0000-0000-000000000006', 'inactive.a@tenanta.test', 'x', '11111111-1111-1111-1111-111111111001', FALSE, NOW(), NOW()),
  ('20000000-0000-0000-0000-000000000001', 'admin.b@tenantb.test', 'x', '22222222-2222-2222-2222-222222222002', TRUE, NOW(), NOW());

-- Memberships (status on user_roles; branch_id NULL = org-wide access).
INSERT INTO user_roles (user_id, role_id, organization_id, branch_id, status) VALUES
  ('10000000-0000-0000-0000-000000000001', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', '11111111-1111-1111-1111-111111111001', NULL, 'active'),
  ('10000000-0000-0000-0000-000000000002', 'cccccccc-cccc-cccc-cccc-cccccccccccc', '11111111-1111-1111-1111-111111111001', '11111111-1111-1111-1111-111111111101', 'active'),
  ('10000000-0000-0000-0000-000000000003', 'dddddddd-dddd-dddd-dddd-dddddddddddd', '11111111-1111-1111-1111-111111111001', NULL, 'active'),
  ('10000000-0000-0000-0000-000000000004', 'cccccccc-cccc-cccc-cccc-cccccccccccc', '11111111-1111-1111-1111-111111111001', '11111111-1111-1111-1111-111111111101', 'revoked'),
  ('10000000-0000-0000-0000-000000000005', 'cccccccc-cccc-cccc-cccc-cccccccccccc', '11111111-1111-1111-1111-111111111101', '11111111-1111-1111-1111-111111111101', 'suspended'),
  ('10000000-0000-0000-0000-000000000006', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', '11111111-1111-1111-1111-111111111001', NULL, 'active'),
  ('20000000-0000-0000-0000-000000000001', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', '22222222-2222-2222-2222-222222222002', NULL, 'active');

-- Customers (one organization each)
INSERT INTO customers (id, organization_id, name, email, loyalty_points, created_at, updated_at) VALUES
  ('c0000000-0000-0000-0000-000000000001', '11111111-1111-1111-1111-111111111001', 'Customer A1', 'a1@tenanta.test', 100, NOW(), NOW()),
  ('c0000000-0000-0000-0000-000000000002', '11111111-1111-1111-1111-111111111001', 'Customer A2', 'a2@tenanta.test', 50, NOW(), NOW()),
  ('c0000000-0000-0000-0000-000000000003', '22222222-2222-2222-2222-222222222002', 'Customer B1', 'b1@tenantb.test', 200, NOW(), NOW());

-- Loyalty accounts (canonical balance projection)
INSERT INTO loyalty_accounts (id, customer_id, organization_id, points_balance, tier, created_at, updated_at) VALUES
  ('e0000000-0000-0000-0000-000000000001', 'c0000000-0000-0000-0000-000000000001', '11111111-1111-1111-1111-111111111001', 100, 'bronze', NOW(), NOW()),
  ('e0000000-0000-0000-0000-000000000002', 'c0000000-0000-0000-0000-000000000002', '11111111-1111-1111-1111-111111111001', 50, 'bronze', NOW(), NOW()),
  ('e0000000-0000-0000-0000-000000000003', 'c0000000-0000-0000-0000-000000000003', '22222222-2222-2222-2222-222222222002', 200, 'bronze', NOW(), NOW());

-- Non-superuser role for RLS testing (superusers bypass RLS).
DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'app_user') THEN
    CREATE ROLE app_user;
  END IF;
END
$$;
GRANT SELECT ON customers TO app_user;
GRANT SELECT ON user_roles TO app_user;
GRANT SELECT ON auth_users TO app_user;