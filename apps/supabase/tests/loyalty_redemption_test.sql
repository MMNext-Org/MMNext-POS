-- pgTAP loyalty redemption tests for Task 6.
-- Fixtures (apps/supabase/tests/fixtures.sql), schema, migrations 002, 010,
-- 011, 012, 013 must be loaded before this file. Each test file runs in its
-- own psql session (see harness).
SELECT plan(32);

-- Reset shared state mutated by earlier suites (earn/lifecycle run against the
-- same database; fixtures are loaded once, so e001/e002 may have been altered).
UPDATE loyalty_accounts
SET status='active', points_balance=100, disenrolled_at=NULL, version=1, updated_at=NOW()
WHERE id='e0000000-0000-0000-0000-000000000001';
UPDATE loyalty_accounts
SET status='active', points_balance=50, disenrolled_at=NULL, version=1, updated_at=NOW()
WHERE id='e0000000-0000-0000-0000-000000000002';
DELETE FROM loyalty_reservations
WHERE loyalty_account_id IN (
    'e0000000-0000-0000-0000-000000000001',
    'e0000000-0000-0000-0000-000000000002'
);
DELETE FROM loyalty_transactions
WHERE loyalty_account_id IN (
    'e0000000-0000-0000-0000-000000000001',
    'e0000000-0000-0000-0000-000000000002'
);

-- ---- Fixture UUIDs (see apps/supabase/tests/fixtures.sql) ----
-- org_a  = 11111111-1111-1111-1111-111111111001  (Tenant A)
-- org_b  = 22222222-2222-2222-2222-222222222002  (Tenant B)
-- e001   = e0000000-0000-0000-0000-000000000001  (Customer A1, org_a, 100 pts)
-- e002   = e0000000-0000-0000-0000-000000000002  (Customer A2, org_a, 50 pts)
-- e003   = e0000000-0000-0000-0000-000000000003  (Customer B1, org_b, 200 pts)
-- admin_a= 10000000-0000-0000-0000-000000000001  (org_a, admin, loyalty.redeem)
-- staff_a= 10000000-0000-0000-0000-000000000002  (org_a, staff, loyalty.redeem, branch A-01)
-- cashier= 10000000-0000-0000-0000-000000000003  (org_a, cashier, NO loyalty.redeem)

-- Reward used by tests (org_a, 30 points). Uses a distinct name/version so it
-- does not collide with rewards inserted by loyalty_earn_test.sql, which runs
-- earlier against the same database (fixtures are loaded once, then each suite
-- runs in its own session against the shared DB).
INSERT INTO loyalty_rewards (
    id, organization_id, name, description, point_cost,
    reward_value_minor_units, reward_type, is_active,
    active_from, active_until, version, created_at, updated_at
) VALUES (
    'a0000000-0000-0000-0000-000000000001',
    '11111111-1111-1111-1111-111111111001',
    'Free Muffin', '30-point reward', 30,
    3000, 'discount', TRUE,
    NOW(), NULL, 1, NOW(), NOW()
);

-- Idempotency keys for reserve tests.
-- k1: reserve+commit (A1, 30 pts)
-- k2: reserve+release (A2, 30 pts)
-- k3: reserve+expire (A2, 30 pts)

-- ---- 1-7) Schema: redemption RPCs exist ----
SELECT has_function('rpc_loyalty_redeem_quote', 'rpc_loyalty_redeem_quote exists');
SELECT has_function('rpc_loyalty_redeem_reserve', 'rpc_loyalty_redeem_reserve exists');
SELECT has_function('rpc_loyalty_redeem_commit', 'rpc_loyalty_redeem_commit exists');
SELECT has_function('rpc_loyalty_redeem_release', 'rpc_loyalty_redeem_release exists');
SELECT has_function('rpc_loyalty_redeem_expire', 'rpc_loyalty_redeem_expire exists');

-- ---- 8) Stub removed: rpc_loyalty_redeem(UUID, UUID, INTEGER, UUID) ----
SELECT throws_like($$SELECT rpc_loyalty_redeem(
    '10000000-0000-0000-0000-000000000001',
    '11111111-1111-1111-1111-111111111001',
    30, NULL
)$$,
                   '%does not exist%',
                   'rpc_loyalty_redeem stub removed');

-- ---- 9) Quote: valid quote returns reward info + projected balance ----
SELECT * FROM rpc_loyalty_redeem_quote(
    '10000000-0000-0000-0000-000000000001',
    'c0000000-0000-0000-0000-000000000001',
    '11111111-1111-1111-1111-111111111001',
    30,
    'a0000000-0000-0000-0000-000000000001',
    NULL
);
SELECT is(
    (SELECT reward_name FROM (
        SELECT * FROM rpc_loyalty_redeem_quote(
            '10000000-0000-0000-0000-000000000001',
            'c0000000-0000-0000-0000-000000000001',
            '11111111-1111-1111-1111-111111111001',
            30,
            'a0000000-0000-0000-0000-000000000001',
            NULL
        ) q
    ) sub),
    'Free Muffin',
    'quote returns reward name'
);
SELECT is(
    (SELECT balance_after FROM (
        SELECT * FROM rpc_loyalty_redeem_quote(
            '10000000-0000-0000-0000-000000000001',
            'c0000000-0000-0000-0000-000000000001',
            '11111111-1111-1111-1111-111111111001',
            30,
            'a0000000-0000-0000-0000-000000000001',
            NULL
        ) q
    ) sub),
    '70',
    'quote balance_after = 100 - 30 = 70'
);

-- ---- 10) Quote: points/reward cost mismatch rejected ----
-- Requesting 200 pts for the 30-pt "Free Muffin" reward: cost-match is
-- validated before balance, so this raises a cost-mismatch error (not
-- "insufficient points").
SELECT throws_like($$SELECT * FROM rpc_loyalty_redeem_quote(
    '10000000-0000-0000-0000-000000000001',
    'c0000000-0000-0000-0000-000000000001',
    '11111111-1111-1111-1111-111111111001',
    200,
    'a0000000-0000-0000-0000-000000000001',
    NULL
)$$,
                   '%point cost% does not match requested points%',
                   'quote rejects points/reward cost mismatch');

-- ---- 11) Quote: inactive reward rejected ----
INSERT INTO loyalty_rewards (
    id, organization_id, name, point_cost, reward_type, is_active,
    active_from, version, created_at, updated_at
) VALUES (
    'a0000000-0000-0000-0000-000000000002',
    '11111111-1111-1111-1111-111111111001',
    'Expired Reward', 10, 'discount', FALSE,
    NOW(), 1, NOW(), NOW()
);
SELECT throws_like($$SELECT * FROM rpc_loyalty_redeem_quote(
    '10000000-0000-0000-0000-000000000001',
    'c0000000-0000-0000-0000-000000000001',
    '11111111-1111-1111-1111-111111111001',
    10,
    'a0000000-0000-0000-0000-000000000002',
    NULL
)$$,
                   '%is not active in organization%',
                   'quote rejects inactive reward');

-- ---- 12) Quote: permission denied (cashier lacks loyalty.redeem) ----
SELECT throws_like($$SELECT * FROM rpc_loyalty_redeem_quote(
    '10000000-0000-0000-0000-000000000003',
    'c0000000-0000-0000-0000-000000000001',
    '11111111-1111-1111-1111-111111111001',
    30,
    'a0000000-0000-0000-0000-000000000001',
    NULL
)$$,
                   '%lacks permission%',
                   'cashier quote denied (no loyalty.redeem)');

-- ---- 13) Reserve: valid reserve creates active reservation, deducts points ----
SELECT * FROM rpc_loyalty_redeem_reserve(
    '10000000-0000-0000-0000-000000000001',
    'c0000000-0000-0000-0000-000000000001',
    '11111111-1111-1111-1111-111111111001',
    30,
    'a0000000-0000-0000-0000-000000000001',
    NULL,
    '11111111-1111-1111-1111-111111111901'
);
SELECT is(
    (SELECT status FROM (
        SELECT * FROM rpc_loyalty_redeem_reserve(
            '10000000-0000-0000-0000-000000000001',
            'c0000000-0000-0000-0000-000000000001',
            '11111111-1111-1111-1111-111111111001',
            30,
            'a0000000-0000-0000-0000-000000000001',
            NULL,
            '11111111-1111-1111-1111-111111111901'
        ) r
    ) sub),
    'active',
    'reserve creates active reservation'
);
SELECT is(
    (SELECT points_balance FROM loyalty_accounts
     WHERE id = 'e0000000-0000-0000-0000-000000000001'),
    '70',
    'reserve deducts points: 100 -> 70'
);

-- ---- 14) Reserve: idempotent, same key returns existing reservation ----
SELECT is(
    (SELECT count(*) FROM loyalty_reservations
     WHERE idempotency_key = '11111111-1111-1111-1111-111111111901'),
    '1',
    'idempotent reserve: only one reservation for key k1'
);

-- ---- 15) Reserve: points/reward cost mismatch rejected ----
-- Requesting 200 pts for the 30-pt "Free Muffin" reward: cost-match is
-- validated before balance, so this raises a cost-mismatch error (not
-- "insufficient points").
SELECT throws_like($$SELECT * FROM rpc_loyalty_redeem_reserve(
    '10000000-0000-0000-0000-000000000001',
    'c0000000-0000-0000-0000-000000000001',
    '11111111-1111-1111-1111-111111111001',
    200,
    'a0000000-0000-0000-0000-000000000001',
    NULL,
    '11111111-1111-1111-1111-111111111904'
)$$,
                   '%point cost% does not match requested points%',
                   'reserve rejects points/reward cost mismatch');

-- ---- 16) Reserve: permission denied (cashier) ----
SELECT throws_like($$SELECT * FROM rpc_loyalty_redeem_reserve(
    '10000000-0000-0000-0000-000000000003',
    'c0000000-0000-0000-0000-000000000001',
    '11111111-1111-1111-1111-111111111001',
    30,
    'a0000000-0000-0000-0000-000000000001',
    NULL,
    '11111111-1111-1111-1111-111111111905'
)$$,
                   '%lacks permission%',
                   'cashier reserve denied (no loyalty.redeem)');

-- ---- 17) Commit: valid commit writes redeem ledger, marks committed ----
SELECT * FROM rpc_loyalty_redeem_commit(
    (SELECT id FROM loyalty_reservations
     WHERE idempotency_key = '11111111-1111-1111-1111-111111111901'),
    '10000000-0000-0000-0000-000000000001'
);
SELECT is(
    (SELECT status FROM loyalty_reservations
     WHERE idempotency_key = '11111111-1111-1111-1111-111111111901'),
    'committed',
    'commit marks reservation committed'
);
SELECT is(
    (SELECT count(*) FROM loyalty_transactions
     WHERE loyalty_account_id = 'e0000000-0000-0000-0000-000000000001'
       AND type = 'redeem'
       AND points = -30
       AND balance_after = 70),
    '1',
    'commit writes redeem ledger entry (-30 pts, balance_after 70)'
);

-- ---- 18) Commit: non-active reservation rejected ----
SELECT throws_like($$SELECT * FROM rpc_loyalty_redeem_commit(
    (SELECT id FROM loyalty_reservations
     WHERE idempotency_key = '11111111-1111-1111-1111-111111111901'),
    '10000000-0000-0000-0000-000000000001'
)$$,
                   '%is not active%',
                   'commit rejects non-active reservation');

-- ---- 19) Release: valid release refunds points, marks released ----
-- k2 (A2, 30 pts) is ACTIVE, so disenroll of A2 is blocked while it holds
-- an active reservation.
SELECT * FROM rpc_loyalty_redeem_reserve(
    '10000000-0000-0000-0000-000000000001',
    'c0000000-0000-0000-0000-000000000002',
    '11111111-1111-1111-1111-111111111001',
    30,
    'a0000000-0000-0000-0000-000000000001',
    NULL,
    '11111111-1111-1111-1111-111111111902'
);
SELECT throws_like($$SELECT * FROM rpc_loyalty_disenroll(
    '10000000-0000-0000-0000-000000000001',
    'c0000000-0000-0000-0000-000000000002',
    '11111111-1111-1111-1111-111111111001',
    NULL
)$$,
                   '%active redemption reservation exists%',
                   'active reservation blocks disenroll');
SELECT * FROM rpc_loyalty_redeem_release(
    (SELECT id FROM loyalty_reservations
     WHERE idempotency_key = '11111111-1111-1111-1111-111111111902'),
    '10000000-0000-0000-0000-000000000001'
);
SELECT is(
    (SELECT status FROM loyalty_reservations
     WHERE idempotency_key = '11111111-1111-1111-1111-111111111902'),
    'released',
    'release marks reservation released'
);
SELECT is(
    (SELECT points_balance FROM loyalty_accounts
     WHERE id = 'e0000000-0000-0000-0000-000000000002'),
    '50',
    'refund reversal: balance restored 50 after release'
);

-- ---- 20) Expire: valid expire refunds points, marks expired ----
-- k3 (A2, 30 pts) is ACTIVE, so a cashier release on it is rejected for
-- lacking permission (not for being inactive).
SELECT * FROM rpc_loyalty_redeem_reserve(
    '10000000-0000-0000-0000-000000000001',
    'c0000000-0000-0000-0000-000000000002',
    '11111111-1111-1111-1111-111111111001',
    30,
    'a0000000-0000-0000-0000-000000000001',
    NULL,
    '11111111-1111-1111-1111-111111111903'
);
SELECT throws_like($$SELECT * FROM rpc_loyalty_redeem_release(
    (SELECT id FROM loyalty_reservations
     WHERE idempotency_key = '11111111-1111-1111-1111-111111111903'),
    '10000000-0000-0000-0000-000000000003'
)$$,
                   '%lacks permission%',
                   'cashier release denied (no loyalty.redeem)');
SELECT * FROM rpc_loyalty_redeem_expire(
    (SELECT id FROM loyalty_reservations
     WHERE idempotency_key = '11111111-1111-1111-1111-111111111903'),
    '10000000-0000-0000-0000-000000000001'
);
SELECT is(
    (SELECT status FROM loyalty_reservations
     WHERE idempotency_key = '11111111-1111-1111-1111-111111111903'),
    'expired',
    'expire marks reservation expired'
);
SELECT is(
    (SELECT points_balance FROM loyalty_accounts
     WHERE id = 'e0000000-0000-0000-0000-000000000002'),
    '50',
    'refund reversal: balance restored 50 after expire'
);
SELECT throws_like($$SELECT * FROM rpc_loyalty_redeem_expire(
    (SELECT id FROM loyalty_reservations
     WHERE idempotency_key = '11111111-1111-1111-1111-111111111903'),
    '10000000-0000-0000-0000-000000000001'
)$$,
                   '%is not active%',
                   'expire rejects non-active reservation');

-- ---- 21) Cross-tenant: org_a admin cannot reserve for org_b customer ----
SELECT throws_like($$SELECT * FROM rpc_loyalty_redeem_reserve(
    '10000000-0000-0000-0000-000000000001',
    'c0000000-0000-0000-0000-000000000003',
    '11111111-1111-1111-1111-111111111001',
    30,
    'a0000000-0000-0000-0000-000000000001',
    NULL,
    '11111111-1111-1111-1111-111111111906'
)$$,
                   '%does not exist in organization%',
                   'cross-tenant reserve rejected');

-- ---- 22) Quote: cross-tenant customer rejected ----
SELECT throws_like($$SELECT * FROM rpc_loyalty_redeem_quote(
    '10000000-0000-0000-0000-000000000001',
    'c0000000-0000-0000-0000-000000000003',
    '11111111-1111-1111-1111-111111111001',
    30,
    'a0000000-0000-0000-0000-000000000001',
    NULL
)$$,
                   '%does not exist in organization%',
                   'quote rejects cross-tenant customer');

-- ---- 23) Commit/Expire: permission denied (cashier lacks loyalty.redeem) ----
-- k4 (active, e001, 30 pts): cashier commit denied.
SELECT * FROM rpc_loyalty_redeem_reserve(
    '10000000-0000-0000-0000-000000000001',
    'c0000000-0000-0000-0000-000000000001',
    '11111111-1111-1111-1111-111111111001',
    30,
    'a0000000-0000-0000-0000-000000000001',
    NULL,
    '11111111-1111-1111-1111-111111111909'
);
SELECT throws_like($$SELECT * FROM rpc_loyalty_redeem_commit(
    (SELECT id FROM loyalty_reservations
     WHERE idempotency_key = '11111111-1111-1111-1111-111111111909'),
    '10000000-0000-0000-0000-000000000003'
)$$,
                   '%lacks permission%',
                   'cashier commit denied (no loyalty.redeem)');
-- Release k4 so k5 can be reserved on the same account.
SELECT * FROM rpc_loyalty_redeem_release(
    (SELECT id FROM loyalty_reservations
     WHERE idempotency_key = '11111111-1111-1111-1111-111111111909'),
    '10000000-0000-0000-0000-000000000001'
);
-- k5 (active, e001, 30 pts): cashier expire denied.
SELECT * FROM rpc_loyalty_redeem_reserve(
    '10000000-0000-0000-0000-000000000001',
    'c0000000-0000-0000-0000-000000000001',
    '11111111-1111-1111-1111-111111111001',
    30,
    'a0000000-0000-0000-0000-000000000001',
    NULL,
    '11111111-1111-1111-1111-111111111910'
);
SELECT throws_like($$SELECT * FROM rpc_loyalty_redeem_expire(
    (SELECT id FROM loyalty_reservations
     WHERE idempotency_key = '11111111-1111-1111-1111-111111111910'),
    '10000000-0000-0000-0000-000000000003'
)$$,
                   '%lacks permission%',
                   'cashier expire denied (no loyalty.redeem)');

-- ---- 24) Reserve: inactive account rejected ----
-- Disenroll A2's account (no active reservation now: k2 released, k3 expired).
SELECT * FROM rpc_loyalty_disenroll(
    '10000000-0000-0000-0000-000000000001',
    'c0000000-0000-0000-0000-000000000002',
    '11111111-1111-1111-1111-111111111001',
    NULL
);
SELECT throws_like($$SELECT * FROM rpc_loyalty_redeem_reserve(
    '10000000-0000-0000-0000-000000000001',
    'c0000000-0000-0000-0000-000000000002',
    '11111111-1111-1111-1111-111111111001',
    30,
    'a0000000-0000-0000-0000-000000000001',
    NULL,
    '11111111-1111-1111-1111-111111111907'
)$$,
                   '%is not active%',
                   'reserve rejects inactive account');

-- ---- 25) Re-enroll restores active status ----
SELECT * FROM rpc_loyalty_enroll(
    '10000000-0000-0000-0000-000000000001',
    'c0000000-0000-0000-0000-000000000002',
    '11111111-1111-1111-1111-111111111001',
    'v3',
    NULL
);
SELECT is(
    (SELECT status FROM loyalty_accounts
     WHERE id = 'e0000000-0000-0000-0000-000000000002'),
    'active',
    're-enrolled account is active again'
);
