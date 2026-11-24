import { useTenant } from '../context/TenantContext';
import { Select } from '@chakra-ui/react';
import { Box, VStack } from '@chakra-ui/react';
import { Organization, Branch } from './types';

export const TenantSelector = () => {
  const {
    selectedOrganization,
    selectedBranch,
    setSelectedOrganization,
    setSelectedBranch,
    organizations,
    branches,
    loading,
  } = useTenant();

  if (loading) {
    return <div>Loading tenant data...</div>;
  }

  return (
    <Box
      maxWidth="md"
      mx="auto"
      p={4}
      textAlign="center"
      borderWidth={1}
      borderRadius="md"
      _hover={{ boxShadow: 'lg' }}
    >
      <VStack spacing={2}>
        <Select
          placeholder="Select organization..."
          onChange={(value) => setSelectedOrganization(value)}
          width="100%"
          data-testid="org-select"
        >
          {organizations.map((org) => (
            <option key={org.id} value={org}>
              {org.name} - {org.description}
            </option>
          ))}
        </Select>

        <Select
          placeholder="Select branch..."
          onChange={(value) => setSelectedBranch(value)}
          width="100%"
          disabled={!selectedOrganization}
          data-testid="branch-select"
        >
          {selectedOrganization
            ? (branches
                .filter((b) => b.organizationId === selectedOrganization.id)
                .map((branch) => (
                  <option key={branch.id} value={branch}>
                    {branch.name} ({branch.address})
                  </option>
                ))
            : (
              branches.map((branch) => (
                <option key={branch.id} value={branch}>
                  {branch.name} (org: {branch.organizationId})
                </option>
              ))
            )}
        </Select>
      </VStack>
    </Box>
  );
};