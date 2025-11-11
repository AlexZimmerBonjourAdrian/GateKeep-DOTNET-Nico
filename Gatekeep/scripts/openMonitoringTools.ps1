# Script para abrir todas las herramientas de observabilidad en el navegador
# GateKeep - Monitoring Tools Launcher

param(
    [switch]$SkipHealthCheck
)

# Colores para output
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

Write-ColorOutput "`n=== GateKeep - Apertura de Herramientas de Observabilidad ===" "Cyan"

# URLs de las herramientas
$urls = @{
    "Seq (Logs)" = "http://localhost:5341"
    "Prometheus (Métricas)" = "http://localhost:9090"
    "Grafana (Dashboards)" = "http://localhost:3000"
    "Swagger (API)" = "http://localhost:5010/swagger"
}

# Verificar que Docker esté corriendo
try {
    $dockerRunning = docker info 2>$null
    if (-not $dockerRunning) {
        Write-ColorOutput "Error: Docker no está corriendo" "Red"
        exit 1
    }
} catch {
    Write-ColorOutput "Error: Docker no está disponible" "Red"
    exit 1
}

# Verificar estado de los contenedores (opcional)
if (-not $SkipHealthCheck) {
    Write-ColorOutput "`nVerificando estado de los contenedores..." "Yellow"
    
    $containers = @{
        "gatekeep-seq" = "Seq"
        "gatekeep-prometheus" = "Prometheus"
        "gatekeep-grafana" = "Grafana"
        "gatekeep-api" = "API"
    }
    
    $allRunning = $true
    foreach ($containerName in $containers.Keys) {
        $containerStatus = docker inspect -f "{{.State.Running}}" $containerName 2>$null
        
        if ($containerStatus -eq "true") {
            Write-ColorOutput "  [OK] $($containers[$containerName]) esta corriendo" "Green"
        } else {
            Write-ColorOutput "  [X] $($containers[$containerName]) NO esta corriendo" "Red"
            $allRunning = $false
        }
    }
    
    if (-not $allRunning) {
        Write-ColorOutput "`nAdvertencia: Algunos contenedores no están corriendo." "Yellow"
        Write-ColorOutput "¿Deseas abrir las URLs de todas formas? (S/N): " "Yellow" -NoNewline
        $response = Read-Host
        if ($response -ne "S" -and $response -ne "s") {
            Write-ColorOutput "Operación cancelada." "Red"
            exit 0
        }
    }
}

# Abrir cada URL en el navegador predeterminado
Write-ColorOutput "`nAbriendo herramientas en el navegador..." "Cyan"

foreach ($tool in $urls.GetEnumerator()) {
    Write-ColorOutput "  -> Abriendo $($tool.Key): $($tool.Value)" "White"
    Start-Process $tool.Value
    Start-Sleep -Milliseconds 500  # Pequeña pausa entre cada apertura
}

Write-ColorOutput "`n=== Todas las herramientas han sido abiertas ===" "Green"
Write-ColorOutput "`nCredenciales de acceso:" "Yellow"
Write-ColorOutput "  - Seq: usuario=admin, contraseña=admin" "White"
Write-ColorOutput "  - Grafana: usuario=admin, contraseña=admin" "White"
Write-ColorOutput "  - Prometheus: sin autenticacion" "White"
Write-ColorOutput "  - Swagger: sin autenticacion (requiere JWT para endpoints protegidos)" "White"

Write-ColorOutput "`nTips:" "Cyan"
Write-ColorOutput "  - Usa -SkipHealthCheck para omitir la verificacion de contenedores" "Gray"
Write-ColorOutput "  - Ejemplo: .\scripts\openMonitoringTools.ps1 -SkipHealthCheck" "Gray"

