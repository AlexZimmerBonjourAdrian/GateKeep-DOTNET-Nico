# 🧪 Prueba condensada - Redis Caching (Swagger + docker logs)

Esta guía breve explica qué operaciones realizar desde Swagger (UI) y qué esperar en los logs. No usa curl ni redis-cli; solo Swagger para las peticiones HTTP y `docker logs` para ver el estado de Redis. Las entradas de cache (miss/hit/set/remove) las emite la aplicación (`GateKeep.Api`); Redis escribe su propio estado (arranque, conexiones, expulsiones).

Requisitos

- La API GateKeep en ejecución y accesible (ej. http://localhost:5011).
- Swagger UI disponible (ej. http://localhost:5011/swagger).
- Contenedor Redis corriendo (esperado: nombre `gatekeep-redis` en Docker).
- Terminal (cmd.exe) para `docker logs`.

Checklist rápido

- [ ] Abrir Swagger UI y autenticarse (si aplica).
# 🧪 Prueba condensada - Redis Caching (Swagger + docker logs)

Este documento describe cómo probar el comportamiento de caching Redis desde la perspectiva de un entorno vacío: **no hay beneficios ni reglas de acceso creadas** al inicio de la prueba. Asumimos que el usuario ya está autenticado y posee un token JWT válido para usar en Swagger.

Requisitos

- La API GateKeep en ejecución y accesible (ej. http://localhost:5011).
- Swagger UI disponible (ej. http://localhost:5011/swagger).
- Contenedor Redis corriendo (esperado: nombre `gatekeep-redis` en Docker).
- Tener un token JWT válido (usuario autenticado). El token debe tener el rol necesario para cada endpoint (p. ej. `Funcionario` o `Admin` para reglas y creación de beneficios).
- Terminal (cmd.exe) para `docker logs`.

Checklist rápido (escenario: base de datos vacía)

- [ ] Abrir Swagger UI y pegar el token en Authorize (`Bearer {token}`).
- [ ] Ejecutar endpoints de health y confirmar estado Redis.
- [ ] Comprobar que no existen recursos: `GET /beneficios` → lista vacía, `GET /api/reglas-acceso` → lista vacía.
- [ ] Forzar flujo Cache MISS → SET → HIT para `beneficios` y `reglas-acceso`.
- [ ] Crear recursos (POST) para validar invalidación de cache.
- [ ] Consultar métricas de cache vía endpoint protegido `/api/cache-metrics`.
- [ ] Observar `docker logs -f gatekeep-redis` para estado del servidor Redis y `docker logs -f <container-api>` para logs de la API.

Comandos Docker (cmd.exe)

Usa estas dos líneas en una consola de Windows (cmd.exe) para ver contenedores y seguir logs de Redis:

```cmd
:: Listar contenedores para confirmar nombre
docker ps --format "{{.Names}}\t{{.Image}}\t{{.Ports}}"

:: Seguir logs del contenedor Redis (reemplaza si tu contenedor tiene otro nombre)
docker logs -f gatekeep-redis
```

Qué buscar en los logs de Redis

- Mensaje de arranque: "* Ready to accept connections".
- Conexiones entrantes desde la API (líneas que indican conexiones o accepted clients).
- Mensajes de error o eviction si hay problemas de memoria.

Importante: las líneas de "Cache miss", "Cache hit", "Cache set" y "Cache removed" las imprime la propia aplicación `GateKeep.Api` en sus logs; Redis solo muestra su propio estado. Para ver las entradas de cache debes mirar los logs del contenedor de la API (si corre en Docker) o la consola donde ejecutaste `dotnet run`.

Flujo de pruebas desde Swagger (escenario vacío: sin datos creados)

0) Preparación e inicio (usuario autenticado)
- En Swagger: usar `POST /api/auth/login` (si necesitás crear un usuario primero, usa `/api/auth/register`) y copia el JWT.
- En Swagger: botón Authorize → `Bearer {TOKEN}`.

1) Health checks
- En Swagger: ejecutar `GET /health` y `GET /health/redis`.
- Esperar: HTTP 200; para `/health/redis` un JSON que indique `Healthy` o `connected: true`.

2) Verificar que la base de datos está vacía
- `GET /beneficios` → Esperado: HTTP 200 con lista vacía `[]`.
    - Dado que no existe la clave aún, la aplicación hará un **Cache MISS** y luego **SET** la representación vacía.
    - Logs esperados en la API:
        - `[CACHE] Cache miss: beneficios:all`
        - `[CACHE] Cache set: beneficios:all, Expiration=00:05:00`


Nota: incluso si la respuesta es una lista vacía, la aplicación la cachea para evitar consultas repetidas a la BD.

3) Comprobar Cache HIT sobre datos vacíos
- Volver a ejecutar `GET /beneficios` inmediatamente.
- Qué esperar: HTTP 200; en los logs de la API:
    - `[CACHE] Cache hit: beneficios:all`

4) Crear recursos y validar invalidación

5) Beneficios
- En Swagger: `POST /beneficios` (rol `Funcionario` o `Admin`).
- Body ejemplo:
```json
{
  "titulo": "Beneficio Test Redis",
  "descripcion": "Prueba invalidación de cache",
  "fechaDeVencimiento": "2025-12-31T23:59:59Z",
  "tipo": "Descuento"
}
```
- Qué esperar: HTTP 201 y en logs de la API:
    - `[CACHE] Cache removed: beneficios:all`
    - `[CACHE] Cache removed: beneficios:vigentes`
- Luego `GET /beneficios` → Cache MISS (por la invalidación) → Cache SET con la lista que ahora contiene el nuevo beneficio.


6) Consultar métricas de cache
- En Swagger (con Authorization de Admin): `GET /api/cache-metrics`.
- Qué esperar: JSON con estadísticas (totalHits, totalMisses, totalInvalidations, hitRate, hitsByKey, missesByKey).

7) Probar TTL/expiración (si aplicable)
- Observa la entrada de log al hacer `Cache set` que incluye el tiempo de expiración (TTL):
    - Beneficios: `Expiration=00:05:00` (5 minutos)
- Pasos rápidos para probar expiración:
    1. Ejecuta `GET /beneficios` → MISS → SET
    2. Ejecuta `GET /beneficios` → HIT
    3. Espera 5 minutos
    4. Ejecuta `GET /beneficios` → MISS (TTL expiró)

Problemas comunes y acciones rápidas

- **No aparecen mensajes de cache en los logs de la API**:
    - Asegúrate de ver los logs correctos (API vs Redis).
    - Los mensajes se emiten con nivel `Information` y prefijo `[CACHE]`.
    - Verifica `appsettings.Development.json` para niveles de logging si no ves mensajes.

- **`/health/redis` devuelve `Unhealthy`**:
    - Revisar `docker logs gatekeep-redis` y la cadena de conexión en variables de entorno.
    - Reiniciar Redis: `docker restart gatekeep-redis` y revisar logs.
    - Verificar conectividad: `docker exec gatekeep-redis redis-cli ping` → debe responder `PONG`.

- **No tienes usuario con el rol adecuado**:
    - Registrar usuario: `POST /api/auth/register`.
    - Asignar rol o usar un Admin para las pruebas de reglas y métricas.

Cómo ver los logs de la API (si corre en Docker)

1) Identificar el nombre del contenedor de la API (cmd.exe):

```cmd
:: Lista los contenedores en ejecución con nombre e imagen para identificar el contenedor de la API
docker ps --format "{{.Names}}\t{{.Image}}\t{{.Status}}"
```

2) Seguir (tail -f) los logs del contenedor de la API:

```cmd
docker logs -f <nombre-contenedor-api>
```

3) Buscar líneas relevantes (Windows cmd):

```cmd
docker logs --tail 500 <nombre-contenedor-api> | findstr /I "CACHE"
```

Resumen (acciones concretas desde Swagger, entorno vacío)

- GET /health, GET /health/redis → esperar Healthy.
- GET /beneficios (1) → lista vacía + Cache MISS + Cache SET.
- GET /beneficios (2) → Cache HIT.
- POST /beneficios → crear recurso → Cache REMOVED.
- GET /beneficios (siguiente) → Cache MISS → Cache SET con recurso nuevo.
- GET /api/reglas-acceso (1) → lista vacía + Cache MISS + Cache SET.
- POST /api/reglas-acceso → crear regla → Cache REMOVED.
- GET /api/reglas-acceso (siguiente) → Cache MISS → Cache SET con la regla nueva.
- GET /api/cache-metrics → JSON con métricas (Admin).
- Observa `docker logs -f gatekeep-redis` y `docker logs -f <nombre-contenedor-api>` para ver estado y mensajes `[CACHE]`.

Fin del documento condensado.
```json
{
  "Logging": {
    "LogLevel": {
      "GateKeep.Api.Infrastructure.Caching": "Debug"
    }
  }
}
```

- **`/health/redis` devuelve `Unhealthy`**:
    - Revisar `docker logs gatekeep-redis` y la cadena de conexión en `config.json`/variables de entorno.
    - Reiniciar Redis: `docker restart gatekeep-redis` y revisar logs.
    - Verificar conectividad: `docker exec gatekeep-redis redis-cli ping` → debe responder `PONG`.

- **Los endpoints requieren autenticación pero no tengo usuario**:
    - Crear usuario: `POST /api/auth/register` con rol "Estudiante", "Funcionario" o "Admin".
    - Login: `POST /api/auth/login` para obtener el token.
    - Nota: `/beneficios` requiere cualquier usuario autenticado, `/api/reglas-acceso` requiere Funcionario/Admin, `/api/cache-metrics` requiere Admin.

- **Cache no se invalida al crear/actualizar**:
    - Verificar que los endpoints estén usando `ICachedBeneficioService` o `ICachedReglaAccesoService` (no los servicios sin cache).
    - Revisar logs de la API para confirmar mensajes `[CACHE] Cache removed`.

Cómo ver los logs de la API (si corre en Docker)

Si tu `GateKeep.Api` corre dentro de un contenedor Docker, las entradas que muestran `Cache miss`, `Cache hit`, `Cache set` y `Cache removed` las genera la propia API; para verlas puedes seguir los logs del contenedor de la API.

1) Identificar el nombre del contenedor de la API (cmd.exe):

```cmd
:: Lista los contenedores en ejecución con nombre e imagen para identificar el contenedor de la API
docker ps --format "{{.Names}}\t{{.Image}}\t{{.Status}}"
```

Busca en la columna `IMAGE` algo relacionado con tu proyecto (`gatekeep`, `gatekeep.api`, etc.) o el nombre que asignaste al contenedor.

2) Seguir (tail -f) los logs del contenedor de la API:

```cmd
:: Ver todos los logs y seguir en tiempo real (reemplaza <nombre-contenedor-api>)
docker logs -f <nombre-contenedor-api>

:: Ver solo las últimas 200 líneas y seguir (útil para no descargar todo el historial)
docker logs --tail 200 -f <nombre-contenedor-api>
```

3) Buscar líneas relevantes (Windows cmd):

```cmd
:: Filtrar las líneas que contienen "Cache" (ejemplo simple con findstr)
docker logs --tail 500 <nombre-contenedor-api> | findstr /I "Cache"
```

Nota rápida:
- Reemplaza `<nombre-contenedor-api>` por el nombre real del contenedor que obtuviste con `docker ps`.
- Para detener `docker logs -f` usa Ctrl + C.
- Si no ves ninguna línea con "Cache" es posible que el nivel de logs esté en `Information` y las entradas sean `Debug`; en ese caso habilita Debug para el namespace de caching (ver sección "Problemas comunes").

Resumen (acciones concretas desde Swagger)

**Health Checks**:
- `GET /health` → esperar Healthy.
- `GET /health/redis` → esperar `{"status": "Healthy"}`.

**Autenticación**:
- `POST /api/auth/login` → obtener token JWT.
- Clic en botón "Authorize" → `Bearer {token}`.

**Pruebas de Cache - BENEFICIOS** (TTL: 5 minutos):
- `GET /beneficios` (1ra vez) → Cache MISS + Cache SET (logs: `[CACHE] Cache miss: beneficios:all`).
- `GET /beneficios` (2da vez) → Cache HIT (logs: `[CACHE] Cache hit: beneficios:all`).
- `POST /beneficios` → Cache REMOVED (logs: `[CACHE] Cache removed: beneficios:all` + `beneficios:vigentes`).
- `GET /beneficios` (3ra vez) → Cache MISS de nuevo (cache fue invalidado).

**Pruebas de Cache - REGLAS DE ACCESO** (TTL: 10 minutos):
- `GET /api/reglas-acceso` (1ra vez) → Cache MISS + Cache SET (logs: `[CACHE] Cache miss: reglas-acceso:all`).
- `GET /api/reglas-acceso` (2da vez) → Cache HIT (logs: `[CACHE] Cache hit: reglas-acceso:all`).
- `POST /api/reglas-acceso` → Cache REMOVED (logs: `[CACHE] Cache removed: reglas-acceso:all` + `reglas-acceso:activas`).
- `GET /api/reglas-acceso` (3ra vez) → Cache MISS de nuevo (cache fue invalidado).

**Métricas**:
- `GET /api/cache-metrics` (requiere Admin) → JSON con totalHits, totalMisses, hitRate, desglose por key.
- `GET /api/cache-metrics/health` (público) → Estado simplificado del cache.

**Observabilidad**:
- Logs de la API: `docker logs -f <nombre-contenedor-api>` (ver mensajes `[CACHE]`).
- Logs de Redis: `docker logs -f gatekeep-redis` (ver estado del servidor).
- Métricas Prometheus: `http://localhost:5011/metrics` (buscar `gatekeep_cache_operations_total`).


Fin del documento condensado.
