# Script para iniciar GateKeep con Docker
# Ubicación: Gatekeep/iniciar-docker.ps1

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "  Iniciando GateKeep con Docker  " -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

# Verificar Docker Desktop
Write-Host "[1/5] Verificando Docker Desktop..." -ForegroundColor Yellow
$dockerProcess = Get-Process "Docker Desktop" -ErrorAction SilentlyContinue
if (!$dockerProcess) {
    Write-Host "Docker Desktop no está ejecutándose. Iniciando..." -ForegroundColor Yellow
    Start-Process "C:\Program Files\Docker\Docker\Docker Desktop.exe"
    Write-Host "Esperando a que Docker Desktop inicie (30 segundos)..." -ForegroundColor Yellow
    Start-Sleep -Seconds 30
} else {
    Write-Host "Docker Desktop está ejecutándose" -ForegroundColor Green
}

# Navegar al directorio src
Write-Host ""
Write-Host "[2/5] Navegando al directorio src..." -ForegroundColor Yellow
$srcPath = Join-Path $PSScriptRoot "src"
Set-Location $srcPath
Write-Host "Directorio actual: $srcPath" -ForegroundColor Green

# Verificar archivo .env
Write-Host ""
Write-Host "[3/5] Verificando archivo .env..." -ForegroundColor Yellow
$envPath = Join-Path $srcPath ".env"
if (!(Test-Path $envPath)) {
    Write-Host "ERROR: Archivo .env no encontrado en: $envPath" -ForegroundColor Red
    Write-Host "Copia .env.example a .env y configura tus variables" -ForegroundColor Red
    Write-Host ""
    Write-Host "Comando: Copy-Item '.env.example' '.env'" -ForegroundColor Yellow
    Read-Host "Presiona Enter para salir"
    exit 1
}
Write-Host "Archivo .env encontrado" -ForegroundColor Green

# Detener servicios anteriores (si existen)
Write-Host ""
Write-Host "[4/5] Verificando servicios existentes..." -ForegroundColor Yellow
$existingContainers = docker-compose ps -q
if ($existingContainers) {
    Write-Host "Deteniendo servicios anteriores..." -ForegroundColor Yellow
    docker-compose down
}

# Levantar servicios
Write-Host ""
Write-Host "[5/5] Levantando servicios con Docker Compose..." -ForegroundColor Yellow
docker-compose up -d

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "==================================" -ForegroundColor Green
    Write-Host "  Servicios iniciados con éxito  " -ForegroundColor Green
    Write-Host "==================================" -ForegroundColor Green
    Write-Host ""
    
    # Mostrar estado
    Write-Host "Estado de los servicios:" -ForegroundColor Cyan
    docker-compose ps
    
    Write-Host ""
    Write-Host "URLs disponibles:" -ForegroundColor Cyan
    Write-Host "  API Swagger:  http://localhost:5011/swagger" -ForegroundColor White
    Write-Host "  Health Check: http://localhost:5011/health" -ForegroundColor White
    Write-Host "  Seq (Logs):   http://localhost:5341" -ForegroundColor White
    Write-Host "  Prometheus:   http://localhost:9090" -ForegroundColor White
    Write-Host "  Grafana:      http://localhost:3001" -ForegroundColor White
    Write-Host ""
    
    # Preguntar si abrir navegador
    $openBrowser = Read-Host "Abrir Swagger en el navegador? (S/N)"
    if ($openBrowser -eq "S" -or $openBrowser -eq "s") {
        Start-Process "http://localhost:5011/swagger"
    }
    
    Write-Host ""
    Write-Host "Para ver logs: docker-compose logs -f api" -ForegroundColor Yellow
    Write-Host "Para detener:  docker-compose down" -ForegroundColor Yellow
    Write-Host ""
    
} else {
    Write-Host ""
    Write-Host "ERROR: Falló al iniciar los servicios" -ForegroundColor Red
    Write-Host "Revisa los logs con: docker-compose logs" -ForegroundColor Yellow
    Write-Host ""
}

Read-Host "Presiona Enter para salir"

