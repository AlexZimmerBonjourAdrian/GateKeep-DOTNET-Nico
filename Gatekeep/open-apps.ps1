# Script para abrir aplicaciones y herramientas de GateKeep
# Menú interactivo para abrir Swagger, Frontend y herramientas de monitoreo

param(
    [ValidateSet("Local", "Docker", "AWS", "Monitoring", "Testing")]
    [string]$Mode = ""
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

# Función para verificar contenedores Docker
function Test-DockerContainers {
    $containers = @{
        "gatekeep-api" = "API"
        "gatekeep-seq" = "Seq"
        "gatekeep-prometheus" = "Prometheus"
        "gatekeep-grafana" = "Grafana"
        "gatekeep-rabbitmq" = "RabbitMQ"
    }
    
    $allRunning = $true
    foreach ($containerName in $containers.Keys) {
        $containerStatus = docker inspect -f "{{.State.Running}}" $containerName 2>$null
        if ($containerStatus -eq "true") {
            Write-ColorOutput "  [OK] $($containers[$containerName])" "Green"
        } else {
            Write-ColorOutput "  [X]  $($containers[$containerName]) NO está corriendo" "Red"
            $allRunning = $false
        }
    }
    
    return $allRunning
}

# Función para obtener URLs de AWS desde Terraform
function Get-AwsUrls {
    $terraformPath = Join-Path $PSScriptRoot "terraform"
    
    if (-not (Test-Path $terraformPath)) {
        return $null
    }
    
    Push-Location $terraformPath
    try {
        $albDns = terraform output -raw alb_dns_name 2>&1
        $applicationUrl = terraform output -raw application_url 2>&1
        
        if ($LASTEXITCODE -eq 0 -and $albDns) {
            return @{
                Swagger = "$applicationUrl/swagger"
                Frontend = $applicationUrl
                Api = "$applicationUrl/api"
            }
        }
        return $null
    } finally {
        Pop-Location
    }
}

# Función para abrir URLs
function Open-Urls {
    param(
        [hashtable]$Urls,
        [string]$Environment
    )
    
    Write-Header "Abriendo aplicaciones - $Environment"
    
    foreach ($url in $Urls.GetEnumerator()) {
        Write-ColorOutput "  -> Abriendo $($url.Key): $($url.Value)" "White"
        Start-Process $url.Value
        Start-Sleep -Milliseconds 500
    }
    
    Write-Host ""
    Write-ColorOutput "Todas las aplicaciones han sido abiertas" "Green"
    Write-Host ""
}

# Función para mostrar menú
function Show-Menu {
    Write-Header "GateKeep - Abrir Aplicaciones"
    Write-Host "Selecciona una opción:" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  1. Local - Swagger y Frontend local" -ForegroundColor White
    Write-Host "  2. Docker - Swagger y Frontend en Docker" -ForegroundColor White
    Write-Host "  3. AWS - Swagger y Frontend en AWS" -ForegroundColor White
    Write-Host "  4. Solo Monitoreo - Herramientas de observabilidad" -ForegroundColor White
    Write-Host "  5. Solo Testing - Swagger y RabbitMQ" -ForegroundColor White
    Write-Host "  6. Todo (Local + Monitoreo)" -ForegroundColor White
    Write-Host "  0. Salir" -ForegroundColor White
    Write-Host ""
}

# URLs locales
$localUrls = @{
    "Swagger" = "http://localhost:5011/swagger"
    "Frontend" = "http://localhost:3000"
}

# URLs Docker (mismas que local)
$dockerUrls = @{
    "Swagger" = "http://localhost:5011/swagger"
    "Frontend" = "http://localhost:3000"
}

# URLs de monitoreo
$monitoringUrls = @{
    "Seq (Logs)" = "http://localhost:5341"
    "Prometheus (Métricas)" = "http://localhost:9090"
    "Grafana (Dashboards)" = "http://localhost:3001"
    "RabbitMQ Management" = "http://localhost:15672"
}

# URLs de testing
$testingUrls = @{
    "Swagger" = "http://localhost:5011/swagger"
    "RabbitMQ Management" = "http://localhost:15672"
    "Health Check" = "http://localhost:5011/health"
}

# Si se pasó un modo por parámetro, usarlo directamente
if ($Mode) {
    $selectedOption = switch ($Mode) {
        "Local" { "1" }
        "Docker" { "2" }
        "AWS" { "3" }
        "Monitoring" { "4" }
        "Testing" { "5" }
        default { "" }
    }
} else {
    Show-Menu
    $selectedOption = Read-Host "Opción"
}

switch ($selectedOption) {
    "1" {
        # Local
        Open-Urls -Urls $localUrls -Environment "Local"
        Write-ColorOutput "Credenciales:" "Yellow"
        Write-ColorOutput "  - Swagger: Sin autenticación (requiere JWT para endpoints protegidos)" "White"
    }
    
    "2" {
        # Docker
        Write-Header "Verificando contenedores Docker"
        
        try {
            docker ps | Out-Null
        } catch {
            Write-ColorOutput "Error: Docker no está corriendo" "Red"
            exit 1
        }
        
        $allRunning = Test-DockerContainers
        
        if (-not $allRunning) {
            Write-Host ""
            Write-ColorOutput "Advertencia: Algunos contenedores no están corriendo" "Yellow"
            $response = Read-Host "¿Deseas abrir las URLs de todas formas? (S/N)"
            if ($response -ne "S" -and $response -ne "s") {
                Write-ColorOutput "Operación cancelada" "Red"
                exit 0
            }
        }
        
        Write-Host ""
        Open-Urls -Urls $dockerUrls -Environment "Docker"
        Write-ColorOutput "Credenciales:" "Yellow"
        Write-ColorOutput "  - Swagger: Sin autenticación (requiere JWT para endpoints protegidos)" "White"
    }
    
    "3" {
        # AWS
        Write-Header "Obteniendo URLs de AWS"
        
        $awsUrls = Get-AwsUrls
        
        if (-not $awsUrls) {
            Write-ColorOutput "Error: No se pudieron obtener las URLs de AWS" "Red"
            Write-ColorOutput "Asegúrate de que la infraestructura esté desplegada: .\start-aws.ps1" "Yellow"
            exit 1
        }
        
        Open-Urls -Urls $awsUrls -Environment "AWS"
    }
    
    "4" {
        # Solo Monitoreo
        Open-Urls -Urls $monitoringUrls -Environment "Monitoreo"
        Write-Host ""
        Write-ColorOutput "Credenciales:" "Yellow"
        Write-ColorOutput "  - Seq: admin / admin" "White"
        Write-ColorOutput "  - Grafana: admin / admin123" "White"
        Write-ColorOutput "  - Prometheus: Sin autenticación" "White"
        Write-ColorOutput "  - RabbitMQ: guest / guest" "White"
    }
    
    "5" {
        # Solo Testing
        Open-Urls -Urls $testingUrls -Environment "Testing"
        Write-Host ""
        Write-ColorOutput "Credenciales:" "Yellow"
        Write-ColorOutput "  - RabbitMQ: guest / guest" "White"
    }
    
    "6" {
        # Todo (Local + Monitoreo)
        Open-Urls -Urls $localUrls -Environment "Local"
        Start-Sleep -Seconds 1
        Open-Urls -Urls $monitoringUrls -Environment "Monitoreo"
        Write-Host ""
        Write-ColorOutput "Todas las aplicaciones y herramientas han sido abiertas" "Green"
    }
    
    "0" {
        Write-ColorOutput "Saliendo..." "Yellow"
        exit 0
    }
    
    default {
        Write-ColorOutput "Opción inválida" "Red"
        exit 1
    }
}

Write-Host ""

