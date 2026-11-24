# Supabase Backend Setup for BREWFLOW POS

This directory contains the necessary files for setting up the Supabase backend for BREWFLOW POS. 

## What's Included

- `schema.sql`: Complete database schema with all required tables and Row Level Security (RLS) policies
- `migrations/`: Migration files for schema management
  - `001_initial_schema.sql`: Initial schema migration
  - `002_roles_schema.sql`: Roles and permissions schema
  - `003_uuid_extensions.sql`: UUID extensions setup
  - `004_auth_extensions.sql`: Auth-related extensions
  - `005_storage_setup.sql`: Storage configuration notes
- `client.js`: Supabase client configuration (imports from config.js)
- `auth.js`: Basic Auth configuration
- `auth_config.js`: Detailed Auth configuration with tenant/role handling
- `storage.js`: Storage configuration for organization-specific buckets
- `auth_service.js`: Comprehensive Auth service class
- `tests/`: Test files for verification
  - `setup_test_data.sql`: Sample data for testing
  - `verify_rls_enforcement.sql`: RLS enforcement verification queries
  - `test_auth_flow.js`: Auth flow testing script

## Setup Instructions

1. **Create Supabase Project**
   - Go to https://app.supabase.com/
   - Create a new project
   - Enable Auth in the dashboard settings
   - Note your project URL and service role key

2. **Configure Environment Variables**
   Create a `.env` file in the root directory with:
   ```
   SUPABASE_URL=https://your-project.supabase.co
   SUPABASE_ANON_KEY=your-anon-key
   SUPABASE_SERVICE_ROLE_KEY=your-service-role-key
   ```

3. **Install Dependencies**
   ```bash
   cd apps/supabase
   npm install @supabase/supabase-js
   ```

4. **Run Migrations**
   ```bash
   npx supabase db push
   ```

5. **Test Setup**
   ```bash
   node tests/setup_test_data.sql
   node tests/verify_rls_enforcement.sql
   ```

## Key Features

- **Tenant Isolation**: RLS policies ensure users can only access data from their organization
- **Role-Based Access Control**: Comprehensive role/permission model for fine-grained access control
- **JWT Token Claims**: organization_id claim included in authentication tokens for tenant isolation
- **Storage Bucket Configuration**: Organization-specific buckets with RLS policies
- **Database Schema Enhancements**: Added UUID primary keys and composite uniqueness constraints (org_id + branch_code, org_id + device_code) to ensure data integrity and prevent duplicate entries within organizations

## Important Notes

- UUID extensions must be enabled in Supabase before creating tables with UUID columns
- Auth configuration requires setting up organization-specific roles in the Supabase dashboard
- Storage bucket policies should mirror the RLS policies for consistent security
- Test scripts should be run against a real Supabase instance for full verification

## Support

For questions or issues, please contact the development team.