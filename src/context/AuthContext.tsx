import React, { createContext, useContext, useEffect, useState } from 'react';
import { createClient } from '@supabase/supabase-js';

type AuthContextType = {
  user: any | null;
  loading: boolean;
  registrationLoading: boolean;
  register: (email: string, password: string, name?: string) => Promise<void>;
  setAppContext: (organizationId: string, branchId: string, roleId: string) => Promise<void>;
};

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider = ({ children }) => {
  const supabase = createClient(
    process.env.REACT_APP_SUPABASE_URL || 'http://localhost:54321',
    process.env.REACT_APP_SUPABASE_ANON_KEY || 'public-anon-key'
  );
  
  const [user, setUser] = useState<any | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [registrationLoading, setRegistrationLoading] = useState<boolean>(false);

  useEffect(() => {
    // Initialize session
    const getSession = async () => {
      const { data: { session } } = await supabase.auth.getSession();
      setUser(session?.user ?? null);
      setLoading(false);
    };
    
    // Subscribe to auth state changes
    const { data: { subscription } } = supabase.auth.onAuthStateChange((event, session) => {
      setUser(session?.user ?? null);
      setLoading(false);
    });

    // Clean up subscription on unmount
    return () => {
      subscription.unsubscribe();
    };
    
    // Get initial session
    getSession();
  }, []);

  const login = async (email: string, password: string) => {
    setLoading(true);
    const { data, error } = await supabase.auth.signInWithPassword({
      email,
      password,
    });
    
    if (error) {
      setLoading(false);
      // Handle login error (e.g., display error message)
      throw error;
    }
    
      // Set app context after successful login if user data is available
      if (data?.user) {
        try {
                // Assuming user metadata contains organization_id, branch_id, role_id, and role_name
                const { app_metadata } = data.user;
                const organizationId = app_metadata?.organization_id;
                const branchId = app_metadata?.branch_id;
                const roleId = app_metadata?.role_id;
                const roleName = app_metadata?.role_name;
          
                if (organizationId && branchId && roleId && roleName) {
                  await setAppContext(organizationId, branchId, roleId, roleName);
                }
              } catch (contextError) {
                console.error('Error setting app context:', contextError);
                // Continue with login even if context setting fails
              }
        
              setUser(data?.user ?? null);
              setLoading(false);
      }
  };

  const logout = () => {
    supabase.auth.signOut();
    setUser(null);
  };

  const register = async (email: string, password: string, name?: string) => {
    setRegistrationLoading(true);
    try {
      const { data, error } = await supabase.auth.signUp({
        email,
        password,
        options: {
          data: { name },
        },
      });
      
      if (error) {
        throw error;
      }
      
      // Optionally handle post-registration flow (e.g., auto-login, redirect)
      // For now, we just set user if data exists
      if (data?.user) {
        setUser(data.user);
        
          try {
          // Set app context after successful registration
          const { app_metadata } = data.user;
          const organizationId = app_metadata?.organization_id;
          const branchId = app_metadata?.branch_id;
          const roleId = app_metadata?.role_id;
                const roleName = app_metadata?.role_name;
          
                if (organizationId && branchId && roleId && roleName) {
                  await setAppContext(organizationId, branchId, roleId, roleName);
          }
              } catch (contextError) {
                console.error('Error setting app context:', contextError);
                // Continue with registration even if context setting fails
              }
      }
    } catch (error) {
      throw error;
    } finally {
      setRegistrationLoading(false);
    }
  };

  const setAppContext = async (organizationId: string, branchId: string, roleId: string, roleName: string) => {
    const { data, error } = await supabase.rpc('set_app_context', {
      organization_id: organizationId,
      branch_id: branchId,
      role_id: roleId,
    });
    
    if (error) {
      console.error('Error setting app context:', error);
      throw error;
    }
  };

  const value = {
    user,
    loading,
    registrationLoading,
    register,
    setAppContext,
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};