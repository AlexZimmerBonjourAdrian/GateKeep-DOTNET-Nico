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

const getDefaultApiBase = () => {
  if (typeof window !== 'undefined' && window.location?.origin) {
    return window.location.origin;
  }

  // En producción, si no hay variable de entorno, usar la URL de producción
  // En desarrollo, usar localhost
  return process.env.NEXT_PUBLIC_API_URL || 
         (process.env.NODE_ENV === 'production' 
           ? 'https://api.zimmzimmgames.com'
           : 'http://localhost:5011');
};

const PUBLIC_API_BASE = (process.env.NEXT_PUBLIC_API_URL || '').replace(/\/$/, '');

const API_BASE = PUBLIC_API_BASE || getDefaultApiBase();

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
 * Calcula el delay para reintentos exponenciales con backoff
 * @param attemptNumber - Número de intento (empezando en 0)
 * @param baseDelay - Delay base en ms (default: 1000ms)
 * @param maxDelay - Delay máximo en ms (default: 30000ms)
 * @returns Delay en milisegundos
 */
function calculateBackoffDelay(attemptNumber: number, baseDelay = 1000, maxDelay = 30000): number {
  const exponentialDelay = baseDelay * Math.pow(2, attemptNumber);
  const jitter = Math.random() * 0.3 * exponentialDelay; // 30% de jitter
  return Math.min(exponentialDelay + jitter, maxDelay);
}

/**
 * Realiza sincronización con servidor con reintentos exponenciales
 * @param authToken - Token de autenticación
 * @param maxRetries - Número máximo de reintentos (default: 3)
 * @returns true si la sincronización fue exitosa
 */
export async function syncWithServer(authToken: string, maxRetries = 3): Promise<boolean> {
  console.log('🔄 Iniciando sincronización...');

  // Verificar conexión
  if (!isOnline()) {
    console.log('📡 Sin conexión. Pendiente para sincronizar luego.');
    return false;
  }

  let lastError: Error | null = null;

  for (let attempt = 0; attempt <= maxRetries; attempt++) {
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
          attemptCount: Number(evt.intentos) || attempt + 1,
        })),
        clientVersion: '1.0.0',
      };

      if (attempt > 0) {
        console.log(`🔄 Reintento ${attempt}/${maxRetries}...`);
      } else {
        console.log(`📤 Enviando ${syncRequest.pendingEvents.length} eventos pendientes...`);
      }

      // Enviar al servidor con timeout
      const controller = new AbortController();
      const timeoutId = setTimeout(() => controller.abort(), 30000); // 30 segundos timeout

      const response = await fetch(`${API_BASE}/api/sync`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${authToken}`,
        },
        body: JSON.stringify(syncRequest),
        signal: controller.signal,
      });

      clearTimeout(timeoutId);

      if (!response.ok) {
        // Si es un error 4xx (cliente), no reintentar
        if (response.status >= 400 && response.status < 500) {
          console.error(`❌ Error de cliente: ${response.status} ${response.statusText}`);
          return false;
        }

        // Si es un error 5xx (servidor), reintentar
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }

      const syncResponse: SyncResponse = await response.json();

      if (!syncResponse.success) {
        console.error('❌ Sincronización fallida:', syncResponse.message);
        return false;
      }

      // Procesar eventos sincronizados
      for (const processed of syncResponse.processedEvents) {
        if (processed.success) {
          markEventoAsSynced(processed.idTemporal, processed.permanentId);
          console.log(`✅ Evento sincronizado: ${processed.idTemporal} -> ${processed.permanentId || 'N/A'}`);
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
    } catch (error: any) {
      lastError = error;

      // Si es el último intento, no esperar
      if (attempt >= maxRetries) {
        console.error('❌ Error durante sincronización después de todos los reintentos:', error);
        break;
      }

      // Si es un error de aborto (timeout), verificar conexión
      if (error.name === 'AbortError') {
        console.warn('⏱️ Timeout en sincronización');
        if (!isOnline()) {
          console.log('📡 Sin conexión. Cancelando reintentos.');
          return false;
        }
      }

      // Calcular delay para el siguiente intento
      const delay = calculateBackoffDelay(attempt);
      console.log(`⏳ Esperando ${Math.round(delay / 1000)}s antes del siguiente intento...`);

      // Esperar antes del siguiente intento
      await new Promise((resolve) => setTimeout(resolve, delay));
    }
  }

  console.error('❌ Sincronización fallida después de todos los reintentos');
  return false;
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
  const intervalId = setInterval(async () => {
    if (isOnline()) {
      await syncWithServer(authToken);
    }
  }, intervalMs);

  console.log(`⏰ Sincronización periódica configurada cada ${intervalMs}ms`);

  return () => clearInterval(intervalId);
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
