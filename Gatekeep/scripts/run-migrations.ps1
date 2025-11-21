# ==============================================
# Script para Ejecutar Migraciones de Base de Datos
# ==============================================

param(
    [Parameter(Mandatory=$true)]
    [string]$ConnectionString,
    
    [string]$ProjectPath = "src\GateKeep.Api"
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Ejecutando Migraciones de Base de Datos" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$scriptRoot = Split-Path -Parent $PSScriptRoot
$fullProjectPath = Join-Path $scriptRoot $ProjectPath

if (-not (Test-Path $fullProjectPath)) {
    Write-Host "Error: No se encontró el proyecto en: $fullProjectPath" -ForegroundColor Red
    exit 1
}

Write-Host "Proyecto: $fullProjectPath" -ForegroundColor Gray
Write-Host "Connection String: $($ConnectionString -replace 'Password=[^;]+', 'Password=***')" -ForegroundColor Gray
Write-Host ""

Push-Location $fullProjectPath
try {
    Write-Host "Verificando migraciones pendientes..." -ForegroundColor Yellow
    
    # Listar migraciones
    dotnet ef migrations list --connection $ConnectionString 2>&1 | ForEach-Object {
        Write-Host "  $_" -ForegroundColor Gray
    }
    
    Write-Host ""
    Write-Host "Aplicando migraciones..." -ForegroundColor Yellow
    
    # Aplicar migraciones
    dotnet ef database update --connection $ConnectionString 2>&1 | ForEach-Object {
        Write-Host "  $_" -ForegroundColor Gray
    }
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error al ejecutar migraciones" -ForegroundColor Red
        exit 1
    }
    
    Write-Host ""
    Write-Host "Migraciones aplicadas exitosamente" -ForegroundColor Green
    
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
    exit 1
} finally {
    Pop-Location
}

