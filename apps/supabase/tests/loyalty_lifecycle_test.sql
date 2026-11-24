-- pgTAP loyalty enrollment/disenrollment tests for Task 5.
-- Fixtures (apps/supabase/tests/fixtures.sql), schema, migrations 002, 010, 011, 012
-- must be loaded before this file. Each test file runs in its own psql session.
SELECT plan(30);

-- ---- Fixture UUIDs (see apps/supabase/tests/fixtures.sql) ----
-- org_a  = 11111111-1111-1111-1111-111111111001  (Tenant A)
-- org_b  = 22222222-2222-2222-2222-222222222002  (Tenant B)
-- e001   = e0000000-0000-0000-0000-000000000001  (Customer A1, org_a, 100 pts, active)
-- e002   = e0000000-0000-0000-0000-000000000002  (Customer A2, org_a, 50 pts, active)
-- e003   = e0000000-0000-0000-0000-000000000003  (Customer B1, org_b, 200 pts, active)
-- admin_a= 10000000-0000-0000-0000-000000000001  (org_a, admin, active, org-wide)
-- staff_a= 10000000-0000-0000-0000-000000000002  (org_a, staff, active, branch A-01)

-- ---- 1) Schema: loyalty_reservations table exists ----
SELECT has_table('loyalty_reservations', 'loyalty_reservations table exists');
SELECT has_column('loyalty_reservations', 'points_reserved', 'reservations.points_reserved exists');
SELECT has_column('loyalty_reservations', 'status', 'reservations.status exists');
SELECT has_column('loyalty_reservations', 'idempotency_key', 'reservations.idempotency_key exists');

-- ---- 2) Schema: new RPC functions exist ----
SELECT has_function('rpc_loyalty_enroll', 'rpc_loyalty_enroll function exists');
SELECT has_function('rpc_loyalty_disenroll', 'rpc_loyalty_disenroll function exists');

-- ---- 3) rpc_loyalty_manage stub removed ----
SELECT throws_like($$SELECT rpc_loyalty_manage(
    '10000000-0000-0000-0000-000000000001',
    '11111111-1111-1111-1111-111111111001',
    'enroll', NULL
)$$,
                   '%does not exist%',
                   'rpc_loyalty_manage stub removed');

-- ---- 4) Duplicate enrollment on active account: idempotent, no new ledger ----
SELECT * FROM rpc_loyalty_enroll(
    '10000000-0000-0000-0000-000000000001',
    'c0000000-0000-0000-0000-000000000001',
    '11111111-1111-1111-1111-111111111001',
    'v1',
    NULL
);
SELECT is(
    (SELECT action FROM (
        SELECT * FROM rpc_loyalty_enroll(
            '10000000-0000-0000-0000-000000000001',
            'c0000000-0000-0000-0000-000000000001',
            '11111111-1111-1111-1111-111111111001',
            'v1',
            NULL
        )
    ) sub),
    'already_enrolled',
    'duplicate enrollment returns already_enrolled'
);

-- ---- 5) Duplicate enrollment does not create duplicate ledger entries ----
SELECT is(
    (SELECT count(*) FROM loyalty_transactions
     WHERE loyalty_account_id = 'e0000000-0000-0000-0000-000000000001'
       AND reason = 'Duplicate enrollment ledger entry'),
    '0', 'no duplicate ledger entry from duplicate enrollment'
);

-- ---- 6) New enrollment creates account with zero balance ----
INSERT INTO customers (
    id, organization_id, name, email, loyalty_points, created_at, updated_at
) VALUES (
    'c0000000-0000-0000-0000-000000000010', '11111111-1111-1111-1111-111111111001',
    'Customer A10', 'a10@tenanta.test', 0, NOW(), NOW()
);
SELECT * FROM rpc_loyalty_enroll(
    '10000000-0000-0000-0000-000000000001',
    'c0000000-0000-0000-0000-000000000010',
    '11111111-1111-1111-1111-111111111001',
    'v1',
    NULL
);
SELECT is(
    (SELECT points_balance FROM loyalty_accounts
     WHERE customer_id = 'c0000000-0000-0000-0000-000000000010'),
    '0', 'new enrollment starts with zero balance');
SELECT is(
    (SELECT status FROM loyalty_accounts
     WHERE customer_id = 'c0000000-0000-0000-0000-000000000010'),
    'active', 'new enrollment status is active');
SELECT is(
    (SELECT consent_version FROM loyalty_accounts
     WHERE customer_id = 'c0000000-0000-0000-0000-000000000010'),
    'v1', 'consent version recorded on enrollment');

-- ---- 7) Enrollment requires loyalty.manage permission ----
SELECT throws_like($$SELECT * FROM rpc_loyalty_enroll(
    '10000000-0000-0000-0000-000000000003',
    'c0000000-0000-0000-0000-000000000002',
    '11111111-1111-1111-1111-111111111001',
    'v1', NULL
)$$,
                   '%lacks permission%',
                   'cashier lacks loyalty.manage for enrollment');

-- ---- 8) Enrollment requires active membership (revoked user) ----
SELECT throws_like($$SELECT * FROM rpc_loyalty_enroll(
    '10000000-0000-0000-0000-000000000004',
    'c0000000-0000-0000-0000-000000000002',
    '11111111-1111-1111-1111-111111111001',
    'v1', NULL
)$$,
                   '%no active membership%',
                   'revoked user cannot enroll');

-- ---- 9) Enrollment rejects customer not in org (cross-tenant customer) ----
SELECT throws_like($$SELECT * FROM rpc_loyalty_enroll(
    '10000000-0000-0000-0000-000000000001',
    'c0000000-0000-0000-0000-000000000003',
    '11111111-1111-1111-1111-111111111001',
    'v1', NULL
)$$,
                   '%does not exist in organization%',
                   'enrollment rejects customer from another org');

-- ---- 10) Cross-tenant disenroll fails ----
SELECT throws_like($$SELECT * FROM rpc_loyalty_disenroll(
    '10000000-0000-0000-0000-000000000001',
    'c0000000-0000-0000-0000-000000000003',
    '11111111-1111-1111-1111-111111111001',
    NULL
)$$,
                   '%does not exist in organization%',
                   'cross-tenant disenroll rejected');

-- ---- 11) Disenroll active account with points: forfeiture entry ----
SELECT * FROM rpc_loyalty_disenroll(
    '10000000-0000-0000-0000-000000000001',
    'c0000000-0000-0000-0000-000000000001',
    '11111111-1111-1111-1111-111111111001',
    NULL
);
SELECT is(
    (SELECT status FROM loyalty_accounts
     WHERE customer_id = 'c0000000-0000-0000-0000-000000000001'),
    'disenrolled', 'account status is disenrolled');
SELECT is(
    (SELECT points_balance FROM loyalty_accounts
     WHERE customer_id = 'c0000000-0000-0000-0000-000000000001'),
    '0', 'available points reduced to zero after disenrollment');

-- ---- 12) Forfeiture ledger entry recorded ----
SELECT is(
    (SELECT count(*) FROM loyalty_transactions
     WHERE loyalty_account_id = 'e0000000-0000-0000-0000-000000000001'
       AND type = 'adjust'
       AND points = -100
       AND balance_after = 0
       AND reason = 'Forfeited on disenrollment'),
    '1', 'forfeiture ledger entry recorded (-100 pts, balance 0)');

-- ---- 13) Disenroll already-disenrolled account: idempotent ----
SELECT * FROM rpc_loyalty_disenroll(
    '10000000-0000-0000-0000-000000000001',
    'c0000000-0000-0000-0000-000000000001',
    '11111111-1111-1111-1111-111111111001',
    NULL
);
SELECT is(
    (SELECT count(*) FROM loyalty_transactions
     WHERE loyalty_account_id = 'e0000000-0000-0000-0000-000000000001'
       AND reason = 'Forfeited on disenrollment'),
    '1', 'repeat disenrollment does not add another forfeiture entry');

-- ---- 14) Re-enroll disenrolled account: starts at zero ----
SELECT * FROM rpc_loyalty_enroll(
    '10000000-0000-0000-0000-000000000001',
    'c0000000-0000-0000-0000-000000000001',
    '11111111-1111-1111-1111-111111111001',
    'v2',
    NULL
);
SELECT is(
    (SELECT status FROM loyalty_accounts
     WHERE customer_id = 'c0000000-0000-0000-0000-000000000001'),
    'active', 're-enrolled account status is active');
SELECT is(
    (SELECT points_balance FROM loyalty_accounts
     WHERE customer_id = 'c0000000-0000-0000-0000-000000000001'),
    '0', 're-enrolled account balance is zero (forfeited points not restored)');
SELECT is(
    (SELECT consent_version FROM loyalty_accounts
     WHERE customer_id = 'c0000000-0000-0000-0000-000000000001'),
    'v2', 'consent version updated on re-enrollment');

-- ---- 15) Disenroll account with zero points: no forfeiture entry needed ----
INSERT INTO customers (
    id, organization_id, name, email, loyalty_points, created_at, updated_at
) VALUES (
    'c0000000-0000-0000-0000-000000000011', '11111111-1111-1111-1111-111111111001',
    'Customer A11', 'a11@tenanta.test', 0, NOW(), NOW()
);
SELECT * FROM rpc_loyalty_enroll(
    '10000000-0000-0000-0000-000000000001',
    'c0000000-0000-0000-0000-000000000011',
    '11111111-1111-1111-1111-111111111001',
    'v1', NULL
);
SELECT * FROM rpc_loyalty_disenroll(
    '10000000-0000-0000-0000-000000000001',
    'c0000000-0000-0000-0000-000000000011',
    '11111111-1111-1111-1111-111111111001',
    NULL
);
SELECT is(
    (SELECT count(*) FROM loyalty_transactions
     WHERE loyalty_account_id = (
         SELECT id FROM loyalty_accounts
         WHERE customer_id = 'c0000000-0000-0000-0000-000000000011'
     )
       AND type = 'adjust'
       AND reason = 'Forfeited on disenrollment'),
    '0', 'no forfeiture entry when balance is already zero');

-- ---- 16) Disenroll requires loyalty.manage permission ----
SELECT throws_like($$SELECT * FROM rpc_loyalty_disenroll(
    '10000000-0000-0000-0000-000000000003',
    'c0000000-0000-0000-0000-000000000002',
    '11111111-1111-1111-1111-111111111001',
    NULL
)$$,
                   '%lacks permission%',
                   'cashier lacks loyalty.manage for disenrollment');

-- ---- 17) Disenroll while active reservation exists is rejected ----
INSERT INTO loyalty_reservations (
    id, loyalty_account_id, organization_id, points_reserved,
    status, idempotency_key, reason, created_at, updated_at
) VALUES (
    uuid_generate_v4(), 'e0000000-0000-0000-0000-000000000002',
    '11111111-1111-1111-1111-111111111001', 25,
    'active', '11111111-1111-1111-1111-111111111801', 'test reservation',
    NOW(), NOW()
);
SELECT throws_like($$SELECT * FROM rpc_loyalty_disenroll(
    '10000000-0000-0000-0000-000000000001',
    'c0000000-0000-0000-0000-000000000002',
    '11111111-1111-1111-1111-111111111001',
    NULL
)$$,
                   '%active redemption reservation exists%',
                   'disenroll rejected while active reservation exists');

-- ---- 18) Disenroll with committed reservation does not block ----
UPDATE loyalty_reservations
SET status = 'committed'
WHERE loyalty_account_id = 'e0000000-0000-0000-0000-000000000002'
  AND idempotency_key = '11111111-1111-1111-1111-111111111801';
SELECT * FROM rpc_loyalty_disenroll(
    '10000000-0000-0000-0000-000000000001',
    'c0000000-0000-0000-0000-000000000002',
    '11111111-1111-1111-1111-111111111001',
    NULL
);
SELECT is(
    (SELECT status FROM loyalty_accounts
     WHERE customer_id = 'c0000000-0000-0000-0000-000000000002'),
    'disenrolled', 'disenroll allowed when reservation is committed');

-- ---- 19) Cross-tenant: org_a admin cannot manage org_b account ----
SELECT throws_like($$SELECT * FROM rpc_loyalty_enroll(
    '10000000-0000-0000-0000-000000000001',
    'c0000000-0000-0000-0000-000000000003',
    '22222222-2222-2222-2222-222222222002',
    'v1', NULL
)$$,
                   '%no active membership%',
                   'org_a admin cannot enroll org_b customer');

-- ---- 20) Inactive account status persists after disenroll+re-enroll cycle ----
SELECT is(
    (SELECT status FROM loyalty_accounts
     WHERE customer_id = 'c0000000-0000-0000-0000-000000000001'),
    'active', 'account is active after disenroll + re-enroll cycle');
