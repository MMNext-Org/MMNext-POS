-- Setup test data for RLS enforcement and tenant isolation testing
-- This script creates sample data to verify RLS policies work correctly

-- Create test organizations
INSERT INTO organizations (id, name, created_at) VALUES
('1', 'Organization A', NOW()),
('2', 'Organization B', NOW());

-- Create test users for each organization
INSERT INTO auth_users (id, email, password_hash, organization_id, is_active, created_at, updated_at) VALUES
('1', 'admin1@org1.com', 'hashed_password1', '1', TRUE, NOW(), NOW()),
('2', 'user1@org1.com', 'hashed_password2', '1', TRUE, NOW(), NOW()),
('3', 'admin2@org2.com', 'hashed_password3', '2', TRUE, NOW(), NOW()),
('4', 'user2@org2.com', 'hashed_password4', '2', TRUE, NOW(), NOW());

-- Create test roles and assign them to users
INSERT INTO roles (id, name, description) VALUES
('1', 'admin', 'Full access to all features'),
('2', 'owner', 'Can manage tenants and settings'),
('3', 'staff', 'Can manage orders and inventory');

-- Assign roles to users
INSERT INTO user_roles (user_id, role_id, organization_id) VALUES
('1', '1', '1'),  -- admin1 is admin of org1
('2', '3', '1'),  -- user1 is staff of org1
('3', '2', '2'),  -- admin2 is owner of org2
('4', '3', '2');  -- user2 is staff of org2

-- Create test products for each organization
INSERT INTO products (id, name, organization_id, created_at) VALUES
('1', 'Product A1', '1', NOW()),
('2', 'Product B1', '1', NOW()),
('3', 'Product A2', '2', NOW()),
('4', 'Product B2', '2', NOW());

-- Create test orders
INSERT INTO orders (id, organization_id, customer_id, total_amount, created_at) VALUES
('1', '1', '1', 100.00, NOW()),
('2', '1', '2', 200.00, NOW()),
('3', '2', '3', 150.00, NOW()),
('4', '2', '4', 300.00, NOW());

-- Create test order items
INSERT INTO order_items (id, order_id, product_id, quantity, price, created_at) VALUES
('1', '1', '1', 2, 50.00, NOW()),
('2', '2', '2', 4, 50.00, NOW()),
('3', '3', '3', 3, 50.00, NOW()),
('4', '4', '4', 6, 50.00, NOW());

-- Create test customers
INSERT INTO customers (id, organization_id, name, email, created_at) VALUES
('1', '1', 'Customer A1', 'customer1@org1.com', NOW()),
('2', '1', 'Customer A2', 'customer2@org1.com', NOW()),
('3', '2', 'Customer B1', 'customer3@org2.com', NOW()),
('4', '2', 'Customer B2', 'customer4@org2.com', NOW());

-- Create test payments
INSERT INTO payments (id, order_id, organization_id, amount, status, created_at) VALUES
('1', '1', '1', 100.00, 'completed', NOW()),
('2', '2', '1', 200.00, 'completed', NOW()),
('3', '3', '2', 150.00, 'completed', NOW()),
('4', '4', '2', 300.00, 'completed', NOW());

-- Create test refunds
INSERT INTO refunds (id, payment_id, organization_id, amount, status, created_at) VALUES
('1', '1', '1', 25.00, 'completed', NOW()),
('2', '2', '1', 50.00, 'completed', NOW()),
('3', '3', '2', 37.50, 'completed', NOW()),
('4', '4', '2', 75.00, 'completed', NOW());