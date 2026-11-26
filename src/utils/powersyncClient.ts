// Browser-safe PowerSync client.
//
// PowerSync is initialized lazily and failure-tolerant so that app boot
// never crashes when the PowerSync service/backend is unavailable (e.g.
// during local development). No Node-only modules are imported at the top
// level, so this module resolves cleanly in the browser/Vite.
//
// Export surface (kept stable for callers such as App.tsx):
//   initializePowerSync()  - attempts PowerSync init; never throws
//   getLocalDatabase()     - returns the local database instance or null
//   getLocalDatabasePath() - returns the local database file path

const LOCAL_DB_PATH = './local.db';

let _database: { readonly connected: boolean } | null = null;
let _initPromise: Promise<void> | null = null;

export const getLocalDatabasePath = (): string => LOCAL_DB_PATH;

export const getLocalDatabase = (): { readonly connected: boolean } | null =>
  _database;

/**
 * Initialize PowerSync once. Safe to call multiple times; subsequent calls
 * reuse the same in-flight promise. Resolves even when PowerSync cannot be
 * initialized (no browser-capable SDK or no backend), so app boot is not
 * blocked by PowerSync failures.
 */
export const initializePowerSync = async (): Promise<void> => {
  if (_initPromise) {
    return _initPromise;
  }

  _initPromise = (async () => {
    try {
      // A browser-capable PowerSync SDK is not installed in this project
      // (@powersync/node is Node-only). Defer to a graceful no-op so the
      // app keeps working offline without a local database attached.
      console.log(
        'PowerSync: browser-safe no-op init; no local database attached',
      );
      _database = { connected: false };
    } catch (error) {
      // Never throw: a PowerSync failure must not break app boot.
      console.error('PowerSync initialization failed (non-fatal):', error);
      _database = { connected: false };
    }
  })();

  return _initPromise;
};
