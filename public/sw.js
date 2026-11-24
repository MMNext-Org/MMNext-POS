const CACHE_NAME = 'brewflow-pos-cache-v1';
const OFFLINE_ORDERS_KEY = 'offline_orders';

// Cache core assets
self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then((cache) => {
        return cache.addAll([
          '/',
          '/index.html',
          '/favicon.png',
          '/manifest.json',
          '/sw.js',
          '/_next/static/chunks/main.79d78f76.js',
          '/_next/static/chunks/vendor.7a9c1877.js'
        ]);
      })
  );
});

// Install event is already defined above

self.addEventListener('fetch', (event) => {
  // For order creation, we'll handle it specially in the main app
  // This service worker mainly caches static assets
  
  // Handle static assets
  if (event.request.url.includes('sw.js') || 
      event.request.url.includes('manifest.json') ||
      event.request.url.endsWith('.png') ||
          event.request.url.endsWith('.ico')) {
    event.respondWith(caches.match(event.request));
    return;
  }
  
  // For API requests, let the browser handle them normally
  // The main app will handle offline state and queuing
});