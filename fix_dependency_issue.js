#!/usr/bin/env node
console.log('Attempting to fix PowerSync dependency issue...');

// Check if we can import @powersync/adapter-sql-js
try {
  const adapterSqlJs = require('@powersync/adapter-sql-js');
  console.log('Successfully imported @powersync/adapter-sql-js:', adapterSqlJs);
  if (adapterSqlJs) {
    if (adapterSqlJs.SQLJSOpenFactory) {
      console.log('Found SQLJSOpenFactory in adapter-sql-js');
    } else {
      console.log('adapter-sql-js does not expose SQLJSOpenFactory');
    }
  } else {
    console.log('@powersync/adapter-sql-js is not available');
  }
} catch (err) {
  console.error('Failed to import @powersync/adapter-sql-js:', err.message);
}

// Let's check what's actually installed
console.log('\nChecking installed PowerSync packages:');
try {
  const powersync = require('powersync');
  console.log('powersync version:', powersync.version);
  console.log('powersync exports:', Object.keys(powersync));
} catch (err) {
  console.error('Failed to import powersync:', err.message);
}

console.log('\nChecking @powersync/node:');
try {
  const powersyncNode = require('@powersync/node');
  console.log('@powersync/node version:', powersyncNode.version);
} catch (err) {
  console.error('Failed to import @powersync/node:', err.message);
}