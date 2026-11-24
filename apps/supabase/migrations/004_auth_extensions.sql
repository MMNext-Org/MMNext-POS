-- Auth extensions and enhancements
-- Enable necessary extensions for Auth functionality

CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Add columns for Auth-related data
-- These would typically be added to existing tables or as new tables

-- Example: Add organization_id to auth_users if not already present
-- ALTER TABLE auth_users ADD COLUMN IF NOT EXISTS organization_id UUID REFERENCES organizations(id);

-- Example: Add tenant information to user sessions
-- This would be handled through JWT claims rather than database columns

-- Additional Auth-related extensions
-- CREATE EXTENSION IF NOT EXISTS "uuid-ossp";  -- Already created in 003_uuid_extensions.sql