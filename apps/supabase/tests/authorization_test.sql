-- pgTAP authorization tests for migration 010.
-- Two organizations (Tenant A, Tenant B), two branches each.
-- Fixtures (apps/supabase/tests/fixtures.sql) must be loaded before this file.
SELECT plan(31);

-- ---- Fixture UUIDs (see apps/supabase/tests/fixtures.sql) ----
-- org_a  = 11111111-1111-1111-1111-111111111001  (Tenant A)
-- org_b  = 22222222-2222-2222-2222-222222222002  (Tenant B)
-- branch_a1 = 11111111-1111-1111-1111-111111111101
-- admin_a = 10000000-0000-0000-0000-000000000001 (admin, org A, active)
-- staff_a   = 10000000-0000-0000-0000-000000000002 (staff, org A branch_a1, active)
-- cashier_a = 10000000-0000-0000-0000-000000000003 (cashier, org A, active)
-- revoked_a = 10000000-0000-0000-0000-000000000004 (staff, org A, revoked)
-- suspended_a = 10000000-0000-0000-0000-000000000005 (staff, org A, suspended)
-- inactive_a  = 10000000-0000-0000-0000-000000000006 (admin, org A, auth disabled)
-- admin_b   = 20000000-0000-0000-0000-000000000001 (admin, org B, active)

-- ---- 1) has_active_membership ----
SELECT ok(has_active_membership('10000000-0000-0000-0000-000000000001',
                                '11111111-1111-1111-1111-111111111001'),
          'admin_a has active membership in org_a');
SELECT ok(has_active_membership('10000000-0000-0000-0000-000000000002',
                                '11111111-1111-1111-1111-111111111001'),
          'staff_a has active membership in org_a');
SELECT ok(NOT has_active_membership('10000000-0000-0000-0000-000000000004',
                                    '11111111-1111-1111-1111-111111111001'),
          'revoked_a has NO active membership in org_a');
SELECT ok(NOT has_active_membership('10000000-0000-0000-0000-000000000005',
                                    '11111111-1111-1111-1111-111111111001'),
          'suspended_a has NO active membership in org_a');
SELECT ok(NOT has_active_membership('10000000-0000-0000-0000-000000000006',
                                    '11111111-1111-1111-1111-111111111001'),
          'inactive_a (auth disabled) has NO active membership in org_a');
SELECT ok(has_active_membership('20000000-0000-0000-0000-000000000001',
                                '22222222-2222-2222-2222-222222222002'),
          'admin_b has active membership in org_b');
SELECT ok(NOT has_active_membership('10000000-0000-0000-0000-000000000001',
                                    '22222222-2222-2222-2222-222222222002'),
          'admin_a has NO membership in org_b (cross-tenant denied)');
SELECT ok(has_active_membership('10000000-0000-0000-0000-000000000002',
                                '11111111-1111-1111-1111-111111111001',
                                '11111111-1111-1111-1111-111111111101'),
          'staff_a has active membership in org_a branch_a1');
SELECT ok(NOT has_active_membership('10000000-0000-0000-0000-000000000002',
                                    '11111111-1111-1111-1111-111111111001',
                                    '22222222-2222-2222-2222-222222222101'),
          'staff_a has NO membership in org_b branch (cross-tenant branch denied)');

-- ---- 2) user_has_permission ----
SELECT ok(user_has_permission('10000000-0000-0000-0000-000000000001',
                              'loyalty.manage',
                              '11111111-1111-1111-1111-111111111001'),
          'admin_a has loyalty.manage in org_a');
SELECT ok(user_has_permission('10000000-0000-0000-0000-000000000001',
                              'loyalty.redeem',
                              '11111111-1111-1111-1111-111111111001'),
          'admin_a has loyalty.redeem in org_a');
SELECT ok(user_has_permission('10000000-0000-0000-0000-000000000001',
                              'customer.export',
                              '11111111-1111-1111-1111-111111111001'),
          'admin_a has customer.export in org_a');
SELECT ok(user_has_permission('10000000-0000-0000-0000-000000000001',
                              'customer.erase',
                              '11111111-1111-1111-1111-111111111001'),
          'admin_a has customer.erase in org_a');
SELECT ok(NOT user_has_permission('10000000-0000-0000-0000-000000000003',
                                  'loyalty.manage',
                                  '11111111-1111-1111-1111-111111111001'),
          'cashier_a lacks loyalty.manage in org_a');
SELECT ok(user_has_permission('10000000-0000-0000-0000-000000000002',
                              'loyalty.redeem',
                              '11111111-1111-1111-1111-111111111001'),
          'staff_a has loyalty.redeem in org_a');
SELECT ok(NOT user_has_permission('10000000-0000-0000-0000-000000000002',
                                  'customer.erase',
                                  '11111111-1111-1111-1111-111111111001'),
          'staff_a lacks customer.erase in org_a');

-- ---- 3) require_permission: allowed (positive) ----
SELECT require_permission('10000000-0000-0000-0000-000000000001',
                          'loyalty.manage',
                          '11111111-1111-1111-1111-111111111001');
SELECT ok(true, 'admin_a require_permission loyalty.manage org_a allowed');
SELECT require_permission('10000000-0000-0000-0000-000000000002',
                          'loyalty.redeem',
                          '11111111-1111-1111-1111-111111111001');
SELECT ok(true, 'staff_a require_permission loyalty.redeem org_a allowed');

-- ---- 4) require_permission: denied (negative) ----
SELECT throws_like($$SELECT require_permission('10000000-0000-0000-0000-000000000003',
                                               'loyalty.manage',
                                               '11111111-1111-1111-1111-111111111001')$$,
                   '%lacks permission%',
                   'cashier_a require_permission loyalty.manage denied');
SELECT throws_like($$SELECT require_permission('10000000-0000-0000-0000-000000000004',
                                               'loyalty.redeem',
                                               '11111111-1111-1111-1111-111111111001')$$,
                   '%no active membership%',
                   'revoked_a require_permission loyalty.redeem denied');
SELECT throws_like($$SELECT require_permission('10000000-0000-0000-0000-000000000001',
                                               'customer.erase',
                                               '22222222-2222-2222-2222-222222222002')$$,
                   '%no active membership%',
                   'admin_a require_permission customer.erase org_b denied (cross-tenant)');
SELECT throws_like($$SELECT require_permission('10000000-0000-0000-0000-000000000002',
                                               'customer.export',
                                               '11111111-1111-1111-1111-111111111001')$$,
                   '%lacks permission%',
                   'staff_a require_permission customer.export denied');

-- ---- 5) RLS organization isolation (app.current_user_id set by server) ----
-- Run as non-superuser app_user (superusers bypass RLS).
SET ROLE app_user;
SET app.current_user_id = '10000000-0000-0000-0000-000000000001';
SELECT is((SELECT count(*) FROM customers)::text, '2',
          'admin_a sees 2 org_a customers (RLS)');
SELECT is((SELECT count(*) FROM customers
           WHERE organization_id = '22222222-2222-2222-2222-222222222002')::text,
          '0', 'admin_a sees 0 org_b customers (cross-tenant RLS denied)');

SET app.current_user_id = '20000000-0000-0000-0000-000000000001';
SELECT is((SELECT count(*) FROM customers)::text, '1',
          'admin_b sees 1 org_b customer (RLS)');

SET app.current_user_id = '10000000-0000-0000-0000-000000000004';
SELECT is((SELECT count(*) FROM customers)::text, '0',
          'revoked_a sees 0 customers (revoked membership RLS denied)');
RESET ROLE;

-- ---- 6) Enrollment/disenrollment RPCs enforce membership + permission ----
SELECT ok(
    (SELECT count(*) FROM rpc_loyalty_enroll(
        '10000000-0000-0000-0000-000000000001',
        'c0000000-0000-0000-0000-000000000001',
        '11111111-1111-1111-1111-111111111001',
        'v1', NULL)) = 1,
    'admin_a rpc_loyalty_enroll org_a allowed');
SELECT throws_like($$SELECT * FROM rpc_loyalty_enroll(
    '10000000-0000-0000-0000-000000000003',
    'c0000000-0000-0000-0000-000000000002',
    '11111111-1111-1111-1111-111111111001',
    'v1', NULL)$$,
                   '%lacks permission%',
                   'cashier_a rpc_loyalty_enroll denied (no permission)');
SELECT throws_like($$SELECT * FROM rpc_loyalty_redeem_quote(
    '10000000-0000-0000-0000-000000000003',
    'c0000000-0000-0000-0000-000000000001',
    '11111111-1111-1111-1111-111111111001',
    30,
    'a0000000-0000-0000-0000-000000000001',
    NULL)$$, '%lacks permission%',
                   'cashier_a rpc_loyalty_redeem_quote denied (no loyalty.redeem)');
SELECT throws_like($$SELECT * FROM rpc_loyalty_redeem_reserve(
    '10000000-0000-0000-0000-000000000003',
    'c0000000-0000-0000-0000-000000000001',
    '11111111-1111-1111-1111-111111111001',
    30,
    'a0000000-0000-0000-0000-000000000001',
    NULL,
    '11111111-1111-1111-1111-111111111909')$$, '%lacks permission%',
                   'cashier_a rpc_loyalty_redeem_reserve denied (no loyalty.redeem)');
SELECT throws_like($$SELECT * FROM rpc_loyalty_enroll(
    '10000000-0000-0000-0000-000000000001',
    'c0000000-0000-0000-0000-000000000003',
    '11111111-1111-1111-1111-111111111001',
    'v1', NULL)$$,
                   '%does not exist in organization%',
                   'admin_a rpc_loyalty_enroll org_b customer denied');

