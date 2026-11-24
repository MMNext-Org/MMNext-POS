-- Roles and Permissions Schema
-- Define role hierarchy and permissions for fine-grained access control
-- Each role can have multiple permissions, and permissions can be applied to tables/rows

CREATE TABLE roles (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name TEXT NOT NULL UNIQUE,  -- e.g., 'admin', 'owner', 'staff', 'cashier'
    description TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE permissions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name TEXT NOT NULL UNIQUE,  -- e.g., 'orders:read', 'orders:write', 'products:create'
    description TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE role_permissions (
    role_id UUID NOT NULL REFERENCES roles(id) ON DELETE CASCADE,
    permission_id UUID NOT NULL REFERENCES permissions(id) ON DELETE CASCADE,
    PRIMARY KEY (role_id, permission_id)
);

CREATE TABLE user_roles (
    user_id UUID NOT NULL REFERENCES auth_users(id) ON DELETE CASCADE,
    role_id UUID NOT NULL REFERENCES roles(id) ON DELETE CASCADE,
    organization_id UUID NOT NULL,
    PRIMARY KEY (user_id, role_id, organization_id)
);

-- Example data (can be populated via seed or admin UI)
-- INSERT INTO roles (name, description) VALUES ('admin', 'Full access to all features');
-- INSERT INTO roles (name, description) VALUES ('owner', 'Can manage tenants and settings');
-- INSERT INTO roles (name, description) VALUES ('staff', 'Can manage orders and inventory');
-- INSERT INTO roles (name, description) VALUES ('cashier', 'Can process payments and manage cash');

-- Example permissions
-- INSERT INTO permissions (name, description) VALUES ('orders:read', 'Read orders');
-- INSERT INTO permissions (name, description) VALUES ('orders:write', 'Create or update orders');
-- INSERT INTO permissions (name, description) VALUES ('products:read', 'Read product catalog');
-- INSERT INTO permissions (name, description) VALUES ('products:create', 'Create new products');
-- INSERT INTO permissions (name, description) VALUES ('inventory:read', 'Read stock levels');
-- INSERT INTO permissions (name, description) VALUES ('inventory:write', 'Update stock levels');

-- Assign permissions to roles
-- INSERT INTO role_permissions (role_id, permission_id) VALUES (1, 1), (1, 2), (1, 3), (1, 4);
-- INSERT INTO role_permissions (role_id, permission_id) VALUES (2, 1), (2, 3);
-- INSERT INTO role_permissions (role_id, permission_id) VALUES (3, 1), (3, 5);
-- INSERT INTO role_permissions (role_id, permission_id) VALUES (4, 1), (4, 5);

-- Assign roles to users (example)
-- INSERT INTO user_roles (user_id, role_id, organization_id) VALUES (1, 1, 1);  -- user 1 is admin of org 1
-- INSERT INTO user_roles (user_id, role_id, organization_id) VALUES (2, 2, 1);  -- user 2 is owner of org 1