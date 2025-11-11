# Script para instalar y ejecutar Redis con Docker
# GateKeep - Sistema de Caching

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  GateKeep - Instalación de Redis" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Verificar si Docker está instalado
$dockerInstalled = Get-Command docker -ErrorAction SilentlyContinue
if (-not $dockerInstalled) {
    Write-Host "❌ Docker no está instalado." -ForegroundColor Red
    Write-Host "   Por favor, instala Docker Desktop desde: https://www.docker.com/products/docker-desktop" -ForegroundColor Yellow
    exit 1
}

Write-Host "✓ Docker está instalado" -ForegroundColor Green

# Verificar si el contenedor de Redis ya existe
$existingContainer = docker ps -a --filter "name=redis-gatekeep" --format "{{.Names}}"

if ($existingContainer -eq "redis-gatekeep") {
    Write-Host ""
    Write-Host "Contenedor 'redis-gatekeep' encontrado." -ForegroundColor Yellow
    
    # Verificar si está corriendo
    $isRunning = docker ps --filter "name=redis-gatekeep" --format "{{.Names}}"
    
    if ($isRunning -eq "redis-gatekeep") {
        Write-Host "✓ Redis ya está corriendo" -ForegroundColor Green
        Write-Host ""
        Write-Host "Puedes verificar el estado con:" -ForegroundColor Cyan
        Write-Host "  docker ps | findstr redis-gatekeep" -ForegroundColor White
    } else {
        Write-Host "Iniciando contenedor existente..." -ForegroundColor Yellow
        docker start redis-gatekeep
        Write-Host "✓ Redis iniciado correctamente" -ForegroundColor Green
    }
} else {
    Write-Host ""
    Write-Host "Creando nuevo contenedor de Redis..." -ForegroundColor Yellow
    docker run -d --name redis-gatekeep -p 6379:6379 redis:latest
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Redis instalado y corriendo correctamente" -ForegroundColor Green
    } else {
        Write-Host "❌ Error al crear el contenedor de Redis" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Información de Conexión" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Host: localhost" -ForegroundColor White
Write-Host "  Puerto: 6379" -ForegroundColor White
Write-Host "  Instancia: GateKeep:" -ForegroundColor White
Write-Host ""

Write-Host "Comandos útiles:" -ForegroundColor Cyan
Write-Host "  Ver logs:    docker logs redis-gatekeep" -ForegroundColor White
Write-Host "  Detener:     docker stop redis-gatekeep" -ForegroundColor White
Write-Host "  Reiniciar:   docker restart redis-gatekeep" -ForegroundColor White
Write-Host "  Eliminar:    docker rm -f redis-gatekeep" -ForegroundColor White
Write-Host "  Conectar:    docker exec -it redis-gatekeep redis-cli" -ForegroundColor White
Write-Host ""

Write-Host "✓ Redis está listo para usar con GateKeep" -ForegroundColor Green
Write-Host ""

