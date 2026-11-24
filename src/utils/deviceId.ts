import { v4 as uuidv4 } from 'uuid';

export const getDeviceId = () => {
  const stored = localStorage.getItem('power_sync_device_id');
  if (stored) return stored;
  
  const newId = uuidv4();
  localStorage.setItem('power_sync_device_id', newId);
  return newId;
};