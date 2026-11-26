import { createContext, useContext, useState, useEffect } from 'react';

type PowerSyncStatus = {
  status: 'disconnected' | 'connecting' | 'connected' | 'syncing' | 'error';
  error?: string;
};

const PowerSyncStatusContext = createContext<PowerSyncStatus>({ 
  status: 'disconnected', 
  error: '' 
});

export const PowerSyncStatusProvider = ({ children }: { children: React.ReactNode }) => {
  const [status, setStatus] = useState<PowerSyncStatus>({ 
    status: 'disconnected', 
    error: '' 
  });

  // Initialize PowerSync connection status tracking
  useEffect(() => {
    // This would ideally listen to PowerSync connection events
    // For now, we'll simulate connection status for demonstration
    
    const simulateConnectionStatus = () => {
      // Simulate connection status changes
      const randomStatus = Math.random();
      
      let newStatus: PowerSyncStatus['status'];
      let newError: string | undefined;
      
      if (randomStatus > 0.95) {
        newStatus = 'disconnected';
        newError = 'Connection lost';
      } else if (randomStatus > 0.9) {
        newStatus = 'connecting';
      } else if (randomStatus > 0.85) {
        newStatus = 'connected';
      } else if (randomStatus > 0.8) {
        newStatus = 'syncing';
      } else {
        newStatus = 'error';
        newError = 'Connection error';
      }
      
      setStatus({
        status: newStatus,
        error: newError
      });
    };
    
    // Simulate status changes every 10 seconds
    const interval = setInterval(simulateConnectionStatus, 10000);
    simulateConnectionStatus(); // Initial check
    
    return () => clearInterval(interval);
  }, []);

  return (
    <PowerSyncStatusContext.Provider value={{ 
      status: status.status, 
      error: status.error 
    }}>
      {children}
    </PowerSyncStatusContext.Provider>
  );
};

export const usePowerSyncStatus = () => {
  const context = useContext(PowerSyncStatusContext);
  if (!context) {
    throw new Error('usePowerSyncStatus must be used within a PowerSyncStatusProvider');
  }
  return context;
};