# Script para actualizar/desplegar a AWS
# Construye imágenes Docker, las sube a ECR y actualiza servicios ECS

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  GateKeep - Actualizar Servicios AWS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Función para obtener ECR repository desde Terraform
function Get-EcrRepository {
    param([string]$TerraformPath, [string]$OutputName)
    
    Push-Location $TerraformPath
    try {
        $output = terraform output -raw $OutputName 2>&1
        if ($LASTEXITCODE -eq 0) {
            return $output.Trim()
        }
        return $null
    } finally {
        Pop-Location
    }
}

# Verificar AWS CLI
Write-Host "Verificando AWS CLI..." -ForegroundColor Yellow
$awsCli = Get-Command aws -ErrorAction SilentlyContinue
if (-not $awsCli) {
    Write-Host "Error: AWS CLI no está instalado" -ForegroundColor Red
    exit 1
}

try {
    aws sts get-caller-identity | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error: AWS CLI no está configurado" -ForegroundColor Red
        Write-Host "Ejecuta: aws configure" -ForegroundColor Yellow
        exit 1
    }
} catch {
    Write-Host "Error: AWS CLI no está configurado" -ForegroundColor Red
    exit 1
}

Write-Host "AWS CLI configurado correctamente" -ForegroundColor Green
Write-Host ""

# Obtener ECR repositories desde Terraform
$terraformPath = Join-Path $PSScriptRoot "terraform"

if (-not (Test-Path $terraformPath)) {
    Write-Host "Error: No se encontró el directorio terraform" -ForegroundColor Red
    exit 1
}

Write-Host "Obteniendo información de ECR desde Terraform..." -ForegroundColor Yellow

$ecrApiUrl = Get-EcrRepository -TerraformPath $terraformPath -OutputName "ecr_repository_url"
$ecrFrontendUrl = Get-EcrRepository -TerraformPath $terraformPath -OutputName "ecr_frontend_repository_url"

if (-not $ecrApiUrl) {
    Write-Host "Error: No se pudo obtener la URL del repositorio ECR para API" -ForegroundColor Red
    Write-Host "Asegúrate de que la infraestructura esté desplegada: .\start-aws.ps1" -ForegroundColor Yellow
    exit 1
}

Write-Host "  API Repository: $ecrApiUrl" -ForegroundColor Gray
if ($ecrFrontendUrl) {
    Write-Host "  Frontend Repository: $ecrFrontendUrl" -ForegroundColor Gray
}
Write-Host ""

# Obtener región de AWS
$awsRegion = aws configure get region
if (-not $awsRegion) {
    $awsRegion = "sa-east-1"
    Write-Host "Usando región por defecto: $awsRegion" -ForegroundColor Yellow
}

# Login a ECR
Write-Host "Autenticando con ECR..." -ForegroundColor Yellow
$ecrLogin = aws ecr get-login-password --region $awsRegion | docker login --username AWS --password-stdin $ecrApiUrl.Split('/')[0]
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Falló la autenticación con ECR" -ForegroundColor Red
    exit 1
}
Write-Host "Autenticado correctamente" -ForegroundColor Green
Write-Host ""

# Construir y subir imagen de API
Write-Host "Construyendo imagen de API..." -ForegroundColor Yellow
$apiPath = Join-Path $PSScriptRoot "src"
Set-Location $apiPath

$imageTag = "$ecrApiUrl:latest"
docker build -t $imageTag -f Dockerfile .
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Falló la construcción de la imagen de API" -ForegroundColor Red
    exit 1
}

Write-Host "Subiendo imagen de API a ECR..." -ForegroundColor Yellow
docker push $imageTag
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Falló el push de la imagen de API" -ForegroundColor Red
    exit 1
}
Write-Host "Imagen de API actualizada" -ForegroundColor Green
Write-Host ""

# Construir y subir imagen de Frontend (si existe)
if ($ecrFrontendUrl) {
    Write-Host "Construyendo imagen de Frontend..." -ForegroundColor Yellow
    $frontendPath = Join-Path $PSScriptRoot "frontend"
    
    if (Test-Path $frontendPath) {
        Set-Location $frontendPath
        
        $frontendImageTag = "$ecrFrontendUrl:latest"
        docker build -t $frontendImageTag -f Dockerfile .
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Subiendo imagen de Frontend a ECR..." -ForegroundColor Yellow
            docker push $frontendImageTag
            if ($LASTEXITCODE -eq 0) {
                Write-Host "Imagen de Frontend actualizada" -ForegroundColor Green
            }
        }
        Write-Host ""
    }
}

# Forzar nueva deployment en ECS
Write-Host "Forzando nueva deployment en ECS..." -ForegroundColor Yellow

$ecsCluster = terraform -chdir=$terraformPath output -raw ecs_cluster_name 2>&1
$ecsService = terraform -chdir=$terraformPath output -raw ecs_service_name 2>&1

if ($ecsCluster -and $ecsService) {
    Write-Host "  Cluster: $ecsCluster" -ForegroundColor Gray
    Write-Host "  Service: $ecsService" -ForegroundColor Gray
    
    aws ecs update-service --cluster $ecsCluster --service $ecsService --force-new-deployment --region $awsRegion | Out-Null
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Deployment iniciado" -ForegroundColor Green
        Write-Host ""
        Write-Host "El servicio se está actualizando. Puede tardar varios minutos." -ForegroundColor Yellow
        Write-Host "Verifica el estado con:" -ForegroundColor Cyan
        Write-Host "  aws ecs describe-services --cluster $ecsCluster --services $ecsService --region $awsRegion" -ForegroundColor White
    } else {
        Write-Host "Advertencia: No se pudo forzar el deployment automáticamente" -ForegroundColor Yellow
        Write-Host "Puedes hacerlo manualmente desde la consola de AWS" -ForegroundColor Gray
    }
} else {
    Write-Host "Advertencia: No se pudo obtener información de ECS" -ForegroundColor Yellow
    Write-Host "Forza el deployment manualmente desde la consola de AWS" -ForegroundColor Gray
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Actualización completada" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

