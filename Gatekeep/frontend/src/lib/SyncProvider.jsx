/**
 * Ejemplo de integración de sincronización offline
 * Agregado en _app.jsx o layout.js
 */

import { useEffect } from 'react';
import { setupConnectivityListeners, startPeriodicSync, getDeviceId } from '@/lib/sync';
import { initializeDatabase } from '@/lib/sqlite-db';
import SyncStatus from '@/components/SyncStatus';

export function SyncProvider({ children }) {
  useEffect(() => {
    const initializeSync = async () => {
      // 1. Inicializar BD local SQLite
      console.log('🚀 Inicializando sistema de sincronización...');
      await initializeDatabase();

      // 2. Obtener token de autenticación (ajustar según tu autenticación)
      const authToken = localStorage.getItem('authToken');
      if (!authToken) {
        console.warn('⚠️ No hay token de autenticación. Sincronización deshabilitada.');
        return;
      }

      // 3. Configurar listeners de conectividad
      setupConnectivityListeners(authToken);

      // 4. Iniciar sincronización periódica (cada 30 segundos)
      startPeriodicSync(authToken, 30000);

      // 5. Log de dispositivo
      console.log(`📱 Dispositivo ID: ${getDeviceId()}`);
    };

    initializeSync();
  }, []);

  return (
    <>
      {children}
      <SyncStatus />
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
