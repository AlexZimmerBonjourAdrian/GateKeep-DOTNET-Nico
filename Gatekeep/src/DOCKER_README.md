# 🐳 Docker Setup - GateKeep

## Servicios Incluidos

- **PostgreSQL 16**: Base de datos principal (puerto 5432)
- **Redis 7**: Sistema de caché (puerto 6379)
- **GateKeep API**: API .NET 8 (puerto 5011)

## 🚀 Inicio Rápido

### Requisitos Previos
- Docker Desktop instalado
- Docker Compose instalado

### 🔨 Build de Docker

Para construir la imagen de Docker de la API:

```powershell
# Construir solo la imagen de la API (sin iniciar)
docker-compose build api

# Construir sin usar cache (útil si hay problemas)
docker-compose build --no-cache api

# Construir todas las imágenes
docker-compose build
```

### Levantar todos los servicios

```powershell
# Construir y levantar todos los servicios en un solo comando
docker-compose up -d --build

# Solo levantar (sin reconstruir)
docker-compose up -d

# Ver logs
docker-compose logs -f

# Ver logs de un servicio específico
docker-compose logs -f api
```

### Verificar que todo está funcionando

```powershell
# Ver estado de los contenedores
docker-compose ps

# Verificar health checks
docker ps

# Probar la API
curl http://localhost:5011/health/redis
```

## 📋 Comandos Útiles

### Gestión de Servicios

```powershell
# Iniciar servicios
docker-compose start

# Detener servicios
docker-compose stop

# Reiniciar servicios
docker-compose restart

# Detener y eliminar contenedores
docker-compose down

# Detener y eliminar contenedores + volúmenes (⚠️ borra datos)
docker-compose down -v
```

### Logs y Debugging

```powershell
# Ver logs en tiempo real
docker-compose logs -f

# Ver logs de la API
docker-compose logs -f api

# Ver logs de PostgreSQL
docker-compose logs -f db

# Ver últimas 100 líneas
docker-compose logs --tail=100 api
```

### Acceder a los Contenedores

```powershell
# Acceder al contenedor de la API
docker exec -it gatekeep-api bash

# Acceder a PostgreSQL
docker exec -it gatekeep-postgres psql -U postgres -d GateKeep

# Acceder a Redis CLI
docker exec -it gatekeep-redis redis-cli
```

## 🔧 Configuración

### Variables de Entorno

Las configuraciones se encuentran en:
- `src/GateKeep.Api/config.Production.json` - Configuración para Docker
- `docker-compose.yml` - Variables de entorno de los contenedores

### Cambiar Puertos

Edita `docker-compose.yml`:

```yaml
services:
  api:
    ports:
      - "8080:5011"  # Cambiar puerto externo
```

### Cambiar Credenciales de PostgreSQL

Edita `docker-compose.yml`:

```yaml
db:
  environment:
    POSTGRES_PASSWORD: tu_nueva_password
```

Y actualiza las variables de entorno del servicio `api` en `docker-compose.yml`:

```yaml
api:
  environment:
    DATABASE__PASSWORD: "tu_nueva_password"
```

## 🗄️ Gestión de Datos

### Backup de PostgreSQL

```powershell
# Crear backup
docker exec gatekeep-postgres pg_dump -U postgres GateKeep > backup.sql

# Restaurar backup
type backup.sql | docker exec -i gatekeep-postgres psql -U postgres -d GateKeep
```

### Limpiar Redis Cache

```powershell
docker exec -it gatekeep-redis redis-cli FLUSHALL
```

### Ver Datos de Redis

```powershell
# Conectar a Redis
docker exec -it gatekeep-redis redis-cli

# Ver todas las claves
KEYS GateKeep:*

# Ver un valor
GET GateKeep:beneficios:all
```

## 🔍 Troubleshooting

### La API no se conecta a PostgreSQL

```powershell
# Verificar que PostgreSQL está healthy
docker-compose ps

# Ver logs de PostgreSQL
docker-compose logs db

# Reiniciar servicios en orden
docker-compose restart db
docker-compose restart api
```

### La API no se conecta a Redis

```powershell
# Verificar Redis
docker exec -it gatekeep-redis redis-cli ping

# Ver logs
docker-compose logs redis

# Reiniciar Redis
docker-compose restart redis
```

### Error al construir la imagen

```powershell
# Limpiar cache de Docker
docker builder prune

# Reconstruir sin cache
docker-compose build --no-cache

# Levantar de nuevo
docker-compose up -d
```

### Puerto ya en uso

```powershell
# Windows: Ver qué proceso usa el puerto
netstat -ano | findstr :5011

# Matar el proceso (usar el PID del comando anterior)
taskkill /PID <PID> /F

# O cambiar el puerto en docker-compose.yml
```

## 🌐 URLs de Acceso

Cuando los servicios estén corriendo:

- **API**: http://localhost:5011
- **Swagger**: http://localhost:5011/swagger
- **Health Check**: http://localhost:5011/health/redis
- **PostgreSQL**: localhost:5432
- **Redis**: localhost:6379

## 📦 Volúmenes Persistentes

Los datos se guardan en volúmenes de Docker:

```powershell
# Listar volúmenes
docker volume ls

# Inspeccionar un volumen
docker volume inspect src_pgdata

# Eliminar volúmenes no usados
docker volume prune
```

## 🚢 Despliegue en Producción

### Consideraciones

1. **Cambiar credenciales**: Usa contraseñas seguras
2. **SSL/HTTPS**: Configura un reverse proxy (nginx)
3. **Backup automático**: Configura backups programados
4. **Monitoreo**: Integra herramientas de monitoreo
5. **Secrets**: Usa Docker secrets o variables de entorno seguras

### Ejemplo con Secrets

```yaml
secrets:
  db_password:
    file: ./secrets/db_password.txt

services:
  postgres:
    secrets:
      - db_password
    environment:
      POSTGRES_PASSWORD_FILE: /run/secrets/db_password
```

## 📝 Notas

- Los health checks aseguran que la API espere a que PostgreSQL y Redis estén listos
- Los volúmenes persisten los datos incluso si los contenedores se eliminan
- Para desarrollo, usa `config.json` con `host: localhost`
- Para producción con Docker, usa `config.Production.json` con nombres de servicios

## 🆘 Soporte

Si encuentras problemas:

1. Revisa los logs: `docker-compose logs -f`
2. Verifica el estado: `docker-compose ps`
3. Revisa la documentación en `docs/`
