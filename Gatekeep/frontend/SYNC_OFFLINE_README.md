# 📱 Sistema de Sincronización Offline - GateKeep PWA

## 🎯 Objetivo

Implementar sincronización offline-first en la PWA de GateKeep utilizando:
- **SQLite local** (sql.js) para almacenamiento persistente en el navegador
- **IndexedDB** para persistencia entre sesiones
- **Sincronización automática** cuando se recupera la conexión

---

## 🏗️ Arquitectura

### Backend (.NET 8 + PostgreSQL)

```
POST /api/sync
├── SyncRequest
│   ├── deviceId: string (fingerprint del dispositivo)
│   ├── lastSyncTime: DateTime (para delta sync)
│   ├── pendingEvents: OfflineEventDto[] (eventos capturados offline)
│   └── clientVersion: string
│
├── Procesa ISyncService
│   ├── Registra DispositivoSync
│   ├── Procesa EventoOffline[] en PostgreSQL
│   ├── Genera SyncResponse
│   └── Retorna SyncDataPayload
│
└── SyncResponse
    ├── processedEvents[]
    ├── dataToSync (usuarios, espacios, beneficios)
    ├── newAuthToken (si aplica)
    └── lastSuccessfulSync
```

### Frontend (Next.js + sql.js)

```
sqlite-db.ts
├── initializeDatabase() - Carga SQLite desde IndexedDB
├── recordOfflineEvent() - Registra evento para sync
├── getPendingOfflineEvents() - Obtiene pendientes
├── syncDataFromServer() - Actualiza caché con datos del servidor
└── saveDatabaseToStorage() - Persiste en IndexedDB

sync.ts
├── isOnline() - Detecta conectividad
├── syncWithServer() - Envía eventos + descarga datos
├── setupConnectivityListeners() - Escucha online/offline
├── startPeriodicSync() - Sincronización periódica
└── recordEvent() - API simple para registrar eventos

SyncStatus.jsx
├── Muestra estado (🌐 Online / 📡 Offline)
├── Eventos pendientes
└── Metadatos de sincronización
```

---

## 🚀 Instalación

### 1. Backend (.NET)

Ya está configurado. Solo asegúrate de ejecutar las migraciones:

```bash
cd Gatekeep/src/GateKeep.Api
dotnet ef database update
```

Las tablas creadas:
- `DispositivosSync` - Registro de dispositivos
- `EventosOffline` - Cola de eventos offline

### 2. Frontend (Next.js)

Instala sql.js:

```bash
npm install sql.js@1.10.3
```

---

## 💻 Uso en Componentes

### 1. Configurar el Provider al inicio de la app

En `pages/_app.jsx` o `app/layout.js`:

```jsx
import { SyncProvider } from '@/lib/SyncProvider';

export default function App({ Component, pageProps }) {
  return (
    <SyncProvider>
      <Component {...pageProps} />
    </SyncProvider>
  );
}
```

Esto inicia automáticamente:
- ✅ SQLite local
- ✅ Listeners de conectividad
- ✅ Sincronización periódica (cada 30s)
- ✅ Componente SyncStatus

### 2. Registrar eventos offline

En tus componentes:

```jsx
import { recordEvent } from '@/lib/sync';

async function handleAcceso(espacioId) {
  // Estamos offline - registrar localmente
  const idTemporal = await recordEvent('Acceso', {
    espacioId,
    usuarioId,
    resultado: 'Permitido',
    timestamp: new Date().toISOString(),
  });

  console.log(`✅ Evento registrado: ${idTemporal}`);
  // Se sincronizará automáticamente cuando recupere conexión
}
```

### 3. O usar los helpers predefinidos

```jsx
import { handleAccesoOfline, handleBeneficioOfline } from '@/lib/offlineEvents';

// En tu componente
const resultado = await handleAccesoOfline(espacioId, usuarioId);
```

---

## 📊 Estructura de la BD Local SQLite

### Tabla: `usuarios`
```sql
id (PK)
email (UNIQUE)
nombre
apellido
rol
credentialActiva
ultimaActualizacion
```

### Tabla: `espacios`
```sql
id (PK)
nombre
tipo
ubicacion
ultimaActualizacion
```

### Tabla: `reglas_acceso`
```sql
id (PK)
espacioId (FK)
perfil
horaInicio
horaFin
activa
ultimaActualizacion
```

### Tabla: `beneficios`
```sql
id (PK)
nombre
tipo
fechaVigenciaInicio
fechaVigenciaFin
cuposDisponibles
activo
ultimaActualizacion
```

### Tabla: `notificaciones`
```sql
id (PK)
tipo
titulo
mensaje
leido
fechaCreacion
```

### Tabla: `eventos_offline` (cola de sincronización)
```sql
idTemporal (PK)      -- ID único del cliente
tipoEvento           -- "Acceso", "Beneficio", "Notificacion"
datosEvento (JSON)   -- Datos completos del evento
fechaCreacion
intentos             -- Número de reintentos
estado               -- "Pendiente", "Procesado", "Error"
```

### Tabla: `sync_metadata`
```sql
clave (PK)           -- "ultimaSincronizacion", "deviceId", etc
valor
fechaActualizacion
```

---

## 🔄 Flujo de Sincronización

### Cuando hay conexión

```
1. Cliente detecta online
   ↓
2. Obtiene eventos offline pendientes de SQLite
   ↓
3. Construye SyncRequest
   {
     deviceId: "device-123456",
     lastSyncTime: "2025-11-18T12:30:00Z",
     pendingEvents: [
       {
         idTemporal: "Acceso-1700396400000-xyz",
         eventType: "Acceso",
         eventData: "{...}",
         createdAt: "2025-11-18T12:00:00Z",
         attemptCount: 1
       }
     ]
   }
   ↓
4. POST /api/sync (con Bearer token)
   ↓
5. Backend procesa
   - Valida autenticación
   - Registra DispositivoSync
   - Inserta EventoOffline en PostgreSQL
   - Construye SyncDataPayload
   ↓
6. Backend retorna SyncResponse
   {
     success: true,
     processedEvents: [{idTemporal, success, permanentId}],
     dataToSync: {
       usuarios: [...],
       espacios: [...],
       beneficios: [...]
     },
     lastSuccessfulSync: "2025-11-18T12:31:00Z"
   }
   ↓
7. Cliente actualiza SQLite local
   - Marca eventos como "Procesado"
   - Inserta/actualiza usuarios, espacios, etc.
   - Actualiza sync_metadata
   ↓
8. Sincronización completada ✅
```

### Cuando NO hay conexión

```
1. Cliente detecta offline (navigator.onLine = false)
   ↓
2. Usuario realiza una acción (ej: toca acceso)
   ↓
3. App registra evento offline en SQLite local
   recordOfflineEvent('Acceso', {...})
   ↓
4. Evento queda en tabla eventos_offline
   estado: "Pendiente"
   ↓
5. Muestra al usuario que se sincronizará después
   "✅ Acceso registrado. Se sincronizará cuando se recupere conexión."
   ↓
6. SyncStatus muestra "📡 Offline - 3 eventos pendientes"
   ↓
7. Cuando se recupera conexión → Vuelve a paso 4 del flujo anterior
```

---

## 🛠️ Configuración Avanzada

### Cambiar intervalo de sincronización

En `SyncProvider.jsx`:

```jsx
// De 30 segundos a 60 segundos
startPeriodicSync(authToken, 60000);
```

### Sincronizar manualmente

```jsx
import { syncWithServer } from '@/lib/sync';

// Botón manual
<button onClick={() => syncWithServer(authToken)}>
  Sincronizar ahora
</button>
```

### Limpiar BD local

```jsx
import { clearProcessedEvents } from '@/lib/sqlite-db';

clearProcessedEvents();
```

---

## 🧪 Testing

### 1. Simular modo offline en DevTools

```javascript
// En la consola del navegador
navigator.__defineGetter__('onLine', function() {
  return false;
});

// O usar Chrome DevTools:
// DevTools → Network → Offline (checkbox)
```

### 2. Verificar eventos en SQLite

```javascript
import { getPendingOfflineEvents, getOfflineStatus } from '@/lib/sqlite-db';

// En consola
getPendingOfflineEvents()
getOfflineStatus()
```

### 3. Monitorear sincronización

```javascript
// En consola del navegador
// Todos los logs de sync.ts y sqlite-db.ts incluyen prefijos:
// 🌐 (online)
// 📡 (offline)
// 📝 (eventos)
// 🔄 (sincronización)
// ✅ (éxito)
// ❌ (error)
```

---

## 📈 Monitoreo en Producción

### Backend

En `SyncService.cs` se generan logs con ILogger:

```
[12:34:56 INF] Iniciando sincronización para dispositivo {DeviceId}
[12:34:57 INF] Evento offline {IdTemporal} procesado exitosamente
[12:34:58 INF] Sincronización completada exitosamente
```

### Frontend

En `SyncStatus.jsx` hay un componente visual que muestra:
- Estado (Online/Offline)
- Eventos pendientes
- Última sincronización
- Device ID

---

## 🔐 Seguridad

### ✅ Lo que tenemos

1. **Autenticación JWT** - `/api/sync` requiere Bearer token
2. **Validación en servidor** - Cada evento se valida antes de guardar
3. **Deduplicación** - `idTemporal` previene duplicados
4. **HTTPS obligatorio** en producción

### ⚠️ Consideraciones

1. **No guardar datos sensibles en SQLite local**
   - Evitar contraseñas, tokens, datos médicos
   - SQLite local es accesible desde DevTools

2. **Limpiar tokens regularmente**
   - Los eventos offline expiran después de X horas
   - Tokens renovables en cada sync

3. **Validar eventos en el servidor**
   - No confiar en datos del cliente
   - Revalidar accesos y permisos

---

## 📝 Checklist de Implementación

### Backend
- [x] Contratos `SyncRequest`, `SyncResponse`
- [x] Entidades `DispositivoSync`, `EventoOffline`
- [x] Servicio `ISyncService`, `SyncService`
- [x] Endpoint `POST /api/sync`
- [x] Migración EF Core
- [x] Compila sin errores

### Frontend
- [x] `sqlite-db.ts` - Gestor de SQLite
- [x] `sync.ts` - Cliente de sincronización
- [x] `SyncStatus.jsx` - UI de estado
- [x] `SyncProvider.jsx` - Inicialización
- [x] `offlineEvents.js` - Helpers
- [x] `package.json` actualizado
- [ ] Integrado en `_app.jsx`
- [ ] Service Worker (opcional pero recomendado)
- [ ] Tests offline

---

## 📚 Próximos Pasos

1. **Integrar SyncProvider en app**
   ```jsx
   // pages/_app.jsx
   import { SyncProvider } from '@/lib/SyncProvider';
   ```

2. **Registrar eventos en componentes**
   ```jsx
   // pages/acceso.jsx
   await recordEvent('Acceso', {...});
   ```

3. **Crear Service Worker** (opcional)
   - `public/sw.js` - Para cache de assets
   - Mejora velocidad offline

4. **Testing offline**
   - Simular sin conexión
   - Verificar sincronización

5. **Monitoreo**
   - Dashboard de eventos offline
   - Alertas de falla de sync

---

## 🎓 Referencias

- [sql.js Documentation](https://sql.js.org/)
- [IndexedDB API](https://developer.mozilla.org/en-US/docs/Web/API/IndexedDB_API)
- [Service Workers](https://developer.mozilla.org/en-US/docs/Web/API/Service_Worker_API)
- [Progressive Web Apps](https://web.dev/progressive-web-apps/)

---

**Creado:** 18 de noviembre de 2025  
**Proyecto:** GateKeep - Sistema de Gestión de Acceso  
**Versión:** 1.0.0
