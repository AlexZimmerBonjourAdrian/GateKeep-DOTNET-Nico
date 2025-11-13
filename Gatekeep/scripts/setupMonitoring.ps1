# GateKeep Monitoring Stack Setup Script
# Script para configurar y verificar el stack de observabilidad
# Incluye: Seq, Prometheus, Grafana

$ErrorActionPreference = "Stop"

# Colores para output
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

# Función para verificar si un puerto está disponible
function Test-Port {
    param(
        [int]$Port,
        [string]$ServiceName
    )
    
    try {
        $tcpClient = New-Object System.Net.Sockets.TcpClient
        $tcpClient.Connect("localhost", $Port)
        if ($tcpClient.Connected) {
            $tcpClient.Close()
            Write-ColorOutput "$ServiceName está corriendo en puerto $Port" "Green"
            return $true
        }
    } catch {
        Write-ColorOutput "$ServiceName NO está disponible en puerto $Port" "Red"
        return $false
    }
}

# Función para verificar Docker
function Test-DockerRunning {
    try {
        docker ps | Out-Null
        return $true
    } catch {
        Write-ColorOutput "Docker no está corriendo. Por favor inicia Docker Desktop." "Red"
        return $false
    }
}

# Función para verificar servicios de monitoreo
function Test-MonitoringServices {
    Write-ColorOutput "`nVerificando servicios de monitoreo..." "Yellow"
    Write-Host ""
    
    $services = @{
        "Seq" = 5341
        "Prometheus" = 9090
        "Grafana" = 3001
        "GateKeep API" = 5011
    }
    
    $allRunning = $true
    
    foreach ($service in $services.GetEnumerator()) {
        $isRunning = Test-Port -Port $service.Value -ServiceName $service.Key
        if (-not $isRunning) {
            $allRunning = $false
        }
    }
    
    return $allRunning
}

# Función para iniciar stack de monitoreo
function Start-MonitoringStack {
    Write-ColorOutput "`nIniciando stack de monitoreo con Docker Compose..." "Yellow"
    
    $composeFile = "src\docker-compose.yaml"
    
    if (-not (Test-Path $composeFile)) {
        Write-ColorOutput "No se encontró docker-compose.yaml en $composeFile" "Red"
        return $false
    }
    
    try {
        Set-Location "src"
        
        Write-ColorOutput "Levantando servicios: seq, prometheus, grafana..." "Cyan"
        docker-compose up -d seq prometheus grafana
        
        if ($LASTEXITCODE -eq 0) {
            Write-ColorOutput "Servicios iniciados exitosamente" "Green"
            Write-ColorOutput "Esperando a que los servicios estén listos (30 segundos)..." "Yellow"
            Start-Sleep -Seconds 30
            return $true
        } else {
            Write-ColorOutput "Error al iniciar servicios" "Red"
            return $false
        }
    } catch {
        Write-ColorOutput "Error: $($_.Exception.Message)" "Red"
        return $false
    } finally {
        Set-Location ".."
    }
}

# Función para detener stack de monitoreo
function Stop-MonitoringStack {
    Write-ColorOutput "`nDeteniendo stack de monitoreo..." "Yellow"
    
    try {
        Set-Location "src"
        docker-compose stop seq prometheus grafana
        
        if ($LASTEXITCODE -eq 0) {
            Write-ColorOutput "Servicios detenidos exitosamente" "Green"
            return $true
        } else {
            Write-ColorOutput "Error al detener servicios" "Red"
            return $false
        }
    } catch {
        Write-ColorOutput "Error: $($_.Exception.Message)" "Red"
        return $false
    } finally {
        Set-Location ".."
    }
}

# Función para mostrar URLs de acceso
function Show-AccessInfo {
    Write-Host ""
    Write-ColorOutput "========================================" "Cyan"
    Write-ColorOutput "  URLs de Acceso" "Cyan"
    Write-ColorOutput "========================================" "Cyan"
    Write-Host ""
    Write-ColorOutput "Seq (Logs Centralizados):" "Yellow"
    Write-ColorOutput "  http://localhost:5341" "Green"
    Write-ColorOutput "  - Ver logs en tiempo real" "Cyan"
    Write-ColorOutput "  - Búsqueda estructurada de eventos" "Cyan"
    Write-Host ""
    Write-ColorOutput "Prometheus (Métricas):" "Yellow"
    Write-ColorOutput "  http://localhost:9090" "Green"
    Write-ColorOutput "  - Consultar métricas" "Cyan"
    Write-ColorOutput "  - Explorar targets y alertas" "Cyan"
    Write-Host ""
    Write-ColorOutput "Grafana (Dashboards):" "Yellow"
    Write-ColorOutput "  http://localhost:3001" "Green"
    Write-ColorOutput "  - Usuario: admin" "Cyan"
    Write-ColorOutput "  - Contraseña: admin123" "Cyan"
    Write-ColorOutput "  - Dashboard: GateKeep Overview" "Cyan"
    Write-Host ""
    Write-ColorOutput "GateKeep API:" "Yellow"
    Write-ColorOutput "  http://localhost:5011" "Green"
    Write-ColorOutput "  - Swagger: http://localhost:5011/swagger" "Cyan"
    Write-ColorOutput "  - Metrics: http://localhost:5011/metrics" "Cyan"
    Write-Host ""
}

# Función para verificar configuración de Prometheus
function Test-PrometheusConfig {
    Write-ColorOutput "`nVerificando configuración de Prometheus..." "Yellow"
    
    $prometheusConfig = "src\monitoring\prometheus.yml"
    
    if (Test-Path $prometheusConfig) {
        Write-ColorOutput "Archivo prometheus.yml encontrado" "Green"
        return $true
    } else {
        Write-ColorOutput "No se encontró prometheus.yml" "Red"
        return $false
    }
}

# Función para verificar configuración de Grafana
function Test-GrafanaConfig {
    Write-ColorOutput "Verificando configuración de Grafana..." "Yellow"
    
    $datasourcesConfig = "src\monitoring\grafana\provisioning\datasources.yml"
    $dashboardsConfig = "src\monitoring\grafana\provisioning\dashboards.yml"
    $dashboard = "src\monitoring\grafana\dashboards\gatekeep-overview.json"
    
    $allGood = $true
    
    if (Test-Path $datasourcesConfig) {
        Write-ColorOutput "Configuración de datasources encontrada" "Green"
    } else {
        Write-ColorOutput "No se encontró datasources.yml" "Red"
        $allGood = $false
    }
    
    if (Test-Path $dashboardsConfig) {
        Write-ColorOutput "Configuración de dashboards encontrada" "Green"
    } else {
        Write-ColorOutput "No se encontró dashboards.yml" "Red"
        $allGood = $false
    }
    
    if (Test-Path $dashboard) {
        Write-ColorOutput "Dashboard GateKeep Overview encontrado" "Green"
    } else {
        Write-ColorOutput "No se encontró gatekeep-overview.json" "Red"
        $allGood = $false
    }
    
    return $allGood
}

# Función principal
function Main {
    param(
        [string]$Action = "start"
    )
    
    Write-ColorOutput "========================================" "Cyan"
    Write-ColorOutput "  GateKeep Monitoring Stack Setup" "Cyan"
    Write-ColorOutput "========================================" "Cyan"
    Write-Host ""
    
    # Verificar Docker
    if (-not (Test-DockerRunning)) {
        Write-ColorOutput "Docker es requerido para el stack de monitoreo" "Red"
        exit 1
    }
    
    switch ($Action.ToLower()) {
        "start" {
            # Verificar configuración
            $prometheusOk = Test-PrometheusConfig
            $grafanaOk = Test-GrafanaConfig
            
            if (-not ($prometheusOk -and $grafanaOk)) {
                Write-ColorOutput "`nAdvertencia: Algunas configuraciones no están completas" "Yellow"
                Write-ColorOutput "El sistema puede no funcionar correctamente" "Yellow"
                Write-Host ""
            }
            
            # Iniciar servicios
            if (Start-MonitoringStack) {
                # Verificar servicios
                if (Test-MonitoringServices) {
                    Write-ColorOutput "`nTodos los servicios están corriendo correctamente" "Green"
                    Show-AccessInfo
                } else {
                    Write-ColorOutput "`nAlgunos servicios no están disponibles. Verifica los logs con:" "Yellow"
                    Write-ColorOutput "  docker-compose -f src\docker-compose.yaml logs" "Cyan"
                }
            }
        }
        
        "stop" {
            Stop-MonitoringStack
        }
        
        "status" {
            Test-MonitoringServices
            Show-AccessInfo
        }
        
        "restart" {
            Stop-MonitoringStack
            Start-Sleep -Seconds 5
            Start-MonitoringStack
            Test-MonitoringServices
            Show-AccessInfo
        }
        
        "open" {
            Write-ColorOutput "Abriendo herramientas de observabilidad..." "Cyan"
            
            # Verificar si existe el script de apertura
            $openScript = "scripts\openMonitoringTools.ps1"
            
            if (Test-Path $openScript) {
                & ".\$openScript"
            } else {
                Write-ColorOutput "No se encontró el script openMonitoringTools.ps1" "Red"
                Write-ColorOutput "Abriendo URLs manualmente..." "Yellow"
                
                # Abrir URLs manualmente como fallback
                Start-Process "http://localhost:5341"
                Start-Process "http://localhost:9090"
                Start-Process "http://localhost:3000"
                Start-Process "http://localhost:5010/swagger"
            }
        }
        
        default {
            Write-ColorOutput "Acción no reconocida: $Action" "Red"
            Write-ColorOutput "Acciones disponibles: start, stop, status, restart, open" "Yellow"
            exit 1
        }
    }
    
    Write-ColorOutput "`n========================================" "Cyan"
    Write-ColorOutput "  Operación completada" "Cyan"
    Write-ColorOutput "========================================" "Cyan"
}

# Verificar parámetros
param(
    [Parameter(Position=0)]
    [string]$Action = "start"
)

# Ejecutar función principal
Main -Action $Action

