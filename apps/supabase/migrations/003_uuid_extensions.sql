-- Enable UUID extensions for Supabase
-- Required for using UUID columns in PostgreSQL

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Additional extensions that might be needed
-- CREATE EXTENSION IF NOT EXISTS "pgcrypto";