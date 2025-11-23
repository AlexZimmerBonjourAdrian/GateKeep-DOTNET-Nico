# 🚀 Guía de Desarrollo Local - GateKeep

**Fecha:** 2025-01-21  
**Versión:** 1.0

---

## 📋 Índice

1. [Requisitos Previos](#requisitos-previos)
2. [Configuración Inicial](#configuración-inicial)
3. [Métodos de Ejecución](#métodos-de-ejecución)
4. [URLs y Accesos](#urls-y-accesos)
5. [Solución de Problemas](#solución-de-problemas)

---

## ✅ Requisitos Previos

### Software Necesario

- **.NET 8.0 SDK** - [Descargar](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Docker Desktop** - [Descargar](https://www.docker.com/products/docker-desktop)
- **PowerShell 5.1+** (incluido en Windows 10/11)
- **Git** (opcional, para clonar el repositorio)

### Verificar Instalaciones

```powershell
# Verificar .NET SDK
dotnet --version
# Debe mostrar: 8.0.x o superior

# Verificar Docker
docker --version
docker-compose --version

# Verificar PowerShell
$PSVersionTable.PSVersion
```

---

## ⚙️ Configuración Inicial

### 1. Crear Archivo de Variables de Entorno

El proyecto necesita un archivo `.env` en la carpeta `src/` con las variables de entorno necesarias.

#### Opción A: Usando el Script (Recomendado)

```powershell
cd Gatekeep\src
Copy-Item ".env.example" ".env"
notepad .env
```

#### Opción B: Crear Manualmente

Crea el archivo `Gatekeep/src/.env` con el siguiente contenido:

```env
# ==============================================
# Base de Datos PostgreSQL
# ==============================================
DB_HOST=localhost
DB_PORT=5432
DB_NAME=Gatekeep
DB_USER=postgres
DB_PASSWORD=897888fg2

# Puerto externo para PostgreSQL (si usas Docker)
DB_EXTERNAL_PORT=5433

# ==============================================
# Aplicación - Configuración General
# ==============================================
APP_ENVIRONMENT=Development
GATEKEEP_PORT=5011

# ==============================================
# JWT - Autenticación y Tokens
# ==============================================
JWT_KEY=clave-secreta-minimo-256-bits-para-desarrollo-local-solo-para-pruebas-no-usar-en-produccion
JWT_ISSUER=GateKeep
JWT_AUDIENCE=GateKeepUsers
JWT_EXPIRATION_HOURS=8

# ==============================================
# MongoDB - Base de Datos de Documentos (Opcional)
# ==============================================
MONGODB_CONNECTION=mongodb://localhost:27017
MONGODB_DATABASE=GateKeepMongo
MONGODB_USE_STABLE_API=false

# ==============================================
# Redis - Sistema de Cache (Opcional)
# ==============================================
REDIS_CONNECTION=localhost:6379
REDIS_INSTANCE=GateKeep:
REDIS_ENABLED=true

# ==============================================
# RabbitMQ - Cola de Mensajes (Opcional)
# ==============================================
RABBITMQ_HOST=localhost
RABBITMQ_PORT=5672
RABBITMQ_USER=guest
RABBITMQ_PASSWORD=guest
RABBITMQ_VHOST=/

# ==============================================
# Grafana - Monitoreo (Opcional)
# ==============================================
GF_SECURITY_ADMIN_USER=admin
GF_SECURITY_ADMIN_PASSWORD=admin123
```

**Nota:** Ajusta las contraseñas según tu configuración local.

---

## 🚀 Métodos de Ejecución

### Método 1: Con Docker Compose (Recomendado) ⭐

Este método levanta todos los servicios necesarios (PostgreSQL, Redis, MongoDB, etc.) automáticamente.

#### Usando el Script PowerShell

```powershell
# Desde la raíz del proyecto
cd Gatekeep
.\iniciar-docker.ps1
```

#### Manualmente

```powershell
cd Gatekeep\src
docker-compose up -d
```

**Ventajas:**
- ✅ Levanta todos los servicios automáticamente
- ✅ No necesitas instalar PostgreSQL, Redis, MongoDB localmente
- ✅ Configuración aislada en contenedores
- ✅ Fácil de limpiar y reiniciar

**Desventajas:**
- ⚠️ Requiere Docker Desktop ejecutándose
- ⚠️ Consume más recursos del sistema

#### Ver Estado de los Servicios

```powershell
cd Gatekeep\src
docker-compose ps
```

#### Ver Logs

```powershell
# Logs de todos los servicios
docker-compose logs -f

# Logs solo de la API
docker-compose logs -f api

# Logs de PostgreSQL
docker-compose logs -f postgres
```

#### Detener Servicios

```powershell
# Detener todos los servicios
docker-compose down

# O usar el script
cd Gatekeep
.\detener-docker.ps1
```

---

### Método 2: Ejecución Directa con .NET

Este método ejecuta solo la API, pero necesitas tener PostgreSQL, Redis, etc. instalados y ejecutándose localmente.

#### Usando el Script PowerShell

```powershell
# Desde la raíz del proyecto
cd Gatekeep
.\run-backend.ps1
```

#### Manualmente

```powershell
cd Gatekeep\src\GateKeep.Api

# Restaurar dependencias
dotnet restore

# Compilar
dotnet build

# Ejecutar
dotnet run
```

**Ventajas:**
- ✅ Más rápido para desarrollo
- ✅ Hot reload con `dotnet watch run`
- ✅ Menor consumo de recursos

**Desventajas:**
- ⚠️ Necesitas instalar PostgreSQL, Redis, MongoDB localmente
- ⚠️ Más configuración manual

#### Con Hot Reload (Recomendado para Desarrollo)

```powershell
cd Gatekeep\src\GateKeep.Api
dotnet watch run
```

Esto recarga automáticamente la aplicación cuando detecta cambios en el código.

---

### Método 3: Solo Frontend (Next.js)

Para ejecutar solo el frontend en modo desarrollo:

```powershell
# Usando el script
cd Gatekeep
.\run-frontend.ps1

# O manualmente
cd Gatekeep\frontend
npm install
npm run dev
```

El frontend se ejecutará en `http://localhost:3000`

---

## 🌐 URLs y Accesos

Una vez que los servicios estén ejecutándose, tendrás acceso a:

| Servicio | URL | Credenciales | Descripción |
|----------|-----|--------------|-------------|
| **API Swagger** | http://localhost:5011/swagger | - | Documentación interactiva de la API |
| **Health Check** | http://localhost:5011/health | - | Estado de salud de la API |
| **API Base** | http://localhost:5011/api | - | Endpoints de la API |
| **Seq (Logs)** | http://localhost:5341 | - | Visualizador de logs (solo con Docker) |
| **Prometheus** | http://localhost:9090 | - | Métricas (solo con Docker) |
| **Grafana** | http://localhost:3001 | admin / admin123 | Dashboards (solo con Docker) |
| **RabbitMQ Management** | http://localhost:15672 | guest / guest | Gestión de colas (solo con Docker) |
| **Frontend** | http://localhost:3000 | - | Aplicación web (si ejecutas frontend) |

---

## 🔧 Comandos Útiles

### Docker Compose

```powershell
# Ver estado de servicios
docker-compose ps

# Ver logs en tiempo real
docker-compose logs -f api

# Reiniciar un servicio específico
docker-compose restart api

# Recrear contenedores (útil después de cambios en docker-compose.yml)
docker-compose up -d --build

# Detener y eliminar contenedores
docker-compose down

# Detener y eliminar contenedores + volúmenes (⚠️ elimina datos)
docker-compose down -v
```

### .NET

```powershell
# Limpiar build
dotnet clean

# Restaurar dependencias
dotnet restore

# Compilar
dotnet build

# Ejecutar
dotnet run

# Ejecutar con hot reload
dotnet watch run

# Ejecutar tests
dotnet test
```

### Base de Datos (PostgreSQL)

```powershell
# Conectar a PostgreSQL (Docker)
docker exec -it gatekeep-postgres psql -U postgres -d Gatekeep

# Ver tablas
docker exec -it gatekeep-postgres psql -U postgres -d Gatekeep -c "\dt"

# Backup
docker exec gatekeep-postgres pg_dump -U postgres Gatekeep > backup.sql

# Restore
docker exec -i gatekeep-postgres psql -U postgres Gatekeep < backup.sql
```

---

## 🐛 Solución de Problemas

### Problema 1: Puerto 5011 ya en uso

**Síntoma:**
```
Error: Address already in use
```

**Solución:**
```powershell
# Encontrar proceso usando el puerto
netstat -ano | findstr :5011

# Terminar proceso (reemplaza <PID> con el número de proceso)
taskkill /PID <PID> /F

# O usar el script que lo hace automáticamente
.\run-backend.ps1
```

### Problema 2: Archivo .env no encontrado

**Síntoma:**
```
ERROR: Archivo .env no encontrado
```

**Solución:**
```powershell
cd Gatekeep\src
Copy-Item ".env.example" ".env"
# Editar .env con tus configuraciones
notepad .env
```

### Problema 3: Error de conexión a PostgreSQL

**Síntoma:**
```
Npgsql.PostgresException: 28P01: password authentication failed
```

**Solución:**
1. Verificar que PostgreSQL esté ejecutándose:
   ```powershell
   docker-compose ps postgres
   ```

2. Verificar la contraseña en `.env`:
   ```powershell
   Get-Content Gatekeep\src\.env | Select-String "DB_PASSWORD"
   ```

3. Probar conexión manual:
   ```powershell
   docker exec -it gatekeep-postgres psql -U postgres -d Gatekeep
   ```

4. Si usas contraseña `897888fg2`, asegúrate de que esté en `.env`:
   ```env
   DB_PASSWORD=897888fg2
   ```

### Problema 4: Docker Desktop no está ejecutándose

**Síntoma:**
```
Cannot connect to the Docker daemon
```

**Solución:**
1. Abrir Docker Desktop
2. Esperar a que esté completamente iniciado (ícono verde)
3. Verificar:
   ```powershell
   docker ps
   ```

### Problema 5: Variables de entorno no se cargan

**Síntoma:**
La aplicación no lee las variables de `.env`

**Solución:**
1. Verificar que `.env` esté en `Gatekeep/src/.env`
2. Verificar formato del archivo (sin espacios alrededor del `=`)
3. Reiniciar la aplicación

### Problema 6: MongoDB no se conecta

**Síntoma:**
```
MongoDB connection failed
```

**Solución:**
Si no necesitas MongoDB, puedes deshabilitarlo en `.env`:
```env
MONGODB_CONNECTION=
MONGODB_DATABASE=
```

O simplemente no iniciar el contenedor de MongoDB:
```powershell
docker-compose up -d postgres redis api
```

### Problema 7: Error al compilar

**Síntoma:**
```
Error: The build failed
```

**Solución:**
```powershell
# Limpiar completamente
dotnet clean
Remove-Item -Recurse -Force bin, obj

# Restaurar dependencias
dotnet restore

# Recompilar
dotnet build
```

---

## 📝 Notas Importantes

### Variables de Entorno

- El archivo `.env` **NO** se sube a Git (está en `.gitignore`)
- Siempre usar `.env.example` como plantilla
- Las credenciales de producción deben estar en variables de entorno del servidor

### Base de Datos

- Con Docker Compose, la base de datos se crea automáticamente
- Los datos persisten en volúmenes de Docker
- Para limpiar completamente: `docker-compose down -v`

### Desarrollo vs Producción

- **Desarrollo:** Usa `APP_ENVIRONMENT=Development`
- **Producción:** Usa `APP_ENVIRONMENT=Production`
- El modo Development incluye:
  - Swagger habilitado
  - Logs más detallados
  - Hot reload disponible
  - Seeding automático de datos de prueba

### Migraciones de Base de Datos

Las migraciones se ejecutan automáticamente al iniciar la aplicación en modo Development.

Para ejecutarlas manualmente:
```powershell
cd Gatekeep\src\GateKeep.Api
dotnet ef database update
```

---

## 🎯 Flujo de Trabajo Recomendado

### Primera Vez

1. ✅ Instalar requisitos (.NET SDK, Docker Desktop)
2. ✅ Crear archivo `.env` desde `.env.example`
3. ✅ Configurar contraseñas y variables
4. ✅ Ejecutar `.\iniciar-docker.ps1`
5. ✅ Verificar que todos los servicios estén saludables
6. ✅ Acceder a http://localhost:5011/swagger

### Desarrollo Diario

1. ✅ Iniciar Docker Desktop
2. ✅ Ejecutar `.\iniciar-docker.ps1` (si usas Docker)
3. ✅ O ejecutar `.\run-backend.ps1` (si usas .NET directo)
4. ✅ Hacer cambios en el código
5. ✅ Ver cambios reflejados (hot reload si usas `dotnet watch run`)
6. ✅ Probar en Swagger o frontend

### Al Finalizar

1. ✅ Detener servicios: `.\detener-docker.ps1`
2. ✅ O presionar `Ctrl+C` si ejecutaste directamente

---

## 📚 Recursos Adicionales

- [README.md](../README.md) - Documentación general del proyecto
- [CAMBIOS_REALIZADOS_AWS.md](./CAMBIOS_REALIZADOS_AWS.md) - Cambios en AWS
- [ANALISIS_COMPLETO_AWS_ENDPOINTS.md](./ANALISIS_COMPLETO_AWS_ENDPOINTS.md) - Endpoints disponibles

---

## ✅ Checklist de Inicio Rápido

- [ ] .NET 8.0 SDK instalado
- [ ] Docker Desktop instalado y ejecutándose
- [ ] Archivo `.env` creado en `Gatekeep/src/.env`
- [ ] Variables de entorno configuradas
- [ ] Servicios iniciados con `.\iniciar-docker.ps1`
- [ ] API accesible en http://localhost:5011/swagger
- [ ] Health check responde en http://localhost:5011/health

---

**Última actualización:** 2025-01-21  
**Mantenido por:** Equipo GateKeep

