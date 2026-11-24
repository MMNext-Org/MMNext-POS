const { createClient } = require('@supabase/supabase-js');

// Create Supabase client instance
const supabase = createClient(
  process.env.SUPABASE_URL || 'https://your-project.supabase.co',
  process.env.SUPABASE_SERVICE_ROLE_KEY || 'your-service-role-key'
);

// Comprehensive Auth service for tenant/role handling
class AuthService {
  constructor() {
    this.supabase = supabase;
    this.authConfig = require('./auth_config').authConfig;
  }

  // Initialize Auth with tenant/role handling
  async initialize() {
    // Check if Auth is enabled
    const { data: { auth } } = await this.supabase.auth.getSession();
    
    if (!data.auth) {
      throw new Error('Auth service not properly configured');
    }

    // Set up Auth event listener for session changes
    this.supabase.auth.on('session', async ({ event, session }) => {
      if (event === 'SIGNED_IN' && session) {
        // Extract organization_id from JWT claims
        const organization_id = session.user.roles[0]?.organization_id;
        
        if (!organization_id) {
          throw new Error('User is not associated with any organization');
        }

        // Store organization_id in session metadata
        await this.supabase.auth.setUserMetadata({
          organization_id: organization_id
        });
      }
    });

    // Set up Auth event listener for sign out
    this.supabase.auth.on('signout', async ({ session }) => {
      if (session) {
        // Clear organization-specific metadata
        await this.supabase.auth.setUserMetadata({
          organization_id: null
        });
      }
    });

    // Method to get user's organization_id
    this.getOrganizationId = async () => {
      const { data, error } = await this.supabase.auth.getSession();
      if (error) throw error;
      
      if (!data.session) {
        throw new Error('User is not authenticated');
      }

      // Extract organization_id from JWT claims
      return data.session.user.roles[0]?.organization_id;
    };

    // Method to get user's role
    this.getUserRole = async () => {
      const { data, error } = await this.supabase.auth.getSession();
      if (error) throw error;
      
      if (!data.session) {
        throw new Error('User is not authenticated');
      }

      return data.session.user.roles[0]?.role;
    };

    // Method to check if user has a specific permission.
    // Authorization is enforced in PostgreSQL (migration 010: user_has_permission
    // + RLS); this is a server-side check that never trusts the browser.
    // userId/orgId must come from the authenticated server session, never from
    // client input. Default-deny when they are not supplied.
    this.hasPermission = async (permission, { userId, orgId } = {}) => {
      if (!permission || !userId || !orgId) {
        return false;
      }

      const { data, error } = await this.supabase.rpc('user_has_permission', {
        p_user_id: userId,
        p_permission: permission,
        p_org_id: orgId
      });

      if (error) {
        return false;
      }
      return data === true;
    };

    return this;
  }

  // Example method to fetch data with tenant isolation
  async getTenantData(userId, tenantOrganizationId) {
    // This would be used in actual application code
    // to ensure RLS policies are enforced
    return this.supabase
      .from('some_tenant_table')
      .select('*')
      .eq('organization_id', tenantOrganizationId)
      .eq('id', userId);
  }

  // Example method to create a new user with organization assignment
  async createUserWithOrganization(email, organizationId, password, role = 'staff') {
    // This would be part of the registration flow
    const { data, error } = await this.supabase.auth.admin.createUser({
      email,
      password,
      email_check: true,
      user_metadata: {
        organization_id: organizationId,
        role: role
      }
    });

    if (error) throw error;
    return data;
  }
}

// Export the Auth service
module.exports = AuthService;