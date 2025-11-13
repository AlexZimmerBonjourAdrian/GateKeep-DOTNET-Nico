# Script para limpiar, compilar y ejecutar GateKeep.Api
# Ubicación: Gatekeep/scripts/run.ps1

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "  GateKeep.Api - Build & Run     " -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

# Obtener la ruta del proyecto
$projectPath = Join-Path $PSScriptRoot "..\src\GateKeep.Api\GateKeep.Api.csproj"
$projectPath = Resolve-Path $projectPath -ErrorAction SilentlyContinue

if (-not $projectPath) {
    Write-Host "Error: No se encontró el archivo del proyecto en: $projectPath" -ForegroundColor Red
    exit 1
}

Write-Host "Proyecto: $projectPath" -ForegroundColor Gray
Write-Host ""

# Paso 1: Limpiar
Write-Host "[1/4] Limpiando proyecto..." -ForegroundColor Yellow
dotnet clean $projectPath --configuration Debug
if ($LASTEXITCODE -ne 0) {
    Write-Host "Advertencia: La limpieza tuvo problemas, pero continuamos..." -ForegroundColor Yellow
}
Write-Host ""

# Paso 2: Restaurar dependencias
Write-Host "[2/4] Restaurando dependencias NuGet..." -ForegroundColor Yellow
dotnet restore $projectPath
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Falló la restauración de dependencias" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Paso 3: Compilar
Write-Host "[3/4] Compilando proyecto..." -ForegroundColor Yellow
dotnet build $projectPath --configuration Debug --no-incremental
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Falló la compilación" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Paso 4: Ejecutar
Write-Host "[4/4] Ejecutando aplicación..." -ForegroundColor Yellow
Write-Host ""
Write-Host "Presiona Ctrl+C para detener la aplicación" -ForegroundColor Gray
Write-Host ""
dotnet run --project $projectPath --configuration Debug
