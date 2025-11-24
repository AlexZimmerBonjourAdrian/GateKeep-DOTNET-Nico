// Service Worker personalizado para cachear páginas y assets
// Este archivo reemplaza el sw.js generado por workbox

const CACHE_NAME = 'gatekeep-v1';
const OFFLINE_PAGE = '/offline';

// Assets estáticos a cachear
const STATIC_ASSETS = [
  '/',
  '/offline',
  '/login',
  '/manifest.json',
];

// Instalar Service Worker
self.addEventListener('install', (event) => {
  console.log('[SW] Instalando Service Worker...');
  event.waitUntil(
    caches.open(CACHE_NAME).then((cache) => {
      console.log('[SW] Cacheando assets estáticos...');
      return cache.addAll(STATIC_ASSETS).catch((err) => {
        console.warn('[SW] Error cacheando algunos assets:', err);
        // Continuar aunque falle el cacheo de algunos assets
        return Promise.resolve();
      });
    })
  );
  self.skipWaiting();
});

// Activar Service Worker
self.addEventListener('activate', (event) => {
  console.log('[SW] Activando Service Worker...');
  event.waitUntil(
    caches.keys().then((cacheNames) => {
      return Promise.all(
        cacheNames.map((cacheName) => {
          if (cacheName !== CACHE_NAME) {
            console.log('[SW] Eliminando cache antiguo:', cacheName);
            return caches.delete(cacheName);
          }
        })
      );
    })
  );
  self.clients.claim();
});

// Estrategia: NetworkFirst con fallback a Cache
// Intenta red primero, si falla usa caché
async function networkFirst(request) {
  try {
    const networkResponse = await fetch(request);
    // Si la respuesta es exitosa, actualizar caché
    if (networkResponse.ok) {
      const cache = await caches.open(CACHE_NAME);
      cache.put(request, networkResponse.clone());
    }
    return networkResponse;
  } catch (error) {
    // Si falla la red, intentar desde caché
    const cachedResponse = await caches.match(request);
    if (cachedResponse) {
      return cachedResponse;
    }
    // Si es una navegación y no hay caché, mostrar página offline
    if (request.mode === 'navigate') {
      const offlinePage = await caches.match(OFFLINE_PAGE);
      if (offlinePage) {
        return offlinePage;
      }
    }
    throw error;
  }
}

// Estrategia: CacheFirst para assets estáticos
async function cacheFirst(request) {
  const cachedResponse = await caches.match(request);
  if (cachedResponse) {
    return cachedResponse;
  }
  try {
    const networkResponse = await fetch(request);
    if (networkResponse.ok) {
      const cache = await caches.open(CACHE_NAME);
      cache.put(request, networkResponse.clone());
    }
    return networkResponse;
  } catch (error) {
    throw error;
  }
}

// Interceptar peticiones
self.addEventListener('fetch', (event) => {
  const { request } = event;
  const url = new URL(request.url);

  // Ignorar peticiones a APIs (no cachear)
  if (url.pathname.startsWith('/api/')) {
    return; // Dejar que pase sin interceptar
  }

  // Ignorar peticiones a _next (assets de Next.js se manejan por separado)
  if (url.pathname.startsWith('/_next/')) {
    event.respondWith(cacheFirst(request));
    return;
  }

  // Para páginas HTML, usar NetworkFirst
  if (request.headers.get('accept')?.includes('text/html') || request.mode === 'navigate') {
    event.respondWith(networkFirst(request));
    return;
  }

  // Para assets estáticos (JS, CSS, imágenes), usar CacheFirst
  if (
    request.destination === 'script' ||
    request.destination === 'style' ||
    request.destination === 'image' ||
    request.destination === 'font'
  ) {
    event.respondWith(cacheFirst(request));
    return;
  }

  // Para todo lo demás, usar NetworkFirst
  event.respondWith(networkFirst(request));
});

// Manejar mensajes del cliente
self.addEventListener('message', (event) => {
  if (event.data && event.data.type === 'SKIP_WAITING') {
    self.skipWaiting();
  }
  if (event.data && event.data.type === 'SYNC_NOW') {
    // Notificar a todos los clientes para sincronizar
    event.waitUntil(
      self.clients.matchAll().then((clients) => {
        clients.forEach((client) => {
          client.postMessage({ type: 'SYNC_NOW' });
        });
      })
    );
  }
});

