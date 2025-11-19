'use client';

import { useEffect } from 'react';
import { SyncProvider } from '@/lib/SyncProvider';

function PWARegister() {
  useEffect(() => {
    if (
      typeof window === 'undefined' ||
      !('serviceWorker' in navigator)
    ) {
      return;
    }

    // En desarrollo, permitir registro manual si se necesita
    const shouldRegister = process.env.NODE_ENV === 'production' || 
                           process.env.NEXT_PUBLIC_ENABLE_SW === 'true';

    if (!shouldRegister) {
      return;
    }

    const registerServiceWorker = async () => {
      try {
        const registration = await navigator.serviceWorker.register('/sw.js', {
          scope: '/',
        });
        
        console.log('🔐 Service Worker registrado', registration.scope);

        // Manejar actualizaciones del Service Worker
        registration.addEventListener('updatefound', () => {
          const newWorker = registration.installing;
          
          if (newWorker) {
            newWorker.addEventListener('statechange', () => {
              if (newWorker.state === 'installed' && navigator.serviceWorker.controller) {
                // Hay una nueva versión disponible
                console.log('🔄 Nueva versión del Service Worker disponible');
                // Opcional: mostrar notificación al usuario para recargar
              }
            });
          }
        });

        // Escuchar mensajes del Service Worker
        navigator.serviceWorker.addEventListener('message', (event) => {
          if (event.data && event.data.type === 'SYNC_NOW') {
            console.log('🔄 Mensaje de sincronización recibido del SW');
            // El SyncProvider ya maneja la sincronización automática
          }
        });

        // Verificar actualizaciones periódicamente
        setInterval(() => {
          registration.update();
        }, 60000); // Cada minuto
      } catch (error) {
        console.error('❌ Error registrando Service Worker', error);
      }
    };

    registerServiceWorker();
  }, []);

  return null;
}

export default function Providers({ children }) {
  return (
    <SyncProvider>
      {children}
      <PWARegister />
    </SyncProvider>
  );
}

