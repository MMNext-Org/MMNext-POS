import { createClient } from '@powersync/node';
import { createSQLiteAdapter } from '@powersync/node/adapters/sqlite';
import { Database } from '@powersync/node';

// Initialize PowerSync client
const powersyncUrl = process.env.REACT_APP_POWER_SYNC_URL || 'http://localhost:8000'; // PowerSync server URL

// Create the PowerSync client
export const powersyncClient = createClient(powersyncUrl);

// Create SQLite adapter for local database
export const sqliteAdapter = createSQLiteAdapter(powersyncClient, './local.db');

// Export functions to interact with the database
export const getLocalDatabase = () => sqliteAdapter;
export const getLocalDatabasePath = () => './local.db';

// Initialize the client (can be called once at app startup)
export const initializePowerSync = async () => {
  try {
    await powersyncClient.init();
    console.log('PowerSync client initialized successfully');
  } catch (error) {
    console.error('Error initializing PowerSync client:', error);
    throw error;
  }
};