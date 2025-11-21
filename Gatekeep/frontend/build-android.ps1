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
            # Preferir HTTPS, pero aceptar HTTP si no hay HTTPS disponible
            $tunnel = $response.tunnels | Where-Object { $_.proto -eq 'https' } | Select-Object -First 1
            if (-not $tunnel) {
                $tunnel = $response.tunnels | Where-Object { $_.proto -eq 'http' } | Select-Object -First 1
            }
            if ($tunnel -and $tunnel.public_url) {
                return $tunnel.public_url
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

# Función para validar requisitos previos
function Test-Prerequisites {
    Write-Host "Validando requisitos previos..." -ForegroundColor Yellow
    
    # Validar Node.js
    if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
        Write-Host "   Error: Node.js no está instalado" -ForegroundColor Red
        Write-Host "   Instala Node.js desde: https://nodejs.org/" -ForegroundColor Yellow
        return $false
    }
    $nodeVersion = node --version
    Write-Host "   Node.js: $nodeVersion" -ForegroundColor Green
    
    # Validar Python (para servidor HTTP)
    if (-not (Get-Command python -ErrorAction SilentlyContinue)) {
        Write-Host "   Error: Python no está instalado" -ForegroundColor Red
        Write-Host "   Instala Python desde: https://www.python.org/downloads/" -ForegroundColor Yellow
        return $false
    }
    $pythonVersion = python --version 2>&1
    Write-Host "   Python: $pythonVersion" -ForegroundColor Green
    
    # Validar ngrok (si no se está saltando)
    if (-not $SkipNgrok) {
        if (-not (Get-Command ngrok -ErrorAction SilentlyContinue)) {
            Write-Host "   Error: ngrok no está instalado" -ForegroundColor Red
            Write-Host "   Instala ngrok desde: https://ngrok.com/download" -ForegroundColor Yellow
            return $false
        }
        Write-Host "   ngrok: Instalado" -ForegroundColor Green
    }
    
    # Validar puertos disponibles
    $port3000 = Get-NetTCPConnection -LocalPort $NextJsPort -ErrorAction SilentlyContinue
    $port8000 = Get-NetTCPConnection -LocalPort $IconServerPort -ErrorAction SilentlyContinue
    
    if ($port3000 -and $port3000.State -eq "Listen") {
        Write-Host "   Puerto ${NextJsPort}: En uso (OK si Next.js ya está corriendo)" -ForegroundColor Yellow
    } else {
        Write-Host "   Puerto ${NextJsPort}: Disponible" -ForegroundColor Green
    }
    
    if ($port8000 -and $port8000.State -eq "Listen") {
        Write-Host "   Puerto ${IconServerPort}: En uso (OK si servidor HTTP ya está corriendo)" -ForegroundColor Yellow
    } else {
        Write-Host "   Puerto ${IconServerPort}: Disponible" -ForegroundColor Green
    }
    
    Write-Host "   Requisitos previos validados" -ForegroundColor Green
    return $true
}

# Función para verificar que un servicio esté respondiendo
function Test-ServiceResponse {
    param(
        [string]$Url,
        [int]$MaxRetries = 5,
        [int]$RetryDelay = 2
    )
    
    $retryCount = 0
    while ($retryCount -lt $MaxRetries) {
        try {
            $response = Invoke-WebRequest -Uri $Url -Method Head -TimeoutSec 5 -UseBasicParsing -ErrorAction Stop
            if ($response.StatusCode -eq 200) {
                return $true
            }
        } catch {
            $retryCount++
            if ($retryCount -lt $MaxRetries) {
                Start-Sleep -Seconds $RetryDelay
            }
        }
    }
    return $false
}

# Validar requisitos previos
# Verificar que la función esté definida
if (-not (Get-Command Test-Prerequisites -ErrorAction SilentlyContinue)) {
    Write-Host "Error: La función Test-Prerequisites no está definida" -ForegroundColor Red
    exit 1
}

if (-not (Test-Prerequisites)) {
    exit 1
}
Write-Host ""

# 1. Construir PWA (siempre ejecutar build a menos que se use --SkipBuild)
Write-Host "1. Construyendo PWA..." -ForegroundColor Yellow
if (-not $SkipBuild) {
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
} else {
    Write-Host "   Saltando construcción (--skip-build)" -ForegroundColor Yellow
}

# Verificar que .next existe
if (-not (Test-Path ".next")) {
    Write-Host "   Error: Directorio .next no encontrado. Ejecutando build..." -ForegroundColor Yellow
    npm run build
    if ($LASTEXITCODE -ne 0) {
        Write-Host "   Error construyendo PWA" -ForegroundColor Red
        exit 1
    }
}

Write-Host "   ✓ PWA lista para servir" -ForegroundColor Green
Write-Host ""

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
                try {
                    $javaVersion = & $javaExe -version 2>&1 | Out-String
                    # Detectar versión - puede ser "version "17.0.1"" o "version "1.8.0_461""
                    if ($javaVersion -match 'version "1\.(\d+)') {
                        # Formato antiguo: 1.8, 1.11, etc.
                        $majorVersion = [int]$matches[1]
                    } elseif ($javaVersion -match 'version "(\d+)') {
                        # Formato nuevo: 17, 21, etc.
                        $majorVersion = [int]$matches[1]
                    } else {
                        continue
                    }
                    
                    if ($majorVersion -ge 17) {
                        $env:JAVA_HOME = $javaExe.DirectoryName -replace "\\bin$", ""
                        Write-Host "   Java $majorVersion encontrado: $($env:JAVA_HOME)" -ForegroundColor Green
                        $java17Path = $env:JAVA_HOME
                        break
                    }
                } catch {
                    # Ignorar errores en la detección de versión
                    continue
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
Write-Host "4. Iniciando servidor de Next.js..." -ForegroundColor Yellow

# Detener procesos node anteriores
Stop-BackgroundProcesses "node"
Start-Sleep -Seconds 2

# Verificar que el puerto 3000 esté libre
$portCheck = Get-NetTCPConnection -LocalPort $NextJsPort -ErrorAction SilentlyContinue | Where-Object { $_.State -eq "Listen" }
if ($portCheck) {
    Write-Host "   Advertencia: Puerto $NextJsPort está en uso, liberando..." -ForegroundColor Yellow
    Stop-BackgroundProcesses "node"
    Start-Sleep -Seconds 3
}

Write-Host "   Iniciando: npm run start (puerto $NextJsPort)..." -ForegroundColor Gray
$npmProcess = Start-Process -FilePath "npm" -ArgumentList "run", "start" -PassThru -WindowStyle Hidden -WorkingDirectory $ScriptDir
Write-Host "   Proceso npm iniciado (PID: $($npmProcess.Id))" -ForegroundColor Gray
Start-Sleep -Seconds 6

# Verificar que el proceso sigue ejecutándose
$npmCheck = Get-Process -Id $npmProcess.Id -ErrorAction SilentlyContinue
if (-not $npmCheck) {
    Write-Host "   Error: El proceso npm se detuvo inmediatamente" -ForegroundColor Red
    Write-Host "   Intenta manualmente: npm run start" -ForegroundColor Yellow
    exit 1
}

# Verificar que localhost:3000 responde
Write-Host "   Esperando que el servidor esté listo..." -ForegroundColor Gray
$serverReady = $false
$maxWait = 20
$waitCount = 0

while ($waitCount -lt $maxWait) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:$NextJsPort" -TimeoutSec 3 -UseBasicParsing -ErrorAction Stop
        if ($response.StatusCode -eq 200) {
            Write-Host "   ✓ Servidor respondiendo en localhost:$NextJsPort" -ForegroundColor Green
            $serverReady = $true
            break
        }
    } catch {
        # Ignorar errores, intentar de nuevo
    }
    
    $waitCount++
    Write-Host "." -NoNewline -ForegroundColor Gray
    Start-Sleep -Seconds 1
}

Write-Host ""

if (-not $serverReady) {
    Write-Host "   Error: Servidor no responde después de $maxWait segundos" -ForegroundColor Red
    Write-Host "   El proceso npm puede tener errores. Intenta manualmente:" -ForegroundColor Yellow
    Write-Host "   npm run start" -ForegroundColor Cyan
    exit 1
}

Write-Host "   ✓ Servidor Next.js listo" -ForegroundColor Green
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
    Start-Sleep -Seconds 5
    
    # Verificar que el servidor esté respondiendo
    $iconServerUrl = "http://localhost:$IconServerPort"
    Write-Host "   Verificando que el servidor de icono esté respondiendo..." -ForegroundColor Gray
    if (Test-ServiceResponse -Url $iconServerUrl -MaxRetries 5) {
        Write-Host "   Servidor de icono iniciado y respondiendo" -ForegroundColor Green
    } else {
        Write-Host "   Advertencia: Servidor de icono iniciado pero no responde aún" -ForegroundColor Yellow
        Write-Host "   Continuando..." -ForegroundColor Gray
    }
} else {
    Write-Host "   Servidor de icono ya está corriendo" -ForegroundColor Green
    # Verificar que esté respondiendo
    $iconServerUrl = "http://localhost:$IconServerPort"
    if (-not (Test-ServiceResponse -Url $iconServerUrl -MaxRetries 3)) {
        Write-Host "   Advertencia: Servidor de icono no responde" -ForegroundColor Yellow
    }
}
Write-Host ""

# 6. Configurar ngrok
if (-not $SkipNgrok) {
    Write-Host "6. Configurando ngrok para localhost:$NextJsPort..." -ForegroundColor Yellow
    
    if (-not $PwaUrl) {
        # Detener procesos de ngrok anteriores
        Stop-BackgroundProcesses "ngrok"
        
        # Verificar que ngrok esté instalado
        if (-not (Get-Command ngrok -ErrorAction SilentlyContinue)) {
            Write-Host "   Error: ngrok no está instalado" -ForegroundColor Red
            Write-Host "   Instala ngrok desde: https://ngrok.com/download" -ForegroundColor Yellow
            exit 1
        }
        
        # Iniciar ngrok simple
        Write-Host "   Iniciando: ngrok http $NextJsPort" -ForegroundColor Gray
        $ngrokProcess = Start-Process -FilePath "ngrok" -ArgumentList "http", $NextJsPort -PassThru -WindowStyle Minimized
        
        if (-not $ngrokProcess) {
            Write-Host "   Error: No se pudo iniciar ngrok" -ForegroundColor Red
            exit 1
        }
        
        Write-Host "   ngrok iniciado (PID: $($ngrokProcess.Id))" -ForegroundColor Green
        Write-Host "   Esperando que se establezca la conexión..." -ForegroundColor Gray
        Start-Sleep -Seconds 6
        
        # Obtener URL de ngrok
        $maxRetries = 15
        $retryCount = 0
        while ($retryCount -lt $maxRetries -and -not $PwaUrl) {
            try {
                $ngrokResponse = Invoke-RestMethod -Uri "http://localhost:4040/api/tunnels" -TimeoutSec 3 -ErrorAction Stop
                if ($ngrokResponse.tunnels -and $ngrokResponse.tunnels.Count -gt 0) {
                    $tunnel = $ngrokResponse.tunnels | Where-Object { $_.proto -eq 'https' -or $_.proto -eq 'http' } | Select-Object -First 1
                    if ($tunnel) {
                        $PwaUrl = $tunnel.public_url
                        Write-Host "   ✓ URL ngrok: $PwaUrl" -ForegroundColor Green
                        break
                    }
                }
            } catch {
                # Ignorar errores de conexión
            }
            
            $retryCount++
            if ($retryCount -lt $maxRetries) {
                Start-Sleep -Seconds 1
            }
        }
        
        if (-not $PwaUrl) {
            Write-Host "   Error: No se pudo obtener URL de ngrok" -ForegroundColor Red
            Write-Host "   Verifica: http://localhost:4040" -ForegroundColor Yellow
            exit 1
        }
    } else {
        Write-Host "   Usando URL proporcionada: $PwaUrl" -ForegroundColor Green
    }
} else {
    if (-not $PwaUrl) {
        Write-Host "6. Error: Se requiere --PwaUrl cuando se usa --SkipNgrok" -ForegroundColor Red
        exit 1
    }
    Write-Host "6. Usando URL: $PwaUrl" -ForegroundColor Yellow
}
Write-Host ""

# 7. Verificar que el manifest sea accesible
Write-Host "7. Verificando manifest..." -ForegroundColor Yellow
$manifestUrl = "$PwaUrl/manifest.json"

# Primero verificar que localhost:3000 está disponible
Write-Host "   Verificando acceso a localhost:$NextJsPort..." -ForegroundColor Gray
try {
    $localResponse = Invoke-WebRequest -Uri "http://localhost:$NextJsPort" -Method Head -TimeoutSec 5 -UseBasicParsing -ErrorAction Stop
    Write-Host "   ✓ localhost:$NextJsPort respondiendo" -ForegroundColor Green
} catch {
    Write-Host "   ✗ localhost:$NextJsPort NO está respondiendo" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "   Diagnóstico:" -ForegroundColor Yellow
    Write-Host "   1. Verifica que npm run dev se ejecutó sin errores" -ForegroundColor White
    Write-Host "   2. Comprueba que no hay otro proceso usando el puerto 3000" -ForegroundColor White
    Write-Host "   3. Abre en el navegador: http://localhost:3000" -ForegroundColor White
    Write-Host ""
    exit 1
}

# Ahora verificar el manifest a través de ngrok
Write-Host "   Verificando acceso a manifest mediante ngrok: $manifestUrl" -ForegroundColor Gray
$manifestRetries = 0
$manifestMaxRetries = 5
while ($manifestRetries -lt $manifestMaxRetries) {
    try {
        $response = Invoke-WebRequest -Uri $manifestUrl -Method Head -TimeoutSec 10 -UseBasicParsing -ErrorAction Stop
        if ($response.StatusCode -eq 200) {
            Write-Host "   ✓ Manifest accesible: $manifestUrl" -ForegroundColor Green
            break
        } else {
            throw "Status code: $($response.StatusCode)"
        }
    } catch {
        $manifestRetries++
        if ($manifestRetries -lt $manifestMaxRetries) {
            Write-Host "   ○ Intento $manifestRetries/$manifestMaxRetries... Error: $($_.Exception.Message)" -ForegroundColor Yellow
            Start-Sleep -Seconds 2
        } else {
            Write-Host "   ✗ Error: No se puede acceder a $manifestUrl" -ForegroundColor Red
            Write-Host "   Detalles: $($_.Exception.Message)" -ForegroundColor Yellow
            Write-Host ""
            Write-Host "   Diagnóstico:" -ForegroundColor Yellow
            Write-Host "   1. localhost:3000 está respondiendo ✓" -ForegroundColor Green
            Write-Host "   2. Pero ngrok no puede acceder al manifest" -ForegroundColor White
            Write-Host "   3. Posibles causas:" -ForegroundColor White
            Write-Host "      - ngrok está accediendo a un puerto diferente de 3000" -ForegroundColor White
            Write-Host "      - La PWA no está completamente construida" -ForegroundColor White
            Write-Host "      - El archivo manifest.json no existe en public/" -ForegroundColor White
            Write-Host ""
            Write-Host "   Prueba estos comandos:" -ForegroundColor Yellow
            Write-Host "   # Ver si el manifest existe localmente:" -ForegroundColor White
            Write-Host "   curl http://localhost:$NextJsPort/manifest.json" -ForegroundColor Cyan
            Write-Host "   # Ver si ngrok está exponiendo correctamente:" -ForegroundColor White
            Write-Host "   curl $manifestUrl -v" -ForegroundColor Cyan
            exit 1
        }
    }
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

# 10. Leer datos del manifest para pre-llenar respuestas
Write-Host "10. Preparando datos del manifest..." -ForegroundColor Yellow
$manifestData = $null
try {
    $manifestResponse = Invoke-RestMethod -Uri $manifestUrl -TimeoutSec 10
    $manifestData = $manifestResponse
    Write-Host "    Manifest cargado exitosamente" -ForegroundColor Green
} catch {
    Write-Host "    Advertencia: No se pudo cargar el manifest, usando valores por defecto" -ForegroundColor Yellow
    $manifestData = @{
        name = "GateKeep"
        short_name = "GateKeep"
        display = "standalone"
        orientation = "portrait-primary"
        theme_color = "#0066cc"
        background_color = "#ffffff"
        shortcuts = @()
    }
}

# Extraer valores del manifest
$appName = if ($manifestData.name) { $manifestData.name } else { "GateKeep" }
$shortName = if ($manifestData.short_name) { $manifestData.short_name } else { "GateKeep" }
$displayMode = if ($manifestData.display) { $manifestData.display } else { "standalone" }
$orientation = if ($manifestData.orientation) { $manifestData.orientation } else { "portrait-primary" }
$themeColor = if ($manifestData.theme_color) { $manifestData.theme_color } else { "#0066CC" }
$backgroundColor = if ($manifestData.background_color) { $manifestData.background_color } else { "#FFFFFF" }
$hasShortcuts = ($manifestData.shortcuts -and $manifestData.shortcuts.Count -gt 0)

# Extraer dominio de la URL de ngrok
$uri = [System.Uri]$PwaUrl
$domain = $uri.Host
$startUrl = if ($manifestData.start_url) { $manifestData.start_url } else { "/" }

Write-Host "    App Name: $appName" -ForegroundColor Gray
Write-Host "    Domain: $domain" -ForegroundColor Gray
Write-Host "    Start URL: $startUrl" -ForegroundColor Gray
Write-Host ""

# 11. Inicializar proyecto Android con respuestas automáticas
Write-Host "11. Inicializando proyecto Android..." -ForegroundColor Yellow
Write-Host "    URL del manifest: $manifestUrl" -ForegroundColor Gray
Write-Host "    Esto puede tardar varios minutos..." -ForegroundColor Gray
Write-Host "    Respondiendo preguntas automáticamente..." -ForegroundColor Gray

# Determinar cómo ejecutar Bubblewrap con rutas absolutas
$bubblewrapCmd = $null
$bubblewrapFullPath = $null
$useCmdExe = $false

$bubblewrapCmdPath = Join-Path $ScriptDir "node_modules\.bin\bubblewrap.cmd"
$bubblewrapPath = Join-Path $ScriptDir "node_modules\.bin\bubblewrap"

if (Test-Path $bubblewrapCmdPath) {
    $bubblewrapFullPath = Resolve-Path $bubblewrapCmdPath
    $bubblewrapCmd = $bubblewrapFullPath.Path
    $useCmdExe = $true
    Write-Host "    Usando Bubblewrap local (cmd): $bubblewrapCmd" -ForegroundColor Gray
} elseif (Test-Path $bubblewrapPath) {
    $bubblewrapFullPath = Resolve-Path $bubblewrapPath
    $bubblewrapCmd = $bubblewrapFullPath.Path
    Write-Host "    Usando Bubblewrap local: $bubblewrapCmd" -ForegroundColor Gray
} elseif (Get-Command bubblewrap -ErrorAction SilentlyContinue) {
    $bubblewrapCmd = (Get-Command bubblewrap).Source
    Write-Host "    Usando Bubblewrap global: $bubblewrapCmd" -ForegroundColor Gray
} else {
    $bubblewrapCmd = "npx"
    Write-Host "    Usando Bubblewrap vía npx" -ForegroundColor Gray
}

# Validar que el ejecutable exista (excepto para npx)
if ($bubblewrapCmd -ne "npx" -and -not (Test-Path $bubblewrapCmd)) {
    Write-Host "    Error: Bubblewrap no encontrado en: $bubblewrapCmd" -ForegroundColor Red
    exit 1
}

# Crear archivo de respuestas para Bubblewrap
$responsesFile = Join-Path $ScriptDir "bubblewrap-responses.txt"
$iconLocalUrl = "http://localhost:$IconServerPort/assets/LogoGateKeep.webp"
$keystorePath = Join-Path $AndroidDir "android.keystore"

# Respuestas en el orden que Bubblewrap las pregunta
$includeShortcuts = if ($hasShortcuts) { "Yes" } else { "No" }
$responses = @(
    $domain,                    # Domain
    $startUrl,                  # URL path
    $appName,                   # Application name
    $shortName,                 # Short name
    $PackageName,               # Application ID
    "1",                        # Starting version code
    $displayMode,               # Display mode
    $orientation,               # Orientation
    $themeColor,                # Status bar color
    $backgroundColor,           # Splash screen color
    $iconLocalUrl,              # Icon URL
    $iconLocalUrl,              # Maskable icon URL
    $includeShortcuts,          # Include app shortcuts?
    "",                         # Monochrome icon URL (vacío)
    "No",                       # Include support for Play Billing?
    "No",                       # Request geolocation permission?
    $keystorePath,              # Key store location
    "android"                   # Key name
)

# Guardar respuestas en archivo temporal
$responses | Out-File -FilePath $responsesFile -Encoding ASCII -NoNewline

try {
    # Ejecutar init de forma interactiva para que el usuario responda las preguntas
    Write-Host "    Ejecutando Bubblewrap init de forma interactiva..." -ForegroundColor Gray
    Write-Host "    Por favor, responde las preguntas que aparezcan a continuación:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "    ========================================" -ForegroundColor Cyan
    Write-Host "    IMPORTANTE: Usa estos valores EXACTOS" -ForegroundColor Yellow
    Write-Host "    ========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "    Domain: $domain" -ForegroundColor White
    Write-Host "    URL path: $startUrl" -ForegroundColor White
    Write-Host "    Application name: $appName" -ForegroundColor White
    Write-Host "    Short name: $shortName" -ForegroundColor White
    Write-Host "    Application ID: $PackageName" -ForegroundColor White
    Write-Host "    Starting version code: 1" -ForegroundColor White
    Write-Host "    Display mode: $displayMode" -ForegroundColor White
    Write-Host "    Orientation: $orientation" -ForegroundColor White
    Write-Host "    Status bar color: $themeColor" -ForegroundColor White
    Write-Host "    Splash screen color: $backgroundColor" -ForegroundColor White
    Write-Host ""
    Write-Host "    ⚠️  CRÍTICO - Icon URL: $iconLocalUrl" -ForegroundColor Yellow -BackgroundColor DarkRed
    Write-Host "    ⚠️  CRÍTICO - Maskable icon URL: $iconLocalUrl" -ForegroundColor Yellow -BackgroundColor DarkRed
    Write-Host "    (NO uses la URL de ngrok, usa localhost:8000)" -ForegroundColor Red
    Write-Host ""
    Write-Host "    Include shortcuts: $(if ($hasShortcuts) { 'Yes' } else { 'No' })" -ForegroundColor White
    Write-Host "    Monochrome icon URL: (dejar vacío o presionar Enter)" -ForegroundColor White
    Write-Host "    Include support for Play Billing?: No" -ForegroundColor White
    Write-Host "    Request geolocation permission?: No" -ForegroundColor White
    Write-Host "    Key store location: $keystorePath" -ForegroundColor White
    Write-Host "    Key name: android" -ForegroundColor White
    Write-Host ""
    
    # Construir comando completo con rutas absolutas
    if ($bubblewrapCmd -eq "npx") {
        $initCommand = "npx @bubblewrap/cli init --manifest `"$manifestUrl`" --directory `"$AndroidDir`""
    } elseif ($useCmdExe) {
        # Para archivos .cmd, usar cmd.exe con ruta absoluta (sin comillas adicionales)
        $initCommand = "cmd.exe /c $bubblewrapCmd init --manifest `"$manifestUrl`" --directory `"$AndroidDir`""
    } else {
        # Usar ruta absoluta directamente
        $initCommand = "& `"$bubblewrapCmd`" init --manifest `"$manifestUrl`" --directory `"$AndroidDir`""
    }
    
    # Ejecutar de forma interactiva (sin redirección de entrada/salida)
    Write-Host "    Ejecutando: $initCommand" -ForegroundColor DarkGray
    Write-Host ""
    
    if ($bubblewrapCmd -eq "npx") {
        & npx @bubblewrap/cli init --manifest $manifestUrl --directory $AndroidDir
    } elseif ($useCmdExe) {
        & cmd.exe /c "$bubblewrapCmd init --manifest `"$manifestUrl`" --directory `"$AndroidDir`""
    } else {
        & $bubblewrapCmd init --manifest $manifestUrl --directory $AndroidDir
    }
    
    if ($LASTEXITCODE -ne 0) {
        throw "Falló init con código: $LASTEXITCODE"
    }
    
    # Validar que el proyecto se haya creado correctamente
    $twaManifestPath = Join-Path $AndroidDir "twa-manifest.json"
    $buildGradlePath = Join-Path $AndroidDir "build.gradle"
    
    if (-not (Test-Path $twaManifestPath)) {
        throw "twa-manifest.json no se generó correctamente"
    }
    
    if (-not (Test-Path $buildGradlePath)) {
        throw "build.gradle no se generó correctamente"
    }
    
    # Actualizar automáticamente el icono con la URL local (corrige el problema de Content-Type) (corrige el problema de Content-Type)
    Write-Host "    Actualizando icono con URL local..." -ForegroundColor Gray
    $twaManifest = Get-Content $twaManifestPath -Raw | ConvertFrom-Json
    $twaManifest.iconUrl = $iconLocalUrl
    $twaManifest.maskableIconUrl = $iconLocalUrl
    $twaManifest | ConvertTo-Json -Depth 10 | Set-Content $twaManifestPath
    Write-Host "    Icono actualizado a URL local: $iconLocalUrl" -ForegroundColor Green
    
    Write-Host "    Proyecto Android inicializado exitosamente" -ForegroundColor Green
    Write-Host "    twa-manifest.json: Verificado y actualizado" -ForegroundColor Gray
    Write-Host "    build.gradle: Verificado" -ForegroundColor Gray
} catch {
    Write-Host "    Error en init: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "    Si el proceso falló, puedes ejecutar manualmente:" -ForegroundColor Yellow
    Write-Host "    $bubblewrapCmd init --manifest `"$manifestUrl`" --directory `"$AndroidDir`"" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "    Valores sugeridos para las preguntas:" -ForegroundColor Yellow
    Write-Host "    Domain: $domain" -ForegroundColor White
    Write-Host "    URL path: $startUrl" -ForegroundColor White
    Write-Host "    Application name: $appName" -ForegroundColor White
    Write-Host "    Short name: $shortName" -ForegroundColor White
    Write-Host "    Application ID: $PackageName" -ForegroundColor White
    Write-Host "    Display mode: $displayMode" -ForegroundColor White
    Write-Host "    Orientation: $orientation" -ForegroundColor White
    Write-Host "    Status bar color: $themeColor" -ForegroundColor White
    Write-Host "    Splash screen color: $backgroundColor" -ForegroundColor White
    Write-Host "    Icon URL: $iconLocalUrl" -ForegroundColor White
    Write-Host "    Maskable icon URL: $iconLocalUrl" -ForegroundColor White
    Write-Host "    Include shortcuts: $(if ($hasShortcuts) { 'Yes' } else { 'No' })" -ForegroundColor White
    Write-Host "    Key store location: $keystorePath" -ForegroundColor White
    Write-Host "    Key name: android" -ForegroundColor White
    exit 1
} finally {
    # Limpiar archivo temporal
    if (Test-Path $responsesFile) {
        Remove-Item $responsesFile -Force -ErrorAction SilentlyContinue
    }
}
Write-Host ""

# 12. Actualizar icono con archivo local
Write-Host "12. Actualizando icono con archivo local..." -ForegroundColor Yellow
# $iconLocalUrl ya está definido en la sección 11

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
    # Determinar cómo ejecutar Bubblewrap (reutilizar la lógica de la sección 11)
    $bubblewrapUpdateCmd = $null
    $bubblewrapUpdateUseCmd = $false
    
    $bubblewrapUpdateCmdPath = Join-Path $ScriptDir "node_modules\.bin\bubblewrap.cmd"
    $bubblewrapUpdatePath = Join-Path $ScriptDir "node_modules\.bin\bubblewrap"
    
    if (Test-Path $bubblewrapUpdateCmdPath) {
        $bubblewrapUpdateCmd = (Resolve-Path $bubblewrapUpdateCmdPath).Path
        $bubblewrapUpdateUseCmd = $true
    } elseif (Test-Path $bubblewrapUpdatePath) {
        $bubblewrapUpdateCmd = (Resolve-Path $bubblewrapUpdatePath).Path
    } elseif (Get-Command bubblewrap -ErrorAction SilentlyContinue) {
        $bubblewrapUpdateCmd = (Get-Command bubblewrap).Source
    } else {
        $bubblewrapUpdateCmd = "npx"
    }
    
    if ($bubblewrapUpdateCmd -eq "npx") {
        $updateOutput = & npx @bubblewrap/cli update --icon $IconPath --directory $AndroidDir 2>&1
    } elseif ($bubblewrapUpdateUseCmd) {
        $updateOutput = & cmd.exe /c "$bubblewrapUpdateCmd update --icon `"$IconPath`" --directory `"$AndroidDir`"" 2>&1
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

# 13. Configurar variables de entorno para Gradle
Write-Host "13. Configurando variables de entorno..." -ForegroundColor Yellow

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

# Configurar app/build.gradle para APK sin firma
$appBuildGradlePath = Join-Path $AndroidDir "app\build.gradle"
if (Test-Path $appBuildGradlePath) {
    $appBuildGradleContent = Get-Content $appBuildGradlePath -Raw
    
    # Modificar buildTypes para que release no requiera firma
    if ($appBuildGradleContent -match "buildTypes\s*\{") {
        # Buscar el bloque release y agregar signingConfig null si no existe
        if ($appBuildGradleContent -match "release\s*\{") {
            # Si release no tiene signingConfig, agregarlo
            if ($appBuildGradleContent -notmatch "release\s*\{[^}]*signingConfig") {
                # Reemplazar release { ... } agregando signingConfig null
                $appBuildGradleContent = $appBuildGradleContent -replace "(release\s*\{[^\}]*?)(minifyEnabled\s+true)", "`$1`$2`n            signingConfig null  // APK sin firma para pruebas"
                
                # Si no tiene minifyEnabled, agregarlo junto con signingConfig
                if ($appBuildGradleContent -notmatch "release\s*\{[^}]*minifyEnabled") {
                    $appBuildGradleContent = $appBuildGradleContent -replace "(release\s*\{)", "`$1`n            minifyEnabled true`n            signingConfig null  // APK sin firma para pruebas"
                }
            }
        }
    }
    
    Set-Content -Path $appBuildGradlePath -Value $appBuildGradleContent -NoNewline
    Write-Host "    app/build.gradle configurado para APK sin firma" -ForegroundColor Green
}
Write-Host ""

# 14. Construir APK
Write-Host "14. Construyendo APK..." -ForegroundColor Yellow
Write-Host "    Esto puede tardar varios minutos..." -ForegroundColor Gray

Set-Location $AndroidDir

try {
    # Detener daemons de Gradle anteriores
    & .\gradlew.bat --stop 2>&1 | Out-Null
    
    # Construir APK sin firma (usar assembleDebug o assembleRelease sin signingConfig)
    Write-Host "    Ejecutando Gradle para generar APK sin firma..." -ForegroundColor Gray
    $buildOutput = & .\gradlew.bat assembleRelease --no-daemon 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "    Intentando con assembleDebug..." -ForegroundColor Yellow
        $buildOutput = & .\gradlew.bat assembleDebug --no-daemon 2>&1
        if ($LASTEXITCODE -ne 0) {
            throw "Gradle falló con código: $LASTEXITCODE"
        }
    }
    
    # Buscar APK generado (primero en release, luego en debug)
    $apk = Get-ChildItem -Path "app\build\outputs\apk\release" -Filter "*.apk" -ErrorAction SilentlyContinue | Select-Object -First 1
    if (-not $apk) {
        $apk = Get-ChildItem -Path "app\build\outputs\apk\debug" -Filter "*.apk" -ErrorAction SilentlyContinue | Select-Object -First 1
    }
    
    if ($apk) {
        # Validar que el APK tenga tamaño válido
        if ($apk.Length -eq 0) {
            throw "APK generado está vacío (0 bytes)"
        }
        
        if ($apk.Length -lt 100KB) {
            Write-Host "    Advertencia: APK muy pequeño ($([math]::Round($apk.Length / 1KB, 2)) KB), puede estar corrupto" -ForegroundColor Yellow
        }
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "  APK construido exitosamente" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Green
        Write-Host ""
        Write-Host "Ubicación: $($apk.FullName)" -ForegroundColor Cyan
        Write-Host "Tamaño: $([math]::Round($apk.Length / 1MB, 2)) MB" -ForegroundColor White
        Write-Host "Tipo: APK sin firma (para pruebas)" -ForegroundColor Yellow
        Write-Host ""
        
        # Intentar obtener información del APK si aapt está disponible
        $aaptPath = Join-Path $env:ANDROID_HOME "build-tools\*\aapt.exe"
        $aapt = Get-ChildItem -Path $aaptPath -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($aapt) {
            try {
                $apkInfo = & $aapt.FullName dump badging $apk.FullName 2>&1 | Select-String -Pattern "package:|versionCode=|versionName="
                if ($apkInfo) {
                    Write-Host "Información del APK:" -ForegroundColor Cyan
                    $apkInfo | ForEach-Object {
                        Write-Host "  $_" -ForegroundColor Gray
                    }
                    Write-Host ""
                }
            } catch {
                # Ignorar errores de aapt
            }
        }
        
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
    Write-Host ""
    Write-Host "    Soluciones posibles:" -ForegroundColor Yellow
    Write-Host "    - Verifica que Java 17 esté configurado correctamente" -ForegroundColor White
    Write-Host "    - Verifica que ANDROID_HOME esté configurado" -ForegroundColor White
    Write-Host "    - Revisa los logs de Gradle para más detalles" -ForegroundColor White
    
    # Limpiar procesos en caso de error
    Write-Host ""
    Write-Host "    Limpiando procesos..." -ForegroundColor Yellow
    Stop-BackgroundProcesses "node"
    Stop-BackgroundProcesses "python"
    if (-not $SkipNgrok) {
        Stop-BackgroundProcesses "ngrok"
    }
    
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

