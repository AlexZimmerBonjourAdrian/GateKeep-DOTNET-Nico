# 🚀 Guía Rápida de Caching con Redis - GateKeep

## Instalación y Configuración

### 1. Instalar Redis

#### Opción A: Usando Docker (Recomendado)
```powershell
# Ejecutar el script de instalación automática
.\scripts\setupRedis.ps1
```

#### Opción B: Manual con Docker
```bash
docker run -d --name redis-gatekeep -p 6379:6379 redis:latest
```

### 2. Verificar Instalación
```bash
# Verificar que Redis está corriendo
docker ps | findstr redis-gatekeep

# Probar conexión
docker exec -it redis-gatekeep redis-cli ping
# Debe responder: PONG
```

## 🎯 Uso del Sistema de Caching

### Endpoints Disponibles

#### 1. Ver Métricas del Cache (Solo Administradores)
```http
GET /api/cache-metrics
Authorization: Bearer {token}
```

**Respuesta:**
```json
{
  "totalHits": 150,
  "totalMisses": 50,
  "totalInvalidations": 10,
  "totalRequests": 200,
  "hitRate": 75.0,
  "lastResetTime": "2025-11-10T10:30:00Z",
  "hitsByKey": {
    "GateKeep:beneficios:all": 80,
    "GateKeep:beneficios:vigentes": 70
  },
  "missesByKey": {
    "GateKeep:beneficios:all": 20,
    "GateKeep:beneficios:vigentes": 30
  }
}
```

#### 2. Reiniciar Métricas (Solo Administradores)
```http
POST /api/cache-metrics/reset
Authorization: Bearer {token}
```

#### 3. Verificar Estado del Cache (Público)
```http
GET /api/cache-metrics/health
```

#### 4. Health Check de Redis (Público)
```http
GET /health/redis
```

### Datos Cacheados

✅ **Beneficios Vigentes** (TTL: 5 minutos)
- `GET /api/beneficios` - Lista completa
- `GET /api/beneficios/{id}` - Beneficio específico
- `GET /api/beneficios/vigentes` - Solo vigentes

## 🔄 Comportamiento del Cache

### Cache Hit (Datos en Redis)
1. ⚡ Petición → Redis (< 1ms)
2. ✅ Retorna datos inmediatamente
3. 📊 Métrica: +1 Hit

### Cache Miss (Datos no en Redis)
1. ❌ Petición → Redis (no existe)
2. 🔍 Consulta → PostgreSQL (50-200ms)
3. 💾 Guarda en Redis con TTL
4. ✅ Retorna datos
5. 📊 Métrica: +1 Miss

### Invalidación Automática
```
Crear/Actualizar/Eliminar Beneficio
    ↓
Invalida cache automáticamente
    ↓
Próxima petición será Cache Miss
    ↓
Se reconstruye el cache
```

## 📊 Monitorear en Tiempo Real

### Logs de la Aplicación
```bash
# Los logs muestran las operaciones de cache
[Cache] Hit: beneficios:vigentes
[Cache] Miss: beneficios:123
[Cache] Invalidation: beneficios:all
```

### Redis CLI
```bash
# Conectar a Redis
docker exec -it redis-gatekeep redis-cli

# Ver todas las claves
KEYS GateKeep:*

# Ver un valor específico
GET GateKeep:beneficios:all

# Ver TTL de una clave
TTL GateKeep:beneficios:vigentes

# Ver info de Redis
INFO stats
```

## 🧪 Probar el Sistema

### 1. Iniciar la Aplicación
```bash
cd src/GateKeep.Api
dotnet run
```

### 2. Obtener Token de Admin
```bash
POST http://localhost:5011/api/auth/login
{
  "email": "admin@gatekeep.com",
  "password": "admin123"
}
```

### 3. Probar Cache de Beneficios
```bash
# Primera petición (Cache Miss)
GET http://localhost:5011/api/beneficios
# Tiempo: ~100ms

# Segunda petición (Cache Hit)
GET http://localhost:5011/api/beneficios
# Tiempo: ~5ms

# Ver métricas
GET http://localhost:5011/api/cache-metrics
Authorization: Bearer {tu-token}
```

## 📈 Interpretación de Métricas

| Hit Rate | Estado | Acción |
|----------|--------|--------|
| > 80% | 🟢 Excelente | Mantener configuración |
| 60-80% | 🟡 Bueno | Monitorear |
| 40-60% | 🟠 Aceptable | Considerar ajustar TTL |
| < 40% | 🔴 Bajo | Revisar estrategia |

## 🛠️ Comandos Útiles

```bash
# Ver estado de Redis
docker ps | findstr redis

# Ver logs de Redis
docker logs redis-gatekeep

# Detener Redis
docker stop redis-gatekeep

# Iniciar Redis
docker start redis-gatekeep

# Reiniciar Redis
docker restart redis-gatekeep

# Limpiar todo el cache
docker exec -it redis-gatekeep redis-cli FLUSHALL

# Ver memoria usada por Redis
docker exec -it redis-gatekeep redis-cli INFO memory
```

## ⚙️ Configuración Avanzada

### Cambiar TTL
Editar `Infrastructure/Caching/CacheKeys.cs`:
```csharp
public static class TTL
{
    public static readonly TimeSpan Beneficios = TimeSpan.FromMinutes(10); // Cambiar aquí
}
```

### Agregar Nuevo Cache
1. Agregar clave en `CacheKeys.cs`
2. Crear servicio cached (ejemplo: `CachedEventoService`)
3. Registrar en `Program.cs`
4. Inyectar en controladores

## 🐛 Troubleshooting

### Redis no conecta
```bash
# Verificar que está corriendo
docker ps

# Si no está corriendo, iniciarlo
docker start redis-gatekeep

# Ver logs de errores
docker logs redis-gatekeep
```

### Cache no funciona
```bash
# Verificar health check
curl http://localhost:5011/health/redis

# Limpiar cache y reintentar
docker exec -it redis-gatekeep redis-cli FLUSHALL
```

### Métricas en cero
```bash
# Las métricas se resetean al reiniciar la aplicación
# Hacer algunas peticiones y verificar de nuevo
```

## 📚 Documentación Completa

Para más detalles, ver:
- `docs/REDIS_CACHING.md` - Documentación técnica completa
- Swagger UI: `http://localhost:5011/swagger`

