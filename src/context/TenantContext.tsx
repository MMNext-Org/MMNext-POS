import { createContext, useContext, useState, useEffect } from 'react';
import { Organization, Branch } from '../types';

// Mock data for organizations and branches for PoC
const mockOrganizations: Organization[] = [
  { id: 'org-1', name: 'Central Café', description: 'Main location' },
  { id: 'org-2', name: 'North Branch', description: 'North side' },
];

const mockBranches: Branch[] = [
  { id: 'branch-1', organizationId: 'org-1', name: 'Main Street', address: '123 Main St', branch_code: 'BR-001' },
  { id: 'branch-2', organizationId: 'org-1', name: 'Downtown', address: '456 Downtown', branch_code: 'BR-002' },
  { id: 'branch-3', organizationId: 'org-2', name: 'North Hill', address: '789 Hill', branch_code: 'BR-003' },
];

// Extend AuthContext type to include tenant context
type AuthContextType = {
  user: any;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
  loading: boolean;
};

type TenantContextType = {
  selectedOrganization: Organization | null;
  selectedBranch: Branch | null;
  setSelectedOrganization: (org: Organization) => void;
  setSelectedBranch: (branch: Branch) => void;
  organizations: Organization[];
  branches: Branch[];
  loading: boolean;
};

const TenantContext = createContext<TenantContextType | undefined>(undefined);

export const useTenant = () => {
  const context = useContext(TenantContext);
  if (!context) {
    throw new Error('useTenant must be used within TenantProvider');
  }
  return context;
};

export const TenantProvider = ({ children }: { children: React.ReactNode }) => {
  const [selectedOrganization, setSelectedOrganization] = useState<Organization | null>(null);
  const [selectedBranch, setSelectedBranch] = useState<Branch | null>(null);
  const [loading, setLoading] = useState(true);

  // In a real app, this would fetch from Supabase or an API
  // For now, we just use mock data
  useEffect(() => {
    setLoading(false);
  }, []);

  const value = {
    selectedOrganization,
    selectedBranch,
    setSelectedOrganization,
    setSelectedBranch,
    organizations: mockOrganizations,
    branches: mockBranches,
    loading,
  };

  return (
    <TenantContext.Provider value={value}>
      {children}
    </TenantContext.Provider>
  );
};