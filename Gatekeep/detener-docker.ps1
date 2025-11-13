# Script para detener GateKeep Docker
# Ubicación: Gatekeep/detener-docker.ps1

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "  Deteniendo GateKeep Docker     " -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

# Navegar al directorio src
$srcPath = Join-Path $PSScriptRoot "src"
Set-Location $srcPath
Write-Host "Directorio: $srcPath" -ForegroundColor Yellow
Write-Host ""

# Mostrar servicios en ejecución
Write-Host "Servicios actuales:" -ForegroundColor Yellow
docker-compose ps
Write-Host ""

# Preguntar confirmación
$confirm = Read-Host "Detener todos los servicios? (S/N)"
if ($confirm -eq "S" -or $confirm -eq "s") {
    Write-Host ""
    Write-Host "Deteniendo servicios..." -ForegroundColor Yellow
    docker-compose down
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "Servicios detenidos exitosamente" -ForegroundColor Green
        Write-Host ""
    } else {
        Write-Host ""
        Write-Host "ERROR al detener servicios" -ForegroundColor Red
        Write-Host ""
    }
} else {
    Write-Host ""
    Write-Host "Operación cancelada" -ForegroundColor Yellow
    Write-Host ""
}

Read-Host "Presiona Enter para salir"

