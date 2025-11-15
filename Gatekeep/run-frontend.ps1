# Script para ejecutar GateKeep Frontend localmente
# Limpia procesos anteriores y ejecuta el frontend

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  GateKeep - Ejecutar Frontend Local" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Función para liberar puerto
function Free-Port {
    param([int]$PortNumber)
    
    $connections = Get-NetTCPConnection -LocalPort $PortNumber -ErrorAction SilentlyContinue
    
    if ($connections) {
        Write-Host "Puerto $PortNumber en uso. Liberando..." -ForegroundColor Yellow
        
        $processes = Get-Process -Name "node" -ErrorAction SilentlyContinue
        if ($processes) {
            foreach ($process in $processes) {
                $processConnections = Get-NetTCPConnection -OwningProcess $process.Id -LocalPort $PortNumber -ErrorAction SilentlyContinue
                if ($processConnections) {
                    Write-Host "  Terminando proceso Node.js (PID: $($process.Id))" -ForegroundColor Gray
                    Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
                }
            }
        }
        
        Start-Sleep -Seconds 2
        Write-Host "Puerto $PortNumber liberado" -ForegroundColor Green
    } else {
        Write-Host "Puerto $PortNumber disponible" -ForegroundColor Green
    }
}

# Navegar al directorio del frontend
$frontendPath = Join-Path $PSScriptRoot "frontend"

if (-not (Test-Path $frontendPath)) {
    Write-Host "Error: No se encontró el directorio frontend" -ForegroundColor Red
    Write-Host "Ruta buscada: $frontendPath" -ForegroundColor Gray
    exit 1
}

Write-Host "Frontend: $frontendPath" -ForegroundColor Gray
Write-Host ""

# Paso 1: Liberar puerto 3000
Write-Host "[1/4] Liberando puerto 3000..." -ForegroundColor Yellow
Free-Port -PortNumber 3000
Write-Host ""

# Paso 2: Limpiar cache
Write-Host "[2/4] Limpiando cache..." -ForegroundColor Yellow
Set-Location $frontendPath

if (Test-Path ".next") {
    Remove-Item -Recurse -Force ".next"
    Write-Host "  Cache .next eliminado" -ForegroundColor Gray
}

if (Test-Path "node_modules\.cache") {
    Remove-Item -Recurse -Force "node_modules\.cache"
    Write-Host "  Cache node_modules eliminado" -ForegroundColor Gray
}

Write-Host "Cache limpiado" -ForegroundColor Green
Write-Host ""

# Paso 3: Verificar dependencias
Write-Host "[3/4] Verificando dependencias..." -ForegroundColor Yellow

if (-not (Test-Path "node_modules")) {
    Write-Host "  node_modules no encontrado. Instalando dependencias..." -ForegroundColor Yellow
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error: Falló la instalación de dependencias" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "  Dependencias encontradas" -ForegroundColor Green
}

Write-Host ""

# Paso 4: Ejecutar
Write-Host "[4/4] Ejecutando servidor de desarrollo..." -ForegroundColor Yellow
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Frontend iniciado" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "URL disponible:" -ForegroundColor Cyan
Write-Host "  Frontend:  http://localhost:3000" -ForegroundColor White
Write-Host ""
Write-Host "Presiona Ctrl+C para detener el servidor" -ForegroundColor Gray
Write-Host ""

npm run dev

