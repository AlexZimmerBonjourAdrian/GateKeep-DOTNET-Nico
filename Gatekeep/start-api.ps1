# Script para iniciar GateKeep.Api con configuracion de puerto flexible
# Uso:
#   .\start-api.ps1                    # Inicia en puerto por defecto (5011)
#   .\start-api.ps1 -Port 5000         # Inicia en puerto especifico
#   .\start-api.ps1 -Force             # Libera automaticamente el puerto ocupado
#   .\start-api.ps1 -Port 5000 -Force  # Combina ambas opciones

param(
    [int]$Port = 5011,
    [switch]$Force,
    [switch]$NoBuild
)

# Colores para output
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

function Write-Header {
    param([string]$Title)
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "  $Title" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
}

# Funcion para liberar puerto
function Free-Port {
    param(
        [int]$PortNumber,
        [bool]$ForceKill
    )
    
    $connections = Get-NetTCPConnection -LocalPort $PortNumber -ErrorAction SilentlyContinue
    
    if ($connections) {
        Write-ColorOutput "`nAdvertencia: Puerto $PortNumber en uso" "Yellow"
        
        $uniqueProcesses = $connections | Select-Object -Property OwningProcess -Unique
        
        foreach ($conn in $uniqueProcesses) {
            $process = Get-Process -Id $conn.OwningProcess -ErrorAction SilentlyContinue
            if ($process) {
                Write-ColorOutput "  - PID: $($process.Id) | Proceso: $($process.Name)" "Gray"
                
                if ($ForceKill) {
                    Write-ColorOutput "    Terminando proceso automaticamente..." "Red"
                    Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
                    Write-ColorOutput "    Proceso terminado" "Green"
                } else {
                    $response = Read-Host "    Terminar este proceso? (s/n)"
                    if ($response -eq 's' -or $response -eq 'S') {
                        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
                        Write-ColorOutput "    Proceso terminado" "Green"
                    } else {
                        Write-ColorOutput "    No se puede iniciar la API. Saliendo..." "Red"
                        exit 1
                    }
                }
            }
        }
        
        Write-ColorOutput "`nEsperando a que el puerto se libere..." "Yellow"
        Start-Sleep -Seconds 2
        
        # Verificar que el puerto este libre
        $stillInUse = Get-NetTCPConnection -LocalPort $PortNumber -ErrorAction SilentlyContinue
        if ($stillInUse) {
            Write-ColorOutput "Error: El puerto $PortNumber todavia esta en uso" "Red"
            exit 1
        }
        
        Write-ColorOutput "Puerto $PortNumber liberado correctamente" "Green"
    } else {
        Write-ColorOutput "Puerto $PortNumber disponible" "Green"
    }
}

# Banner de inicio
Write-Header "GateKeep API Launcher"

Write-ColorOutput "Configuracion:" "Cyan"
Write-ColorOutput "  Puerto: $Port" "White"
Write-ColorOutput "  Forzar: $(if ($Force) { 'Si' } else { 'No' })" "White"
Write-ColorOutput "  Sin compilar: $(if ($NoBuild) { 'Si' } else { 'No' })" "White"

# Liberar puerto si esta ocupado
Write-ColorOutput "`nVerificando disponibilidad del puerto..." "Cyan"
Free-Port -PortNumber $Port -ForceKill $Force

# Configurar variable de entorno
$env:GATEKEEP_PORT = $Port
Write-ColorOutput "`nVariable de entorno establecida: GATEKEEP_PORT=$Port" "Green"

# Navegar al directorio del proyecto
$apiPath = Join-Path $PSScriptRoot "Gatekeep\src\GateKeep.Api"
if (-not (Test-Path $apiPath)) {
    $apiPath = Join-Path $PSScriptRoot "src\GateKeep.Api"
}

if (-not (Test-Path $apiPath)) {
    Write-ColorOutput "`nError: No se encontro el directorio de GateKeep.Api" "Red"
    Write-ColorOutput "Ruta buscada: $apiPath" "Gray"
    exit 1
}

Write-ColorOutput "`nNavegando a: $apiPath" "Cyan"
Set-Location $apiPath

# Compilar y ejecutar
Write-Header "Iniciando GateKeep.Api"

if ($NoBuild) {
    Write-ColorOutput "Ejecutando sin compilar..." "Yellow"
    dotnet run --no-build
} else {
    Write-ColorOutput "Compilando y ejecutando..." "Green"
    dotnet run
}

# Mensaje de salida
Write-Host ""
Write-ColorOutput "GateKeep.Api detenida" "Yellow"
Write-Host ""

