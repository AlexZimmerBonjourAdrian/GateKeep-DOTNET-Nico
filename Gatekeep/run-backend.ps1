# Script para ejecutar GateKeep.Api localmente
# Limpia procesos anteriores y ejecuta el backend
# Prueba automáticamente las contraseñas de base de datos: 897888fg2 o 1234

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  GateKeep - Ejecutar Backend Local" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Función para liberar puerto
function Free-Port {
    param([int]$PortNumber)
    
    $connections = Get-NetTCPConnection -LocalPort $PortNumber -ErrorAction SilentlyContinue
    
    if ($connections) {
        Write-Host "Puerto $PortNumber en uso. Liberando..." -ForegroundColor Yellow
        
        $uniqueProcesses = $connections | Select-Object -Property OwningProcess -Unique
        
        foreach ($conn in $uniqueProcesses) {
            $process = Get-Process -Id $conn.OwningProcess -ErrorAction SilentlyContinue
            if ($process) {
                Write-Host "  Terminando proceso: $($process.Name) (PID: $($process.Id))" -ForegroundColor Gray
                Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
            }
        }
        
        Start-Sleep -Seconds 2
        Write-Host "Puerto $PortNumber liberado" -ForegroundColor Green
    } else {
        Write-Host "Puerto $PortNumber disponible" -ForegroundColor Green
    }
}

# Función para probar conexión a PostgreSQL
function Test-PostgresConnection {
    param([string]$Password)
    
    # Intentar con psql si está disponible
    $psqlPath = Get-Command psql -ErrorAction SilentlyContinue
    if ($psqlPath) {
        try {
            $env:PGPASSWORD = $Password
            $result = & psql -h localhost -U postgres -d Gatekeep -c "SELECT 1;" 2>&1 | Out-Null
            $success = $LASTEXITCODE -eq 0
            $env:PGPASSWORD = $null
            return $success
        } catch {
            $env:PGPASSWORD = $null
            return $false
        }
    }
    
    # Si psql no está disponible, intentar con Test-NetConnection para verificar que el puerto está abierto
    # y luego dejar que la aplicación pruebe la contraseña
    $portOpen = Test-NetConnection -ComputerName localhost -Port 5432 -WarningAction SilentlyContinue -InformationLevel Quiet
    if ($portOpen) {
        # El puerto está abierto, pero no podemos probar la contraseña sin psql
        # Retornamos null para indicar que no se pudo verificar
        return $null
    }
    
    return $false
}

# Función para encontrar contraseña válida
function Get-ValidPassword {
    Write-Host "Probando contraseñas de base de datos..." -ForegroundColor Yellow
    
    $passwords = @("897888fg2", "1234")
    
    foreach ($pwd in $passwords) {
        Write-Host "  Probando contraseña: $pwd" -ForegroundColor Gray
        $result = Test-PostgresConnection -Password $pwd
        
        if ($result -eq $true) {
            Write-Host "  Contraseña válida: $pwd" -ForegroundColor Green
            return $pwd
        } elseif ($result -eq $null) {
            # No se pudo verificar, pero el puerto está abierto
            # Usar esta contraseña y dejar que la aplicación pruebe
            Write-Host "  No se pudo verificar (psql no disponible), usando: $pwd" -ForegroundColor Yellow
            Write-Host "  La aplicación probará la conexión al iniciar" -ForegroundColor Gray
            return $pwd
        }
        # Si result es false, continuar con la siguiente contraseña
    }
    
    Write-Host "  Advertencia: No se pudo verificar ninguna contraseña" -ForegroundColor Yellow
    Write-Host "  Usando primera contraseña por defecto: 897888fg2" -ForegroundColor Yellow
    Write-Host "  Si falla, la aplicación intentará automáticamente con la segunda" -ForegroundColor Gray
    return "897888fg2"
}

# Navegar al directorio del proyecto
$apiPath = Join-Path $PSScriptRoot "src\GateKeep.Api"

if (-not (Test-Path $apiPath)) {
    Write-Host "Error: No se encontró el directorio de GateKeep.Api" -ForegroundColor Red
    Write-Host "Ruta buscada: $apiPath" -ForegroundColor Gray
    exit 1
}

Write-Host "Proyecto: $apiPath" -ForegroundColor Gray
Write-Host ""

# Paso 1: Liberar puerto 5011
Write-Host "[1/6] Liberando puerto 5011..." -ForegroundColor Yellow
Free-Port -PortNumber 5011
Write-Host ""

# Paso 2: Limpiar build anterior
Write-Host "[2/6] Limpiando build anterior..." -ForegroundColor Yellow
Set-Location $apiPath
dotnet clean --configuration Debug
if ($LASTEXITCODE -ne 0) {
    Write-Host "Advertencia: La limpieza tuvo problemas, pero continuamos..." -ForegroundColor Yellow
}
Write-Host ""

# Paso 3: Restaurar dependencias
Write-Host "[3/6] Restaurando dependencias NuGet..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Falló la restauración de dependencias" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Paso 4: Compilar
Write-Host "[4/6] Compilando proyecto..." -ForegroundColor Yellow
dotnet build --configuration Debug --no-incremental
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Falló la compilación" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Paso 5: Configurar contraseña de base de datos
Write-Host "[5/6] Configurando contraseña de base de datos..." -ForegroundColor Yellow
$validPassword = Get-ValidPassword
$env:DB_PASSWORD = $validPassword
$env:DATABASE__PASSWORD = $validPassword
Write-Host "Contraseña configurada: $validPassword" -ForegroundColor Green
Write-Host ""

# Paso 6: Ejecutar
Write-Host "[6/6] Ejecutando aplicación..." -ForegroundColor Yellow
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Backend iniciado" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "URLs disponibles:" -ForegroundColor Cyan
Write-Host "  Swagger:  http://localhost:5011/swagger" -ForegroundColor White
Write-Host "  Health:    http://localhost:5011/health" -ForegroundColor White
Write-Host ""
Write-Host "Presiona Ctrl+C para detener la aplicación" -ForegroundColor Gray
Write-Host ""

dotnet run --configuration Debug

