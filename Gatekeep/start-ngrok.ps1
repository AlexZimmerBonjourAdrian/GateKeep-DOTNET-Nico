# Script para iniciar ngrok y exponer la aplicación local
# Expone el puerto 80 (nginx) que enruta tanto frontend como backend

param(
    [switch]$Stop,
    [switch]$Status
)

$ErrorActionPreference = "Continue"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  GateKeep - Gestor de ngrok" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Función para obtener el estado de ngrok
function Get-NgrokStatus {
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:4040/api/tunnels" -TimeoutSec 3 -ErrorAction Stop
        if ($response.tunnels -and $response.tunnels.Count -gt 0) {
            $httpsTunnel = $response.tunnels | Where-Object { $_.proto -eq 'https' } | Select-Object -First 1
            if ($httpsTunnel) {
                return @{
                    Url = $httpsTunnel.public_url
                    Status = "running"
                }
            }
        }
    } catch {
        return @{
            Url = $null
            Status = "not_running"
        }
    }
    return @{
        Url = $null
        Status = "not_running"
    }
}

# Función para detener ngrok
function Stop-Ngrok {
    $processes = Get-Process -Name "ngrok" -ErrorAction SilentlyContinue
    if ($processes) {
        Write-Host "Deteniendo procesos de ngrok..." -ForegroundColor Yellow
        $processes | Stop-Process -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 2
        Write-Host "✓ ngrok detenido" -ForegroundColor Green
        return $true
    } else {
        Write-Host "No hay procesos de ngrok corriendo" -ForegroundColor Gray
        return $false
    }
}

# Si se solicita detener
if ($Stop) {
    Stop-Ngrok
    exit 0
}

# Si se solicita el estado
if ($Status) {
    $status = Get-NgrokStatus
    if ($status.Status -eq "running" -and $status.Url) {
        Write-Host "✓ ngrok está corriendo" -ForegroundColor Green
        Write-Host "URL: $($status.Url)" -ForegroundColor Cyan
        $domain = ($status.Url -replace 'https://', '')
        Write-Host "Backend esperado: https://api.$domain" -ForegroundColor Gray
    } else {
        Write-Host "✗ ngrok no está corriendo" -ForegroundColor Red
    }
    exit 0
}

# Verificar si ngrok está instalado
$ngrok = Get-Command ngrok -ErrorAction SilentlyContinue
if (-not $ngrok) {
    Write-Host "Error: ngrok no está instalado" -ForegroundColor Red
    Write-Host "Instala ngrok desde: https://ngrok.com/download" -ForegroundColor Yellow
    Write-Host "O usa: choco install ngrok" -ForegroundColor Yellow
    exit 1
}

Write-Host "✓ ngrok encontrado: $($ngrok.Path)" -ForegroundColor Green

# Configurar authtoken si no está configurado
Write-Host "Verificando configuración de ngrok..." -ForegroundColor Yellow
$ngrokConfigPath1 = "$env:LOCALAPPDATA\ngrok\ngrok.yml"
$ngrokConfigPath2 = "$env:APPDATA\ngrok\ngrok.yml"
$ngrokConfigPath3 = "$env:USERPROFILE\.ngrok2\ngrok.yml"
$authtoken = "2yK9mpFEZXa9gvXRS2IHS6baOgL_7ABJwf2GPr1zMA28yPgLd"

$configExists = (Test-Path $ngrokConfigPath1) -or (Test-Path $ngrokConfigPath2) -or (Test-Path $ngrokConfigPath3)

if (-not $configExists) {
    Write-Host "Configurando authtoken de ngrok..." -ForegroundColor Yellow
    & ngrok config add-authtoken $authtoken 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Authtoken configurado correctamente" -ForegroundColor Green
    } else {
        Write-Host "⚠ No se pudo configurar el authtoken automáticamente" -ForegroundColor Yellow
        Write-Host "Ejecuta manualmente: ngrok config add-authtoken $authtoken" -ForegroundColor Gray
    }
} else {
    Write-Host "✓ Configuración de ngrok encontrada" -ForegroundColor Green
}

# Verificar si ya hay un proceso de ngrok corriendo
$existingStatus = Get-NgrokStatus
if ($existingStatus.Status -eq "running") {
    Write-Host "Advertencia: ngrok ya está corriendo" -ForegroundColor Yellow
    Write-Host "URL actual: $($existingStatus.Url)" -ForegroundColor Cyan
    Write-Host "¿Deseas detenerlo y reiniciar? (S/N): " -NoNewline -ForegroundColor Yellow
    $response = Read-Host
    if ($response -eq 'S' -or $response -eq 's' -or $response -eq 'Y' -or $response -eq 'y') {
        Stop-Ngrok
        Start-Sleep -Seconds 2
    } else {
        Write-Host "Usando ngrok existente" -ForegroundColor Green
        Write-Host "Para ver el estado: .\start-ngrok.ps1 -Status" -ForegroundColor Gray
        Write-Host "Para detener: .\start-ngrok.ps1 -Stop" -ForegroundColor Gray
        exit 0
    }
} else {
    # Detener cualquier proceso zombie
    $zombieProcesses = Get-Process -Name "ngrok" -ErrorAction SilentlyContinue
    if ($zombieProcesses) {
        Write-Host "Limpiando procesos de ngrok..." -ForegroundColor Yellow
        Stop-Ngrok | Out-Null
    }
}

# Verificar que nginx esté corriendo en el puerto 80
Write-Host "Verificando que el puerto 80 esté disponible..." -ForegroundColor Yellow
$port80 = Get-NetTCPConnection -LocalPort 80 -ErrorAction SilentlyContinue
if (-not $port80) {
    Write-Host "Error: No hay ningún servicio escuchando en el puerto 80" -ForegroundColor Red
    Write-Host "Asegúrate de que Docker Compose esté corriendo:" -ForegroundColor Yellow
    Write-Host "  docker-compose -f docker-compose.prod.yml up -d" -ForegroundColor White
    exit 1
}
Write-Host "✓ Puerto 80 está disponible" -ForegroundColor Green

Write-Host ""
Write-Host "Iniciando ngrok en el puerto 80..." -ForegroundColor Yellow

# Iniciar ngrok en background
try {
    $ngrokProcess = Start-Process -FilePath "ngrok" -ArgumentList "http", "80" -PassThru -WindowStyle Hidden
    if (-not $ngrokProcess) {
        throw "No se pudo iniciar el proceso de ngrok"
    }
    Write-Host "✓ Proceso de ngrok iniciado (PID: $($ngrokProcess.Id))" -ForegroundColor Green
} catch {
    Write-Host "Error al iniciar ngrok: $_" -ForegroundColor Red
    exit 1
}

# Esperar a que ngrok se inicie y obtener la URL
Write-Host "Esperando a que ngrok se inicie..." -ForegroundColor Gray
$maxRetries = 15
$retryCount = 0
$publicUrl = $null

while ($retryCount -lt $maxRetries -and -not $publicUrl) {
    Start-Sleep -Seconds 2
    $status = Get-NgrokStatus
    if ($status.Status -eq "running" -and $status.Url) {
        $publicUrl = $status.Url
    } else {
        $retryCount++
        Write-Host "." -NoNewline -ForegroundColor Gray
    }
}
Write-Host ""

if ($publicUrl) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  ✓ ngrok iniciado correctamente" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "URL Pública HTTPS: " -NoNewline -ForegroundColor Cyan
    Write-Host "$publicUrl" -ForegroundColor White -BackgroundColor DarkBlue
    Write-Host ""
    
    $domain = ($publicUrl -replace 'https://', '')
    Write-Host "URLs esperadas:" -ForegroundColor Yellow
    Write-Host "  Frontend: $publicUrl" -ForegroundColor White
    Write-Host "  Backend:  " -NoNewline -ForegroundColor White
    Write-Host "https://api.$domain" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  Instrucciones" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "1. Abre esta URL en tu navegador:" -ForegroundColor Cyan
    Write-Host "   $publicUrl" -ForegroundColor White
    Write-Host ""
    Write-Host "2. Abre la consola del navegador (F12 → Network)" -ForegroundColor Cyan
    Write-Host "   y verifica que las peticiones vayan a:" -ForegroundColor Cyan
    Write-Host "   https://api.$domain/api/..." -ForegroundColor White
    Write-Host ""
    Write-Host "3. El frontend detectará automáticamente" -ForegroundColor Cyan
    Write-Host "   que está en HTTPS y construirá api.*" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "4. Dashboard de ngrok:" -ForegroundColor Cyan
    Write-Host "   http://localhost:4040" -ForegroundColor White
    Write-Host ""
    Write-Host "5. Comandos útiles:" -ForegroundColor Cyan
    Write-Host "   Ver estado: .\start-ngrok.ps1 -Status" -ForegroundColor White
    Write-Host "   Detener:   .\start-ngrok.ps1 -Stop" -ForegroundColor White
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    
    # Verificar que el backend responda
    Write-Host ""
    Write-Host "Verificando conectividad..." -ForegroundColor Yellow
    try {
        $healthCheck = Invoke-WebRequest -Uri "http://localhost:5011/health" -TimeoutSec 3 -UseBasicParsing -ErrorAction Stop
        if ($healthCheck.StatusCode -eq 200) {
            Write-Host "✓ Backend respondiendo correctamente" -ForegroundColor Green
        }
    } catch {
        Write-Host "⚠ Backend no responde aún (puede tardar unos segundos)" -ForegroundColor Yellow
    }
    
    Write-Host ""
    Write-Host "ngrok está corriendo. Presiona Ctrl+C para detener..." -ForegroundColor Gray
    Write-Host ""
    
    # Función para limpiar al salir
    function Cleanup-Ngrok {
        Write-Host "`nDeteniendo ngrok..." -ForegroundColor Yellow
        if ($ngrokProcess) {
            Stop-Process -Id $ngrokProcess.Id -Force -ErrorAction SilentlyContinue
        }
        Stop-Ngrok | Out-Null
        Write-Host "✓ ngrok detenido" -ForegroundColor Green
    }
    
    # Registrar handler para Ctrl+C
    [Console]::CancelKeyPress += {
        Cleanup-Ngrok
        exit 0
    }
    
    # Esperar indefinidamente hasta que el usuario presione Ctrl+C o ngrok se detenga
    try {
        while ($true) {
            Start-Sleep -Seconds 1
            # Verificar que ngrok siga corriendo
            $process = Get-Process -Id $ngrokProcess.Id -ErrorAction SilentlyContinue
            if (-not $process) {
                Write-Host "`n⚠ ngrok se detuvo inesperadamente" -ForegroundColor Yellow
                break
            }
        }
    } catch {
        Cleanup-Ngrok
    }
} else {
    Write-Host ""
    Write-Host "Error: No se pudo obtener la URL pública de ngrok" -ForegroundColor Red
    Write-Host ""
    Write-Host "Posibles soluciones:" -ForegroundColor Yellow
    Write-Host "1. Verifica manualmente en: http://localhost:4040" -ForegroundColor White
    Write-Host "2. Asegúrate de tener una cuenta de ngrok configurada" -ForegroundColor White
    Write-Host "3. Verifica que el puerto 4040 no esté en uso" -ForegroundColor White
    Write-Host ""
    Write-Host "Para obtener la URL manualmente:" -ForegroundColor Yellow
    Write-Host "  Invoke-RestMethod http://localhost:4040/api/tunnels | ConvertTo-Json" -ForegroundColor White
    Write-Host ""
    
    # Intentar detener el proceso si existe
    if ($ngrokProcess) {
        Stop-Process -Id $ngrokProcess.Id -Force -ErrorAction SilentlyContinue
    }
    exit 1
}

