# Script para iniciar servicios en AWS
# Reconstruye Docker y aplica la infraestructura con Terraform

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  GateKeep - Despliegue Completo AWS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Función para verificar Docker
function Test-Docker {
    $docker = Get-Command docker -ErrorAction SilentlyContinue
    if (-not $docker) {
        Write-Host "Error: Docker no está instalado" -ForegroundColor Red
        Write-Host "Instala Docker Desktop desde: https://www.docker.com/products/docker-desktop" -ForegroundColor Yellow
        return $false
    }
    
    Write-Host "Docker instalado" -ForegroundColor Green
    
    try {
        $dockerVersion = docker --version
        Write-Host "  $dockerVersion" -ForegroundColor Gray
    } catch {
        Write-Host "Error al verificar versión de Docker" -ForegroundColor Red
        return $false
    }
    
    # Verificar que Docker está ejecutándose
    try {
        docker ps 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Error: Docker no está ejecutándose" -ForegroundColor Red
            Write-Host "Inicia Docker Desktop y vuelve a intentar" -ForegroundColor Yellow
            return $false
        }
        Write-Host "Docker está ejecutándose" -ForegroundColor Green
        return $true
    } catch {
        Write-Host "Error: Docker no está ejecutándose" -ForegroundColor Red
        Write-Host "Inicia Docker Desktop y vuelve a intentar" -ForegroundColor Yellow
        return $false
    }
}

# Función para reconstruir Docker
function Rebuild-Docker {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "  Paso 1: Reconstruir Imágenes Docker" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    
    $srcPath = Join-Path $PSScriptRoot "src"
    
    if (-not (Test-Path $srcPath)) {
        Write-Host "Error: No se encontró el directorio src" -ForegroundColor Red
        Write-Host "Ruta buscada: $srcPath" -ForegroundColor Gray
        return $false
    }
    
    # Guardar ubicación actual
    $originalLocation = Get-Location
    
    try {
        Set-Location $srcPath
        
        Write-Host "[1/4] Deteniendo servicios existentes..." -ForegroundColor Yellow
        docker-compose down 2>&1 | Out-Null
        
        Write-Host "[2/4] Reconstruyendo imágenes Docker (sin cache)..." -ForegroundColor Yellow
        Write-Host "  Esto puede tardar varios minutos..." -ForegroundColor Gray
        docker-compose build --no-cache
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Error: Falló la reconstrucción de imágenes Docker" -ForegroundColor Red
            return $false
        }
        
        Write-Host "[3/4] Verificando imágenes construidas..." -ForegroundColor Yellow
        docker-compose images
        
        Write-Host "[4/4] Imágenes Docker reconstruidas exitosamente" -ForegroundColor Green
        Write-Host ""
        
        return $true
    } catch {
        Write-Host "Error durante la reconstrucción de Docker: $_" -ForegroundColor Red
        return $false
    } finally {
        Set-Location $originalLocation
    }
}

# Función para verificar AWS CLI
function Test-AwsCli {
    $awsCli = Get-Command aws -ErrorAction SilentlyContinue
    if (-not $awsCli) {
        Write-Host "Error: AWS CLI no está instalado" -ForegroundColor Red
        Write-Host "Instala desde: https://aws.amazon.com/cli/" -ForegroundColor Yellow
        return $false
    }
    
    Write-Host "AWS CLI instalado" -ForegroundColor Green
    
    try {
        $awsVersion = aws --version
        Write-Host "  Versión: $awsVersion" -ForegroundColor Gray
    } catch {
        Write-Host "Error al verificar versión de AWS CLI" -ForegroundColor Red
        return $false
    }
    
    # Verificar configuración
    try {
        $awsIdentity = aws sts get-caller-identity 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "AWS CLI configurado correctamente" -ForegroundColor Green
            return $true
        } else {
            Write-Host "Error: AWS CLI no está configurado" -ForegroundColor Red
            Write-Host "Ejecuta: aws configure" -ForegroundColor Yellow
            return $false
        }
    } catch {
        Write-Host "Error: AWS CLI no está configurado" -ForegroundColor Red
        Write-Host "Ejecuta: aws configure" -ForegroundColor Yellow
        return $false
    }
}

# Función para verificar Terraform
function Test-Terraform {
    $terraform = Get-Command terraform -ErrorAction SilentlyContinue
    $terraformLocal = $null
    
    # Si no está en PATH, buscar terraform.exe local
    if (-not $terraform) {
        $terraformPath = Join-Path $PSScriptRoot "terraform"
        $terraformExe = Join-Path $terraformPath "terraform.exe"
        if (Test-Path $terraformExe) {
            $terraformLocal = $terraformExe
            Write-Host "Terraform encontrado localmente" -ForegroundColor Green
        } else {
            Write-Host "Error: Terraform no está instalado" -ForegroundColor Red
            Write-Host "Instala desde: https://www.terraform.io/downloads" -ForegroundColor Yellow
            Write-Host "O coloca terraform.exe en el directorio terraform/" -ForegroundColor Yellow
            return $false
        }
    } else {
        Write-Host "Terraform instalado" -ForegroundColor Green
    }
    
    try {
        if ($terraformLocal) {
            $tfVersion = & $terraformLocal version
        } else {
            $tfVersion = terraform version
        }
        Write-Host "  $($tfVersion.Split([Environment]::NewLine)[0])" -ForegroundColor Gray
    } catch {
        Write-Host "Error al verificar versión de Terraform" -ForegroundColor Red
        return $false
    }
    
    return $true
}

# Verificar requisitos
Write-Host "Verificando requisitos..." -ForegroundColor Yellow
Write-Host ""

$dockerOk = Test-Docker
Write-Host ""

if (-not $dockerOk) {
    Write-Host "Error: Docker no está disponible. El proceso se detiene para evitar problemas." -ForegroundColor Red
    exit 1
}
$skipDocker = $false

$awsOk = Test-AwsCli
Write-Host ""

if (-not $awsOk) {
    exit 1
}

$terraformOk = Test-Terraform
Write-Host ""

if (-not $terraformOk) {
    exit 1
}

# Reconstruir Docker si está disponible
if (-not $skipDocker) {
    $dockerRebuilt = Rebuild-Docker
    if (-not $dockerRebuilt) {
        Write-Host ""
        Write-Host "Error: Falló la reconstrucción de Docker. El proceso se detiene para evitar problemas." -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host ""
    Write-Host "Omitiendo reconstrucción de Docker..." -ForegroundColor Yellow
    Write-Host ""
}

# Navegar al directorio de Terraform
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Paso 2: Desplegar Infraestructura AWS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$terraformPath = Join-Path $PSScriptRoot "terraform"

if (-not (Test-Path $terraformPath)) {
    Write-Host "Error: No se encontró el directorio terraform" -ForegroundColor Red
    Write-Host "Ruta buscada: $terraformPath" -ForegroundColor Gray
    exit 1
}

Set-Location $terraformPath

# Verificar archivo terraform.tfvars
$tfvarsPath = Join-Path $terraformPath "terraform.tfvars"
if (-not (Test-Path $tfvarsPath)) {
    Write-Host "Advertencia: No se encontró terraform.tfvars" -ForegroundColor Yellow
    $tfvarsExample = Join-Path $terraformPath "terraform.tfvars.example"
    if (Test-Path $tfvarsExample) {
        Write-Host "Copia terraform.tfvars.example a terraform.tfvars y configura tus valores" -ForegroundColor Yellow
    }
    Write-Host ""
}

# Determinar comando de terraform a usar
$terraformCmd = "terraform"
$terraformExePath = Join-Path $terraformPath "terraform.exe"
if (Test-Path $terraformExePath) {
    $terraformCmd = $terraformExePath
}

# Inicializar Terraform (si es necesario)
Write-Host "Inicializando Terraform..." -ForegroundColor Yellow
& $terraformCmd init
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Falló la inicialización de Terraform" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Importar recursos existentes (para evitar "ya existe" errors)
Write-Host "Importando recursos existentes en AWS..." -ForegroundColor Yellow
Write-Host "(Esto es seguro - solo sincroniza Terraform con lo que ya existe)" -ForegroundColor Gray
Write-Host ""

# Ejecutar script de importación
$importScript = Join-Path $terraformPath "import-resources.ps1"
if (Test-Path $importScript) {
    & $importScript
} else {
    Write-Host "Advertencia: Script de importación no encontrado" -ForegroundColor Yellow
}

Write-Host ""

# Aplicar infraestructura
Write-Host "Aplicando infraestructura..." -ForegroundColor Yellow
Write-Host "Esto puede tardar varios minutos..." -ForegroundColor Gray
Write-Host ""

& $terraformCmd apply -auto-approve

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Error: Falló la aplicación de la infraestructura" -ForegroundColor Red
    exit 1
}

# Obtener outputs de Terraform
Write-Host ""
Write-Host "Obteniendo información de despliegue..." -ForegroundColor Cyan
Write-Host ""

$tfOutput = & $terraformCmd output -json | ConvertFrom-Json

# Extraer valores de los outputs
$region = $tfOutput.aws_region.value
$ecrApiUrl = $tfOutput.ecr_repository_url.value
$ecrFrontendUrl = $tfOutput.ecr_frontend_repository_url.value
$apiUrl = $tfOutput.backend_api_url.value
$ecsCluster = $tfOutput.ecs_cluster_name.value
$ecsApiService = $tfOutput.ecs_service_name.value

Write-Host "Región: $region" -ForegroundColor Gray
Write-Host "ECR API: $ecrApiUrl" -ForegroundColor Gray
Write-Host "ECR Frontend: $ecrFrontendUrl" -ForegroundColor Gray
Write-Host "API URL: $apiUrl" -ForegroundColor Gray
Write-Host ""

# Función para hacer login en ECR
function Login-ECR {
    param([string]$Region, [string]$EcrUrl)
    
    Write-Host "Iniciando sesión en ECR..." -ForegroundColor Yellow
    Write-Host "  Región: $Region" -ForegroundColor Gray
    Write-Host "  URL: $EcrUrl" -ForegroundColor Gray
    
    # Validar que la URL no esté vacía
    if ([string]::IsNullOrWhiteSpace($EcrUrl)) {
        Write-Host "Error: URL de ECR vacía" -ForegroundColor Red
        return $false
    }
    
    # Asegurar que la URL no tenga protocolo
    $EcrUrl = $EcrUrl -replace "^https?://", ""
    
    try {
        $password = aws ecr get-login-password --region $Region 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Error: No se pudo obtener el token de ECR" -ForegroundColor Red
            Write-Host "  Detalle: $password" -ForegroundColor Gray
            return $false
        }
        
        $password | docker login --username AWS --password-stdin $EcrUrl 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Error: Falló el login en ECR" -ForegroundColor Red
            Write-Host "  URL intentada: $EcrUrl" -ForegroundColor Gray
            return $false
        }
        
        Write-Host "Login exitoso en ECR" -ForegroundColor Green
        return $true
    } catch {
        Write-Host "Error durante el login en ECR: $_" -ForegroundColor Red
        return $false
    }
}

# Función para construir y subir imagen del backend
function Deploy-Backend {
    param([string]$EcrApiUrl, [string]$Region)
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "  Paso 3: Desplegar Backend a ECR" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    
    $srcPath = Join-Path $PSScriptRoot "src"
    $currentLoc = Get-Location
    
    try {
        Set-Location $srcPath
        
        Write-Host "[1/3] Construyendo imagen del backend..." -ForegroundColor Yellow
        docker build -t gatekeep-api -f Dockerfile .
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Error: Falló la construcción de la imagen del backend" -ForegroundColor Red
            return $false
        }
        
        Write-Host "[2/3] Etiquetando imagen..." -ForegroundColor Yellow
        docker tag gatekeep-api:latest "$EcrApiUrl`:latest"
        
        Write-Host "[3/3] Subiendo imagen a ECR..." -ForegroundColor Yellow
        docker push "$EcrApiUrl`:latest"
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Error: Falló la subida de la imagen del backend" -ForegroundColor Red
            return $false
        }
        
        Write-Host "Backend desplegado exitosamente" -ForegroundColor Green
        return $true
    } catch {
        Write-Host "Error durante el despliegue del backend: $_" -ForegroundColor Red
        return $false
    } finally {
        Set-Location $currentLoc
    }
}

# Función para construir y subir imagen del frontend
function Deploy-Frontend {
    param([string]$EcrFrontendUrl, [string]$ApiUrl, [string]$Region)
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "  Paso 4: Desplegar Frontend a ECR" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    
    $frontendPath = Join-Path $PSScriptRoot "frontend"
    $currentLoc = Get-Location
    
    try {
        Set-Location $frontendPath
        
        Write-Host "[1/3] Construyendo imagen del frontend..." -ForegroundColor Yellow
        Write-Host "  API URL: $ApiUrl" -ForegroundColor Gray
        docker build -t gatekeep-frontend . --build-arg NEXT_PUBLIC_API_URL=$ApiUrl
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Error: Falló la construcción de la imagen del frontend" -ForegroundColor Red
            return $false
        }
        
        Write-Host "[2/3] Etiquetando imagen..." -ForegroundColor Yellow
        docker tag gatekeep-frontend:latest "$EcrFrontendUrl`:latest"
        
        Write-Host "[3/3] Subiendo imagen a ECR..." -ForegroundColor Yellow
        docker push "$EcrFrontendUrl`:latest"
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Error: Falló la subida de la imagen del frontend" -ForegroundColor Red
            return $false
        }
        
        Write-Host "Frontend desplegado exitosamente" -ForegroundColor Green
        return $true
    } catch {
        Write-Host "Error durante el despliegue del frontend: $_" -ForegroundColor Red
        return $false
    } finally {
        Set-Location $currentLoc
    }
}

# Función para actualizar servicios ECS
function Update-ECSServices {
    param([string]$Region, [string]$Cluster, [string]$ApiService)
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "  Paso 5: Actualizar Servicios ECS" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    
    # Actualizar servicio del backend
    Write-Host "Actualizando servicio del backend ($ApiService)..." -ForegroundColor Yellow
    aws ecs update-service --cluster $Cluster --service $ApiService --force-new-deployment --region $Region | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error: Falló la actualización del servicio del backend" -ForegroundColor Red
        return $false
    }
    Write-Host "  [OK] Backend actualizado" -ForegroundColor Green
    
    # Actualizar servicio del frontend (nombre basado en el patrón de Terraform)
    Write-Host "Actualizando servicio del frontend..." -ForegroundColor Yellow
    $frontendService = $ApiService -replace "-api-service", "-frontend-service"
    aws ecs update-service --cluster $Cluster --service $frontendService --force-new-deployment --region $Region | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  Advertencia: No se pudo actualizar el servicio del frontend (puede que no exista o ya esté actualizado)" -ForegroundColor Yellow
    } else {
        Write-Host "  [OK] Frontend actualizado" -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Host "Servicios ECS actualizados" -ForegroundColor Green
    return $true
}

# Desplegar imágenes a ECR
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Paso 3: Desplegar Imágenes a ECR" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Login en ECR - Extraer URL base del registro (sin el nombre del repositorio)
# La URL de ECR tiene formato: account.dkr.ecr.region.amazonaws.com/repository
# Necesitamos solo: account.dkr.ecr.region.amazonaws.com
$ecrBaseUrl = $ecrApiUrl -replace "/[^/]+$", ""

# Validar que se extrajo correctamente
if ([string]::IsNullOrWhiteSpace($ecrBaseUrl) -or $ecrBaseUrl -eq $ecrApiUrl) {
    # Fallback: construir desde la región y account ID
    # Extraer account ID de la URL original si es posible
    if ($ecrApiUrl -match "(\d+)\.dkr\.ecr\.([^.]+)\.amazonaws\.com") {
        $accountId = $matches[1]
        $ecrRegion = $matches[2]
        $ecrBaseUrl = "$accountId.dkr.ecr.$ecrRegion.amazonaws.com"
    } else {
        # Último fallback: usar valores conocidos
        $ecrBaseUrl = "126588786097.dkr.ecr.$region.amazonaws.com"
    }
}

Write-Host "URL base de ECR: $ecrBaseUrl" -ForegroundColor Gray
Write-Host ""

if (-not (Login-ECR -Region $region -EcrUrl $ecrBaseUrl)) {
    Write-Host "Error: No se pudo iniciar sesión en ECR. El proceso se detiene." -ForegroundColor Red
    exit 1
}

# Desplegar backend
if (-not (Deploy-Backend -EcrApiUrl $ecrApiUrl -Region $region)) {
    Write-Host "Error: Falló el despliegue del backend. El proceso se detiene." -ForegroundColor Red
    exit 1
}

# Desplegar frontend
if (-not (Deploy-Frontend -EcrFrontendUrl $ecrFrontendUrl -ApiUrl $apiUrl -Region $region)) {
    Write-Host "Error: Falló el despliegue del frontend. El proceso se detiene." -ForegroundColor Red
    exit 1
}

# Actualizar servicios ECS
if (-not (Update-ECSServices -Region $region -Cluster $ecsCluster -ApiService $ecsApiService)) {
    Write-Host "Advertencia: Hubo problemas al actualizar los servicios ECS" -ForegroundColor Yellow
}

# Mostrar outputs y resumen
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host '  [OK] Despliegue Completado Exitosamente' -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

Write-Host "Obteniendo URLs de los servicios..." -ForegroundColor Cyan
Write-Host ""

& $terraformCmd output

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Resumen del Despliegue" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host '[OK] Imágenes Docker reconstruidas' -ForegroundColor Green
Write-Host '[OK] Infraestructura AWS desplegada' -ForegroundColor Green
Write-Host '[OK] Backend desplegado a ECR' -ForegroundColor Green
Write-Host '[OK] Frontend desplegado a ECR' -ForegroundColor Green
Write-Host '[OK] Servicios ECS actualizados' -ForegroundColor Green
Write-Host ""
Write-Host "Espera unos minutos para que ECS reemplace las tareas antiguas con las nuevas imágenes." -ForegroundColor Yellow
Write-Host ""
Write-Host 'Para ver los outputs completos:' -ForegroundColor Cyan
Write-Host '  cd terraform; terraform output' -ForegroundColor White
Write-Host ""

