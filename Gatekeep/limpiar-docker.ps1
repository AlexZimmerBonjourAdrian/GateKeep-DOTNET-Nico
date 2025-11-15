# Script para limpiar completamente Docker
# Elimina todos los contenedores, imágenes, volúmenes, redes y caché
# ADVERTENCIA: Esto eliminará TODO en Docker, no solo los contenedores de GateKeep

Write-Host "========================================" -ForegroundColor Red
Write-Host "  GateKeep - Limpieza Completa Docker" -ForegroundColor Red
Write-Host "========================================" -ForegroundColor Red
Write-Host ""
Write-Host "ADVERTENCIA: Este script eliminará:" -ForegroundColor Yellow
Write-Host "  - Todos los contenedores (en ejecución y detenidos)" -ForegroundColor Yellow
Write-Host "  - Todas las imágenes Docker" -ForegroundColor Yellow
Write-Host "  - Todos los volúmenes" -ForegroundColor Yellow
Write-Host "  - Todas las redes personalizadas" -ForegroundColor Yellow
Write-Host "  - Todo el caché de Docker" -ForegroundColor Yellow
Write-Host ""
Write-Host "Esto liberará espacio pero eliminará TODO en Docker." -ForegroundColor Yellow
Write-Host ""

# Verificar Docker
try {
    docker ps | Out-Null
} catch {
    Write-Host "Error: Docker no está corriendo o no está instalado" -ForegroundColor Red
    exit 1
}

# Mostrar uso actual de espacio
Write-Host "Espacio usado por Docker antes de la limpieza:" -ForegroundColor Cyan
docker system df
Write-Host ""

# Confirmación
$confirm = Read-Host "¿Estás seguro de que deseas continuar? Escribe 'SI' para confirmar"
if ($confirm -ne "SI") {
    Write-Host ""
    Write-Host "Operación cancelada" -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "Iniciando limpieza completa..." -ForegroundColor Yellow
Write-Host ""

# Paso 1: Detener todos los contenedores
Write-Host "[1/6] Deteniendo todos los contenedores..." -ForegroundColor Yellow
docker stop $(docker ps -aq) 2>&1 | Out-Null
if ($LASTEXITCODE -eq 0 -or $LASTEXITCODE -eq 1) {
    # Exit code 1 puede significar que no hay contenedores, lo cual está bien
    Write-Host "  Contenedores detenidos" -ForegroundColor Green
} else {
    Write-Host "  No había contenedores en ejecución" -ForegroundColor Gray
}
Write-Host ""

# Paso 2: Eliminar todos los contenedores
Write-Host "[2/6] Eliminando todos los contenedores..." -ForegroundColor Yellow
$containers = docker ps -aq
if ($containers) {
    docker rm -f $containers 2>&1 | Out-Null
    Write-Host "  Contenedores eliminados" -ForegroundColor Green
} else {
    Write-Host "  No había contenedores para eliminar" -ForegroundColor Gray
}
Write-Host ""

# Paso 3: Eliminar todas las imágenes
Write-Host "[3/6] Eliminando todas las imágenes..." -ForegroundColor Yellow
$images = docker images -aq
if ($images) {
    docker rmi -f $images 2>&1 | Out-Null
    Write-Host "  Imágenes eliminadas" -ForegroundColor Green
} else {
    Write-Host "  No había imágenes para eliminar" -ForegroundColor Gray
}
Write-Host ""

# Paso 4: Eliminar todos los volúmenes
Write-Host "[4/6] Eliminando todos los volúmenes..." -ForegroundColor Yellow
$volumes = docker volume ls -q
if ($volumes) {
    docker volume rm $volumes 2>&1 | Out-Null
    Write-Host "  Volúmenes eliminados" -ForegroundColor Green
} else {
    Write-Host "  No había volúmenes para eliminar" -ForegroundColor Gray
}
Write-Host ""

# Paso 5: Eliminar todas las redes personalizadas (excepto las predeterminadas)
Write-Host "[5/6] Eliminando redes personalizadas..." -ForegroundColor Yellow
$networks = docker network ls -q --filter "type=custom"
if ($networks) {
    docker network rm $networks 2>&1 | Out-Null
    Write-Host "  Redes eliminadas" -ForegroundColor Green
} else {
    Write-Host "  No había redes personalizadas para eliminar" -ForegroundColor Gray
}
Write-Host ""

# Paso 6: Limpieza completa del sistema (incluye caché, contenedores huérfanos, etc.)
Write-Host "[6/6] Limpieza completa del sistema Docker..." -ForegroundColor Yellow
Write-Host "  Esto puede tardar unos momentos..." -ForegroundColor Gray
docker system prune -a --volumes -f
if ($LASTEXITCODE -eq 0) {
    Write-Host "  Sistema limpiado completamente" -ForegroundColor Green
} else {
    Write-Host "  Advertencia: Algunos elementos pueden no haberse eliminado" -ForegroundColor Yellow
}
Write-Host ""

# Mostrar espacio liberado
Write-Host "Espacio usado por Docker después de la limpieza:" -ForegroundColor Cyan
docker system df
Write-Host ""

# Resumen de limpieza
Write-Host "Resumen de limpieza:" -ForegroundColor Cyan
try {
    $containers = docker ps -aq 2>&1
    $containersCount = if ($containers -and $containers.Count -gt 0) { ($containers | Measure-Object -Line).Lines } else { 0 }
    
    $images = docker images -aq 2>&1
    $imagesCount = if ($images -and $images.Count -gt 0) { ($images | Measure-Object -Line).Lines } else { 0 }
    
    $volumes = docker volume ls -q 2>&1
    $volumesCount = if ($volumes -and $volumes.Count -gt 0) { ($volumes | Measure-Object -Line).Lines } else { 0 }
    
    Write-Host "  Contenedores restantes: $containersCount" -ForegroundColor White
    Write-Host "  Imágenes restantes: $imagesCount" -ForegroundColor White
    Write-Host "  Volúmenes restantes: $volumesCount" -ForegroundColor White
} catch {
    Write-Host "  No se pudo obtener el resumen" -ForegroundColor Yellow
}
Write-Host ""

Write-Host "========================================" -ForegroundColor Green
Write-Host "  Limpieza completada" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Docker está ahora completamente limpio." -ForegroundColor Green
Write-Host "Todos los contenedores, imágenes, volúmenes y caché han sido eliminados." -ForegroundColor Green
Write-Host ""
Write-Host "Para iniciar GateKeep nuevamente, ejecuta:" -ForegroundColor Cyan
Write-Host "  .\iniciar-docker.ps1" -ForegroundColor White
Write-Host ""

