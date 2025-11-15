# Script para iniciar GateKeep con Docker
# Ubicación: Gatekeep/iniciar-docker.ps1

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "  Iniciando GateKeep con Docker  " -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

# Verificar Docker Desktop
Write-Host "[1/5] Verificando Docker Desktop..." -ForegroundColor Yellow
$dockerProcess = Get-Process "Docker Desktop" -ErrorAction SilentlyContinue
if (!$dockerProcess) {
    Write-Host "Docker Desktop no está ejecutándose. Iniciando..." -ForegroundColor Yellow
    Start-Process "C:\Program Files\Docker\Docker\Docker Desktop.exe"
    Write-Host "Esperando a que Docker Desktop inicie (30 segundos)..." -ForegroundColor Yellow
    Start-Sleep -Seconds 30
} else {
    Write-Host "Docker Desktop está ejecutándose" -ForegroundColor Green
}

# Navegar al directorio src
Write-Host ""
Write-Host "[2/5] Navegando al directorio src..." -ForegroundColor Yellow
$srcPath = Join-Path $PSScriptRoot "src"
Set-Location $srcPath
Write-Host "Directorio actual: $srcPath" -ForegroundColor Green

# Verificar archivo .env
Write-Host ""
Write-Host "[3/5] Verificando archivo .env..." -ForegroundColor Yellow
$envPath = Join-Path $srcPath ".env"
if (!(Test-Path $envPath)) {
    Write-Host "ERROR: Archivo .env no encontrado en: $envPath" -ForegroundColor Red
    Write-Host "Copia .env.example a .env y configura tus variables" -ForegroundColor Red
    Write-Host ""
    Write-Host "Comando: Copy-Item '.env.example' '.env'" -ForegroundColor Yellow
    Read-Host "Presiona Enter para salir"
    exit 1
}
Write-Host "Archivo .env encontrado" -ForegroundColor Green

# Detener servicios anteriores (si existen)
Write-Host ""
Write-Host "[4/5] Verificando servicios existentes..." -ForegroundColor Yellow
$existingContainers = docker-compose ps -q
if ($existingContainers) {
    Write-Host "Deteniendo servicios anteriores..." -ForegroundColor Yellow
    docker-compose down
}

# Levantar servicios
Write-Host ""
Write-Host "[5/6] Levantando servicios con Docker Compose..." -ForegroundColor Yellow
docker-compose up -d

if ($LASTEXITCODE -eq 0) {
    # Esperar a que los servicios se inicien
    Write-Host ""
    Write-Host "[6/6] Esperando a que los servicios se estabilicen..." -ForegroundColor Yellow
    Write-Host "  Esto puede tardar hasta 2 minutos..." -ForegroundColor Gray
    Start-Sleep -Seconds 30
    
    # Verificar estado de los servicios
    $maxWait = 120  # 2 minutos máximo
    $elapsed = 0
    $allHealthy = $false
    
    while ($elapsed -lt $maxWait -and -not $allHealthy) {
        Start-Sleep -Seconds 10
        $elapsed += 10
        
        $psOutput = docker-compose ps
        $unhealthyCount = ($psOutput | Select-String -Pattern "unhealthy|Error|Exited" | Measure-Object).Count
        
        if ($unhealthyCount -eq 0) {
            $allHealthy = $true
            Write-Host "  Todos los servicios están listos" -ForegroundColor Green
        } else {
            Write-Host "  Esperando... ($elapsed/$maxWait segundos)" -ForegroundColor Gray
        }
    }
    
    if (-not $allHealthy) {
        Write-Host "  Algunos servicios pueden necesitar más tiempo" -ForegroundColor Yellow
    }
    Write-Host ""
    Write-Host "==================================" -ForegroundColor Green
    Write-Host "  Servicios iniciados con éxito  " -ForegroundColor Green
    Write-Host "==================================" -ForegroundColor Green
    Write-Host ""
    
    # Mostrar estado
    Write-Host "Estado de los servicios:" -ForegroundColor Cyan
    docker-compose ps
    
    Write-Host ""
    Write-Host "URLs disponibles:" -ForegroundColor Cyan
    Write-Host "  API Swagger:  http://localhost:5011/swagger" -ForegroundColor White
    Write-Host "  Health Check: http://localhost:5011/health" -ForegroundColor White
    Write-Host "  Seq (Logs):   http://localhost:5341" -ForegroundColor White
    Write-Host "  Prometheus:   http://localhost:9090" -ForegroundColor White
    Write-Host "  Grafana:      http://localhost:3001" -ForegroundColor White
    Write-Host ""
    
    # Preguntar si abrir aplicaciones
    $openApps = Read-Host "Abrir aplicaciones y herramientas? (S/N)"
    if ($openApps -eq "S" -or $openApps -eq "s") {
        $openAppsScript = Join-Path $PSScriptRoot "open-apps.ps1"
        if (Test-Path $openAppsScript) {
            & $openAppsScript -Mode "Docker"
        } else {
            Write-Host "Abriendo Swagger..." -ForegroundColor Yellow
            Start-Process "http://localhost:5011/swagger"
        }
    }
    
    Write-Host ""
    Write-Host "Para ver logs: docker-compose logs -f api" -ForegroundColor Yellow
    Write-Host "Para detener:  docker-compose down" -ForegroundColor Yellow
    Write-Host "Para abrir apps: .\open-apps.ps1" -ForegroundColor Yellow
    Write-Host ""
    
} else {
    Write-Host ""
    Write-Host "ERROR: Falló al iniciar los servicios" -ForegroundColor Red
    Write-Host ""
    
    # Mostrar estado de los servicios
    Write-Host "Estado de los servicios:" -ForegroundColor Cyan
    docker-compose ps
    Write-Host ""
    
    # Identificar servicios con problemas
    Write-Host "Servicios con problemas:" -ForegroundColor Yellow
    
    # Obtener lista de servicios problemáticos
    $problematicServices = @()
    $psOutput = docker-compose ps
    $lines = $psOutput -split "`n"
    
    foreach ($line in $lines) {
        if ($line -match "unhealthy|Error|Exited" -and $line -match "gatekeep-") {
            $parts = $line -split '\s+', 5
            if ($parts.Length -ge 2) {
                $containerName = $parts[0]
                $state = $parts[3]
                if ($state -like "*unhealthy*" -or $state -like "*Error*" -or $state -like "*Exited*") {
                    $problematicServices += @{
                        Name = $containerName
                        State = $state
                        ServiceName = $containerName -replace 'gatekeep-', ''
                    }
                    Write-Host "  - $containerName : $state" -ForegroundColor Red
                }
            }
        }
    }
    
    if ($problematicServices.Count -gt 0) {
        Write-Host ""
        
        # Mostrar logs de los servicios problemáticos
        Write-Host "Mostrando logs de los servicios con problemas (últimas 20 líneas):" -ForegroundColor Cyan
        Write-Host ""
        
        foreach ($service in $problematicServices) {
            Write-Host "=== Logs de $($service.Name) ===" -ForegroundColor Yellow
            docker-compose logs --tail 20 $service.ServiceName 2>&1
            Write-Host ""
        }
        
        Write-Host "Opciones:" -ForegroundColor Cyan
        Write-Host "  1. Reintentar iniciar los servicios" -ForegroundColor White
        Write-Host "  2. Ver todos los logs: docker-compose logs" -ForegroundColor White
        Write-Host "  3. Reiniciar servicios problemáticos: docker-compose restart [nombre-servicio]" -ForegroundColor White
        Write-Host "  4. Continuar de todas formas (algunos servicios pueden no estar listos)" -ForegroundColor White
        Write-Host ""
        
        $opcion = Read-Host "Selecciona una opción (1-4) o presiona Enter para salir"
        
        switch ($opcion) {
            "1" {
                Write-Host ""
                Write-Host "Reintentando iniciar servicios..." -ForegroundColor Yellow
                docker-compose up -d
                Write-Host ""
                Write-Host "Esperando 10 segundos para que los servicios se estabilicen..." -ForegroundColor Gray
                Start-Sleep -Seconds 10
                docker-compose ps
            }
            "2" {
                Write-Host ""
                Write-Host "Mostrando todos los logs..." -ForegroundColor Yellow
                docker-compose logs
            }
            "4" {
                Write-Host ""
                Write-Host "Continuando de todas formas..." -ForegroundColor Yellow
                Write-Host "Algunos servicios pueden no estar completamente listos." -ForegroundColor Yellow
                Write-Host "Puedes intentar iniciar la API manualmente:" -ForegroundColor Cyan
                Write-Host "  docker-compose up -d api" -ForegroundColor White
            }
            default {
                Write-Host ""
                Write-Host "Saliendo..." -ForegroundColor Yellow
            }
        }
    } else {
        Write-Host "No se detectaron servicios con problemas obvios en el análisis." -ForegroundColor Yellow
        Write-Host "Revisa los logs con: docker-compose logs" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Si algunos servicios están 'unhealthy', pueden necesitar más tiempo." -ForegroundColor Gray
        Write-Host "Intenta esperar unos minutos y verificar con: docker-compose ps" -ForegroundColor Gray
    }
    
    Write-Host ""
}

Read-Host "Presiona Enter para salir"

