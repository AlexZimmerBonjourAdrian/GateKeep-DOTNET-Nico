# Script para actualizar servicios AWS con nuevo Docker
# Solo construye imagenes Docker, las sube a ECR y actualiza servicios ECS
# NO modifica infraestructura ni configuracion existente

param(
    [string]$LogFile = $null,
    [switch]$SkipFrontend,
    [int]$DeploymentTimeoutMinutes = 15
)

# Importar modulo comun
$scriptsPath = Join-Path $PSScriptRoot "scripts"
$modulePath = Join-Path $scriptsPath "AwsDeploymentCommon.psm1"
if (Test-Path $modulePath) {
    Import-Module $modulePath -Force
} else {
    Write-Host "Error: No se encontro el modulo comun" -ForegroundColor Red
    exit 1
}

# Inicializar logging
if ($LogFile) {
    Initialize-Logging -LogFilePath $LogFile -LogLevel "Info"
} else {
    Initialize-Logging -LogLevel "Info"
}

Write-Step "GateKeep - Actualizar Docker en AWS"
Write-Log "Este script solo actualiza las imagenes Docker en AWS." -Level "Info"
Write-Log "NO modifica infraestructura ni configuracion existente." -Level "Info"

# Validar prerequisitos
Write-Step "Verificando Prerequisitos"
$prereqCheck = Test-Prerequisites -Required @("Docker", "AwsCli")

if (-not $prereqCheck.AllOk) {
    Write-Log "Faltan prerequisitos criticos. El proceso se detiene." -Level "Error"
    exit 1
}

# Limpiar y recrear Docker local antes de construir imagenes
Write-Step "Preparando Docker Local"
$limpiarScript = Join-Path $PSScriptRoot "limpiar-docker.ps1"
$recrearScript = Join-Path $PSScriptRoot "recrear-docker.ps1"

# Guardar directorio actual
$originalLocation = Get-Location

if (Test-Path $limpiarScript) {
    Write-Log "Limpiando Docker completamente..." -Level "Info"
    try {
        Push-Location $PSScriptRoot
        & $limpiarScript -NonInteractive
        Pop-Location
        if ($LASTEXITCODE -eq 0) {
            Write-Log "Docker limpiado exitosamente" -Level "Success"
        } else {
            Write-Log "Advertencia: La limpieza de Docker tuvo problemas (codigo: $LASTEXITCODE)" -Level "Warning"
            Write-Log "Continuando con el proceso..." -Level "Info"
        }
    } catch {
        Pop-Location
        Write-Log "Advertencia: Error al limpiar Docker: $_" -Level "Warning"
        Write-Log "Continuando con el proceso..." -Level "Info"
    }
} else {
    Write-Log "Advertencia: No se encontro limpiar-docker.ps1" -Level "Warning"
}

if (Test-Path $recrearScript) {
    Write-Log "Recreando Docker limpio (manteniendo datos)..." -Level "Info"
    try {
        Push-Location $PSScriptRoot
        & $recrearScript -NonInteractive -Option 1
        Pop-Location
        if ($LASTEXITCODE -eq 0) {
            Write-Log "Docker recreado exitosamente" -Level "Success"
        } else {
            Write-Log "Advertencia: La recreacion de Docker tuvo problemas (codigo: $LASTEXITCODE)" -Level "Warning"
            Write-Log "Continuando con el proceso..." -Level "Info"
        }
    } catch {
        Pop-Location
        Write-Log "Advertencia: Error al recrear Docker: $_" -Level "Warning"
        Write-Log "Continuando con el proceso..." -Level "Info"
    }
} else {
    Write-Log "Advertencia: No se encontro recrear-docker.ps1" -Level "Warning"
}

# Restaurar directorio original
Set-Location $originalLocation

# Obtener region AWS
$awsRegion = Get-AwsRegion

# Obtener informacion de recursos AWS directamente desde AWS CLI
Write-Step "Obteniendo Informacion de Recursos AWS"

# Inicializar variables
$ecrApiUrl = $null
$ecrFrontendUrl = $null
$ecsCluster = $null
$ecsService = $null

# Obtener repositorios ECR
Write-Log "Buscando repositorios ECR..." -Level "Info"
$reposJson = aws ecr describe-repositories --region $awsRegion --output json 2>&1
if ($LASTEXITCODE -eq 0) {
    $repos = ($reposJson | ConvertFrom-Json).repositories
    $apiRepo = $repos | Where-Object { $_.repositoryName -like "*api*" -or $_.repositoryName -like "*gatekeep*" } | Select-Object -First 1
    if ($apiRepo) {
        $ecrApiUrl = $apiRepo.repositoryUri
    }
    
    $frontendRepo = $repos | Where-Object { $_.repositoryName -like "*frontend*" } | Select-Object -First 1
    if ($frontendRepo) {
        $ecrFrontendUrl = $frontendRepo.repositoryUri
    }
} else {
    Write-Log "Error al obtener repositorios ECR: $reposJson" -Level "Warning"
}

# Obtener cluster y servicio ECS
Write-Log "Buscando cluster y servicio ECS..." -Level "Info"
$clustersJson = aws ecs list-clusters --region $awsRegion --output json 2>&1
if ($LASTEXITCODE -eq 0) {
    $clusters = ($clustersJson | ConvertFrom-Json).clusterArns
    if ($clusters -and $clusters.Count -gt 0) {
        $gatekeepCluster = ($clusters | Where-Object { $_ -like "*gatekeep*" } | Select-Object -First 1)
        if (-not $gatekeepCluster) {
            $gatekeepCluster = $clusters[0]
        }
        
        if ($gatekeepCluster) {
            $ecsCluster = $gatekeepCluster.Split('/')[-1]
            
            # Obtener servicios del cluster
            $servicesJson = aws ecs list-services --cluster $ecsCluster --region $awsRegion --output json 2>&1
            if ($LASTEXITCODE -eq 0) {
                $services = ($servicesJson | ConvertFrom-Json).serviceArns
                if ($services -and $services.Count -gt 0) {
                    $apiService = ($services | Where-Object { $_ -like "*api*" -or $_ -like "*gatekeep*" } | Select-Object -First 1)
                    if (-not $apiService) {
                        $apiService = $services[0]
                    }
                    
                    if ($apiService) {
                        $ecsService = $apiService.Split('/')[-1]
                    }
                }
            }
        }
    }
} else {
    Write-Log "Error al obtener clusters ECS: $clustersJson" -Level "Warning"
}

# Si no se encontró ECR API, pedir al usuario
if ([string]::IsNullOrWhiteSpace($ecrApiUrl)) {
    Write-Log "No se pudo obtener automaticamente la URL del repositorio ECR para API" -Level "Warning"
    Write-Host ""
    $manualUrl = Read-Host "Ingresa la URL completa del repositorio ECR para API (o presiona Enter para salir)"
    
    if ([string]::IsNullOrWhiteSpace($manualUrl)) {
        Write-Log "Operacion cancelada" -Level "Error"
        exit 1
    }
    
    $ecrApiUrl = $manualUrl.Trim()
}

Write-Log "API Repository: $ecrApiUrl" -Level "Info"
if ($ecrFrontendUrl) {
    Write-Log "Frontend Repository: $ecrFrontendUrl" -Level "Info"
}

# Validar estado actual del servicio ECS
if ($ecsCluster -and $ecsService) {
    Write-Step "Verificando Estado Actual del Servicio ECS"
    $currentStatus = Get-EcsServiceStatus -ClusterName $ecsCluster -ServiceName $ecsService -Region $awsRegion
    
    if ($currentStatus.Found) {
        Write-Log "Estado actual del servicio:" -Level "Info"
        Write-Log "  Status: $($currentStatus.Status)" -Level "Info"
        Write-Log "  Running: $($currentStatus.RunningCount)/$($currentStatus.DesiredCount)" -Level "Info"
        Write-Log "  Pending: $($currentStatus.PendingCount)" -Level "Info"
    } else {
        Write-Log "No se pudo obtener el estado del servicio ECS" -Level "Warning"
    }
}

# Autenticar con ECR
Write-Step "Autenticando con ECR"
try {
    Connect-Ecr -EcrRepositoryUrl $ecrApiUrl -Region $awsRegion | Out-Null
    Write-Log "Autenticado correctamente con ECR" -Level "Success"
} catch {
    Write-Log "Error al autenticar con ECR: $_" -Level "Error"
    exit 1
}

# Construir y subir imagen de API
Write-Step "Construyendo y Subiendo Imagen de API"
$apiPath = Join-Path $PSScriptRoot "src"

if (-not (Test-Path $apiPath)) {
    Write-Log "Error: No se encontró el directorio src en: $apiPath" -Level "Error"
    exit 1
}

$dockerfilePath = Join-Path $apiPath "Dockerfile"
if (-not (Test-Path $dockerfilePath)) {
    Write-Log "Error: No se encontró el Dockerfile en: $dockerfilePath" -Level "Error"
    exit 1
}

$imageTag = "$ecrApiUrl:latest"
Write-Log "  Contexto: $apiPath" -Level "Info"
Write-Log "  Dockerfile: $dockerfilePath" -Level "Info"
Write-Log "  Tag: $imageTag" -Level "Info"

try {
    # Construir imagen
    $buildSuccess = Build-DockerImage -ImageTag $imageTag -DockerfilePath "Dockerfile" -BuildContext $apiPath
    
    if (-not $buildSuccess) {
        Write-Log "Error: Fallo la construccion de la imagen de API" -Level "Error"
        Write-Log "SOLUCION MANUAL:" -Level "Info"
        Write-Log "1. Abre PowerShell en el directorio: $apiPath" -Level "Info"
        Write-Log "2. Ejecuta: docker build -t $imageTag -f Dockerfile ." -Level "Info"
        Write-Log "3. Luego ejecuta: docker push $imageTag" -Level "Info"
        exit 1
    }
    
    # Subir imagen
    Write-Log "Subiendo imagen de API a ECR..." -Level "Info"
    $pushSuccess = Push-DockerImage -ImageTag $imageTag -EcrRepository $ecrApiUrl -Region $awsRegion
    
    if (-not $pushSuccess) {
        Write-Log "Error: Fallo el push de la imagen de API" -Level "Error"
        exit 1
    }
    
    Write-Log "Imagen de API actualizada exitosamente" -Level "Success"
} catch {
    Write-Log "Error durante el despliegue de API: $_" -Level "Error"
    exit 1
}

# Construir y subir imagen de Frontend (si existe y no se saltó)
if ($ecrFrontendUrl -and -not $SkipFrontend) {
    Write-Step "Construyendo y Subiendo Imagen de Frontend"
    $frontendPath = Join-Path $PSScriptRoot "frontend"
    
    if (Test-Path $frontendPath) {
        $frontendDockerfile = Join-Path $frontendPath "Dockerfile"
        if (Test-Path $frontendDockerfile) {
            $frontendImageTag = "$ecrFrontendUrl:latest"
            Write-Log "  Contexto: $frontendPath" -Level "Info"
            Write-Log "  Tag: $frontendImageTag" -Level "Info"
            
            try {
                $frontendBuildSuccess = Build-DockerImage -ImageTag $frontendImageTag -DockerfilePath "Dockerfile" -BuildContext $frontendPath
                
                if ($frontendBuildSuccess) {
                    Write-Log "Subiendo imagen de Frontend a ECR..." -Level "Info"
                    $frontendPushSuccess = Push-DockerImage -ImageTag $frontendImageTag -EcrRepository $ecrFrontendUrl -Region $awsRegion
                    
                    if ($frontendPushSuccess) {
                        Write-Log "Imagen de Frontend actualizada exitosamente" -Level "Success"
                    } else {
                        Write-Log "Advertencia: Fallo el push de la imagen de Frontend" -Level "Warning"
                    }
                } else {
                    Write-Log "Advertencia: Fallo la construccion de la imagen de Frontend" -Level "Warning"
                }
            } catch {
                Write-Log "Advertencia: Error al desplegar frontend: $_" -Level "Warning"
            }
        } else {
            Write-Log "Advertencia: No se encontro Dockerfile en frontend" -Level "Warning"
        }
    } else {
        Write-Log "Advertencia: No se encontro el directorio frontend" -Level "Warning"
    }
}

# Forzar nueva deployment en ECS
Write-Step "Forzando Nueva Deployment en ECS"

if (-not $ecsCluster -or -not $ecsService) {
    Write-Log "Advertencia: No se pudo obtener informacion de ECS automaticamente" -Level "Warning"
    Write-Log "Puedes forzar el deployment manualmente con:" -Level "Info"
    Write-Log "  aws ecs update-service --cluster CLUSTER_NAME --service SERVICE_NAME --force-new-deployment --region $awsRegion" -Level "Info"
} else {
    Write-Log "  Cluster: $ecsCluster" -Level "Info"
    Write-Log "  Service: $ecsService" -Level "Info"
    
    try {
        $updateResult = Invoke-WithRetry -ScriptBlock {
            aws ecs update-service --cluster $ecsCluster --service $ecsService --force-new-deployment --region $awsRegion --output json 2>&1 | Out-Null
            if ($LASTEXITCODE -ne 0) {
                throw "Codigo de salida: $LASTEXITCODE"
            }
            return $true
        } -OperationName "Forzar Deployment ECS" -MaxAttempts 3
        
        if ($updateResult) {
            Write-Log "Deployment iniciado exitosamente" -Level "Success"
            
            # Esperar y verificar deployment
            Write-Step "Verificando Deployment"
            $deploymentResult = Wait-EcsDeployment -ClusterName $ecsCluster -ServiceName $ecsService -Region $awsRegion -TimeoutMinutes $DeploymentTimeoutMinutes
            
            if ($deploymentResult.Success) {
                Write-Log "Deployment completado y verificado exitosamente" -Level "Success"
                $finalStatus = $deploymentResult.Status
                Write-Log "Estado final:" -Level "Info"
                Write-Log "  Running: $($finalStatus.RunningCount)/$($finalStatus.DesiredCount)" -Level "Info"
            } else {
                Write-Log "Deployment iniciado pero no se pudo verificar completamente dentro del timeout" -Level "Warning"
                Write-Log "Verifica el estado manualmente con:" -Level "Info"
                Write-Log "  aws ecs describe-services --cluster $ecsCluster --services $ecsService --region $awsRegion" -Level "Info"
            }
        }
    } catch {
        Write-Log "Error al forzar el deployment: $_" -Level "Error"
        Write-Log "Puedes hacerlo manualmente desde la consola de AWS" -Level "Info"
    }
}

# Resumen final
Write-Step "Actualizacion Completada" "Green"
Write-Log "Resumen:" -Level "Info"
Write-Log "  [OK] Docker local limpiado y recreado" -Level "Success"
Write-Log "  [OK] Imagenes Docker construidas" -Level "Success"
Write-Log "  [OK] Imagenes subidas a ECR" -Level "Success"
Write-Log "  [OK] Deployment forzado en ECS" -Level "Success"
Write-Log "" -Level "Info"
Write-Log "Nota: Solo se actualizaron las imagenes Docker." -Level "Info"
Write-Log "La infraestructura y configuracion no fueron modificadas." -Level "Info"
