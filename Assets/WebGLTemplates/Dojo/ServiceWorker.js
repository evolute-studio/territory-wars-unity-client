#if USE_DATA_CACHING
const cacheName = {{{JSON.stringify(COMPANY_NAME + "-" + PRODUCT_NAME + "-" + PRODUCT_VERSION )}}};
const contentToCache = [
    "./",
    "index.html",
    "manifest.json",
    "Build/{{{ LOADER_FILENAME }}}",
    "Build/{{{ FRAMEWORK_FILENAME }}}",
#if USE_THREADS
    "Build/{{{ WORKER_FILENAME }}}",
#endif
    "Build/{{{ DATA_FILENAME }}}",
    "Build/{{{ CODE_FILENAME }}}",
    "TemplateData/style.css",
    "TemplateData/icons/icon-144x144.png",
    "offline.html"
];

const OFFLINE_URL = 'offline.html';
#endif

self.addEventListener('install', function (e) {
    console.log('[Service Worker] Install');
    
#if USE_DATA_CACHING
    e.waitUntil((async function () {
      const cache = await caches.open(cacheName);
      console.log('[Service Worker] Caching all: app shell and content');
      await cache.addAll(contentToCache);
    })());
#endif
});

#if USE_DATA_CACHING
self.addEventListener('fetch', function (e) {
    // Перевіряємо, чи URL використовує підтримувану схему
    if (!e.request.url.startsWith('http')) {
        return;
    }

    e.respondWith((async function () {
        try {
            const normalizedUrl = new URL(e.request.url);
            // Перевіряємо, чи запит на manifest.webmanifest
            if (normalizedUrl.pathname.endsWith('manifest.webmanifest')) {
                const manifestResponse = await caches.match('manifest.json');
                if (manifestResponse) {
                    return manifestResponse;
                }
            }

            let response = await caches.match(e.request);
            console.log(`[Service Worker] Fetching resource: ${e.request.url}`);
            
            if (response) {
                return response;
            }

            try {
                response = await fetch(e.request);
                
                // Перевіряємо, чи відповідь валідна
                if (!response || response.status !== 200 || response.type !== 'basic') {
                    return response;
                }

                const cache = await caches.open(cacheName);
                console.log(`[Service Worker] Caching new resource: ${e.request.url}`);
                cache.put(e.request, response.clone());
                return response;
            } catch (error) {
                // Якщо запит не вдався (офлайн), показуємо офлайн сторінку для навігаційних запитів
                if (e.request.mode === 'navigate') {
                    const offlineResponse = await caches.match(OFFLINE_URL);
                    if (offlineResponse) {
                        return offlineResponse;
                    }
                }
                throw error;
            }
        } catch (error) {
            console.log('[Service Worker] Error:', error);
            return new Response('Offline page not found', {
                status: 404,
                statusText: 'Not Found'
            });
        }
    })());
});
#endif