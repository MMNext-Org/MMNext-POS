import { createContext, useContext } from 'react';
import { useTenant } from './TenantContext';
import { getDeviceId } from '@/utils/deviceId';

type PowerSyncConfig = {
  deviceId: string;
  branchId: string | null;
  organizationId: string | null;
  branchCode: string | null;
};

const PowerSyncContext = createContext<PowerSyncConfig | undefined>(undefined);

export const usePowerSyncConfig = () => {
  const context = useContext(PowerSyncContext);
  if (!context) {
    throw new Error('usePowerSyncConfig must be used within a PowerSyncProvider');
  }
  return context;
};

export const PowerSyncProvider = ({ children }: { children: React.ReactNode }) => {
  const { selectedOrganization, selectedBranch } = useTenant();
  const deviceId = getDeviceId();

  const config: PowerSyncConfig = {
    deviceId,
    branchId: selectedBranch?.id ?? null,
    organizationId: selectedOrganization?.id ?? null,
      branchCode: selectedBranch?.branch_code ?? null,
  };

  return (
    <PowerSyncContext.Provider value={config}>
      {children}
    </PowerSyncContext.Provider>
  );
};