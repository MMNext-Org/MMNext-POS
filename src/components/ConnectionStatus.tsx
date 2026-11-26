import { useState, useEffect } from 'react';
import { usePowerSyncStatus } from '../context/PowerSyncStatusContext';

export const ConnectionStatus = () => {
  const { status, error } = usePowerSyncStatus();
  const [errorMessage, setErrorMessage] = useState('');

  // Update error message based on PowerSync status
  useEffect(() => {
    setErrorMessage(error || '');
    return;
  }, [status, error]);

  return (
    <div 
      className="connection-status" 
      role="status" 
      aria-live="polite"
      data-testid="connection-status"
    >
      {status === 'error' ? (
        <span className="status-error">⚠️ Connection Issue</span>
      ) : (
        <span className={`status-${status}`}>
          {status === 'connecting' ? 'Connecting...' : 
           status === 'connected' ? 'Connected' : 
           status === 'disconnected' ? 'Disconnected' : 
           status === 'syncing' ? 'Syncing...' : 'Error'}
        </span>
      )}
      {error && <span className="status-error"> {errorMessage} </span>}
    </div>
  );
};