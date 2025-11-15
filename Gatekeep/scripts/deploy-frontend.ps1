# Script para construir y subir la imagen Docker del frontend a ECR

$ErrorActionPreference = "Stop"

Write-Host "=== Desplegar Frontend a AWS ===" -ForegroundColor Cyan
Write-Host ""

# Verificar que Docker está corriendo
Write-Host "1. Verificando Docker..." -ForegroundColor Yellow
$dockerCheck = docker ps 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Docker Desktop no esta corriendo" -ForegroundColor Red
    Write-Host "   Por favor, inicia Docker Desktop y vuelve a intentar" -ForegroundColor Yellow
    exit 1
}
Write-Host "OK: Docker esta corriendo" -ForegroundColor Green
Write-Host ""

# Obtener configuración desde Terraform
Write-Host "2. Obteniendo configuracion de Terraform..." -ForegroundColor Yellow
Push-Location "terraform"
$ecrUrl = (terraform output -raw ecr_frontend_repository_url).Trim()
$region = (terraform output -raw aws_region).Trim()
$albDns = (terraform output -raw alb_dns_name).Trim()
Pop-Location

# Validar que las variables no estén vacías
if ([string]::IsNullOrWhiteSpace($ecrUrl)) {
    Write-Host "ERROR: No se pudo obtener ECR URL de Terraform" -ForegroundColor Red
    exit 1
}
if ([string]::IsNullOrWhiteSpace($region)) {
    Write-Host "ERROR: No se pudo obtener region de Terraform" -ForegroundColor Red
    exit 1
}
if ([string]::IsNullOrWhiteSpace($albDns)) {
    Write-Host "ERROR: No se pudo obtener ALB DNS de Terraform" -ForegroundColor Red
    exit 1
}

Write-Host "   ECR URL: $ecrUrl" -ForegroundColor Gray
Write-Host "   Region: $region" -ForegroundColor Gray
Write-Host "   ALB DNS: $albDns" -ForegroundColor Gray
Write-Host ""

# Login a ECR
Write-Host "3. Login a ECR..." -ForegroundColor Yellow
$password = aws ecr get-login-password --region $region --profile Alex
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: No se pudo obtener password de ECR" -ForegroundColor Red
    exit 1
}
$password | docker login --username AWS --password-stdin $ecrUrl
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: No se pudo hacer login a Docker" -ForegroundColor Red
    exit 1
}
Write-Host "OK: Login exitoso" -ForegroundColor Green
Write-Host ""

# Build imagen con variable de entorno
Write-Host "4. Construyendo imagen Docker del frontend..." -ForegroundColor Yellow
Write-Host "   Esto puede tardar varios minutos..." -ForegroundColor Gray
$apiUrl = "http://$albDns"
docker build -t gatekeep-frontend:latest --build-arg NEXT_PUBLIC_API_URL=$apiUrl -f frontend/Dockerfile frontend/
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: No se pudo construir imagen" -ForegroundColor Red
    exit 1
}
Write-Host "OK: Imagen construida" -ForegroundColor Green
Write-Host ""

# Tag imagen - Usar sintaxis más robusta
Write-Host "5. Taggeando imagen para ECR..." -ForegroundColor Yellow
$targetImage = "${ecrUrl}:latest"
Write-Host "   Tagging: gatekeep-frontend:latest -> $targetImage" -ForegroundColor Gray
docker tag gatekeep-frontend:latest $targetImage
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: No se pudo taggear imagen" -ForegroundColor Red
    Write-Host "   ECR URL: $ecrUrl" -ForegroundColor Red
    Write-Host "   Target: $targetImage" -ForegroundColor Red
    exit 1
}
Write-Host "OK: Imagen taggeada" -ForegroundColor Green
Write-Host ""

# Push imagen
Write-Host "6. Subiendo imagen a ECR..." -ForegroundColor Yellow
Write-Host "   Esto puede tardar varios minutos..." -ForegroundColor Gray
docker push $targetImage
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: No se pudo subir imagen" -ForegroundColor Red
    exit 1
}
Write-Host "OK: Imagen subida exitosamente" -ForegroundColor Green
Write-Host ""

# Forzar actualización del servicio ECS
Write-Host "7. Forzando actualizacion del servicio ECS..." -ForegroundColor Yellow
aws ecs update-service --cluster gatekeep-cluster --service gatekeep-frontend-service --force-new-deployment --region $region --profile Alex | Out-Null
if ($LASTEXITCODE -eq 0) {
    Write-Host "OK: Servicio ECS actualizado" -ForegroundColor Green
} else {
    Write-Host "ADVERTENCIA: No se pudo actualizar el servicio automaticamente" -ForegroundColor Yellow
    Write-Host "   El servicio usara la nueva imagen en el proximo despliegue" -ForegroundColor Gray
}
Write-Host ""

# Mostrar información
Write-Host "=== COMPLETADO ===" -ForegroundColor Green
Write-Host ""
Write-Host "URL del frontend:" -ForegroundColor Cyan
Write-Host "   http://$albDns" -ForegroundColor White
Write-Host ""
Write-Host "URL del backend API:" -ForegroundColor Cyan
Write-Host "   http://$albDns/api" -ForegroundColor White
Write-Host ""
Write-Host "El servicio puede tardar 2-5 minutos en estar disponible" -ForegroundColor Yellow

