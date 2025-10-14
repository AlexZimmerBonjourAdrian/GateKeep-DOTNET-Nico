param(
    [int]$Port = 4200,
    [switch]$Open
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Write-Info($msg) { Write-Host "[INFO] $msg" -ForegroundColor Cyan }
function Write-Warn($msg) { Write-Host "[WARN] $msg" -ForegroundColor Yellow }
function Write-Err($msg)  { Write-Host "[ERROR] $msg" -ForegroundColor Red }

function Test-Command {
    param([Parameter(Mandatory=$true)][string]$Name)
    $null -ne (Get-Command $Name -ErrorAction SilentlyContinue)
}

function Get-NodeVersionMajorMinor {
    try {
        $raw = (& node --version).Trim()  # e.g. v20.11.1
        if ($raw -match '^v(\d+)\.(\d+)\.(\d+)$') {
            return @{ Major = [int]$Matches[1]; Minor = [int]$Matches[2]; Patch = [int]$Matches[3]; Raw = $raw }
        }
        return $null
    } catch {
        return $null
    }
}

function Test-PortInUse {
    param([int]$CheckPort)
    try {
        $conn = Get-NetTCPConnection -State Listen -LocalPort $CheckPort -ErrorAction SilentlyContinue
        if ($null -ne $conn) { return $true }
    } catch {
        # Fallback a netstat si Get-NetTCPConnection no está disponible
        $lines = & netstat -ano | Select-String -Pattern ":$CheckPort\s+LISTENING"
        if ($lines) { return $true }
    }
    return $false
}

function Find-FreePort {
    param([int]$StartPort)
    $p = [Math]::Max(1024, $StartPort)
    while (Test-PortInUse -CheckPort $p) { $p++ }
    return $p
}

# Cambiar al directorio del script (raíz del frontend)
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

if (-not (Test-Path -Path './package.json')) {
    Write-Err 'No se encontró package.json. Ejecuta este script dentro de la carpeta frontend.'
    exit 1
}

Write-Info 'Validando dependencias...'

if (-not (Test-Command -Name 'node')) {
    Write-Err 'Node.js no está instalado o no está en PATH. Instala Node 18+ (recomendado 20 LTS).'
    exit 1
}

if (-not (Test-Command -Name 'npm')) {
    Write-Err 'npm no está instalado o no está en PATH.'
    exit 1
}

$ver = Get-NodeVersionMajorMinor
if ($null -eq $ver) {
    Write-Warn 'No se pudo determinar la versión de Node. Continuando bajo tu propio riesgo.'
} else {
    if ($ver.Major -lt 18) {
        Write-Err "Se requiere Node 18+ (recomendado 20 LTS). Detectado: $($ver.Raw)"
        exit 1
    }
    Write-Info "Node detectado: $($ver.Raw)"
}

Write-Info 'Instalando dependencias...'
$useCi = Test-Path -Path './package-lock.json'
try {
    if ($useCi) {
        Write-Info 'Usando npm ci (lockfile encontrado)'
        & npm ci | Write-Host
    } else {
        Write-Warn 'No se encontró package-lock.json. Usando npm install.'
        & npm install | Write-Host
    }
} catch {
    Write-Warn "Fallo instalando dependencias: $($_.Exception.Message)"
    Write-Info 'Intentando recuperación: limpiando instalación parcial y reintentando con npm install...'
    try { if (Test-Path './node_modules') { Remove-Item -Recurse -Force './node_modules' } } catch {}
    try { if (Test-Path './package-lock.json') { Remove-Item -Force './package-lock.json' } } catch {}
    try { & npm cache clean --force | Write-Host } catch {}
    & npm install | Write-Host
}

# Preferir CLI local
$ngPath = Join-Path $scriptDir 'node_modules/.bin/ng.cmd'
if (-not (Test-Path $ngPath)) {
    Write-Warn 'Angular CLI local no encontrado. Se usará npx ng.'
}

# Resolver puerto libre
if (Test-PortInUse -CheckPort $Port) {
    $free = Find-FreePort -StartPort $Port
    Write-Warn "El puerto $Port está en uso. Se usará el puerto $free."
    $Port = $free
}

Write-Info "Levantando el servidor de desarrollo en http://localhost:$Port ..."
$argsList = @('start', '--', '--port', "$Port")
if ($Open.IsPresent) { $argsList += '--open' }

# Iniciar servidor
try {
    # Usamos npm start para respetar scripts y CLI local
    & npm @argsList
} catch {
    Write-Err "Fallo al iniciar el servidor: $($_.Exception.Message)"
    exit 1
}


