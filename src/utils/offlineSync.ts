import { useEffect, useState } from 'react';

export const useOfflineSync = () => {
  const [isOnline, setIsOnline] = useState(true);

  // Listen to online/offline events to update UI state
  useEffect(() => {
    const updateOnlineStatus = () => setIsOnline(navigator.onLine);
    window.addEventListener('online', updateOnlineStatus);
    window.addEventListener('offline', updateOnlineStatus);
    // Initial check
    updateOnlineStatus();

    return () => {
      window.removeEventListener('online', updateOnlineStatus);
      window.removeEventListener('offline', updateOnlineStatus);
    };
  }, []);

  // This is where PowerSync would handle synchronization automatically
  // The SQLite adapter ensures local changes are persisted
  // When online, PowerSync automatically syncs these changes to the cloud
  // No need for manual queuing or syncing logic

  return isOnline;
};
