# Configuración PWA en el Backend - GateKeep

## 📋 Tabla de Contenidos
1. [Visión General](#visión-general)
2. [Arquitectura PWA](#arquitectura-pwa)
3. [Configuración del Backend](#configuración-del-backend)
4. [API REST para PWA](#api-rest-para-pwa)
5. [Sincronización Offline](#sincronización-offline)
6. [Persistencia de Datos](#persistencia-de-datos)
7. [CORS y Seguridad](#cors-y-seguridad)
8. [Caché y Performance](#caché-y-performance)
9. [Deployment PWA](#deployment-pwa)
10. [Troubleshooting](#troubleshooting)

---

## Visión General

GateKeep implementa una **Progressive Web Application (PWA)** que funciona perfectamente tanto online como offline. El backend está diseñado para:

- ✅ Servir APIs REST que se sincronizan con SQLite local
- ✅ Procesar eventos registrados offline
- ✅ Mantener consistencia entre múltiples dispositivos
- ✅ Gestionar conflictos de datos
- ✅ Optimizar ancho de banda con sync incremental

### Stack Tecnológico

**Backend:**
- .NET 8 (C#)
- Entity Framework Core 9.0.0
- PostgreSQL (Base de datos principal)
- MongoDB (Auditoría)
- Redis (Caché)

**Frontend:**
- Next.js 15 (React 18)
- sql.js 1.13.0 (SQLite en WebAssembly)
- Service Workers
- IndexedDB
- next-pwa 5.6.0

---

## Arquitectura PWA

### 1. Flujo de Datos en PWA

```
┌─────────────────────────────────────────────────────────────┐
│                    APLICACIÓN FRONTEND (Next.js)            │
│  ┌──────────────────────────────────────────────────────┐   │
│  │   UI Components (React)                              │   │
│  │   - Eventos, Espacios, Usuarios, etc.               │   │
│  └──────────────────────────────────────────────────────┘   │
│                          ↕ (read/write)                      │
│  ┌──────────────────────────────────────────────────────┐   │
│  │   SyncProvider Context                               │   │
│  │   - Detecta conectividad (online/offline)           │   │
│  │   - Gestiona estado de sincronización               │   │
│  └──────────────────────────────────────────────────────┘   │
│                          ↕ (dispatch)                        │
│  ┌──────────────────────────────────────────────────────┐   │
│  │   sync.ts (Sync Client)                              │   │
│  │   - Orchestrador de sincronización                   │   │
│  │   - Detecta cambios offline                          │   │
│  │   - Construye SyncRequest                            │   │
│  └──────────────────────────────────────────────────────┘   │
│                          ↕ (POST /api/sync)                  │
│  ┌──────────────────────────────────────────────────────┐   │
│  │   SQLite (sql.js en WASM)                            │   │
│  │   - Tabla: eventos_offline                           │   │
│  │   - Tabla: sync_metadata                             │   │
│  │   - Tabla: usuarios, espacios, eventos              │   │
│  └──────────────────────────────────────────────────────┘   │
│                          ↕ (persist)                         │
│  ┌──────────────────────────────────────────────────────┐   │
│  │   IndexedDB                                          │   │
│  │   - Persistencia a largo plazo                       │   │
│  │   - Fallback si SQLite falla                         │   │
│  └──────────────────────────────────────────────────────┘   │
│                          ↕ (GET /api/sync)                   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │   Service Worker                                     │   │
│  │   - Intercept requests                               │   │
│  │   - Caché strategies                                 │   │
│  │   - Background sync                                  │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                            ↕ (HTTP)
┌─────────────────────────────────────────────────────────────┐
│                   BACKEND API (.NET 8)                       │
│  ┌──────────────────────────────────────────────────────┐   │
│  │   SyncController                                     │   │
│  │   - POST /api/sync -> Procesa eventos offline       │   │
│  │   - GET /api/sync -> Retorna datos actualizados     │   │
│  └──────────────────────────────────────────────────────┘   │
│                          ↕                                    │
│  ┌──────────────────────────────────────────────────────┐   │
│  │   SyncService                                        │   │
│  │   - ProcesarEventosAccesoOfflineAsync()             │   │
│  │   - ObtenerDatosActualizadosAsync()                 │   │
│  │   - ResolverConflictosAsync()                        │   │
│  └──────────────────────────────────────────────────────┘   │
│                          ↕ (Transacciones)                   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │   DbContext (EF Core)                                │   │
│  │   - Entidades sincronizables                         │   │
│  │   - Tracking de cambios                              │   │
│  │   - Migrations                                       │   │
│  └──────────────────────────────────────────────────────┘   │
│                          ↕ (SQL)                             │
│  ┌──────────────────────────────────────────────────────┐   │
│  │   PostgreSQL Database                                │   │
│  │   - Tabla: eventos_offline                           │   │
│  │   - Tabla: dispositivos_sync                         │   │
│  │   - Tablas normales con timestamps                   │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

### 2. Ciclo de Sincronización Completo

```
1. ONLINE - Usuario interactúa normalmente
   ├─ Cambios se guardan en SQLite local
   ├─ sync.ts detecta cambios
   └─ Se envía POST /api/sync con pendingEvents

2. BACKEND RECIBE SYNC REQUEST
   ├─ SyncController.SyncAsync(SyncRequest request)
   ├─ Valida JWT token
   ├─ Obtiene deviceId del cliente
   └─ Pasa a SyncService

3. SYNCSERVICE PROCESA EVENTOS OFFLINE
   ├─ Itera sobre cada EventoOfflineDto
   ├─ Crea entidad DispositivoSync
   ├─ Busca objeto original (Usuario, Evento, etc)
   ├─ Aplica cambios según tipo de evento
   └─ Guarda en DbContext

4. SYNCSERVICE OBTIENE DATOS ACTUALIZADOS
   ├─ Lee sync_metadata.ultima_sincronizacion
   ├─ Busca cambios desde esa fecha
   ├─ Construye SyncResponse con datos nuevos
   └─ Incluye metadata actualizada

5. BACKEND ENVÍA RESPUESTA
   ├─ HTTP 200 con SyncResponse
   ├─ Incluye datos sincronizados
   ├─ Timestamp del servidor
   └─ Status de cada evento procesado

6. FRONTEND RECIBE Y ACTUALIZA
   ├─ sync.ts procesa SyncResponse
   ├─ Actualiza SQLite local
   ├─ Actualiza React Context
   ├─ UI se re-renderiza
   └─ Usuario ve cambios automáticamente
```

---

## Configuración del Backend

### 1. Program.cs - Configuración Principal

```csharp
// En Program.cs (línea ~307-315)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
        builder
            .WithOrigins("http://localhost:3000", "https://tu-dominio.com")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .WithExposedHeaders("X-Total-Count", "X-Page-Count")
    );
});
```

### 2. Registrar SyncService en IoC Container

```csharp
// En Program.cs (después de línea ~400)
builder.Services.AddScoped<ISyncService, SyncService>();

// Registrar DbContext con PostgreSQL
builder.Services.AddDbContext<GateKeepDbContext>(options =>
    options.UseNpgsql(connectionString)
           .EnableSensitiveDataLogging(isDevelopment)
           .EnableDetailedErrors(isDevelopment)
);
```

### 3. Configurar Middleware para PWA

```csharp
// En Program.cs (línea ~723+)
// Middleware stack
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

// Headers necesarios para PWA
app.Use(async (context, next) =>
{
    // Permitir que el Service Worker se cachee
    if (context.Request.Path == "/service-worker.js")
    {
        context.Response.Headers["Cache-Control"] = "public, max-age=0, must-revalidate";
        context.Response.Headers["Service-Worker-Allowed"] = "/";
    }
    
    // Headers de seguridad
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    
    // Permitir JSON Web Tokens
    context.Response.Headers["Access-Control-Allow-Headers"] = 
        "Authorization, Content-Type, Accept";
    
    await next();
});

app.MapControllers();
app.Run();
```

---

## API REST para PWA

### 1. Endpoint Principal: /api/sync

**Descripción:** Sincroniza eventos offline y datos actualizados entre cliente y servidor.

#### POST /api/sync

**Propósito:** Enviar eventos grabados offline al servidor

**Headers Requeridos:**
```
Authorization: Bearer {jwt_token}
Content-Type: application/json
```

**Request Body (SyncRequest):**
```json
{
  "deviceId": "uuid-dispositivo-unico",
  "lastSyncTimestamp": "2025-11-18T14:30:00Z",
  "pendingEvents": [
    {
      "idTemporal": "temp-123",
      "tipoEvento": "CrearAcceso",
      "idEntidad": "usuario-456",
      "nombreEntidad": "Usuario",
      "cambios": {
        "nombre": "Felipe Rodríguez",
        "email": "felipe@example.com"
      },
      "timestamp": "2025-11-18T14:25:00Z",
      "intentos": 1
    }
  ]
}
```

**Response Body (SyncResponse):**
```json
{
  "success": true,
  "message": "Sincronización completada",
  "serverTimestamp": "2025-11-18T14:31:00Z",
  "processedEvents": [
    {
      "idTemporal": "temp-123",
      "success": true,
      "idReal": "usuario-456",
      "message": "Evento procesado correctamente"
    }
  ],
  "updatedData": {
    "usuarios": [
      {
        "id": "usuario-456",
        "nombre": "Felipe Rodríguez",
        "email": "felipe@example.com",
        "updatedAt": "2025-11-18T14:30:30Z"
      }
    ],
    "espacios": [],
    "eventos": []
  }
}
```

**Código de Respuesta:**
- `200 OK` - Sincronización exitosa
- `400 Bad Request` - Datos inválidos
- `401 Unauthorized` - Token JWT inválido
- `409 Conflict` - Conflicto de datos (requiere resolver)
- `500 Internal Server Error` - Error del servidor

#### GET /api/sync

**Propósito:** Obtener datos actualizados desde último sync

**Query Parameters:**
```
?lastSync=2025-11-18T14:30:00Z
&includeDeleted=false
```

**Response Body:**
```json
{
  "success": true,
  "updatedData": {
    "usuarios": [...],
    "espacios": [...],
    "eventos": [...]
  },
  "deletedIds": {
    "usuarios": ["deleted-id-1"],
    "espacios": [],
    "eventos": []
  },
  "serverTimestamp": "2025-11-18T14:31:00Z"
}
```

### 2. Entidades de Sincronización

#### SyncRequest (Contracts/Sync/SyncRequest.cs)

```csharp
public class SyncRequest
{
    public string DeviceId { get; set; }
    public DateTime LastSyncTimestamp { get; set; }
    public List<OfflineEventDto> PendingEvents { get; set; }
}

public class OfflineEventDto
{
    public string IdTemporal { get; set; }
    public string TipoEvento { get; set; }  // CrearAcceso, ModificarUsuario, etc
    public string IdEntidad { get; set; }
    public string NombreEntidad { get; set; }
    public Dictionary<string, object> Cambios { get; set; }
    public DateTime Timestamp { get; set; }
    public int Intentos { get; set; }
}
```

#### SyncResponse (Contracts/Sync/SyncResponse.cs)

```csharp
public class SyncResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public DateTime ServerTimestamp { get; set; }
    public List<ProcessedEventResult> ProcessedEvents { get; set; }
    public SyncDataResponse UpdatedData { get; set; }
}

public class ProcessedEventResult
{
    public string IdTemporal { get; set; }
    public bool Success { get; set; }
    public string IdReal { get; set; }
    public string Message { get; set; }
}
```

### 3. Controlador: SyncController.cs

```csharp
[ApiController]
[Route("api/[controller]")]
public class SyncController : ControllerBase
{
    private readonly ISyncService _syncService;
    private readonly ILogger<SyncController> _logger;

    public SyncController(ISyncService syncService, ILogger<SyncController> logger)
    {
        _syncService = syncService;
        _logger = logger;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<SyncResponse>> Sync([FromBody] SyncRequest request)
    {
        try
        {
            _logger.LogInformation("Sync iniciado para dispositivo: {DeviceId}", request.DeviceId);
            
            var response = await _syncService.SyncAsync(request);
            
            _logger.LogInformation("Sync completado: {EventCount} eventos procesados", 
                response.ProcessedEvents.Count);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en sincronización");
            return StatusCode(500, new SyncResponse 
            { 
                Success = false, 
                Message = $"Error: {ex.Message}" 
            });
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<SyncResponse>> GetUpdatedData(
        [FromQuery] DateTime lastSync, 
        [FromQuery] bool includeDeleted = false)
    {
        try
        {
            var response = await _syncService.ObtenerDatosActualizadosAsync(lastSync);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo datos actualizados");
            return StatusCode(500, new SyncResponse 
            { 
                Success = false, 
                Message = $"Error: {ex.Message}" 
            });
        }
    }
}
```

---

## Sincronización Offline

### 1. Flujo de Sincronización en SyncService.cs

```csharp
public class SyncService : ISyncService
{
    private readonly GateKeepDbContext _dbContext;
    private readonly ILogger<SyncService> _logger;

    public async Task<SyncResponse> SyncAsync(SyncRequest request)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        
        try
        {
            // 1. Procesar eventos offline
            var processedEvents = await ProcesarEventosAccesoOfflineAsync(request);

            // 2. Obtener datos actualizados
            var updatedData = await ObtenerDatosActualizadosAsync(request.LastSyncTimestamp);

            // 3. Crear respuesta
            var response = new SyncResponse
            {
                Success = true,
                Message = "Sincronización completada",
                ServerTimestamp = DateTime.UtcNow,
                ProcessedEvents = processedEvents,
                UpdatedData = updatedData
            };

            await transaction.CommitAsync();
            return response;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error en SyncAsync");
            throw;
        }
    }

    private async Task<List<ProcessedEventResult>> ProcesarEventosAccesoOfflineAsync(
        SyncRequest request)
    {
        var results = new List<ProcessedEventResult>();

        foreach (var evento in request.PendingEvents)
        {
            try
            {
                var result = evento.TipoEvento switch
                {
                    "CrearAcceso" => await ProcesarCrearAccesoAsync(evento),
                    "ModificarUsuario" => await ProcesarModificarUsuarioAsync(evento),
                    "CrearEvento" => await ProcesarCrearEventoAsync(evento),
                    _ => new ProcessedEventResult 
                    { 
                        IdTemporal = evento.IdTemporal,
                        Success = false,
                        Message = "Tipo de evento no reconocido"
                    }
                };

                results.Add(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando evento: {IdTemporal}", evento.IdTemporal);
                
                results.Add(new ProcessedEventResult
                {
                    IdTemporal = evento.IdTemporal,
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        return results;
    }

    private async Task<ProcessedEventResult> ProcesarCrearAccesoAsync(OfflineEventDto evento)
    {
        // 1. Obtener usuario desde cambios
        var usuarioId = evento.Cambios["usuarioId"].ToString();
        var usuario = await _dbContext.Usuarios.FindAsync(usuarioId);

        if (usuario == null)
            return new ProcessedEventResult 
            { 
                IdTemporal = evento.IdTemporal,
                Success = false,
                Message = "Usuario no encontrado"
            };

        // 2. Crear registro de acceso real
        var acceso = new Acceso
        {
            Id = Guid.NewGuid().ToString(),
            UsuarioId = usuarioId,
            FechaEntrada = DateTime.Parse(evento.Cambios["fechaEntrada"].ToString()),
            Estado = "completado",
            FechaCreacion = DateTime.UtcNow
        };

        _dbContext.Accesos.Add(acceso);
        await _dbContext.SaveChangesAsync();

        // 3. Crear registro en DispositivoSync
        var dispositivoSync = new DispositivoSync
        {
            Id = Guid.NewGuid().ToString(),
            DeviceId = evento.IdTemporal.Split('-')[0],
            TipoOperacion = "CrearAcceso",
            IdEntidadLocal = evento.IdTemporal,
            IdEntidadServidor = acceso.Id,
            Estado = "sincronizado",
            FechaCreacion = DateTime.UtcNow
        };

        _dbContext.DispositivoSyncs.Add(dispositivoSync);
        await _dbContext.SaveChangesAsync();

        return new ProcessedEventResult
        {
            IdTemporal = evento.IdTemporal,
            Success = true,
            IdReal = acceso.Id,
            Message = "Acceso registrado correctamente"
        };
    }
}
```

### 2. Tablas de Base de Datos para Sync

```sql
-- Tabla para rastrear dispositivos sincronizados
CREATE TABLE dispositivos_sync (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    device_id VARCHAR(255) NOT NULL,
    tipo_operacion VARCHAR(50),
    id_entidad_local VARCHAR(255),
    id_entidad_servidor UUID,
    estado VARCHAR(50),
    fecha_creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ultima_actualizacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Tabla para eventos grabados offline
CREATE TABLE eventos_offline (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    device_id VARCHAR(255) NOT NULL,
    tipo_evento VARCHAR(100),
    id_entidad VARCHAR(255),
    nombre_entidad VARCHAR(100),
    cambios JSONB,
    timestamp TIMESTAMP,
    intentos INT DEFAULT 1,
    fecha_creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ultima_actualizacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Índices para performance
CREATE INDEX idx_dispositivos_sync_device_id ON dispositivos_sync(device_id);
CREATE INDEX idx_dispositivos_sync_estado ON dispositivos_sync(estado);
CREATE INDEX idx_eventos_offline_device_id ON eventos_offline(device_id);
CREATE INDEX idx_eventos_offline_timestamp ON eventos_offline(timestamp);
```

---

## Persistencia de Datos

### 1. SQLite en el Frontend (sql.js)

SQLite se ejecuta completamente en el navegador usando WebAssembly:

```typescript
// frontend/src/lib/sqlite-db.ts
import initSqlJs, { Database, SqlValue } from 'sql.js';

class SqliteManager {
    private db: Database | null = null;
    private SQL: any = null;

    async initializeDatabase(): Promise<void> {
        this.SQL = await initSqlJs();
        
        // Intentar cargar desde IndexedDB
        const savedDb = await this.loadFromIndexedDB();
        
        if (savedDb) {
            this.db = new this.SQL.Database(savedDb);
        } else {
            this.db = new this.SQL.Database();
            await this.createTables();
        }
    }

    private async createTables(): Promise<void> {
        if (!this.db) return;

        const tables = `
            CREATE TABLE IF NOT EXISTS usuarios (
                id TEXT PRIMARY KEY,
                nombre TEXT,
                email TEXT,
                rol TEXT,
                updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS espacios (
                id TEXT PRIMARY KEY,
                nombre TEXT,
                ubicacion TEXT,
                capacidad INTEGER,
                updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS eventos (
                id TEXT PRIMARY KEY,
                titulo TEXT,
                descripcion TEXT,
                fecha_inicio TIMESTAMP,
                fecha_fin TIMESTAMP,
                updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS eventos_offline (
                id TEXT PRIMARY KEY,
                id_temporal TEXT,
                tipo_evento TEXT,
                id_entidad TEXT,
                nombre_entidad TEXT,
                cambios TEXT,
                timestamp TIMESTAMP,
                intentos INTEGER DEFAULT 1,
                sincronizado BOOLEAN DEFAULT 0
            );

            CREATE TABLE IF NOT EXISTS sync_metadata (
                key TEXT PRIMARY KEY,
                value TEXT,
                updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            );
        `;

        this.db.run(tables);
        await this.saveToIndexedDB();
    }

    async recordOfflineEvent(evento: OfflineEvent): Promise<void> {
        if (!this.db) return;

        const stmt = this.db.prepare(`
            INSERT INTO eventos_offline 
            (id, id_temporal, tipo_evento, id_entidad, nombre_entidad, cambios, timestamp, intentos)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?)
        `);

        stmt.bind([
            this.generateId(),
            evento.idTemporal,
            evento.tipoEvento,
            evento.idEntidad,
            evento.nombreEntidad,
            JSON.stringify(evento.cambios),
            new Date().toISOString(),
            1
        ]);

        stmt.step();
        stmt.free();
        
        await this.saveToIndexedDB();
    }

    async getPendingOfflineEvents(): Promise<OfflineEvent[]> {
        if (!this.db) return [];

        const stmt = this.db.prepare(`
            SELECT * FROM eventos_offline WHERE sincronizado = 0
        `);

        const events: OfflineEvent[] = [];
        while (stmt.step()) {
            const row = stmt.getAsObject();
            events.push({
                idTemporal: String(row.id_temporal),
                tipoEvento: String(row.tipo_evento),
                idEntidad: String(row.id_entidad),
                nombreEntidad: String(row.nombre_entidad),
                cambios: JSON.parse(String(row.cambios)),
                timestamp: new Date(String(row.timestamp)),
                intentos: Number(row.intentos) || 1
            });
        }
        stmt.free();

        return events;
    }

    private async saveToIndexedDB(): Promise<void> {
        if (!this.db) return;

        const data = this.db.export();
        const arr = Array.from(data);
        
        const db = await new Promise<IDBDatabase>((resolve) => {
            const req = indexedDB.open('gatekeep-db', 1);
            req.onsuccess = () => resolve(req.result as IDBDatabase);
        });

        const tx = db.transaction('sqlite', 'readwrite');
        tx.objectStore('sqlite').put(arr, 'database');
    }

    private async loadFromIndexedDB(): Promise<Uint8Array | null> {
        return new Promise((resolve) => {
            const req = indexedDB.open('gatekeep-db', 1);
            
            req.onsuccess = () => {
                const db = req.result as IDBDatabase;
                const tx = db.transaction('sqlite', 'readonly');
                const get = tx.objectStore('sqlite').get('database');
                
                get.onsuccess = () => {
                    const data = get.result as number[] | undefined;
                    resolve(data ? new Uint8Array(data) : null);
                };
            };
            
            req.onerror = () => resolve(null);
        });
    }
}
```

### 2. Estrategia de Persistencia Multi-Capas

```
┌─────────────────────────────────────────┐
│  1. MEMORIA (React Context)             │ ← Más rápido, temporal
│  └─ Estado actual de la aplicación      │
├─────────────────────────────────────────┤
│  2. SQLite (WASM Browser)               │ ← Semi-persistente
│  └─ Base de datos local completa        │
├─────────────────────────────────────────┤
│  3. IndexedDB (Browser Storage)         │ ← Persistencia fallback
│  └─ Binario SQLite exportado            │
├─────────────────────────────────────────┤
│  4. PostgreSQL (Server)                 │ ← Fuente de verdad
│  └─ Base de datos central               │
└─────────────────────────────────────────┘
```

### 3. Timestamps Automáticos en Base de Datos

```csharp
// En GateKeepDbContext.cs
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    var entries = ChangeTracker.Entries()
        .Where(e => e.Entity is ISyncable && e.State != EntityState.Unchanged)
        .ToList();

    foreach (var entry in entries)
    {
        if (entry.State == EntityState.Added)
        {
            entry.Property(nameof(ISyncable.FechaCreacion)).CurrentValue = DateTime.UtcNow;
        }
        
        if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
        {
            entry.Property(nameof(ISyncable.UltimaActualizacion)).CurrentValue = DateTime.UtcNow;
        }
    }

    return await base.SaveChangesAsync(cancellationToken);
}

// Interface para todas las entidades sincronizables
public interface ISyncable
{
    DateTime FechaCreacion { get; set; }
    DateTime UltimaActualizacion { get; set; }
}
```

---

## CORS y Seguridad

### 1. Configuración CORS para PWA

```csharp
// En Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
        builder
            .WithOrigins(
                "http://localhost:3000",                    // Desarrollo
                "https://zimmzimmgames.com",               // Producción
                "https://www.zimmzimmgames.com",           // www
                "https://api.zimmzimmgames.com"            // API subdomain
            )
            .AllowAnyMethod()                      // GET, POST, PUT, DELETE, etc
            .AllowAnyHeader()                      // Content-Type, Authorization, etc
            .AllowCredentials()                    // Cookies y auth headers
            .WithExposedHeaders(
                "X-Total-Count",                   // Para paginación
                "X-Page-Count",
                "X-Request-Id"                     // Para trazabilidad
            )
            .SetPreflightMaxAge(TimeSpan.FromSeconds(3600))
    );
});
```

### 2. Seguridad en Headers

```csharp
// Middleware personalizado en Program.cs
app.Use(async (context, next) =>
{
    // Headers de seguridad estándar
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
    
    // Específico para Service Worker
    if (context.Request.Path == "/service-worker.js")
    {
        context.Response.Headers["Cache-Control"] = "public, max-age=0, must-revalidate";
        context.Response.Headers["Service-Worker-Allowed"] = "/";
    }
    
    // Específico para PWA Manifest
    if (context.Request.Path == "/manifest.json")
    {
        context.Response.Headers["Content-Type"] = "application/manifest+json";
        context.Response.Headers["Cache-Control"] = "public, max-age=86400";
    }
    
    await next();
});
```

### 3. JWT Authentication para PWA

```csharp
// En Program.cs
var jwtSettings = builder.Configuration.GetSection("JwtSettings");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"])),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };
        
        // Configuración para PWA (eventos offline sin JWT)
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    context.Response.Headers.Add("Token-Expired", "true");
                }
                return Task.CompletedTask;
            }
        };
    });
```

---

## Caché y Performance

### 1. Redis para Caché en Backend

```csharp
// En Program.cs
builder.Services.AddStackExchangeRedisCache(options =>
{
    var redisConnection = builder.Configuration.GetConnectionString("Redis");
    options.Configuration = redisConnection;
    options.InstanceName = "GateKeep:";
});

// En SyncService - Caché de datos frecuentes
public class SyncService : ISyncService
{
    private readonly IDistributedCache _cache;
    
    public async Task<List<UsuarioDto>> GetUsuariosActualizadosAsync(DateTime since)
    {
        var cacheKey = $"usuarios:updated:{since:yyyyMMddHHmm}";
        
        var cached = await _cache.GetStringAsync(cacheKey);
        if (cached != null)
            return JsonSerializer.Deserialize<List<UsuarioDto>>(cached);

        var usuarios = await _dbContext.Usuarios
            .Where(u => u.UltimaActualizacion >= since)
            .ToListAsync();

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        await _cache.SetStringAsync(cacheKey, 
            JsonSerializer.Serialize(usuarios), 
            options);

        return usuarios;
    }
}
```

### 2. Estrategia de Caché del Frontend

```typescript
// En sync.ts - Control de caché
const CACHE_STRATEGIES = {
    // Cache-First: Ideal para datos estáticos (usuarios, espacios)
    "usuarios": {
        version: 1,
        maxAge: 1800000 // 30 minutos
    },
    
    // Network-First: Ideal para datos dinámicos (eventos)
    "eventos": {
        version: 1,
        maxAge: 300000 // 5 minutos
    },
    
    // Stale-While-Revalidate: Mostrar viejo mientras actualiza
    "accesos": {
        version: 1,
        maxAge: 600000 // 10 minutos
    }
};
```

### 3. Service Worker - Estrategias de Caché

```typescript
// frontend/public/service-worker.js
self.addEventListener('fetch', (event) => {
    const { request } = event;
    const url = new URL(request.url);

    // API de sync - Network First
    if (url.pathname.startsWith('/api/sync')) {
        event.respondWith(
            fetch(request)
                .then(response => {
                    const responseClone = response.clone();
                    caches.open('sync-cache').then(cache => {
                        cache.put(request, responseClone);
                    });
                    return response;
                })
                .catch(() => caches.match(request))
        );
        return;
    }

    // Assets estáticos - Cache First
    if (url.pathname.match(/\.(js|css|png|jpg|svg)$/)) {
        event.respondWith(
            caches.match(request)
                .then(response => response || fetch(request))
        );
        return;
    }

    // Por defecto - Network First con fallback
    event.respondWith(
        fetch(request)
            .catch(() => caches.match('/offline.html'))
    );
});
```

---

## Deployment PWA

### 1. Configuración en docker-compose.prod.yml

```yaml
version: '3.8'

services:
  gatekeep-backend:
    build:
      context: ./Gatekeep/src
      dockerfile: Dockerfile
    environment:
      - DOTNET_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:5011
      - CORS_ORIGINS=https://zimmzimmgames.com,https://www.zimmzimmgames.com,https://api.zimmzimmgames.com
      - DATABASE_URL=postgresql://user:pass@postgres:5432/gatekeep
      - REDIS_URL=redis://redis:6379
    ports:
      - "5011:5011"
    depends_on:
      - postgres
      - redis
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5011/health"]
      interval: 30s
      timeout: 10s
      retries: 3

  gatekeep-frontend:
    build:
      context: ./Gatekeep/frontend
      dockerfile: Dockerfile
    environment:
      - NEXT_PUBLIC_API_URL=https://api.zimmzimmgames.com
      - NODE_ENV=production
    ports:
      - "3000:3000"
    depends_on:
      - gatekeep-backend

  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf
      - ./certs:/etc/nginx/certs
    depends_on:
      - gatekeep-backend
      - gatekeep-frontend

  postgres:
    image: postgres:15-alpine
    environment:
      - POSTGRES_DB=gatekeep
      - POSTGRES_USER=gatekeep
      - POSTGRES_PASSWORD=secure_password
    volumes:
      - postgres_data:/var/lib/postgresql/data

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"

volumes:
  postgres_data:
```

### 2. Configuración de Nginx para PWA

```nginx
# nginx.conf
upstream backend {
    server gatekeep-backend:5011;
}

upstream frontend {
    server gatekeep-frontend:3000;
}

# Redirect HTTP to HTTPS
server {
    listen 80;
    listen [::]:80;
    server_name zimmzimmgames.com www.zimmzimmgames.com api.zimmzimmgames.com;

    # Redirigir HTTP a HTTPS
    return 301 https://$server_name$request_uri;
}

# HTTPS Main Server
server {
    listen 443 ssl http2;
    listen [::]:443 ssl http2;
    server_name zimmzimmgames.com www.zimmzimmgames.com api.zimmzimmgames.com;

    # SSL Configuration (Let's Encrypt)
    ssl_certificate /etc/letsencrypt/live/zimmzimmgames.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/zimmzimmgames.com/privkey.pem;

    # SSL Best Practices
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;
    ssl_prefer_server_ciphers on;
    ssl_session_cache shared:SSL:10m;
    ssl_session_timeout 10m;
    ssl_stapling on;
    ssl_stapling_verify on;
    resolver 8.8.8.8 8.8.4.4 valid=300s;
    resolver_timeout 5s;

    # Headers de seguridad para PWA
    add_header Strict-Transport-Security "max-age=31536000; includeSubDomains; preload" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-XSS-Protection "1; mode=block" always;

    # Service Worker
    location = /service-worker.js {
        proxy_pass http://frontend;
        proxy_cache off;
        add_header Cache-Control "public, max-age=0, must-revalidate";
        add_header Service-Worker-Allowed "/";
    }

    # PWA Manifest
    location = /manifest.json {
        proxy_pass http://frontend;
        add_header Content-Type "application/manifest+json";
        add_header Cache-Control "public, max-age=86400";
    }

    # API Backend
    location /api/ {
        proxy_pass http://backend;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # Frontend
    location / {
        proxy_pass http://frontend;
        proxy_set_header Host $host;
        proxy_buffering off;
    }
}
```

---

---

## Configuración del Frontend (Next.js PWA)

### 1. Estructura del Proyecto Frontend

```
frontend/
├── public/
│   ├── manifest.json          # PWA manifest
│   ├── service-worker.js      # Service Worker
│   ├── offline.html           # Página offline fallback
│   └── assets/
│       ├── icon-192x192.png
│       ├── icon-512x512.png
│       └── ...
├── src/
│   ├── app/
│   │   ├── layout.js          # Root layout
│   │   ├── page.jsx           # Home page
│   │   ├── globals.css        # Estilos globales
│   │   └── ...
│   ├── components/
│   │   ├── Header.jsx
│   │   ├── SyncStatus.jsx     # Indicador de sync
│   │   └── ...
│   ├── contexts/
│   │   ├── SyncContext.jsx    # Context para sync status
│   │   └── UserContext.jsx
│   ├── lib/
│   │   ├── sqlite-db.ts       # SQLite manager
│   │   ├── sync.ts            # Sync client
│   │   ├── SyncProvider.jsx   # Context Provider
│   │   └── offlineEvents.js   # Offline event types
│   ├── services/
│   │   ├── api.ts             # API client
│   │   └── auth.ts            # Auth service
│   ├── hooks/
│   │   ├── useSync.ts         # Hook para sync
│   │   ├── useOnline.ts       # Hook para conectividad
│   │   └── useLocalStorage.ts
│   └── utils/
│       └── helpers.ts
├── cypress/
│   ├── e2e/
│   ├── support/
│   └── config.ts
├── next.config.js
├── package.json
├── jest.config.js
├── tsconfig.json
└── Dockerfile
```

### 2. next.config.js - Configuración PWA

```javascript
import { fileURLToPath } from 'url'
import { dirname } from 'path'

const __filename = fileURLToPath(import.meta.url)
const __dirname = dirname(__filename)

const isDev = process.env.NODE_ENV !== 'production'

/** @type {import('next').NextConfig} */
const nextConfig = {
    // Configuración de imágenes
    images: {
        domains: ['api.tu-dominio.com'],
        unoptimized: isDev // No optimizar en desarrollo
    },

    // Transpile de librerías externas
    transpilePackages: ['primereact', 'primeicons', 'primeflex'],

    // Headers personalizados
    async headers() {
        return [
            {
                source: '/service-worker.js',
                headers: [
                    {
                        key: 'Cache-Control',
                        value: 'public, max-age=0, must-revalidate'
                    },
                    {
                        key: 'Service-Worker-Allowed',
                        value: '/'
                    },
                    {
                        key: 'Content-Type',
                        value: 'application/javascript'
                    }
                ]
            },
            {
                source: '/manifest.json',
                headers: [
                    {
                        key: 'Content-Type',
                        value: 'application/manifest+json'
                    },
                    {
                        key: 'Cache-Control',
                        value: 'public, max-age=86400'
                    }
                ]
            }
        ];
    },

    // Reescritura de URLs
    async rewrites() {
        return {
            beforeFiles: [
                {
                    source: '/api/:path*',
                    destination: `${process.env.NEXT_PUBLIC_API_URL}/api/:path*`
                }
            ]
        };
    },

    // Webpack personalizado
    webpack(config) {
        config.module.rules.push({
            test: /\.svg$/,
            use: ['@svgr/webpack']
        });
        return config;
    },

    outputFileTracingRoot: __dirname,
};

export default nextConfig;
```

### 3. package.json - Dependencias PWA

```json
{
    "name": "gatekeep-pwa",
    "version": "1.0.0",
    "private": true,
    "type": "module",
    "scripts": {
        "dev": "next dev",
        "build": "next build",
        "start": "next start",
        "lint": "next lint",
        "test": "jest",
        "test:coverage": "jest --coverage",
        "test:e2e": "cypress open",
        "test:e2e:headless": "cypress run"
    },
    "dependencies": {
        "react": "^18.2.0",
        "react-dom": "^18.2.0",
        "next": "^15.5.6",
        "next-pwa": "^5.6.0",
        "primereact": "^10.9.4",
        "primeicons": "^7.0.0",
        "primeflex": "^4.0.0",
        "sql.js": "^1.13.0",
        "axios": "^1.13.2",
        "idb": "^8.0.3",
        "dexie": "^4.2.1",
        "react-router-dom": "^7.9.4"
    },
    "devDependencies": {
        "@types/react": "^18.2.64",
        "@types/react-dom": "^18.2.21",
        "@types/jest": "^30.0.0",
        "@types/sql.js": "^1.4.9",
        "@svgr/webpack": "^8.1.0",
        "@testing-library/react": "^16.3.0",
        "@testing-library/jest-dom": "^6.9.1",
        "ts-jest": "^29.4.5",
        "jest": "^30.2.0",
        "jest-environment-jsdom": "^30.2.0",
        "typescript": "^5.3.0",
        "eslint": "^8.57.0",
        "cypress": "^15.6.0"
    }
}
```

### 4. Service Worker - frontend/public/service-worker.js

```javascript
// Nombre del cache
const CACHE_VERSION = 'v1';
const CACHE_NAME = `gatekeep-${CACHE_VERSION}`;
const SYNC_CACHE = 'sync-cache';
const ASSET_CACHE = 'asset-cache';

// Archivos estáticos que cachear
const PRECACHE_URLS = [
    '/',
    '/offline.html',
    '/manifest.json',
    '/favicon.ico'
];

// Instalar Service Worker
self.addEventListener('install', (event) => {
    console.log('[SW] Installing Service Worker...');
    
    event.waitUntil(
        caches.open(CACHE_NAME).then((cache) => {
            console.log('[SW] Pre-caching urls');
            return cache.addAll(PRECACHE_URLS);
        }).then(() => {
            console.log('[SW] Skip waiting');
            return self.skipWaiting();
        })
    );
});

// Activar Service Worker
self.addEventListener('activate', (event) => {
    console.log('[SW] Activating Service Worker');
    
    event.waitUntil(
        caches.keys().then((cacheNames) => {
            return Promise.all(
                cacheNames.map((cacheName) => {
                    if (cacheName !== CACHE_NAME && 
                        cacheName !== SYNC_CACHE && 
                        cacheName !== ASSET_CACHE) {
                        console.log(`[SW] Deleting old cache: ${cacheName}`);
                        return caches.delete(cacheName);
                    }
                })
            );
        }).then(() => self.clients.claim())
    );
});

// Intercept fetch requests
self.addEventListener('fetch', (event) => {
    const { request } = event;
    const url = new URL(request.url);

    // Skip non-GET requests
    if (request.method !== 'GET') {
        return;
    }

    // API de sincronización - Network First
    if (url.pathname.startsWith('/api/sync')) {
        event.respondWith(
            fetch(request)
                .then((response) => {
                    const responseClone = response.clone();
                    caches.open(SYNC_CACHE).then((cache) => {
                        cache.put(request, responseClone);
                    });
                    return response;
                })
                .catch(() => caches.match(request))
        );
        return;
    }

    // Otras APIs - Network First con fallback
    if (url.pathname.startsWith('/api/')) {
        event.respondWith(
            fetch(request)
                .then((response) => {
                    if (response.status === 200) {
                        const responseClone = response.clone();
                        caches.open(CACHE_NAME).then((cache) => {
                            cache.put(request, responseClone);
                        });
                    }
                    return response;
                })
                .catch(() => caches.match(request))
        );
        return;
    }

    // Assets estáticos - Cache First
    if (url.pathname.match(/\.(js|css|png|jpg|jpeg|svg|woff|woff2)$/i)) {
        event.respondWith(
            caches.match(request)
                .then((response) => {
                    if (response) {
                        return response;
                    }
                    return fetch(request).then((response) => {
                        if (response.status === 200) {
                            const responseClone = response.clone();
                            caches.open(ASSET_CACHE).then((cache) => {
                                cache.put(request, responseClone);
                            });
                        }
                        return response;
                    });
                })
                .catch(() => caches.match('/offline.html'))
        );
        return;
    }

    // Por defecto - Network First
    event.respondWith(
        fetch(request)
            .catch(() => {
                // Si estamos offline, retornar página offline
                return caches.match('/offline.html');
            })
    );
});

// Background Sync
self.addEventListener('sync', (event) => {
    console.log('[SW] Background sync triggered');
    
    if (event.tag === 'sync-offline-events') {
        event.waitUntil(
            // Notificar al cliente para que sincronice
            self.clients.matchAll().then((clients) => {
                clients.forEach((client) => {
                    client.postMessage({
                        type: 'SYNC_REQUIRED',
                        data: { timestamp: new Date().toISOString() }
                    });
                });
            })
        );
    }
});

// Mensajes desde el cliente
self.addEventListener('message', (event) => {
    if (event.data && event.data.type === 'SKIP_WAITING') {
        self.skipWaiting();
    }
    
    if (event.data && event.data.type === 'CLEAR_CACHE') {
        const cacheName = event.data.cacheName;
        caches.delete(cacheName).then(() => {
            event.ports[0].postMessage({ success: true });
        });
    }
});

// Push notifications
self.addEventListener('push', (event) => {
    const data = event.data?.json() ?? {};
    const title = data.title || 'GateKeep Notification';
    const options = {
        body: data.body || 'Nuevo evento de sincronización',
        icon: '/icon-192x192.png',
        badge: '/badge-72x72.png',
        tag: 'gatekeep-notification',
        requireInteraction: false
    };

    event.waitUntil(
        self.registration.showNotification(title, options)
    );
});

// Notificación click
self.addEventListener('notificationclick', (event) => {
    event.notification.close();
    event.waitUntil(
        clients.matchAll({ type: 'window' }).then((clientList) => {
            for (let i = 0; i < clientList.length; i++) {
                const client = clientList[i];
                if (client.url === '/' && 'focus' in client) {
                    return client.focus();
                }
            }
            if (clients.openWindow) {
                return clients.openWindow('/');
            }
        })
    );
});
```

### 5. manifest.json - PWA Manifest

```json
{
    "name": "GateKeep - Sistema de Acceso",
    "short_name": "GateKeep",
    "description": "Aplicación PWA para gestión de acceso a espacios con sincronización offline",
    "start_url": "/",
    "scope": "/",
    "display": "standalone",
    "orientation": "portrait-primary",
    "background_color": "#ffffff",
    "theme_color": "#0070f3",
    "categories": ["productivity", "business"],
    "screenshots": [
        {
            "src": "/assets/screenshot-1.png",
            "sizes": "540x720",
            "type": "image/png",
            "form_factor": "narrow"
        },
        {
            "src": "/assets/screenshot-2.png",
            "sizes": "1280x720",
            "type": "image/png",
            "form_factor": "wide"
        }
    ],
    "icons": [
        {
            "src": "/icon-72x72.png",
            "sizes": "72x72",
            "type": "image/png",
            "purpose": "any"
        },
        {
            "src": "/icon-96x96.png",
            "sizes": "96x96",
            "type": "image/png",
            "purpose": "any"
        },
        {
            "src": "/icon-128x128.png",
            "sizes": "128x128",
            "type": "image/png",
            "purpose": "any"
        },
        {
            "src": "/icon-144x144.png",
            "sizes": "144x144",
            "type": "image/png",
            "purpose": "any"
        },
        {
            "src": "/icon-152x152.png",
            "sizes": "152x152",
            "type": "image/png",
            "purpose": "any"
        },
        {
            "src": "/icon-192x192.png",
            "sizes": "192x192",
            "type": "image/png",
            "purpose": "any"
        },
        {
            "src": "/icon-384x384.png",
            "sizes": "384x384",
            "type": "image/png",
            "purpose": "any"
        },
        {
            "src": "/icon-512x512.png",
            "sizes": "512x512",
            "type": "image/png",
            "purpose": "any"
        },
        {
            "src": "/maskable-192x192.png",
            "sizes": "192x192",
            "type": "image/png",
            "purpose": "maskable"
        },
        {
            "src": "/maskable-512x512.png",
            "sizes": "512x512",
            "type": "image/png",
            "purpose": "maskable"
        }
    ],
    "shortcuts": [
        {
            "name": "Registrar Acceso",
            "short_name": "Acceso",
            "description": "Registra un nuevo acceso a un espacio",
            "url": "/acceso/crear?utm_source=shortcut",
            "icons": [
                {
                    "src": "/icon-192x192.png",
                    "sizes": "192x192"
                }
            ]
        },
        {
            "name": "Ver Eventos",
            "short_name": "Eventos",
            "description": "Visualiza los eventos próximos",
            "url": "/evento?utm_source=shortcut",
            "icons": [
                {
                    "src": "/icon-192x192.png",
                    "sizes": "192x192"
                }
            ]
        }
    ],
    "share_target": {
        "action": "/share",
        "method": "POST",
        "enctype": "multipart/form-data",
        "params": {
            "title": "title",
            "text": "text",
            "url": "url",
            "files": [
                {
                    "name": "file",
                    "accept": ["image/*"]
                }
            ]
        }
    }
}
```

### 6. SyncProvider - Contexto de Sincronización

```jsx
// src/lib/SyncProvider.jsx
'use client';

import React, { createContext, useState, useCallback, useEffect } from 'react';
import { syncWithServer } from './sync';
import { getPendingOfflineEvents } from './sqlite-db';

export const SyncContext = createContext();

export function SyncProvider({ children }) {
    const [syncStatus, setSyncStatus] = useState('idle'); // idle, syncing, online, offline
    const [lastSync, setLastSync] = useState(null);
    const [pendingEvents, setPendingEvents] = useState(0);
    const [isOnline, setIsOnline] = useState(true);

    // Detectar conectividad
    useEffect(() => {
        const handleOnline = () => {
            setIsOnline(true);
            setSyncStatus('online');
            performSync();
        };

        const handleOffline = () => {
            setIsOnline(false);
            setSyncStatus('offline');
        };

        window.addEventListener('online', handleOnline);
        window.addEventListener('offline', handleOffline);

        return () => {
            window.removeEventListener('online', handleOnline);
            window.removeEventListener('offline', handleOffline);
        };
    }, []);

    // Contar eventos pendientes
    useEffect(() => {
        const checkPendingEvents = async () => {
            const events = await getPendingOfflineEvents();
            setPendingEvents(events.length);
        };

        checkPendingEvents();
        const interval = setInterval(checkPendingEvents, 5000);

        return () => clearInterval(interval);
    }, []);

    const performSync = useCallback(async () => {
        if (!isOnline || syncStatus === 'syncing') return;

        setSyncStatus('syncing');
        try {
            const token = localStorage.getItem('authToken');
            const response = await syncWithServer(token);

            if (response.success) {
                setLastSync(new Date());
                setSyncStatus('online');
                setPendingEvents(0);
                
                // Notificar a componentes que escuchen cambios
                window.dispatchEvent(new CustomEvent('sync-complete', {
                    detail: { data: response.updatedData }
                }));
            }
        } catch (error) {
            console.error('Sync error:', error);
            setSyncStatus('error');
            setTimeout(() => setSyncStatus('offline'), 3000);
        }
    }, [isOnline, syncStatus]);

    return (
        <SyncContext.Provider value={{
            syncStatus,
            lastSync,
            pendingEvents,
            isOnline,
            performSync
        }}>
            {children}
        </SyncContext.Provider>
    );
}
```

### 7. Hook useSync - Usar sincronización

```typescript
// src/hooks/useSync.ts
import { useContext, useCallback, useEffect, useState } from 'react';
import { SyncContext } from '@/lib/SyncProvider';

export function useSync() {
    const context = useContext(SyncContext);
    const [shouldSync, setShouldSync] = useState(false);

    if (!context) {
        throw new Error('useSync debe estar dentro de SyncProvider');
    }

    useEffect(() => {
        // Sincronizar cada 30 segundos si estamos online
        if (context.isOnline) {
            const interval = setInterval(() => {
                context.performSync();
            }, 30000);

            return () => clearInterval(interval);
        }
    }, [context.isOnline, context.performSync]);

    const triggerSync = useCallback(async () => {
        await context.performSync();
    }, [context]);

    return {
        ...context,
        triggerSync
    };
}
```

### 8. Componente SyncStatus - Indicador de Sincronización

```jsx
// src/components/SyncStatus.jsx
'use client';

import React from 'react';
import { useSync } from '@/hooks/useSync';

export function SyncStatus() {
    const { syncStatus, pendingEvents, isOnline, lastSync } = useSync();

    const getStatusIcon = () => {
        switch (syncStatus) {
            case 'syncing':
                return '⟳'; // Spinner
            case 'online':
                return '✓';
            case 'offline':
                return '✗';
            default:
                return '○';
        }
    };

    const getStatusColor = () => {
        switch (syncStatus) {
            case 'syncing':
                return 'text-yellow-500';
            case 'online':
                return 'text-green-500';
            case 'offline':
                return 'text-red-500';
            default:
                return 'text-gray-500';
        }
    };

    const getStatusText = () => {
        switch (syncStatus) {
            case 'syncing':
                return 'Sincronizando...';
            case 'online':
                return pendingEvents > 0 
                    ? `${pendingEvents} evento(s) pendiente(s)`
                    : 'Sincronizado';
            case 'offline':
                return `Offline - ${pendingEvents} cambio(s)`;
            default:
                return 'Estado desconocido';
        }
    };

    return (
        <div className={`flex items-center gap-2 px-3 py-1 rounded ${getStatusColor()}`}>
            <span className="text-lg">{getStatusIcon()}</span>
            <span className="text-sm font-medium">{getStatusText()}</span>
            {lastSync && (
                <span className="text-xs opacity-70">
                    ({lastSync.toLocaleTimeString()})
                </span>
            )}
        </div>
    );
}
```

### 9. app/layout.js - Root Layout

```jsx
import './globals.css';
import { SyncProvider } from '@/lib/SyncProvider';
import { SyncStatus } from '@/components/SyncStatus';

export const metadata = {
    title: 'GateKeep - Sistema de Acceso',
    description: 'Aplicación PWA para gestión de acceso',
    manifest: '/manifest.json',
    appleWebApp: {
        capable: true,
        statusBarStyle: 'black-translucent',
        title: 'GateKeep'
    },
    formatDetection: {
        telephone: false
    }
};

export default function RootLayout({ children }) {
    return (
        <html lang="es">
            <head>
                <meta charSet="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1" />
                <meta name="theme-color" content="#0070f3" />
                <link rel="icon" href="/favicon.ico" />
                <link rel="apple-touch-icon" href="/icon-192x192.png" />
                <link rel="manifest" href="/manifest.json" />
            </head>
            <body>
                <SyncProvider>
                    <header className="bg-blue-600 text-white p-4 flex justify-between items-center">
                        <h1>GateKeep</h1>
                        <SyncStatus />
                    </header>
                    <main className="min-h-screen">
                        {children}
                    </main>
                </SyncProvider>

                {/* Registrar Service Worker */}
                <script
                    dangerouslySetInnerHTML={{
                        __html: `
                            if ('serviceWorker' in navigator) {
                                window.addEventListener('load', () => {
                                    navigator.serviceWorker.register('/service-worker.js')
                                        .then(reg => console.log('SW registered'))
                                        .catch(err => console.log('SW registration failed:', err));
                                });
                            }
                        `
                    }}
                />
            </body>
        </html>
    );
}
```

### 10. Dockerfile - Build optimizado

```dockerfile
# Build stage
FROM node:20-alpine AS builder

WORKDIR /app

COPY package*.json ./

RUN npm ci

COPY . .

RUN npm run build

# Production stage
FROM node:20-alpine

WORKDIR /app

ENV NODE_ENV=production

COPY package*.json ./

RUN npm ci --only=production

COPY --from=builder /app/.next ./.next
COPY --from=builder /app/public ./public
COPY --from=builder /app/next.config.js ./

EXPOSE 3000

CMD ["npm", "start"]
```

### 11. tsconfig.json - Configuración TypeScript

```json
{
    "compilerOptions": {
        "target": "ES2020",
        "useDefineForClassFields": true,
        "lib": ["ES2020", "DOM", "DOM.Iterable"],
        "module": "ESNext",
        "skipLibCheck": true,
        "esModuleInterop": true,
        "allowSyntheticDefaultImports": true,

        /* Bundling mode */
        "moduleResolution": "bundler",
        "allowImportingTsExtensions": true,
        "resolveJsonModule": true,
        "isolatedModules": true,
        "noEmit": true,
        "jsx": "react-jsx",

        /* Linting */
        "strict": true,
        "noUnusedLocals": true,
        "noUnusedParameters": true,
        "noFallthroughCasesInSwitch": true,

        /* Rutas */
        "baseUrl": ".",
        "paths": {
            "@/*": ["./src/*"]
        }
    },
    "include": ["src"],
    "exclude": ["node_modules", "dist", ".next"]
}
```

---

## Troubleshooting

**Causa:** El archivo service-worker.js no tiene los headers correctos

**Solución:**
```csharp
// En middleware
if (context.Request.Path == "/service-worker.js")
{
    context.Response.Headers["Cache-Control"] = "public, max-age=0, must-revalidate";
    context.Response.Headers["Service-Worker-Allowed"] = "/";
    context.Response.ContentType = "application/javascript";
}
```

### Problema 2: "CORS error al sincronizar"

**Causa:** No se permite credenciales en CORS

**Solución:**
```csharp
options.AddPolicy("AllowFrontend", builder =>
    builder
        .WithOrigins("https://tu-dominio.com")
        .AllowCredentials()  // ← Agregar esto
        .AllowAnyHeader()
        .AllowAnyMethod()
);
```

### Problema 3: "SQLite: 'database disk image is malformed'"

**Causa:** Corrupción en IndexedDB

**Solución:**
```typescript
// Limpiar storage
await indexedDB.deleteDatabase('gatekeep-db');
localStorage.clear();
// Reinicializar
```

### Problema 4: "JWT token expirado en PWA offline"

**Causa:** El token expira mientras la app está offline

**Solución:**
```typescript
// En sync.ts
if (isOnline() && isTokenExpired()) {
    await refreshToken();
}

// Enviar eventos aunque el token sea viejo
const events = await getPendingEvents();
```

### Problema 5: "Conflicto de datos entre dispositivos"

**Causa:** Dos dispositivos modifican lo mismo offline

**Solución:**
```csharp
// En SyncService - Detectar conflictos
private bool DetectarConflicto(OfflineEventDto evento, dynamic entidadActual)
{
    var lastModified = evento.Timestamp;
    var serverModified = entidadActual.UltimaActualizacion;
    
    return serverModified > lastModified;
}

// Estrategia: Server-wins o Last-write-wins
```

---

## Referencias

- [Next.js PWA Plugin](https://www.npmjs.com/package/next-pwa)
- [sql.js Documentation](https://sql.js.org/)
- [Service Workers MDN](https://developer.mozilla.org/en-US/docs/Web/API/Service_Worker_API)
- [IndexedDB API](https://developer.mozilla.org/en-US/docs/Web/API/IndexedDB_API)
- [ASP.NET Core CORS](https://docs.microsoft.com/en-us/aspnet/core/security/cors)
- [JWT Authentication](https://tools.ietf.org/html/rfc7519)

---

**Última actualización:** 18 de Noviembre 2025  
**Versión:** 1.0  
**Autor:** GateKeep Development Team
