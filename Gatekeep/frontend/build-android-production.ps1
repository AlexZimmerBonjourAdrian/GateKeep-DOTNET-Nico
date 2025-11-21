# Script para generar APK de Android desde la PWA de GateKeep (Producción)
# Uso: .\build-android-production.ps1 [--url <pwa-url>] [--package-name <package>]

param(
    [Parameter(Mandatory=$false)]
    [string]$PwaUrl = "https://zimmzimmgames.com",
    
    [Parameter(Mandatory=$false)]
    [string]$PackageName = "com.gatekeep.app"
)

$ErrorActionPreference = "Stop"
$ScriptDir = $PSScriptRoot
$AndroidDir = Join-Path $ScriptDir "android"
$LogPath = Join-Path $ScriptDir "build-android.log"

if (Test-Path $LogPath) {
    Remove-Item $LogPath -Force
}
"=== LOG build-android-production.ps1 ===" | Out-File -FilePath $LogPath -Encoding UTF8
"$(Get-Date -Format o)" | Out-File -FilePath $LogPath -Append

Write-Host "=== Generando APK Android para Producción ===" -ForegroundColor Cyan
Write-Host "PWA URL: $PwaUrl" -ForegroundColor White
Write-Host "Package: $PackageName" -ForegroundColor White
Write-Host ""

Set-Location $ScriptDir

# 1. Configurar entorno Java (JDK 11+)
Write-Host "1. Configurando Java..." -ForegroundColor Yellow
function Get-JavaPath {
    $paths = @(
        "C:\Program Files\Eclipse Adoptium",
        "C:\Program Files\Java",
        "C:\Program Files\Microsoft",
        "C:\Program Files\Azul",
        "${env:ProgramFiles}\Java"
    )
    
    foreach ($p in $paths) {
        if (Test-Path $p) {
            $java = Get-ChildItem $p -Filter "java.exe" -Recurse | Select-Object -First 1
            if ($java) { return $java }
        }
    }
    return $null
}

if (-not (Get-Command java -ErrorAction SilentlyContinue)) {
    $javaExe = Get-JavaPath
    if ($javaExe) {
        $env:JAVA_HOME = $javaExe.DirectoryName -replace "\\bin$", ""
        $env:PATH = "$($javaExe.DirectoryName);$env:PATH"
        Write-Host "   ✅ Java configurado desde: $($javaExe.FullName)" -ForegroundColor Green
    } else {
        Write-Host "   ❌ Java no encontrado. Instala JDK 11+." -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "   ✅ Java detectado en PATH" -ForegroundColor Green
}
Write-Host ""

# 2. Verificar Bubblewrap
Write-Host "2. Verificando Bubblewrap..." -ForegroundColor Yellow
if (-not (Get-Command bubblewrap -ErrorAction SilentlyContinue)) {
    try {
        $bw = npx @bubblewrap/cli --version 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "   ✅ Bubblewrap disponible vía npx" -ForegroundColor Green
        } else {
            throw "npx falló"
        }
    } catch {
        Write-Host "   Instalando Bubblewrap globalmente..." -ForegroundColor Gray
        npm install -g @bubblewrap/cli
    }
}

# 3. Preparar TWA Manifest e Iconos
Write-Host "3. Preparando configuración TWA..." -ForegroundColor Yellow

# Validar icono local
$localIconPath = Join-Path $ScriptDir "public/assets/LogoGateKeep.webp"
if (-not (Test-Path $localIconPath)) {
    Write-Host "❌ Error: No se encuentra el icono local en $localIconPath" -ForegroundColor Red
    exit 1
}

Write-Host "⚠️  AVISO: Bubblewrap requiere que el manifest esté en una URL HTTPS accesible públicamente." -ForegroundColor Yellow
Write-Host "   Se verificará $PwaUrl/manifest.json antes de continuar." -ForegroundColor Yellow

function Test-ManifestUrl {
    param([string]$Url)
    try {
        $response = Invoke-WebRequest -Uri $Url -Method Head -TimeoutSec 10
        return ($response.StatusCode -eq 200)
    } catch {
        return $false
    }
}

$manifestUrl = "$PwaUrl/manifest.json"
if (-not (Test-ManifestUrl $manifestUrl)) {
    Write-Host "❌ No se pudo acceder a $manifestUrl" -ForegroundColor Red
    Write-Host "   Asegúrate de desplegar la PWA en producción o usa un túnel HTTPS temporal (ngrok/localtunnel)." -ForegroundColor Yellow
    Write-Host "   Ejemplo túnel: npx localtunnel --port 3000 --subdomain gatekeep" -ForegroundColor Gray
    exit 1
}

$twaManifestPath = Join-Path $ScriptDir "twa-manifest.json"
$twaConfig = @{
    packageId = $PackageName
    host = "zimmzimmgames.com"
    name = "GateKeep"
    launcherName = "GateKeep"
    display = "standalone"
    themeColor = "#0066cc"
    navigationColor = "#0066cc"
    backgroundColor = "#ffffff"
    startUrl = "/"
    iconUrl = "$PwaUrl/assets/LogoGateKeep.webp"
    maskableIconUrl = "$PwaUrl/assets/LogoGateKeep.webp"
    appVersionName = "1.0.0"
    appVersionCode = 1
    generatorApp = "bubblewrap-cli"
    webManifestUrl = "$PwaUrl/manifest.json"
    signingKey = @{
        path = "./android.keystore"
        alias = "android"
    }
}
$twaConfig | ConvertTo-Json -Depth 4 | Set-Content $twaManifestPath
Write-Host "   ✅ Configuración TWA lista" -ForegroundColor Green

# 4. Construir APK
Write-Host "4. Construyendo APK..." -ForegroundColor Yellow

# Limpiar anterior
if (Test-Path $AndroidDir) { Remove-Item -Recurse -Force $AndroidDir }
New-Item -ItemType Directory -Path $AndroidDir | Out-Null

# Init
Write-Host "   Inicializando proyecto Android..." -ForegroundColor Cyan
# Aquí está el cambio clave: pasamos la URL HTTPS del manifest, NO la ruta del archivo local.
# Bubblewrap descargará el manifest de esa URL.
$initCmd = "npx @bubblewrap/cli init --manifest `"$manifestUrl`" --directory `"$AndroidDir`""
Write-Host "   > $initCmd" -ForegroundColor DarkGray

try {
    # Ejecutar init redirigiendo salida
    $initOutput = Invoke-Expression $initCmd 2>&1
    $initOutput | Out-File -FilePath $LogPath -Append
    
    if ($LASTEXITCODE -ne 0) {
        throw "Falló init. Revisa si $PwaUrl/manifest.json es accesible."
    }
} catch {
    Write-Host "❌ Error en init: $_" -ForegroundColor Red
    Write-Host "   DETALLE: Bubblewrap necesita descargar el manifest desde $manifestUrl" -ForegroundColor Yellow
    Write-Host "   Asegúrate de que tu PWA esté desplegada en esa URL (HTTPS) antes de generar el APK." -ForegroundColor Yellow
    exit 1
}

# Build
Write-Host "   Compilando APK..." -ForegroundColor Cyan
Set-Location $AndroidDir
$buildCmd = "npx @bubblewrap/cli build"
Write-Host "   > $buildCmd" -ForegroundColor DarkGray

try {
    $buildOutput = Invoke-Expression $buildCmd 2>&1
    $buildOutput | Out-File -FilePath $LogPath -Append
    
    if ($LASTEXITCODE -ne 0) {
        throw "Falló build."
    }
    
    $apk = Get-ChildItem -Filter "*.apk" -Recurse | Select-Object -First 1
    Write-Host ""
    Write-Host "=== ✅ APK LISTO ===" -ForegroundColor Green
    Write-Host "Archivo: $($apk.FullName)" -ForegroundColor White
    explorer.exe ($apk.DirectoryName)

} catch {
    Write-Host "❌ Error en build: $_" -ForegroundColor Red
    Write-Host "   Consulta el log en: $LogPath" -ForegroundColor Yellow
} finally {
    Set-Location $ScriptDir
}
