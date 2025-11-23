# Documentación Completa de Configuración AWS - GateKeep

## Tabla de Contenidos
1. [Información General](#información-general)
2. [VPC y Networking](#vpc-y-networking)
3. [Application Load Balancer (ALB)](#application-load-balancer-alb)
4. [ECS (Elastic Container Service)](#ecs-elastic-container-service)
5. [ECR (Elastic Container Registry)](#ecr-elastic-container-registry)
6. [RDS (PostgreSQL)](#rds-postgresql)
7. [ElastiCache (Redis)](#elasticache-redis)
8. [Secrets Manager](#secrets-manager)
9. [Parameter Store (SSM)](#parameter-store-ssm)
10. [Route53 (DNS)](#route53-dns)
11. [Security Groups](#security-groups)
12. [IAM Roles y Políticas](#iam-roles-y-políticas)
13. [Comandos AWS CLI](#comandos-aws-cli)
14. [Cambios y Configuración del Load Balancer](#cambios-y-configuración-del-load-balancer)

---

## Información General

### Cuenta y Región
- **Account ID**: `126588786097`
- **Región**: `sa-east-1` (São Paulo, Brasil)
- **Proyecto**: `gatekeep`
- **Ambiente**: `dev`
- **Dominio**: `zimmzimmgames.com`

### URLs Públicas
- **Frontend**: `https://zimmzimmgames.com`
- **Backend API**: `https://api.zimmzimmgames.com`
- **ALB DNS**: `gatekeep-alb-878011876.sa-east-1.elb.amazonaws.com`

---

## VPC y Networking

### VPC Principal
- **VPC ID**: `vpc-020bbcab6b221869d` (puede haber múltiples VPCs con el mismo nombre)
- **CIDR Block**: `10.0.0.0/16`
- **DNS Hostnames**: Habilitado
- **DNS Support**: Habilitado

### Subnets

#### Subnets Públicas
- **Subnet 1**: `10.0.1.0/24` - Zona: `sa-east-1a`
- **Subnet 2**: `10.0.2.0/24` - Zona: `sa-east-1b`
- **Propósito**: ALB, ECS Tasks (con IPs públicas)

#### Subnets Privadas
- **Subnet 1**: `10.0.10.0/24` - Zona: `sa-east-1a`
- **Subnet 2**: `10.0.11.0/24` - Zona: `sa-east-1b`
- **Propósito**: RDS, ElastiCache Redis

### Internet Gateway
- **Nombre**: `gatekeep-igw`
- **Propósito**: Conecta la VPC a Internet

### Route Tables
- **Pública**: Ruta `0.0.0.0/0` → Internet Gateway
- **Privada**: Sin ruta a Internet (solo tráfico interno)

### Comandos AWS CLI

```powershell
# Listar VPCs
aws ec2 describe-vpcs --filters "Name=tag:Name,Values=gatekeep-vpc" --region sa-east-1 --query "Vpcs[*].[VpcId,CidrBlock,Tags[?Key=='Name'].Value|[0]]" --output table

# Listar Subnets
aws ec2 describe-subnets --filters "Name=vpc-id,Values=vpc-020bbcab6b221869d" --region sa-east-1 --query "Subnets[*].[SubnetId,CidrBlock,AvailabilityZone,Tags[?Key=='Name'].Value|[0]]" --output table

# Listar Route Tables
aws ec2 describe-route-tables --filters "Name=vpc-id,Values=vpc-020bbcab6b221869d" --region sa-east-1 --query "RouteTables[*].[RouteTableId,Tags[?Key=='Name'].Value|[0]]" --output table
```

---

## Application Load Balancer (ALB)

### Información General
- **Nombre**: `gatekeep-alb`
- **ARN**: `arn:aws:elasticloadbalancing:sa-east-1:126588786097:loadbalancer/app/gatekeep-alb/ff82ae699b9862d2`
- **DNS**: `gatekeep-alb-878011876.sa-east-1.elb.amazonaws.com`
- **Estado**: `active`
- **Tipo**: Application Load Balancer
- **Scheme**: Internet-facing
- **Subnets**: Subnets públicas (2 zonas de disponibilidad)

### Listeners

#### Listener HTTP (Puerto 80)
- **ARN**: `arn:aws:elasticloadbalancing:sa-east-1:126588786097:listener/app/gatekeep-alb/ff82ae699b9862d2/d15f140471f37a0d`
- **Protocolo**: HTTP
- **Acción por defecto**: Redirigir a HTTPS (puerto 443)

#### Listener HTTPS (Puerto 443)
- **ARN**: `arn:aws:elasticloadbalancing:sa-east-1:126588786097:listener/app/gatekeep-alb/ff82ae699b9862d2/d277ff2b7c7f46a8`
- **Protocolo**: HTTPS
- **SSL Policy**: `ELBSecurityPolicy-TLS-1-2-2017-01`
- **Certificado**: ACM Certificate para `zimmzimmgames.com` y `api.zimmzimmgames.com`
- **Acción por defecto**: Forward a `gatekeep-frontend-tg` (Frontend)

### Reglas de Enrutamiento (Listener HTTP - Puerto 80)

| Prioridad | Condición | Acción | Target Group |
|-----------|-----------|--------|--------------|
| 90 | Path: `/api/auth/*` | Forward | `gatekeep-tg` |
| 100 | Path: `/api/*` | Forward | `gatekeep-tg` |
| 120 | Path: `/usuarios/*` | Forward | `gatekeep-tg` |
| 130 | Path: `/swagger*`, `/swagger/*` | Forward | `gatekeep-tg` |
| 140 | Path: `/health` | Forward | `gatekeep-tg` |
| Default | - | Redirect a HTTPS | - |

**Nota**: La regla `/auth/*` fue eliminada porque la aplicación solo expone endpoints bajo `/api/auth/*`. Ver sección [Cambios y Configuración del Load Balancer](#cambios-y-configuración-del-load-balancer) para más detalles.

### Reglas de Enrutamiento (Listener HTTPS - Puerto 443)

| Prioridad | Condición | Acción | Target Group |
|-----------|-----------|--------|--------------|
| 90 | Path: `/api/auth/*` | Forward | `gatekeep-tg` |
| 100 | Path: `/api/*` | Forward | `gatekeep-tg` |
| 120 | Path: `/usuarios/*` | Forward | `gatekeep-tg` |
| 130 | Path: `/swagger*`, `/swagger/*` | Forward | `gatekeep-tg` |
| 140 | Path: `/health*` | Forward | `gatekeep-tg` |
| Default | - | Forward | `gatekeep-frontend-tg` |

**Nota**: La regla `/auth/*` fue eliminada porque la aplicación solo expone endpoints bajo `/api/auth/*`. Ver sección [Cambios y Configuración del Load Balancer](#cambios-y-configuración-del-load-balancer) para más detalles.

### Target Groups

#### Target Group Backend (API)
- **Nombre**: `gatekeep-tg`
- **ARN**: `arn:aws:elasticloadbalancing:sa-east-1:126588786097:targetgroup/gatekeep-tg/27437174e066b9ee`
- **Puerto**: `5011`
- **Protocolo**: HTTP
- **Target Type**: IP (Fargate)
- **Health Check**:
  - Path: `/health`
  - Protocol: HTTP
  - Interval: 30 segundos
  - Timeout: 5 segundos
  - Healthy Threshold: 2
  - Unhealthy Threshold: 3
  - Matcher: `200`

#### Target Group Frontend
- **Nombre**: `gatekeep-frontend-tg`
- **ARN**: `arn:aws:elasticloadbalancing:sa-east-1:126588786097:targetgroup/gatekeep-frontend-tg/fe6df7144ffc7ee4`
- **Puerto**: `3000`
- **Protocolo**: HTTP
- **Target Type**: IP (Fargate)
- **Health Check**:
  - Path: `/`
  - Protocol: HTTP
  - Interval: 30 segundos
  - Timeout: 5 segundos
  - Healthy Threshold: 2
  - Unhealthy Threshold: 3
  - Matcher: `200,404`

### Comandos AWS CLI

```powershell
# Obtener información del ALB
$albArn = "arn:aws:elasticloadbalancing:sa-east-1:126588786097:loadbalancer/app/gatekeep-alb/ff82ae699b9862d2"
aws elbv2 describe-load-balancers --load-balancer-arns $albArn --region sa-east-1

# Listar Listeners
aws elbv2 describe-listeners --load-balancer-arn $albArn --region sa-east-1

# Listar Reglas de un Listener
$listenerArn = "arn:aws:elasticloadbalancing:sa-east-1:126588786097:listener/app/gatekeep-alb/ff82ae699b9862d2/d277ff2b7c7f46a8"
aws elbv2 describe-rules --listener-arn $listenerArn --region sa-east-1

# Listar Target Groups
aws elbv2 describe-target-groups --region sa-east-1 --query "TargetGroups[*].[TargetGroupName,TargetGroupArn,Port,Protocol,HealthCheckPath]" --output table

# Ver estado de Targets
$tgArn = "arn:aws:elasticloadbalancing:sa-east-1:126588786097:targetgroup/gatekeep-tg/27437174e066b9ee"
aws elbv2 describe-target-health --target-group-arn $tgArn --region sa-east-1
```

---

## ECS (Elastic Container Service)

### Cluster
- **Nombre**: `gatekeep-cluster`
- **Container Insights**: Habilitado
- **Región**: `sa-east-1`

### Servicios ECS

#### Servicio Backend (API)
- **Nombre**: `gatekeep-api-service`
- **Cluster**: `gatekeep-cluster`
- **Task Definition**: `gatekeep-api:8` (revisión 8)
- **Versión de Imagen**: `latest` (tag actual en ECR)
- **Imagen ECR**: `126588786097.dkr.ecr.sa-east-1.amazonaws.com/gatekeep-api:latest`
- **Launch Type**: FARGATE
- **Desired Count**: 1
- **Running Count**: 1
- **Estado**: ACTIVE
- **Target Group**: `gatekeep-tg`
- **Container Port**: 5011
- **Última Actualización**: Verificar en ECR para fecha exacta

#### Servicio Frontend
- **Nombre**: `gatekeep-frontend-service`
- **Cluster**: `gatekeep-cluster`
- **Task Definition**: `gatekeep-frontend:7` (revisión 7)
- **Versión de Imagen**: `latest` (tag actual en ECR)
- **Imagen ECR**: `126588786097.dkr.ecr.sa-east-1.amazonaws.com/gatekeep-frontend:latest`
- **Launch Type**: FARGATE
- **Desired Count**: 1
- **Running Count**: 1
- **Estado**: ACTIVE
- **Target Group**: `gatekeep-frontend-tg`
- **Container Port**: 3000
- **Última Actualización**: Verificar en ECR para fecha exacta

### Versiones Actuales en Producción

#### Backend (gatekeep-api)
- **Task Definition**: `gatekeep-api:8`
- **Revisión**: 8
- **Estado**: ACTIVE
- **Imagen ECR**: `126588786097.dkr.ecr.sa-east-1.amazonaws.com/gatekeep-api:latest`
- **Tag de Imagen**: `latest`
- **Última Actualización de Imagen**: 2025-11-22T21:25:08 (UTC-3)
- **Tamaño de Imagen**: 111.82 MB
- **CPU**: 512 (0.5 vCPU)
- **Memoria**: 1024 MB (1 GB)
- **Desired Count**: 1
- **Running Count**: 1

**Comando para verificar versión actual**:
```powershell
aws ecs describe-services --cluster gatekeep-cluster --services gatekeep-api-service --region sa-east-1 --query "services[0].taskDefinition"
```

**Comando para ver fecha exacta de la imagen**:
```powershell
aws ecr describe-images --repository-name gatekeep-api --region sa-east-1 --query "imageDetails[?imageTags[?contains(@, 'latest')]].[imagePushedAt,imageTags,imageSizeInBytes]" --output table
```

#### Frontend (gatekeep-frontend)
- **Task Definition**: `gatekeep-frontend:7`
- **Revisión**: 7
- **Estado**: ACTIVE
- **Imagen ECR**: `126588786097.dkr.ecr.sa-east-1.amazonaws.com/gatekeep-frontend:latest`
- **Tag de Imagen**: `latest`
- **Última Actualización de Imagen**: 2025-11-21T19:08:18 (UTC-3)
- **Tamaño de Imagen**: 449.61 MB
- **CPU**: 256 (0.25 vCPU)
- **Memoria**: 512 MB (0.5 GB)
- **Desired Count**: 1
- **Running Count**: 1

**Comando para verificar versión actual**:
```powershell
aws ecs describe-services --cluster gatekeep-cluster --services gatekeep-frontend-service --region sa-east-1 --query "services[0].taskDefinition"
```

**Comando para ver fecha exacta de la imagen**:
```powershell
aws ecr describe-images --repository-name gatekeep-frontend --region sa-east-1 --query "imageDetails[?imageTags[?contains(@, 'latest')]].[imagePushedAt,imageTags,imageSizeInBytes]" --output table
```

**Nota Importante**: Ambas aplicaciones utilizan el tag `latest` en ECR. Para un mejor control de versiones en producción, se recomienda usar tags semánticos (ej: `v1.0.0`, `v1.0.1`) en lugar de `latest`.

### Task Definitions

#### Task Definition Backend (gatekeep-api:8)
- **Family**: `gatekeep-api`
- **Revisión**: 8 (ACTIVE)
- **Versión de Imagen**: `latest`
- **Imagen Completa**: `126588786097.dkr.ecr.sa-east-1.amazonaws.com/gatekeep-api:latest`
- **CPU**: 512 (0.5 vCPU)
- **Memoria**: 1024 MB (1 GB)
- **Network Mode**: `awsvpc`
- **Execution Role**: `gatekeep-ecs-execution-role`
- **Task Role**: `gatekeep-ecs-task-role`
- **Container**:
  - **Nombre**: `gatekeep-api`
  - **Imagen**: `126588786097.dkr.ecr.sa-east-1.amazonaws.com/gatekeep-api:latest`
  - **Puerto**: 5011
  - **Health Check**: `curl -f http://localhost:5011/health || exit 1`
- **Nota**: Esta es la revisión actualmente en producción. Para verificar la fecha exacta de la imagen, consultar ECR.

**Variables de Entorno**:
- `ASPNETCORE_ENVIRONMENT`: `dev`
- `ASPNETCORE_URLS`: `http://+:5011`
- `GATEKEEP_PORT`: `5011`
- `AWS_REGION`: `sa-east-1`
- `DATABASE__HOST`: Desde Parameter Store `/gatekeep/db/host`
- `DATABASE__PORT`: Desde Parameter Store `/gatekeep/db/port`
- `DATABASE__NAME`: Desde Parameter Store `/gatekeep/db/name`
- `DATABASE__USER`: Desde Parameter Store `/gatekeep/db/username`
- `MONGODB_DATABASE`: `GateKeepMongo`
- `MONGODB_USE_STABLE_API`: `true`
- `REDIS_CONNECTION`: Endpoint de ElastiCache Redis
- `REDIS_INSTANCE`: `GateKeep:`

**Secrets (desde Secrets Manager)**:
- `DATABASE__PASSWORD`: `arn:aws:secretsmanager:sa-east-1:126588786097:secret:gatekeep/db/password-14XBlu`
- `JWT__KEY`: `arn:aws:secretsmanager:sa-east-1:126588786097:secret:gatekeep/jwt/key-14XBlu`
- `MONGODB_CONNECTION`: `arn:aws:secretsmanager:sa-east-1:126588786097:secret:gatekeep/mongodb/connection-TJsSv0`

#### Task Definition Frontend (gatekeep-frontend:7)
- **Family**: `gatekeep-frontend`
- **Revisión**: 7 (ACTIVE)
- **Versión de Imagen**: `latest`
- **Imagen Completa**: `126588786097.dkr.ecr.sa-east-1.amazonaws.com/gatekeep-frontend:latest`
- **CPU**: 256 (0.25 vCPU)
- **Memoria**: 512 MB (0.5 GB)
- **Network Mode**: `awsvpc`
- **Execution Role**: `gatekeep-ecs-execution-role`
- **Task Role**: `gatekeep-ecs-task-role`
- **Container**:
  - **Nombre**: `gatekeep-frontend`
  - **Imagen**: `126588786097.dkr.ecr.sa-east-1.amazonaws.com/gatekeep-frontend:latest`
  - **Puerto**: 3000
- **Nota**: Esta es la revisión actualmente en producción. Para verificar la fecha exacta de la imagen, consultar ECR.

**Variables de Entorno**:
- `NODE_ENV`: `production`
- `PORT`: `3000`
- `NEXT_PUBLIC_API_URL`: `https://api.zimmzimmgames.com`

### CloudWatch Logs
- **Log Group**: `/ecs/gatekeep`
- **Retention**: 7 días
- **Stream Prefix**: `ecs` (backend), `ecs-frontend` (frontend)

### Comandos AWS CLI

```powershell
# Listar Clusters
aws ecs list-clusters --region sa-east-1

# Describir Cluster
aws ecs describe-clusters --clusters gatekeep-cluster --region sa-east-1

# Listar Servicios
aws ecs list-services --cluster gatekeep-cluster --region sa-east-1

# Describir Servicio
aws ecs describe-services --cluster gatekeep-cluster --services gatekeep-api-service --region sa-east-1

# Describir Task Definition
aws ecs describe-task-definition --task-definition gatekeep-api:8 --region sa-east-1

# Verificar versión actual del backend
aws ecs describe-services --cluster gatekeep-cluster --services gatekeep-api-service --region sa-east-1 --query "services[0].taskDefinition"

# Verificar versión actual del frontend
aws ecs describe-services --cluster gatekeep-cluster --services gatekeep-frontend-service --region sa-east-1 --query "services[0].taskDefinition"

# Ver imágenes disponibles en ECR (Backend)
aws ecr describe-images --repository-name gatekeep-api --region sa-east-1 --output table

# Ver imágenes disponibles en ECR (Frontend)
aws ecr describe-images --repository-name gatekeep-frontend --region sa-east-1 --output table

# Ver fecha exacta de la imagen latest (Backend)
aws ecr describe-images --repository-name gatekeep-api --region sa-east-1 --query "imageDetails[?imageTags[?contains(@, 'latest')]].[imagePushedAt,imageTags,imageSizeInBytes]" --output table

# Ver fecha exacta de la imagen latest (Frontend)
aws ecr describe-images --repository-name gatekeep-frontend --region sa-east-1 --query "imageDetails[?imageTags[?contains(@, 'latest')]].[imagePushedAt,imageTags,imageSizeInBytes]" --output table

# Listar Tasks
aws ecs list-tasks --cluster gatekeep-cluster --service-name gatekeep-api-service --region sa-east-1

# Describir Task
$taskArn = "arn:aws:ecs:sa-east-1:126588786097:task/gatekeep-cluster/..."
aws ecs describe-tasks --cluster gatekeep-cluster --tasks $taskArn --region sa-east-1

# Ver Logs de un Task
aws logs get-log-events --log-group-name "/ecs/gatekeep" --log-stream-name "ecs/gatekeep-api/..." --region sa-east-1

# Forzar nuevo despliegue
aws ecs update-service --cluster gatekeep-cluster --service gatekeep-api-service --force-new-deployment --region sa-east-1

# Actualizar Task Definition
aws ecs update-service --cluster gatekeep-cluster --service gatekeep-api-service --task-definition gatekeep-api:9 --region sa-east-1
```

---

## ECR (Elastic Container Registry)

### Repositorios

#### Repositorio API
- **Nombre**: `gatekeep-api`
- **URI**: `126588786097.dkr.ecr.sa-east-1.amazonaws.com/gatekeep-api`
- **ARN**: `arn:aws:ecr:sa-east-1:126588786097:repository/gatekeep-api`
- **Image Tag Mutability**: MUTABLE
- **Image Scanning**: Habilitado (scan on push)
- **Encryption**: AES256
- **Lifecycle Policy**: Mantener últimas 10 imágenes

#### Repositorio Frontend
- **Nombre**: `gatekeep-frontend`
- **URI**: `126588786097.dkr.ecr.sa-east-1.amazonaws.com/gatekeep-frontend`
- **ARN**: `arn:aws:ecr:sa-east-1:126588786097:repository/gatekeep-frontend`
- **Image Tag Mutability**: MUTABLE
- **Image Scanning**: Habilitado (scan on push)
- **Encryption**: AES256
- **Lifecycle Policy**: Mantener últimas 10 imágenes

### Comandos AWS CLI

```powershell
# Listar Repositorios
aws ecr describe-repositories --region sa-east-1

# Autenticar Docker con ECR
aws ecr get-login-password --region sa-east-1 | docker login --username AWS --password-stdin 126588786097.dkr.ecr.sa-east-1.amazonaws.com

# Listar Imágenes
aws ecr list-images --repository-name gatekeep-api --region sa-east-1

# Describir Imagen
aws ecr describe-images --repository-name gatekeep-api --image-ids imageTag=latest --region sa-east-1

# Subir Imagen
docker tag gatekeep-api:latest 126588786097.dkr.ecr.sa-east-1.amazonaws.com/gatekeep-api:latest
docker push 126588786097.dkr.ecr.sa-east-1.amazonaws.com/gatekeep-api:latest

# Eliminar Imagen
aws ecr batch-delete-image --repository-name gatekeep-api --image-ids imageTag=old-tag --region sa-east-1
```

---

## RDS (PostgreSQL)

### Instancia de Base de Datos
- **Identifier**: `gatekeep-db`
- **Engine**: PostgreSQL
- **Engine Version**: 16.11
- **Instance Class**: `db.t3.micro`
- **Endpoint**: `gatekeep-db.c7o0qk42qmwh.sa-east-1.rds.amazonaws.com`
- **Puerto**: `5432`
- **Estado**: `available`
- **Database Name**: `Gatekeep`
- **Username**: `postgres`
- **Password**: Almacenado en Secrets Manager (`gatekeep/db/password`)
- **Storage**: 
  - Allocated: 20 GB
  - Max Allocated: 100 GB
  - Type: gp3
  - Encrypted: Sí
- **Multi-AZ**: No (desactivado para reducir costos)
- **Publicly Accessible**: No
- **Backup**:
  - Retention: 7 días
  - Window: 03:00-04:00 UTC
- **Maintenance Window**: dom 04:00-05:00 UTC
- **Monitoring**: Habilitado (intervalo 60 segundos)
- **Deletion Protection**: No
- **Subnet Group**: `gatekeep-db-subnet-group` (subnets privadas)
- **Security Group**: `gatekeep-rds-sg`

### Parameter Group
- **Nombre**: `gatekeep-postgres16-params`
- **Family**: `postgres16`
- **Parámetros**:
  - `shared_preload_libraries`: `pg_stat_statements`
  - `log_statement`: `all`
  - `log_min_duration_statement`: `1000`

### Comandos AWS CLI

```powershell
# Describir Instancia RDS
aws rds describe-db-instances --db-instance-identifier gatekeep-db --region sa-east-1

# Listar DB Instances
aws rds describe-db-instances --region sa-east-1 --query "DBInstances[?contains(DBInstanceIdentifier, 'gatekeep')].[DBInstanceIdentifier,Endpoint.Address,Endpoint.Port,DBInstanceStatus]" --output table

# Obtener Password desde Secrets Manager
aws secretsmanager get-secret-value --secret-id gatekeep/db/password --region sa-east-1 --query SecretString --output text

# Conectar a la Base de Datos (desde un bastion o local con VPN)
# psql -h gatekeep-db.c7o0qk42qmwh.sa-east-1.rds.amazonaws.com -U postgres -d Gatekeep
```

---

## ElastiCache (Redis)

### Replication Group
- **Replication Group ID**: `gatekeep-redis`
- **Engine**: Redis
- **Engine Version**: 7.0
- **Node Type**: `cache.t3.micro` (0.5 vCPU, 0.6 GB RAM)
- **Puerto**: 6379
- **Estado**: `available`
- **Endpoint**: `gatekeep-redis.35dilc.ng.0001.sae1.cache.amazonaws.com`
- **Configuration Endpoint**: `gatekeep-redis.35dilc.ng.0001.sae1.cache.amazonaws.com`
- **Num Cache Clusters**: 1
- **Automatic Failover**: Deshabilitado
- **Multi-AZ**: Deshabilitado
- **Snapshot Retention**: 1 día
- **Snapshot Window**: 03:00-05:00 UTC
- **Maintenance Window**: dom 05:00-06:00 UTC
- **Subnet Group**: `gatekeep-redis-subnet-group` (subnets privadas)
- **Security Group**: `gatekeep-redis-sg`

### Parameter Group
- **Nombre**: `gatekeep-redis-params`
- **Family**: `redis7`
- **Parámetros**:
  - `maxmemory-policy`: `allkeys-lru`

### Comandos AWS CLI

```powershell
# Describir Replication Group
aws elasticache describe-replication-groups --replication-group-id gatekeep-redis --region sa-east-1

# Listar Replication Groups
aws elasticache describe-replication-groups --region sa-east-1 --query "ReplicationGroups[?contains(ReplicationGroupId, 'gatekeep')].[ReplicationGroupId,Status,ConfigurationEndpoint.Address]" --output table

# Obtener Endpoint
aws elasticache describe-replication-groups --replication-group-id gatekeep-redis --region sa-east-1 --query "ReplicationGroups[0].ConfigurationEndpoint.Address" --output text
```

---

## Secrets Manager

### Secretos Almacenados

#### Database Password
- **Nombre**: `gatekeep/db/password`
- **ARN**: `arn:aws:secretsmanager:sa-east-1:126588786097:secret:gatekeep/db/password-14XBlu`
- **Descripción**: Password para la base de datos RDS PostgreSQL
- **Recovery Window**: 7 días

#### JWT Key
- **Nombre**: `gatekeep/jwt/key`
- **ARN**: `arn:aws:secretsmanager:sa-east-1:126588786097:secret:gatekeep/jwt/key-14XBlu`
- **Descripción**: Clave secreta para JWT tokens
- **Recovery Window**: 7 días

#### MongoDB Connection
- **Nombre**: `gatekeep/mongodb/connection`
- **ARN**: `arn:aws:secretsmanager:sa-east-1:126588786097:secret:gatekeep/mongodb/connection-TJsSv0`
- **Descripción**: MongoDB Atlas connection string
- **Recovery Window**: 7 días

### Comandos AWS CLI

```powershell
# Listar Secrets
aws secretsmanager list-secrets --region sa-east-1 --query "SecretList[?contains(Name, 'gatekeep')].[Name,ARN,Description]" --output table

# Obtener Valor de un Secret
aws secretsmanager get-secret-value --secret-id gatekeep/db/password --region sa-east-1 --query SecretString --output text

# Crear Nuevo Secret
aws secretsmanager create-secret --name gatekeep/nuevo-secret --description "Descripción" --secret-string "valor" --region sa-east-1

# Actualizar Secret
aws secretsmanager put-secret-value --secret-id gatekeep/db/password --secret-string "nuevo-valor" --region sa-east-1

# Eliminar Secret (después del período de recuperación)
aws secretsmanager delete-secret --secret-id gatekeep/nuevo-secret --region sa-east-1
```

---

## Parameter Store (SSM)

### Parámetros Almacenados

#### Database Host
- **Nombre**: `/gatekeep/db/host`
- **Tipo**: String
- **Valor**: `gatekeep-db.c7o0qk42qmwh.sa-east-1.rds.amazonaws.com`

#### Database Port
- **Nombre**: `/gatekeep/db/port`
- **Tipo**: String
- **Valor**: `5432`

#### Database Name
- **Nombre**: `/gatekeep/db/name`
- **Tipo**: String
- **Valor**: `Gatekeep`

#### Database Username
- **Nombre**: `/gatekeep/db/username`
- **Tipo**: String
- **Valor**: `postgres`

#### ECR Repository URI
- **Nombre**: `/gatekeep/ecr/repository-uri`
- **Tipo**: String
- **Valor**: `126588786097.dkr.ecr.sa-east-1.amazonaws.com/gatekeep-api`

### Comandos AWS CLI

```powershell
# Listar Parámetros
aws ssm describe-parameters --region sa-east-1 --query "Parameters[?contains(Name, 'gatekeep')].[Name,Type,LastModifiedDate]" --output table

# Obtener Valor de un Parámetro
aws ssm get-parameter --name "/gatekeep/db/host" --region sa-east-1 --query Parameter.Value --output text

# Obtener Múltiples Parámetros
aws ssm get-parameters --names "/gatekeep/db/host" "/gatekeep/db/port" "/gatekeep/db/name" --region sa-east-1

# Obtener Parámetros por Path
aws ssm get-parameters-by-path --path "/gatekeep/" --region sa-east-1

# Crear Parámetro
aws ssm put-parameter --name "/gatekeep/nuevo-param" --value "valor" --type "String" --region sa-east-1

# Actualizar Parámetro
aws ssm put-parameter --name "/gatekeep/db/host" --value "nuevo-valor" --overwrite --region sa-east-1

# Eliminar Parámetro
aws ssm delete-parameter --name "/gatekeep/nuevo-param" --region sa-east-1
```

---

## Route53 (DNS)

### Hosted Zone
- **Nombre**: `zimmzimmgames.com.`
- **Zone ID**: `Z038254635T8HIPT0Z245`
- **Tipo**: Public

### Registros DNS

#### Registro Principal (Frontend)
- **Nombre**: `zimmzimmgames.com`
- **Tipo**: A (Alias)
- **Alias Target**: `gatekeep-alb-878011876.sa-east-1.elb.amazonaws.com`
- **Evaluate Target Health**: No

#### Registro API (Backend)
- **Nombre**: `api.zimmzimmgames.com`
- **Tipo**: A (Alias)
- **Alias Target**: `gatekeep-alb-878011876.sa-east-1.elb.amazonaws.com`
- **Evaluate Target Health**: No

#### Registros de Validación ACM
- Múltiples registros CNAME para validación de certificados SSL/TLS

### Comandos AWS CLI

```powershell
# Listar Hosted Zones
aws route53 list-hosted-zones --query "HostedZones[?contains(Name, 'zimmzimmgames')].[Name,Id]" --output table

# Listar Registros DNS
$zoneId = "Z038254635T8HIPT0Z245"
aws route53 list-resource-record-sets --hosted-zone-id $zoneId --query "ResourceRecordSets[?contains(Name, 'api') || contains(Name, 'zimmzimmgames')].[Name,Type,TTL,ResourceRecords[0].Value]" --output table

# Crear Registro DNS
aws route53 change-resource-record-sets --hosted-zone-id $zoneId --change-batch file://route53-change.json

# Obtener Registro Específico
aws route53 list-resource-record-sets --hosted-zone-id $zoneId --query "ResourceRecordSets[?Name=='api.zimmzimmgames.com.']"
```

---

## Security Groups

### Security Groups Configurados

#### ALB Security Group
- **Nombre**: `gatekeep-alb-sg`
- **Descripción**: Security group para Application Load Balancer
- **Reglas de Entrada**:
  - Puerto 80 (HTTP) desde `0.0.0.0/0`
  - Puerto 443 (HTTPS) desde `0.0.0.0/0`
- **Reglas de Salida**:
  - Todo el tráfico (`0.0.0.0/0`)

#### ECS Security Group
- **Nombre**: `gatekeep-ecs-sg`
- **Descripción**: Security group para ECS Fargate
- **Reglas de Entrada**:
  - Puerto 5011 (Backend) desde ALB Security Group
  - Puerto 3000 (Frontend) desde ALB Security Group
- **Reglas de Salida**:
  - Todo el tráfico (`0.0.0.0/0`)

#### RDS Security Group
- **Nombre**: `gatekeep-rds-sg`
- **Descripción**: Security group para RDS PostgreSQL
- **Reglas de Entrada**:
  - Puerto 5432 (PostgreSQL) desde VPC CIDR (`10.0.0.0/16`)
- **Reglas de Salida**:
  - Todo el tráfico (`0.0.0.0/0`)

#### Redis Security Group
- **Nombre**: `gatekeep-redis-sg`
- **Descripción**: Security group para ElastiCache Redis
- **Reglas de Entrada**:
  - Puerto 6379 (Redis) desde ECS Security Group
- **Reglas de Salida**:
  - Todo el tráfico (`0.0.0.0/0`)

#### App Runner Connector Security Group
- **Nombre**: `gatekeep-apprunner-connector-sg`
- **Descripción**: Security group para App Runner VPC Connector
- **Reglas de Salida**:
  - Todo el tráfico (`0.0.0.0/0`)

### Comandos AWS CLI

```powershell
# Listar Security Groups
aws ec2 describe-security-groups --filters "Name=tag:Name,Values=gatekeep-*" --region sa-east-1 --query "SecurityGroups[*].[GroupName,GroupId,Description]" --output table

# Describir Security Group
aws ec2 describe-security-groups --group-ids sg-xxxxx --region sa-east-1

# Ver Reglas de Entrada
aws ec2 describe-security-groups --group-ids sg-xxxxx --region sa-east-1 --query "SecurityGroups[0].IpPermissions"

# Ver Reglas de Salida
aws ec2 describe-security-groups --group-ids sg-xxxxx --region sa-east-1 --query "SecurityGroups[0].IpPermissionsEgress"
```

---

## IAM Roles y Políticas

### ECS Execution Role
- **Nombre**: `gatekeep-ecs-execution-role`
- **ARN**: `arn:aws:iam::126588786097:role/gatekeep-ecs-execution-role`
- **Propósito**: Permite a ECS ejecutar tareas y acceder a recursos AWS
- **Políticas Adjuntas**:
  - `AmazonECSTaskExecutionRolePolicy` (AWS Managed)
  - Policy personalizada para Secrets Manager y Parameter Store:
    - `secretsmanager:GetSecretValue` en secretos de GateKeep
    - `ssm:GetParameter` en parámetros `/gatekeep/*`

### ECS Task Role
- **Nombre**: `gatekeep-ecs-task-role`
- **ARN**: `arn:aws:iam::126588786097:role/gatekeep-ecs-task-role`
- **Propósito**: Permite a la aplicación dentro del contenedor acceder a servicios AWS
- **Políticas Adjuntas**:
  - Policy personalizada para CloudWatch Metrics:
    - `cloudwatch:PutMetricData` en namespaces `GateKeep/Redis` y `GateKeep/Redis/Logs`

### RDS Monitoring Role
- **Nombre**: `gatekeep-rds-monitoring-role`
- **ARN**: `arn:aws:iam::126588786097:role/gatekeep-rds-monitoring-role`
- **Propósito**: Permite a RDS enviar métricas a CloudWatch
- **Políticas Adjuntas**:
  - `AmazonRDSEnhancedMonitoringRole` (AWS Managed)

### Comandos AWS CLI

```powershell
# Obtener Información de un Role
aws iam get-role --role-name gatekeep-ecs-execution-role --region sa-east-1

# Listar Políticas Adjuntas a un Role
aws iam list-attached-role-policies --role-name gatekeep-ecs-execution-role --region sa-east-1

# Obtener Política Personalizada
aws iam get-role-policy --role-name gatekeep-ecs-execution-role --policy-name gatekeep-ecs-execution-secrets --region sa-east-1

# Listar Roles
aws iam list-roles --query "Roles[?contains(RoleName, 'gatekeep')].[RoleName,Arn]" --output table
```

---

## Comandos AWS CLI

### Configuración Inicial

```powershell
# Verificar Configuración de AWS CLI
aws configure list

# Verificar Identidad
aws sts get-caller-identity

# Verificar Región
aws configure get region

# Cambiar Región
aws configure set region sa-east-1
```

### Comandos Útiles de Diagnóstico

```powershell
# Ver Estado General de la Infraestructura
Write-Host "=== RESUMEN DE INFRAESTRUCTURA ===" -ForegroundColor Cyan

Write-Host "`n1. ECS Services:" -ForegroundColor Yellow
aws ecs describe-services --cluster gatekeep-cluster --services gatekeep-api-service gatekeep-frontend-service --region sa-east-1 --query "services[*].[serviceName,status,runningCount,desiredCount]" --output table

Write-Host "`n2. ALB Target Health:" -ForegroundColor Yellow
$tgArn = "arn:aws:elasticloadbalancing:sa-east-1:126588786097:targetgroup/gatekeep-tg/27437174e066b9ee"
aws elbv2 describe-target-health --target-group-arn $tgArn --region sa-east-1 --query "TargetHealthDescriptions[*].[Target.Id,TargetHealth.State,TargetHealth.Reason]" --output table

Write-Host "`n3. ECS Tasks:" -ForegroundColor Yellow
aws ecs list-tasks --cluster gatekeep-cluster --service-name gatekeep-api-service --region sa-east-1

Write-Host "`n4. CloudWatch Logs (últimas 50 líneas):" -ForegroundColor Yellow
$logStream = aws logs describe-log-streams --log-group-name "/ecs/gatekeep" --order-by LastEventTime --descending --max-items 1 --region sa-east-1 --query "logStreams[0].logStreamName" --output text
aws logs get-log-events --log-group-name "/ecs/gatekeep" --log-stream-name $logStream --limit 50 --region sa-east-1 --query "events[*].message" --output text
```

### Comandos de Despliegue

```powershell
# 1. Autenticar Docker con ECR
aws ecr get-login-password --region sa-east-1 | docker login --username AWS --password-stdin 126588786097.dkr.ecr.sa-east-1.amazonaws.com

# 2. Construir Imagen Docker
cd src
docker build -t gatekeep-api:latest -f GateKeep.Api/Dockerfile .

# 3. Etiquetar Imagen
docker tag gatekeep-api:latest 126588786097.dkr.ecr.sa-east-1.amazonaws.com/gatekeep-api:latest

# 4. Subir Imagen a ECR
docker push 126588786097.dkr.ecr.sa-east-1.amazonaws.com/gatekeep-api:latest

# 5. Forzar Nuevo Despliegue
aws ecs update-service --cluster gatekeep-cluster --service gatekeep-api-service --force-new-deployment --region sa-east-1

# 6. Verificar Estado del Despliegue
aws ecs describe-services --cluster gatekeep-cluster --services gatekeep-api-service --region sa-east-1 --query "services[0].deployments[*].[status,desiredCount,runningCount]" --output table
```

### Comandos de Monitoreo

```powershell
# Ver Métricas de CloudWatch
aws cloudwatch get-metric-statistics --namespace AWS/ECS --metric-name CPUUtilization --dimensions Name=ServiceName,Value=gatekeep-api-service Name=ClusterName,Value=gatekeep-cluster --start-time (Get-Date).AddHours(-1).ToString("yyyy-MM-ddTHH:mm:ss") --end-time (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss") --period 300 --statistics Average --region sa-east-1

# Ver Alarmas
aws cloudwatch describe-alarms --alarm-name-prefix gatekeep --region sa-east-1 --query "MetricAlarms[*].[AlarmName,StateValue,AlarmDescription]" --output table
```

---

## Endpoints y Rutas

### Endpoints Públicos

#### Frontend
- **URL**: `https://zimmzimmgames.com`
- **Protocolo**: HTTPS
- **Puerto**: 443
- **Target Group**: `gatekeep-frontend-tg`
- **Container Port**: 3000

#### Backend API
- **URL Base**: `https://api.zimmzimmgames.com`
- **Protocolo**: HTTPS
- **Puerto**: 443
- **Target Group**: `gatekeep-tg`
- **Container Port**: 5011

### Rutas del Backend

#### Autenticación
- `POST /api/auth/login` - Iniciar sesión
- `POST /api/auth/register` - Registro de usuario
- `GET /api/auth/validate-token` - Validar token JWT
- `POST /api/auth/generate-qr` - Generar código QR

#### Usuarios
- `GET /api/usuarios` - Listar usuarios
- `GET /api/usuarios/{id}` - Obtener usuario por ID
- `POST /api/usuarios` - Crear usuario
- `PUT /api/usuarios/{id}` - Actualizar usuario
- `DELETE /api/usuarios/{id}` - Eliminar usuario
- `PUT /api/usuarios/{id}/role` - Cambiar rol de usuario

#### Health Check
- `GET /health` - Health check endpoint

#### Swagger
- `GET /swagger` - Documentación de la API
- `GET /swagger/index.html` - Interfaz Swagger UI

### Flujo de Tráfico

1. **Cliente** → `https://api.zimmzimmgames.com/api/auth/login`
2. **Route53** → Resuelve a `gatekeep-alb-878011876.sa-east-1.elb.amazonaws.com`
3. **ALB (HTTPS Listener 443)** → Evalúa reglas de enrutamiento
4. **Regla 100** → Path `/api/*` coincide → Forward a `gatekeep-tg`
5. **Target Group** → Selecciona un target (ECS Task IP)
6. **ECS Task** → Container `gatekeep-api` en puerto 5011
7. **Aplicación** → Procesa la solicitud y retorna respuesta

---

## Notas Importantes

1. **RabbitMQ**: Completamente eliminado de la infraestructura. No hay referencias en:
   - Task Definitions
   - Secrets Manager
   - Security Groups
   - Terraform
   - Docker Compose

2. **Task Definition Revisión 8**: Esta es la revisión actual sin RabbitMQ. Contiene solo:
   - Secrets: Database Password, JWT Key, MongoDB Connection
   - Environment Variables: Database, MongoDB, Redis

3. **Health Checks**: El ALB verifica `/health` cada 30 segundos. Si falla 3 veces consecutivas, el target se marca como unhealthy.

4. **Despliegues**: Para desplegar una nueva versión:
   - Construir y subir imagen a ECR
   - Forzar nuevo despliegue del servicio ECS
   - El servicio automáticamente crea nuevas tasks y elimina las antiguas

5. **Logs**: Todos los logs de ECS se almacenan en CloudWatch Logs (`/ecs/gatekeep`) con retención de 7 días.

6. **Costos**: La infraestructura está optimizada para desarrollo:
   - RDS: `db.t3.micro` (no Multi-AZ)
   - Redis: `cache.t3.micro` (1 nodo, sin failover)
   - ECS: 1 task por servicio
   - ALB: Sin WAF (puede agregarse en producción)

---

## Cambios y Configuración del Load Balancer

### Resumen de Cambios Realizados

Esta sección documenta los cambios realizados en el Load Balancer para asegurar que funcione correctamente con la aplicación GateKeep.

#### Fecha de Cambios: 2025-11-23

### Problema Identificado

1. **Desajuste entre rutas del Load Balancer y la aplicación**:
   - El Load Balancer tenía reglas para `/auth/*` (sin prefijo `/api/`)
   - La aplicación solo expone endpoints bajo `/api/auth/*`
   - Esto causaba que las peticiones a `/auth/login` devolvieran 404

2. **Orden de prioridades incorrecto**:
   - Inicialmente `/auth/*` tenía prioridad 90 (mayor que `/api/auth/*`)
   - Esto causaba que las peticiones se enrutaran incorrectamente

### Cambios Implementados

#### 1. Reorganización de Prioridades

**Antes:**
- Priority 90: `/auth/*` → Backend
- Priority 100: `/api/auth/*` → Backend
- Priority 110: `/api/*` → Backend

**Después:**
- Priority 90: `/api/auth/*` → Backend (máxima prioridad)
- Priority 100: `/api/*` → Backend
- `/auth/*` → Eliminada

**Razón**: La aplicación solo tiene endpoints bajo `/api/auth/*`, por lo que `/api/auth/*` debe tener la máxima prioridad para que coincida antes que la regla genérica `/api/*`.

#### 2. Eliminación de Regla `/auth/*`

**Acción**: Se eliminó la regla `/auth/*` de ambos listeners (HTTP y HTTPS).

**Razón**: 
- La aplicación no tiene endpoints bajo `/auth/*` (sin prefijo `/api/`)
- Todos los endpoints de autenticación están bajo `/api/auth/*`
- Mantener la regla `/auth/*` causaba confusión y peticiones 404

**Comando utilizado**:
```powershell
# Eliminar regla /auth/* del listener HTTP
aws elbv2 delete-rule --rule-arn <rule-arn> --region sa-east-1

# Eliminar regla /auth/* del listener HTTPS
aws elbv2 delete-rule --rule-arn <rule-arn> --region sa-east-1
```

### Configuración Final del Load Balancer

#### Listener HTTP (Puerto 80)

| Prioridad | Condición | Acción | Target Group | Estado |
|-----------|-----------|--------|--------------|--------|
| 90 | Path: `/api/auth/*` | Forward | `gatekeep-tg` | ✅ Activa |
| 100 | Path: `/api/*` | Forward | `gatekeep-tg` | ✅ Activa |
| 120 | Path: `/usuarios/*` | Forward | `gatekeep-tg` | ✅ Activa |
| 130 | Path: `/swagger*` | Forward | `gatekeep-tg` | ✅ Activa |
| 140 | Path: `/health` | Forward | `gatekeep-tg` | ✅ Activa |
| Default | - | Redirect a HTTPS | - | ✅ Activa |

#### Listener HTTPS (Puerto 443)

| Prioridad | Condición | Acción | Target Group | Estado |
|-----------|-----------|--------|--------------|--------|
| 90 | Path: `/api/auth/*` | Forward | `gatekeep-tg` | ✅ Activa |
| 100 | Path: `/api/*` | Forward | `gatekeep-tg` | ✅ Activa |
| 120 | Path: `/usuarios/*` | Forward | `gatekeep-tg` | ✅ Activa |
| 130 | Path: `/swagger*` | Forward | `gatekeep-tg` | ✅ Activa |
| 140 | Path: `/health*` | Forward | `gatekeep-tg` | ✅ Activa |
| Default | - | Forward | `gatekeep-frontend-tg` | ✅ Activa |

### Endpoints que Funcionan Correctamente

Después de los cambios, los siguientes endpoints funcionan correctamente:

- ✅ `POST /api/auth/login` - Login de usuarios
- ✅ `POST /api/auth/register` - Registro de usuarios (requiere Admin)
- ✅ `GET /api/auth/qr` - Generar código QR del JWT
- ✅ `GET /api/auth/validate` - Validar token JWT
- ✅ `POST /api/auth/create-test-users` - Crear usuarios de prueba
- ✅ `GET /api/auth/list-users` - Listar usuarios (testing)

### Endpoints que NO Funcionan

Los siguientes endpoints no funcionan porque la aplicación no los expone:

- ❌ `POST /auth/login` - No existe (la aplicación solo tiene `/api/auth/login`)
- ❌ `POST /auth/register` - No existe (la aplicación solo tiene `/api/auth/register`)
- ❌ Cualquier ruta bajo `/auth/*` sin el prefijo `/api/`

### Verificación de Funcionamiento

#### Prueba de Login Exitoso

**Endpoint**: `POST https://api.zimmzimmgames.com/api/auth/login`

**Credenciales de prueba**:
- Email: `admin1@gatekeep.com`
- Password: `admin123`

**Resultado esperado**:
- Status: `200 OK`
- Respuesta: Token JWT válido y datos del usuario
- Logs: `HTTP POST /api/auth/login responded 200`

**Comando de prueba**:
```powershell
$body = @{
    email = "admin1@gatekeep.com"
    password = "admin123"
} | ConvertTo-Json

Invoke-WebRequest -Uri "https://api.zimmzimmgames.com/api/auth/login" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"
```

### Comandos Útiles para Verificar Configuración

#### Ver todas las reglas de un listener

```powershell
# HTTP (Puerto 80)
$httpListenerArn = "arn:aws:elasticloadbalancing:sa-east-1:126588786097:listener/app/gatekeep-alb/ff82ae699b9862d2/d15f140471f37a0d"
aws elbv2 describe-rules --listener-arn $httpListenerArn --region sa-east-1

# HTTPS (Puerto 443)
$httpsListenerArn = "arn:aws:elasticloadbalancing:sa-east-1:126588786097:listener/app/gatekeep-alb/ff82ae699b9862d2/d277ff2b7c7f46a8"
aws elbv2 describe-rules --listener-arn $httpsListenerArn --region sa-east-1
```

#### Verificar que no hay reglas `/auth/*` (sin `/api/`)

```powershell
$rules = aws elbv2 describe-rules --listener-arn $listenerArn --region sa-east-1 --output json | ConvertFrom-Json
$rules.Rules | Where-Object { 
    $_.Priority -ne "default" -and 
    $_.Conditions[0].Values[0] -eq "/auth/*" 
}
# No debe devolver ningún resultado
```

#### Ver estado de los targets

```powershell
$tgArn = "arn:aws:elasticloadbalancing:sa-east-1:126588786097:targetgroup/gatekeep-tg/27437174e066b9ee"
aws elbv2 describe-target-health --target-group-arn $tgArn --region sa-east-1
```

### Notas Importantes

1. **Coherencia con el código**: La configuración del Load Balancer ahora coincide exactamente con los endpoints definidos en el código de la aplicación (`AuthEndpoints.cs`).

2. **Prioridad de reglas**: El orden de prioridades es crítico. `/api/auth/*` debe tener prioridad 90 (mayor que `/api/*` con prioridad 100) para que las peticiones de autenticación se enruten correctamente.

3. **Eliminación de reglas innecesarias**: Eliminar la regla `/auth/*` evita confusión y asegura que solo funcionen los endpoints que realmente existen en la aplicación.

4. **Testing**: Siempre verificar que los endpoints funcionen después de hacer cambios en el Load Balancer usando las credenciales válidas.

### Próximos Pasos Recomendados

1. **Sincronizar con Terraform**: Si se realizan cambios manuales, considerar actualizar los archivos de Terraform para mantener la consistencia.

2. **Documentar cambios futuros**: Cualquier cambio en las reglas del Load Balancer debe documentarse en esta sección.

3. **Monitoreo**: Configurar alertas en CloudWatch para detectar errores 404 o problemas de enrutamiento.

---

## Troubleshooting

### Problema: 404 en `/api/auth/login`
**Solución**: Verificar:
1. Estado de ECS tasks: `aws ecs list-tasks --cluster gatekeep-cluster --service-name gatekeep-api-service`
2. Health de targets: `aws elbv2 describe-target-health --target-group-arn <tg-arn>`
3. Logs de ECS: `aws logs tail /ecs/gatekeep --follow`

### Problema: Tasks no inician
**Solución**: Verificar:
1. Secrets Manager: `aws secretsmanager get-secret-value --secret-id gatekeep/db/password`
2. IAM Permissions: `aws iam get-role-policy --role-name gatekeep-ecs-execution-role --policy-name gatekeep-ecs-execution-secrets`
3. Task Definition: `aws ecs describe-task-definition --task-definition gatekeep-api:8`

### Problema: No se puede conectar a RDS
**Solución**: Verificar:
1. Security Group de RDS permite tráfico desde ECS Security Group
2. RDS está en subnets privadas
3. ECS puede alcanzar las subnets privadas (NAT Gateway si es necesario)

---

**Última Actualización**: 2025-11-23
**Versión de Documentación**: 1.1
**Cambios en esta versión**: 
- Actualizada configuración de reglas del Load Balancer
- Eliminada regla `/auth/*` (la aplicación solo expone `/api/auth/*`)
- Reorganizadas prioridades: `/api/auth/*` ahora tiene prioridad 90 (máxima)
- Agregada sección "Cambios y Configuración del Load Balancer"

