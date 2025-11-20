# Script para generar APK de Android desde la PWA de GateKeep usando ngrok
# Uso: .\build-android.ps1 [--package-name <package>] [--skip-build] [--skip-ngrok]

param(
    [Parameter(Mandatory=$false)]
    [string]$PackageName = "com.gatekeep.app",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipBuild,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipNgrok,
    
    [Parameter(Mandatory=$false)]
    [string]$PwaUrl = $null
)

$ErrorActionPreference = "Stop"
$ScriptDir = $PSScriptRoot
$AndroidDir = Join-Path $ScriptDir "android"
$IconPath = Join-Path $ScriptDir "public\assets\LogoGateKeep.webp"
$IconServerPort = 8000
$NextJsPort = 3000

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  GateKeep - Build Android APK con ngrok" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Set-Location $ScriptDir

# Función para obtener URL de ngrok
function Get-NgrokUrl {
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:4040/api/tunnels" -TimeoutSec 3 -ErrorAction Stop
        if ($response.tunnels -and $response.tunnels.Count -gt 0) {
            $httpsTunnel = $response.tunnels | Where-Object { $_.proto -eq 'https' } | Select-Object -First 1
            if ($httpsTunnel) {
                return $httpsTunnel.public_url
            }
        }
    } catch {
        return $null
    }
    return $null
}

# Función para detener procesos
function Stop-BackgroundProcesses {
    param([string]$ProcessName)
    $processes = Get-Process -Name $ProcessName -ErrorAction SilentlyContinue
    if ($processes) {
        Write-Host "Deteniendo procesos de $ProcessName..." -ForegroundColor Yellow
        $processes | Stop-Process -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 2
    }
}

# 1. Construir PWA
if (-not $SkipBuild) {
    Write-Host "1. Construyendo PWA..." -ForegroundColor Yellow
    if (-not (Test-Path "node_modules")) {
        Write-Host "   Instalando dependencias..." -ForegroundColor Gray
        npm install
        if ($LASTEXITCODE -ne 0) {
            Write-Host "   Error instalando dependencias" -ForegroundColor Red
            exit 1
        }
    }
    
    Write-Host "   Compilando Next.js..." -ForegroundColor Gray
    npm run build
    if ($LASTEXITCODE -ne 0) {
        Write-Host "   Error construyendo PWA" -ForegroundColor Red
        exit 1
    }
    Write-Host "   PWA construida exitosamente" -ForegroundColor Green
    Write-Host ""
} else {
    Write-Host "1. Saltando construcción (--skip-build)" -ForegroundColor Yellow
    Write-Host ""
}

# 2. Verificar icono
Write-Host "2. Verificando icono..." -ForegroundColor Yellow
if (-not (Test-Path $IconPath)) {
    Write-Host "   Error: Icono no encontrado en $IconPath" -ForegroundColor Red
    exit 1
}
Write-Host "   Icono encontrado: $IconPath" -ForegroundColor Green
Write-Host ""

# 3. Configurar Java
Write-Host "3. Configurando Java..." -ForegroundColor Yellow
$bubblewrapJavaPath = "$env:USERPROFILE\.bubblewrap\jdk"
$java17Path = $null

if (Test-Path $bubblewrapJavaPath) {
    $java17Dir = Get-ChildItem -Path $bubblewrapJavaPath -Directory | Select-Object -First 1
    if ($java17Dir) {
        $java17Path = $java17Dir.FullName
        $env:JAVA_HOME = $java17Path
        Write-Host "   Java 17 encontrado: $java17Path" -ForegroundColor Green
    }
}

if (-not $java17Path) {
    # Buscar Java en ubicaciones comunes
    $javaPaths = @(
        "C:\Program Files\Eclipse Adoptium",
        "C:\Program Files\Java",
        "C:\Program Files\Microsoft",
        "C:\Program Files\Azul"
    )
    
    foreach ($p in $javaPaths) {
        if (Test-Path $p) {
            $javaExe = Get-ChildItem $p -Filter "java.exe" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
            if ($javaExe) {
                $javaVersion = & $javaExe -version 2>&1 | Out-String
                if ($javaVersion -match 'version "(\d+)') {
                    $majorVersion = [int]$matches[1]
                    if ($majorVersion -ge 17) {
                        $env:JAVA_HOME = $javaExe.DirectoryName -replace "\\bin$", ""
                        Write-Host "   Java $majorVersion encontrado: $($env:JAVA_HOME)" -ForegroundColor Green
                        $java17Path = $env:JAVA_HOME
                        break
                    }
                }
            }
        }
    }
}

if (-not $java17Path) {
    Write-Host "   Advertencia: Java 17+ no encontrado. Bubblewrap lo instalará automáticamente." -ForegroundColor Yellow
}
Write-Host ""

# 4. Iniciar servidor de desarrollo
Write-Host "4. Iniciando servidor de desarrollo..." -ForegroundColor Yellow
$nodeProcess = Get-Process -Name node -ErrorAction SilentlyContinue | Where-Object { 
    $_.Path -like "*node.exe*" -and 
    (Get-NetTCPConnection -LocalPort $NextJsPort -ErrorAction SilentlyContinue)
}

if (-not $nodeProcess) {
    Write-Host "   Iniciando servidor Next.js en puerto $NextJsPort..." -ForegroundColor Gray
    Start-Process -FilePath "npm" -ArgumentList "run", "dev" -WindowStyle Hidden
    Start-Sleep -Seconds 5
    Write-Host "   Servidor iniciado" -ForegroundColor Green
} else {
    Write-Host "   Servidor ya está corriendo en puerto $NextJsPort" -ForegroundColor Green
}
Write-Host ""

# 5. Iniciar servidor HTTP para el icono
Write-Host "5. Iniciando servidor HTTP para el icono..." -ForegroundColor Yellow
$pythonProcess = Get-Process -Name python -ErrorAction SilentlyContinue | Where-Object {
    (Get-NetTCPConnection -LocalPort $IconServerPort -ErrorAction SilentlyContinue)
}

if (-not $pythonProcess) {
    Write-Host "   Iniciando servidor Python en puerto $IconServerPort..." -ForegroundColor Gray
    $publicDir = Join-Path $ScriptDir "public"
    Start-Process -FilePath "python" -ArgumentList "-m", "http.server", $IconServerPort, "--directory", $publicDir -WindowStyle Hidden
    Start-Sleep -Seconds 3
    Write-Host "   Servidor de icono iniciado" -ForegroundColor Green
} else {
    Write-Host "   Servidor de icono ya está corriendo" -ForegroundColor Green
}
Write-Host ""

# 6. Configurar ngrok
if (-not $SkipNgrok) {
    Write-Host "6. Configurando ngrok..." -ForegroundColor Yellow
    
    if (-not $PwaUrl) {
        # Verificar si ngrok ya está corriendo
        $existingNgrokUrl = Get-NgrokUrl
        if ($existingNgrokUrl) {
            Write-Host "   ngrok ya está corriendo: $existingNgrokUrl" -ForegroundColor Green
            $PwaUrl = $existingNgrokUrl
        } else {
            # Verificar si ngrok está instalado
            if (-not (Get-Command ngrok -ErrorAction SilentlyContinue)) {
                Write-Host "   Error: ngrok no está instalado" -ForegroundColor Red
                Write-Host "   Instala ngrok desde: https://ngrok.com/download" -ForegroundColor Yellow
                exit 1
            }
            
            # Detener procesos de ngrok anteriores
            Stop-BackgroundProcesses "ngrok"
            
            # Iniciar ngrok
            Write-Host "   Iniciando ngrok en puerto $NextJsPort..." -ForegroundColor Gray
            Start-Process -FilePath "ngrok" -ArgumentList "http", $NextJsPort -WindowStyle Hidden
            Start-Sleep -Seconds 5
            
            # Obtener URL de ngrok
            $maxRetries = 10
            $retryCount = 0
            while ($retryCount -lt $maxRetries -and -not $PwaUrl) {
                $PwaUrl = Get-NgrokUrl
                if (-not $PwaUrl) {
                    Start-Sleep -Seconds 2
                    $retryCount++
                    Write-Host "." -NoNewline -ForegroundColor Gray
                }
            }
            Write-Host ""
            
            if (-not $PwaUrl) {
                Write-Host "   Error: No se pudo obtener URL de ngrok" -ForegroundColor Red
                Write-Host "   Verifica que ngrok esté corriendo en: http://localhost:4040" -ForegroundColor Yellow
                exit 1
            }
            
            Write-Host "   ngrok iniciado: $PwaUrl" -ForegroundColor Green
        }
    } else {
        Write-Host "   Usando URL proporcionada: $PwaUrl" -ForegroundColor Green
    }
} else {
    if (-not $PwaUrl) {
        Write-Host "6. Error: Se requiere --PwaUrl cuando se usa --SkipNgrok" -ForegroundColor Red
        exit 1
    }
    Write-Host "6. Saltando ngrok, usando URL: $PwaUrl" -ForegroundColor Yellow
}
Write-Host ""

# 7. Verificar que el manifest sea accesible
Write-Host "7. Verificando manifest..." -ForegroundColor Yellow
$manifestUrl = "$PwaUrl/manifest.json"
try {
    $response = Invoke-WebRequest -Uri $manifestUrl -Method Head -TimeoutSec 10 -UseBasicParsing
    if ($response.StatusCode -ne 200) {
        throw "Status code: $($response.StatusCode)"
    }
    Write-Host "   Manifest accesible: $manifestUrl" -ForegroundColor Green
} catch {
    Write-Host "   Error: No se puede acceder a $manifestUrl" -ForegroundColor Red
    Write-Host "   Verifica que el servidor esté corriendo y ngrok esté configurado correctamente" -ForegroundColor Yellow
    exit 1
}
Write-Host ""

# 8. Verificar/Instalar Bubblewrap
Write-Host "8. Verificando Bubblewrap..." -ForegroundColor Yellow

# Intentar instalar localmente primero
if (-not (Test-Path "node_modules\@bubblewrap\cli")) {
    Write-Host "   Instalando Bubblewrap localmente..." -ForegroundColor Gray
    npm install @bubblewrap/cli --save-dev
    if ($LASTEXITCODE -ne 0) {
        Write-Host "   Error instalando Bubblewrap localmente" -ForegroundColor Red
        exit 1
    }
    Write-Host "   Bubblewrap instalado localmente" -ForegroundColor Green
}

# Verificar que funciona
$bubblewrapAvailable = $false
try {
    # Intentar con npx primero
    $bw = npx @bubblewrap/cli --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   Bubblewrap disponible vía npx" -ForegroundColor Green
        $bubblewrapAvailable = $true
    }
} catch {
    Write-Host "   npx no funciona, intentando instalación global..." -ForegroundColor Yellow
}

# Si npx no funciona, intentar instalar globalmente
if (-not $bubblewrapAvailable) {
    try {
        Write-Host "   Instalando Bubblewrap globalmente..." -ForegroundColor Gray
        npm install -g @bubblewrap/cli
        if ($LASTEXITCODE -eq 0) {
            Write-Host "   Bubblewrap instalado globalmente" -ForegroundColor Green
            $bubblewrapAvailable = $true
        }
    } catch {
        Write-Host "   Error instalando Bubblewrap globalmente" -ForegroundColor Red
    }
}

if (-not $bubblewrapAvailable) {
    Write-Host "   Error: No se pudo instalar o verificar Bubblewrap" -ForegroundColor Red
    exit 1
}
Write-Host ""

# 9. Limpiar directorio Android anterior
Write-Host "9. Limpiando directorio Android anterior..." -ForegroundColor Yellow
if (Test-Path $AndroidDir) {
    Remove-Item -Recurse -Force $AndroidDir
    Write-Host "   Directorio limpiado" -ForegroundColor Green
}
New-Item -ItemType Directory -Path $AndroidDir -Force | Out-Null
Write-Host ""

# 10. Inicializar proyecto Android
Write-Host "10. Inicializando proyecto Android..." -ForegroundColor Yellow
Write-Host "    URL del manifest: $manifestUrl" -ForegroundColor Gray
Write-Host "    Esto puede tardar varios minutos..." -ForegroundColor Gray

# Determinar cómo ejecutar Bubblewrap
$bubblewrapCmd = $null
$bubblewrapArgs = @("init", "--manifest", $manifestUrl, "--directory", $AndroidDir)

# Intentar usar la ruta local primero
if (Test-Path "node_modules\.bin\bubblewrap.cmd") {
    $bubblewrapCmd = ".\node_modules\.bin\bubblewrap.cmd"
    Write-Host "    Usando Bubblewrap local (cmd)" -ForegroundColor Gray
} elseif (Test-Path "node_modules\.bin\bubblewrap") {
    $bubblewrapCmd = ".\node_modules\.bin\bubblewrap"
    Write-Host "    Usando Bubblewrap local" -ForegroundColor Gray
} elseif (Get-Command bubblewrap -ErrorAction SilentlyContinue) {
    $bubblewrapCmd = "bubblewrap"
    Write-Host "    Usando Bubblewrap global" -ForegroundColor Gray
} else {
    # Usar npx como último recurso
    $bubblewrapCmd = "npx"
    $bubblewrapArgs = @("@bubblewrap/cli", "init", "--manifest", $manifestUrl, "--directory", $AndroidDir)
    Write-Host "    Usando Bubblewrap vía npx" -ForegroundColor Gray
}

$initCmd = "$bubblewrapCmd $($bubblewrapArgs -join ' ')"
Write-Host "    Comando: $initCmd" -ForegroundColor DarkGray

try {
    # Ejecutar init
    if ($bubblewrapCmd -eq "npx") {
        $initOutput = & npx @bubblewrap/cli init --manifest $manifestUrl --directory $AndroidDir 2>&1
    } else {
        $initOutput = & $bubblewrapCmd $bubblewrapArgs 2>&1
    }
    
    if ($LASTEXITCODE -ne 0) {
        throw "Falló init con código: $LASTEXITCODE"
    }
    
    Write-Host "    Proyecto Android inicializado exitosamente" -ForegroundColor Green
} catch {
    Write-Host "    Error en init: $_" -ForegroundColor Red
    Write-Host "    Output: $initOutput" -ForegroundColor Gray
    exit 1
}
Write-Host ""

# 11. Actualizar icono con archivo local
Write-Host "11. Actualizando icono con archivo local..." -ForegroundColor Yellow
$iconLocalUrl = "http://localhost:$IconServerPort/assets/LogoGateKeep.webp"

# Actualizar twa-manifest.json
$twaManifestPath = Join-Path $AndroidDir "twa-manifest.json"
if (Test-Path $twaManifestPath) {
    $twaManifest = Get-Content $twaManifestPath -Raw | ConvertFrom-Json
    $twaManifest.iconUrl = $iconLocalUrl
    $twaManifest.maskableIconUrl = $iconLocalUrl
    $twaManifest | ConvertTo-Json -Depth 10 | Set-Content $twaManifestPath
    Write-Host "    twa-manifest.json actualizado con icono local" -ForegroundColor Green
}

# Actualizar con bubblewrap
try {
    # Determinar cómo ejecutar Bubblewrap (reutilizar la lógica de la sección 10)
    $bubblewrapUpdateCmd = $null
    if (Test-Path "node_modules\.bin\bubblewrap.cmd") {
        $bubblewrapUpdateCmd = ".\node_modules\.bin\bubblewrap.cmd"
    } elseif (Test-Path "node_modules\.bin\bubblewrap") {
        $bubblewrapUpdateCmd = ".\node_modules\.bin\bubblewrap"
    } elseif (Get-Command bubblewrap -ErrorAction SilentlyContinue) {
        $bubblewrapUpdateCmd = "bubblewrap"
    } else {
        $bubblewrapUpdateCmd = "npx"
    }
    
    if ($bubblewrapUpdateCmd -eq "npx") {
        $updateOutput = & npx @bubblewrap/cli update --icon $IconPath --directory $AndroidDir 2>&1
    } else {
        $updateOutput = & $bubblewrapUpdateCmd update --icon $IconPath --directory $AndroidDir 2>&1
    }
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "    Advertencia: update del icono falló, pero continuamos..." -ForegroundColor Yellow
    } else {
        Write-Host "    Icono actualizado exitosamente" -ForegroundColor Green
    }
} catch {
    Write-Host "    Advertencia: Error al actualizar icono: $_" -ForegroundColor Yellow
    Write-Host "    Continuando con la construcción..." -ForegroundColor Gray
}
Write-Host ""

# 12. Configurar variables de entorno para Gradle
Write-Host "12. Configurando variables de entorno..." -ForegroundColor Yellow

# Configurar ANDROID_HOME
$bubblewrapAndroidSdk = "$env:USERPROFILE\.bubblewrap\android_sdk"
if (Test-Path $bubblewrapAndroidSdk) {
    $env:ANDROID_HOME = $bubblewrapAndroidSdk
    Write-Host "    ANDROID_HOME: $env:ANDROID_HOME" -ForegroundColor Green
} else {
    Write-Host "    Advertencia: Android SDK de Bubblewrap no encontrado" -ForegroundColor Yellow
    Write-Host "    Bubblewrap lo instalará automáticamente" -ForegroundColor Gray
}

# Eliminar ANDROID_SDK_ROOT para evitar conflictos
$env:ANDROID_SDK_ROOT = $null

# Configurar Java en gradle.properties
$gradlePropertiesPath = Join-Path $AndroidDir "gradle.properties"
if (Test-Path $gradlePropertiesPath -and $java17Path) {
    $gradleProps = Get-Content $gradlePropertiesPath -Raw
    $javaHomePath = $java17Path -replace "\\", "/"
    if ($gradleProps -notmatch "org\.gradle\.java\.home") {
        Add-Content -Path $gradlePropertiesPath -Value "`norg.gradle.java.home=$javaHomePath"
        Write-Host "    Java 17 configurado en gradle.properties" -ForegroundColor Green
    }
}

# Actualizar build.gradle para usar mavenCentral en lugar de jcenter
$buildGradlePath = Join-Path $AndroidDir "build.gradle"
if (Test-Path $buildGradlePath) {
    $buildGradleContent = Get-Content $buildGradlePath -Raw
    if ($buildGradleContent -match "jcenter\(\)") {
        $buildGradleContent = $buildGradleContent -replace "jcenter\(\)", "mavenCentral()"
        Set-Content -Path $buildGradlePath -Value $buildGradleContent
        Write-Host "    build.gradle actualizado (jcenter -> mavenCentral)" -ForegroundColor Green
    }
}
Write-Host ""

# 13. Construir APK
Write-Host "13. Construyendo APK..." -ForegroundColor Yellow
Write-Host "    Esto puede tardar varios minutos..." -ForegroundColor Gray

Set-Location $AndroidDir

try {
    # Detener daemons de Gradle anteriores
    & .\gradlew.bat --stop 2>&1 | Out-Null
    
    # Construir APK
    Write-Host "    Ejecutando Gradle..." -ForegroundColor Gray
    $buildOutput = & .\gradlew.bat assembleRelease --no-daemon 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        throw "Gradle falló con código: $LASTEXITCODE"
    }
    
    # Buscar APK generado
    $apk = Get-ChildItem -Path "app\build\outputs\apk\release" -Filter "*.apk" -ErrorAction SilentlyContinue | Select-Object -First 1
    
    if ($apk) {
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "  APK construido exitosamente" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Green
        Write-Host ""
        Write-Host "Ubicación: $($apk.FullName)" -ForegroundColor Cyan
        Write-Host "Tamaño: $([math]::Round($apk.Length / 1MB, 2)) MB" -ForegroundColor White
        Write-Host ""
        Write-Host "Para instalar en un dispositivo Android:" -ForegroundColor Yellow
        Write-Host "1. Transfiere el APK a tu dispositivo" -ForegroundColor White
        Write-Host "2. Habilita 'Orígenes desconocidos' en Configuración" -ForegroundColor White
        Write-Host "3. Abre el APK e instálalo" -ForegroundColor White
        Write-Host ""
        
        # Abrir directorio del APK
        explorer.exe $apk.DirectoryName
    } else {
        Write-Host "    Advertencia: APK no encontrado en la ubicación esperada" -ForegroundColor Yellow
        Write-Host "    Buscando en todas las ubicaciones..." -ForegroundColor Gray
        $allApks = Get-ChildItem -Path . -Recurse -Filter "*.apk" -ErrorAction SilentlyContinue
        if ($allApks) {
            $allApks | ForEach-Object {
                Write-Host "    Encontrado: $($_.FullName)" -ForegroundColor Cyan
            }
        }
    }
    
} catch {
    Write-Host "    Error al construir APK: $_" -ForegroundColor Red
    Write-Host "    Output: $buildOutput" -ForegroundColor Gray
    exit 1
} finally {
    Set-Location $ScriptDir
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Proceso completado" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Nota: Los siguientes procesos están corriendo en background:" -ForegroundColor Yellow
Write-Host "  - Servidor Next.js (puerto $NextJsPort)" -ForegroundColor White
Write-Host "  - Servidor HTTP para icono (puerto $IconServerPort)" -ForegroundColor White
if (-not $SkipNgrok) {
    Write-Host "  - ngrok (puerto 4040)" -ForegroundColor White
}
Write-Host ""
Write-Host "Para detenerlos, cierra las ventanas o reinicia PowerShell" -ForegroundColor Gray
Write-Host ""

