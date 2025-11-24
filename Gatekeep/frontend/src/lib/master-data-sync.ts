import { 
  syncDataFromServer,
  getSyncMetadata,
  setSyncMetadata 
} from './sqlite-db';
import { isOnline } from './sync';
import { URLService } from '@/services/urlService';

const API_BASE = URLService.getBaseUrl(); // Sin /api/

/**
 * Descarga y cachea datos maestros del servidor
 * @param authToken - Token de autenticación
 * @returns true si la sincronización fue exitosa
 */
export async function syncMasterData(authToken: string): Promise<boolean> {
  if (!isOnline()) {
    console.log('📡 Sin conexión. No se pueden sincronizar datos maestros.');
    return false;
  }

  try {
    console.log('🔄 Sincronizando datos maestros...');
    
    // Obtener última sincronización
    const lastSync = getSyncMetadata('ultimaSincronizacionMaster');
    
    // Construir headers
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${authToken}`,
    };

    // Descargar todos los datos maestros en paralelo
    const [usuariosRes, espaciosRes, beneficiosRes, notificacionesRes, reglasRes] = await Promise.all([
      fetch(`${API_BASE}/api/usuarios`, { headers }),
      fetch(`${API_BASE}/api/espacios`, { headers }),
      fetch(`${API_BASE}/api/beneficios`, { headers }),
      fetch(`${API_BASE}/api/notificaciones`, { headers }),
      fetch(`${API_BASE}/api/reglas-acceso`, { headers }),
    ]);

    // Verificar respuestas
    if (!usuariosRes.ok || !espaciosRes.ok || !beneficiosRes.ok) {
      throw new Error('Error descargando datos maestros');
    }

    // Parsear respuestas
    const usuariosData = await usuariosRes.json();
    const espaciosData = await espaciosRes.json();
    const beneficiosData = await beneficiosRes.json();
    const notificacionesData = notificacionesRes.ok ? await notificacionesRes.json() : { data: [] };
    const reglasData = reglasRes.ok ? await reglasRes.json() : { data: [] };

    // Extraer arrays de datos (pueden venir como { data: [...] } o directamente como array)
    const usuarios = Array.isArray(usuariosData) ? usuariosData : (usuariosData.data || []);
    const espacios = Array.isArray(espaciosData) ? espaciosData : (espaciosData.data || []);
    const beneficios = Array.isArray(beneficiosData) ? beneficiosData : (beneficiosData.data || []);
    const notificaciones = Array.isArray(notificacionesData) ? notificacionesData : (notificacionesData.data || []);
    const reglas = Array.isArray(reglasData) ? reglasData : (reglasData.data || []);

    // Guardar en SQLite local
    syncDataFromServer({
      usuarios: usuarios.map((u: any) => ({
        ...u,
        ultimaActualizacion: new Date().toISOString(),
      })),
      espacios: espacios.map((e: any) => ({
        ...e,
        ultimaActualizacion: new Date().toISOString(),
      })),
      beneficios: beneficios.map((b: any) => ({
        ...b,
        ultimaActualizacion: new Date().toISOString(),
      })),
      notificaciones: notificaciones.map((n: any) => ({
        ...n,
        fechaCreacion: n.fechaCreacion || new Date().toISOString(),
      })),
      reglasAcceso: reglas.map((r: any) => ({
        ...r,
        ultimaActualizacion: new Date().toISOString(),
      })),
    });

    // Actualizar timestamp
    setSyncMetadata('ultimaSincronizacionMaster', new Date().toISOString());
    
    console.log(`✅ Datos maestros sincronizados: ${usuarios.length} usuarios, ${espacios.length} espacios, ${beneficios.length} beneficios`);
    return true;
  } catch (error: any) {
    console.error('❌ Error sincronizando datos maestros:', error);
    return false;
  }
}

/**
 * Inicia sincronización periódica de datos maestros
 * @param authToken - Token de autenticación
 * @param intervalMs - Intervalo en milisegundos (default: 5 minutos)
 * @returns Función para detener la sincronización
 */
export function startMasterDataSync(authToken: string, intervalMs = 300000): () => void {
  // Sincronizar inmediatamente si hay conexión
  if (isOnline()) {
    syncMasterData(authToken).catch(err => {
      console.error('Error en sincronización inicial de datos maestros:', err);
    });
  }

  // Luego sincronizar periódicamente
  const intervalId = setInterval(() => {
    if (isOnline()) {
      syncMasterData(authToken).catch(err => {
        console.error('Error en sincronización periódica de datos maestros:', err);
      });
    }
  }, intervalMs);

  console.log(`⏰ Sincronización de datos maestros cada ${intervalMs / 1000 / 60} minutos`);

  return () => {
    clearInterval(intervalId);
    console.log('🛑 Sincronización de datos maestros detenida');
  };
}

