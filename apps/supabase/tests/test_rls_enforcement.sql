-- Test script for RLS enforcement across tenants and branches
-- Uses pgTAP assertions to verify row-level security policies

-- Plan: run 6 tests
SELECT plan(6);

-- Test 1: Tenant A, Branch 1 user should see only Tenant A, Branch 1 data
SELECT is(
  (SELECT COUNT(*) FROM customers 
   WHERE tenant_id = '1' 
     AND branch_id = '1' 
     AND id IN (SELECT customer_id FROM orders 
                WHERE organization_id = '1' 
                  AND branch_id = '1')),
  2, 
  'Test 1: User in Tenant A, Branch 1 sees 2 customers (expected)'
);

-- Test 2: Tenant A, Branch 2 user should see only Tenant A, Branch 2 data
SELECT is(
  (SELECT COUNT(*) FROM customers 
   WHERE tenant_id = '1' 
     AND branch_id = '2' 
     AND id IN (SELECT customer_id FROM orders 
                WHERE organization_id = '1' 
                  AND branch_id = '2')),
  1, 
  'Test 2: User in Tenant A, Branch 2 sees 1 customer (expected)'
);

-- Test 3: Tenant B, Branch 1 user should see only Tenant B, Branch 1 data
SELECT is(
  (SELECT COUNT(*) FROM customers 
   WHERE tenant_id = '2' 
     AND branch_id = '1' 
     AND id IN (SELECT customer_id FROM orders 
                WHERE organization_id = '2' 
                  AND branch_id = '1')),
  1, 
  'Test 3: User in Tenant B, Branch 1 sees 1 customer (expected)'
);

-- Test 4: Cross-tenant query should return 0 rows
SELECT is(
  (SELECT COUNT(*) FROM customers 
   WHERE tenant_id = '2' 
     AND id IN (SELECT customer_id FROM orders 
                WHERE organization_id = '1' 
                  AND branch_id = '1')),
  0, 
  'Test 4: Cross-tenant query returns 0 rows (expected)'
);

-- Test 5: Cross-branch query within same tenant should return 0 rows
SELECT is(
  (SELECT COUNT(*) FROM customers 
   WHERE tenant_id = '1' 
     AND branch_id = '2' 
     AND id IN (SELECT customer_id FROM orders 
                WHERE organization_id = '1' 
                  AND branch_id = '1')),
  0, 
  'Test 5: Cross-branch query returns 0 rows (expected)'
);

-- Test 6: Cross-tenant and cross-branch query should return 0 rows
SELECT is(
  (SELECT COUNT(*) FROM customers 
   WHERE tenant_id = '2' 
     AND branch_id = '2' 
     AND id IN (SELECT customer_id FROM orders 
                WHERE organization_id = '1' 
                  AND branch_id = '1')),
  0, 
  'Test 6: Cross-tenant and cross-branch query returns 0 rows (expected)'
;