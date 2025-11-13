# Script para instalar y ejecutar Redis con Docker
# GateKeep - Sistema de Caching

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  GateKeep - Instalación de Redis" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Verificar si Docker está instalado
$dockerInstalled = Get-Command docker -ErrorAction SilentlyContinue
if (-not $dockerInstalled) {
    Write-Host "[ERROR] Docker no está instalado." -ForegroundColor Red
    Write-Host "  Por favor, instala Docker Desktop desde: https://www.docker.com/products/docker-desktop" -ForegroundColor Yellow
    exit 1
}

Write-Host "[OK] Docker está instalado" -ForegroundColor Green

# Función para verificar si el puerto 6379 está en uso
function Test-Port {
    param([int]$Port = 6379)
    
    $connection = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue
    return $connection -ne $null
}

# Verificar si el contenedor de Redis ya existe
$existingContainer = docker ps -a --filter "name=redis-gatekeep" --format "{{.Names}}"

if ($existingContainer -eq "redis-gatekeep") {
    Write-Host ""
    Write-Host "Contenedor 'redis-gatekeep' encontrado." -ForegroundColor Yellow
    
    # Verificar si está corriendo
    $isRunning = docker ps --filter "name=redis-gatekeep" --format "{{.Names}}"
    
    if ($isRunning -eq "redis-gatekeep") {
        Write-Host "[OK] Redis ya está corriendo" -ForegroundColor Green
        Write-Host ""
        Write-Host "Puedes verificar el estado con:" -ForegroundColor Cyan
        Write-Host "  docker ps | findstr redis-gatekeep" -ForegroundColor White
    } else {
        Write-Host "Iniciando contenedor existente..." -ForegroundColor Yellow
        docker start redis-gatekeep | Out-Null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "[OK] Redis iniciado correctamente" -ForegroundColor Green
        } else {
            Write-Host "[ERROR] No se pudo iniciar el contenedor existente" -ForegroundColor Red
            exit 1
        }
    }
} else {
    # Verificar si el puerto 6379 está ocupado
    Write-Host ""
    Write-Host "Verificando disponibilidad del puerto 6379..." -ForegroundColor Yellow
    
    if (Test-Port -Port 6379) {
        Write-Host "[ADVERTENCIA] El puerto 6379 está siendo usado por otro proceso" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Buscando contenedores huérfanos de Redis..." -ForegroundColor Yellow
        
        # Buscar cualquier contenedor de Redis corriendo
        $allRedisContainers = docker ps --filter "ancestor=redis" --format "{{.Names}}"
        
        if ($allRedisContainers) {
            Write-Host "Contenedores Redis encontrados:" -ForegroundColor Yellow
            docker ps --filter "ancestor=redis" --format "table {{.Names}}\t{{.Ports}}\t{{.Status}}"
            Write-Host ""
            
            $response = Read-Host "¿Deseas detener estos contenedores? (S/N)"
            if ($response -eq "S" -or $response -eq "s") {
                foreach ($container in $allRedisContainers) {
                    Write-Host "Deteniendo $container..." -ForegroundColor Yellow
                    docker stop $container | Out-Null
                    docker rm $container | Out-Null
                }
                Write-Host "[OK] Contenedores detenidos" -ForegroundColor Green
            } else {
                Write-Host "[INFO] No se modificaron los contenedores existentes" -ForegroundColor Cyan
                Write-Host "       No se puede crear redis-gatekeep mientras el puerto esté ocupado" -ForegroundColor Cyan
                exit 0
            }
        } else {
            # El puerto está ocupado pero no es un contenedor Docker
            Write-Host "[INFO] El puerto está ocupado por un proceso que no es Docker" -ForegroundColor Cyan
            Write-Host ""
            Write-Host "Para ver qué proceso usa el puerto 6379:" -ForegroundColor Yellow
            Write-Host "  netstat -ano | findstr :6379" -ForegroundColor White
            Write-Host ""
            
            $response = Read-Host "¿Intentar usar el puerto 6380 en su lugar? (S/N)"
            if ($response -eq "S" -or $response -eq "s") {
                Write-Host ""
                Write-Host "Creando contenedor de Redis en puerto 6380..." -ForegroundColor Yellow
                docker run -d --name redis-gatekeep -p 6380:6379 redis:latest | Out-Null
                
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "[OK] Redis instalado y corriendo en puerto 6380" -ForegroundColor Green
                    Write-Host "[IMPORTANTE] Actualiza config.json con: 'localhost:6380'" -ForegroundColor Yellow
                } else {
                    Write-Host "[ERROR] Error al crear el contenedor de Redis" -ForegroundColor Red
                    exit 1
                }
                
                Write-Host ""
                Write-Host "================================================" -ForegroundColor Cyan
                Write-Host "  Información de Conexión" -ForegroundColor Cyan
                Write-Host "================================================" -ForegroundColor Cyan
                Write-Host "  Host: localhost" -ForegroundColor White
                Write-Host "  Puerto: 6380 (modificado)" -ForegroundColor Yellow
                Write-Host "  Instancia: GateKeep:" -ForegroundColor White
                Write-Host ""
                Write-Host "[ACCIÓN REQUERIDA] Actualiza src/GateKeep.Api/config.json:" -ForegroundColor Yellow
                Write-Host '  "connectionString": "localhost:6380"' -ForegroundColor White
                Write-Host ""
                exit 0
            } else {
                Write-Host "[INFO] Operación cancelada" -ForegroundColor Cyan
                exit 0
            }
        }
    }
    
    Write-Host ""
    Write-Host "Creando nuevo contenedor de Redis..." -ForegroundColor Yellow
    docker run -d --name redis-gatekeep -p 6379:6379 redis:latest | Out-Null
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[OK] Redis instalado y corriendo correctamente" -ForegroundColor Green
    } else {
        Write-Host "[ERROR] Error al crear el contenedor de Redis" -ForegroundColor Red
        Write-Host "Ejecuta 'docker logs redis-gatekeep' para más detalles" -ForegroundColor Yellow
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

Write-Host "[OK] Redis está listo para usar con GateKeep" -ForegroundColor Green
Write-Host ""
