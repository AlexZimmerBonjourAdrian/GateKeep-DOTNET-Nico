/**
 * Cliente de sincronización offline para PWA
 * Detecta conectividad y sincroniza eventos offline con servidor
 */

import {
  initializeDatabase,
  getPendingOfflineEvents,
  markEventoAsSynced,
  syncDataFromServer,
  setSyncMetadata,
  getSyncMetadata,
  clearProcessedEvents,
} from './sqlite-db';

const API_BASE = typeof window !== 'undefined' 
  ? (window as any).__NEXT_PUBLIC_API_URL__ || 'http://localhost:5011'
  : 'http://localhost:5011';

/**
 * Estructura de evento offline compatible con backend
 */
interface OfflineEventDto {
  idTemporal: string;
  eventType: string;
  eventData: string;
  createdAt: string;
  attemptCount: number;
}

/**
 * Request de sincronización
 */
interface SyncRequest {
  deviceId: string;
  lastSyncTime: string | null;
  pendingEvents: OfflineEventDto[];
  clientVersion: string;
}

/**
 * Response del servidor
 */
interface SyncResponse {
  serverTime: string;
  success: boolean;
  message?: string;
  processedEvents: Array<{
    idTemporal: string;
    success: boolean;
    permanentId?: string;
    errorMessage?: string;
    processedAt: string;
  }>;
  dataToSync?: {
    usuarios: any[];
    espacios: any[];
    reglasAcceso: any[];
    beneficios: any[];
    notificaciones: any[];
  };
  newAuthToken?: string;
  lastSuccessfulSync: string;
}

/**
 * Obtiene ID único del dispositivo (fingerprint)
 */
export function getDeviceId(): string {
  const storedId = localStorage.getItem('gatekeep_device_id');
  if (storedId) return storedId;

  const newId = `device-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  localStorage.setItem('gatekeep_device_id', newId);
  return newId;
}

/**
 * Detecta si hay conexión a internet
 */
export function isOnline(): boolean {
  return navigator.onLine;
}

/**
 * Realiza sincronización con servidor
 */
export async function syncWithServer(authToken: string): Promise<boolean> {
  console.log('🔄 Iniciando sincronización...');

  // Verificar conexión
  if (!isOnline()) {
    console.log('📡 Sin conexión. Pendiente para sincronizar luego.');
    return false;
  }

  try {
    // Inicializar BD si no existe
    await initializeDatabase();

    // Obtener eventos pendientes
    const pendingEvents = getPendingOfflineEvents();
    const lastSync = getSyncMetadata('ultimaSincronizacion');

    // Construir request
    const syncRequest: SyncRequest = {
      deviceId: getDeviceId(),
      lastSyncTime: lastSync || null,
      pendingEvents: pendingEvents.map((evt) => ({
        idTemporal: String(evt.idTemporal || ''),
        eventType: String(evt.tipoEvento || ''),
        eventData: String(evt.datosEvento || ''),
        createdAt: String(evt.fechaCreacion || ''),
        attemptCount: Number(evt.intentos) || 1,
      })),
      clientVersion: '1.0.0',
    };

    console.log(`📤 Enviando ${syncRequest.pendingEvents.length} eventos pendientes...`);

    // Enviar al servidor
    const response = await fetch(`${API_BASE}/api/sync`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${authToken}`,
      },
      body: JSON.stringify(syncRequest),
    });

    if (!response.ok) {
      console.error(`❌ Error de sync: ${response.status} ${response.statusText}`);
      return false;
    }

    const syncResponse: SyncResponse = await response.json();

    if (!syncResponse.success) {
      console.error('❌ Sincronización fallida:', syncResponse.message);
      return false;
    }

    // Procesar eventos sincronizados
    for (const processed of syncResponse.processedEvents) {
      if (processed.success) {
        markEventoAsSynced(processed.idTemporal);
        console.log(`✅ Evento sincronizado: ${processed.idTemporal}`);
      } else {
        console.error(
          `❌ Error en evento ${processed.idTemporal}: ${processed.errorMessage}`
        );
      }
    }

    // Sincronizar datos descargados
    if (syncResponse.dataToSync) {
      syncDataFromServer(syncResponse.dataToSync);
    }

    // Actualizar metadata
    setSyncMetadata('ultimaSincronizacion', new Date().toISOString());
    setSyncMetadata('lastSuccessfulSync', syncResponse.lastSuccessfulSync);

    // Limpiar eventos procesados
    clearProcessedEvents();

    // Renovar token si es necesario
    if (syncResponse.newAuthToken) {
      localStorage.setItem('authToken', syncResponse.newAuthToken);
    }

    console.log('✅ Sincronización completada exitosamente');
    return true;
  } catch (error) {
    console.error('❌ Error durante sincronización:', error);
    return false;
  }
}

/**
 * Configura listeners de conectividad
 */
export function setupConnectivityListeners(authToken: string) {
  window.addEventListener('online', async () => {
    console.log('🌐 Conexión recuperada. Sincronizando...');
    await syncWithServer(authToken);
  });

  window.addEventListener('offline', () => {
    console.log('📡 Modo offline activado');
  });
}

/**
 * Inicia sincronización periódica (cada 30 segundos si online)
 */
export function startPeriodicSync(authToken: string, intervalMs = 30000) {
  setInterval(async () => {
    if (isOnline()) {
      await syncWithServer(authToken);
    }
  }, intervalMs);

  console.log(`⏰ Sincronización periódica configurada cada ${intervalMs}ms`);
}

/**
 * Registra un evento para sincronización offline
 */
export async function recordEvent(tipoEvento: string, datosEvento: any) {
  const { recordOfflineEvent } = await import('./sqlite-db');
  const idTemporal = recordOfflineEvent(tipoEvento, datosEvento);
  console.log(`📝 Evento registrado offline: ${tipoEvento}`);
  return idTemporal;
}
