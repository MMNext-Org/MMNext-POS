-- pgTAP earn rules & rewards tests for Task 4.
-- Fixtures (apps/supabase/tests/fixtures.sql) and migration 011 must be loaded
-- before this file. Each test file runs in its own psql session (see harness).
SELECT plan(49);

-- ---- Fixture UUIDs (see apps/supabase/tests/fixtures.sql) ----
-- org_a  = 11111111-1111-1111-1111-111111111001  (Tenant A)
-- org_b  = 22222222-2222-2222-2222-222222222002  (Tenant B)
-- e001   = e0000000-0000-0000-0000-000000000001  (Customer A1, org_a, 100 pts)
-- e002   = e0000000-0000-0000-0000-000000000002  (Customer A2, org_a, 50 pts)

-- ---- 1) Schema: loyalty_earn_rules columns exist ----
SELECT has_column('loyalty_earn_rules', 'points_per_currency_unit', 'earn_rules.points_per_currency_unit exists');
SELECT has_column('loyalty_earn_rules', 'currency_minor_unit', 'earn_rules.currency_minor_unit exists');
SELECT has_column('loyalty_earn_rules', 'min_spend_minor_units', 'earn_rules.min_spend_minor_units exists');
SELECT has_column('loyalty_earn_rules', 'max_points_per_transaction', 'earn_rules.max_points_per_transaction exists');
SELECT has_column('loyalty_earn_rules', 'eligible_tiers', 'earn_rules.eligible_tiers exists');
SELECT has_column('loyalty_earn_rules', 'eligible_channels', 'earn_rules.eligible_channels exists');
SELECT has_column('loyalty_earn_rules', 'active_from', 'earn_rules.active_from exists');
SELECT has_column('loyalty_earn_rules', 'active_until', 'earn_rules.active_until exists');
SELECT has_column('loyalty_earn_rules', 'version', 'earn_rules.version exists');
SELECT has_column('loyalty_earn_rules', 'is_active', 'earn_rules.is_active exists');

-- ---- 2) Schema: loyalty_rewards columns exist ----
SELECT has_column('loyalty_rewards', 'point_cost', 'rewards.point_cost exists');
SELECT has_column('loyalty_rewards', 'reward_value_minor_units', 'rewards.reward_value_minor_units exists');
SELECT has_column('loyalty_rewards', 'reward_type', 'rewards.reward_type exists');
SELECT has_column('loyalty_rewards', 'version', 'rewards.version exists');

-- ---- 3) Schema: loyalty_payment_events columns exist ----
SELECT has_column('loyalty_payment_events', 'idempotency_key', 'payment_events.idempotency_key exists');
SELECT has_column('loyalty_payment_events', 'event_type', 'payment_events.event_type exists');
SELECT has_column('loyalty_payment_events', 'points_earned', 'payment_events.points_earned exists');

-- ---- 4) Schema: helper functions exist ----
SELECT has_function('loyalty_points_earned', 'loyalty_points_earned function exists');
SELECT has_function('loyalty_prorate_points', 'loyalty_prorate_points function exists');
SELECT has_function('loyalty_active_earn_rule', 'loyalty_active_earn_rule function exists');
SELECT has_function('loyalty_active_reward', 'loyalty_active_reward function exists');

-- ---- 5) Insert valid earn rule succeeds ----
INSERT INTO loyalty_earn_rules (
    organization_id, points_per_currency_unit, currency_minor_unit,
    min_spend_minor_units, max_points_per_transaction,
    eligible_tiers, eligible_channels, active_from, active_until,
    version, is_active
) VALUES (
    '11111111-1111-1111-1111-111111111001', 1, 100,
    0, 1000,
    ARRAY['bronze', 'silver', 'gold'],
    ARRAY['pos', 'online'],
    NOW(), NULL,
    1, TRUE
);

SELECT is(
    (SELECT count(*) FROM loyalty_earn_rules WHERE organization_id = '11111111-1111-1111-1111-111111111001'),
    '1', 'one earn rule for org_a');

-- ---- 6) Earn rule with zero points_per_currency_unit rejected ----
SELECT throws_like($$INSERT INTO loyalty_earn_rules (
    organization_id, points_per_currency_unit, currency_minor_unit,
    version, is_active
) VALUES (
    '11111111-1111-1111-1111-111111111001', 0, 100, 2, TRUE
)$$,
                   '%points_per_currency_unit%',
                   'zero points_per_currency_unit rejected');

-- ---- 7) Earn rule with negative currency_minor_unit rejected ----
SELECT throws_like($$INSERT INTO loyalty_earn_rules (
    organization_id, points_per_currency_unit, currency_minor_unit,
    version, is_active
) VALUES (
    '11111111-1111-1111-1111-111111111001', 1, -100, 3, TRUE
)$$,
                   '%currency_minor_unit%',
                   'negative currency_minor_unit rejected');

-- ---- 8) Versioning: multiple versions per org allowed ----
INSERT INTO loyalty_earn_rules (
    organization_id, points_per_currency_unit, currency_minor_unit,
    version, is_active
) VALUES (
    '11111111-1111-1111-1111-111111111001', 2, 100, 2, TRUE
);
SELECT is(
    (SELECT count(*) FROM loyalty_earn_rules WHERE organization_id = '11111111-1111-1111-1111-111111111001'),
    '2', 'two earn rule versions for org_a');

-- ---- 9) loyalty_points_earned: 1050 minor units, 1pt per 100 = 10 pts ----
SELECT is(
    loyalty_points_earned(1050, 1, 100)::text,
    '10', '1050 minor units earns 10 points (1 per 100)');

-- ---- 10) loyalty_points_earned: 500 minor units, 1pt per 100 = 5 pts ----
SELECT is(
    loyalty_points_earned(500, 1, 100)::text,
    '5', '500 minor units earns 5 points (1 per 100)');

-- ---- 11) loyalty_points_earned: large amount floors correctly ----
SELECT is(
    loyalty_points_earned(50000, 1, 100)::text,
    '500', '50000 minor units earns 500 points');

-- ---- 12) loyalty_prorate_points: 100 pts, 50% refund = 50 pts ----
SELECT is(
    loyalty_prorate_points(100, 5000, 10000)::text,
    '50', 'half refund restores half points');

-- ---- 13) loyalty_prorate_points: round half up (3 pts * 1/3 = 1) ----
SELECT is(
    loyalty_prorate_points(3, 1, 3)::text,
    '1', 'one-third refund of 3 points = 1 point');

-- ---- 14) loyalty_prorate_points: full refund restores all ----
SELECT is(
    loyalty_prorate_points(100, 10000, 10000)::text,
    '100', 'full refund restores all points');

-- ---- 15) loyalty_prorate_points: zero original returns 0 ----
SELECT is(
    loyalty_prorate_points(100, 5000, 0)::text,
    '0', 'zero original amount returns 0');

-- ---- 16) loyalty_active_earn_rule returns active rule for org_a ----
SELECT is(
    (SELECT id FROM loyalty_active_earn_rule('11111111-1111-1111-1111-111111111001')) IS NOT NULL,
    't', 'active earn rule found for org_a');

-- ---- 17) loyalty_active_earn_rule returns nothing for org with no rule ----
SELECT is(
    (SELECT count(*) FROM loyalty_active_earn_rule('22222222-2222-2222-2222-222222222002'))::text,
    '0', 'no earn rule for org_b (tenant isolation)');

-- ---- 18) Insert reward for org_a ----
INSERT INTO loyalty_rewards (
    organization_id, name, description, point_cost,
    reward_value_minor_units, reward_type, version, is_active
) VALUES (
    '11111111-1111-1111-1111-111111111001',
    'Free Coffee', '100 points for a free coffee',
    100, 500, 'free_item', 1, TRUE
);
SELECT is(
    (SELECT count(*) FROM loyalty_rewards WHERE organization_id = '11111111-1111-1111-1111-111111111001'),
    '1', 'one reward for org_a');

-- ---- 19) Reward with zero point_cost rejected ----
SELECT throws_like($$INSERT INTO loyalty_rewards (
    organization_id, name, point_cost, version, is_active
) VALUES (
    '11111111-1111-1111-1111-111111111001', 'Bad Reward', 0, 2, TRUE
)$$,
                   '%point_cost%',
                   'zero point_cost rejected');

-- ---- 20) loyalty_active_reward returns active reward ----
SELECT is(
    (SELECT name FROM loyalty_active_reward('11111111-1111-1111-1111-111111111001', 'Free Coffee')),
    'Free Coffee', 'active reward Free Coffee found for org_a');

-- ---- 21) Payment event idempotency: duplicate key rejected ----
SELECT throws_like($t1$DO $t2$
BEGIN
    INSERT INTO loyalty_payment_events (
        loyalty_account_id, organization_id, customer_id,
        event_type, points_earned, balance_after, idempotency_key, reason
    ) VALUES (
        'e0000000-0000-0000-0000-000000000001',
        '11111111-1111-1111-1111-111111111001',
        'c0000000-0000-0000-0000-000000000001',
        'payment_completed', 10, 110,
        '11111111-1111-1111-1111-111111111901', 'earn from payment'
    );
    INSERT INTO loyalty_payment_events (
        loyalty_account_id, organization_id, customer_id,
        event_type, points_earned, balance_after, idempotency_key, reason
    ) VALUES (
        'e0000000-0000-0000-0000-000000000001',
        '11111111-1111-1111-1111-111111111001',
        'c0000000-0000-0000-0000-000000000001',
        'payment_completed', 10, 120,
        '11111111-1111-1111-1111-111111111901', 'duplicate earn'
    );
END $t2$$t1$,
                   'duplicate key%',
                   'duplicate payment event idempotency_key rejected');

-- ---- 22) Insert payment_completed event with positive points ----
INSERT INTO loyalty_payment_events (
    loyalty_account_id, organization_id, customer_id,
    event_type, points_earned, balance_after, idempotency_key, reason
) VALUES (
    'e0000000-0000-0000-0000-000000000001',
    '11111111-1111-1111-1111-111111111001',
    'c0000000-0000-0000-0000-000000000001',
    'payment_completed', 10, 110,
    '11111111-1111-1111-1111-111111111902', 'earn from payment'
);
SELECT is(
    (SELECT count(*) FROM loyalty_payment_events WHERE idempotency_key = '11111111-1111-1111-1111-111111111902'),
    '1', 'payment_completed event inserted');

-- ---- 23) Insert payment_refunded event with negative points ----
INSERT INTO loyalty_payment_events (
    loyalty_account_id, organization_id, customer_id,
    event_type, points_earned, balance_after, idempotency_key, reason
) VALUES (
    'e0000000-0000-0000-0000-000000000001',
    '11111111-1111-1111-1111-111111111001',
    'c0000000-0000-0000-0000-000000000001',
    'payment_refunded', -10, 100,
    '11111111-1111-1111-1111-111111111903', 'refund reversal'
);
SELECT is(
    (SELECT count(*) FROM loyalty_payment_events WHERE event_type = 'payment_refunded'),
    '1', 'payment_refunded event inserted with negative points');

-- ---- 24) Zero points_earned rejected ----
SELECT throws_like($$INSERT INTO loyalty_payment_events (
    loyalty_account_id, organization_id, customer_id,
    event_type, points_earned, balance_after, idempotency_key, reason
) VALUES (
    'e0000000-0000-0000-0000-000000000001',
    '11111111-1111-1111-1111-111111111001',
    'c0000000-0000-0000-0000-000000000001',
    'payment_completed', 0, 110,
    '11111111-1111-1111-1111-111111111904', 'zero points'
)$$,
                   '%points_earned%',
                   'zero points_earned rejected');

-- ---- 25) Cross-tenant: earn rule for org_a not visible to org_b ----
SELECT is(
    (SELECT count(*) FROM loyalty_earn_rules WHERE organization_id = '22222222-2222-2222-2222-222222222002')::text,
    '0', 'org_b has no earn rules (tenant isolation)');

-- ---- 26) Balance cannot go negative via payment_events CHECK ----
SELECT throws_like($$INSERT INTO loyalty_payment_events (
    loyalty_account_id, organization_id, customer_id,
    event_type, points_earned, balance_after, idempotency_key, reason
) VALUES (
    'e0000000-0000-0000-0000-000000000001',
    '11111111-1111-1111-1111-111111111001',
    'c0000000-0000-0000-0000-000000000001',
    'payment_refunded', -500, -1,
    '11111111-1111-1111-1111-111111111905', 'negative balance'
)$$,
                   '%balance_after%',
                   'negative balance_after rejected');

-- ---- 27) Reward versioning: multiple versions per org/name ----
INSERT INTO loyalty_rewards (
    organization_id, name, point_cost, reward_value_minor_units,
    reward_type, version, is_active
) VALUES (
    '11111111-1111-1111-1111-111111111001',
    'Free Coffee', 150, 500, 'free_item', 2, TRUE
);
SELECT is(
    (SELECT count(*) FROM loyalty_rewards WHERE organization_id = '11111111-1111-1111-1111-111111111001' AND name = 'Free Coffee'),
    '2', 'two versions of Free Coffee reward for org_a');

-- ---- 28) loyalty_active_reward returns latest version ----
SELECT is(
    (SELECT point_cost FROM loyalty_active_reward('11111111-1111-1111-1111-111111111001', 'Free Coffee')),
    '150', 'active reward returns latest version (150 pts)');

-- ---- 29) inactive earn rule not returned by active lookup ----
UPDATE loyalty_earn_rules SET is_active = FALSE WHERE version = 1 AND organization_id = '11111111-1111-1111-1111-111111111001';
SELECT is(
    (SELECT count(*) FROM loyalty_active_earn_rule('11111111-1111-1111-1111-111111111001'))::text,
    '1', 'inactive rule excluded, only active v2 returned');

-- ---- 30) earn rule expired by active_until not returned ----
UPDATE loyalty_earn_rules SET active_until = NOW() - INTERVAL '1 hour' WHERE version = 2 AND organization_id = '11111111-1111-1111-1111-111111111001';
SELECT is(
    (SELECT count(*) FROM loyalty_active_earn_rule('11111111-1111-1111-1111-111111111001'))::text,
    '0', 'expired rule not returned by active lookup');
