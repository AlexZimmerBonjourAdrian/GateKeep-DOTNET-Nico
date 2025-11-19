# Script para generar APK de Android desde la PWA de GateKeep
# Usa Bubblewrap (TWA - Trusted Web Activity) de Google
# Uso: .\build-android-apk.ps1 [--url <pwa-url>] [--package-name <package>] [--skip-build]

param(
    [Parameter(Mandatory=$false)]
    [string]$PwaUrl = "https://zimmzimmgames.com",
    
    [Parameter(Mandatory=$false)]
    [string]$PackageName = "com.gatekeep.app",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipBuild,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipSign
)

$ErrorActionPreference = "Stop"

Write-Host "=== Generador de APK Android para GateKeep PWA ===" -ForegroundColor Cyan
Write-Host ""

# Obtener el directorio del script
$ScriptDir = $PSScriptRoot
$FrontendDir = $ScriptDir
$AndroidDir = Join-Path $FrontendDir "android"

Set-Location $FrontendDir

# 1. Verificar que el build de Next.js existe
if (-not $SkipBuild) {
    Write-Host "1. Verificando build de Next.js..." -ForegroundColor Yellow
    if (-not (Test-Path ".next")) {
        Write-Host "   Build no encontrado. Construyendo..." -ForegroundColor Yellow
        $env:NODE_ENV = "production"
        npm run build
        if ($LASTEXITCODE -ne 0) {
            Write-Host "   Error: Falló la construcción de Next.js" -ForegroundColor Red
            exit 1
        }
        Write-Host "   ✅ Build completado" -ForegroundColor Green
    } else {
        Write-Host "   ✅ Build de Next.js encontrado" -ForegroundColor Green
    }
    Write-Host ""
} else {
    Write-Host "1. Saltando verificación de build (--skip-build)" -ForegroundColor Yellow
    Write-Host ""
}

# 2. Verificar/Instalar Bubblewrap
Write-Host "2. Verificando Bubblewrap..." -ForegroundColor Yellow
$bubblewrapInstalled = $false
try {
    $bubblewrapVersion = npx @bubblewrap/cli --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        $bubblewrapInstalled = $true
        Write-Host "   ✅ Bubblewrap instalado" -ForegroundColor Green
    }
} catch {
    # Bubblewrap no está instalado
}

if (-not $bubblewrapInstalled) {
    Write-Host "   Bubblewrap no encontrado. Instalando..." -ForegroundColor Yellow
    Write-Host "   Nota: Esto instalará Bubblewrap globalmente usando npm" -ForegroundColor White
    Write-Host ""
    Write-Host "   Opción 1: Instalar globalmente (recomendado)" -ForegroundColor Cyan
    Write-Host "   npm install -g @bubblewrap/cli" -ForegroundColor White
    Write-Host ""
    Write-Host "   Opción 2: Usar npx (sin instalación)" -ForegroundColor Cyan
    Write-Host "   npx @bubblewrap/cli init" -ForegroundColor White
    Write-Host ""
    $install = Read-Host "¿Instalar Bubblewrap globalmente ahora? (S/N)"
    if ($install -eq "S" -or $install -eq "s") {
        npm install -g @bubblewrap/cli
        if ($LASTEXITCODE -ne 0) {
            Write-Host "   Error: No se pudo instalar Bubblewrap" -ForegroundColor Red
            exit 1
        }
        Write-Host "   ✅ Bubblewrap instalado" -ForegroundColor Green
    } else {
        Write-Host "   Usando npx para ejecutar Bubblewrap" -ForegroundColor Yellow
    }
}
Write-Host ""

# 3. Crear configuración TWA
Write-Host "3. Creando configuración TWA..." -ForegroundColor Yellow
$twaManifestPath = Join-Path $FrontendDir "twa-manifest.json"

if (-not (Test-Path $twaManifestPath)) {
    Write-Host "   Creando twa-manifest.json..." -ForegroundColor Yellow
    
    # Leer manifest.json para obtener información
    $manifestPath = Join-Path $FrontendDir "public/manifest.json"
    $manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
    
    $twaManifest = @{
        packageId = $PackageName
        name = $manifest.name
        launcherName = $manifest.short_name
        display = "standalone"
        themeColor = $manifest.theme_color
        navigationColor = $manifest.theme_color
        backgroundColor = $manifest.background_color
        enableNotifications = $true
        startUrl = "/"
        iconUrl = $manifest.icons[0].src
        maskableIconUrl = $manifest.icons[0].src
        splashScreenFadeOutDuration = 300
        signingKey = @{
            path = "./android.keystore"
            alias = "android"
        }
        appVersionName = "1.0.0"
        appVersionCode = 1
        shortcuts = @()
        generatorApp = "bubblewrap-cli"
        webManifestUrl = "$PwaUrl/manifest.json"
    }
    
    # Agregar shortcuts si existen
    if ($manifest.shortcuts) {
        foreach ($shortcut in $manifest.shortcuts) {
            $twaManifest.shortcuts += @{
                name = $shortcut.name
                shortName = $shortcut.short_name
                url = $shortcut.url
                icons = @(@{
                    src = $shortcut.icons[0].src
                    sizes = $shortcut.icons[0].sizes
                })
            }
        }
    }
    
    $twaManifest | ConvertTo-Json -Depth 10 | Set-Content -Path $twaManifestPath -Encoding UTF8
    Write-Host "   ✅ twa-manifest.json creado" -ForegroundColor Green
} else {
    Write-Host "   ✅ twa-manifest.json ya existe" -ForegroundColor Green
}
Write-Host ""

# 4. Inicializar proyecto Android (si no existe)
Write-Host "4. Inicializando proyecto Android..." -ForegroundColor Yellow
if (-not (Test-Path $AndroidDir)) {
    Write-Host "   Creando proyecto Android con Bubblewrap..." -ForegroundColor Yellow
    Write-Host "   URL de la PWA: $PwaUrl" -ForegroundColor White
    Write-Host "   Package Name: $PackageName" -ForegroundColor White
    Write-Host ""
    
    # Crear el directorio primero para evitar confirmación interactiva
    New-Item -ItemType Directory -Path $AndroidDir -Force | Out-Null
    Write-Host "   Directorio android creado" -ForegroundColor Green
    
    # Usar npx para ejecutar bubblewrap init
    # Nota: Bubblewrap puede pedir confirmación interactiva
    Write-Host "   Ejecutando Bubblewrap init..." -ForegroundColor Yellow
    Write-Host "   (Puede pedir confirmación - responde 'Yes' o 'Y' si se solicita)" -ForegroundColor Gray
    
    # Ejecutar el comando y capturar la salida
    $initOutput = npx @bubblewrap/cli init --manifest "$twaManifestPath" --directory "$AndroidDir" 2>&1
    
    # Mostrar la salida
    $initOutput | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "   Error: No se pudo inicializar el proyecto Android" -ForegroundColor Red
        Write-Host "   Intenta ejecutar manualmente:" -ForegroundColor Yellow
        Write-Host "   npx @bubblewrap/cli init --manifest twa-manifest.json --directory android" -ForegroundColor White
        exit 1
    }
    Write-Host "   ✅ Proyecto Android creado" -ForegroundColor Green
} else {
    Write-Host "   ✅ Proyecto Android ya existe" -ForegroundColor Green
}
Write-Host ""

# Función para verificar versión de Java
function Test-JavaVersion {
    param([string]$versionOutput)
    
    # Extraer número de versión (ej: "1.8.0_461" -> 8, "17.0.1" -> 17)
    if ($versionOutput -match 'version "(\d+)\.(\d+)') {
        $major = [int]$matches[1]
        $minor = [int]$matches[2]
        
        # Java 9+ usa formato diferente (ej: "17.0.1")
        if ($major -eq 1) {
            return $minor  # Java 8 = 8
        } else {
            return $major  # Java 17 = 17
        }
    }
    return 0
}

# Función para buscar Java en el sistema
function Find-Java {
    Write-Host "   Buscando Java en el sistema..." -ForegroundColor Yellow
    
    # 1. Verificar en PATH
    try {
        $javaCmd = Get-Command java -ErrorAction SilentlyContinue
        if ($javaCmd) {
            $versionOutput = java -version 2>&1 | Select-Object -First 1
            $version = Test-JavaVersion $versionOutput
            
            if ($version -ge 11) {
                Write-Host "   ✅ Java encontrado en PATH: $versionOutput" -ForegroundColor Green
                return $true
            } else {
                Write-Host "   ⚠️  Java encontrado pero versión antigua (v$version). Se requiere Java 11 o superior" -ForegroundColor Yellow
                return $false
            }
        }
    } catch {
        # Java no está en PATH
    }
    
    # 2. Buscar en ubicaciones comunes
    $javaPaths = @(
        "${env:ProgramFiles}\Java",
        "${env:ProgramFiles(x86)}\Java",
        "${env:ProgramFiles}\Eclipse Adoptium",
        "${env:ProgramFiles}\Microsoft",
        "${env:LOCALAPPDATA}\Programs\Eclipse Adoptium",
        "C:\Program Files\Java",
        "C:\Program Files (x86)\Java"
    )
    
    foreach ($path in $javaPaths) {
        if (Test-Path $path) {
            $javaExe = Get-ChildItem -Path $path -Filter "java.exe" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
            if ($javaExe) {
                # Verificar versión
                $tempPath = $env:PATH
                $env:PATH = "$($javaExe.DirectoryName);$env:PATH"
                try {
                    $versionOutput = & $javaExe.FullName -version 2>&1 | Select-Object -First 1
                    $version = Test-JavaVersion $versionOutput
                    
                    if ($version -ge 11) {
                        Write-Host "   ✅ Java encontrado en: $($javaExe.DirectoryName)" -ForegroundColor Green
                        Write-Host "   Versión: $versionOutput" -ForegroundColor White
                        return $true
                    } else {
                        Write-Host "   ⚠️  Java encontrado pero versión antigua (v$version) en: $($javaExe.DirectoryName)" -ForegroundColor Yellow
                    }
                } catch {
                    # No se pudo verificar versión
                }
                $env:PATH = $tempPath
            }
        }
    }
    
    # 3. Verificar JAVA_HOME
    if ($env:JAVA_HOME) {
        $javaExe = Join-Path $env:JAVA_HOME "bin\java.exe"
        if (Test-Path $javaExe) {
            try {
                $versionOutput = & $javaExe -version 2>&1 | Select-Object -First 1
                $version = Test-JavaVersion $versionOutput
                
                if ($version -ge 11) {
                    Write-Host "   ✅ Java encontrado en JAVA_HOME: $env:JAVA_HOME" -ForegroundColor Green
                    Write-Host "   Versión: $versionOutput" -ForegroundColor White
                    $env:PATH = "$env:JAVA_HOME\bin;$env:PATH"
                    return $true
                } else {
                    Write-Host "   ⚠️  Java en JAVA_HOME es versión antigua (v$version)" -ForegroundColor Yellow
                }
            } catch {
                # No se pudo verificar versión
            }
        }
    }
    
    return $false
}

# Función para instalar Java automáticamente
function Install-Java {
    Write-Host "   Java no encontrado. Intentando instalar automáticamente..." -ForegroundColor Yellow
    
    # Opción 1: Intentar con Chocolatey (si está instalado)
    $chocoInstalled = Get-Command choco -ErrorAction SilentlyContinue
    if ($chocoInstalled) {
        Write-Host "   Chocolatey encontrado. Instalando Java JDK 17 (LTS)..." -ForegroundColor Cyan
        Write-Host "   Esto puede tomar varios minutos..." -ForegroundColor Yellow
        
        try {
            # Instalar Java JDK 17 (LTS) con Chocolatey
            choco install temurin17jdk -y --no-progress
            if ($LASTEXITCODE -eq 0) {
                Write-Host "   ✅ Java instalado exitosamente con Chocolatey" -ForegroundColor Green
                # Refrescar PATH
                $env:PATH = [System.Environment]::GetEnvironmentVariable("Path", "Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path", "User")
                Start-Sleep -Seconds 2
                return $true
            }
        } catch {
            Write-Host "   ⚠️  Error instalando con Chocolatey: $_" -ForegroundColor Yellow
        }
    }
    
    # Opción 2: Descargar e instalar manualmente desde Adoptium
    Write-Host "   Chocolatey no disponible. Descargando Java desde Adoptium..." -ForegroundColor Cyan
    Write-Host "   URL: https://adoptium.net/temurin/releases/" -ForegroundColor White
    Write-Host ""
    Write-Host "   Por favor, descarga e instala Java JDK 17 (LTS) manualmente:" -ForegroundColor Yellow
    Write-Host "   1. Visita: https://adoptium.net/temurin/releases/?version=17" -ForegroundColor Cyan
    Write-Host "   2. Descarga: Windows x64 JDK (Installer)" -ForegroundColor White
    Write-Host "   3. Ejecuta el instalador" -ForegroundColor White
    Write-Host "   4. Vuelve a ejecutar este script" -ForegroundColor White
    Write-Host ""
    
    # Intentar abrir el navegador
    $openBrowser = Read-Host "¿Abrir el navegador para descargar Java? (S/N)"
    if ($openBrowser -eq "S" -or $openBrowser -eq "s") {
        Start-Process "https://adoptium.net/temurin/releases/?version=17"
    }
    
    return $false
}

# 5. Construir APK
Write-Host "5. Verificando Java..." -ForegroundColor Yellow
Set-Location $AndroidDir

# Buscar Java en el sistema
$javaInstalled = Find-Java

# Si no se encontró, intentar instalar
if (-not $javaInstalled) {
    Write-Host "   ⚠️  Java no encontrado en el sistema" -ForegroundColor Yellow
    $install = Read-Host "¿Instalar Java automáticamente? (S/N)"
    if ($install -eq "S" -or $install -eq "s") {
        $javaInstalled = Install-Java
        if ($javaInstalled) {
            # Verificar nuevamente después de la instalación
            $javaInstalled = Find-Java
        }
    } else {
        Write-Host "   Instalación cancelada. Java es necesario para construir el APK" -ForegroundColor Yellow
        Write-Host "   Instala Java manualmente y vuelve a ejecutar este script" -ForegroundColor White
    }
}

if ($javaInstalled) {
    Write-Host "   Construyendo APK con Bubblewrap..." -ForegroundColor Yellow
    npx @bubblewrap/cli build
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "   Error: Falló la construcción del APK" -ForegroundColor Red
        Write-Host "   Revisa los errores arriba" -ForegroundColor Yellow
        exit 1
    }
    
    # Buscar el APK generado
    $apkPath = Get-ChildItem -Path $AndroidDir -Filter "*.apk" -Recurse | Where-Object { $_.Name -like "*release*.apk" -or $_.Name -like "*app-release*.apk" } | Select-Object -First 1
    
    if ($apkPath) {
        Write-Host "   ✅ APK generado: $($apkPath.FullName)" -ForegroundColor Green
        Write-Host ""
        Write-Host "=== APK GENERADO EXITOSAMENTE ===" -ForegroundColor Green
        Write-Host "Ubicación: $($apkPath.FullName)" -ForegroundColor White
        Write-Host "Tamaño: $([math]::Round($apkPath.Length/1MB, 2)) MB" -ForegroundColor White
        Write-Host ""
        Write-Host "Para instalar en un dispositivo Android:" -ForegroundColor Yellow
        Write-Host "  adb install $($apkPath.FullName)" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "O transfiere el archivo al dispositivo e instálalo manualmente" -ForegroundColor White
    } else {
        Write-Host "   ⚠️  APK no encontrado. Revisa la salida de Bubblewrap" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ⚠️  No se puede construir APK sin Java" -ForegroundColor Yellow
    Write-Host "   Instala Java JDK y vuelve a ejecutar este script" -ForegroundColor White
}

Set-Location $FrontendDir

Write-Host ""
Write-Host "=== Resumen ===" -ForegroundColor Cyan
Write-Host "PWA URL: $PwaUrl" -ForegroundColor White
Write-Host "Package Name: $PackageName" -ForegroundColor White
Write-Host "Directorio Android: $AndroidDir" -ForegroundColor White
Write-Host ""
Write-Host "Para más información sobre Bubblewrap:" -ForegroundColor Yellow
Write-Host "  https://github.com/GoogleChromeLabs/bubblewrap" -ForegroundColor Cyan

