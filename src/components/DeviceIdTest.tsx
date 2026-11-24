import { useEffect, useState } from 'react';
import { usePowerSyncConfig } from '@/context/PowerSyncContext';

export const DeviceIdTest = () => {
  const { deviceId, branchId, organizationId, branchCode } = usePowerSyncConfig();
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
    console.log('Device ID:', deviceId);
    console.log('Branch ID:', branchId);
    console.log('Branch Code:', branchCode);
    console.log('Organization ID:', organizationId);
  }, []);

  return (
    <div className="device-id-test">
      <h3>PowerSync Configuration</h3>
      <p>Device ID: {mounted ? deviceId : 'loading...'}</p>
      <p>Branch ID: {mounted ? branchId : 'loading...'}</p>
      <p>Branch Code: {mounted ? branchCode : 'loading...'}</p>
      <p>Organization ID: {mounted ? organizationId : 'loading...'}</p>
    </div>
  );
};