# ==============================================
# Módulo Común para Despliegues AWS
# ==============================================
# Este módulo contiene funciones compartidas para los scripts de despliegue AWS
# Proporciona logging estructurado, validaciones, retry logic y operaciones comunes
# ==============================================

# Variables del módulo
$script:LogFile = $null
$script:LogLevel = "Info" # Info, Warning, Error, Success, Debug

# ==============================================
# FUNCIONES DE LOGGING
# ==============================================

function Initialize-Logging {
    param(
        [string]$LogFilePath = $null,
        [string]$LogLevel = "Info"
    )
    
    $script:LogLevel = $LogLevel
    
    if ($LogFilePath) {
        $script:LogFile = $LogFilePath
        $logDir = Split-Path -Parent $LogFilePath
        if (-not (Test-Path $logDir)) {
            New-Item -ItemType Directory -Path $logDir -Force | Out-Null
        }
        # Limpiar archivo de log si existe
        if (Test-Path $LogFilePath) {
            Clear-Content $LogFilePath
        }
    }
}

function Write-Log {
    param(
        [Parameter(Mandatory=$true)]
        [string]$Message,
        [ValidateSet("Info", "Warning", "Error", "Success", "Debug")]
        [string]$Level = "Info",
        [switch]$NoConsole
    )
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] [$Level] $Message"
    
    # Determinar si mostrar según nivel
    $shouldShow = $true
    $levels = @("Debug", "Info", "Warning", "Error", "Success")
    $currentLevelIndex = $levels.IndexOf($script:LogLevel)
    $messageLevelIndex = $levels.IndexOf($Level)
    
    if ($messageLevelIndex -lt $currentLevelIndex) {
        $shouldShow = $false
    }
    
    # Escribir a consola
    if (-not $NoConsole -and $shouldShow) {
        $color = switch ($Level) {
            "Info" { "Gray" }
            "Warning" { "Yellow" }
            "Error" { "Red" }
            "Success" { "Green" }
            "Debug" { "Cyan" }
            default { "White" }
        }
        Write-Host $Message -ForegroundColor $color
    }
    
    # Escribir a archivo si está configurado
    if ($script:LogFile) {
        try {
            Add-Content -Path $script:LogFile -Value $logMessage -ErrorAction SilentlyContinue
        } catch {
            # Ignorar errores de escritura de log
        }
    }
}

function Write-Step {
    param(
        [string]$Message,
        [string]$Color = "Cyan"
    )
    Write-Host ""
    Write-Host "========================================" -ForegroundColor $Color
    Write-Host "  $Message" -ForegroundColor $Color
    Write-Host "========================================" -ForegroundColor $Color
    Write-Host ""
    Write-Log "=== $Message ===" -Level "Info"
}

# ==============================================
# FUNCIONES DE VALIDACIÓN
# ==============================================

function Test-Prerequisite {
    param(
        [Parameter(Mandatory=$true)]
        [ValidateSet("Docker", "AwsCli", "Terraform", "DotNet")]
        [string]$Prerequisite
    )
    
    $result = @{
        Installed = $false
        Running = $false
        Version = $null
        Error = $null
    }
    
    try {
        switch ($Prerequisite) {
            "Docker" {
                $docker = Get-Command docker -ErrorAction SilentlyContinue
                if ($docker) {
                    $result.Installed = $true
                    $result.Version = docker --version 2>&1
                    
                    # Verificar que Docker esta corriendo
                    docker ps 2>&1 | Out-Null
                    if ($LASTEXITCODE -eq 0) {
                        $result.Running = $true
                    } else {
                        $result.Error = "Docker no esta ejecutandose"
                    }
                } else {
                    $result.Error = "Docker no esta instalado"
                }
            }
            "AwsCli" {
                $awsCli = Get-Command aws -ErrorAction SilentlyContinue
                if ($awsCli) {
                    $result.Installed = $true
                    $result.Version = aws --version 2>&1
                    
                    # Verificar configuracion
                    $identity = aws sts get-caller-identity 2>&1
                    if ($LASTEXITCODE -eq 0) {
                        $result.Running = $true
                    } else {
                        $result.Error = "AWS CLI no esta configurado correctamente"
                    }
                } else {
                    $result.Error = "AWS CLI no esta instalado"
                }
            }
            "Terraform" {
                # Buscar terraform en PATH primero
                $terraform = Get-Command terraform -ErrorAction SilentlyContinue
                if ($terraform) {
                    $result.Installed = $true
                    $result.Version = (terraform version 2>&1 | Select-Object -First 1)
                    $result.Running = $true
                } else {
                    # Buscar terraform.exe localmente (asumiendo que TerraformPath puede estar disponible)
                    # Si no se encuentra, marcar como no instalado
                    $result.Error = "Terraform no esta instalado"
                }
            }
            "DotNet" {
                $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
                if ($dotnet) {
                    $result.Installed = $true
                    $result.Version = dotnet --version 2>&1
                    $result.Running = $true
                } else {
                    $result.Error = ".NET SDK no esta instalado"
                }
            }
        }
    } catch {
        $result.Error = $_.Exception.Message
    }
    
    return $result
}

function Test-Prerequisites {
    param(
        [string[]]$Required = @("Docker", "AwsCli"),
        [string[]]$Optional = @()
    )
    
    $allOk = $true
    $results = @{}
    
    Write-Log "Verificando prerequisitos..." -Level "Info"
    
    # Verificar requeridos
    foreach ($prereq in $Required) {
        $result = Test-Prerequisite -Prerequisite $prereq
        $results[$prereq] = $result
        
        if ($result.Installed -and $result.Running) {
            Write-Log "  [$prereq] OK - $($result.Version)" -Level "Success"
        } else {
            Write-Log "  [$prereq] FALLO - $($result.Error)" -Level "Error"
            $allOk = $false
        }
    }
    
    # Verificar opcionales
    foreach ($prereq in $Optional) {
        $result = Test-Prerequisite -Prerequisite $prereq
        $results[$prereq] = $result
        
        if ($result.Installed -and $result.Running) {
            Write-Log "  [$prereq] OK (opcional) - $($result.Version)" -Level "Success"
        } else {
            Write-Log "  [$prereq] No disponible (opcional) - $($result.Error)" -Level "Warning"
        }
    }
    
    return @{
        AllOk = $allOk
        Results = $results
    }
}

# ==============================================
# FUNCIONES DE RETRY
# ==============================================

function Invoke-WithRetry {
    param(
        [Parameter(Mandatory=$true)]
        [scriptblock]$ScriptBlock,
        [int]$MaxAttempts = 3,
        [int]$InitialDelaySeconds = 2,
        [int]$MaxDelaySeconds = 30,
        [string]$OperationName = "Operación",
        [switch]$ExponentialBackoff
    )
    
    $attempt = 1
    $delay = $InitialDelaySeconds
    
    while ($attempt -le $MaxAttempts) {
        try {
            Write-Log "$OperationName - Intento $attempt/$MaxAttempts" -Level "Debug"
            
            $result = & $ScriptBlock
            
            if ($LASTEXITCODE -eq 0 -or $result -ne $false) {
                if ($attempt -gt 1) {
                    Write-Log "$OperationName completada exitosamente después de $attempt intentos" -Level "Success"
                }
                return $result
            }
            
            throw "Operación falló con código de salida $LASTEXITCODE"
        } catch {
            $errorMessage = $_.Exception.Message
            
            if ($attempt -lt $MaxAttempts) {
                Write-Log "$OperationName falló (intento $attempt/$MaxAttempts): $errorMessage" -Level "Warning"
                Write-Log "Reintentando en $delay segundos..." -Level "Info"
                Start-Sleep -Seconds $delay
                
                if ($ExponentialBackoff) {
                    $delay = [Math]::Min($delay * 2, $MaxDelaySeconds)
                }
            } else {
                Write-Log "$OperationName falló después de $MaxAttempts intentos: $errorMessage" -Level "Error"
                throw
            }
        }
        
        $attempt++
    }
    
    throw "$OperationName falló después de $MaxAttempts intentos"
}

# ==============================================
# FUNCIONES AWS
# ==============================================

function Get-AwsRegion {
    param(
        [string]$DefaultRegion = "sa-east-1"
    )
    
    $region = aws configure get region 2>&1
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($region)) {
        $region = $DefaultRegion
        Write-Log "Usando región por defecto: $region" -Level "Warning"
    }
    
    return $region.Trim()
}

function Get-TerraformCommand {
    param(
        [string]$TerraformPath = $null
    )
    
    # Buscar terraform en PATH primero
    $terraformCmd = Get-Command terraform -ErrorAction SilentlyContinue
    if ($terraformCmd) {
        return "terraform"
    }
    
    # Si no está en PATH, buscar terraform.exe localmente
    if ($TerraformPath -and (Test-Path $TerraformPath)) {
        $terraformExe = Join-Path $TerraformPath "terraform.exe"
        if (Test-Path $terraformExe) {
            return $terraformExe
        }
    }
    
    return $null
}

function Get-TerraformOutput {
    param(
        [Parameter(Mandatory=$true)]
        [string]$TerraformPath,
        [Parameter(Mandatory=$true)]
        [string]$OutputName,
        [switch]$Required
    )
    
    if (-not (Test-Path $TerraformPath)) {
        if ($Required) {
            throw "Directorio de Terraform no encontrado: $TerraformPath"
        }
        return $null
    }
    
    # Obtener comando de terraform
    $terraformCmd = Get-TerraformCommand -TerraformPath $TerraformPath
    if (-not $terraformCmd) {
        if ($Required) {
            throw "Terraform no encontrado en PATH ni en $TerraformPath"
        }
        return $null
    }
    
    Push-Location $TerraformPath
    try {
        $output = & $terraformCmd output -raw $OutputName 2>&1
        if ($LASTEXITCODE -eq 0 -and -not [string]::IsNullOrWhiteSpace($output)) {
            return $output.Trim()
        }
        
        if ($Required) {
            throw "Output de Terraform '$OutputName' no encontrado o vacio"
        }
        
        return $null
    } finally {
        Pop-Location
    }
}

function Get-AwsResourceInfo {
    param(
        [string]$TerraformPath = $null,
        [string]$Region = $null,
        [string]$ResourceType = "ECR" # ECR, ECS
    )
    
    if (-not $Region) {
        $Region = Get-AwsRegion
    }
    
    $result = @{
        EcrApiUrl = $null
        EcrFrontendUrl = $null
        EcsCluster = $null
        EcsService = $null
        Region = $Region
    }
    
    # Intentar obtener desde Terraform primero
    if ($TerraformPath -and (Test-Path $TerraformPath)) {
        Write-Log "Obteniendo información desde Terraform..." -Level "Debug"
        
        $result.EcrApiUrl = Get-TerraformOutput -TerraformPath $TerraformPath -OutputName "ecr_repository_url" -Required:$false
        $result.EcrFrontendUrl = Get-TerraformOutput -TerraformPath $TerraformPath -OutputName "ecr_frontend_repository_url" -Required:$false
        $result.EcsCluster = Get-TerraformOutput -TerraformPath $TerraformPath -OutputName "ecs_cluster_name" -Required:$false
        $result.EcsService = Get-TerraformOutput -TerraformPath $TerraformPath -OutputName "ecs_service_name" -Required:$false
    }
    
    # Si no se obtuvo desde Terraform, intentar desde AWS CLI
    if (-not $result.EcrApiUrl -or -not $result.EcsCluster) {
        Write-Log "Obteniendo información desde AWS CLI..." -Level "Debug"
        
        # Obtener ECR repositories
        if (-not $result.EcrApiUrl) {
            $reposJson = aws ecr describe-repositories --region $Region --output json 2>&1
            if ($LASTEXITCODE -eq 0) {
                $repos = ($reposJson | ConvertFrom-Json).repositories
                $apiRepo = $repos | Where-Object { $_.repositoryName -like "*api*" -or $_.repositoryName -like "*gatekeep*" } | Select-Object -First 1
                if ($apiRepo) {
                    $result.EcrApiUrl = $apiRepo.repositoryUri
                }
                
                $frontendRepo = $repos | Where-Object { $_.repositoryName -like "*frontend*" } | Select-Object -First 1
                if ($frontendRepo) {
                    $result.EcrFrontendUrl = $frontendRepo.repositoryUri
                }
            }
        }
        
        # Obtener ECS cluster y service
        if (-not $result.EcsCluster) {
            $clustersJson = aws ecs list-clusters --region $Region --output json 2>&1
            if ($LASTEXITCODE -eq 0) {
                $clusters = ($clustersJson | ConvertFrom-Json).clusterArns
                $gatekeepCluster = ($clusters | Where-Object { $_ -like "*gatekeep*" } | Select-Object -First 1)
                if ($gatekeepCluster) {
                    $result.EcsCluster = $gatekeepCluster.Split('/')[-1]
                    
                    # Obtener servicios del cluster
                    $servicesJson = aws ecs list-services --cluster $result.EcsCluster --region $Region --output json 2>&1
                    if ($LASTEXITCODE -eq 0) {
                        $services = ($servicesJson | ConvertFrom-Json).serviceArns
                        $apiService = ($services | Where-Object { $_ -like "*api*" -or $_ -like "*gatekeep*" } | Select-Object -First 1)
                        if ($apiService) {
                            $result.EcsService = $apiService.Split('/')[-1]
                        }
                    }
                }
            }
        }
    }
    
    return $result
}

function Connect-Ecr {
    param(
        [Parameter(Mandatory=$true)]
        [string]$EcrRepositoryUrl,
        [Parameter(Mandatory=$true)]
        [string]$Region
    )
    
    # Extraer URL base de ECR
    $ecrBaseUrl = $EcrRepositoryUrl -replace "/[^/]+$", ""
    if ([string]::IsNullOrWhiteSpace($ecrBaseUrl) -or $ecrBaseUrl -eq $EcrRepositoryUrl) {
        # Fallback: extraer desde regex
        if ($EcrRepositoryUrl -match "(\d+\.dkr\.ecr\.[^.]+\.amazonaws\.com)") {
            $ecrBaseUrl = $matches[1]
        } else {
            throw "No se pudo extraer la URL base de ECR desde: $EcrRepositoryUrl"
        }
    }
    
    Write-Log "Autenticando con ECR: $ecrBaseUrl" -Level "Info"
    
    return Invoke-WithRetry -ScriptBlock {
        # Obtener password de ECR
        $passwordOutput = aws ecr get-login-password --region $Region 2>&1
        if ($LASTEXITCODE -ne 0) {
            throw "Error al obtener token de ECR (codigo $LASTEXITCODE): $passwordOutput"
        }
        
        # Convertir a string si es necesario
        $password = if ($passwordOutput -is [string]) { $passwordOutput } else { $passwordOutput | Out-String -NoNewline }
        
        # Hacer login con docker
        $loginOutput = $password | docker login --username AWS --password-stdin $ecrBaseUrl 2>&1
        if ($LASTEXITCODE -ne 0) {
            $errorMsg = if ($loginOutput) { $loginOutput | Out-String -NoNewline } else { "Error desconocido" }
            throw "Error al autenticar con ECR (codigo $LASTEXITCODE): $errorMsg"
        }
        
        return $true
    } -OperationName "Login ECR" -MaxAttempts 3 -ExponentialBackoff
}

function Get-EcsServiceStatus {
    param(
        [Parameter(Mandatory=$true)]
        [string]$ClusterName,
        [Parameter(Mandatory=$true)]
        [string]$ServiceName,
        [Parameter(Mandatory=$true)]
        [string]$Region
    )
    
    try {
        $serviceJson = aws ecs describe-services --cluster $ClusterName --services $ServiceName --region $Region --output json 2>&1
        if ($LASTEXITCODE -ne 0) {
            return @{
                Found = $false
                Status = "UNKNOWN"
                RunningCount = 0
                DesiredCount = 0
                Error = $serviceJson
            }
        }
        
        $serviceData = ($serviceJson | ConvertFrom-Json).services
        if (-not $serviceData -or $serviceData.Count -eq 0) {
            return @{
                Found = $false
                Status = "NOT_FOUND"
                RunningCount = 0
                DesiredCount = 0
            }
        }
        
        $service = $serviceData[0]
        return @{
            Found = $true
            Status = $service.status
            RunningCount = $service.runningCount
            DesiredCount = $service.desiredCount
            PendingCount = $service.pendingCount
            TaskDefinition = $service.taskDefinition
            Deployments = $service.deployments
        }
    } catch {
        return @{
            Found = $false
            Status = "ERROR"
            RunningCount = 0
            DesiredCount = 0
            Error = $_.Exception.Message
        }
    }
}

function Wait-EcsDeployment {
    param(
        [Parameter(Mandatory=$true)]
        [string]$ClusterName,
        [Parameter(Mandatory=$true)]
        [string]$ServiceName,
        [Parameter(Mandatory=$true)]
        [string]$Region,
        [int]$TimeoutMinutes = 15,
        [int]$CheckIntervalSeconds = 30
    )
    
    Write-Log "Monitoreando deployment de ECS..." -Level "Info"
    Write-Log "Timeout: $TimeoutMinutes minutos, Verificando cada $CheckIntervalSeconds segundos" -Level "Info"
    
    $timeout = (Get-Date).AddMinutes($TimeoutMinutes)
    $lastStatus = $null
    
    while ((Get-Date) -lt $timeout) {
        $status = Get-EcsServiceStatus -ClusterName $ClusterName -ServiceName $ServiceName -Region $Region
        
        if (-not $status.Found) {
            Write-Log "Servicio no encontrado" -Level "Warning"
            Start-Sleep -Seconds $CheckIntervalSeconds
            continue
        }
        
        $deployments = $status.Deployments
        if ($deployments) {
            $primary = $deployments | Where-Object { $_.status -eq "PRIMARY" } | Select-Object -First 1
            
            if ($primary) {
                $statusMessage = "Estado: $($primary.status) | Running: $($primary.runningCount)/$($primary.desiredCount) | Pending: $($primary.pendingCount)"
                
                # Solo mostrar si cambió
                if ($statusMessage -ne $lastStatus) {
                    Write-Log $statusMessage -Level "Info"
                    $lastStatus = $statusMessage
                }
                
                # Verificar si el deployment está completo
                if ($deployments.Count -eq 1 -and 
                    $primary.status -eq "PRIMARY" -and 
                    $primary.runningCount -eq $primary.desiredCount -and
                    $primary.desiredCount -gt 0) {
                    Write-Log "Deployment completado exitosamente" -Level "Success"
                    return @{
                        Success = $true
                        Status = $status
                    }
                }
            }
        }
        
        Start-Sleep -Seconds $CheckIntervalSeconds
    }
    
    Write-Log "Timeout alcanzado. El deployment puede estar aún en progreso." -Level "Warning"
    return @{
        Success = $false
        Status = $status
    }
}

# ==============================================
# FUNCIONES DOCKER
# ==============================================

function Build-DockerImage {
    param(
        [Parameter(Mandatory=$true)]
        [string]$ImageTag,
        [Parameter(Mandatory=$true)]
        [string]$DockerfilePath,
        [Parameter(Mandatory=$true)]
        [string]$BuildContext,
        [int]$TimeoutMinutes = 30
    )
    
    Write-Log "Construyendo imagen Docker: $ImageTag" -Level "Info"
    Write-Log "  Contexto: $BuildContext" -Level "Debug"
    Write-Log "  Dockerfile: $DockerfilePath" -Level "Debug"
    
    Push-Location $BuildContext
    try {
        # Guardar variable de entorno original
        $originalBuildkit = $env:DOCKER_BUILDKIT
        
        # Intentar con builder tradicional primero
        $buildSuccess = $false
        $buildError = $null
        
        try {
            $env:DOCKER_BUILDKIT = "0"
            Write-Log "Intentando construir con builder tradicional..." -Level "Debug"
            
            docker build -t $ImageTag -f $DockerfilePath . 2>&1 | ForEach-Object {
                Write-Log "  $_" -Level "Debug" -NoConsole
            }
            
            if ($LASTEXITCODE -eq 0) {
                $buildSuccess = $true
            } else {
                $buildError = "Builder tradicional falló con código $LASTEXITCODE"
            }
        } catch {
            $buildError = $_.Exception.Message
        }
        
        # Si falló, intentar con buildx
        if (-not $buildSuccess) {
            Write-Log "Builder tradicional falló, intentando con buildx..." -Level "Warning"
            
            try {
                if ($originalBuildkit) {
                    $env:DOCKER_BUILDKIT = $originalBuildkit
                } else {
                    Remove-Item Env:\DOCKER_BUILDKIT -ErrorAction SilentlyContinue
                }
                
                docker buildx build --load -t $ImageTag -f $DockerfilePath . 2>&1 | ForEach-Object {
                    Write-Log "  $_" -Level "Debug" -NoConsole
                }
                
                if ($LASTEXITCODE -eq 0) {
                    $buildSuccess = $true
                } else {
                    throw "Buildx también falló con código $LASTEXITCODE"
                }
            } catch {
                throw "Ambos métodos de construcción fallaron. Último error: $_"
            }
        }
        
        if ($buildSuccess) {
            Write-Log "Imagen construida exitosamente" -Level "Success"
            return $true
        }
        
    } finally {
        Pop-Location
        # Restaurar variable de entorno
        if ($originalBuildkit) {
            $env:DOCKER_BUILDKIT = $originalBuildkit
        } else {
            Remove-Item Env:\DOCKER_BUILDKIT -ErrorAction SilentlyContinue
        }
    }
    
    return $false
}

function Push-DockerImage {
    param(
        [Parameter(Mandatory=$true)]
        [string]$ImageTag,
        [Parameter(Mandatory=$true)]
        [string]$EcrRepository,
        [Parameter(Mandatory=$true)]
        [string]$Region
    )
    
    Write-Log "Subiendo imagen a ECR: $ImageTag" -Level "Info"
    
    return Invoke-WithRetry -ScriptBlock {
        docker push $ImageTag 2>&1 | ForEach-Object {
            Write-Log "  $_" -Level "Debug" -NoConsole
        }
        
        if ($LASTEXITCODE -ne 0) {
            throw "Error al subir imagen (código $LASTEXITCODE)"
        }
        
        return $true
    } -OperationName "Push Docker Image" -MaxAttempts 3 -ExponentialBackoff
}

function Test-DockerImageExists {
    param(
        [Parameter(Mandatory=$true)]
        [string]$ImageTag
    )
    
    $images = docker images --format "{{.Repository}}:{{.Tag}}" 2>&1
    return $images -contains $ImageTag
}

# ==============================================
# FUNCIONES TERRAFORM
# ==============================================

function Test-TerraformInitialized {
    param(
        [Parameter(Mandatory=$true)]
        [string]$TerraformPath
    )
    
    if (-not (Test-Path $TerraformPath)) {
        return $false
    }
    
    $terraformDir = Join-Path $TerraformPath ".terraform"
    return Test-Path $terraformDir
}

# Exportar funciones del modulo
Export-ModuleMember -Function @(
    'Initialize-Logging',
    'Write-Log',
    'Write-Step',
    'Test-Prerequisite',
    'Test-Prerequisites',
    'Invoke-WithRetry',
    'Get-AwsRegion',
    'Get-TerraformCommand',
    'Get-TerraformOutput',
    'Get-AwsResourceInfo',
    'Connect-Ecr',
    'Get-EcsServiceStatus',
    'Wait-EcsDeployment',
    'Build-DockerImage',
    'Push-DockerImage',
    'Test-DockerImageExists',
    'Test-TerraformInitialized'
)

