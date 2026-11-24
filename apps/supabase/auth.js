const { createClient } = require('@supabase/supabase-js');

// Create Supabase client instance
const supabase = createClient(
  process.env.SUPABASE_URL || 'https://your-project.supabase.co',
  process.env.SUPABASE_ANON_KEY || 'your-anon-key'
);

// Auth configuration for tenant/role handling
const authConfig = {
  // Enable organization_id claim in JWT tokens
  jwt: {
    accessTokenExpiration: 60 * 60 * 24 * 7, // 7 days
    refreshTokenExpiration: 60 * 60 * 24 * 30, // 30 days
    // organization_id will be added automatically based on the user's organization
  },
  
  // Role-based access control configuration
  role: {
    // Default role for new users
    defaultRole: 'staff',
    
    // Map of organization_id to default role
    organizationDefaultRoles: {
      // Example: 
      // '1': 'owner', // Organization 1 defaults to owner role
      // '2': 'staff',  // Organization 2 defaults to staff role
    }
  }
};

module.exports = {
  supabase,
  authConfig
};