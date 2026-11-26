import { useTenant } from '../context/TenantContext';
import type { Branch } from '../types';
import { LoadingSpinner } from '@/components/ui/LoadingSpinner';

export const TenantSelector = () => {
  const {
    selectedOrganization,
    setSelectedOrganization,
    setSelectedBranch,
    organizations,
    branches,
    loading,
  } = useTenant();

  if (loading) {
    return <LoadingSpinner message="Loading tenant data…" />;
  }

  return (
    <div className="max-w-md mx-auto p-4 text-center border border-gray-200 rounded-lg shadow-sm hover:shadow-md transition-shadow">
      <div className="flex flex-col gap-4 items-center">
        <select
          onChange={(e) => {
            const id = e.target.value;
            const org = organizations.find((o) => o.id === id);
            if (org) setSelectedOrganization(org);
          }}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-gray-500 focus:outline-none"
        >
          <option value="">-- Select organization --</option>
          {organizations.map((org) => (
            <option key={org.id} value={org.id}>
              {org.name} - {org.description}
            </option>
          ))}
        </select>

        <select
          onChange={(e) => {
            const id = e.target.value;
            const branch = branches.find((b) => b.id === id);
            if (branch) setSelectedBranch(branch);
          }}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-gray-500 focus:outline-none"
          disabled={!selectedOrganization}
        >
          <option value="">-- Select branch --</option>
          {selectedOrganization
            ? branches
                .filter((b) => b.organizationId === selectedOrganization.id)
                .map((branch) => (
                  <option key={branch.id} value={branch.id}>
                    {branch.name} ({branch.address})
                  </option>
                ))
            : branches.map((branch: Branch) => (
                <option key={branch.id} value={branch.id}>
                  {branch.name} (org: {branch.organizationId})
                </option>
              ))}
        </select>
      </div>
    </div>
  );
};