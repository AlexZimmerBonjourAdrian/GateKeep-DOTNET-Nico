# ==============================================
# Script de Despliegue Automático a AWS
# ==============================================
# Este script automatiza el despliegue completo a AWS incluyendo:
# - Construcción de imágenes Docker (API y Frontend)
# - Subida a ECR
# - Ejecución de migraciones de base de datos
# - Actualización de variables de entorno en ECS
# - Deployment en ECS
# ==============================================

param(
    [switch]$SkipMigrations,
    [switch]$SkipFrontend,
    [switch]$DryRun,
    [string]$EnvFile = ".env",
    [string]$LogFile = $null
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

# Configurar manejo de errores (permitir continuar en algunos casos)
$ErrorActionPreference = "Continue"

# ==============================================
# FUNCIÓN: Cargar Variables desde .env
# ==============================================

function Load-EnvFile {
    param([string]$FilePath)
    
    Write-Log "Cargando variables desde: $FilePath" -Level "Info"
    
    if (-not (Test-Path $FilePath)) {
        Write-Log "Error: No se encontró el archivo .env en: $FilePath" -Level "Error"
        Write-Log "Crea el archivo .env basándote en env.example" -Level "Info"
        Write-Log "Ejecuta: cp env.example .env" -Level "Info"
        exit 1
    }
    
    $envVars = @{}
    $lines = Get-Content $FilePath
    
    foreach ($line in $lines) {
        # Ignorar comentarios y líneas vacías
        if ($line -match '^\s*#' -or $line -match '^\s*$') {
            continue
        }
        
        # Parsear línea KEY=VALUE
        if ($line -match '^([^=]+)=(.*)$') {
            $key = $matches[1].Trim()
            $value = $matches[2].Trim()
            
            # Remover comillas si existen
            $value = $value -replace '^["\']|["\']$', ''
            
            $envVars[$key] = $value
        }
    }
    
    # Validar variables críticas
    $requiredVars = @(
        "AWS_ACCESS_KEY_ID",
        "AWS_SECRET_ACCESS_KEY",
        "AWS_REGION",
        "ECR_API_REPOSITORY",
        "ECS_CLUSTER_NAME",
        "ECS_SERVICE_NAME_API"
    )
    
    $missingVars = @()
    foreach ($var in $requiredVars) {
        if (-not $envVars.ContainsKey($var) -or [string]::IsNullOrWhiteSpace($envVars[$var])) {
            $missingVars += $var
        }
    }
    
    if ($missingVars.Count -gt 0) {
        Write-Log "Error: Faltan variables requeridas en .env:" -Level "Error"
        foreach ($var in $missingVars) {
            Write-Log "  - $var" -Level "Error"
        }
        exit 1
    }
    
    Write-Log "Variables cargadas correctamente ($($envVars.Count) variables)" -Level "Success"
    return $envVars
}

# ==============================================
# FUNCIÓN: Ejecutar Migraciones de Base de Datos
# ==============================================

function Run-DatabaseMigrations {
    param([hashtable]$EnvVars)
    
    Write-Step "Ejecutando Migraciones de Base de Datos"
    
    if ($DryRun) {
        Write-Log "[DRY RUN] Se ejecutarían migraciones de base de datos" -Level "Warning"
        return $true
    }
    
    $apiPath = Join-Path $PSScriptRoot "src" "GateKeep.Api"
    
    if (-not (Test-Path $apiPath)) {
        Write-Log "No se encontró el directorio del proyecto API: $apiPath" -Level "Error"
        return $false
    }
    
    # Validar que tenemos las variables necesarias
    $requiredDbVars = @("DB_HOST", "DB_PORT", "DB_NAME", "DB_USER", "DB_PASSWORD")
    foreach ($var in $requiredDbVars) {
        if (-not $EnvVars.ContainsKey($var) -or [string]::IsNullOrWhiteSpace($EnvVars[$var])) {
            Write-Log "Error: Falta variable requerida para migraciones: $var" -Level "Error"
            return $false
        }
    }
    
    # Construir connection string
    $connectionString = "Host=$($EnvVars['DB_HOST']);Port=$($EnvVars['DB_PORT']);Database=$($EnvVars['DB_NAME']);Username=$($EnvVars['DB_USER']);Password=$($EnvVars['DB_PASSWORD']);"
    
    Write-Log "Conectando a base de datos: $($EnvVars['DB_HOST']):$($EnvVars['DB_PORT'])/$($EnvVars['DB_NAME'])" -Level "Info"
    
    Push-Location $apiPath
    try {
        # Ejecutar migraciones usando dotnet ef
        Write-Log "Ejecutando migraciones..." -Level "Info"
        
        $env:DATABASE__HOST = $EnvVars['DB_HOST']
        $env:DATABASE__PORT = $EnvVars['DB_PORT']
        $env:DATABASE__NAME = $EnvVars['DB_NAME']
        $env:DATABASE__USER = $EnvVars['DB_USER']
        $env:DATABASE__PASSWORD = $EnvVars['DB_PASSWORD']
        
        $migrationResult = Invoke-WithRetry -ScriptBlock {
            dotnet ef database update --connection $connectionString 2>&1 | ForEach-Object {
                Write-Log "  $_" -Level "Debug" -NoConsole
            }
            
            if ($LASTEXITCODE -ne 0) {
                throw "Migraciones fallaron con código $LASTEXITCODE"
            }
            
            return $true
        } -OperationName "Ejecutar Migraciones" -MaxAttempts 2
        
        if ($migrationResult) {
            Write-Log "Migraciones ejecutadas exitosamente" -Level "Success"
            return $true
        }
        
        return $false
    } catch {
        Write-Log "Error al ejecutar migraciones: $_" -Level "Error"
        return $false
    } finally {
        Pop-Location
        Remove-Item Env:\DATABASE__HOST -ErrorAction SilentlyContinue
        Remove-Item Env:\DATABASE__PORT -ErrorAction SilentlyContinue
        Remove-Item Env:\DATABASE__NAME -ErrorAction SilentlyContinue
        Remove-Item Env:\DATABASE__USER -ErrorAction SilentlyContinue
        Remove-Item Env:\DATABASE__PASSWORD -ErrorAction SilentlyContinue
    }
}

# ==============================================
# FUNCIÓN: Actualizar Variables de Entorno en ECS
# ==============================================

function Update-ECSEnvironmentVariables {
    param(
        [string]$ClusterName,
        [string]$ServiceName,
        [string]$Region,
        [string]$EnvVarsFile = "ecs-env-vars.json"
    )
    
    Write-Step "Actualizando Variables de Entorno en ECS"
    
    if (-not (Test-Path $EnvVarsFile)) {
        Write-Log "No se encontró el archivo de variables de entorno: $EnvVarsFile" -Level "Warning"
        Write-Log "Se usará la configuración actual de ECS" -Level "Info"
        return $true
    }
    
    if ($DryRun) {
        Write-Log "[DRY RUN] Se actualizarían variables de entorno en ECS" -Level "Warning"
        return $true
    }
    
    try {
        # Obtener task definition actual
        Write-Log "Obteniendo task definition actual..." -Level "Info"
        $taskDefJsonOutput = aws ecs describe-services --cluster $ClusterName --services $ServiceName --region $Region --output json 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Log "Error al obtener información del servicio: $taskDefJsonOutput" -Level "Error"
            return $false
        }
        
        $taskDefJson = $taskDefJsonOutput | ConvertFrom-Json
        
        if (-not $taskDefJson.services -or $taskDefJson.services.Count -eq 0) {
            Write-Log "No se encontró el servicio: $ServiceName" -Level "Error"
            return $false
        }
        
        $currentTaskDefArn = $taskDefJson.services[0].taskDefinition
        Write-Log "Task definition actual: $currentTaskDefArn" -Level "Info"
        
        # Obtener task definition completa
        $taskDefName = $currentTaskDefArn.Split('/')[-1]
        $taskDefFullOutput = aws ecs describe-task-definition --task-definition $taskDefName --region $Region --output json 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Log "Error al obtener task definition: $taskDefFullOutput" -Level "Error"
            return $false
        }
        
        $taskDefFull = $taskDefFullOutput | ConvertFrom-Json
        $taskDef = $taskDefFull.taskDefinition
        
        # Leer nuevas variables de entorno
        try {
            $newEnvVars = Get-Content $EnvVarsFile | ConvertFrom-Json
        } catch {
            Write-Log "Error al leer archivo de variables de entorno: $_" -Level "Error"
            return $false
        }
        
        # Actualizar variables de entorno en el contenedor (asumiendo primer contenedor)
        if ($taskDef.containerDefinitions.Count -gt 0) {
            $containerDef = $taskDef.containerDefinitions[0]
            
            # Preservar variables existentes que no están en el archivo de configuración
            $existingEnv = @{}
            if ($containerDef.environment) {
                foreach ($env in $containerDef.environment) {
                    $existingEnv[$env.name] = $env.value
                }
            }
            
            # Actualizar con nuevas variables
            $updatedEnv = @()
            if ($newEnvVars.environment) {
                foreach ($envVar in $newEnvVars.environment) {
                    $updatedEnv += @{
                        name = $envVar.name
                        value = $envVar.value
                    }
                }
            }
            
            # Agregar variables existentes que no están en la nueva configuración (preservar)
            foreach ($key in $existingEnv.Keys) {
                $exists = $false
                foreach ($newVar in $updatedEnv) {
                    if ($newVar.name -eq $key) {
                        $exists = $true
                        break
                    }
                }
                if (-not $exists) {
                    $updatedEnv += @{
                        name = $key
                        value = $existingEnv[$key]
                    }
                }
            }
            
            $containerDef.environment = $updatedEnv
            
            # Actualizar secrets (preservar existentes)
            $existingSecrets = @{}
            if ($containerDef.secrets) {
                foreach ($secret in $containerDef.secrets) {
                    $existingSecrets[$secret.name] = $secret.valueFrom
                }
            }
            
            $updatedSecrets = @()
            if ($newEnvVars.secrets) {
                foreach ($secret in $newEnvVars.secrets) {
                    $updatedSecrets += @{
                        name = $secret.name
                        valueFrom = $secret.valueFrom
                    }
                }
            }
            
            # Agregar secrets existentes que no están en la nueva configuración
            foreach ($key in $existingSecrets.Keys) {
                $exists = $false
                foreach ($newSecret in $updatedSecrets) {
                    if ($newSecret.name -eq $key) {
                        $exists = $true
                        break
                    }
                }
                if (-not $exists) {
                    $updatedSecrets += @{
                        name = $key
                        valueFrom = $existingSecrets[$key]
                    }
                }
            }
            
            $containerDef.secrets = $updatedSecrets
        }
        
        # Crear nueva task definition
        $newTaskDef = @{
            family = $taskDef.family
            containerDefinitions = $taskDef.containerDefinitions
            requiresCompatibilities = $taskDef.requiresCompatibilities
            networkMode = $taskDef.networkMode
            cpu = $taskDef.cpu
            memory = $taskDef.memory
            executionRoleArn = $taskDef.executionRoleArn
            taskRoleArn = $taskDef.taskRoleArn
        } | ConvertTo-Json -Depth 10
        
        # Registrar nueva task definition
        Write-Log "Registrando nueva task definition..." -Level "Info"
        $newTaskDefResult = Invoke-WithRetry -ScriptBlock {
            $result = aws ecs register-task-definition --cli-input-json $newTaskDef --region $Region --output json | ConvertFrom-Json
            if (-not $result.taskDefinition) {
                throw "No se pudo registrar la task definition"
            }
            return $result
        } -OperationName "Registrar Task Definition" -MaxAttempts 3
        
        $newTaskDefArn = $newTaskDefResult.taskDefinition.taskDefinitionArn
        Write-Log "Nueva task definition registrada: $newTaskDefArn" -Level "Success"
        
        # Actualizar servicio con nueva task definition
        Write-Log "Actualizando servicio con nueva task definition..." -Level "Info"
        $updateResult = Invoke-WithRetry -ScriptBlock {
            aws ecs update-service --cluster $ClusterName --service $ServiceName --task-definition $newTaskDefArn --region $Region --output json 2>&1 | Out-Null
            if ($LASTEXITCODE -ne 0) {
                throw "Error al actualizar el servicio"
            }
            return $true
        } -OperationName "Actualizar Servicio" -MaxAttempts 3
        
        if ($updateResult) {
            Write-Log "Variables de entorno actualizadas exitosamente" -Level "Success"
            return $true
        }
        
        return $false
        
    } catch {
        Write-Log "Error al actualizar variables de entorno: $_" -Level "Error"
        return $false
    }
}

# ==============================================
# FUNCIÓN: Actualizar Configuración AWS (Preservando existente)
# ==============================================

function Update-AWSConfig {
    param(
        [hashtable]$EnvVars,
        [string]$Region
    )
    
    Write-Step "Actualizando Configuración AWS (Preservando Existente)"
    
    if ($DryRun) {
        Write-Log "[DRY RUN] Se actualizaría la configuración de AWS" -Level "Warning"
        return $true
    }
    
    try {
        Write-Log "Verificando configuración actual en AWS..." -Level "Info"
        
        # Verificar y actualizar Parameter Store si es necesario
        $ssmParams = @(
            @{ Name = "/gatekeep/db/host"; Value = $EnvVars['DB_HOST'] }
            @{ Name = "/gatekeep/db/port"; Value = $EnvVars['DB_PORT'] }
            @{ Name = "/gatekeep/db/name"; Value = $EnvVars['DB_NAME'] }
            @{ Name = "/gatekeep/db/username"; Value = $EnvVars['DB_USER'] }
        )
        
        foreach ($param in $ssmParams) {
            if (-not $param.Value) {
                continue
            }
            
            Write-Log "Verificando parámetro: $($param.Name)" -Level "Debug"
            
            # Intentar obtener parámetro existente
            $existingJson = aws ssm get-parameter --name $param.Name --region $Region --output json 2>&1
            if ($LASTEXITCODE -eq 0) {
                try {
                    $existing = $existingJson | ConvertFrom-Json
                    if ($existing -and $existing.Parameter -and $existing.Parameter.Value -ne $param.Value) {
                        Write-Log "  Actualizando: $($param.Name)" -Level "Info"
                        aws ssm put-parameter --name $param.Name --value $param.Value --type String --overwrite --region $Region | Out-Null
                        if ($LASTEXITCODE -eq 0) {
                            Write-Log "  Parámetro actualizado" -Level "Success"
                        } else {
                            Write-Log "  No se pudo actualizar el parámetro" -Level "Warning"
                        }
                    } else {
                        Write-Log "  Sin cambios necesarios" -Level "Debug"
                    }
                } catch {
                    Write-Log "  Error al procesar parámetro existente: $_" -Level "Warning"
                }
            } else {
                Write-Log "  Parámetro no existe (se creará si es necesario)" -Level "Debug"
            }
        }
        
        # Verificar Secrets Manager (solo verificar, no actualizar por seguridad)
        Write-Log "Verificando secrets en Secrets Manager..." -Level "Info"
        $secrets = @(
            "gatekeep/db/password",
            "gatekeep/jwt/key",
            "gatekeep/mongodb/connection"
        )
        
        foreach ($secretName in $secrets) {
            $secretExists = aws secretsmanager describe-secret --secret-id $secretName --region $Region --output json 2>&1
            if ($LASTEXITCODE -eq 0) {
                Write-Log "  Secret existe: $secretName" -Level "Success"
            } else {
                Write-Log "  Secret no encontrado: $secretName (se mantendrá la configuración actual)" -Level "Warning"
            }
        }
        
        Write-Log "Configuración de AWS verificada y actualizada (preservando existente)" -Level "Success"
        return $true
        
    } catch {
        Write-Log "Advertencia al actualizar configuración: $_" -Level "Warning"
        Write-Log "Se continuará con la configuración actual" -Level "Info"
        return $true
    }
}

# ==============================================
# FLUJO PRINCIPAL
# ==============================================

Write-Step "GateKeep - Despliegue Automático a AWS"

if ($DryRun) {
    Write-Log "MODO DRY RUN: No se realizarán cambios reales" -Level "Warning"
}

# 1. Cargar variables de entorno
$envVars = Load-EnvFile -FilePath (Join-Path $PSScriptRoot $EnvFile)

# 2. Configurar credenciales AWS
$env:AWS_ACCESS_KEY_ID = $envVars['AWS_ACCESS_KEY_ID']
$env:AWS_SECRET_ACCESS_KEY = $envVars['AWS_SECRET_ACCESS_KEY']
$env:AWS_REGION = $envVars['AWS_REGION']

# 3. Verificar prerequisitos
$prereqCheck = Test-Prerequisites -Required @("Docker", "AwsCli")
if (-not $prereqCheck.AllOk) {
    Write-Log "Faltan prerequisitos. Por favor instálalos antes de continuar." -Level "Error"
    exit 1
}

# Verificar .NET SDK si se van a ejecutar migraciones
if ($envVars['RUN_MIGRATIONS'] -eq "true" -and -not $SkipMigrations) {
    $dotnetCheck = Test-Prerequisite -Prerequisite "DotNet"
    if (-not $dotnetCheck.Installed) {
        Write-Log ".NET SDK no está instalado (necesario para migraciones)" -Level "Warning"
        Write-Log "Las migraciones se saltarán" -Level "Info"
        $SkipMigrations = $true
    }
}

# 4. Actualizar configuración AWS (preservando existente)
Update-AWSConfig -EnvVars $envVars -Region $envVars['AWS_REGION']

# 5. Autenticar con ECR
Write-Step "Autenticando con ECR"
try {
    Connect-Ecr -EcrRepositoryUrl $envVars['ECR_API_REPOSITORY'] -Region $envVars['AWS_REGION'] | Out-Null
    Write-Log "Autenticado correctamente con ECR" -Level "Success"
} catch {
    Write-Log "Error: No se pudo autenticar con ECR. El proceso se detiene." -Level "Error"
    exit 1
}

# 6. Construir y subir imagen de API
Write-Step "Construyendo y Subiendo Imagen de API"

$apiImageTag = "$($envVars['ECR_API_REPOSITORY']):latest"
$apiPath = Join-Path $PSScriptRoot "src"

if ($DryRun) {
    Write-Log "[DRY RUN] Se construiría: docker build -t $apiImageTag -f Dockerfile $apiPath" -Level "Warning"
} else {
    if (-not (Build-DockerImage -ImageTag $apiImageTag -DockerfilePath "Dockerfile" -BuildContext $apiPath)) {
        exit 1
    }
    
    if (-not (Push-DockerImage -ImageTag $apiImageTag -EcrRepository $envVars['ECR_API_REPOSITORY'] -Region $envVars['AWS_REGION'])) {
        exit 1
    }
}

# 7. Construir y subir imagen de Frontend (si está habilitado)
if ($envVars['DEPLOY_FRONTEND'] -eq "true" -and -not $SkipFrontend) {
    Write-Step "Construyendo y Subiendo Imagen de Frontend"
    
    if ($envVars.ContainsKey('ECR_FRONTEND_REPOSITORY') -and -not [string]::IsNullOrWhiteSpace($envVars['ECR_FRONTEND_REPOSITORY'])) {
        $frontendImageTag = "$($envVars['ECR_FRONTEND_REPOSITORY']):latest"
        $frontendPath = Join-Path $PSScriptRoot "frontend"
        $frontendDockerfile = Join-Path $frontendPath "Dockerfile"
        
        if (Test-Path $frontendDockerfile) {
            if ($DryRun) {
                Write-Log "[DRY RUN] Se construiría frontend" -Level "Warning"
            } else {
                if (-not (Build-DockerImage -ImageTag $frontendImageTag -DockerfilePath "Dockerfile" -BuildContext $frontendPath)) {
                    Write-Log "Error al construir frontend, continuando con API..." -Level "Warning"
                } else {
                    if (-not (Push-DockerImage -ImageTag $frontendImageTag -EcrRepository $envVars['ECR_FRONTEND_REPOSITORY'] -Region $envVars['AWS_REGION'])) {
                        Write-Log "Error al subir frontend, continuando con API..." -Level "Warning"
                    }
                }
            }
        } else {
            Write-Log "Dockerfile de frontend no encontrado, saltando..." -Level "Warning"
        }
    }
}

# 8. Ejecutar migraciones (si está habilitado)
if ($envVars['RUN_MIGRATIONS'] -eq "true" -and -not $SkipMigrations) {
    if (-not (Run-DatabaseMigrations -EnvVars $envVars)) {
        Write-Log "Error al ejecutar migraciones, pero continuando con el deployment..." -Level "Warning"
    }
} else {
    Write-Log "Migraciones deshabilitadas o saltadas" -Level "Info"
}

# 9. Actualizar variables de entorno en ECS (si está habilitado)
if ($envVars['UPDATE_ENV_VARS'] -eq "true") {
    if (-not (Update-ECSEnvironmentVariables -ClusterName $envVars['ECS_CLUSTER_NAME'] -ServiceName $envVars['ECS_SERVICE_NAME_API'] -Region $envVars['AWS_REGION'])) {
        Write-Log "Error al actualizar variables de entorno, pero continuando con el deployment..." -Level "Warning"
    }
}

# 10. Forzar nuevo deployment
Write-Step "Forzando Nuevo Deployment en ECS"

if ($DryRun) {
    Write-Log "[DRY RUN] Se forzaría nuevo deployment en ECS" -Level "Warning"
} else {
    try {
        $forceResult = Invoke-WithRetry -ScriptBlock {
            $resultOutput = aws ecs update-service --cluster $envVars['ECS_CLUSTER_NAME'] --service $envVars['ECS_SERVICE_NAME_API'] --force-new-deployment --region $envVars['AWS_REGION'] --output json 2>&1
            if ($LASTEXITCODE -ne 0) {
                throw "Error al forzar el deployment: $resultOutput"
            }
            
            $result = $resultOutput | ConvertFrom-Json
            if (-not $result.service) {
                throw "Respuesta inválida de ECS"
            }
            
            return $result
        } -OperationName "Forzar Deployment ECS" -MaxAttempts 3
        
        Write-Log "Deployment iniciado" -Level "Success"
        Write-Log "Service ARN: $($forceResult.service.serviceArn)" -Level "Info"
    } catch {
        Write-Log "Error al forzar el deployment: $_" -Level "Error"
        exit 1
    }
}

# 11. Esperar y verificar deployment
$timeout = if ($envVars.ContainsKey('DEPLOYMENT_TIMEOUT_MINUTES')) { [int]$envVars['DEPLOYMENT_TIMEOUT_MINUTES'] } else { 15 }

if (-not $DryRun) {
    $deploymentResult = Wait-EcsDeployment -ClusterName $envVars['ECS_CLUSTER_NAME'] -ServiceName $envVars['ECS_SERVICE_NAME_API'] -Region $envVars['AWS_REGION'] -TimeoutMinutes $timeout
    
    if ($deploymentResult.Success) {
        Write-Log "Deployment completado y verificado exitosamente" -Level "Success"
        $finalStatus = $deploymentResult.Status
        Write-Log "Estado final:" -Level "Info"
        Write-Log "  Running: $($finalStatus.RunningCount)/$($finalStatus.DesiredCount)" -Level "Info"
    } else {
        Write-Log "Deployment iniciado pero no se pudo verificar completamente dentro del timeout" -Level "Warning"
    }
} else {
    Write-Log "[DRY RUN] Se monitorearía el estado del deployment" -Level "Warning"
}

# Finalizar
Write-Step "Despliegue Completado" "Green"
Write-Log "El proceso de despliegue ha finalizado" -Level "Success"
Write-Log "Verifica el estado del servicio en la consola de AWS" -Level "Info"
