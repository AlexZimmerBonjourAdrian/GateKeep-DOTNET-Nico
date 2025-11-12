# Guía de Despliegue en AWS

Esta guía te ayudará a desplegar GateKeep en AWS usando App Runner y RDS PostgreSQL.

## Prerequisitos

- Cuenta AWS con permisos adecuados
- AWS CLI instalado y configurado (`aws configure`)
  - **Ver guía completa:** [AWS_SETUP.md](./AWS_SETUP.md)
- Docker instalado (para construir imágenes localmente)
- Git y acceso al repositorio

### Verificar Instalación

Antes de continuar, verifica que AWS CLI está instalado:

```powershell
# Ejecutar script de verificación
.\scripts\verify-aws-cli.ps1

# O manualmente
aws --version
aws configure list
aws sts get-caller-identity
```

## Resumen Rápido

1. Crear recursos AWS (ECR, RDS, Secrets Manager, Parameter Store)
2. Crear App Runner Services
3. Configurar GitHub Secrets
4. Push inicial de imágenes
5. Verificar despliegue

## Paso 1: Crear ECR Repositories

1. Ir a [ECR Console](https://console.aws.amazon.com/ecr)
2. Click en **Create repository**
3. Crear dos repositorios:
   - **gatekeep-api** (tipo: Private)
   - **gatekeep-frontend** (tipo: Private)
4. Anotar las URIs de los repositorios

**Ejemplo de URI:**
```
123456789.dkr.ecr.us-east-1.amazonaws.com/gatekeep-api
```

## Paso 2: Crear RDS PostgreSQL

1. Ir a [RDS Console](https://console.aws.amazon.com/rds)
2. Click en **Create database**
3. Configuración recomendada:
   - **Engine**: PostgreSQL 16
   - **Template**: Free tier (si aplica) o Dev/Test
   - **DB instance identifier**: `gatekeep-db`
   - **Master username**: `postgres`
   - **Master password**: Generar contraseña segura y guardarla
   - **DB instance class**: `db.t4g.micro` (más económico)
   - **Storage**: 20 GB (gp3)
   - **Public access**: **SÍ** (para simplificar)
   - **VPC**: Default VPC
   - **Security group**: Crear nuevo
     - Regla: Permitir PostgreSQL (5432) desde 0.0.0.0/0 (temporal, ajustar después)
   - **Database name**: `Gatekeep`
   - **Backup**: Habilitar (7 días de retención)

4. Anotar el **endpoint** de RDS

**Ejemplo de endpoint:**
```
gatekeep-db.xxxxx.us-east-1.rds.amazonaws.com
```

## Paso 3: Configurar Secrets Manager

1. Ir a [Secrets Manager Console](https://console.aws.amazon.com/secretsmanager)
2. Click en **Store a new secret**

### Secret para DB Password

- **Secret type**: Other type of secret
- **Key/value**:
  - Key: `password`
  - Value: [password de RDS creado anteriormente]
- **Secret name**: `gatekeep/db/password`

### Secret para JWT Key

- **Secret type**: Other type of secret
- **Key/value**:
  - Key: `key`
  - Value: [generar clave JWT de 256 bits, ej: usar `openssl rand -base64 32`]
- **Secret name**: `gatekeep/jwt/key`

## Paso 4: Configurar Parameter Store

1. Ir a [Systems Manager Parameter Store](https://console.aws.amazon.com/systems-manager/parameters)
2. Click en **Create parameter** para cada uno:

| Nombre | Tipo | Valor |
|--------|------|-------|
| `/gatekeep/db/host` | String | [RDS endpoint] |
| `/gatekeep/db/port` | String | `5432` |
| `/gatekeep/db/name` | String | `Gatekeep` |
| `/gatekeep/db/username` | String | `postgres` |
| `/gatekeep/app/environment` | String | `Production` |
| `/gatekeep/app/port` | String | `5011` |

## Paso 5: Crear App Runner Service para API

1. Ir a [App Runner Console](https://console.aws.amazon.com/apprunner)
2. Click en **Create service**

### Source Configuration

- **Source type**: Container registry
- **Provider**: Amazon ECR
- **Container image URI**: Seleccionar `gatekeep-api:latest` del repositorio ECR
- **Deployment trigger**: Automatic

### Service Settings

- **Service name**: `gatekeep-api`
- **Virtual CPU**: 1 vCPU
- **Memory**: 2 GB

### Auto Scaling

- **Min size**: 1
- **Max size**: 3
- **Concurrency**: 100 requests/instance

### Health Check

- **Path**: `/health`
- **Interval**: 10 seconds
- **Timeout**: 5 seconds

### Network

- **Egress type**: Default (no VPC)

### Environment Variables

Configurar las siguientes variables:

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:5011
DATABASE__HOST=[desde Parameter Store: /gatekeep/db/host]
DATABASE__PORT=[desde Parameter Store: /gatekeep/db/port]
DATABASE__NAME=[desde Parameter Store: /gatekeep/db/name]
DATABASE__USER=[desde Parameter Store: /gatekeep/db/username]
DATABASE__PASSWORD=[desde Secrets Manager: gatekeep/db/password]
JWT__KEY=[desde Secrets Manager: gatekeep/jwt/key]
JWT__ISSUER=GateKeep
JWT__AUDIENCE=GateKeepUsers
REDIS__ENABLED=false
MONGODB_CONNECTION=
```

### Port

- **Port**: `5011`

3. Anotar la **URL del servicio** (ej: `https://gatekeep-api.xxxxx.us-east-1.awsapprunner.com`)

## Paso 6: Crear App Runner Service para Frontend

Similar al anterior, pero con estas diferencias:

- **Service name**: `gatekeep-frontend`
- **Container image URI**: `gatekeep-frontend:latest`
- **Virtual CPU**: 0.5 vCPU
- **Memory**: 1 GB
- **Min size**: 1, **Max size**: 2
- **Port**: `3000`
- **Environment variables**:
  ```
  REACT_APP_API_URL=[URL del servicio API de App Runner]
  NODE_ENV=production
  PORT=3000
  ```

## Paso 7: Primer Despliegue Manual

### Construir y Push Imágenes a ECR

```bash
# Login a ECR
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin [ECR_URI]

# API
cd src
docker build -t gatekeep-api:latest -f Dockerfile .
docker tag gatekeep-api:latest [ECR_URI_API]:latest
docker push [ECR_URI_API]:latest

# Frontend
cd ../frontend
docker build -t gatekeep-frontend:latest -f Dockerfile .
docker tag gatekeep-frontend:latest [ECR_URI_FRONTEND]:latest
docker push [ECR_URI_FRONTEND]:latest
```

App Runner detectará automáticamente los cambios y desplegará.

## Paso 8: Configurar GitHub Secrets

1. Ir a tu repositorio en GitHub
2. **Settings** → **Secrets and variables** → **Actions**
3. Click en **New repository secret**
4. Agregar:
   - `AWS_ACCESS_KEY_ID`: Tu access key de AWS
   - `AWS_SECRET_ACCESS_KEY`: Tu secret access key de AWS

## Paso 9: Verificar Despliegue

1. Esperar a que App Runner complete el deployment (5-10 minutos)
2. Verificar estado en App Runner Console (debe estar "Running")
3. Probar endpoints:
   - API Health: `https://[api-url]/health`
   - Frontend: `https://[frontend-url]`
4. Verificar logs en CloudWatch

## Troubleshooting

### App Runner no puede conectarse a RDS

1. Verificar Security Group de RDS permite tráfico desde App Runner
2. Verificar que RDS tiene "Public access" habilitado
3. Verificar variables de entorno en App Runner

### Imágenes no se despliegan automáticamente

1. Verificar que "Deployment trigger" está en "Automatic"
2. Verificar que la imagen en ECR tiene tag `latest`
3. Forzar nuevo deployment manualmente desde App Runner Console

### Errores en los logs

1. Revisar CloudWatch Logs del servicio App Runner
2. Verificar que todas las variables de entorno están configuradas
3. Verificar que los secrets en Secrets Manager existen

## Costos Estimados

- RDS db.t4g.micro: ~$12/mes
- App Runner API: ~$20/mes
- App Runner Frontend: ~$10/mes
- ECR: ~$1/mes
- **Total: ~$43/mes**

## Próximos Pasos

Una vez desplegado, puedes:
1. Configurar dominio personalizado (opcional)
2. Habilitar Redis/ElastiCache si es necesario
3. Configurar VPC para mayor seguridad
4. Implementar Terraform para infraestructura como código

Para más detalles, consulta: `docs/PLAN_DESPLIEGUE_AUTOMATIZACION.md`

