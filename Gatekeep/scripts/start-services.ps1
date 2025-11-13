# Script para iniciar TODA la aplicación en Docker
# Incluye: PostgreSQL, Redis, MongoDB, Seq, Prometheus, Grafana y la API .NET

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  GateKeep - Iniciar TODO en Docker" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Este script inicia TODOS los servicios incluyendo la API .NET:" -ForegroundColor Yellow
Write-Host "  - PostgreSQL (puerto 5432)" -ForegroundColor White
Write-Host "  - Redis (puerto 6379)" -ForegroundColor White
Write-Host "  - Seq Logs (puerto 5341)" -ForegroundColor White
Write-Host "  - Prometheus (puerto 9090)" -ForegroundColor White
Write-Host "  - Grafana (puerto 3001)" -ForegroundColor White
Write-Host "  - API .NET (puerto 5011)" -ForegroundColor Green
Write-Host ""

# Verificar si Docker está instalado
$dockerInstalled = Get-Command docker -ErrorAction SilentlyContinue
if (-not $dockerInstalled) {
    Write-Host "[ERROR] Docker no está instalado." -ForegroundColor Red
    exit 1
}

Write-Host "[OK] Docker está instalado" -ForegroundColor Green

# Cambiar al directorio src donde está docker-compose.yml
$srcPath = Join-Path $PSScriptRoot "..\src"
if (-not (Test-Path $srcPath)) {
    Write-Host "[ERROR] No se encontró el directorio src" -ForegroundColor Red
    exit 1
}

Set-Location $srcPath
Write-Host "[OK] Directorio: $srcPath" -ForegroundColor Green

# Verificar si existe .env
if (-not (Test-Path ".env")) {
    Write-Host "[ADVERTENCIA] No se encontró archivo .env" -ForegroundColor Yellow
    
    if (Test-Path ".env.example") {
        Write-Host "Creando .env desde .env.example..." -ForegroundColor Yellow
        Copy-Item ".env.example" ".env"
        Write-Host "[OK] Archivo .env creado" -ForegroundColor Green
        Write-Host ""
        Write-Host "[IMPORTANTE] Debes editar src\.env y configurar:" -ForegroundColor Yellow
        Write-Host "  - DB_PASSWORD (contraseña de PostgreSQL)" -ForegroundColor White
        Write-Host "  - JWT_KEY (mínimo 32 caracteres aleatorios)" -ForegroundColor White
        Write-Host "  - MONGODB_CONNECTION (si usas MongoDB Atlas)" -ForegroundColor White
        Write-Host ""
        Write-Host "[ERROR] El archivo .env fue creado pero tiene valores de ejemplo." -ForegroundColor Red
        Write-Host "       Edita src\.env con tus valores reales y vuelve a ejecutar." -ForegroundColor Red
        exit 1
    } else {
        Write-Host "[ERROR] No se encontró .env ni .env.example" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "[OK] Archivo .env encontrado" -ForegroundColor Green
}

# Verificar si existe Dockerfile
if (-not (Test-Path "Dockerfile")) {
    Write-Host "[ERROR] No se encontró Dockerfile en src/" -ForegroundColor Red
    exit 1
}

Write-Host "[OK] Dockerfile encontrado" -ForegroundColor Green
Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Construyendo e Iniciando Contenedores" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Esto puede tardar varios minutos la primera vez..." -ForegroundColor Yellow
Write-Host ""

# Detener contenedores anteriores si existen
Write-Host "Limpiando contenedores anteriores..." -ForegroundColor Yellow
docker-compose down 2>$null

# Construir y levantar todos los servicios
Write-Host "Construyendo imágenes y levantando servicios..." -ForegroundColor Cyan
docker-compose up -d --build

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "[OK] Servicios iniciados correctamente" -ForegroundColor Green
    Write-Host ""
    Write-Host "Esperando a que los servicios estén listos..." -ForegroundColor Yellow
    Start-Sleep -Seconds 10
    
    Write-Host ""
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host "  Estado de los Servicios" -ForegroundColor Cyan
    Write-Host "================================================" -ForegroundColor Cyan
    docker-compose ps
    
    Write-Host ""
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host "  URLs de Acceso" -ForegroundColor Cyan
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "API .NET:" -ForegroundColor Green
    Write-Host "  http://localhost:5011" -ForegroundColor White
    Write-Host "  http://localhost:5011/swagger" -ForegroundColor White
    Write-Host "  http://localhost:5011/health" -ForegroundColor White
    Write-Host ""
    Write-Host "Bases de Datos:" -ForegroundColor Cyan
    Write-Host "  PostgreSQL: localhost:5432" -ForegroundColor White
    Write-Host "  Redis: localhost:6379" -ForegroundColor White
    Write-Host ""
    Write-Host "Monitoreo:" -ForegroundColor Cyan
    Write-Host "  Seq (Logs): http://localhost:5341" -ForegroundColor White
    Write-Host "  Prometheus: http://localhost:9090" -ForegroundColor White
    Write-Host "  Grafana: http://localhost:3001 (admin/admin)" -ForegroundColor White
    Write-Host ""
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host "  Comandos Útiles" -ForegroundColor Cyan
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host "  Ver logs de todos:  docker-compose logs -f" -ForegroundColor White
    Write-Host "  Ver logs de API:    docker-compose logs -f api" -ForegroundColor White
    Write-Host "  Detener todo:       docker-compose down" -ForegroundColor White
    Write-Host "  Reiniciar API:      docker-compose restart api" -ForegroundColor White
    Write-Host "  Reconstruir API:    docker-compose up -d --build api" -ForegroundColor White
    Write-Host ""
    
    # Verificar health de la API
    Write-Host "Verificando health de la API..." -ForegroundColor Yellow
    Start-Sleep -Seconds 5
    
    try {
        $health = Invoke-WebRequest -Uri "http://localhost:5011/health" -TimeoutSec 5 -ErrorAction SilentlyContinue
        if ($health.StatusCode -eq 200) {
            Write-Host "[OK] API está respondiendo correctamente" -ForegroundColor Green
        }
    } catch {
        Write-Host "[ADVERTENCIA] La API aún no está lista" -ForegroundColor Yellow
        Write-Host "Espera unos segundos y verifica: http://localhost:5011/health" -ForegroundColor Yellow
        Write-Host "Ver logs con: docker-compose logs -f api" -ForegroundColor Cyan
    }
    
    Write-Host ""
    Write-Host "[OK] GateKeep está corriendo en Docker" -ForegroundColor Green
    Write-Host ""
    
} else {
    Write-Host ""
    Write-Host "[ERROR] Error al iniciar los servicios" -ForegroundColor Red
    Write-Host ""
    Write-Host "Verifica los logs con:" -ForegroundColor Yellow
    Write-Host "  docker-compose logs" -ForegroundColor White
    Write-Host ""
    Write-Host "Posibles causas:" -ForegroundColor Yellow
    Write-Host "  - Puertos ocupados (5011, 5432, 6379)" -ForegroundColor White
    Write-Host "  - Error en el Dockerfile" -ForegroundColor White
    Write-Host "  - Variables de entorno mal configuradas en .env" -ForegroundColor White
    exit 1
}
