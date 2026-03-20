const CACHE = 'liri-games-v1';

const PRECACHE = [
    './',
    './index.html',
    './catch-game/index.html',
    './math-game/index.html',
    './pet-game/index.html',
    './word-game/index.html',
    './jump-game/index.html',
    './manifest.json',
    './icon-192.svg',
    './icon-512.svg'
];

// Install: pre-cache all game files
self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(CACHE)
            .then(cache => cache.addAll(PRECACHE))
            .then(() => self.skipWaiting())
    );
});

// Activate: clean up old caches
self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys().then(keys =>
            Promise.all(
                keys.filter(k => k !== CACHE).map(k => caches.delete(k))
            )
        ).then(() => self.clients.claim())
    );
});

// Fetch: cache-first, fall back to network
self.addEventListener('fetch', event => {
    event.respondWith(
        caches.match(event.request).then(cached => {
            if (cached) return cached;
            return fetch(event.request).then(response => {
                // Cache new HTML pages on the fly
                if (response.ok && event.request.url.endsWith('.html')) {
                    const clone = response.clone();
                    caches.open(CACHE).then(cache => cache.put(event.request, clone));
                }
                return response;
            });
        })
    );
});
