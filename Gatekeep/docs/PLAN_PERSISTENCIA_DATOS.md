# Plan de Implementación: 3.3 Persistencia y Datos

**Fecha de creación:** 11 de noviembre de 2025  
**Proyecto:** GateKeep - Sistema de Gestión de Acceso  
**Requisito:** Grupos de 3 y Grupos de 4

---

## 📋 Resumen del Requisito

### Especificación Original
> La base de datos principal queda a elección, pero debe ser administrada mediante Entity Framework Core con migraciones controladas.
> 
> La aplicación PWA (Progressive Web App) deberá operar con modo offline, utilizando SQLite para almacenamiento local y sincronización posterior.

---

## 🎯 Estado Actual del Proyecto

### ✅ **LO QUE YA TIENES IMPLEMENTADO**

#### 1. Base de Datos Principal con EF Core
- **✅ PostgreSQL** configurado como base de datos principal
- **✅ Entity Framework Core 9.0.0** instalado y funcionando
- **✅ DbContext:** `GateKeepDbContext` implementado en `Infrastructure/Persistence/`
- **✅ Migraciones:** Sistema de migraciones controladas activo
  - Migración inicial: `20251111153600_InitialCreate`
  - Historial de migraciones en esquema `infra.__EFMigrationsHistory`

#### 2. Entidades del Dominio
- **✅ 15 entidades** definidas in `Domain/Entities/`:
  - Usuario, Beneficio, BeneficioUsuario
  - Espacio, Edificio, Laboratorio, Salon, UsuarioEspacio
  - ReglaAcceso, EventoAcceso, Evento, EventoHistorico
  - Anuncio, Notificacion, NotificacionUsuario

#### 3. Configuraciones EF Core
- **✅ Fluent API:** Configuraciones en `Infrastructure/Persistence/Configurations/`
- **✅ Repositorios:** Implementados para todas las entidades principales
- **✅ Connection String:** Configurable vía variables de entorno y `config.json`

#### 4. Arquitectura Híbrida
- **✅ MongoDB:** Para auditoría y notificaciones
- **✅ PostgreSQL:** Para datos transaccionales
- **✅ Redis:** Para caché

### ❌ **LO QUE FALTA IMPLEMENTAR**

#### 1. PWA (Progressive Web App) con Modo Offline
- ❌ **No hay configuración PWA** en el frontend Next.js
- ❌ **No existe Service Worker** para modo offline
- ❌ **No hay manifest.json** para instalación como app
- ❌ **No hay SQLite local** configurado con sql.js en el navegador
- ❌ **No hay estrategia de sincronización offline** implementada
- ❌ **No hay endpoints de sincronización** en el backend API

#### 2. Backend: API de Sincronización
- ❌ **No existen contratos de sincronización** (`SyncRequest`, `SyncResponse`)
- ❌ **No hay servicio de sincronización** (`ISyncService`, `SyncService`)
- ❌ **No hay endpoints** `/api/sync` para sincronización
- ❌ **Faltan timestamps** en entidades para tracking de cambios (`FechaCreacion`, `UltimaActualizacion`)
- ❌ **No hay migración** para agregar campos de sincronización

---

## 📐 Arquitectura Propuesta

```
┌─────────────────────────────────────────────────────────────┐
│                    PROGRESSIVE WEB APP (PWA)                 │
│              Next.js 15 Frontend (YA EXISTE)                 │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌────────────────────────────────────────────────────┐    │
│  │   SQLite Local (sql.js + WebAssembly)              │    │
│  │  - Base de datos SQLite en el navegador            │    │
│  │  - Persistida en localStorage/IndexedDB            │    │
│  │  - Tablas:                                          │    │
│  │    * usuarios (cache)                               │    │
│  │    * espacios (cache)                               │    │
│  │    * eventos_acceso_pendientes                      │    │
│  │    * notificaciones (cache)                         │    │
│  │    * sync_metadata                                  │    │
│  └────────────────────────────────────────────────────┘    │
│                          ↕                                   │
│  ┌────────────────────────────────────────────────────┐    │
│  │    Service Worker + Sync Client                     │    │
│  │  - Cache API (recursos estáticos)                  │    │
│  │  - Detecta conectividad (navigator.onLine)         │    │
│  │  - Envía datos pendientes al servidor              │    │
│  │  - Descarga actualizaciones                         │    │
│  │  - Background Sync API (opcional)                   │    │
│  └────────────────────────────────────────────────────┘    │
└──────────────────────────┬──────────────────────────────────┘
                           │ HTTP/HTTPS (Fetch API)
                           │ (cuando hay conexión)
                           ↓
┌─────────────────────────────────────────────────────────────┐
│                    BACKEND API (.NET 8)                      │
│                    GateKeep.Api (YA EXISTE)                  │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌────────────────────┐  ┌────────────────┐  ┌───────────┐│
│  │   PostgreSQL       │  │    MongoDB     │  │   Redis   ││
│  │  (Datos Principales)│ │   (Auditoría)  │  │  (Caché)  ││
│  │  ✅ YA EXISTE      │  │  ✅ YA EXISTE  │  │✅ YA EXISTE││
│  └────────────────────┘  └────────────────┘  └───────────┘│
└─────────────────────────────────────────────────────────────┘
```

**Ventajas de SQLite (sql.js) en PWA:**
- ✅ **SQLite real** ejecutado en el navegador vía WebAssembly
- ✅ **Misma sintaxis SQL** que en el backend
- ✅ **Mejor rendimiento** para consultas complejas vs IndexedDB
- ✅ **Transacciones ACID** completas
- ✅ **Compatibilidad** con migraciones del backend
- ✅ **Persistencia** mediante localStorage o IndexedDB como storage
- ✅ **Sin dependencias nativas** - funciona en cualquier navegador moderno

**Por qué PWA sobre App Nativa:**
- ✅ **No requiere instalación** desde tiendas de apps
- ✅ **Actualización automática** sin intervención del usuario
- ✅ **Un solo codebase** para web, móvil y escritorio
- ✅ **Menor complejidad** de desarrollo y mantenimiento
- ✅ **Funciona offline** igual que app nativa
- ✅ **Instalable** en dispositivos móviles como app

---

## 🗺️ Plan de Implementación Completo

### **FASE 1: Preparación del Backend para Sincronización PWA** ⏱️ 3-5 días

#### 1.1 Instalar Paquete SQLite en Backend (Opcional para Testing)

**Ubicación:** `src/GateKeep.Api/`

**Acción:** Ejecutar comando para instalar EF Core SQLite versión 9.0.0

**Propósito:** 
- Permite testing local de sincronización
- Crear migraciones compatibles con SQLite
- Testing de integración sin necesidad de PostgreSQL

---

#### 1.2 Crear Contratos de Sincronización

**Archivos a crear:**

**`src/GateKeep.Api/Contracts/Sync/SyncRequest.cs`**
- Crear record `SyncRequest` con:
  - `DateTime? UltimaActualizacion`: Para saber qué datos enviar
  - `List<EventoAccesoOffline> EventosAccesoPendientes`: Eventos creados offline
  - `string? DispositivoId`: Identificador único del navegador/dispositivo

- Crear record `EventoAccesoOffline` con:
  - `Guid IdTemporal`: ID temporal generado en el cliente
  - `int UsuarioId`, `int EspacioId`: Referencias a entidades
  - `DateTime FechaHora`: Cuándo ocurrió el evento
  - `string TipoAcceso`: Tipo de acceso (entrada/salida)
  - `bool Exitoso`: Si fue exitoso o denegado
  - `string? Motivo`: Razón en caso de denegación

**`src/GateKeep.Api/Contracts/Sync/SyncResponse.cs`**
- Crear record `SyncResponse` con:
  - `bool Exitoso`: Indica si la sincronización fue exitosa
  - `DateTime FechaSincronizacion`: Timestamp de la sincronización
  - `SyncData? Datos`: Datos actualizados para el cliente
  - `List<string> Errores`: Mensajes de error si los hay

- Crear record `SyncData` con listas de:
  - `List<UsuarioSync> Usuarios`: Usuarios actualizados
  - `List<EspacioSync> Espacios`: Espacios actualizados
  - `List<ReglaAccesoSync> ReglasAcceso`: Reglas actualizadas
  - `List<NotificacionSync> Notificaciones`: Notificaciones nuevas

- Crear records para cada tipo de sincronización:
  - `UsuarioSync`: Id, Rut, Nombre, Email, Rol, UltimaActualizacion
  - `EspacioSync`: Id, Nombre, Tipo, EdificioId, EdificioNombre, UltimaActualizacion
  - `ReglaAccesoSync`: Id, EspacioId, TipoRegla, FechaInicio, FechaFin, UltimaActualizacion
  - `NotificacionSync`: Id, Titulo, Mensaje, FechaCreacion, Leido

---

#### 1.3 Crear Servicio de Sincronización

**`src/GateKeep.Api/Application/Sync/ISyncService.cs`**
- Crear interfaz con tres métodos principales:
  - `SincronizarAsync`: Método principal que orquesta la sincronización completa
  - `ObtenerDatosActualizadosAsync`: Obtiene datos del servidor que cambiaron desde última sync
  - `ProcesarEventosAccesoOfflineAsync`: Procesa y persiste eventos creados offline

**`src/GateKeep.Api/Application/Sync/SyncService.cs`**
- Implementar la interfaz
- Inyectar dependencias:
  - `GateKeepDbContext`: Para acceso a la base de datos
  - `ILogger<SyncService>`: Para logging
  - `IEventoAccesoService`: Para crear eventos de acceso

**Lógica a implementar en `SincronizarAsync`:**
1. Validar el request y el usuario
2. Procesar eventos offline enviados por el cliente
3. Obtener datos actualizados desde la última sincronización
4. Retornar SyncResponse con datos actualizados

**Lógica en `ObtenerDatosActualizadosAsync`:**
1. Consultar usuarios modificados después de `ultimaActualizacion`
2. Consultar espacios modificados
3. Consultar reglas de acceso modificadas
4. Obtener notificaciones para el usuario
5. Retornar todo en un objeto `SyncData`

**Lógica en `ProcesarEventosAccesoOfflineAsync`:**
1. Iterar eventos recibidos
2. Validar que el usuario y espacio existan
3. Crear EventoAcceso en la base de datos
4. Registrar en auditoría (MongoDB)
5. Retornar lista de IDs de servidor para eventos creados

---

#### 1.4 Crear Endpoints de Sincronización

**`src/GateKeep.Api/Endpoints/Sync/SyncEndpoints.cs`**

- Crear método estático `MapSyncEndpoints` que:
  - Crea un grupo de endpoints bajo `/api/sync`
  - Requiere autorización (usuario autenticado)
  - Agrega tag "Sincronización" para Swagger

**Endpoints a crear:**

1. **POST `/api/sync`** - Sincronizar
   - Recibe `SyncRequest`
   - Obtiene el ID del usuario del token JWT
   - Llama a `SyncService.SincronizarAsync`
   - Retorna `SyncResponse`

2. **GET `/api/sync/datos`** - Obtener Datos
   - Recibe `ultimaActualizacion` como query parameter
   - Obtiene el ID del usuario del token JWT
   - Llama a `SyncService.ObtenerDatosActualizadosAsync`
   - Retorna `SyncData`

**Registrar endpoints:**
- Agregar llamada a `MapSyncEndpoints()` en `Program.cs`

---

#### 1.5 Agregar Timestamps a Entidades

**Entidades a modificar:** Usuario, Espacio, ReglaAcceso, Evento, Anuncio, Beneficio

**Campos a agregar:**
- `DateTime FechaCreacion { get; set; }`
- `DateTime UltimaActualizacion { get; set; }`

**Configuración en DbContext:**
- Sobrescribir `SaveChangesAsync`
- Antes de guardar, iterar entidades modificadas
- Si es nueva: establecer `FechaCreacion` y `UltimaActualizacion` al momento actual
- Si es modificada: actualizar solo `UltimaActualizacion`

**Crear migración:**
- Ejecutar comando para crear migración "AgregarTimestampsParaSync"
- Ejecutar comando para aplicar migración a la base de datos
- Verificar que las columnas se agregaron correctamente

---

### **FASE 2: Implementar PWA con SQLite Local (sql.js)** ⏱️ 5-7 días

#### 2.1 Instalar Paquetes para PWA + SQLite

**Ubicación:** `frontend/`

**Paquetes a instalar:**
- `sql.js`: SQLite compilado a WebAssembly
- `workbox-window`: Para comunicación con Service Workers
- `next-pwa` (dev dependency): Plugin de PWA para Next.js

**Archivo WebAssembly:**
- Copiar `sql-wasm.wasm` desde `node_modules/sql.js/dist/` a `public/`
- Agregar script `postinstall` en `package.json` para automatizar la copia

---

#### 2.2 Crear Manifest PWA

**`frontend/public/manifest.json`**

**Configurar:**
- Nombre completo y nombre corto de la aplicación
- Descripción de la PWA
- URL de inicio: "/"
- Modo de visualización: "standalone" (como app nativa)
- Colores de tema y fondo
- Orientación: "portrait-primary" (vertical)

**Iconos:**
- Crear iconos en múltiples tamaños: 72x72, 96x96, 128x128, 144x144, 152x152, 192x192, 384x384, 512x512
- Guardar en `/public/icons/`
- Configurar en el manifest con propósito "any maskable"

**Shortcuts (atajos):**
- "Registrar Acceso": Link directo a funcionalidad principal
- "Notificaciones": Link directo a notificaciones

---

#### 2.3 Crear Service Worker

**`frontend/public/sw.js`**

**Definir nombres de caché:**
- `STATIC_CACHE`: Para recursos estáticos (HTML, manifest, iconos, WASM)
- `DYNAMIC_CACHE`: Para respuestas de API y recursos dinámicos

**Evento `install`:**
- Abrir caché estático
- Pre-cachear recursos críticos: página principal, login, offline.html, manifest, iconos, sql-wasm.wasm
- Llamar a `skipWaiting()` para activar inmediatamente

**Evento `activate`:**
- Limpiar cachés antiguos
- Mantener solo versiones actuales (STATIC_CACHE y DYNAMIC_CACHE)
- Llamar a `clients.claim()` para tomar control de todas las páginas

**Evento `fetch` - Estrategias de caché:**

1. **Para llamadas `/api/*`** - Network First:
   - Intentar fetch de red primero
   - Si es exitoso, guardar en caché dinámico
   - Si falla (offline), retornar respuesta JSON indicando modo offline

2. **Para recursos estáticos** - Cache First:
   - Buscar en caché primero
   - Si está en caché, retornarlo y actualizar en segundo plano
   - Si no está, hacer fetch y guardar en caché
   - Si todo falla, mostrar página offline

**Evento `sync` - Sincronización en segundo plano:**
- Escuchar eventos de tipo "sync-eventos-acceso"
- Enviar mensaje a todos los clientes activos para que sincronicen desde SQLite

**Evento `message`:**
- Escuchar comando "SKIP_WAITING" para actualizar el SW

---

#### 2.4 Crear Capa de Almacenamiento con SQLite (sql.js)

**`frontend/src/lib/sqlite-db.ts`**

**Función `initDatabase()`:**
- Cargar librería sql.js con WebAssembly
- Intentar cargar base de datos existente desde localStorage
- Si existe, deserializar y cargar
- Si no existe, crear nueva base de datos
- Llamar a `createSchema()` para crear tablas

**Función `createSchema()`:**
- Crear tabla `usuarios`: id, rut, nombre, email, rol, ultima_actualizacion
- Crear tabla `espacios`: id, nombre, tipo, edificio_id, edificio_nombre, ultima_actualizacion
- Crear tabla `eventos_acceso_pendientes`: id_temporal, usuario_id, espacio_id, fecha_hora, tipo_acceso, exitoso, motivo, sincronizado, id_servidor
- Crear índice en `eventos_acceso_pendientes` por campo `sincronizado`
- Crear tabla `notificaciones`: id, titulo, mensaje, fecha_creacion, leido
- Crear tabla `sync_metadata`: clave, valor (para guardar última sincronización)

**Función `saveDatabase()`:**
- Exportar base de datos a Uint8Array
- Convertir a array normal
- Serializar a JSON y guardar en localStorage como "gatekeep-sqlite-db"

**Funciones para Usuarios:**
- `guardarUsuariosLocales(usuarios)`: INSERT OR REPLACE múltiples usuarios
- `obtenerUsuariosLocales()`: SELECT * y mapear resultados a objetos
- `obtenerUsuarioPorId(id)`: SELECT con WHERE id = ?

**Funciones para Espacios:**
- `guardarEspaciosLocales(espacios)`: INSERT OR REPLACE múltiples espacios
- `obtenerEspaciosLocales()`: SELECT * y mapear resultados

**Funciones para Eventos de Acceso:**
- `guardarEventoAccesoLocal(evento)`: INSERT con sincronizado = 0, genera UUID si no tiene
- `obtenerEventosPendientes()`: SELECT WHERE sincronizado = 0
- `marcarEventoComoSincronizado(idTemporal, idServidor)`: UPDATE sincronizado = 1
- `contarEventosPendientes()`: SELECT COUNT(*) WHERE sincronizado = 0

**Funciones para Notificaciones:**
- `guardarNotificacionesLocales(notificaciones)`: INSERT OR REPLACE múltiples
- `obtenerNotificacionesLocales()`: SELECT * ORDER BY fecha_creacion DESC

**Funciones para Metadata:**
- `guardarUltimaSincronizacion()`: INSERT OR REPLACE clave='ultima_sync', valor=timestamp
- `obtenerUltimaSincronizacion()`: SELECT valor WHERE clave='ultima_sync'

**Funciones de Utilidad:**
- `limpiarDatosLocales()`: DELETE FROM todas las tablas
- `exportarBaseDatos()`: Retornar Uint8Array de la BD completa

---

#### 2.5 Crear Cliente de Sincronización

**`frontend/src/lib/sync.ts`**

**Clase `SyncClient`:**

**Propiedades:**
- `syncInProgress`: Flag booleano para evitar sincronizaciones simultáneas
- `apiBaseUrl`: URL base del backend API

**Método `isOnline()`:**
- Retornar `navigator.onLine` para verificar conectividad

**Método `attemptSync()`:**
1. Verificar si hay conexión, si no, cancelar
2. Verificar si ya hay sincronización en progreso, si sí, cancelar
3. Obtener eventos pendientes desde SQLite local
4. Obtener timestamp de última sincronización
5. Construir objeto `SyncRequest`
6. Hacer POST a `/api/sync` con token JWT en headers
7. Si es exitoso:
   - Marcar todos los eventos como sincronizados en SQLite
   - Guardar usuarios actualizados recibidos
   - Guardar espacios actualizados recibidos
   - Guardar notificaciones recibidas
   - Actualizar timestamp de última sincronización
8. Si falla, loggear error
9. Retornar true/false según éxito

**Método `startAutoSync(intervalMinutes)`:**
- Hacer sincronización inicial inmediatamente si hay conexión
- Configurar `setInterval` para sincronizar periódicamente
- Verificar conexión antes de cada intento

**Método `getDeviceId()` (privado):**
- Obtener "device-id" de localStorage
- Si no existe, generar UUID y guardarlo
- Retornar el ID

**Exportar instancia global:**
- Crear instancia de `SyncClient` con URL del backend
- Exportar como `syncClient` para uso en toda la aplicación

---

#### 2.6 Registrar Service Worker en la App

**`frontend/src/lib/register-sw.ts`**

**Función `registerServiceWorker()`:**

**Verificaciones iniciales:**
- Verificar que no es server-side (typeof window !== 'undefined')
- Verificar que Service Workers están soportados ('serviceWorker' in navigator)

**En evento `load` del window:**
1. Registrar Service Worker desde `/sw.js`
2. Cuando se registre exitosamente:
   - Loggear scope del SW
   - Configurar verificación de actualizaciones cada 1 hora
   - Escuchar mensajes del SW
   - Si recibe mensaje "SYNC_NOW", llamar a `syncClient.attemptSync()`

**Eventos de conectividad:**
- Escuchar evento `online`: Llamar a `syncClient.attemptSync()` cuando se recupere conexión
- Escuchar evento `offline`: Loggear que se perdió la conexión

**Manejo de errores:**
- Loggear error si falla el registro del SW

---

#### 2.7 Integrar en Layout Principal

**`frontend/src/app/layout.js`**

**Modificaciones:**
- Agregar directiva `'use client'` si no existe
- Importar: `useEffect`, `registerServiceWorker`, `syncClient`, `initDatabase`

**En `useEffect` (solo en mount):**
1. Llamar a `initDatabase()`
2. Cuando se resuelva:
   - Loggear que SQLite se inicializó
   - Llamar a `registerServiceWorker()`
   - Llamar a `syncClient.startAutoSync(15)` para sincronizar cada 15 minutos
3. Catch de errores con log

**En `<head>`:**
- Agregar `<link rel="manifest" href="/manifest.json" />`
- Meta tag `theme-color` con color principal
- Meta tags para iOS: `apple-mobile-web-app-capable`, `apple-mobile-web-app-status-bar-style`, `apple-mobile-web-app-title`
- Meta description mencionando PWA y modo offline

---

#### 2.8 Crear Componente de Estado de Sincronización

**`frontend/src/components/SyncStatus.jsx`**

**Estados del componente:**
- `isOnline`: Boolean para estado de conexión
- `pendingCount`: Número de eventos pendientes de sincronizar
- `syncing`: Boolean para indicar sincronización en progreso
- `lastSync`: String con hora de última sincronización exitosa

**En `useEffect`:**
1. Establecer `isOnline` inicial con `navigator.onLine`
2. Agregar listeners para eventos `online` y `offline`
3. Crear función para verificar eventos pendientes llamando a `contarEventosPendientes()`
4. Ejecutar verificación inicial
5. Configurar interval cada 5 segundos para actualizar contador
6. Cleanup: remover listeners y limpiar interval

**Función `handleManualSync`:**
1. Establecer `syncing = true`
2. Llamar a `syncClient.attemptSync()`
3. Si es exitoso, guardar hora actual en `lastSync`
4. Establecer `syncing = false`
5. Actualizar contador de pendientes

**Render:**
- Indicador de estado online/offline con íconos
- Badge con número de eventos pendientes (solo si hay)
- Botón de sincronización manual (solo si está online)
  - Deshabilitado mientras sincroniza
  - Muestra spinner durante sincronización
- Texto con hora de última sincronización (si existe)
- Estilos responsive con flexbox

---

#### 2.9 Crear Página Offline

**`frontend/public/offline.html`**

**Contenido:**
- HTML standalone (no requiere JavaScript)
- Diseño centrado con gradiente de fondo
- Ícono grande de "sin conexión"
- Título "Sin Conexión"
- Mensaje explicativo:
  - Informar que no hay conexión al servidor
  - Tranquilizar que la PWA funciona offline con SQLite
  - Indicar que los datos se sincronizarán automáticamente al reconectar
- Botón para volver a "/"
- Estilos inline responsive

---

#### 2.10 Configurar Next.js para PWA

**`frontend/next.config.js`**

**Modificaciones:**
- Importar `next-pwa`
- Configurar wrapper con:
  - `dest: 'public'`: Destino de archivos generados
  - `register: true`: Registrar SW automáticamente
  - `skipWaiting: true`: Actualizar SW sin esperar
  - `disable: process.env.NODE_ENV === 'development'`: Deshabilitar en desarrollo (opcional)

---

#### 2.11 Generar Iconos PWA

**Herramientas sugeridas:**
- PWA Asset Generator
- RealFaviconGenerator
- O usar tu herramienta de diseño preferida

**Tamaños necesarios:**
- 72x72, 96x96, 128x128, 144x144, 152x152, 192x192, 384x384, 512x512

**Ubicación:** `frontend/public/icons/`

**Formato:** PNG con fondo sólido o transparente

---

### **FASE 3: Testing y Validación** ⏱️ 2-3 días

#### 3.1 Pruebas de Funcionalidad Offline

**Test Manual - Modo Offline:**
1. Abrir la PWA en Chrome
2. Abrir DevTools (F12)
3. Ir a Application → Service Workers
4. Marcar checkbox "Offline" para simular sin conexión
5. Intentar usar la aplicación:
   - Registrar eventos de acceso
   - Ver datos cacheados
   - Navegar entre páginas
6. Verificar en Application → Storage → Local Storage:
   - Buscar key "gatekeep-sqlite-db"
   - Ver que contiene datos
7. Desmarcar "Offline"
8. Verificar que se sincroniza automáticamente
9. Revisar en DevTools Console los logs de sincronización

**Test con SQL en Consola:**
- Abrir consola del navegador
- Importar dinámicamente la función `getDb`
- Ejecutar queries SQL para verificar datos:
  - Ver todas las tablas
  - Ver eventos pendientes
  - Ver usuarios en cache
  - Ver metadata de sincronización

---

#### 3.2 Pruebas de Performance

**Con Lighthouse:**
1. Abrir DevTools → Lighthouse
2. Seleccionar categoría "Progressive Web App"
3. Ejecutar análisis
4. Objetivo: Score mínimo de 90/100
5. Revisar métricas:
   - Instalable
   - Funciona offline
   - Service Worker registrado
   - Manifest válido
   - HTTPS (en producción)

**Con Chrome DevTools:**
1. Ir a Performance tab
2. Grabar interacción típica del usuario
3. Analizar:
   - Tiempo de carga inicial
   - Tiempo de queries SQLite
   - Tiempo de sincronización
   - Uso de memoria

**Network Tab:**
1. Ver estrategia de caché
2. Verificar que recursos estáticos se sirven desde caché
3. Verificar que API calls usan caché como fallback

---

#### 3.3 Pruebas de Sincronización

**Escenarios de prueba:**

**Escenario 1: Sincronización exitosa con eventos pendientes**
- Crear varios eventos offline
- Verificar que se guardan en SQLite
- Reconectar
- Verificar que se envían al servidor
- Verificar que se marcan como sincronizados
- Verificar que desaparece el contador de pendientes

**Escenario 2: Sincronización sin eventos pendientes**
- Estar online
- No crear eventos offline
- Llamar a sincronización manual
- Verificar que solo se descargan datos actualizados
- Sin errores en consola

**Escenario 3: Sincronización con error del servidor**
- Crear eventos offline
- Detener el backend o simular error 500
- Intentar sincronizar
- Verificar que:
  - Los eventos permanecen como no sincronizados
  - Se muestra error en consola
  - El contador de pendientes no cambia
  - Se puede reintentar después

**Escenario 4: Sincronización después de reconexión**
- Crear eventos offline
- Perder conexión (marcar offline en DevTools)
- Recuperar conexión (desmarcar offline)
- Verificar que automáticamente se sincroniza
- Event listener de `online` funciona correctamente

**Escenario 5: Múltiples dispositivos**
- Abrir PWA en dos navegadores diferentes
- Crear eventos en ambos offline
- Conectar ambos
- Verificar que ambos sincronizan correctamente
- Verificar que no hay conflictos
- Cada uno tiene su `device-id` único

---

#### 3.4 Pruebas de Persistencia

**Test de Persistencia:**
1. Crear eventos offline
2. Cerrar completamente el navegador
3. Abrir nuevamente
4. Verificar que los datos siguen en SQLite
5. Verificar que el contador de pendientes es correcto

**Test de Limpieza:**
1. Limpiar datos del navegador (Ctrl+Shift+Del)
2. Marcar "Local Storage"
3. Limpiar
4. Recargar aplicación
5. Verificar que se crea nueva base de datos vacía
6. Verificar que se puede usar normalmente

---

#### 3.5 Pruebas de Instalación PWA

**En móvil (Android/iOS):**
1. Abrir PWA en Chrome/Safari móvil
2. Buscar banner de instalación o menú "Agregar a pantalla de inicio"
3. Instalar
4. Verificar que aparece ícono en home screen
5. Abrir desde el ícono
6. Verificar que se abre en modo standalone (sin barra de navegador)
7. Probar funcionalidad offline en móvil

**En escritorio (Chrome/Edge):**
1. Abrir PWA
2. Buscar ícono de instalación en barra de direcciones
3. Instalar
4. Verificar que se abre como ventana independiente
5. Verificar que aparece en menú de aplicaciones del sistema operativo

---

## 📊 Checklist de Implementación

### Backend (.NET)
- [ ] Instalar EF Core SQLite (opcional)
- [ ] Crear contratos de sincronización (SyncRequest, SyncResponse, SyncData, records individuales)
- [ ] Crear interfaz ISyncService
- [ ] Implementar SyncService con lógica de sincronización
- [ ] Crear endpoints /api/sync (POST y GET)
- [ ] Registrar endpoints en Program.cs
- [ ] Agregar campos FechaCreacion y UltimaActualizacion a entidades
- [ ] Configurar auto-actualización de timestamps en DbContext
- [ ] Crear migración AgregarTimestampsParaSync
- [ ] Aplicar migración a base de datos
- [ ] Agregar logging en SyncService
- [ ] Implementar manejo de errores completo
- [ ] Testing unitario de SyncService
- [ ] Testing de integración de endpoints con Postman/Swagger

### Frontend PWA
- [ ] Instalar paquetes: sql.js, workbox-window, next-pwa
- [ ] Copiar sql-wasm.wasm a public/
- [ ] Crear manifest.json con configuración completa
- [ ] Generar iconos PWA en todos los tamaños
- [ ] Crear Service Worker (sw.js) con estrategias de caché
- [ ] Crear sqlite-db.ts con schema y funciones CRUD
- [ ] Crear sync.ts con clase SyncClient
- [ ] Crear register-sw.ts con lógica de registro
- [ ] Actualizar layout.js con inicialización de SQLite y SW
- [ ] Crear componente SyncStatus.jsx
- [ ] Crear página offline.html
- [ ] Configurar next-pwa en next.config.js
- [ ] Agregar script postinstall en package.json
- [ ] Testing en Chrome DevTools modo offline
- [ ] Testing de sincronización manual y automática
- [ ] Testing en dispositivo móvil real (Android/iOS)
- [ ] Validar instalación como PWA
- [ ] Ejecutar Lighthouse PWA audit
- [ ] Verificar performance de queries SQLite

### Documentación
- [ ] Documentar arquitectura de sincronización
- [ ] Crear guía de usuario para modo offline
- [ ] Documentar schema de SQLite local
- [ ] Crear guía de troubleshooting
- [ ] Documentar proceso de instalación PWA
- [ ] Documentar estrategias de caché del Service Worker

---

## 🚀 Comandos Rápidos

### Backend
```bash
# Agregar paquete SQLite (opcional)
cd src/GateKeep.Api
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 9.0.0

# Crear migración para timestamps
dotnet ef migrations add AgregarTimestampsParaSync

# Aplicar migración
dotnet ef database update

# Ejecutar API
dotnet run
```

### Frontend
```bash
# Instalar dependencias de PWA y SQLite
cd frontend
npm install sql.js workbox-window
npm install --save-dev next-pwa

# Copiar WebAssembly de SQLite
npm run postinstall

# O manualmente en PowerShell (Windows)
Copy-Item node_modules/sql.js/dist/sql-wasm.wasm -Destination public/

# Ejecutar en desarrollo
npm run dev

# Build para producción
npm run build

# Ejecutar producción
npm start
```

### Testing SQLite en Consola del Navegador
```javascript
// Ver estructura de la base de datos
const { getDb } = await import('/src/lib/sqlite-db.ts');
const db = await getDb();

// Ver todas las tablas
db.exec("SELECT name FROM sqlite_master WHERE type='table'");

// Ver eventos pendientes
db.exec('SELECT * FROM eventos_acceso_pendientes WHERE sincronizado = 0');

// Ver usuarios en cache
db.exec('SELECT * FROM usuarios');

// Contar eventos pendientes
db.exec('SELECT COUNT(*) FROM eventos_acceso_pendientes WHERE sincronizado = 0');

// Ver metadata
db.exec('SELECT * FROM sync_metadata');
```

---

### Seguridad

**Datos Sensibles:**
- ❌ **NUNCA** guardar contraseñas en SQLite local
- ❌ **NUNCA** guardar datos médicos, financieros o altamente sensibles sin cifrado
- ✅ Solo cachear datos que el usuario ya puede ver en su sesión

**Tokens de Autenticación:**
- Token JWT en localStorage es conveniente pero vulnerable a XSS
- **Alternativa más segura:** httpOnly cookies (requiere configuración especial)
- **Mitigación:** Implementar Content Security Policy (CSP)

**HTTPS Obligatorio:**
- Service Workers solo funcionan en HTTPS (excepto localhost)
- PWA no se puede instalar sin HTTPS
- **Acción:** Configurar certificado SSL en producción

**Validación en Backend:**
- **SIEMPRE** validar datos recibidos de la sincronización
- No confiar ciegamente en datos del cliente
- Verificar permisos del usuario antes de persistir eventos

### Mantenimiento

**Migración de Schema SQLite:**
- Cuando cambies el schema del backend, debes actualizar el schema de SQLite local
- **Estrategia:** Implementar versionado del schema
- Al detectar versión antigua, ejecutar migraciones en el cliente

**Monitoreo de Tamaño:**
- Implementar función para reportar tamaño de la base de datos
- Alertar al usuario si se acerca al límite
- Ofrecer opción de limpiar datos antiguos

**Versionado del Service Worker:**
- Cambiar nombre de caché cuando actualices la PWA
- Asegurar que usuarios obtengan la versión más reciente
- Implementar estrategia de actualización gradual

---

## 🔧 Troubleshooting Común

### Service Worker no se registra
**Síntomas:** No aparece en DevTools → Application → Service Workers

**Causas posibles:**
- No estás en HTTPS (solo localhost es excepción)
- Ruta incorrecta del archivo sw.js
- Error de sintaxis en sw.js

**Solución:**
- Verificar que sw.js está en /public/
- Verificar consola del navegador para errores
- Probar en localhost primero

---

### SQLite no se inicializa
**Síntomas:** Error al cargar sql.js o crear base de datos

**Causas posibles:**
- sql-wasm.wasm no está en /public/
- Ruta incorrecta en locateFile
- CORS bloqueando carga de WASM

**Solución:**
- Verificar que wasm existe: http://localhost:3000/sql-wasm.wasm
- Revisar next.config.js para configuración de archivos estáticos
- Verificar headers CORS

---

### Sincronización no funciona
**Síntomas:** Eventos pendientes no se sincronizan

**Causas posibles:**
- Endpoint /api/sync no existe o da error
- Token JWT expirado
- CORS bloqueando petición
- Backend no está corriendo

**Solución:**
- Verificar en Network tab de DevTools la petición
- Revisar response del servidor
- Verificar token en localStorage
- Probar endpoint con Postman

---

### Datos se pierden al cerrar navegador
**Síntomas:** SQLite se resetea cada vez

**Causas posibles:**
- saveDatabase() no se está llamando
- localStorage está lleno
- Navegador en modo incógnito
- Configuración del navegador borra datos al cerrar

**Solución:**
- Verificar llamadas a saveDatabase() después de cada operación
- Verificar tamaño de localStorage
- Probar en modo normal (no incógnito)
- Cambiar a IndexedDB si el problema persiste

---

### PWA no se puede instalar
**Síntomas:** No aparece opción de instalar

**Causas posibles:**
- manifest.json tiene errores
- No estás en HTTPS
- Service Worker no está registrado
- Manifest no está linkeado en HTML

**Solución:**
- Validar manifest.json con herramienta online
- Verificar en DevTools → Application → Manifest
- Revisar que Service Worker esté activo
- Verificar <link rel="manifest"> en layout

