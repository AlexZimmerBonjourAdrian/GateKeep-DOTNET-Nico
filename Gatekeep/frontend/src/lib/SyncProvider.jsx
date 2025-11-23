/**
* Ejemplo de integración de sincronización offline
* Agregado en _app.jsx o layout.js
*/

'use client';

import { useEffect } from 'react';
import { setupConnectivityListeners, startPeriodicSync, getDeviceId } from '@/lib/sync';
import { initializeDatabase } from '@/lib/sqlite-db';

export function SyncProvider({ children }) {
  useEffect(() => {
    let stopPeriodicSync = undefined;

    const initializeSync = async () => {
      if (typeof window === 'undefined') {
        return;
      }

      console.log('🚀 Inicializando sistema de sincronización...');
      await initializeDatabase();

      const authToken = window.localStorage.getItem('authToken');
      if (!authToken) {
        console.warn('⚠️ No hay token de autenticación. Sincronización deshabilitada.');
        return;
      }

      setupConnectivityListeners(authToken);
      stopPeriodicSync = startPeriodicSync(authToken, 30000);

      console.log(`📱 Dispositivo ID: ${getDeviceId()}`);
    };

    initializeSync();

    return () => {
      if (typeof stopPeriodicSync === 'function') {
        stopPeriodicSync();
      }
    };
  }, []);

  return (
    <>
      {children}
    </>
  );
}

// Uso en _app.jsx:
// export default function App({ Component, pageProps }) {
//   return (
//     <SyncProvider>
//       <Component {...pageProps} />
//     </SyncProvider>
//   );
// }
