# Variables de Entorno

Esta guía documenta todas las variables de entorno necesarias para GateKeep.

## Variables para App Runner (API)

### Configuración de Aplicación

| Variable | Descripción | Valor por Defecto | Dónde Configurarla |
|----------|-------------|-------------------|---------------------|
| `ASPNETCORE_ENVIRONMENT` | Entorno de ejecución | `Production` | App Runner Environment Variables |
| `ASPNETCORE_URLS` | URLs donde escucha la API | `http://+:5011` | App Runner Environment Variables |
| `GATEKEEP_PORT` | Puerto de la aplicación | `5011` | App Runner Environment Variables |

### Base de Datos PostgreSQL

| Variable | Descripción | Valor por Defecto | Dónde Configurarla |
|----------|-------------|-------------------|---------------------|
| `DATABASE__HOST` | Host de PostgreSQL | - | Parameter Store: `/gatekeep/db/host` |
| `DATABASE__PORT` | Puerto de PostgreSQL | `5432` | Parameter Store: `/gatekeep/db/port` |
| `DATABASE__NAME` | Nombre de la base de datos | `Gatekeep` | Parameter Store: `/gatekeep/db/name` |
| `DATABASE__USER` | Usuario de PostgreSQL | `postgres` | Parameter Store: `/gatekeep/db/username` |
| `DATABASE__PASSWORD` | Contraseña de PostgreSQL | - | Secrets Manager: `gatekeep/db/password` |

**Nota:** El formato `DATABASE__HOST` (con doble guión bajo) es el formato de .NET para nested configuration.

### JWT (Autenticación)

| Variable | Descripción | Valor por Defecto | Dónde Configurarla |
|----------|-------------|-------------------|---------------------|
| `JWT__KEY` | Clave secreta para firmar tokens JWT | - | Secrets Manager: `gatekeep/jwt/key` |
| `JWT__ISSUER` | Emisor de los tokens | `GateKeep` | App Runner Environment Variables |
| `JWT__AUDIENCE` | Audiencia de los tokens | `GateKeepUsers` | App Runner Environment Variables |
| `JWT__EXPIRATIONHOURS` | Horas de expiración del token | `8` | App Runner Environment Variables |

### Redis (Caché) - Opcional

| Variable | Descripción | Valor por Defecto | Dónde Configurarla |
|----------|-------------|-------------------|---------------------|
| `REDIS__ENABLED` | Habilitar Redis | `false` | App Runner Environment Variables |
| `REDIS__CONNECTIONSTRING` | Connection string de Redis | - | App Runner Environment Variables |
| `REDIS__INSTANCENAME` | Nombre de instancia | `GateKeep:` | App Runner Environment Variables |

**Nota:** En la versión simplificada, Redis está deshabilitado (`REDIS__ENABLED=false`).

### MongoDB (Auditoría) - Opcional

| Variable | Descripción | Valor por Defecto | Dónde Configurarla |
|----------|-------------|-------------------|---------------------|
| `MONGODB_CONNECTION` | Connection string de MongoDB | - | App Runner Environment Variables |
| `MONGODB_DATABASE` | Nombre de la base de datos | `GateKeepMongo` | App Runner Environment Variables |
| `MONGODB_USE_STABLE_API` | Usar API estable de MongoDB | `false` | App Runner Environment Variables |

**Nota:** En la versión simplificada, MongoDB está deshabilitado (variable vacía).

### Seguridad

| Variable | Descripción | Valor por Defecto | Dónde Configurarla |
|----------|-------------|-------------------|---------------------|
| `SECURITY__PASSWORDMINLENGTH` | Longitud mínima de contraseña | `8` | App Runner Environment Variables |
| `SECURITY__MAXLOGINATTEMPTS` | Intentos máximos de login | `5` | App Runner Environment Variables |
| `SECURITY__LOCKOUTDURATIONMINUTES` | Duración de bloqueo (minutos) | `15` | App Runner Environment Variables |

## Variables para App Runner (Frontend)

| Variable | Descripción | Valor por Defecto | Dónde Configurarla |
|----------|-------------|-------------------|---------------------|
| `REACT_APP_API_URL` | URL de la API de backend | - | App Runner Environment Variables |
| `NODE_ENV` | Entorno de Node.js | `production` | App Runner Environment Variables |
| `PORT` | Puerto del frontend | `3000` | App Runner Environment Variables |

## Configuración Local (.env)

Para desarrollo local, crear archivo `src/.env`:

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
JWT_KEY=clave-secreta-minimo-256-bits-para-desarrollo
JWT_ISSUER=GateKeep
JWT_AUDIENCE=GateKeepUsers

# Redis (opcional)
REDIS_CONNECTION=localhost:6379
REDIS_INSTANCE=GateKeep:
REDIS_ENABLED=true

# MongoDB (opcional)
MONGODB_CONNECTION=mongodb://localhost:27017
MONGODB_DATABASE=GateKeepMongo
MONGODB_USE_STABLE_API=false
```

## Mapeo Local vs AWS

### Local (Docker Compose)

Las variables se leen desde `src/.env` y se pasan a los contenedores Docker.

### AWS (App Runner)

Las variables se configuran de dos formas:

1. **Desde Parameter Store** (para valores no sensibles):
   - Se referencian en App Runner como: `{{resolve:ssm:/gatekeep/db/host}}`

2. **Desde Secrets Manager** (para valores sensibles):
   - Se referencian en App Runner como: `{{resolve:secretsmanager:gatekeep/db/password:password::}}`

3. **Directamente en App Runner** (para valores fijos):
   - Se configuran en la sección "Environment variables" del servicio

## Ejemplo de Configuración en App Runner

### Para API

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:5011
DATABASE__HOST={{resolve:ssm:/gatekeep/db/host}}
DATABASE__PORT={{resolve:ssm:/gatekeep/db/port}}
DATABASE__NAME={{resolve:ssm:/gatekeep/db/name}}
DATABASE__USER={{resolve:ssm:/gatekeep/db/username}}
DATABASE__PASSWORD={{resolve:secretsmanager:gatekeep/db/password:password::}}
JWT__KEY={{resolve:secretsmanager:gatekeep/jwt/key:key::}}
JWT__ISSUER=GateKeep
JWT__AUDIENCE=GateKeepUsers
REDIS__ENABLED=false
MONGODB_CONNECTION=
```

### Para Frontend

```
REACT_APP_API_URL=https://gatekeep-api.xxxxx.us-east-1.awsapprunner.com
NODE_ENV=production
PORT=3000
```

## Generar Clave JWT

Para generar una clave JWT segura de 256 bits:

```bash
# Linux/Mac
openssl rand -base64 32

# Windows PowerShell
[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }))
```

## Verificar Variables Configuradas

### En App Runner

1. Ir a App Runner Console
2. Seleccionar el servicio
3. Ir a la pestaña "Configuration"
4. Ver "Environment variables"

### En Parameter Store

```bash
aws ssm get-parameters --names /gatekeep/db/host /gatekeep/db/port --region us-east-1
```

### En Secrets Manager

```bash
aws secretsmanager get-secret-value --secret-id gatekeep/db/password --region us-east-1
```

## Troubleshooting

### Variable no se lee correctamente

1. Verificar que el nombre de la variable es correcto (case-sensitive)
2. Verificar que está configurada en App Runner
3. Verificar formato de referencia a Parameter Store/Secrets Manager
4. Revisar logs en CloudWatch

### Error de conexión a base de datos

1. Verificar que `DATABASE__HOST` tiene el endpoint correcto de RDS
2. Verificar que `DATABASE__PASSWORD` está correctamente configurado
3. Verificar Security Group de RDS permite conexiones desde App Runner

