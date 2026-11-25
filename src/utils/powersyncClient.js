import { createClient } from '@powersync/node';
import { SQLJSOpenFactory } from '@powersync/adapter-sql-js';

// Initialize PowerSync client
const powersyncUrl = process.env.REACT_APP_POWER_SYNC_URL || 'http://localhost:8000'; // PowerSync server URL

// Create the PowerSync client
export const powersyncClient = createClient(powersyncUrl);

// Create SQLite adapter for local database using SQLJSOpenFactory
const sqlJsOptions = {
  dbFilename: './local.db',
  dbLocation: 'browser', // This tells SQL.js to use browser storage
  debugMode: false,
};

const sqlJsOpenFactory = new SQLJSOpenFactory(sqlJsOptions);
export const sqliteAdapter = sqlJsOpenFactory.openDB();

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