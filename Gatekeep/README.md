# GateKeep API

Sistema de gestión de acceso y control para espacios universitarios construido con .NET 8, PostgreSQL, MongoDB, Redis y arquitectura ECS.

## 🚀 Inicio Rápido

### Con Scripts PowerShell (Recomendado)

```powershell
# Iniciar todo con Docker
.\iniciar-docker.ps1

# Detener servicios
.\detener-docker.ps1

# Recrear contenedores
.\recrear-docker.ps1
```

### Comandos Manuales

**Desarrollo Local:**
```powershell
cd src\GateKeep.Api
dotnet run
```

**Con Docker:**
```powershell
cd src
docker-compose up -d
```

## 📋 Requisitos

- **.NET 8.0 SDK**
- **Docker Desktop** (para ejecución con contenedores)
- **PostgreSQL** (para desarrollo local)
- **Redis** (para desarrollo local)
- **MongoDB** (opcional, para funciones de auditoría)

## ⚙️ Configuración

### 1. Variables de Entorno

Copia y edita el archivo de ejemplo:

```powershell
cd src
Copy-Item ".env.example" ".env"
notepad .env
```

**Variables principales:**
```env
# Base de Datos
DB_HOST=localhost
DB_PORT=5432
DB_NAME=Gatekeep
DB_USER=postgres
DB_PASSWORD=tu_contraseña

# API
GATEKEEP_PORT=5011
APP_ENVIRONMENT=Development

# JWT
JWT_KEY=clave-secreta-minimo-256-bits
JWT_ISSUER=GateKeep
JWT_AUDIENCE=GateKeepUsers

# MongoDB
MONGODB_CONNECTION=tu_connection_string
MONGODB_DATABASE=GateKeepMongo

# Redis
REDIS_CONNECTION=localhost:6379
REDIS_INSTANCE=GateKeep:
```

### 2. Primera Ejecución

```powershell
# Con Docker (incluye PostgreSQL y Redis)
.\iniciar-docker.ps1

# O manualmente
cd src
docker-compose up -d
```

## 🌐 URLs de Acceso

| Servicio | URL | Credenciales |
|----------|-----|--------------|
| **API Swagger** | http://localhost:5011/swagger | - |
| **Health Check** | http://localhost:5011/health | - |
| **Seq (Logs)** | http://localhost:5341 | admin / admin |
| **Prometheus** | http://localhost:9090 | - |
| **Grafana** | http://localhost:3001 | admin / admin123 |

## 📁 Estructura del Proyecto

```
Gatekeep/
├── src/
│   ├── .env                      # Variables de entorno (NO en git)
│   ├── .env.example              # Plantilla de variables
│   ├── docker-compose.yaml       # Configuración Docker
│   ├── README-COMANDOS.md        # Comandos rápidos
│   └── GateKeep.Api/
│       ├── Program.cs
│       ├── Application/          # Lógica de negocio
│       ├── Domain/               # Entidades y enums
│       ├── Infrastructure/       # Repositorios y servicios
│       ├── Endpoints/            # Minimal API endpoints
│       └── Contracts/            # DTOs y contratos
├── docs/                         # Documentación técnica
├── scripts/                      # Scripts útiles
├── iniciar-docker.ps1           # Script para iniciar
├── detener-docker.ps1           # Script para detener
├── recrear-docker.ps1           # Script para recrear
├── GUIA-INICIO.md               # Guía completa
└── README.md                    # Este archivo
```

## 🔧 Comandos Útiles

### Docker

```powershell
# Ver logs en tiempo real
docker-compose logs -f api

# Reiniciar un servicio
docker-compose restart api

# Ver estado
docker-compose ps

# Detener todo
docker-compose down

# Recrear contenedores
docker-compose down && docker-compose up -d --build
```

### .NET

```powershell
# Limpiar y reconstruir
dotnet clean && dotnet build

# Ejecutar en modo watch (recarga automática)
dotnet watch run

# Ejecutar tests
dotnet test
```

### Base de Datos

```powershell
# Conectar a PostgreSQL (Docker)
docker exec -it gatekeep-postgres psql -U postgres -d Gatekeep

# Backup
docker exec gatekeep-postgres pg_dump -U postgres Gatekeep > backup.sql

# Restore
docker exec -i gatekeep-postgres psql -U postgres Gatekeep < backup.sql
```

## 🎯 Características Principales

- ✅ **Autenticación JWT** - Sistema seguro de tokens
- ✅ **Control de Acceso** - Reglas configurables por rol y espacio
- ✅ **Arquitectura ECS** - Entity-Component-System reutilizable
- ✅ **Caching con Redis** - Optimización de rendimiento
- ✅ **Auditoría MongoDB** - Registro de eventos históricos
- ✅ **Observabilidad** - Logs, métricas y trazabilidad
- ✅ **QR Codes** - Generación de códigos para acceso
- ✅ **Minimal API** - Endpoints ligeros y rápidos

## 📊 Stack Tecnológico

- **Backend:** .NET 8.0, C# 12
- **Base de Datos:** PostgreSQL 16, MongoDB 7, Redis 7
- **ORM:** Entity Framework Core 9
- **Logging:** Serilog, Seq
- **Métricas:** OpenTelemetry, Prometheus
- **Visualización:** Grafana
- **Seguridad:** BCrypt, JWT Bearer
- **Contenedores:** Docker, Docker Compose

## 🔄 Cambiar Puerto

1. Editar `src/.env`:
   ```env
   GATEKEEP_PORT=5020
   ```

2. Reiniciar:
   ```powershell
   docker-compose down && docker-compose up -d
   ```

## 🐛 Solución de Problemas

### Puerto ya en uso
```powershell
netstat -ano | findstr :5011
taskkill /PID <PID> /F
```

### Variables no cargadas
```powershell
# Verificar que .env existe
Test-Path "src\.env"

# Ver contenido
Get-Content "src\.env"
```

### Error de autenticación PostgreSQL
```powershell
# Verificar contraseña en .env
Get-Content "src\.env" | Select-String "DB_PASSWORD"
```

### Docker no responde
```powershell
# Reiniciar servicios
docker-compose down
docker-compose up -d

# O usar el script
.\recrear-docker.ps1
```

## 📚 Documentación

- **[GUIA-INICIO.md](./GUIA-INICIO.md)** - Guía completa de configuración
- **[src/README-COMANDOS.md](./src/README-COMANDOS.md)** - Comandos rápidos
- **[docs/](./docs/)** - Documentación técnica detallada
  - **[AWS_SETUP.md](./docs/AWS_SETUP.md)** - Instalación y configuración de AWS CLI
  - **[PLAN_DESPLIEGUE_AUTOMATIZACION.md](./docs/PLAN_DESPLIEGUE_AUTOMATIZACION.md)** - Plan completo para CI/CD y despliegue en AWS
  - **[DEPLOYMENT.md](./docs/DEPLOYMENT.md)** - Guía paso a paso para desplegar en AWS
  - **[ENVIRONMENT_VARIABLES.md](./docs/ENVIRONMENT_VARIABLES.md)** - Variables de entorno y configuración

## 🤝 Contribuir

1. Las variables de entorno son **obligatorias** - usar `src/.env`
2. Los archivos `config.json` están vacíos - no agregar credenciales
3. Seguir la arquitectura ECS establecida
4. Documentar cambios importantes

## 📝 Notas Importantes

- El archivo `.env` **NO** se sube a Git (está en `.gitignore`)
- Siempre usar `.env.example` como plantilla
- Las credenciales de producción deben estar en variables de entorno del servidor
- Los archivos `config.json` y `config.Production.json` están vacíos intencionalmente

## 📞 Soporte

Para problemas o dudas:
1. Revisa la [Guía de Inicio](./GUIA-INICIO.md)
2. Consulta los logs: `docker-compose logs -f api`
3. Verifica configuración: `Get-Content src\.env`

---

**Última actualización:** 11 de noviembre de 2025

