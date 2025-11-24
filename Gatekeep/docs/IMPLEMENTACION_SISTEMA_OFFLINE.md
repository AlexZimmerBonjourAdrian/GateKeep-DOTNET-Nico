# 📱 Documentación: Implementación del Sistema Offline Completo

## 📋 Tabla de Contenidos

1. [Resumen Ejecutivo](#resumen-ejecutivo)
2. [Arquitectura Actual](#arquitectura-actual)
3. [Mejoras a Implementar](#mejoras-a-implementar)
4. [Guía de Implementación Paso a Paso](#guía-de-implementación-paso-a-paso)
5. [Archivos a Crear/Modificar](#archivos-a-crear-modificar)
6. [Pruebas y Validación](#pruebas-y-validación)
7. [Troubleshooting](#troubleshooting)

---

## 🎯 Resumen Ejecutivo

Este documento describe cómo implementar un sistema completo de sincronización offline que permite:

- ✅ **Interceptar automáticamente** todas las llamadas API y guardarlas offline cuando no hay conexión
- ✅ **Mostrar estado de sincronización** al usuario (eventos pendientes, estado de conexión)
- ✅ **Cachear datos maestros** periódicamente cuando hay conexión
- ✅ **Sincronizar automáticamente** 2 minutos después de recuperar la conexión

### Flujo de Trabajo

```
Usuario crea algo → ¿Hay conexión?
├─ SÍ → Enviar al servidor directamente
└─ NO → Guardar en SQLite local
         ↓
    Cuando vuelve conexión
         ↓
    Esperar 2 minutos
         ↓
    Sincronizar automáticamente
         ↓
    Actualizar estado en UI
```

---

## 🏗️ Arquitectura Actual

### Componentes Existentes

1. **SQLite Local** (`lib/sqlite-db.ts`)
   - Base de datos en memoria usando `sql.js`
   - Persistencia en IndexedDB
   - Tablas: usuarios, espacios, eventos_offline, etc.

2. **Sistema de Sincronización** (`lib/sync.ts`)
   - Detecta conectividad
   - Sincroniza eventos pendientes
   - Reintentos exponenciales

3. **SyncProvider** (`lib/SyncProvider.jsx`)
   - Inicializa el sistema
   - Configura listeners de conectividad

### Problemas Actuales

❌ Las llamadas API no se interceptan automáticamente  
❌ No hay indicador visual de eventos pendientes  
❌ Los datos maestros no se cachean automáticamente  
❌ La sincronización ocurre inmediatamente al recuperar conexión

---

## 🚀 Mejoras a Implementar

### 1. Interceptor Automático de Llamadas API

**Objetivo:** Capturar automáticamente todas las peticiones HTTP y guardarlas offline cuando no hay conexión.

**Beneficios:**
- No requiere modificar cada servicio manualmente
- Funciona transparentemente para todas las llamadas
- Reduce errores y código duplicado

### 2. Indicador de Estado de Sincronización

**Objetivo:** Mostrar al usuario cuántos eventos están pendientes y el estado de la conexión.

**Beneficios:**
- Mejor experiencia de usuario
- Transparencia sobre el estado de sincronización
- Confianza en que los datos se guardarán

### 3. Cacheo Automático de Datos Maestros

**Objetivo:** Descargar y cachear usuarios, espacios, beneficios, etc. periódicamente cuando hay conexión.

**Beneficios:**
- La app funciona offline con datos actualizados
- Mejor rendimiento (menos llamadas al servidor)
- Datos disponibles incluso sin conexión

### 4. Retraso de 2 Minutos en Sincronización

**Objetivo:** Esperar 2 minutos después de recuperar conexión antes de sincronizar.

**Beneficios:**
- Evita saturar el servidor con múltiples reconexiones
- Permite que la conexión se estabilice
- Reduce consumo de batería

---

## 📝 Guía de Implementación Paso a Paso

### Paso 1: Crear Interceptor de Axios para Offline

**Archivo:** `frontend/src/lib/axios-offline-interceptor.ts`

```typescript
import axios, { AxiosInstance, AxiosError, InternalAxiosRequestConfig } from 'axios';
import { recordEvent } from './sync';
import { isOnline } from './sync';
import { URLService } from '@/services/urlService';

// Crear una instancia base de axios con configuración global
const apiClient: AxiosInstance = axios.create({
  baseURL: URLService.getLink(), // Incluye /api/
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 30000, // 30 segundos
});

// INTERCEPTOR DE REQUEST: Agregar token y detectar offline
apiClient.interceptors.request.use(
  async (config: InternalAxiosRequestConfig) => {
    // Agregar token de autenticación
    const token = localStorage.getItem('token');
    if (token) {
      const h: any = config.headers ?? {};
      if (typeof h.set === 'function') {
        h.set('Authorization', `Bearer ${token}`);
      } else {
        h['Authorization'] = `Bearer ${token}`;
      }
      (config as any).headers = h;
    }

    // Si no hay conexión y es una petición que modifica datos (POST, PUT, DELETE)
    if (!isOnline() && config.method && ['post', 'put', 'delete', 'patch'].includes(config.method.toLowerCase())) {
      const offlineData = {
        url: config.url,
        method: config.method.toUpperCase(),
        data: config.data,
        headers: config.headers,
        baseURL: config.baseURL,
      };

      // Guardar en SQLite para sincronizar después
      try {
        await recordEvent('api_request', offlineData);
        console.log('📝 Petición guardada offline:', config.method?.toUpperCase(), config.url);
      } catch (error) {
        console.error('❌ Error guardando petición offline:', error);
      }

      // Rechazar la petición pero devolver un error especial
      return Promise.reject({
        isOffline: true,
        message: 'Sin conexión. La petición se guardó para sincronizar después.',
        offlineData,
        config,
      } as AxiosError);
    }

    // Si hay conexión, continuar normalmente
    return config;
  },
  (error: AxiosError) => Promise.reject(error)
);

// INTERCEPTOR DE RESPONSE: Capturar errores de red y guardar offline
apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    // Si es un error de red (sin conexión) y no es GET
    if (
      !error.response &&
      error.config &&
      error.config.method &&
      !['get', 'head', 'options'].includes(error.config.method.toLowerCase()) &&
      !(error as any).isOffline // Evitar duplicados
    ) {
      const offlineData = {
        url: error.config.url,
        method: error.config.method?.toUpperCase(),
        data: error.config.data,
        headers: error.config.headers,
        baseURL: error.config.baseURL,
      };

      try {
        await recordEvent('api_request', offlineData);
        console.log('📝 Petición guardada offline (error de red):', error.config.method?.toUpperCase(), error.config.url);
      } catch (saveError) {
        console.error('❌ Error guardando petición offline:', saveError);
      }

      return Promise.reject({
        ...error,
        isOffline: true,
        message: 'Error de red. La petición se guardó para sincronizar después.',
        offlineData,
      } as AxiosError);
    }

    return Promise.reject(error);
  }
);

export default apiClient;
```

**Notas:**
- Solo guarda offline peticiones que modifican datos (POST, PUT, DELETE, PATCH)
- Las peticiones GET no se guardan porque son solo lectura
- El error `isOffline: true` permite que los componentes sepan que se guardó offline

---

### Paso 2: Modificar Servicios para Usar el Interceptor

**Archivos a modificar:**
- `frontend/src/services/UsuarioService.ts`
- `frontend/src/services/AccesoService.ts`
- `frontend/src/services/BeneficioService.ts`
- `frontend/src/services/EdificioService.ts`
- `frontend/src/services/SalonService.ts`
- `frontend/src/services/EventoService.ts`
- `frontend/src/services/AnuncioService.ts`
- `frontend/src/services/ReglaAccesoService.ts`
- `frontend/src/services/NotificacionService.ts`

**Ejemplo de modificación en `UsuarioService.ts`:**

```typescript
// ANTES:
import axios, { AxiosInstance } from "axios";
const api: AxiosInstance = axios.create({
  baseURL: API_URL,
  headers: { "Content-Type": "application/json" },
});

// DESPUÉS:
import apiClient from '@/lib/axios-offline-interceptor';
// Eliminar la creación de instancia axios local
// Usar apiClient directamente en lugar de api
```

**Cambios específicos:**

1. **Reemplazar import:**
```typescript
// Eliminar:
import axios, { AxiosInstance } from "axios";

// Agregar:
import apiClient from '@/lib/axios-offline-interceptor';
```

2. **Eliminar creación de instancia:**
```typescript
// Eliminar todo esto:
const api: AxiosInstance = axios.create({
  baseURL: API_URL,
  headers: { "Content-Type": "application/json" },
});

api.interceptors.request.use((config) => {
  // ... código de token ...
});
```

3. **Reemplazar `api` por `apiClient` en todos los métodos:**
```typescript
// ANTES:
return api.get(USUARIOS_URL);

// DESPUÉS:
return apiClient.get(USUARIOS_URL);
```

4. **Manejar errores offline:**
```typescript
try {
  const response = await apiClient.put(`${USUARIOS_URL}${id}`, data);
  return response.data;
} catch (error: any) {
  if (error.isOffline) {
    // Ya se guardó offline, mostrar mensaje amigable
    throw new Error('Cambios guardados offline. Se sincronizarán cuando haya conexión.');
  }
  throw error;
}
```

---

### Paso 3: Crear Componente de Estado de Sincronización

**Archivo:** `frontend/src/components/SyncStatusBadge.jsx`

```jsx
'use client';

import { useState, useEffect } from 'react';
import { contarEventosPendientes, getOfflineStatus } from '@/lib/sqlite-db';
import { isOnline } from '@/lib/sync';

export function SyncStatusBadge() {
  const [pendingCount, setPendingCount] = useState(0);
  const [isOnlineState, setIsOnlineState] = useState(true);
  const [lastSync, setLastSync] = useState(null);
  const [isVisible, setIsVisible] = useState(false);

  useEffect(() => {
    // Función para actualizar estado
    const updateStatus = () => {
      try {
        const count = contarEventosPendientes();
        const status = getOfflineStatus();
        const online = isOnline();
        
        setPendingCount(count);
        setIsOnlineState(online);
        setLastSync(status.ultimaSincronizacion);
        
        // Mostrar badge si hay eventos pendientes o está offline
        setIsVisible(count > 0 || !online);
      } catch (error) {
        console.error('Error actualizando estado de sincronización:', error);
      }
    };

    // Actualizar inmediatamente
    updateStatus();

    // Actualizar cada 5 segundos
    const interval = setInterval(updateStatus, 5000);

    // Escuchar cambios de conexión
    const handleOnline = () => {
      setIsOnlineState(true);
      updateStatus();
    };
    
    const handleOffline = () => {
      setIsOnlineState(false);
      setIsVisible(true);
    };
    
    window.addEventListener('online', handleOnline);
    window.addEventListener('offline', handleOffline);

    return () => {
      clearInterval(interval);
      window.removeEventListener('online', handleOnline);
      window.removeEventListener('offline', handleOffline);
    };
  }, []);

  // No mostrar nada si no hay eventos pendientes y está online
  if (!isVisible) {
    return null;
  }

  return (
    <div className="sync-status-badge" style={styles.container}>
      {!isOnlineState && (
        <div className="offline-indicator" style={styles.offline}>
          📡 Sin conexión
        </div>
      )}
      
      {pendingCount > 0 && (
        <div className="pending-events" style={styles.pending}>
          ⏳ {pendingCount} evento{pendingCount > 1 ? 's' : ''} pendiente{pendingCount > 1 ? 's' : ''}
        </div>
      )}
      
      {lastSync && isOnlineState && (
        <div className="last-sync" style={styles.lastSync}>
          Última sincronización: {new Date(lastSync).toLocaleTimeString()}
        </div>
      )}
    </div>
  );
}

const styles = {
  container: {
    position: 'fixed',
    top: '10px',
    right: '10px',
    backgroundColor: '#fff',
    border: '1px solid #ddd',
    borderRadius: '8px',
    padding: '12px 16px',
    boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
    zIndex: 9999,
    fontSize: '14px',
    minWidth: '200px',
  },
  offline: {
    color: '#d32f2f',
    fontWeight: 'bold',
    marginBottom: '8px',
  },
  pending: {
    color: '#f57c00',
    fontWeight: '500',
    marginBottom: '4px',
  },
  lastSync: {
    color: '#666',
    fontSize: '12px',
    marginTop: '4px',
  },
};
```

**Agregar estilos CSS (opcional):**

```css
/* frontend/src/app/globals.css */
.sync-status-badge {
  position: fixed;
  top: 10px;
  right: 10px;
  background-color: #fff;
  border: 1px solid #ddd;
  border-radius: 8px;
  padding: 12px 16px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
  z-index: 9999;
  font-size: 14px;
  min-width: 200px;
}

.sync-status-badge .offline-indicator {
  color: #d32f2f;
  font-weight: bold;
  margin-bottom: 8px;
}

.sync-status-badge .pending-events {
  color: #f57c00;
  font-weight: 500;
  margin-bottom: 4px;
}

.sync-status-badge .last-sync {
  color: #666;
  font-size: 12px;
  margin-top: 4px;
}
```

**Integrar en el layout:**

```jsx
// frontend/src/app/layout.jsx o el layout principal
import { SyncStatusBadge } from '@/components/SyncStatusBadge';

export default function RootLayout({ children }) {
  return (
    <html>
      <body>
        <Header />
        <SyncStatusBadge />
        {children}
      </body>
    </html>
  );
}
```

---

### Paso 4: Crear Sistema de Cacheo de Datos Maestros

**Archivo:** `frontend/src/lib/master-data-sync.ts`

```typescript
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
```

---

### Paso 5: Modificar Sistema de Sincronización para Esperar 2 Minutos

**Archivo:** `frontend/src/lib/sync.ts`

**Modificar la función `setupConnectivityListeners`:**

```typescript
// Variable global para almacenar el timeout
let syncTimeoutId: NodeJS.Timeout | null = null;

/**
 * Configura listeners de conectividad con retraso de 2 minutos
 */
export function setupConnectivityListeners(authToken: string) {
  window.addEventListener('online', async () => {
    console.log('🌐 Conexión recuperada. Esperando 2 minutos antes de sincronizar...');
    
    // Cancelar timeout anterior si existe
    if (syncTimeoutId) {
      clearTimeout(syncTimeoutId);
      syncTimeoutId = null;
    }
    
    // Esperar 2 minutos (120000 ms) antes de sincronizar
    syncTimeoutId = setTimeout(async () => {
      console.log('🔄 Sincronizando después de 2 minutos de conexión estable...');
      try {
        await syncWithServer(authToken);
      } catch (error) {
        console.error('Error en sincronización después de recuperar conexión:', error);
      }
      syncTimeoutId = null;
    }, 120000); // 2 minutos = 120,000 milisegundos
  });

  window.addEventListener('offline', () => {
    console.log('📡 Modo offline activado');
    
    // Cancelar sincronización pendiente si se pierde la conexión
    if (syncTimeoutId) {
      clearTimeout(syncTimeoutId);
      syncTimeoutId = null;
      console.log('⏹️ Sincronización cancelada (pérdida de conexión)');
    }
  });
}
```

---

### Paso 6: Actualizar SyncProvider

**Archivo:** `frontend/src/lib/SyncProvider.jsx`

```jsx
'use client';

import { useEffect } from 'react';
import { setupConnectivityListeners, startPeriodicSync, getDeviceId } from '@/lib/sync';
import { startMasterDataSync } from '@/lib/master-data-sync';
import { initializeDatabase } from '@/lib/sqlite-db';

export function SyncProvider({ children }) {
  useEffect(() => {
    let stopPeriodicSync: (() => void) | undefined;
    let stopMasterSync: (() => void) | undefined;

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
      
      // Iniciar sincronización periódica (cada 30 segundos si online)
      stopPeriodicSync = startPeriodicSync(authToken, 30000);
      
      // Iniciar sincronización de datos maestros (cada 5 minutos)
      stopMasterSync = startMasterDataSync(authToken, 300000);

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
```

---

## 📁 Archivos a Crear/Modificar

### Archivos Nuevos a Crear

1. ✅ `frontend/src/lib/axios-offline-interceptor.ts` - Interceptor de Axios
2. ✅ `frontend/src/components/SyncStatusBadge.jsx` - Componente de estado
3. ✅ `frontend/src/lib/master-data-sync.ts` - Sincronización de datos maestros

### Archivos a Modificar

1. ✅ `frontend/src/lib/sync.ts` - Agregar retraso de 2 minutos
2. ✅ `frontend/src/lib/SyncProvider.jsx` - Integrar sincronización de datos maestros
3. ✅ `frontend/src/app/layout.jsx` - Agregar SyncStatusBadge
4. ✅ Todos los servicios en `frontend/src/services/` - Usar apiClient

### Orden de Implementación Recomendado

1. **Paso 1:** Crear `axios-offline-interceptor.ts`
2. **Paso 2:** Modificar `sync.ts` (retraso de 2 minutos)
3. **Paso 3:** Crear `master-data-sync.ts`
4. **Paso 4:** Actualizar `SyncProvider.jsx`
5. **Paso 5:** Crear `SyncStatusBadge.jsx`
6. **Paso 6:** Modificar servicios uno por uno
7. **Paso 7:** Integrar `SyncStatusBadge` en layout

---

## 🧪 Pruebas y Validación

### Prueba 1: Interceptor Offline

1. Abrir DevTools → Network → Throttling → Offline
2. Intentar crear/editar algo (usuario, espacio, etc.)
3. Verificar en consola: "📝 Petición guardada offline"
4. Verificar en SQLite: `SELECT * FROM eventos_offline WHERE estado = 'Pendiente'`
5. Volver a Online
6. Esperar 2 minutos
7. Verificar que se sincroniza automáticamente

### Prueba 2: Estado de Sincronización

1. Crear eventos offline
2. Verificar que aparece el badge con el contador
3. Verificar que muestra "Sin conexión" cuando está offline
4. Verificar que muestra última sincronización cuando está online

### Prueba 3: Cacheo de Datos Maestros

1. Estar online
2. Esperar 5 minutos (o forzar sincronización)
3. Verificar en consola: "✅ Datos maestros sincronizados"
4. Ir offline
5. Verificar que se pueden leer usuarios/espacios desde caché local

### Prueba 4: Retraso de 2 Minutos

1. Estar offline
2. Crear varios eventos
3. Volver a online
4. Verificar en consola: "🌐 Conexión recuperada. Esperando 2 minutos..."
5. Esperar 2 minutos
6. Verificar: "🔄 Sincronizando después de 2 minutos..."

---

## 🔧 Troubleshooting

### Problema: Las peticiones no se guardan offline

**Solución:**
- Verificar que `isOnline()` retorna `false`
- Verificar que el método es POST/PUT/DELETE/PATCH
- Verificar que `recordEvent` no lanza errores
- Revisar consola del navegador

### Problema: El badge no aparece

**Solución:**
- Verificar que `SyncStatusBadge` está en el layout
- Verificar que `contarEventosPendientes()` funciona
- Verificar que hay eventos pendientes en SQLite
- Revisar estilos CSS

### Problema: Los datos maestros no se sincronizan

**Solución:**
- Verificar que hay token de autenticación
- Verificar que las URLs del API son correctas
- Verificar que el servidor responde correctamente
- Revisar errores en consola

### Problema: La sincronización no espera 2 minutos

**Solución:**
- Verificar que `setupConnectivityListeners` usa `setTimeout` de 120000ms
- Verificar que no hay otros listeners que sincronicen inmediatamente
- Revisar que el timeout no se cancela prematuramente

---

## 📊 Métricas y Monitoreo

### Logs a Monitorear

- `📝 Petición guardada offline` - Cuántas peticiones se guardan offline
- `🔄 Sincronizando...` - Frecuencia de sincronización
- `✅ Datos maestros sincronizados` - Éxito de cacheo
- `❌ Error sincronizando` - Errores de sincronización

### Métricas Recomendadas

- Número de eventos pendientes promedio
- Tiempo entre recuperar conexión y sincronizar
- Tasa de éxito de sincronización
- Frecuencia de uso offline

---

## ✅ Checklist de Implementación

- [ ] Crear `axios-offline-interceptor.ts`
- [ ] Modificar `sync.ts` para retraso de 2 minutos
- [ ] Crear `master-data-sync.ts`
- [ ] Actualizar `SyncProvider.jsx`
- [ ] Crear `SyncStatusBadge.jsx`
- [ ] Integrar badge en layout
- [ ] Modificar `UsuarioService.ts`
- [ ] Modificar `AccesoService.ts`
- [ ] Modificar `BeneficioService.ts`
- [ ] Modificar `EdificioService.ts`
- [ ] Modificar `SalonService.ts`
- [ ] Modificar `EventoService.ts`
- [ ] Modificar `AnuncioService.ts`
- [ ] Modificar `ReglaAccesoService.ts`
- [ ] Modificar `NotificacionService.ts`
- [ ] Probar interceptor offline
- [ ] Probar estado de sincronización
- [ ] Probar cacheo de datos maestros
- [ ] Probar retraso de 2 minutos
- [ ] Documentar cambios en código

---

## 📚 Referencias

- [SQLite DB Documentation](./sqlite-db.md)
- [Sync System Documentation](./sync-system.md)
- [PWA Best Practices](https://web.dev/progressive-web-apps/)

---

**Última actualización:** 2025-11-24  
**Versión:** 1.0.0

