# Script simple para desplegar GateKeep en AWS

$ErrorActionPreference = "Stop"

Write-Host "=== Despliegue Simple de GateKeep ===" -ForegroundColor Cyan
Write-Host ""

# Paso 1: Construir y subir imagen Docker a ECR
Write-Host "1. Construyendo y subiendo imagen Docker a ECR..." -ForegroundColor Yellow

$ecrUrl = terraform -chdir=terraform output -raw ecr_repository_url
$region = terraform -chdir=terraform output -raw aws_region

# Login a ECR
Write-Host "   Login a ECR..." -ForegroundColor Gray
aws ecr get-login-password --region $region --profile Alex | docker login --username AWS --password-stdin $ecrUrl
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Error al hacer login a ECR" -ForegroundColor Red
    exit 1
}

# Build imagen
Write-Host "   Construyendo imagen..." -ForegroundColor Gray
docker build -t gatekeep-api:latest -f src/Dockerfile src/
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Error al construir imagen" -ForegroundColor Red
    exit 1
}

# Tag imagen
Write-Host "   Taggeando imagen..." -ForegroundColor Gray
docker tag gatekeep-api:latest "$ecrUrl:latest"

# Push imagen
Write-Host "   Subiendo imagen a ECR..." -ForegroundColor Gray
docker push "$ecrUrl:latest"
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Error al subir imagen" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Imagen subida exitosamente" -ForegroundColor Green
Write-Host ""

# Paso 2: Desplegar infraestructura ECS
Write-Host "2. Desplegando infraestructura ECS..." -ForegroundColor Yellow
Push-Location terraform

terraform apply -auto-approve
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Error al desplegar infraestructura" -ForegroundColor Red
    Pop-Location
    exit 1
}

Write-Host "✓ Infraestructura desplegada" -ForegroundColor Green
Write-Host ""

# Paso 3: Mostrar URLs
Write-Host "3. Obteniendo información del despliegue..." -ForegroundColor Yellow
$appUrl = terraform output -raw application_url
$albDns = terraform output -raw alb_dns_name

Pop-Location

Write-Host ""
Write-Host "=== DESPLIEGUE COMPLETADO ===" -ForegroundColor Green
Write-Host ""
Write-Host "🌐 URL de la aplicación:" -ForegroundColor Cyan
Write-Host "   $appUrl" -ForegroundColor White
Write-Host ""
Write-Host "📋 Endpoints:" -ForegroundColor Cyan
Write-Host "   API: $appUrl" -ForegroundColor White
Write-Host "   Health: $appUrl/health" -ForegroundColor White
Write-Host "   Swagger: $appUrl/swagger" -ForegroundColor White
Write-Host ""
Write-Host "⏱️  Nota: El servicio puede tardar 2-3 minutos en estar disponible" -ForegroundColor Yellow
Write-Host "   Verifica el estado con: aws ecs describe-services --cluster gatekeep-cluster --services gatekeep-api-service --region sa-east-1 --profile Alex" -ForegroundColor Gray

