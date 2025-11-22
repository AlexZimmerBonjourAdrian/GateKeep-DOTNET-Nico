# Script para detener todos los procesos de ngrok
# Uso: .\stop-ngrok.ps1

$ErrorActionPreference = "Continue"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Deteniendo procesos de ngrok" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Buscar todos los procesos de ngrok
$processes = Get-Process -Name "ngrok" -ErrorAction SilentlyContinue

if ($processes) {
    Write-Host "Encontrados $($processes.Count) proceso(s) de ngrok:" -ForegroundColor Yellow
    $processes | ForEach-Object {
        Write-Host "  - PID: $($_.Id) | Iniciado: $($_.StartTime)" -ForegroundColor Gray
    }
    Write-Host ""
    Write-Host "Deteniendo procesos..." -ForegroundColor Yellow
    
    # Detener todos los procesos
    $processes | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
    
    # Verificar que se detuvieron
    $remaining = Get-Process -Name "ngrok" -ErrorAction SilentlyContinue
    if ($remaining) {
        Write-Host "Algunos procesos no se detuvieron, intentando de nuevo..." -ForegroundColor Yellow
        $remaining | Stop-Process -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 1
    }
    
    $finalCheck = Get-Process -Name "ngrok" -ErrorAction SilentlyContinue
    if (-not $finalCheck) {
        Write-Host "Todos los procesos de ngrok han sido detenidos" -ForegroundColor Green
    } else {
        Write-Host "Algunos procesos aun estan corriendo:" -ForegroundColor Red
        $finalCheck | ForEach-Object {
            Write-Host "  - PID: $($_.Id)" -ForegroundColor Red
        }
    }
} else {
    Write-Host "No hay procesos de ngrok corriendo" -ForegroundColor Gray
}

Write-Host ""

