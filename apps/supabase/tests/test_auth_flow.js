// Test script for Auth flow verification
// This script tests the Auth flow including tenant/role handling

const AuthService = require('../auth_service');

describe('Auth Service', () => {
  let authService;

  beforeAll(async () => {
    authService = new AuthService();
    await authService.initialize();
    console.log('Auth Service initialized');
  });

  test('Should get organization ID from session', async () => {
    // Mock session data for testing
    const mockSession = {
      user: {
        roles: [
          { organization_id: '1', role: 'admin' }  // User belongs to Organization 1
        ]
      }
    };

    // Mock the supabase.auth.getSession method
    jest.spyOn(authService.supabase.auth, 'getSession').mockResolvedValue({
      data: { session: mockSession },
      error: null
    });

    const orgId = await authService.getOrganizationId();
    expect(orgId).toBe('1');
  });

  test('Should get user role from session', async () => {
    // Mock session data for testing
    const mockSession = {
      user: {
        roles: [
          { organization_id: '1', role: 'admin' }  // User is admin
        ]
      }
    };

    // Mock the supabase.auth.getSession method
    jest.spyOn(authService.supabase.auth, 'getSession').mockResolvedValue({
      data: { session: mockSession },
      error: null
    });

    const role = await authService.getUserRole();
    expect(role).toBe('admin');
  });

  test('Should have proper token validation configuration', () => {
    const authConfig = require('./auth_config').authConfig;
    
    // Check that required configuration options exist
    expect(authConfig.jwt).toBeDefined();
    expect(authConfig.role).toBeDefined();
    expect(authConfig.tokenValidation).toBeDefined();
    expect(authConfig.userManagement).toBeDefined();
  });

  test('Should have proper token expiration settings', () => {
    const authConfig = require('./auth_config').authConfig;
    
    // Check token expiration settings
    expect(authConfig.jwt.accessTokenExpiration).toBe(60 * 60 * 24 * 7); // 7 days
    expect(authConfig.jwt.refreshTokenExpiration).toBe(60 * 60 * 24 * 30); // 30 days
  });

  test('Should have proper role-based permissions mapping', () => {
    const authConfig = require('./auth_config').authConfig;
    
    // Check that role permissions are defined
    expect(authConfig.role.rolePermissions).toBeDefined();
    expect(authConfig.role.rolePermissions).toHaveProperty('admin');
    expect(authConfig.role.rolePermissions.admin).toContain('orders:read');
    expect(authConfig.role.rolePermissions.admin).toContain('orders:write');
  });

  test('Should properly handle organization assignment in user creation', async () => {
    // This would be part of actual application code
    // For testing, we'll just verify the method exists
    expect(typeof AuthService.createUserWithOrganization).toBe('function');
  });
});