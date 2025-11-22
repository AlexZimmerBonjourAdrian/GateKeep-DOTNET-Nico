# ==============================================
# Script Simple para Desplegar Docker a AWS ECR
# ==============================================
# Construye y sube la imagen Docker de la API a ECR
# ==============================================

param(
    [string]$Region = "sa-east-1",
    [string]$EcrRepository = "126588786097.dkr.ecr.sa-east-1.amazonaws.com/gatekeep-api",
    [switch]$ForceDeployment
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Despliegue de Docker a AWS ECR" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar que Docker esté disponible
Write-Host "Verificando Docker..." -ForegroundColor Yellow
try {
    docker --version | Out-Null
    Write-Host "✓ Docker está disponible" -ForegroundColor Green
} catch {
    Write-Host "✗ Error: Docker no está disponible" -ForegroundColor Red
    exit 1
}

# Verificar credenciales AWS
Write-Host "Verificando credenciales AWS..." -ForegroundColor Yellow
try {
    aws sts get-caller-identity | Out-Null
    Write-Host "✓ Credenciales AWS configuradas" -ForegroundColor Green
} catch {
    Write-Host "✗ Error: Credenciales AWS no configuradas" -ForegroundColor Red
    Write-Host "  Configura las credenciales con: aws configure" -ForegroundColor Yellow
    exit 1
}

# Extraer URL base de ECR
$ecrBaseUrl = $EcrRepository -replace "/[^/]+$", ""
if ([string]::IsNullOrWhiteSpace($ecrBaseUrl) -or $ecrBaseUrl -eq $EcrRepository) {
    if ($EcrRepository -match "(\d+\.dkr\.ecr\.[^.]+\.amazonaws\.com)") {
        $ecrBaseUrl = $matches[1]
    } else {
        Write-Host "✗ Error: No se pudo extraer la URL base de ECR" -ForegroundColor Red
        exit 1
    }
}

Write-Host "Región: $Region" -ForegroundColor Cyan
Write-Host "ECR Base URL: $ecrBaseUrl" -ForegroundColor Cyan
Write-Host "Repositorio: $EcrRepository" -ForegroundColor Cyan
Write-Host ""

# Paso 1: Autenticar con ECR
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Paso 1: Autenticando con ECR" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

try {
    Write-Host "Obteniendo token de ECR..." -ForegroundColor Yellow
    $password = aws ecr get-login-password --region $Region
    if ($LASTEXITCODE -ne 0) {
        throw "Error al obtener token de ECR"
    }
    
    Write-Host "Autenticando con Docker..." -ForegroundColor Yellow
    $password | docker login --username AWS --password-stdin $ecrBaseUrl
    if ($LASTEXITCODE -ne 0) {
        throw "Error al autenticar con ECR"
    }
    
    Write-Host "✓ Autenticación exitosa" -ForegroundColor Green
} catch {
    Write-Host "✗ Error en autenticación: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Paso 2: Construir imagen Docker
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Paso 2: Construyendo imagen Docker" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$imageTag = "$EcrRepository:latest"
$dockerfilePath = Join-Path $PSScriptRoot "src" "Dockerfile"
$buildContext = Join-Path $PSScriptRoot "src"

if (-not (Test-Path $dockerfilePath)) {
    Write-Host "✗ Error: Dockerfile no encontrado en: $dockerfilePath" -ForegroundColor Red
    exit 1
}

Write-Host "Dockerfile: $dockerfilePath" -ForegroundColor Cyan
Write-Host "Contexto: $buildContext" -ForegroundColor Cyan
Write-Host "Tag: $imageTag" -ForegroundColor Cyan
Write-Host ""

try {
    Write-Host "Construyendo imagen (esto puede tomar varios minutos)..." -ForegroundColor Yellow
    Push-Location $buildContext
    docker build -t $imageTag -f Dockerfile .
    
    if ($LASTEXITCODE -ne 0) {
        throw "Error al construir la imagen"
    }
    
    Write-Host "✓ Imagen construida exitosamente" -ForegroundColor Green
    Pop-Location
} catch {
    Write-Host "✗ Error al construir imagen: $_" -ForegroundColor Red
    Pop-Location
    exit 1
}

Write-Host ""

# Paso 3: Subir imagen a ECR
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Paso 3: Subiendo imagen a ECR" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

try {
    Write-Host "Subiendo imagen (esto puede tomar varios minutos)..." -ForegroundColor Yellow
    docker push $imageTag
    
    if ($LASTEXITCODE -ne 0) {
        throw "Error al subir la imagen"
    }
    
    Write-Host "✓ Imagen subida exitosamente" -ForegroundColor Green
} catch {
    Write-Host "✗ Error al subir imagen: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Paso 4: Forzar nuevo deployment en ECS (opcional)
if ($ForceDeployment) {
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Paso 4: Forzando nuevo deployment en ECS" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    
    # Intentar obtener información de ECS desde variables de entorno o configuración
    $clusterName = $env:ECS_CLUSTER_NAME
    $serviceName = $env:ECS_SERVICE_NAME_API
    
    if ([string]::IsNullOrWhiteSpace($clusterName)) {
        $clusterName = "gatekeep-cluster"
        Write-Host "Usando cluster por defecto: $clusterName" -ForegroundColor Yellow
    }
    
    if ([string]::IsNullOrWhiteSpace($serviceName)) {
        $serviceName = "gatekeep-api-service"
        Write-Host "Usando servicio por defecto: $serviceName" -ForegroundColor Yellow
    }
    
    try {
        Write-Host "Forzando nuevo deployment..." -ForegroundColor Yellow
        aws ecs update-service `
            --cluster $clusterName `
            --service $serviceName `
            --force-new-deployment `
            --region $Region `
            --output json | Out-Null
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ Deployment forzado exitosamente" -ForegroundColor Green
            Write-Host "  El servicio se actualizará automáticamente con la nueva imagen" -ForegroundColor Cyan
        } else {
            Write-Host "⚠ Advertencia: No se pudo forzar el deployment (puede que el servicio no exista)" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "⚠ Advertencia: Error al forzar deployment: $_" -ForegroundColor Yellow
        Write-Host "  Puedes forzar el deployment manualmente desde la consola de AWS" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "✓ Despliegue completado exitosamente" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Imagen disponible en: $imageTag" -ForegroundColor Cyan
Write-Host ""

