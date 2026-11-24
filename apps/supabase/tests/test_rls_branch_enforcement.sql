-- Test script to verify Row Level Security (RLS) enforcement for tenants and branches
-- This script tests that users can only access data from their own tenant and branch

-- Connect to the database as different users with different tenant/branch combinations
-- Test cases:
-- 1. User in Tenant A, Branch 1 should only see data from Tenant A, Branch 1
-- 2. User in Tenant A, Branch 2 should only see data from Tenant A, Branch 2
-- 3. User in Tenant B, Branch 1 should only see data from Tenant B, Branch 1
-- 4. Cross-tenant queries should be blocked
-- 5. Cross-branch queries within same tenant should be blocked

-- Example query to test tenant and branch isolation for customers
-- Replace 'user1_id' with the actual user ID from Tenant A, Branch 1
SELECT * FROM customers
WHERE tenant_id = '1'      -- Tenant ID
  AND branch_id = '1'      -- Branch ID
  AND id IN (
    SELECT customer_id FROM orders
    WHERE organization_id = '1'  -- User's tenant
      AND branch_id = '1'        -- User's branch
  );

-- This query should return results only if the user belongs to Tenant A, Branch 1
-- If the user belongs to Tenant A, Branch 2, this query should return 0 rows

-- Test 1: User from Tenant A, Branch 1 should see data from Tenant A, Branch 1 only
-- Replace 'user1_id' with actual user ID from Tenant A, Branch 1
SELECT * FROM customers
WHERE tenant_id = '1'
  AND branch_id = '1'
  AND id IN (
    SELECT customer_id FROM orders
    WHERE organization_id = '1'
      AND branch_id = '1'
  );

-- Test 2: User from Tenant A, Branch 2 should see data from Tenant A, Branch 2 only
-- Replace 'user2_id' with actual user ID from Tenant A, Branch 2
SELECT * FROM customers
WHERE tenant_id = '1'
  AND branch_id = '2'
  AND id IN (
    SELECT customer_id FROM orders
    WHERE organization_id = '1'
      AND branch_id = '2'
  );

-- Test 3: User from Tenant B, Branch 1 should see data from Tenant B, Branch 1 only
-- Replace 'user3_id' with actual user ID from Tenant B, Branch 1
SELECT * FROM customers
WHERE tenant_id = '2'
  AND branch_id = '1'
  AND id IN (
    SELECT customer_id FROM orders
    WHERE organization_id = '2'
      AND branch_id = '1'
  );

-- Test 4: Cross-tenant query (should return 0 rows for user from Tenant A)
-- This query should fail for users not in Tenant 1
SELECT * FROM customers
WHERE tenant_id = '2'      -- Different tenant
  AND id IN (
    SELECT customer_id FROM orders
    WHERE organization_id = '1'  -- User's tenant
      AND branch_id = '1'        -- User's branch
  );

-- Test 5: Cross-branch query within same tenant (should return 0 rows for user from Branch 1)
-- This query should fail for users not in Branch 1
SELECT * FROM customers
WHERE tenant_id = '1'      -- Same tenant
  AND branch_id = '2'      -- Different branch
  AND id IN (
    SELECT customer_id FROM orders
    WHERE organization_id = '1'  -- User's tenant
      AND branch_id = '1'        -- User's branch
  );

-- Test 6: Cross-tenant and cross-branch query (should return 0 rows for user from Tenant A, Branch 1)
-- This query should fail for users not in Tenant 2, Branch 1
SELECT * FROM customers
WHERE tenant_id = '2'      -- Different tenant
  AND branch_id = '2'      -- Different branch
  AND id IN (
    SELECT customer_id FROM orders
    WHERE organization_id = '1'  -- User's tenant
      AND branch_id = '1'        -- User's branch
  );

-- Additional tests for other tables
-- Orders table
SELECT * FROM orders
WHERE tenant_id = '1'
  AND branch_id = '1'
  AND id IN (
    SELECT order_id FROM order_items
    WHERE product_id IN (
      SELECT id FROM products
      WHERE tenant_id = '1'
        AND branch_id = '1'
    )
  );

-- Products table
SELECT * FROM products
WHERE tenant_id = '1'
  AND branch_id = '1'
  AND id IN (
    SELECT product_id FROM order_items
    WHERE order_id IN (
      SELECT order_id FROM orders
      WHERE tenant_id = '1'
        AND branch_id = '1'
    )
  );

-- Payments table
SELECT * FROM payments
WHERE tenant_id = '1'
  AND branch_id = '1'
  AND id IN (
    SELECT payment_id FROM refunds
    WHERE organization_id = '1'
      AND branch_id = '1'
  );

-- Note: In a real testing environment, these queries would be executed
-- through the Supabase client with proper authentication as different users
-- and the RLS policies should prevent unauthorized access