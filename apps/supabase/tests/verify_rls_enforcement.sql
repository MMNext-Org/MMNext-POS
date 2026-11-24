-- Test script to verify Row Level Security (RLS) enforcement
-- This script tests that users can only access data from their own organization

-- Connect to the database as a test user (example: user1@org1.com)
-- Note: In a real scenario, this would be executed through the Supabase client

-- Test 1: User from Organization A should only see data from Organization A
-- Test 2: User from Organization B should only see data from Organization B
-- Test 3: Verify that cross-tenant queries are blocked

-- Example query to test tenant isolation for orders
-- Replace 'user1_id' with the actual user ID from Organization A
SELECT * FROM orders 
WHERE organization_id = '1'  -- User's organization
  AND id IN (
    SELECT order_id FROM order_items 
    WHERE product_id IN (
      SELECT id FROM products 
      WHERE organization_id = '1'  -- Only products from user's organization
    )
  );

-- This query should return results only if the user belongs to Organization A
-- If the user belongs to Organization B, this query should return 0 rows

-- Test 2: User from Organization B should see data from Organization B only
-- Replace 'user2_id' with the actual user ID from Organization B
SELECT * FROM orders 
WHERE organization_id = '2'  -- User's organization
  AND id IN (
    SELECT order_id FROM order_items 
    WHERE product_id IN (
      SELECT id FROM products 
      WHERE organization_id = '2'  -- Only products from user's organization
    )
  );

-- Test 3: Verify that a user cannot access data from another organization
-- This query should return 0 rows for a user from Organization A trying to access Organization B data
SELECT * FROM orders 
WHERE organization_id = '2'  -- Different organization
  AND id IN (
    SELECT order_id FROM order_items 
    WHERE product_id IN (
      SELECT id FROM products 
      WHERE organization_id = '1'  -- Different organization
    )
  );

-- Additional tests for other tables
-- Products table
SELECT * FROM products 
WHERE organization_id = '1'  -- User's organization

-- Customers table
SELECT * FROM customers 
WHERE organization_id = '1'  -- User's organization

-- Payments table
SELECT * FROM payments 
WHERE organization_id = '1'  -- User's organization

-- Refunds table
SELECT * FROM refunds 
WHERE organization_id = '1'  -- User's organization

-- Test query that should fail (cross-tenant access)
-- This should return 0 rows for a user from Organization A
SELECT * FROM customers 
WHERE organization_id = '2'  -- Different organization
  AND id IN (
    SELECT customer_id FROM orders 
    WHERE organization_id = '1'  -- User's organization
  );

-- Note: In a real testing environment, these queries would be executed
-- through the Supabase client with proper authentication as different users
-- and the RLS policies should prevent unauthorized access