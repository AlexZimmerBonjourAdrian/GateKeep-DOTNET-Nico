# Script para construir y probar la PWA de GateKeep
# Uso: .\build-and-test-pwa.ps1 [--api-url <url>] [--skip-build] [--skip-test]

param(
    [Parameter(Mandatory=$false)]
    [string]$ApiUrl = "https://api.zimmzimmgames.com",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipBuild,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipTest,
    
    [Parameter(Mandatory=$false)]
    [switch]$EnableSW
)

$ErrorActionPreference = "Stop"

Write-Host "=== Script de Construcción y Prueba de PWA ===" -ForegroundColor Cyan
Write-Host ""

# Obtener el directorio del script
$ScriptDir = $PSScriptRoot
$FrontendDir = $ScriptDir

# Verificar que estamos en el directorio correcto
if (-not (Test-Path (Join-Path $FrontendDir "package.json"))) {
    Write-Host "Error: No se encontró package.json. Asegúrate de ejecutar el script desde el directorio frontend." -ForegroundColor Red
    exit 1
}

Set-Location $FrontendDir

# 1. Crear archivo .env.local con la configuración
Write-Host "1. Configurando variables de entorno..." -ForegroundColor Yellow
$envLocalPath = Join-Path $FrontendDir ".env.local"
$envContent = @"
NEXT_PUBLIC_API_URL=$ApiUrl
NEXT_PUBLIC_ENABLE_SW=$($EnableSW.IsPresent -or $true)
"@

Set-Content -Path $envLocalPath -Value $envContent -Encoding UTF8
Write-Host "   Archivo .env.local creado con:" -ForegroundColor Green
Write-Host "   - NEXT_PUBLIC_API_URL=$ApiUrl" -ForegroundColor White
Write-Host "   - NEXT_PUBLIC_ENABLE_SW=true" -ForegroundColor White
Write-Host ""

# 2. Instalar dependencias si es necesario
if (-not (Test-Path (Join-Path $FrontendDir "node_modules"))) {
    Write-Host "2. Instalando dependencias..." -ForegroundColor Yellow
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Host "   Error instalando dependencias" -ForegroundColor Red
        exit 1
    }
    Write-Host "   Dependencias instaladas" -ForegroundColor Green
    Write-Host ""
} else {
    Write-Host "2. Dependencias ya instaladas" -ForegroundColor Green
    Write-Host ""
}

# 3. Construir la PWA
if (-not $SkipBuild) {
    Write-Host "3. Construyendo la PWA..." -ForegroundColor Yellow
    $env:NODE_ENV = "production"
    npm run build
    if ($LASTEXITCODE -ne 0) {
        Write-Host "   Error construyendo la PWA" -ForegroundColor Red
        exit 1
    }
    Write-Host "   Build completado exitosamente" -ForegroundColor Green
    Write-Host ""
} else {
    Write-Host "3. Saltando construcción (--skip-build)" -ForegroundColor Yellow
    Write-Host ""
}

# 4. Verificar archivos PWA
Write-Host "4. Verificando archivos PWA..." -ForegroundColor Yellow
$pwaFiles = @(
    @{Path="public/manifest.json"; Required=$true; Description="Manifest de PWA"},
    @{Path="public/sw.js"; Required=$true; Description="Service Worker"},
    @{Path="public/offline.html"; Required=$true; Description="Página offline"},
    @{Path=".next"; Required=$true; Description="Directorio de build"}
)

$allFilesPresent = $true
foreach ($file in $pwaFiles) {
    $fullPath = Join-Path $FrontendDir $file.Path
    if (Test-Path $fullPath) {
        Write-Host "   $($file.Description): OK" -ForegroundColor Green
    } else {
        if ($file.Required) {
            Write-Host "   $($file.Description): FALTANTE" -ForegroundColor Red
            $allFilesPresent = $false
        } else {
            Write-Host "   $($file.Description): No encontrado (opcional)" -ForegroundColor Yellow
        }
    }
}

if (-not $allFilesPresent) {
    Write-Host "   Algunos archivos PWA están faltando" -ForegroundColor Red
    exit 1
}
Write-Host "   Todos los archivos PWA están presentes" -ForegroundColor Green
Write-Host ""

# 5. Verificar contenido del manifest
Write-Host "5. Verificando manifest.json..." -ForegroundColor Yellow
$manifestPath = Join-Path $FrontendDir "public/manifest.json"
if (Test-Path $manifestPath) {
    try {
        $manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
        Write-Host "   Nombre: $($manifest.name)" -ForegroundColor White
        Write-Host "   Short Name: $($manifest.short_name)" -ForegroundColor White
        Write-Host "   Display: $($manifest.display)" -ForegroundColor White
        Write-Host "   Icons: $($manifest.icons.Count) iconos" -ForegroundColor White
        Write-Host "   Manifest válido" -ForegroundColor Green
    } catch {
        Write-Host "   Error leyendo manifest.json: $_" -ForegroundColor Red
    }
}
Write-Host ""

# 6. Verificar Service Worker
Write-Host "6. Verificando Service Worker..." -ForegroundColor Yellow
$swPath = Join-Path $FrontendDir "public/sw.js"
if (Test-Path $swPath) {
    $swSize = (Get-Item $swPath).Length
    Write-Host "   sw.js encontrado ($([math]::Round($swSize/1KB, 2)) KB)" -ForegroundColor Green
    Write-Host "   Service Worker listo" -ForegroundColor Green
} else {
    Write-Host "   sw.js no encontrado" -ForegroundColor Red
}
Write-Host ""

# 7. Probar la PWA (opcional)
if (-not $SkipTest) {
    Write-Host "7. Iniciando servidor de desarrollo para pruebas..." -ForegroundColor Yellow
    Write-Host "   El servidor se iniciará en: http://localhost:3000" -ForegroundColor Cyan
    Write-Host "   Presiona Ctrl+C para detener el servidor" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "   Para probar la PWA:" -ForegroundColor White
    Write-Host "   1. Abre http://localhost:3000 en tu navegador" -ForegroundColor White
    Write-Host "   2. Abre DevTools (F12) -> Application -> Service Workers" -ForegroundColor White
    Write-Host "   3. Verifica que el Service Worker esté registrado" -ForegroundColor White
    Write-Host "   4. Prueba modo offline: DevTools -> Network -> Offline" -ForegroundColor White
    Write-Host "   5. Verifica IndexedDB: DevTools -> Application -> IndexedDB" -ForegroundColor White
    Write-Host "   6. Verifica Manifest: DevTools -> Application -> Manifest" -ForegroundColor White
    Write-Host ""
    
    # Iniciar servidor de desarrollo
    $env:NODE_ENV = "development"
    npm run dev
} else {
    Write-Host "7. Saltando pruebas (--skip-test)" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "   Para probar manualmente:" -ForegroundColor White
    Write-Host "   npm run dev" -ForegroundColor Cyan
    Write-Host ""
}

Write-Host "=== Resumen ===" -ForegroundColor Cyan
Write-Host "PWA construida y lista para probar" -ForegroundColor Green
Write-Host "Archivos PWA verificados" -ForegroundColor Green
Write-Host "Configuración:" -ForegroundColor White
Write-Host "  - API URL: $ApiUrl" -ForegroundColor White
Write-Host "  - Service Worker: Habilitado" -ForegroundColor White
Write-Host ""
Write-Host "Para desplegar a producción, usa:" -ForegroundColor Yellow
Write-Host "  terraform/scripts/upload-frontend-to-s3.ps1" -ForegroundColor Cyan

