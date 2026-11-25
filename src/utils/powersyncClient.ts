import { PowerSyncDatabase } from '@powersync/node';
import { DatabaseSource } from '@powersync/common';
import { openDatabase } from '@powersync/shared-internals';

// Configure PowerSync worker options
const workerOptions = {
  extensionPath() {
    // Return the absolute path to the PowerSync SQLite extension
    const extensionFilename = 'powersync_x64.dll'; // This would be dynamically determined in real code
    const resolved = new URL(`../${extensionFilename}`, import.meta.url);
    return resolved.pathname;
  },
  async loadBetterSqlite3() {
    const module = await import('better-sqlite3');
    return module.default;
  }
};

// Create the database source
const databaseSource: DatabaseSource<any> = {
  database: {
    dbFilename: 'local.db',
    dbLocation: './',
    debugMode: process.env.NODE_ENV !== 'production'
  }
};

// Create the PowerSync database instance
export const powersyncDatabase = new PowerSyncDatabase({
  schema: {/* your schema definition here */},
  database: databaseSource
});

// Export functions to interact with the database
export const getLocalDatabase = () => powersyncDatabase;
export const getLocalDatabasePath = () => './local.db';

// Initialize PowerSync (can be called once at app startup)
export const initializePowerSync = async () => {
  try {
    // Initialize the database connection first
    await openDatabase(workerOptions, {
      implementation: 'node:sqlite'
    });
    
    console.log('PowerSync database initialized successfully');
  } catch (error) {
    console.error('Error initializing PowerSync:', error);
    throw error;
  }
};
