# Script para recrear completamente los contenedores Docker
# Ubicación: Gatekeep/recrear-docker.ps1

param(
    [switch]$NonInteractive,
    [int]$Option = 1  # 1 = mantener datos, 2 = eliminar datos
)

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "  Recrear GateKeep Docker        " -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "ADVERTENCIA: Esto recreará todos los contenedores" -ForegroundColor Yellow
Write-Host ""

# Navegar al directorio src
$srcPath = Join-Path $PSScriptRoot "src"
if (-not (Test-Path $srcPath)) {
    Write-Host "Error: No se encontró el directorio src en: $srcPath" -ForegroundColor Red
    exit 1
}
Set-Location $srcPath

# Preguntar si mantener datos (solo si no es no interactivo)
if (-not $NonInteractive) {
    Write-Host "Opciones:" -ForegroundColor Cyan
    Write-Host "  1. Recrear contenedores (mantener datos)" -ForegroundColor White
    Write-Host "  2. Recrear todo (ELIMINAR DATOS)" -ForegroundColor Red
    Write-Host "  3. Cancelar" -ForegroundColor White
    Write-Host ""
    $opcion = Read-Host "Selecciona una opción (1-3)"
} else {
    $opcion = $Option.ToString()
    Write-Host "Modo no interactivo: usando opción $opcion" -ForegroundColor Cyan
    Write-Host ""
}

switch ($opcion) {
    "1" {
        Write-Host ""
        Write-Host "[1/4] Deteniendo servicios..." -ForegroundColor Yellow
        docker-compose down
        
        Write-Host "[2/4] Reconstruyendo imágenes..." -ForegroundColor Yellow
        docker-compose build --no-cache
        
        Write-Host "[3/4] Levantando servicios..." -ForegroundColor Yellow
        docker-compose up -d
        
        Write-Host "[4/4] Verificando estado..." -ForegroundColor Yellow
        docker-compose ps
        
        Write-Host ""
        Write-Host "Contenedores recreados exitosamente (datos mantenidos)" -ForegroundColor Green
        Write-Host ""
    }
    "2" {
        Write-Host ""
        if (-not $NonInteractive) {
            Write-Host "ULTIMA ADVERTENCIA: Esto eliminará TODOS los datos (PostgreSQL, Redis, etc.)" -ForegroundColor Red
            $confirmar = Read-Host "Estás seguro? Escribe 'ELIMINAR' para confirmar"
        } else {
            $confirmar = "ELIMINAR"
            Write-Host "Modo no interactivo: eliminando datos automáticamente" -ForegroundColor Yellow
        }
        
        if ($confirmar -eq "ELIMINAR") {
            Write-Host ""
            Write-Host "[1/5] Deteniendo y eliminando contenedores..." -ForegroundColor Yellow
            docker-compose down -v
            
            Write-Host "[2/5] Limpiando sistema Docker..." -ForegroundColor Yellow
            docker system prune -f
            
            Write-Host "[3/5] Reconstruyendo imágenes..." -ForegroundColor Yellow
            docker-compose build --no-cache
            
            Write-Host "[4/5] Levantando servicios..." -ForegroundColor Yellow
            docker-compose up -d
            
            Write-Host "[5/5] Verificando estado..." -ForegroundColor Yellow
            docker-compose ps
            
            Write-Host ""
            Write-Host "Sistema completamente recreado" -ForegroundColor Green
            Write-Host "NOTA: Base de datos está vacía, se creará automáticamente al conectar" -ForegroundColor Yellow
            Write-Host ""
        } else {
            Write-Host ""
            Write-Host "Operación cancelada" -ForegroundColor Yellow
            Write-Host ""
        }
    }
    "3" {
        Write-Host ""
        Write-Host "Operación cancelada" -ForegroundColor Yellow
        Write-Host ""
    }
    default {
        Write-Host ""
        Write-Host "Opción inválida" -ForegroundColor Red
        Write-Host ""
        exit 1
    }
}

if (-not $NonInteractive) {
    Read-Host "Presiona Enter para salir"
}

