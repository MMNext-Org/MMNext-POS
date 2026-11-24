const { createClient } = require('@supabase/supabase-js');

// Create Supabase client instance
const supabase = createClient(
  process.env.SUPABASE_URL || 'https://your-project.supabase.co',
  process.env.SUPABASE_SERVICE_ROLE_KEY || 'your-service-role-key'
);

// Comprehensive Auth configuration for tenant/role handling
const authConfig = {
  // JWT Configuration
  jwt: {
    // Token expiration times
    accessTokenExpiration: 60 * 60 * 24 * 7,    // 7 days
    refreshTokenExpiration: 60 * 60 * 24 * 30,  // 30 days
    
    // Custom claims to include in JWT tokens
    claims: {
      // organization_id will be automatically added based on user's organization
      organization_id: true,
      role: true,
      tenant: true,
      
      // Additional custom claims can be added here
      // For example:
      // is_verified: true,
      // created_at: true
    }
  },
  
  // Role-Based Access Control (RBAC) configuration
  role: {
    // Default role for new users
    defaultRole: 'staff',
    
    // Organization-specific default roles
    organizationDefaultRoles: {
      '1': 'owner',    // Organization 1 defaults to owner role
      '2': 'staff',    // Organization 2 defaults to staff role
      '3': 'admin',    // Organization 3 defaults to admin role
      // Add more organizations as needed
    },
    
    // Role permissions mapping.
    // Scoped keys (enforced in PostgreSQL by migration 010):
    //   loyalty.manage, loyalty.redeem, customer.export, customer.erase
    rolePermissions: {
      'admin': ['orders:read', 'orders:write', 'products:read', 'products:create', 'inventory:read', 'inventory:write', 'customers:read', 'customers:write', 'payments:read', 'payments:write', 'loyalty.manage', 'loyalty.redeem', 'customer.export', 'customer.erase'],
      'owner': ['orders:read', 'orders:write', 'products:read', 'products:create', 'inventory:read', 'customers:read', 'customers:write', 'payments:read', 'payments:write', 'settings:read', 'settings:write', 'loyalty.manage', 'loyalty.redeem', 'customer.export', 'customer.erase'],
      'staff': ['orders:read', 'orders:write', 'products:read', 'inventory:read', 'customers:read', 'payments:read', 'loyalty.redeem'],
      'cashier': ['orders:read', 'orders:write', 'payments:read', 'payments:write']
    }
  },
  
  // Token validation and user management
  tokenValidation: {
    // Validate token signatures
    validateSignature: true,
    
    // Check token revocation status
    checkRevocation: true,
    
    // Token refresh logic
    autoRefresh: true,
    
    // Session management
    sessionCheckFrequency: 300000 // Check session every 5 minutes
  },
  
  // User management
  userManagement: {
    // User registration settings
    emailVerification: true,
    passwordPolicy: {
      minLength: 8,
      maxLength: 128,
      requireSpecialChar: true,
      requireNumber: true
    },
    
    // Profile settings
    profileFields: [
      'full_name',
      'phone_number',
      'address',
      'profile_picture_url'
    ]
  }
};

module.exports = {
  supabase,
  authConfig
};