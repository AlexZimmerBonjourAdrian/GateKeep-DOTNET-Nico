# Script de Setup General para GateKeep
# Verifica y configura todos los requisitos del proyecto

param(
    [switch]$AutoFix
)

# Colores para output
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

function Write-Header {
    param([string]$Title)
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "  $Title" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
}

function Write-Check {
    param(
        [string]$Item,
        [bool]$Status,
        [string]$Message = ""
    )
    if ($Status) {
        Write-ColorOutput "  [OK] $Item" "Green"
        if ($Message) {
            Write-ColorOutput "      $Message" "Gray"
        }
    } else {
        Write-ColorOutput "  [X]  $Item" "Red"
        if ($Message) {
            Write-ColorOutput "      $Message" "Yellow"
        }
    }
}

# Función para verificar Docker
function Test-Docker {
    Write-ColorOutput "Verificando Docker..." "Yellow"
    
    $dockerInstalled = Get-Command docker -ErrorAction SilentlyContinue
    if (-not $dockerInstalled) {
        Write-Check "Docker instalado" $false "Instala Docker Desktop desde https://www.docker.com/products/docker-desktop"
        return $false
    }
    
    Write-Check "Docker instalado" $true
    
    try {
        docker ps | Out-Null
        Write-Check "Docker corriendo" $true
        return $true
    } catch {
        Write-Check "Docker corriendo" $false "Inicia Docker Desktop"
        if ($AutoFix) {
            Write-ColorOutput "      Intentando iniciar Docker Desktop..." "Yellow"
            Start-Process "C:\Program Files\Docker\Docker\Docker Desktop.exe" -ErrorAction SilentlyContinue
            Write-ColorOutput "      Esperando 30 segundos..." "Yellow"
            Start-Sleep -Seconds 30
            try {
                docker ps | Out-Null
                Write-Check "Docker corriendo" $true "Iniciado automáticamente"
                return $true
            } catch {
                Write-Check "Docker corriendo" $false "No se pudo iniciar automáticamente"
                return $false
            }
        }
        return $false
    }
}

# Función para verificar .NET SDK
function Test-DotNet {
    Write-ColorOutput "Verificando .NET SDK..." "Yellow"
    
    $dotnetInstalled = Get-Command dotnet -ErrorAction SilentlyContinue
    if (-not $dotnetInstalled) {
        Write-Check ".NET SDK instalado" $false "Instala .NET 8 SDK desde https://dotnet.microsoft.com/download"
        return $false
    }
    
    $version = dotnet --version
    Write-Check ".NET SDK instalado" $true "Versión: $version"
    
    if ($version -lt "8.0") {
        Write-Check ".NET 8.0 o superior" $false "Versión actual: $version. Se requiere .NET 8.0 o superior"
        return $false
    }
    
    Write-Check ".NET 8.0 o superior" $true
    return $true
}

# Función para verificar Node.js y npm
function Test-NodeJS {
    Write-ColorOutput "Verificando Node.js y npm..." "Yellow"
    
    $nodeInstalled = Get-Command node -ErrorAction SilentlyContinue
    if (-not $nodeInstalled) {
        Write-Check "Node.js instalado" $false "Instala Node.js desde https://nodejs.org/"
        return $false
    }
    
    $nodeVersion = node --version
    Write-Check "Node.js instalado" $true "Versión: $nodeVersion"
    
    $npmInstalled = Get-Command npm -ErrorAction SilentlyContinue
    if (-not $npmInstalled) {
        Write-Check "npm instalado" $false "npm debería venir con Node.js"
        return $false
    }
    
    $npmVersion = npm --version
    Write-Check "npm instalado" $true "Versión: $npmVersion"
    
    return $true
}

# Función para verificar archivo .env
function Test-EnvFile {
    Write-ColorOutput "Verificando archivo .env..." "Yellow"
    
    $envPath = Join-Path $PSScriptRoot "src\.env"
    $envExamplePath = Join-Path $PSScriptRoot "src\.env.example"
    
    if (Test-Path $envPath) {
        Write-Check "Archivo .env existe" $true "Ubicación: $envPath"
        return $true
    } else {
        Write-Check "Archivo .env existe" $false "No encontrado en: $envPath"
        
        if (Test-Path $envExamplePath) {
            if ($AutoFix) {
                Write-ColorOutput "      Copiando .env.example a .env..." "Yellow"
                Copy-Item $envExamplePath $envPath
                Write-Check "Archivo .env creado" $true "Edita el archivo con tus credenciales"
                return $true
            } else {
                Write-ColorOutput "      Ejecuta: Copy-Item 'src\.env.example' 'src\.env'" "Gray"
            }
        } else {
            Write-Check "Archivo .env.example existe" $false "No se encontró plantilla"
        }
        return $false
    }
}

# Función para verificar puertos
function Test-Ports {
    Write-ColorOutput "Verificando puertos disponibles..." "Yellow"
    
    $ports = @{
        "6379" = "Redis"
        "5011" = "API"
        "3000" = "Frontend"
        "5432" = "PostgreSQL"
        "5341" = "Seq"
        "9090" = "Prometheus"
        "3001" = "Grafana"
        "15672" = "RabbitMQ Management"
    }
    
    $allAvailable = $true
    foreach ($port in $ports.GetEnumerator()) {
        $connection = Get-NetTCPConnection -LocalPort $port.Key -ErrorAction SilentlyContinue
        if ($connection) {
            Write-Check "Puerto $($port.Key) ($($port.Value))" $false "En uso"
            $allAvailable = $false
        } else {
            Write-Check "Puerto $($port.Key) ($($port.Value))" $true "Disponible"
        }
    }
    
    return $allAvailable
}

# Función para verificar configuración de monitoreo
function Test-MonitoringConfig {
    Write-ColorOutput "Verificando configuración de monitoreo..." "Yellow"
    
    $prometheusConfig = Join-Path $PSScriptRoot "src\monitoring\prometheus.yml"
    $grafanaDatasources = Join-Path $PSScriptRoot "src\monitoring\grafana\provisioning\datasources.yml"
    $grafanaDashboards = Join-Path $PSScriptRoot "src\monitoring\grafana\provisioning\dashboards.yml"
    
    $allGood = $true
    
    if (Test-Path $prometheusConfig) {
        Write-Check "Prometheus config" $true
    } else {
        Write-Check "Prometheus config" $false "No encontrado: $prometheusConfig"
        $allGood = $false
    }
    
    if (Test-Path $grafanaDatasources) {
        Write-Check "Grafana datasources" $true
    } else {
        Write-Check "Grafana datasources" $false "No encontrado: $grafanaDatasources"
        $allGood = $false
    }
    
    if (Test-Path $grafanaDashboards) {
        Write-Check "Grafana dashboards" $true
    } else {
        Write-Check "Grafana dashboards" $false "No encontrado: $grafanaDashboards"
        $allGood = $false
    }
    
    return $allGood
}

# Función principal
function Main {
    Write-Header "GateKeep - Setup General"
    
    $results = @{
        Docker = $false
        DotNet = $false
        NodeJS = $false
        EnvFile = $false
        Ports = $false
        Monitoring = $false
    }
    
    # Verificaciones
    $results.Docker = Test-Docker
    Write-Host ""
    
    $results.DotNet = Test-DotNet
    Write-Host ""
    
    $results.NodeJS = Test-NodeJS
    Write-Host ""
    
    $results.EnvFile = Test-EnvFile
    Write-Host ""
    
    $results.Ports = Test-Ports
    Write-Host ""
    
    $results.Monitoring = Test-MonitoringConfig
    Write-Host ""
    
    # Resumen
    Write-Header "Resumen"
    
    $allOk = $true
    foreach ($check in $results.GetEnumerator()) {
        if (-not $check.Value) {
            $allOk = $false
        }
    }
    
    if ($allOk) {
        Write-ColorOutput "Todos los requisitos están configurados correctamente" "Green"
        Write-Host ""
        Write-ColorOutput "Puedes ejecutar:" "Cyan"
        Write-ColorOutput "  .\run-backend.ps1    - Para ejecutar el backend localmente" "White"
        Write-ColorOutput "  .\run-frontend.ps1   - Para ejecutar el frontend localmente" "White"
        Write-ColorOutput "  .\iniciar-docker.ps1 - Para iniciar todos los servicios con Docker" "White"
        Write-Host ""
    } else {
        Write-ColorOutput "Algunos requisitos no están configurados" "Yellow"
        Write-Host ""
        Write-ColorOutput "Para configurar automáticamente, ejecuta:" "Cyan"
        Write-ColorOutput "  .\setup.ps1 -AutoFix" "White"
        Write-Host ""
        Write-ColorOutput "O configura manualmente los elementos marcados con [X]" "Yellow"
        Write-Host ""
    }
}

# Ejecutar
Main

