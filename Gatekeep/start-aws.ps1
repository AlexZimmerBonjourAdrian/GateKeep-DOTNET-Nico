# Script para iniciar servicios en AWS
# Verifica si el servicio está levantado, si no está, lo levanta con todo lo necesario
# Si ya está levantado, no hace nada

param(
    [string]$LogFile = $null,
    [switch]$SkipDockerRebuild,
    [int]$DeploymentTimeoutMinutes = 15
)

# Importar módulo común
$modulePath = Join-Path $PSScriptRoot "scripts" "AwsDeploymentCommon.psm1"
if (Test-Path $modulePath) {
    Import-Module $modulePath -Force
} else {
    Write-Host "Error: No se encontró el módulo común en: $modulePath" -ForegroundColor Red
    exit 1
}

# Inicializar logging
if ($LogFile) {
    Initialize-Logging -LogFilePath $LogFile -LogLevel "Info"
} else {
    Initialize-Logging -LogLevel "Info"
}

Write-Step "GateKeep - Despliegue Completo AWS"

# Verificar prerequisitos
Write-Step "Verificando Prerequisitos"
$prereqCheck = Test-Prerequisites -Required @("Docker", "AwsCli") -Optional @("Terraform")

if (-not $prereqCheck.AllOk) {
    Write-Log "Faltan prerequisitos críticos. El proceso se detiene." -Level "Error"
    exit 1
}

# Verificar Terraform (necesario para este script)
$terraformCheck = Test-Prerequisite -Prerequisite "Terraform"
if (-not $terraformCheck.Installed) {
    # Buscar terraform.exe local
    $terraformPath = Join-Path $PSScriptRoot "terraform"
    $terraformExe = Join-Path $terraformPath "terraform.exe"
    if (-not (Test-Path $terraformExe)) {
        Write-Log "Error: Terraform no está instalado y no se encontró terraform.exe local" -Level "Error"
        Write-Log "Instala desde: https://www.terraform.io/downloads" -Level "Info"
        Write-Log "O coloca terraform.exe en el directorio terraform/" -Level "Info"
        exit 1
    }
}

# Verificar estado del servicio ECS
Write-Step "Verificando Estado del Servicio AWS"
$awsRegion = Get-AwsRegion

# Obtener información de recursos para verificar estado
$terraformPath = Join-Path $PSScriptRoot "terraform"
$resourceInfo = Get-AwsResourceInfo -TerraformPath $terraformPath -Region $awsRegion

$ecsCluster = $resourceInfo.EcsCluster
$ecsService = $resourceInfo.EcsService

# Verificar si el servicio está corriendo
$serviceRunning = $false
if ($ecsCluster -and $ecsService) {
    $serviceStatus = Get-EcsServiceStatus -ClusterName $ecsCluster -ServiceName $ecsService -Region $awsRegion
    
    if ($serviceStatus.Found -and 
        $serviceStatus.Status -eq "ACTIVE" -and 
        $serviceStatus.RunningCount -gt 0 -and 
        $serviceStatus.RunningCount -ge $serviceStatus.DesiredCount) {
        $serviceRunning = $true
        Write-Log "Servicio ECS está corriendo:" -Level "Success"
        Write-Log "  Cluster: $ecsCluster" -Level "Info"
        Write-Log "  Service: $ecsService" -Level "Info"
        Write-Log "  Running: $($serviceStatus.RunningCount)/$($serviceStatus.DesiredCount)" -Level "Info"
        Write-Log "  Status: $($serviceStatus.Status)" -Level "Info"
    }
}

if ($serviceRunning) {
    Write-Step "Servicio ya está corriendo" "Green"
    Write-Log "El servicio ECS ya está activo y funcionando." -Level "Success"
    Write-Log "No es necesario realizar el despliegue completo." -Level "Info"
    Write-Log "" -Level "Info"
    Write-Log "Si necesitas actualizar el código, usa: .\update-aws.ps1" -Level "Info"
    exit 0
}

Write-Log "Servicio no está corriendo. Iniciando despliegue completo..." -Level "Info"

# Reconstruir Docker local (opcional)
if (-not $SkipDockerRebuild) {
    Write-Step "Paso 1: Reconstruir Imágenes Docker Localmente"
    
    $srcPath = Join-Path $PSScriptRoot "src"
    
    if (-not (Test-Path $srcPath)) {
        Write-Log "Error: No se encontró el directorio src" -Level "Error"
        Write-Log "Ruta buscada: $srcPath" -Level "Error"
        exit 1
    }
    
    $originalLocation = Get-Location
    
    try {
        Set-Location $srcPath
        
        Write-Log "[1/4] Deteniendo servicios existentes..." -Level "Info"
        docker-compose down 2>&1 | Out-Null
        
        Write-Log "[2/4] Reconstruyendo imágenes Docker (sin cache)..." -Level "Info"
        Write-Log "  Esto puede tardar varios minutos..." -Level "Info"
        
        docker-compose build --no-cache 2>&1 | ForEach-Object {
            Write-Log "  $_" -Level "Debug" -NoConsole
        }
        
        if ($LASTEXITCODE -ne 0) {
            Write-Log "Error: Falló la reconstrucción de imágenes Docker" -Level "Error"
            exit 1
        }
        
        Write-Log "[3/4] Verificando imágenes construidas..." -Level "Info"
        docker-compose images 2>&1 | ForEach-Object {
            Write-Log "  $_" -Level "Debug" -NoConsole
        }
        
        Write-Log "[4/4] Imágenes Docker reconstruidas exitosamente" -Level "Success"
    } catch {
        Write-Log "Error durante la reconstrucción de Docker: $_" -Level "Error"
        exit 1
    } finally {
        Set-Location $originalLocation
    }
} else {
    Write-Log "Omitiendo reconstrucción de Docker local..." -Level "Info"
}

# Desplegar infraestructura con Terraform
Write-Step "Paso 2: Desplegar Infraestructura AWS"

$terraformPath = Join-Path $PSScriptRoot "terraform"

if (-not (Test-Path $terraformPath)) {
    Write-Log "Error: No se encontró el directorio terraform" -Level "Error"
    Write-Log "Ruta buscada: $terraformPath" -Level "Error"
    exit 1
}

Set-Location $terraformPath

# Verificar archivo terraform.tfvars
$tfvarsPath = Join-Path $terraformPath "terraform.tfvars"
if (-not (Test-Path $tfvarsPath)) {
    Write-Log "Advertencia: No se encontró terraform.tfvars" -Level "Warning"
    $tfvarsExample = Join-Path $terraformPath "terraform.tfvars.example"
    if (Test-Path $tfvarsExample) {
        Write-Log "Copia terraform.tfvars.example a terraform.tfvars y configura tus valores" -Level "Info"
    }
}

# Determinar comando de terraform a usar
$terraformCmd = "terraform"
$terraformExePath = Join-Path $terraformPath "terraform.exe"
if (Test-Path $terraformExePath) {
    $terraformCmd = $terraformExePath
}

# Inicializar Terraform (si es necesario)
Write-Log "Inicializando Terraform..." -Level "Info"
& $terraformCmd init 2>&1 | ForEach-Object {
    Write-Log "  $_" -Level "Debug" -NoConsole
}

if ($LASTEXITCODE -ne 0) {
    Write-Log "Error: Falló la inicialización de Terraform" -Level "Error"
    exit 1
}

# Importar recursos existentes (para evitar "ya existe" errors)
Write-Log "Importando recursos existentes en AWS..." -Level "Info"
Write-Log "(Esto es seguro - solo sincroniza Terraform con lo que ya existe)" -Level "Info"

$importScript = Join-Path $terraformPath "import-resources.ps1"
if (Test-Path $importScript) {
    & $importScript
} else {
    Write-Log "Advertencia: Script de importación no encontrado" -Level "Warning"
}

# Aplicar infraestructura
Write-Log "Aplicando infraestructura..." -Level "Info"
Write-Log "Esto puede tardar varios minutos..." -Level "Info"

& $terraformCmd apply -auto-approve 2>&1 | ForEach-Object {
    Write-Log "  $_" -Level "Debug" -NoConsole
}

if ($LASTEXITCODE -ne 0) {
    Write-Log "Error: Falló la aplicación de la infraestructura" -Level "Error"
    exit 1
}

# Obtener outputs de Terraform
Write-Step "Obteniendo Información de Despliegue"

try {
    $region = Get-TerraformOutput -TerraformPath $terraformPath -OutputName "aws_region" -Required
    $ecrApiUrl = Get-TerraformOutput -TerraformPath $terraformPath -OutputName "ecr_repository_url" -Required
    $ecrFrontendUrl = Get-TerraformOutput -TerraformPath $terraformPath -OutputName "ecr_frontend_repository_url" -Required:$false
    $apiUrl = Get-TerraformOutput -TerraformPath $terraformPath -OutputName "backend_api_url" -Required:$false
    $ecsCluster = Get-TerraformOutput -TerraformPath $terraformPath -OutputName "ecs_cluster_name" -Required
    $ecsApiService = Get-TerraformOutput -TerraformPath $terraformPath -OutputName "ecs_service_name" -Required
    
    # Validar que los valores críticos no estén vacíos
    if ([string]::IsNullOrWhiteSpace($region) -or [string]::IsNullOrWhiteSpace($ecrApiUrl) -or [string]::IsNullOrWhiteSpace($ecsCluster) -or [string]::IsNullOrWhiteSpace($ecsApiService)) {
        Write-Log "Error: Algunos outputs de Terraform están vacíos" -Level "Error"
        Write-Log "  Region: $region" -Level "Error"
        Write-Log "  ECR API URL: $ecrApiUrl" -Level "Error"
        Write-Log "  ECS Cluster: $ecsCluster" -Level "Error"
        Write-Log "  ECS Service: $ecsApiService" -Level "Error"
        exit 1
    }
} catch {
    Write-Log "Error al procesar outputs de Terraform: $_" -Level "Error"
    exit 1
}

Write-Log "Región: $region" -Level "Info"
Write-Log "ECR API: $ecrApiUrl" -Level "Info"
if ($ecrFrontendUrl) {
    Write-Log "ECR Frontend: $ecrFrontendUrl" -Level "Info"
}
if ($apiUrl) {
    Write-Log "API URL: $apiUrl" -Level "Info"
}

# Desplegar imágenes a ECR
Write-Step "Paso 3: Desplegar Imágenes a ECR"

# Autenticar con ECR
try {
    Connect-Ecr -EcrRepositoryUrl $ecrApiUrl -Region $region | Out-Null
    Write-Log "Autenticado correctamente con ECR" -Level "Success"
} catch {
    Write-Log "Error: No se pudo iniciar sesión en ECR. El proceso se detiene." -Level "Error"
    exit 1
}

# Desplegar backend
Write-Step "Desplegar Backend a ECR"
$srcPath = Join-Path $PSScriptRoot "src"
$apiImageTag = "$ecrApiUrl:latest"

try {
    $buildSuccess = Build-DockerImage -ImageTag $apiImageTag -DockerfilePath "Dockerfile" -BuildContext $srcPath
    
    if (-not $buildSuccess) {
        Write-Log "Error: Falló la construcción de la imagen del backend" -Level "Error"
        exit 1
    }
    
    $pushSuccess = Push-DockerImage -ImageTag $apiImageTag -EcrRepository $ecrApiUrl -Region $region
    
    if (-not $pushSuccess) {
        Write-Log "Error: Falló la subida de la imagen del backend" -Level "Error"
        exit 1
    }
    
    Write-Log "Backend desplegado exitosamente" -Level "Success"
} catch {
    Write-Log "Error durante el despliegue del backend: $_" -Level "Error"
    exit 1
}

# Desplegar frontend (si existe)
if ($ecrFrontendUrl) {
    Write-Step "Desplegar Frontend a ECR"
    $frontendPath = Join-Path $PSScriptRoot "frontend"
    $frontendImageTag = "$ecrFrontendUrl:latest"
    
    if (Test-Path $frontendPath) {
        $frontendDockerfile = Join-Path $frontendPath "Dockerfile"
        if (Test-Path $frontendDockerfile) {
            try {
                # Construir con build arg si tenemos API URL
                $buildArgs = ""
                if ($apiUrl) {
                    $buildArgs = "--build-arg NEXT_PUBLIC_API_URL=$apiUrl"
                }
                
                Push-Location $frontendPath
                try {
                    Write-Log "Construyendo imagen del frontend..." -Level "Info"
                    if ($apiUrl) {
                        Write-Log "  API URL: $apiUrl" -Level "Info"
                    }
                    
                    # Usar Build-DockerImage pero necesitamos pasar build args
                    # Por ahora, construir directamente con build args
                    $originalBuildkit = $env:DOCKER_BUILDKIT
                    $env:DOCKER_BUILDKIT = "0"
                    
                    if ($apiUrl) {
                        docker build -t $frontendImageTag -f Dockerfile . --build-arg NEXT_PUBLIC_API_URL=$apiUrl 2>&1 | ForEach-Object {
                            Write-Log "  $_" -Level "Debug" -NoConsole
                        }
                    } else {
                        docker build -t $frontendImageTag -f Dockerfile . 2>&1 | ForEach-Object {
                            Write-Log "  $_" -Level "Debug" -NoConsole
                        }
                    }
                    
                    if ($LASTEXITCODE -ne 0) {
                        throw "Construcción falló con código $LASTEXITCODE"
                    }
                    
                    Write-Log "Imagen construida exitosamente" -Level "Success"
                } finally {
                    Pop-Location
                    if ($originalBuildkit) {
                        $env:DOCKER_BUILDKIT = $originalBuildkit
                    } else {
                        Remove-Item Env:\DOCKER_BUILDKIT -ErrorAction SilentlyContinue
                    }
                }
                
                $pushSuccess = Push-DockerImage -ImageTag $frontendImageTag -EcrRepository $ecrFrontendUrl -Region $region
                
                if ($pushSuccess) {
                    Write-Log "Frontend desplegado exitosamente" -Level "Success"
                } else {
                    Write-Log "Error: Falló la subida de la imagen del frontend" -Level "Error"
                }
            } catch {
                Write-Log "Error durante el despliegue del frontend: $_" -Level "Error"
            }
        } else {
            Write-Log "Advertencia: No se encontró Dockerfile en frontend" -Level "Warning"
        }
    } else {
        Write-Log "Advertencia: No se encontró el directorio frontend" -Level "Warning"
    }
}

# Actualizar servicios ECS
Write-Step "Paso 4: Actualizar Servicios ECS"

try {
    Write-Log "Actualizando servicio del backend ($ecsApiService)..." -Level "Info"
    
    $updateResult = Invoke-WithRetry -ScriptBlock {
        aws ecs update-service --cluster $ecsCluster --service $ecsApiService --force-new-deployment --region $region --output json 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) {
            throw "Código de salida: $LASTEXITCODE"
        }
        return $true
    } -OperationName "Actualizar Servicio Backend" -MaxAttempts 3
    
    if ($updateResult) {
        Write-Log "  [OK] Backend actualizado" -Level "Success"
    }
    
    # Intentar actualizar frontend (puede que no exista)
    $frontendService = $ecsApiService -replace "-api-service", "-frontend-service"
    Write-Log "Actualizando servicio del frontend ($frontendService)..." -Level "Info"
    
    $frontendUpdate = aws ecs update-service --cluster $ecsCluster --service $frontendService --force-new-deployment --region $region --output json 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Log "  [OK] Frontend actualizado" -Level "Success"
    } else {
        Write-Log "  Advertencia: No se pudo actualizar el servicio del frontend (puede que no exista)" -Level "Warning"
    }
    
    Write-Log "Servicios ECS actualizados" -Level "Success"
} catch {
    Write-Log "Advertencia: Hubo problemas al actualizar los servicios ECS: $_" -Level "Warning"
}

# Verificar deployment
Write-Step "Verificando Deployment"
$deploymentResult = Wait-EcsDeployment -ClusterName $ecsCluster -ServiceName $ecsApiService -Region $region -TimeoutMinutes $DeploymentTimeoutMinutes

if ($deploymentResult.Success) {
    Write-Log "Deployment completado y verificado exitosamente" -Level "Success"
    $finalStatus = $deploymentResult.Status
    Write-Log "Estado final:" -Level "Info"
    Write-Log "  Running: $($finalStatus.RunningCount)/$($finalStatus.DesiredCount)" -Level "Info"
} else {
    Write-Log "Deployment iniciado pero no se pudo verificar completamente dentro del timeout" -Level "Warning"
}

# Mostrar outputs y resumen
Write-Step "Despliegue Completado" "Green"

Write-Log "Obteniendo URLs de los servicios..." -Level "Info"
& $terraformCmd output 2>&1 | ForEach-Object {
    Write-Log "  $_" -Level "Info"
}

Write-Step "Resumen del Despliegue" "Cyan"
Write-Log "[OK] Imágenes Docker reconstruidas" -Level "Success"
Write-Log "[OK] Infraestructura AWS desplegada" -Level "Success"
Write-Log "[OK] Backend desplegado a ECR" -Level "Success"
if ($ecrFrontendUrl) {
    Write-Log "[OK] Frontend desplegado a ECR" -Level "Success"
}
Write-Log "[OK] Servicios ECS actualizados" -Level "Success"
Write-Log "" -Level "Info"
Write-Log "Espera unos minutos para que ECS reemplace las tareas antiguas con las nuevas imágenes." -Level "Info"
Write-Log "" -Level "Info"
Write-Log "Para ver los outputs completos:" -Level "Info"
Write-Log "  cd terraform; terraform output" -Level "Info"
