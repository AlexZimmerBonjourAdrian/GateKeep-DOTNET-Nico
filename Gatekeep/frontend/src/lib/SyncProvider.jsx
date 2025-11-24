/**
* Ejemplo de integración de sincronización offline
* Agregado en _app.jsx o layout.js
*/

'use client';

import { useEffect } from 'react';
import { setupConnectivityListeners, startPeriodicSync, getDeviceId } from '@/lib/sync';
import { startMasterDataSync } from '@/lib/master-data-sync';
import { initializeDatabase } from '@/lib/sqlite-db';

export function SyncProvider({ children }) {
  useEffect(() => {
    let stopPeriodicSync;
    let stopMasterSync;

    const initializeSync = async () => {
      if (typeof window === 'undefined') {
        return;
      }

      console.log('🚀 Inicializando sistema de sincronización...');
      
      // Inicializar base de datos SQLite
      await initializeDatabase();

      // Obtener token de autenticación
      const authToken = window.localStorage.getItem('token') || 
                       window.localStorage.getItem('authToken');
      
      if (!authToken) {
        console.warn('⚠️ No hay token de autenticación. Sincronización deshabilitada.');
        return;
      }

      // Configurar listeners de conectividad (con retraso de 2 minutos)
      setupConnectivityListeners(authToken);
      
      // Iniciar sincronización periódica (cada 1 minuto si online)
      stopPeriodicSync = startPeriodicSync(authToken, 60000);
      
      // Iniciar sincronización de datos maestros (cada 1 minuto)
      stopMasterSync = startMasterDataSync(authToken, 60000);

      console.log(`📱 Dispositivo ID: ${getDeviceId()}`);
      console.log('✅ Sistema de sincronización inicializado');
    };

    initializeSync();

    // Cleanup al desmontar
    return () => {
      if (typeof stopPeriodicSync === 'function') {
        stopPeriodicSync();
      }
      if (typeof stopMasterSync === 'function') {
        stopMasterSync();
      }
    };
  }, []);

  return <>{children}</>;
}

// Uso en _app.jsx:
// export default function App({ Component, pageProps }) {
//   return (
//     <SyncProvider>
//       <Component {...pageProps} />
//     </SyncProvider>
//   );
// }
