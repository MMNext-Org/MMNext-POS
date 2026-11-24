-- pgTAP loyalty ledger & lifecycle schema tests for Task 3.
-- Fixtures (apps/supabase/tests/fixtures.sql) must be loaded before this file.
-- 3 loyalty accounts (e001=100, e002=50, e003=200) exist with NO ledger rows.
SELECT plan(13);

-- ---- 1) loyalty_accounts lifecycle columns exist ----
SELECT has_column('loyalty_accounts', 'status', 'loyalty_accounts.status exists');
SELECT has_column('loyalty_accounts', 'enrolled_at', 'loyalty_accounts.enrolled_at exists');
SELECT has_column('loyalty_accounts', 'disenrolled_at', 'loyalty_accounts.disenrolled_at exists');
SELECT has_column('loyalty_accounts', 'consent_version', 'loyalty_accounts.consent_version exists');
SELECT has_column('loyalty_accounts', 'consented_at', 'loyalty_accounts.consented_at exists');
SELECT has_column('loyalty_accounts', 'version', 'loyalty_accounts.version exists');

-- ---- 2) status defaults to 'active' ----
SELECT ok((SELECT column_default
           FROM information_schema.columns
           WHERE table_name = 'loyalty_accounts' AND column_name = 'status') ILIKE '%active%',
          'status defaults to active');

-- ---- 3) loyalty_transactions idempotency key exists ----
SELECT has_column('loyalty_transactions', 'idempotency_key', 'loyalty_transactions.idempotency_key exists');

-- ---- 4) discrepancy report: all 3 fixture accounts imbalanced (balance but no ledger) ----
SELECT is((SELECT count(*) FROM loyalty_discrepancy_report()), '3',
          'all 3 accounts discrepant before backfill');

-- ---- 5) backfill seeds one opening row per account ----
SELECT is(loyalty_backfill_opening_balances(), '3',
          'backfill inserts 3 opening rows');

-- ---- 6) backfill is idempotent (second run seeds nothing) ----
SELECT is(loyalty_backfill_opening_balances(), '0',
          'backfill idempotent: 0 rows on re-run');

-- ---- 7) discrepancy report empty after backfill ----
SELECT is((SELECT count(*) FROM loyalty_discrepancy_report()), '0',
          'no discrepancies after backfill');

-- ---- 8) duplicate idempotency_key is rejected ----
SELECT throws_like($t1$DO $t2$ BEGIN
    INSERT INTO loyalty_transactions (loyalty_account_id, organization_id, customer_id, type, points, balance_after, reason, idempotency_key)
    VALUES ('e0000000-0000-0000-0000-000000000001', '11111111-1111-1111-1111-111111111001', 'c0000000-0000-0000-0000-000000000001', 'adjust', 10, 10, 'idempotency test', '11111111-1111-1111-1111-111111111901');
    INSERT INTO loyalty_transactions (loyalty_account_id, organization_id, customer_id, type, points, balance_after, reason, idempotency_key)
    VALUES ('e0000000-0000-0000-0000-000000000001', '11111111-1111-1111-1111-111111111001', 'c0000000-0000-0000-0000-000000000001', 'adjust', 10, 10, 'idempotency test', '11111111-1111-1111-1111-111111111901');
END
$t2$$t1$,
                   'duplicate key%',
                   'duplicate idempotency_key rejected');
