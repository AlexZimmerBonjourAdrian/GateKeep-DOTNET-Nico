# Documentación Completa de AWS CLI en GateKeep

Este documento contiene toda la información sobre el uso de AWS CLI en el proyecto GateKeep, incluyendo todos los comandos, scripts y configuraciones actuales.

**Fecha de creación:** 2025-01-21  
**Región AWS:** sa-east-1 (São Paulo)  
**Account ID:** 126588786097

---

## Tabla de Contenidos

1. [Configuración Actual](#configuración-actual)
2. [Comandos AWS CLI por Servicio](#comandos-aws-cli-por-servicio)
3. [Scripts que Usan AWS CLI](#scripts-que-usan-aws-cli)
4. [Variables de Configuración](#variables-de-configuración)
5. [Comandos de Referencia Rápida](#comandos-de-referencia-rápida)
6. [Documentación Relacionada](#documentación-relacionada)

---

## Configuración Actual

### Región y Recursos Principales

- **Región:** `sa-east-1`
- **Cluster ECS:** `gatekeep-cluster`
- **Servicios ECS:**
  - `gatekeep-api-service` (Backend)
  - `gatekeep-frontend-service` (Frontend)
- **Repositorios ECR:**
  - `126588786097.dkr.ecr.sa-east-1.amazonaws.com/gatekeep-api`
  - `126588786097.dkr.ecr.sa-east-1.amazonaws.com/gatekeep-frontend`
- **Task Definitions:**
  - `gatekeep-api`
  - `gatekeep-frontend`

### Verificar Configuración

```powershell
# Verificar versión de AWS CLI
aws --version

# Ver configuración actual
aws configure list

# Verificar identidad
aws sts get-caller-identity --region sa-east-1
```

---

## Comandos AWS CLI por Servicio

### ECS (Elastic Container Service)

#### Verificar Estado de Servicios

```powershell
# Listar todos los servicios del cluster
aws ecs list-services --cluster gatekeep-cluster --region sa-east-1

# Ver estado del servicio API
aws ecs describe-services `
  --cluster gatekeep-cluster `
  --services gatekeep-api-service `
  --region sa-east-1 `
  --query 'services[0].{Status:status,DesiredCount:desiredCount,RunningCount:runningCount,PendingCount:pendingCount}' `
  --output table

# Ver estado del servicio Frontend
aws ecs describe-services `
  --cluster gatekeep-cluster `
  --services gatekeep-frontend-service `
  --region sa-east-1 `
  --query 'services[0].{Status:status,DesiredCount:desiredCount,RunningCount:runningCount,PendingCount:pendingCount}' `
  --output table

# Ver estado de ambos servicios
aws ecs describe-services `
  --cluster gatekeep-cluster `
  --services gatekeep-api-service gatekeep-frontend-service `
  --region sa-east-1 `
  --query 'services[*].{ServiceName:serviceName,Status:status,DesiredCount:desiredCount,RunningCount:runningCount,PendingCount:pendingCount}' `
  --output table
```

#### Actualizar Servicios

```powershell
# Forzar nuevo deployment del API
aws ecs update-service `
  --cluster gatekeep-cluster `
  --service gatekeep-api-service `
  --force-new-deployment `
  --region sa-east-1

# Forzar nuevo deployment del Frontend
aws ecs update-service `
  --cluster gatekeep-cluster `
  --service gatekeep-frontend-service `
  --force-new-deployment `
  --region sa-east-1

# Escalar servicio a 2 tareas
aws ecs update-service `
  --cluster gatekeep-cluster `
  --service gatekeep-api-service `
  --desired-count 2 `
  --region sa-east-1

# Detener servicio (desired-count = 0)
aws ecs update-service `
  --cluster gatekeep-cluster `
  --service gatekeep-api-service `
  --desired-count 0 `
  --region sa-east-1
```

#### Ver Tareas y Deployments

```powershell
# Listar tareas del servicio API
aws ecs list-tasks `
  --cluster gatekeep-cluster `
  --service-name gatekeep-api-service `
  --region sa-east-1

# Ver detalles de una tarea específica
$TASK_ARN = "arn:aws:ecs:sa-east-1:126588786097:task/gatekeep-cluster/TASK_ID"
aws ecs describe-tasks `
  --cluster gatekeep-cluster `
  --tasks $TASK_ARN `
  --region sa-east-1

# Ver deployments activos
aws ecs describe-services `
  --cluster gatekeep-cluster `
  --services gatekeep-api-service `
  --region sa-east-1 `
  --query 'services[0].deployments[*].{Status:status,DesiredCount:desiredCount,RunningCount:runningCount,Id:id,CreatedAt:createdAt}' `
  --output table
```

#### Task Definitions

```powershell
# Obtener Task Definition actual del API
aws ecs describe-task-definition `
  --task-definition gatekeep-api `
  --region sa-east-1 `
  --output json

# Obtener Task Definition actual del Frontend
aws ecs describe-task-definition `
  --task-definition gatekeep-frontend `
  --region sa-east-1 `
  --output json

# Registrar nueva Task Definition desde archivo JSON
aws ecs register-task-definition `
  --cli-input-json file://task-definition-new.json `
  --region sa-east-1

# Actualizar servicio con nueva Task Definition
aws ecs update-service `
  --cluster gatekeep-cluster `
  --service gatekeep-api-service `
  --task-definition gatekeep-api:4 `
  --region sa-east-1
```

### ECR (Elastic Container Registry)

#### Autenticación

```powershell
# Obtener token de login y autenticar Docker
aws ecr get-login-password --region sa-east-1 | `
  docker login --username AWS --password-stdin 126588786097.dkr.ecr.sa-east-1.amazonaws.com
```

#### Gestión de Repositorios

```powershell
# Listar todos los repositorios ECR
aws ecr describe-repositories --region sa-east-1

# Ver detalles de un repositorio específico
aws ecr describe-repositories `
  --repository-names gatekeep-api `
  --region sa-east-1

# Crear nuevo repositorio
aws ecr create-repository `
  --repository-name gatekeep-api `
  --region sa-east-1

# Listar imágenes en un repositorio
aws ecr list-images `
  --repository-name gatekeep-api `
  --region sa-east-1
```

### CloudWatch Logs

```powershell
# Ver logs del API (últimas líneas)
aws logs tail /ecs/gatekeep `
  --since 1h `
  --region sa-east-1 `
  --filter-pattern "ecs-api" `
  --format short

# Ver logs del Frontend
aws logs tail /ecs/gatekeep `
  --since 1h `
  --region sa-east-1 `
  --filter-pattern "ecs-frontend" `
  --format short

# Seguir logs en tiempo real
aws logs tail /ecs/gatekeep `
  --follow `
  --region sa-east-1 `
  --filter-pattern "ecs-api"
```

### S3

#### Gestión de Buckets

```powershell
# Listar buckets
aws s3 ls

# Verificar si un bucket existe
aws s3api head-bucket --bucket gatekeep-terraform-state --region sa-east-1

# Crear bucket
aws s3api create-bucket `
  --bucket gatekeep-terraform-state `
  --region sa-east-1 `
  --create-bucket-configuration LocationConstraint=sa-east-1

# Habilitar versionado
aws s3api put-bucket-versioning `
  --bucket gatekeep-terraform-state `
  --versioning-configuration Status=Enabled

# Bloquear acceso público
aws s3api put-public-access-block `
  --bucket gatekeep-terraform-state `
  --public-access-block-configuration `
  "BlockPublicAcls=true,IgnorePublicAcls=true,BlockPublicPolicy=true,RestrictPublicBuckets=true"
```

#### Subir Archivos a S3

```powershell
# Subir archivo individual
aws s3 cp archivo.txt s3://gatekeep-frontend-dev/archivo.txt

# Subir directorio completo (sync)
aws s3 sync ./build s3://gatekeep-frontend-dev `
  --delete `
  --cache-control "public, max-age=31536000, immutable"

# Subir con metadata específica
aws s3 cp index.html s3://gatekeep-frontend-dev/index.html `
  --content-type "text/html" `
  --cache-control "public, max-age=0, must-revalidate" `
  --metadata-directive REPLACE
```

### CloudFront

```powershell
# Crear invalidación de caché
aws cloudfront create-invalidation `
  --distribution-id E1234567890ABC `
  --paths "/*" `
  --query 'Invalidation.Id' `
  --output text

# Ver estado de invalidación
aws cloudfront get-invalidation `
  --distribution-id E1234567890ABC `
  --id I1234567890ABC

# Esperar a que la invalidación se complete
aws cloudfront wait invalidation-completed `
  --distribution-id E1234567890ABC `
  --id I1234567890ABC

# Listar políticas de respuesta
aws cloudfront list-response-headers-policies `
  --query "ResponseHeadersPoliciesList[?Name=='gatekeep-frontend-headers'].Id" `
  --output text

# Listar políticas de caché
aws cloudfront list-cache-policies `
  --query "CachePoliciesList[?CachePolicyConfig.Name=='gatekeep-frontend-static-cache'].Id" `
  --output text
```

### DynamoDB

```powershell
# Verificar si una tabla existe
aws dynamodb describe-table `
  --table-name gatekeep-terraform-locks `
  --region sa-east-1

# Crear tabla para locks de Terraform
aws dynamodb create-table `
  --table-name gatekeep-terraform-locks `
  --attribute-definitions AttributeName=LockID,AttributeType=S `
  --key-schema AttributeName=LockID,KeyType=HASH `
  --billing-mode PAY_PER_REQUEST `
  --region sa-east-1

# Esperar a que la tabla esté lista
aws dynamodb wait table-exists `
  --table-name gatekeep-terraform-locks `
  --region sa-east-1
```

### Secrets Manager

```powershell
# Listar todos los secrets
aws secretsmanager list-secrets --region sa-east-1

# Obtener valor de un secret
aws secretsmanager get-secret-value `
  --secret-id gatekeep/db/password `
  --region sa-east-1

# Crear nuevo secret
aws secretsmanager create-secret `
  --name gatekeep/jwt/key `
  --secret-string "tu-clave-secreta" `
  --region sa-east-1

# Verificar si un secret existe
aws secretsmanager describe-secret `
  --secret-id gatekeep/db/password `
  --region sa-east-1
```

### Systems Manager (Parameter Store)

```powershell
# Listar todos los parámetros
aws ssm describe-parameters --region sa-east-1

# Obtener valor de un parámetro
aws ssm get-parameter `
  --name /gatekeep/db/host `
  --region sa-east-1

# Crear o actualizar parámetro
aws ssm put-parameter `
  --name /gatekeep/db/host `
  --value "tu-host" `
  --type String `
  --overwrite `
  --region sa-east-1
```

### RDS

```powershell
# Listar instancias RDS
aws rds describe-db-instances --region sa-east-1

# Ver detalles de una instancia específica
aws rds describe-db-instances `
  --db-instance-identifier gatekeep-db `
  --region sa-east-1
```

### App Runner

```powershell
# Listar servicios App Runner
aws apprunner list-services --region sa-east-1

# Ver detalles de un servicio
aws apprunner describe-service `
  --service-arn "arn:aws:apprunner:sa-east-1:126588786097:service/..." `
  --region sa-east-1
```

### ELB (Elastic Load Balancer)

```powershell
# Ver estado de salud de targets
aws elbv2 describe-target-health `
  --target-group-arn arn:aws:elasticloadbalancing:sa-east-1:126588786097:targetgroup/gatekeep-frontend-tg/fe6df7144ffc7ee4 `
  --region sa-east-1 `
  --query 'TargetHealthDescriptions[*].{Target:Target.Id,Status:TargetHealth.State,Reason:TargetHealth.Reason}' `
  --output table
```

---

## Scripts que Usan AWS CLI

### Scripts PowerShell

#### 1. `update-aws.ps1`
**Propósito:** Actualizar servicios AWS con nuevo Docker (solo imágenes, no infraestructura)

**Comandos AWS CLI usados:**
- `aws ecr describe-repositories` - Obtener repositorios ECR
- `aws ecs list-clusters` - Listar clusters ECS
- `aws ecs list-services` - Listar servicios del cluster
- `aws ecs update-service` - Forzar nuevo deployment
- `aws ecs describe-services` - Verificar estado del servicio

**Uso:**
```powershell
.\update-aws.ps1
.\update-aws.ps1 -SkipFrontend
.\update-aws.ps1 -DeploymentTimeoutMinutes 20
```

#### 2. `deploy-aws-complete.ps1`
**Propósito:** Despliegue completo a AWS incluyendo migraciones y actualización de variables de entorno

**Comandos AWS CLI usados:**
- `aws ecs describe-services` - Obtener información del servicio
- `aws ecs describe-task-definition` - Obtener Task Definition actual
- `aws ecs register-task-definition` - Registrar nueva Task Definition
- `aws ecs update-service` - Actualizar servicio
- `aws ssm get-parameter` - Obtener parámetros de Parameter Store
- `aws ssm put-parameter` - Actualizar parámetros
- `aws secretsmanager describe-secret` - Verificar secrets

**Uso:**
```powershell
.\deploy-aws-complete.ps1
.\deploy-aws-complete.ps1 -SkipMigrations
.\deploy-aws-complete.ps1 -SkipFrontend -DryRun
```

#### 3. `start-aws.ps1`
**Propósito:** Iniciar servicios en AWS (verifica si están corriendo, si no, los levanta)

**Comandos AWS CLI usados:**
- `aws ecs update-service` - Actualizar servicios ECS
- `aws ecs describe-services` - Verificar estado

**Uso:**
```powershell
.\start-aws.ps1
.\start-aws.ps1 -SkipDockerRebuild
```

#### 4. `deploy-docker-aws.ps1`
**Propósito:** Script simple para construir y subir imagen Docker a ECR

**Comandos AWS CLI usados:**
- `aws sts get-caller-identity` - Verificar credenciales
- `aws ecr get-login-password` - Obtener token de ECR
- `aws ecs update-service` - Forzar deployment (opcional)

**Uso:**
```powershell
.\deploy-docker-aws.ps1
.\deploy-docker-aws.ps1 -ForceDeployment
.\deploy-docker-aws.ps1 -Region us-east-1 -EcrRepository "account.dkr.ecr.region.amazonaws.com/repo"
```

#### 5. `terraform/setup-remote-backend.ps1`
**Propósito:** Crear backend remoto de Terraform en S3 + DynamoDB

**Comandos AWS CLI usados:**
- `aws s3api head-bucket` - Verificar si bucket existe
- `aws s3api create-bucket` - Crear bucket S3
- `aws s3api put-bucket-versioning` - Habilitar versionado
- `aws s3api put-public-access-block` - Bloquear acceso público
- `aws dynamodb describe-table` - Verificar si tabla existe
- `aws dynamodb create-table` - Crear tabla DynamoDB
- `aws dynamodb wait table-exists` - Esperar a que tabla esté lista

**Uso:**
```powershell
cd terraform
.\setup-remote-backend.ps1
```

#### 6. `terraform/scripts/upload-frontend-to-s3.ps1`
**Propósito:** Subir build del frontend a S3 e invalidar CloudFront

**Comandos AWS CLI usados:**
- `aws s3 cp` - Subir archivos individuales a S3
- `aws s3 sync` - Sincronizar directorio con S3
- `aws cloudfront create-invalidation` - Crear invalidación
- `aws cloudfront get-invalidation` - Ver estado de invalidación

**Uso:**
```powershell
.\terraform\scripts\upload-frontend-to-s3.ps1 -BucketName "gatekeep-frontend-dev" -DistributionId "E1234567890ABC"
```

### Scripts Bash

#### 1. `terraform/scripts/upload-frontend-to-s3.sh`
**Propósito:** Subir build del frontend a S3 e invalidar CloudFront (versión Bash)

**Comandos AWS CLI usados:**
- `aws s3 sync` - Sincronizar directorio con S3
- `aws s3 cp` - Subir archivos específicos
- `aws cloudfront create-invalidation` - Crear invalidación
- `aws cloudfront wait invalidation-completed` - Esperar invalidación

**Uso:**
```bash
./terraform/scripts/upload-frontend-to-s3.sh gatekeep-frontend-dev E1234567890ABC
```

### Módulo PowerShell Común

#### `scripts/AwsDeploymentCommon.psm1`
**Propósito:** Funciones comunes para scripts de despliegue

**Funciones que usan AWS CLI:**
- `Get-AwsRegion` - Obtener región configurada (`aws configure get region`)
- `Connect-Ecr` - Autenticar con ECR (`aws ecr get-login-password`)
- `Get-EcsServiceStatus` - Obtener estado del servicio (`aws ecs describe-services`)
- `Get-AwsResourceInfo` - Obtener información de recursos (`aws ecr describe-repositories`, `aws ecs list-clusters`, `aws ecs list-services`)

---

## Variables de Configuración

### Variables de Entorno Recomendadas

```powershell
# Región de AWS
$env:AWS_REGION = "sa-east-1"

# Cluster ECS
$env:ECS_CLUSTER_NAME = "gatekeep-cluster"

# Servicios ECS
$env:ECS_SERVICE_API = "gatekeep-api-service"
$env:ECS_SERVICE_FRONTEND = "gatekeep-frontend-service"

# Repositorios ECR
$env:ECR_API_REPO = "126588786097.dkr.ecr.sa-east-1.amazonaws.com/gatekeep-api"
$env:ECR_FRONTEND_REPO = "126588786097.dkr.ecr.sa-east-1.amazonaws.com/gatekeep-frontend"

# Task Definitions
$env:TASK_DEF_API = "gatekeep-api"
$env:TASK_DEF_FRONTEND = "gatekeep-frontend"
```

### Configuración en Archivo .env

```bash
AWS_REGION=sa-east-1
AWS_ACCESS_KEY_ID=AKIA...
AWS_SECRET_ACCESS_KEY=...
ECS_CLUSTER_NAME=gatekeep-cluster
ECS_SERVICE_NAME_API=gatekeep-api-service
ECR_API_REPOSITORY=126588786097.dkr.ecr.sa-east-1.amazonaws.com/gatekeep-api
```

---

## Comandos de Referencia Rápida

### Backend (API)

```powershell
# Levantar
aws ecs update-service --cluster gatekeep-cluster --service gatekeep-api-service --desired-count 1 --force-new-deployment --region sa-east-1

# Detener
aws ecs update-service --cluster gatekeep-cluster --service gatekeep-api-service --desired-count 0 --region sa-east-1

# Estado
aws ecs describe-services --cluster gatekeep-cluster --services gatekeep-api-service --region sa-east-1 --query 'services[0].{Status:status,Running:runningCount,Desired:desiredCount}' --output table

# Logs
aws logs tail /ecs/gatekeep --follow --region sa-east-1 --filter-pattern "ecs-api"
```

### Frontend

```powershell
# Levantar
aws ecs update-service --cluster gatekeep-cluster --service gatekeep-frontend-service --desired-count 1 --force-new-deployment --region sa-east-1

# Detener
aws ecs update-service --cluster gatekeep-cluster --service gatekeep-frontend-service --desired-count 0 --region sa-east-1

# Estado
aws ecs describe-services --cluster gatekeep-cluster --services gatekeep-frontend-service --region sa-east-1 --query 'services[0].{Status:status,Running:runningCount,Desired:desiredCount}' --output table

# Logs
aws logs tail /ecs/gatekeep --follow --region sa-east-1 --filter-pattern "ecs-frontend"
```

### ECR

```powershell
# Login
aws ecr get-login-password --region sa-east-1 | docker login --username AWS --password-stdin 126588786097.dkr.ecr.sa-east-1.amazonaws.com

# Listar repositorios
aws ecr describe-repositories --region sa-east-1

# Listar imágenes
aws ecr list-images --repository-name gatekeep-api --region sa-east-1
```

### Verificación General

```powershell
# Verificar identidad
aws sts get-caller-identity --region sa-east-1

# Ver configuración
aws configure list

# Ver todos los servicios del cluster
aws ecs list-services --cluster gatekeep-cluster --region sa-east-1
```

---

## Documentación Relacionada

### Documentos en el Proyecto

1. **`docs/AWS_CLI_DEPLOYMENT.md`**
   - Guía completa de despliegue con AWS CLI
   - Comandos para levantar/detener servicios
   - Monitoreo y logs

2. **`docs/AWS_SETUP.md`**
   - Instalación y configuración de AWS CLI
   - Configuración de credenciales
   - Troubleshooting

3. **`docs/AWS_USAGE.md`**
   - Uso de AWS desde CLI y aplicación .NET
   - Ejemplos de código
   - Configuración de permisos IAM

4. **`docs/PLAN_REPARACION_AWS.md`**
   - Plan de reparación de problemas
   - Scripts de corrección
   - Verificaciones post-reparación

5. **`docs/TERRAFORM_AWS_SETUP.md`**
   - Configuración de Terraform con AWS
   - Conexión de credenciales

### Recursos Externos

- [Documentación oficial AWS CLI](https://docs.aws.amazon.com/cli/latest/userguide/)
- [Referencia de comandos ECS](https://docs.aws.amazon.com/cli/latest/reference/ecs/)
- [Referencia de comandos ECR](https://docs.aws.amazon.com/cli/latest/reference/ecr/)
- [Referencia de comandos CloudWatch Logs](https://docs.aws.amazon.com/cli/latest/reference/logs/)
- [Referencia de comandos S3](https://docs.aws.amazon.com/cli/latest/reference/s3/)

---

## Notas Importantes

### Región
- **Todas las operaciones usan la región `sa-east-1`** (São Paulo)
- Asegúrate de especificar `--region sa-east-1` en todos los comandos si no está configurada por defecto

### Permisos IAM
El usuario IAM necesita los siguientes permisos mínimos:
- `ecs:*` - Gestión de servicios ECS
- `ecr:*` - Gestión de repositorios ECR
- `logs:*` - Acceso a CloudWatch Logs
- `s3:*` - Gestión de buckets S3
- `cloudfront:*` - Gestión de distribuciones CloudFront
- `secretsmanager:*` - Acceso a Secrets Manager
- `ssm:*` - Acceso a Parameter Store
- `dynamodb:*` - Gestión de tablas DynamoDB (para Terraform backend)

### Costos
- Mantener servicios con `desired-count > 0` genera costos
- Usa `desired-count 0` para detener servicios y ahorrar costos
- Los deployments pueden tardar 5-15 minutos

### Health Checks
- Los servicios tienen health checks configurados
- Espera a que los health checks pasen antes de considerar el servicio listo
- Monitorea los logs si un servicio no inicia correctamente

---

**Última actualización:** 2025-01-21  
**Mantenido por:** Equipo de desarrollo GateKeep

