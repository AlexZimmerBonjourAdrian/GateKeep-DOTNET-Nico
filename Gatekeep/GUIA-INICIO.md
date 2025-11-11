# Guía de Inicio - GateKeep API

Esta guía contiene todos los comandos necesarios para levantar, configurar y gestionar el proyecto GateKeep.

## 📋 Requisitos Previos

- .NET 8.0 SDK
- Docker Desktop (para ejecutar con contenedores)
- PostgreSQL (para desarrollo local sin Docker)
- Redis (para desarrollo local sin Docker)
- MongoDB Atlas o local (opcional)

---

## 🔧 Configuración Inicial

### 1. Configurar Variables de Entorno

Las variables de entorno se gestionan desde el archivo `.env` ubicado en `src/.env`

**Variables obligatorias:**

```env
# PostgreSQL
DB_HOST=localhost
DB_PORT=5432
DB_NAME=Gatekeep
DB_USER=postgres
DB_PASSWORD=tu_contraseña_real

# MongoDB
MONGODB_CONNECTION=mongodb+srv://user:pass@host/?appName=GateKeepMongo
MONGODB_DATABASE=GateKeepMongo
MONGODB_USE_STABLE_API=true

# Redis
REDIS_CONNECTION=localhost:6379
REDIS_INSTANCE=GateKeep:
REDIS_ENABLED=true

# API
GATEKEEP_PORT=5011
APP_ENVIRONMENT=Development

# JWT
JWT_KEY=clave-secreta-muy-larga-minimo-256-bits
JWT_ISSUER=GateKeep
JWT_AUDIENCE=GateKeepUsers
JWT_EXPIRATION_HOURS=8

# Security
SECURITY_PASSWORD_MIN_LENGTH=8
SECURITY_MAX_LOGIN_ATTEMPTS=5
SECURITY_LOCKOUT_DURATION_MINUTES=15
```

**Crear el archivo .env:**
```powershell
# Copiar desde el ejemplo
cd C:\Github\GateKeep-DOTNET-Nico\Gatekeep\src
Copy-Item ".env.example" ".env"

# Editar con tus valores reales
notepad .env
```

---

## 🚀 Levantar el Proyecto

### Opción 1: Desarrollo Local (sin Docker)

```powershell
# 1. Navegar al proyecto
cd C:\Github\GateKeep-DOTNET-Nico\Gatekeep\src\GateKeep.Api

# 2. Restaurar dependencias
dotnet restore

# 3. Compilar
dotnet build

# 4. Ejecutar
dotnet run

# O todo en uno:
dotnet clean && dotnet build && dotnet run
```

**Acceder a la API:**
- Swagger: `http://localhost:5011/swagger`
- Health Check: `http://localhost:5011/health`

---

### Opción 2: Con Docker Compose

```powershell
# 1. Navegar al directorio src
cd C:\Github\GateKeep-DOTNET-Nico\Gatekeep\src

# 2. Asegurar que existe el archivo .env
Test-Path ".env"  # Debe devolver True

# 3. Construir las imágenes
docker-compose build

# 4. Levantar todos los servicios
docker-compose up -d

# 5. Ver logs en tiempo real
docker-compose logs -f api

# 6. Verificar estado de servicios
docker-compose ps
```

**Servicios levantados:**
- API: `http://localhost:5011`
- PostgreSQL: `localhost:5432`
- Redis: `localhost:6379`
- Seq (Logs): `http://localhost:5341`
- Prometheus: `http://localhost:9090`
- Grafana: `http://localhost:3001`

---

## 🔄 Cambiar Puertos

### En Desarrollo Local

**Opción 1: Archivo .env**
```powershell
# Editar .env
notepad C:\Github\GateKeep-DOTNET-Nico\Gatekeep\src\.env

# Cambiar:
GATEKEEP_PORT=5011  # Por el puerto deseado (ej: 5020)
```

**Opción 2: Variable de entorno temporal**
```powershell
$env:GATEKEEP_PORT="5020"
dotnet run
```

### En Docker

**Editar docker-compose.yaml:**
```yaml
services:
  api:
    ports:
      - "${GATEKEEP_PORT}:${GATEKEEP_PORT}"  # Puerto externo:interno
```

**Cambiar puerto en .env:**
```env
GATEKEEP_PORT=5020
```

**Aplicar cambios:**
```powershell
docker-compose down
docker-compose up -d
```

---

## 🐳 Gestión de Docker

### Comandos Básicos

```powershell
# Ver servicios en ejecución
docker-compose ps

# Ver logs
docker-compose logs -f           # Todos los servicios
docker-compose logs -f api       # Solo API
docker-compose logs -f postgres  # Solo PostgreSQL

# Reiniciar un servicio
docker-compose restart api

# Detener todos los servicios
docker-compose stop

# Iniciar servicios detenidos
docker-compose start

# Detener y eliminar contenedores
docker-compose down

# Detener y eliminar contenedores + volúmenes (CUIDADO: borra datos)
docker-compose down -v
```

### Recrear Contenedores

**Recrear solo la API:**
```powershell
cd C:\Github\GateKeep-DOTNET-Nico\Gatekeep\src

# Reconstruir imagen
docker-compose build api

# Recrear contenedor
docker-compose up -d --force-recreate api
```

**Recrear todos los servicios:**
```powershell
# Detener todo
docker-compose down

# Limpiar imágenes antiguas (opcional)
docker-compose build --no-cache

# Levantar todo de nuevo
docker-compose up -d
```

**Limpiar todo y empezar desde cero:**
```powershell
# ADVERTENCIA: Esto elimina TODOS los datos
docker-compose down -v
docker system prune -a --volumes -f
docker-compose up -d
```

---

## 🗄️ Gestión de Base de Datos

### PostgreSQL Local

```powershell
# Conectar a PostgreSQL
psql -U postgres -h localhost -p 5432

# Crear base de datos
CREATE DATABASE Gatekeep;

# Eliminar y recrear base de datos
DROP DATABASE IF EXISTS Gatekeep;
CREATE DATABASE Gatekeep;

# Ver bases de datos
\l

# Conectar a base de datos específica
\c Gatekeep

# Ver tablas
\dt

# Salir
\q
```

### PostgreSQL en Docker

```powershell
# Conectar al contenedor
docker exec -it gatekeep-postgres psql -U postgres -d Gatekeep

# Backup de base de datos
docker exec gatekeep-postgres pg_dump -U postgres Gatekeep > backup.sql

# Restaurar backup
docker exec -i gatekeep-postgres psql -U postgres Gatekeep < backup.sql
```

---

## 🔍 Diagnóstico y Solución de Problemas

### Verificar Configuración

```powershell
# Verificar que existe .env
Test-Path "C:\Github\GateKeep-DOTNET-Nico\Gatekeep\src\.env"

# Ver contenido de .env
Get-Content "C:\Github\GateKeep-DOTNET-Nico\Gatekeep\src\.env"

# Verificar puertos en uso
netstat -ano | findstr :5011
netstat -ano | findstr :5432
netstat -ano | findstr :6379
```

### Limpiar y Reconstruir Proyecto

```powershell
cd C:\Github\GateKeep-DOTNET-Nico\Gatekeep\src\GateKeep.Api

# Limpiar completamente
dotnet clean
Remove-Item -Recurse -Force bin, obj

# Restaurar paquetes
dotnet restore

# Reconstruir
dotnet build

# Ejecutar
dotnet run
```

### Problemas Comunes

**Error: "Puerto ya en uso"**
```powershell
# Encontrar proceso usando el puerto
netstat -ano | findstr :5011

# Matar proceso (reemplazar PID con el número del proceso)
taskkill /PID <PID> /F
```

**Error: "Archivo .env no encontrado"**
```powershell
# Verificar ubicación
cd C:\Github\GateKeep-DOTNET-Nico\Gatekeep\src
Test-Path ".env"

# Crear desde ejemplo
Copy-Item ".env.example" ".env"
```

**Error: "Autenticación PostgreSQL fallida"**
```powershell
# Verificar contraseña en .env
Get-Content ".env" | Select-String "DB_PASSWORD"

# Probar conexión manualmente
psql -U postgres -h localhost -p 5432 -W
```

**Error: "No se puede conectar a Docker"**
```powershell
# Reiniciar Docker Desktop
# Verificar que Docker está corriendo
docker ps

# Verificar servicio
Get-Service *docker*
```

---

## 📊 Herramientas de Observabilidad

### Seq (Logs centralizados)
```
URL: http://localhost:5341
Usuario: admin
Contraseña: admin
```

### Prometheus (Métricas)
```
URL: http://localhost:9090
```

### Grafana (Dashboards)
```
URL: http://localhost:3001
Usuario: admin
Contraseña: admin123
```

### Redis Insight (opcional)
```powershell
# Descargar desde: https://redis.com/redis-enterprise/redis-insight/
# Conectar a: localhost:6379
```

---

## 🔄 Actualizar Configuración sin Reiniciar

### Variables de Entorno

**Para cambios en .env necesitas reiniciar:**
```powershell
# Desarrollo local
Ctrl+C  # Detener aplicación
dotnet run  # Reiniciar

# Docker
docker-compose restart api
```

### Archivos de Configuración (config.json)

Los archivos `config.json` ahora están vacíos. Todas las configuraciones se manejan con variables de entorno.

---

## 📝 Scripts Útiles

### Script para Levantar Todo (PowerShell)

Crear `iniciar-proyecto.ps1`:
```powershell
# Verificar Docker
if (!(Get-Process "Docker Desktop" -ErrorAction SilentlyContinue)) {
    Write-Host "Iniciando Docker Desktop..."
    Start-Process "C:\Program Files\Docker\Docker\Docker Desktop.exe"
    Start-Sleep -Seconds 10
}

# Navegar al directorio
Set-Location "C:\Github\GateKeep-DOTNET-Nico\Gatekeep\src"

# Verificar .env
if (!(Test-Path ".env")) {
    Write-Host "ERROR: Archivo .env no encontrado" -ForegroundColor Red
    exit 1
}

# Levantar servicios
Write-Host "Levantando servicios..." -ForegroundColor Green
docker-compose up -d

# Esperar a que estén listos
Start-Sleep -Seconds 5

# Mostrar estado
docker-compose ps

# Abrir navegador
Start-Process "http://localhost:5011/swagger"
```

### Script para Detener Todo
Crear `detener-proyecto.ps1`:
```powershell
Set-Location "C:\Github\GateKeep-DOTNET-Nico\Gatekeep\src"
docker-compose down
Write-Host "Servicios detenidos" -ForegroundColor Green
```

---

## 🎯 Comandos Rápidos de Referencia

```powershell
# DESARROLLO LOCAL
cd C:\Github\GateKeep-DOTNET-Nico\Gatekeep\src\GateKeep.Api
dotnet run

# DOCKER - Iniciar
cd C:\Github\GateKeep-DOTNET-Nico\Gatekeep\src
docker-compose up -d

# DOCKER - Ver logs
docker-compose logs -f api

# DOCKER - Reiniciar API
docker-compose restart api

# DOCKER - Detener todo
docker-compose down

# DOCKER - Recrear todo
docker-compose down && docker-compose build && docker-compose up -d

# CAMBIAR PUERTO
# Editar: src/.env → GATEKEEP_PORT=<nuevo_puerto>
# Reiniciar aplicación o Docker

# VERIFICAR .ENV
Test-Path "C:\Github\GateKeep-DOTNET-Nico\Gatekeep\src\.env"
Get-Content "C:\Github\GateKeep-DOTNET-Nico\Gatekeep\src\.env"
```

---

## 📚 Archivos de Configuración Clave

```
Gatekeep/
├── src/
│   ├── .env                          ← Variables de entorno (PRINCIPAL)
│   ├── .env.example                  ← Plantilla de ejemplo
│   ├── docker-compose.yaml           ← Configuración Docker
│   └── GateKeep.Api/
│       ├── Program.cs                ← Punto de entrada de la aplicación
│       ├── appsettings.json          ← Configuración de logging
│       ├── appsettings.Development.json
│       ├── appsettings.Production.json
│       ├── config.json               ← Vacío (usar variables de entorno)
│       └── config.Production.json    ← Vacío (usar variables de entorno)
```

---

## ⚡ Flujo de Trabajo Recomendado

### Para Desarrollo Diario

1. **Iniciar servicios:**
   ```powershell
   cd C:\Github\GateKeep-DOTNET-Nico\Gatekeep\src
   docker-compose up -d postgres redis
   ```

2. **Ejecutar API localmente:**
   ```powershell
   cd GateKeep.Api
   dotnet run
   ```

3. **Al terminar:**
   ```powershell
   Ctrl+C  # Detener API
   docker-compose stop  # Detener servicios
   ```

### Para Producción/Testing Completo

```powershell
cd C:\Github\GateKeep-DOTNET-Nico\Gatekeep\src
docker-compose up -d
```

---

## 📞 Soporte

Si encuentras problemas:

1. Revisa los logs: `docker-compose logs -f api`
2. Verifica configuración: `Get-Content src\.env`
3. Limpia y reconstruye: `dotnet clean && dotnet build`
4. Reinicia Docker: `docker-compose down && docker-compose up -d`

---

**Última actualización:** 11 de noviembre de 2025

