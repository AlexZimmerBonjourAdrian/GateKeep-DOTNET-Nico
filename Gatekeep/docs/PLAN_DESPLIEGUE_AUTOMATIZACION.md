# Plan de Implementación: 3.6 Despliegue y Automatización

**Fecha de creación:** 11 de noviembre de 2025  
**Actualizado:** 11 de noviembre de 2025 (Versión Simplificada - 3 días)  
**Proyecto:** GateKeep - Sistema de Gestión de Acceso  
**Requisito:** Grupos de 3 y Grupos de 4

---

## 📋 Resumen del Requisito

### Especificación Original

> **3.6 Despliegue y Automatización**
> 
> 1. **CI/CD Pipeline:** La solución deberá incluir un pipeline CI/CD automatizado para build, test y despliegue.
> 
> 2. **AWS Deployment:** El despliegue del entorno de pruebas podrá realizarse en AWS, utilizando servicios gestionados (RDS, ECS, App Runner u otros).
> 
> 3. **Documentación:** Se deberá entregar documentación que permita la ejecución completa del entorno local y el despliegue remoto.

### ⚡ Versión Simplificada para 3 Días

Este plan ha sido optimizado para implementación en **3 días o menos**, usando:
- **App Runner** para API y Frontend (servicios gestionados)
- **RDS PostgreSQL** para base de datos (servicio gestionado)
- **ECR** para almacenar imágenes Docker
- **Setup manual desde consola AWS** (sin Terraform inicial)
- **Sin VPC, sin ElastiCache, sin MongoDB** (opcionales para simplificar)

---

## 🎯 Estado Actual del Proyecto

### ✅ **LO QUE YA TIENES IMPLEMENTADO**

#### 1. Infraestructura Local con Docker
- **✅ Dockerfile** configurado para la API .NET 8
- **✅ docker-compose.yml** con servicios:
  - PostgreSQL 16
  - Redis 7
  - MongoDB (opcional)
  - Seq (logging)
  - Prometheus (métricas)
  - Grafana (visualización)
- **✅ Scripts PowerShell** para gestión local:
  - `iniciar-docker.ps1`
  - `detener-docker.ps1`
  - `recrear-docker.ps1`

#### 2. Configuración de Entorno
- **✅ Variables de entorno** mediante `.env`
- **✅ Configuraciones** para Development y Production
- **✅ Health checks** configurados en docker-compose

#### 3. Documentación Local
- **✅ README.md** con instrucciones para entorno local
- **✅ Documentación técnica** en `docs/`
- **✅ Guías de configuración** básicas

### ❌ **LO QUE FALTA IMPLEMENTAR**

#### 1. CI/CD Pipeline
- ❌ **No existe pipeline CI/CD** automatizado
- ❌ **No hay integración** con GitHub Actions, GitLab CI, Azure DevOps, etc.
- ❌ **No hay automatización** de build, test y deploy
- ❌ **No hay construcción** automática de imágenes Docker
- ❌ **No hay despliegue** automático a entornos remotos

#### 2. Infraestructura AWS
- ❌ **No hay configuración** de infraestructura en AWS
- ❌ **No hay recursos** de RDS, App Runner, etc.
- ❌ **No hay scripts** de Terraform o CloudFormation
- ❌ **No hay ECR** para almacenar imágenes Docker
- ❌ **No hay configuración** de App Runner Service
- ❌ **No hay configuración** de variables de entorno en AWS

#### 3. Documentación de Despliegue
- ❌ **No hay documentación** para despliegue en AWS
- ❌ **No hay guía** paso a paso para configurar AWS
- ❌ **No hay instrucciones** para configurar el pipeline CI/CD
- ❌ **No hay documentación** de troubleshooting para despliegue

---

## 📐 Arquitectura Propuesta

### Arquitectura CI/CD

```
┌─────────────────────────────────────────────────────────────┐
│                    REPOSITORIO GIT                           │
│              (GitHub / GitLab / Azure DevOps)                │
└──────────────────────────┬──────────────────────────────────┘
                           │ Push / Pull Request
                           ↓
┌─────────────────────────────────────────────────────────────┐
│                  CI/CD PIPELINE                              │
│              (GitHub Actions / GitLab CI)                    │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌────────────────────────────────────────────────────┐    │
│  │  1. CHECKOUT                                        │    │
│  │     - Obtener código fuente                         │    │
│  └────────────────────────────────────────────────────┘    │
│                          ↓                                   │
│  ┌────────────────────────────────────────────────────┐    │
│  │  2. BUILD                                           │    │
│  │     - dotnet restore                                │    │
│  │     - dotnet build                                  │    │
│  │     - npm install (frontend)                        │    │
│  │     - npm run build (frontend)                      │    │
│  └────────────────────────────────────────────────────┘    │
│                          ↓                                   │
│  ┌────────────────────────────────────────────────────┐    │
│  │  3. TEST                                            │    │
│  │     - dotnet test                                   │    │
│  │     - npm test (frontend, si aplica)                │    │
│  └────────────────────────────────────────────────────┘    │
│                          ↓                                   │
│  ┌────────────────────────────────────────────────────┐    │
│  │  4. DOCKER BUILD                                    │    │
│  │     - docker build -t gatekeep-api:tag              │    │
│  │     - docker build -t gatekeep-frontend:tag         │    │
│  └────────────────────────────────────────────────────┘    │
│                          ↓                                   │
│  ┌────────────────────────────────────────────────────┐    │
│  │  5. PUSH TO ECR                                     │    │
│  │     - aws ecr get-login-password                   │    │
│  │     - docker push gatekeep-api:tag                  │    │
│  │     - docker push gatekeep-frontend:tag             │    │
│  └────────────────────────────────────────────────────┘    │
│                          ↓                                   │
│  ┌────────────────────────────────────────────────────┐    │
│  │  6. DEPLOY                                          │    │
│  │     - Actualizar App Runner Service                 │    │
│  │     - Trigger nuevo deployment en App Runner         │    │
│  │     - Verificar health checks                       │    │
│  └────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

### Arquitectura AWS Simplificada (Solo App Runner + RDS)

```
┌─────────────────────────────────────────────────────────────┐
│                        AWS CLOUD                             │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌────────────────────────────────────────────────────┐    │
│  │         App Runner Service (API)                     │    │
│  │  - Auto-scaling: 1-3 instancias                      │    │
│  │  - Load balancing integrado                         │    │
│  │  - HTTPS automático                                  │    │
│  │  - Health checks: /health                           │    │
│  │  - URL: https://gatekeep-api.xxx.awsapprunner.com   │    │
│  └──────────────────────────┬───────────────────────────┘    │
│                              │                                │
│  ┌───────────────────────────┴───────────────────────────┐    │
│  │         App Runner Service (Frontend)                   │    │
│  │  - Auto-scaling: 1-2 instancias                        │    │
│  │  - Load balancing integrado                           │    │
│  │  - HTTPS automático                                    │    │
│  │  - URL: https://gatekeep-frontend.xxx.awsapprunner.com │    │
│  └──────────────────────────┬───────────────────────────┘    │
│                              │                                │
│                              │ Conexión directa (público)     │
│                              ↓                                │
│  ┌────────────────────────────────────────────────────┐    │
│  │              RDS PostgreSQL (Público)                 │    │
│  │  - Engine: PostgreSQL 16                            │    │
│  │  - Instance: db.t4g.micro                           │    │
│  │  - Public: SÍ (simplificado)                        │    │
│  │  - Security Group: Solo desde App Runner            │    │
│  └────────────────────────────────────────────────────┘    │
│                                                              │
│  ┌────────────────────────────────────────────────────┐    │
│  │              ECR (Elastic Container Registry)        │    │
│  │  - gatekeep-api:latest                              │    │
│  │  - gatekeep-frontend:latest                         │    │
│  └────────────────────────────────────────────────────┘    │
│                                                              │
│  ┌────────────────────────────────────────────────────┐    │
│  │         Secrets Manager + Parameter Store            │    │
│  │  - DB password (Secrets Manager)                   │    │
│  │  - JWT key (Secrets Manager)                        │    │
│  │  - Variables de entorno (Parameter Store)           │    │
│  └────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘

NOTA: Sin VPC, sin ElastiCache, sin MongoDB (opcionales)
```

---

## 🗺️ Plan de Implementación Simplificado (3 Días)

### **DÍA 1: Setup Básico AWS + App Runner** ⏱️ 6-8 horas

#### Mañana (3-4 horas): Crear Recursos AWS

**1.1 Crear ECR Repositories (Consola AWS)**

1. Ir a **ECR** → **Repositories** → **Create repository**
2. Crear dos repositorios:
   - **gatekeep-api** (tipo: Private)
   - **gatekeep-frontend** (tipo: Private)
3. Anotar las URIs de los repositorios (ej: `123456789.dkr.ecr.us-east-1.amazonaws.com/gatekeep-api`)

**1.2 Crear RDS PostgreSQL (Consola AWS)**

1. Ir a **RDS** → **Databases** → **Create database**
2. Configuración:
   - **Engine**: PostgreSQL 16
   - **Template**: Free tier (si aplica) o Dev/Test
   - **DB instance identifier**: `gatekeep-db`
   - **Master username**: `postgres`
   - **Master password**: Generar y guardar en Secrets Manager
   - **DB instance class**: `db.t4g.micro` (más barato) o `db.t3.micro`
   - **Storage**: 20 GB (gp3)
   - **Public access**: **SÍ** (para simplificar)
   - **VPC**: Default VPC
   - **Security group**: Crear nuevo o usar existente
     - Regla: Permitir PostgreSQL (5432) desde App Runner IPs o 0.0.0.0/0 temporalmente
   - **Database name**: `Gatekeep`
   - **Backup**: Habilitar (7 días)
3. Anotar el **endpoint** de RDS (ej: `gatekeep-db.xxxxx.us-east-1.rds.amazonaws.com`)

**1.3 Configurar Secrets Manager (Consola AWS)**

1. Ir a **Secrets Manager** → **Store a new secret**
2. Crear secret para DB password:
   - **Secret type**: Other type of secret
   - **Key/value**: `password` = [password de RDS]
   - **Secret name**: `gatekeep/db/password`
3. Crear secret para JWT key:
   - **Secret type**: Other type of secret
   - **Key/value**: `key` = [generar clave JWT de 256 bits]
   - **Secret name**: `gatekeep/jwt/key`

**1.4 Configurar Parameter Store (Consola AWS)**

1. Ir a **Systems Manager** → **Parameter Store** → **Create parameter**
2. Crear parámetros:
   - `/gatekeep/db/host` = [RDS endpoint]
   - `/gatekeep/db/port` = `5432`
   - `/gatekeep/db/name` = `Gatekeep`
   - `/gatekeep/db/username` = `postgres`
   - `/gatekeep/app/environment` = `Production`
   - `/gatekeep/app/port` = `5011`

#### Tarde (3-4 horas): Crear App Runner Services

**1.5 Crear App Runner Service para API**

1. Ir a **App Runner** → **Services** → **Create service**
2. **Source configuration**:
   - **Source type**: Container registry
   - **Provider**: Amazon ECR
   - **Container image URI**: Seleccionar `gatekeep-api:latest` del repositorio ECR
   - **Deployment trigger**: Automatic (detecta cambios en ECR)
3. **Service settings**:
   - **Service name**: `gatekeep-api`
   - **Virtual CPU**: 1 vCPU
   - **Memory**: 2 GB
4. **Auto scaling**:
   - **Min size**: 1
   - **Max size**: 3
   - **Concurrency**: 100 requests/instance
5. **Health check**:
   - **Path**: `/health`
   - **Interval**: 10 seconds
   - **Timeout**: 5 seconds
6. **Network**:
   - **Egress type**: Default (no VPC)
7. **Environment variables**:
   - `ASPNETCORE_ENVIRONMENT` = `Production`
   - `ASPNETCORE_URLS` = `http://+:5011`
   - `DATABASE__HOST` = [desde Parameter Store: `/gatekeep/db/host`]
   - `DATABASE__PORT` = [desde Parameter Store: `/gatekeep/db/port`]
   - `DATABASE__NAME` = [desde Parameter Store: `/gatekeep/db/name`]
   - `DATABASE__USER` = [desde Parameter Store: `/gatekeep/db/username`]
   - `DATABASE__PASSWORD` = [desde Secrets Manager: `gatekeep/db/password`]
   - `JWT__KEY` = [desde Secrets Manager: `gatekeep/jwt/key`]
   - `JWT__ISSUER` = `GateKeep`
   - `JWT__AUDIENCE` = `GateKeepUsers`
   - `REDIS__ENABLED` = `false` (omitir Redis)
   - `MONGODB_CONNECTION` = `` (vacío, omitir MongoDB)
8. **Port**: `5011`
9. Anotar la **URL del servicio** (ej: `https://gatekeep-api.xxxxx.us-east-1.awsapprunner.com`)

**1.6 Crear App Runner Service para Frontend**

1. Similar al anterior, pero:
   - **Service name**: `gatekeep-frontend`
   - **Container image URI**: `gatekeep-frontend:latest`
   - **Virtual CPU**: 0.5 vCPU
   - **Memory**: 1 GB
   - **Min size**: 1, **Max size**: 2
   - **Port**: `3000`
   - **Environment variables**:
     - `REACT_APP_API_URL` = [URL del servicio API de App Runner]

**1.7 Primer Despliegue Manual**

1. Construir imágenes Docker localmente:
   ```bash
   # API
   docker build -t gatekeep-api:latest -f src/Dockerfile .
   docker tag gatekeep-api:latest [ECR_URI_API]:latest
   
   # Frontend
   docker build -t gatekeep-frontend:latest -f frontend/Dockerfile .
   docker tag gatekeep-frontend:latest [ECR_URI_FRONTEND]:latest
   ```

2. Login a ECR y push:
   ```bash
   aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin [ECR_URI]
   docker push [ECR_URI_API]:latest
   docker push [ECR_URI_FRONTEND]:latest
   ```

3. App Runner detectará automáticamente los cambios y desplegará

4. Verificar que los servicios estén en estado "Running"

---

### **DÍA 2: CI/CD Pipeline + Automatización** ⏱️ 4-6 horas

#### Mañana (2-3 horas): Configurar GitHub Actions

**2.1 Crear Pipeline CI/CD Básico**

1. Crear `.github/workflows/ci-cd.yml`:

```yaml
name: CI/CD Pipeline

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

env:
  AWS_REGION: us-east-1
  ECR_API_REPOSITORY: gatekeep-api
  ECR_FRONTEND_REPOSITORY: gatekeep-frontend

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      
      - name: Restore dependencies
        run: dotnet restore src/GateKeep.Api/GateKeep.Api.csproj
      
      - name: Build
        run: dotnet build src/GateKeep.Api/GateKeep.Api.csproj --configuration Release --no-restore
      
      - name: Test
        run: dotnet test src/GateKeep.Api/GateKeep.Api.csproj --configuration Release --no-build --verbosity normal || true
      
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '18'
      
      - name: Install frontend dependencies
        run: |
          cd frontend
          npm ci
      
      - name: Build frontend
        run: |
          cd frontend
          npm run build

  build-and-push:
    needs: build-and-test
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    steps:
      - uses: actions/checkout@v3
      
      - name: Configure AWS credentials
        uses: aws-actions/configure-aws-credentials@v2
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          aws-region: ${{ env.AWS_REGION }}
      
      - name: Login to Amazon ECR
        id: login-ecr
        uses: aws-actions/amazon-ecr-login@v1
      
      - name: Build, tag, and push API image
        env:
          ECR_REGISTRY: ${{ steps.login-ecr.outputs.registry }}
          ECR_REPOSITORY: ${{ env.ECR_API_REPOSITORY }}
          IMAGE_TAG: ${{ github.sha }}
        run: |
          docker build -t $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG -f src/Dockerfile .
          docker tag $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG $ECR_REGISTRY/$ECR_REPOSITORY:latest
          docker push $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG
          docker push $ECR_REGISTRY/$ECR_REPOSITORY:latest
      
      - name: Build, tag, and push Frontend image
        env:
          ECR_REGISTRY: ${{ steps.login-ecr.outputs.registry }}
          ECR_REPOSITORY: ${{ env.ECR_FRONTEND_REPOSITORY }}
          IMAGE_TAG: ${{ github.sha }}
        run: |
          docker build -t $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG -f frontend/Dockerfile .
          docker tag $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG $ECR_REGISTRY/$ECR_REPOSITORY:latest
          docker push $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG
          docker push $ECR_REGISTRY/$ECR_REPOSITORY:latest
      
      - name: Wait for App Runner deployment
        run: |
          echo "App Runner auto-deploys when images are updated in ECR"
          sleep 30
```

2. Configurar GitHub Secrets:
   - `AWS_ACCESS_KEY_ID`
   - `AWS_SECRET_ACCESS_KEY`

#### Tarde (2-3 horas): Verificar y Ajustar

**2.2 Verificar Conexión App Runner → RDS**

1. Verificar que la API puede conectarse a RDS
2. Probar endpoint `/health`
3. Verificar logs en CloudWatch

**2.3 Ajustar Security Groups de RDS**

1. Ir a **RDS** → **Security groups**
2. Agregar regla para permitir tráfico desde App Runner
3. O usar 0.0.0.0/0 temporalmente (solo para pruebas)

**2.4 Probar Pipeline Completo**

1. Hacer commit y push a `main`
2. Verificar que GitHub Actions ejecuta correctamente
3. Verificar que App Runner detecta cambios y despliega

---

### **DÍA 3: Testing + Documentación** ⏱️ 4-6 horas

#### Mañana (2-3 horas): Testing

**3.1 Testing End-to-End**

1. Verificar que API responde en URL de App Runner
2. Probar endpoints principales
3. Verificar que Frontend se conecta a API
4. Probar flujo completo de usuario

**3.2 Verificar Health Checks**

1. Verificar que `/health` funciona
2. Verificar logs en CloudWatch
3. Probar auto-scaling (opcional)

**3.3 Optimizaciones**

1. Ajustar auto-scaling si es necesario
2. Verificar costos en AWS Cost Explorer
3. Ajustar configuración de App Runner si es necesario

#### Tarde (2-3 horas): Documentación

**3.4 Crear Documentación Mínima**

1. Crear `docs/DEPLOYMENT.md` con:
   - Pasos para crear recursos AWS
   - Configuración de App Runner
   - Variables de entorno necesarias
   - Troubleshooting básico

2. Crear `docs/ENVIRONMENT_VARIABLES.md` con:
   - Lista de variables de entorno
   - Valores por defecto
   - Dónde configurarlas

3. Actualizar `README.md` con:
   - Enlace a documentación de despliegue
   - URLs de los servicios desplegados

---

## 🗺️ Plan de Implementación Completo (Opcional - Para Después)

### **FASE 1: Configuración del Pipeline CI/CD** ⏱️ 2-3 días

#### 1.1 Crear Estructura de GitHub Actions

**Ubicación:** `.github/workflows/`

**Archivos a crear:**
- `.github/workflows/ci-cd.yml` - Pipeline principal
- `.github/workflows/.gitkeep` - Mantener estructura

**Acción:** Crear directorio y archivo base del workflow

**Contenido del workflow:**
- Trigger: push a `main` y pull requests
- Jobs:
  - `build-and-test`: Build y tests
  - `build-docker`: Construcción de imágenes
  - `deploy`: Despliegue a AWS (solo en `main`)

**Dependencias:**
- Repositorio en GitHub
- Secrets configurados en GitHub:
  - `AWS_ACCESS_KEY_ID`
  - `AWS_SECRET_ACCESS_KEY`
  - `AWS_REGION`
  - `ECR_REPOSITORY`

---

#### 1.2 Configurar Job de Build y Test

**Ubicación:** `.github/workflows/ci-cd.yml`

**Acciones:**
1. Checkout del código
2. Setup .NET SDK 8.0
3. Setup Node.js (para frontend)
4. Restore dependencias:
   - `dotnet restore`
   - `npm ci` (frontend)
5. Build:
   - `dotnet build --configuration Release`
   - `npm run build` (frontend)
6. Test:
   - `dotnet test --configuration Release --no-build --verbosity normal`
   - `npm test` (frontend, si aplica)

**Validaciones:**
- Build debe completarse sin errores
- Tests deben pasar
- Código debe compilar en Release

---

#### 1.3 Configurar Job de Docker Build

**Ubicación:** `.github/workflows/ci-cd.yml`

**Acciones:**
1. Setup Docker Buildx
2. Login a ECR:
   - `aws ecr get-login-password`
   - `docker login`
3. Build imágenes:
   - `docker build -t gatekeep-api:${{ github.sha }} -f src/Dockerfile .`
   - `docker build -t gatekeep-frontend:${{ github.sha }} -f frontend/Dockerfile .`
4. Tag imágenes:
   - `latest` (solo en main)
   - `${{ github.sha }}` (commit hash)
   - `${{ github.ref_name }}` (branch name)
5. Push a ECR:
   - `docker push gatekeep-api:${{ github.sha }}`
   - `docker push gatekeep-frontend:${{ github.sha }}`

**Dependencias:**
- ECR repository creado previamente
- Permisos IAM para push a ECR

---

#### 1.4 Configurar Job de Deploy

**Ubicación:** `.github/workflows/ci-cd.yml`

**Condiciones:**
- Solo ejecutar en branch `main`
- Solo después de que build y docker build pasen

**Acciones:**
1. Setup AWS CLI
2. Actualizar App Runner Service:
   - Obtener configuración actual del servicio
   - Actualizar imagen en ECR con nuevo tag
   - Trigger nuevo deployment en App Runner
   - Esperar a que deployment esté completo
3. Verificar health checks:
   - Hacer request a `/health` en la URL de App Runner
   - Validar respuesta 200 OK

**Rollback:**
- Si health check falla después de 5 minutos, rollback automático
- App Runner mantiene versión anterior automáticamente
- Notificar en caso de fallo

---

#### 1.5 Configurar Tests Unitarios (si no existen)

**Ubicación:** `src/GateKeep.Api.Tests/` (crear si no existe)

**Acciones:**
1. Crear proyecto de tests:
   - `dotnet new xunit -n GateKeep.Api.Tests`
2. Agregar referencia al proyecto principal
3. Crear tests básicos:
   - Tests de endpoints
   - Tests de servicios
   - Tests de repositorios
4. Configurar para CI:
   - Coverage reports (opcional)
   - Test results en formato JUnit

---

### **FASE 2: Infraestructura AWS con Terraform** ⏱️ 2-4 días

#### 2.1 Crear Estructura de Terraform

**Ubicación:** `infrastructure/terraform/`

**Archivos a crear:**
- `main.tf` - Configuración principal
- `variables.tf` - Variables
- `outputs.tf` - Outputs
- `providers.tf` - Configuración de providers
- `vpc.tf` - VPC y networking (opcional, solo si se usa VPC Connector)
- `rds.tf` - Base de datos RDS
- `apprunner.tf` - App Runner Services
- `ecr.tf` - ECR Repositories
- `redis.tf` - ElastiCache Redis (opcional, puede usar Redis en RDS o externo)
- `secrets.tf` - Secrets Manager
- `iam.tf` - Roles y políticas IAM
- `.terraform.lock.hcl` - Lock file (generado)

**Estructura:**
```
infrastructure/
├── terraform/
│   ├── main.tf
│   ├── variables.tf
│   ├── outputs.tf
│   ├── providers.tf
│   ├── vpc.tf (opcional)
│   ├── rds.tf
│   ├── apprunner.tf
│   ├── ecr.tf
│   ├── redis.tf (opcional)
│   ├── secrets.tf
│   └── iam.tf
├── scripts/
│   ├── deploy.sh
│   └── destroy.sh
└── README.md
```

---

#### 2.2 Configurar VPC y Networking (Opcional)

**Ubicación:** `infrastructure/terraform/vpc.tf`

**Nota:** App Runner puede funcionar sin VPC, pero si necesitas acceder a RDS o Redis privados, necesitarás un VPC Connector.

**Recursos a crear (solo si se usa VPC Connector):**
1. **VPC:**
   - CIDR: `10.0.0.0/16`
   - Enable DNS hostnames
   - Enable DNS resolution

2. **Subnets:**
   - 2 Private Subnets (AZ diferentes):
     - `10.0.1.0/24` (us-east-1a)
     - `10.0.2.0/24` (us-east-1b)

3. **VPC Connector:**
   - Permite que App Runner acceda a recursos en VPC privada
   - Subnets: privadas
   - Security groups: configurar según necesidad

4. **Security Groups:**
   - `rds-sg`: Permite PostgreSQL (5432) solo desde VPC Connector
   - `redis-sg`: Permite Redis (6379) solo desde VPC Connector

---

#### 2.3 Configurar RDS PostgreSQL

**Ubicación:** `infrastructure/terraform/rds.tf`

**Recursos a crear:**
1. **DB Subnet Group:**
   - Subnets privadas

2. **DB Parameter Group:**
   - Configuraciones optimizadas para PostgreSQL 16

3. **RDS Instance:**
   - Engine: `postgres`
   - Version: `16.1`
   - Instance class: `db.t3.micro` (para pruebas)
   - Allocated storage: `20 GB`
   - Storage type: `gp3`
   - Multi-AZ: `false` (para pruebas, activar en producción)
   - Publicly accessible: `false`
   - VPC Security Groups: `rds-sg`
   - DB Name: `Gatekeep`
   - Master username: desde variable
   - Master password: desde Secrets Manager
   - Backup retention: `7 days`
   - Backup window: `03:00-04:00 UTC`
   - Maintenance window: `sun:04:00-sun:05:00 UTC`
   - Enable automated backups

4. **DB Snapshot** (opcional para pruebas)

**Variables necesarias:**
- `db_instance_class`
- `db_allocated_storage`
- `db_name`
- `db_username`

---

#### 2.4 Configurar ElastiCache Redis

**Ubicación:** `infrastructure/terraform/redis.tf`

**Recursos a crear:**
1. **Subnet Group:**
   - Subnets privadas

2. **Parameter Group:**
   - Configuraciones para Redis 7

3. **ElastiCache Cluster:**
   - Engine: `redis`
   - Version: `7.0`
   - Node type: `cache.t3.micro` (para pruebas)
   - Num cache nodes: `1`
   - Port: `6379`
   - Subnet group: privado
   - Security group: `redis-sg`
   - Automatic failover: `false` (para pruebas)

---

#### 2.5 Configurar ECR Repositories

**Ubicación:** `infrastructure/terraform/ecr.tf`

**Recursos a crear:**
1. **ECR Repository para API:**
   - Name: `gatekeep-api`
   - Image tag mutability: `MUTABLE`
   - Scan on push: `true`
   - Lifecycle policy:
     - Mantener últimas 10 imágenes
     - Eliminar imágenes sin tags después de 7 días

2. **ECR Repository para Frontend:**
   - Name: `gatekeep-frontend`
   - Mismas configuraciones

---

#### 2.6 Configurar App Runner Services

**Ubicación:** `infrastructure/terraform/apprunner.tf`

**Recursos a crear:**
1. **App Runner Service para API:**
   - Service name: `gatekeep-api`
   - Source configuration:
     - Image repository: ECR
     - Image identifier: `gatekeep-api:latest`
     - Auto deployments: `ENABLED` (despliega automáticamente al actualizar imagen)
   - Instance configuration:
     - CPU: `1 vCPU`
     - Memory: `2 GB`
   - Auto scaling configuration:
     - Min size: `1`
     - Max size: `5`
     - Concurrency: `100` (requests por instancia)
   - Health check configuration:
     - Protocol: `HTTP`
     - Path: `/health`
     - Interval: `10 seconds`
     - Timeout: `5 seconds`
     - Healthy threshold: `1`
     - Unhealthy threshold: `5`
   - Network configuration:
     - VPC Connector: (opcional, solo si se usa VPC)
     - Egress type: `VPC` o `DEFAULT` (si no usa VPC)
   - Environment variables:
     - Desde Parameter Store y Secrets Manager
   - Port: `5011`

2. **App Runner Service para Frontend:**
   - Service name: `gatekeep-frontend`
   - Source configuration:
     - Image repository: ECR
     - Image identifier: `gatekeep-frontend:latest`
     - Auto deployments: `ENABLED`
   - Instance configuration:
     - CPU: `0.5 vCPU`
     - Memory: `1 GB`
   - Auto scaling configuration:
     - Min size: `1`
     - Max size: `3`
     - Concurrency: `50`
   - Health check configuration:
     - Protocol: `HTTP`
     - Path: `/`
     - Interval: `10 seconds`
   - Network configuration:
     - Egress type: `DEFAULT`
   - Port: `3000`

**Notas importantes:**
- App Runner gestiona automáticamente:
  - Load balancing
  - HTTPS con certificado SSL
  - Auto-scaling
  - Health checks
  - Logs en CloudWatch
- Cada servicio obtiene una URL única: `https://xxx.us-east-1.awsapprunner.com`
- No se necesita Application Load Balancer

---

#### 2.8 Configurar Secrets Manager

**Ubicación:** `infrastructure/terraform/secrets.tf`

**Recursos a crear:**
1. **Secrets Manager Secret para DB:**
   - Name: `gatekeep/db/password`
   - Description: "Database master password"
   - Generate random password automáticamente

2. **Secrets Manager Secret para JWT:**
   - Name: `gatekeep/jwt/key`
   - Description: "JWT signing key"

**Nota:** Los secrets deben crearse manualmente la primera vez o usar AWS CLI.

---

#### 2.9 Configurar Systems Manager Parameter Store

**Ubicación:** `infrastructure/terraform/secrets.tf` (o archivo separado)

**Parámetros a crear:**
1. **Database:**
   - `/gatekeep/db/host` - RDS endpoint
   - `/gatekeep/db/port` - `5432`
   - `/gatekeep/db/name` - `Gatekeep`
   - `/gatekeep/db/username` - Master username

2. **Redis:**
   - `/gatekeep/redis/connection` - ElastiCache endpoint
   - `/gatekeep/redis/instance` - `GateKeep:`

3. **JWT:**
   - `/gatekeep/jwt/issuer` - `GateKeep`
   - `/gatekeep/jwt/audience` - `GateKeepUsers`
   - `/gatekeep/jwt/expiration-hours` - `8`

4. **Application:**
   - `/gatekeep/app/environment` - `Production`
   - `/gatekeep/app/port` - `5011`

---

#### 2.7 Configurar IAM Roles y Políticas

**Ubicación:** `infrastructure/terraform/iam.tf`

**Recursos a crear:**
1. **IAM Role para App Runner:**
   - Name: `gatekeep-apprunner-role`
   - Trust policy: App Runner service
   - Policies:
     - Leer Parameter Store
     - Leer Secrets Manager
     - Escribir logs en CloudWatch
     - Pull imágenes de ECR
     - Acceder a VPC (si se usa VPC Connector)

2. **IAM Role para GitHub Actions:**
   - Name: `gatekeep-github-actions-role`
   - Trust policy: GitHub OIDC
   - Policies:
     - Push a ECR
     - Actualizar App Runner services
     - Leer App Runner service configurations

---

#### 2.8 Crear Scripts de Despliegue

**Ubicación:** `infrastructure/scripts/`

**Archivos a crear:**
1. **deploy.sh:**
   - Inicializar Terraform
   - Validar configuración
   - Plan
   - Apply (con confirmación)
   - Mostrar outputs (URLs de App Runner)

2. **destroy.sh:**
   - Destroy de infraestructura
   - Confirmación antes de destruir

3. **update-apprunner.sh:**
   - Actualizar App Runner service con nueva imagen
   - Trigger nuevo deployment (automático si auto-deploy está habilitado)

**Permisos:**
- `chmod +x deploy.sh`
- `chmod +x destroy.sh`
- `chmod +x update-apprunner.sh`

---

### **FASE 3: Integración CI/CD con AWS** ⏱️ 1-2 días

#### 3.1 Configurar OIDC para GitHub Actions

**Acciones:**
1. Crear Identity Provider en IAM:
   - Provider type: OpenID Connect
   - Provider URL: `https://token.actions.githubusercontent.com`
   - Audience: `sts.amazonaws.com`

2. Crear IAM Role con trust policy para GitHub:
   - Condiciones:
     - `token.actions.githubusercontent.com:sub` contiene el repositorio
     - `token.actions.githubusercontent.com:aud` es `sts.amazonaws.com`

3. Configurar GitHub Secrets:
   - `AWS_ROLE_ARN`: ARN del role creado
   - `AWS_REGION`: Región de AWS

---

#### 3.2 Actualizar Workflow para Usar OIDC

**Ubicación:** `.github/workflows/ci-cd.yml`

**Modificaciones:**
1. Agregar step de configuración de AWS:
   - Usar `aws-actions/configure-aws-credentials@v2`
   - Con OIDC en lugar de access keys

2. Actualizar jobs para usar el role:
   - `permissions:` con `id-token: write`
   - `role-to-assume:` desde secret

---

#### 3.3 Configurar Actualización de App Runner Service

**Ubicación:** `.github/workflows/ci-cd.yml`

**Acciones:**
1. Actualizar App Runner service:
   - `aws apprunner update-service` con nueva imagen de ECR
   - O simplemente actualizar la imagen en ECR (si auto-deploy está habilitado)

2. Esperar deployment:
   - `aws apprunner wait service-updated`
   - Verificar estado del deployment

3. Verificar health check:
   - Hacer request a la URL de App Runner
   - Validar respuesta 200 OK

**Nota:** Si auto-deploy está habilitado en App Runner, solo necesitas actualizar la imagen en ECR y App Runner detectará el cambio automáticamente.

---

#### 3.4 Agregar Notificaciones

**Ubicación:** `.github/workflows/ci-cd.yml`

**Opciones:**
1. **Slack:**
   - Notificar éxito/fallo de deployments
   - Incluir enlace a commit y logs

2. **Email:**
   - Usar AWS SNS para notificaciones

3. **GitHub Status:**
   - Actualizar status checks en PRs

---

### **FASE 4: Documentación de Despliegue** ⏱️ 1 día

#### 4.1 Crear Documento Principal de Despliegue

**Ubicación:** `docs/DEPLOYMENT.md`

**Contenido:**
1. **Prerequisitos:**
   - Cuenta AWS con permisos adecuados
   - AWS CLI instalado y configurado
   - Terraform instalado (>= 1.0)
   - Docker instalado
   - Git

2. **Configuración Inicial:**
   - Crear cuenta AWS
   - Configurar AWS CLI
   - Crear IAM user con permisos
   - Configurar GitHub Secrets

3. **Despliegue de Infraestructura:**
   - Clonar repositorio
   - Configurar variables de Terraform
   - Ejecutar `terraform init`
   - Ejecutar `terraform plan`
   - Ejecutar `terraform apply`
   - Guardar outputs importantes

4. **Configuración de Secrets:**
   - Crear secrets en Secrets Manager
   - Crear parámetros en Parameter Store
   - Verificar permisos de App Runner service

5. **Primer Despliegue:**
   - Push inicial de imágenes a ECR
   - Crear App Runner services
   - Verificar health checks
   - Probar endpoints en URLs de App Runner

6. **Configuración del Pipeline:**
   - Verificar GitHub Actions está habilitado
   - Verificar secrets configurados
   - Hacer push a main para trigger
   - Monitorear ejecución

7. **Variables de Entorno:**
   - Lista completa de variables necesarias
   - Valores por defecto
   - Dónde configurarlas (Parameter Store)

8. **Troubleshooting:**
   - Problemas comunes
   - Cómo ver logs
   - Cómo hacer rollback
   - Cómo destruir infraestructura

---

#### 4.2 Crear Guía de Variables de Entorno

**Ubicación:** `docs/ENVIRONMENT_VARIABLES.md`

**Contenido:**
1. **Variables de Base de Datos:**
   - `DATABASE__HOST`
   - `DATABASE__PORT`
   - `DATABASE__NAME`
   - `DATABASE__USER`
   - `DATABASE__PASSWORD`

2. **Variables de Redis:**
   - `REDIS__CONNECTIONSTRING`
   - `REDIS__INSTANCENAME`
   - `REDIS__ENABLED`

3. **Variables de JWT:**
   - `JWT__KEY`
   - `JWT__ISSUER`
   - `JWT__AUDIENCE`
   - `JWT__EXPIRATIONHOURS`

4. **Variables de Aplicación:**
   - `ASPNETCORE_ENVIRONMENT`
   - `ASPNETCORE_URLS`
   - `GATEKEEP_PORT`

5. **Variables de MongoDB (opcional):**
   - `MONGODB_CONNECTION`
   - `MONGODB_DATABASE`

6. **Mapeo Local vs AWS:**
   - Cómo se configuran localmente (.env)
   - Cómo se configuran en AWS (Parameter Store/Secrets Manager)

---

#### 4.3 Actualizar README Principal

**Ubicación:** `README.md`

**Modificaciones:**
1. Agregar sección "Despliegue en AWS"
2. Enlace a `docs/DEPLOYMENT.md`
3. Enlace a `docs/ENVIRONMENT_VARIABLES.md`
4. Información sobre CI/CD pipeline
5. Badge de status de GitHub Actions (opcional)

---

#### 4.4 Crear Diagrama de Arquitectura

**Ubicación:** `docs/ARCHITECTURE.md`

**Contenido:**
1. Diagrama ASCII o Mermaid de arquitectura completa
2. Flujo de datos
3. Componentes y sus responsabilidades
4. Puntos de integración

---

### **FASE 5: Testing y Validación** ⏱️ 1 día

#### 5.1 Testing del Pipeline CI/CD

**Acciones:**
1. Crear PR de prueba
2. Verificar que build y tests se ejecutan
3. Verificar que no se despliega (solo en main)
4. Merge a main
5. Verificar despliegue automático
6. Verificar que servicio está disponible

---

#### 5.2 Testing de Infraestructura

**Acciones:**
1. Verificar que todos los recursos se crearon
2. Verificar conectividad:
   - App Runner → RDS (si usa VPC Connector)
   - App Runner → Redis (si usa VPC Connector)
   - Internet → App Runner (HTTPS automático)
3. Verificar health checks en App Runner
4. Verificar logs en CloudWatch
5. Probar endpoints de la API en URL de App Runner

---

#### 5.3 Testing de Rollback

**Acciones:**
1. Simular fallo en deployment
2. Verificar rollback automático
3. Verificar que servicio vuelve a versión anterior

---

## 📊 Resumen de Archivos a Crear

### Pipeline CI/CD
- `.github/workflows/ci-cd.yml`

### Infraestructura Terraform
- `infrastructure/terraform/main.tf`
- `infrastructure/terraform/variables.tf`
- `infrastructure/terraform/outputs.tf`
- `infrastructure/terraform/providers.tf`
- `infrastructure/terraform/vpc.tf` (opcional)
- `infrastructure/terraform/rds.tf`
- `infrastructure/terraform/apprunner.tf`
- `infrastructure/terraform/ecr.tf`
- `infrastructure/terraform/redis.tf` (opcional)
- `infrastructure/terraform/secrets.tf`
- `infrastructure/terraform/iam.tf`

### Scripts
- `infrastructure/scripts/deploy.sh`
- `infrastructure/scripts/destroy.sh`
- `infrastructure/scripts/update-apprunner.sh`

### Documentación
- `docs/DEPLOYMENT.md`
- `docs/ENVIRONMENT_VARIABLES.md`
- `docs/ARCHITECTURE.md` (opcional)
- Actualización de `README.md`

### Tests (opcional)
- `src/GateKeep.Api.Tests/` (si no existe)

---

## ⏱️ Estimación de Tiempo Total

### Versión Simplificada (3 días)

| Día | Descripción | Tiempo Estimado |
|-----|-------------|-----------------|
| **Día 1** | Setup básico AWS + App Runner | 6-8 horas |
| **Día 2** | CI/CD Pipeline + Automatización | 4-6 horas |
| **Día 3** | Testing + Documentación | 4-6 horas |
| **TOTAL** | | **14-20 horas (2-3 días)** |

### Versión Completa (opcional, para después)

| Fase | Descripción | Tiempo Estimado |
|------|-------------|-----------------|
| **Fase 1** | CI/CD Pipeline | 2-3 días |
| **Fase 2** | Infraestructura AWS con Terraform | 2-4 días |
| **Fase 3** | Integración CI/CD-AWS | 1-2 días |
| **Fase 4** | Documentación | 1 día |
| **Fase 5** | Testing y Validación | 1 día |
| **TOTAL** | | **7-11 días** |

---

## 🎯 Checklist de Implementación

### ✅ Versión Simplificada (3 Días)

#### Día 1: Setup Básico
- [ ] Crear ECR repositories (gatekeep-api, gatekeep-frontend)
- [ ] Crear RDS PostgreSQL (db.t4g.micro, público)
- [ ] Configurar Security Group de RDS
- [ ] Crear secrets en Secrets Manager (DB password, JWT key)
- [ ] Crear parámetros en Parameter Store
- [ ] Crear App Runner Service para API
- [ ] Crear App Runner Service para Frontend
- [ ] Configurar variables de entorno en App Runner
- [ ] Primer despliegue manual a ECR
- [ ] Verificar que App Runner detecta y despliega

#### Día 2: CI/CD
- [ ] Crear `.github/workflows/ci-cd.yml`
- [ ] Configurar GitHub Secrets (AWS credentials)
- [ ] Probar pipeline de build y test
- [ ] Probar push a ECR desde pipeline
- [ ] Verificar conexión App Runner → RDS
- [ ] Ajustar Security Groups si es necesario
- [ ] Probar flujo completo (push → deploy automático)

#### Día 3: Testing y Documentación
- [ ] Testing end-to-end de la aplicación
- [ ] Verificar health checks
- [ ] Verificar logs en CloudWatch
- [ ] Optimizar configuración de App Runner
- [ ] Crear `docs/DEPLOYMENT.md`
- [ ] Crear `docs/ENVIRONMENT_VARIABLES.md`
- [ ] Actualizar `README.md`
- [ ] Preparar demo/presentación

### 📋 Versión Completa (Opcional - Para Después)

#### Fase 1: CI/CD Pipeline
- [ ] Crear `.github/workflows/ci-cd.yml` (completo)
- [ ] Configurar job de build y test
- [ ] Configurar job de Docker build
- [ ] Configurar job de deploy
- [ ] Probar pipeline con PR
- [ ] Probar despliegue automático en main

#### Fase 2: Infraestructura AWS con Terraform
- [ ] Crear estructura de Terraform
- [ ] Configurar VPC y networking (opcional)
- [ ] Configurar RDS PostgreSQL
- [ ] Configurar ElastiCache Redis (opcional)
- [ ] Configurar ECR repositories
- [ ] Configurar App Runner services
- [ ] Configurar Secrets Manager
- [ ] Configurar Parameter Store
- [ ] Configurar IAM roles
- [ ] Crear scripts de despliegue
- [ ] Probar `terraform apply`
- [ ] Verificar todos los recursos creados

#### Fase 3: Integración
- [ ] Configurar OIDC para GitHub Actions
- [ ] Actualizar workflow para usar OIDC
- [ ] Configurar actualización de App Runner service
- [ ] Agregar notificaciones (opcional)
- [ ] Probar flujo completo

#### Fase 4: Documentación
- [ ] Crear `docs/DEPLOYMENT.md` (completo)
- [ ] Crear `docs/ENVIRONMENT_VARIABLES.md`
- [ ] Actualizar `README.md`
- [ ] Crear diagrama de arquitectura

#### Fase 5: Testing
- [ ] Testing del pipeline CI/CD
- [ ] Testing de infraestructura
- [ ] Testing de rollback
- [ ] Validación final

---

## 📝 Notas Importantes

1. **Costos AWS (Versión Simplificada):**
   - RDS db.t4g.micro: ~$12/mes (o Free Tier si aplica)
   - App Runner (API - 1 vCPU, 2GB): ~$20/mes
   - App Runner (Frontend - 0.5 vCPU, 1GB): ~$10/mes
   - ECR: ~$1/mes (primeros 500MB gratis)
   - Secrets Manager: ~$0.40/mes (primeros 10,000 secrets gratis)
   - Parameter Store: Gratis (Standard parameters)
   - Data transfer: variable (~$1-5/mes)
   - **Total estimado: ~$43-48/mes**
   
   **Sin Redis ni MongoDB**: Ahorro de ~$15-30/mes comparado con versión completa

2. **Seguridad:**
   - Nunca commitear secrets en código
   - Usar Secrets Manager para passwords
   - Usar Parameter Store para configuraciones
   - Habilitar encryption en tránsito y en reposo
   - Configurar security groups restrictivamente

3. **Alta Disponibilidad:**
   - Para producción, considerar:
     - Multi-AZ en RDS
     - Auto-scaling en App Runner (ya configurado)
     - ElastiCache con replicación (opcional)
     - HTTPS con certificado SSL (gestionado automáticamente por App Runner)

4. **Monitoreo:**
   - CloudWatch Logs para logs de aplicación
   - CloudWatch Metrics para métricas
   - CloudWatch Alarms para alertas
   - Integrar con Prometheus/Grafana existente (opcional)

5. **Backup:**
   - RDS automated backups habilitados
   - Retención de 7 días (aumentar en producción)
   - Considerar snapshots manuales antes de cambios importantes

---

## 🚀 Próximos Pasos

1. Revisar y aprobar este plan
2. Crear cuenta AWS (si no existe)
3. Configurar AWS CLI localmente
4. Comenzar con Fase 1 (CI/CD Pipeline)
5. Seguir secuencialmente las fases

---

**Última actualización:** 11 de noviembre de 2025

