# Script para desplegar el frontend con la URL de API correcta
$region = "sa-east-1"
$ecrUrl = "126588786097.dkr.ecr.sa-east-1.amazonaws.com"
$repoName = "gatekeep-frontend"
$fullImage = "$ecrUrl/$repoName`:latest"
$apiUrl = "https://api.zimmzimmgames.com"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  GateKeep - Despliegue de Frontend" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "API URL: $apiUrl" -ForegroundColor Gray
Write-Host "ECR: $fullImage" -ForegroundColor Gray
Write-Host ""

# 1. Login ECR
Write-Host "[1/4] Iniciando sesión en ECR..." -ForegroundColor Yellow
aws ecr get-login-password --region $region | docker login --username AWS --password-stdin $ecrUrl
if ($LASTEXITCODE -ne 0) { Write-Error "Fallo en docker login"; exit 1 }

# 2. Build
Write-Host "[2/4] Construyendo imagen Docker..." -ForegroundColor Yellow
$frontendPath = Join-Path $PSScriptRoot "frontend"
$currentLoc = Get-Location
Set-Location $frontendPath

# AQUÍ ESTÁ LA CLAVE: Pasamos la URL de producción como build-arg
docker build -t $repoName . --build-arg NEXT_PUBLIC_API_URL=$apiUrl
if ($LASTEXITCODE -ne 0) { 
    Set-Location $currentLoc
    Write-Error "Fallo en docker build"
    exit 1 
}

# 3. Tag & Push
Write-Host "[3/4] Subiendo imagen a ECR..." -ForegroundColor Yellow
docker tag $repoName`:latest $fullImage
docker push $fullImage
if ($LASTEXITCODE -ne 0) { 
    Set-Location $currentLoc
    Write-Error "Fallo en docker push"
    exit 1 
}

Set-Location $currentLoc

# 4. ECS Update
Write-Host "[4/4] Actualizando servicio ECS..." -ForegroundColor Yellow
aws ecs update-service --cluster gatekeep-cluster --service gatekeep-frontend-service --force-new-deployment --region $region | Out-Null
if ($LASTEXITCODE -ne 0) { Write-Error "Fallo al actualizar servicio ECS"; exit 1 }

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  [OK] Frontend actualizado exitosamente" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "Espera unos minutos para que ECS reemplace las tareas antiguas." -ForegroundColor Cyan
