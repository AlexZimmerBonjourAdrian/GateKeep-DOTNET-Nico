# GateKeep Infrastructure Setup Script
# Script para instalar y configurar todas las dependencias del proyecto
# Incluye: Docker, RabbitMQ, PostgreSQL (verificación), .NET SDK, Paquetes NuGet

$ErrorActionPreference = "Stop"
$ProjectPath = "src\GateKeep.Api"

# Colores para output
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

# Función para verificar si un comando existe
function Test-Command {
    param([string]$Command)
    $null = Get-Command $Command -ErrorAction SilentlyContinue
    return $?
}

# Función para verificar Docker
function Test-Docker {
    Write-ColorOutput "Verificando Docker..." "Yellow"
    
    if (Test-Command "docker") {
        $version = docker --version
        Write-ColorOutput "Docker encontrado: $version" "Green"
        return $true
    } else {
        Write-ColorOutput "Docker no está instalado" "Red"
        Write-ColorOutput "Por favor, instala Docker Desktop desde: https://www.docker.com/products/docker-desktop" "Yellow"
        return $false
    }
}

# Función para verificar si Docker está corriendo
function Test-DockerRunning {
    try {
        docker ps | Out-Null
        return $true
    } catch {
        return $false
    }
}

# Función para iniciar RabbitMQ en Docker
function Start-RabbitMQ {
    Write-ColorOutput "Configurando RabbitMQ..." "Yellow"
    
    if (-not (Test-DockerRunning)) {
        Write-ColorOutput "Docker no está corriendo. Por favor inicia Docker Desktop y vuelve a ejecutar este script." "Red"
        return $false
    }
    
    # Verificar si el contenedor ya existe
    $containerExists = docker ps -a --filter "name=rabbitmq" --format "{{.Names}}" | Select-String "rabbitmq"
    
    if ($containerExists) {
        Write-ColorOutput "Contenedor RabbitMQ ya existe" "Cyan"
        
        # Verificar si está corriendo
        $containerRunning = docker ps --filter "name=rabbitmq" --format "{{.Names}}" | Select-String "rabbitmq"
        
        if ($containerRunning) {
            Write-ColorOutput "RabbitMQ ya está corriendo" "Green"
        } else {
            Write-ColorOutput "Iniciando contenedor RabbitMQ existente..." "Yellow"
            docker start rabbitmq
            Start-Sleep -Seconds 3
            Write-ColorOutput "RabbitMQ iniciado exitosamente" "Green"
        }
    } else {
        Write-ColorOutput "Creando e iniciando contenedor RabbitMQ..." "Yellow"
        docker run -d `
            --name rabbitmq `
            -p 5672:5672 `
            -p 15672:15672 `
            -e RABBITMQ_DEFAULT_USER=guest `
            -e RABBITMQ_DEFAULT_PASS=guest `
            rabbitmq:3-management
        
        if ($LASTEXITCODE -eq 0) {
            Write-ColorOutput "Esperando a que RabbitMQ inicie (10 segundos)..." "Yellow"
            Start-Sleep -Seconds 10
            Write-ColorOutput "RabbitMQ iniciado exitosamente" "Green"
            Write-ColorOutput "Panel de gestión disponible en: http://localhost:15672" "Cyan"
            Write-ColorOutput "Credenciales: guest / guest" "Cyan"
        } else {
            Write-ColorOutput "Error al iniciar RabbitMQ" "Red"
            return $false
        }
    }
    
    return $true
}

# Función para verificar .NET SDK
function Test-DotNetSDK {
    Write-ColorOutput "Verificando .NET SDK..." "Yellow"
    
    if (Test-Command "dotnet") {
        $version = dotnet --version
        Write-ColorOutput ".NET SDK encontrado: $version" "Green"
        
        # Verificar versión mínima (8.0)
        $versionNumber = [Version]$version
        if ($versionNumber.Major -lt 8) {
            Write-ColorOutput "Se requiere .NET SDK 8.0 o superior. Versión actual: $version" "Yellow"
            Write-ColorOutput "Descarga desde: https://dotnet.microsoft.com/download" "Yellow"
            return $false
        }
        
        return $true
    } else {
        Write-ColorOutput ".NET SDK no está instalado" "Red"
        Write-ColorOutput "Descarga desde: https://dotnet.microsoft.com/download" "Yellow"
        return $false
    }
}

# Función para verificar PostgreSQL (opcional, solo verifica conexión)
function Test-PostgreSQL {
    Write-ColorOutput "Verificando configuración PostgreSQL..." "Yellow"
    
    if (Test-Path "$ProjectPath\config.json") {
        try {
            $config = Get-Content "$ProjectPath\config.json" | ConvertFrom-Json
            
            if ($config.database) {
                $dbHost = $config.database.host
                $dbPort = $config.database.port
                
                Write-ColorOutput "PostgreSQL configurado en config.json: $dbHost`:$dbPort" "Cyan"
                Write-ColorOutput "Asegúrate de que PostgreSQL esté corriendo en esa dirección" "Yellow"
                return $true
            } else {
                Write-ColorOutput "No se encontró configuración de PostgreSQL en config.json" "Yellow"
                return $false
            }
        } catch {
            Write-ColorOutput "Error al leer config.json: $_" "Yellow"
            return $false
        }
    } else {
        Write-ColorOutput "No se encontró config.json" "Yellow"
        return $false
    }
}

# Función para agregar configuración RabbitMQ a config.json
function Add-RabbitMQConfig {
    Write-ColorOutput "Configurando RabbitMQ en config.json..." "Yellow"
    
    $configPath = "$ProjectPath\config.json"
    
    if (-not (Test-Path $configPath)) {
        Write-ColorOutput "No se encontró config.json" "Red"
        return $false
    }
    
    try {
        $config = Get-Content $configPath | ConvertFrom-Json
        
        # Verificar si ya existe configuración de RabbitMQ
        if ($config.rabbitmq) {
            Write-ColorOutput "La configuración de RabbitMQ ya existe en config.json" "Green"
            return $true
        }
        
        # Agregar configuración de RabbitMQ
        $rabbitmqConfig = @{
            host = "localhost"
            port = "5672"
            username = "guest"
            password = "guest"
            virtualHost = "/"
            connectionName = "GateKeep.Api"
        }
        
        $config | Add-Member -MemberType NoteProperty -Name "rabbitmq" -Value $rabbitmqConfig
        
        # Convertir a JSON con formato bonito
        $jsonContent = $config | ConvertTo-Json -Depth 10
        
        # Guardar archivo
        $jsonContent | Set-Content $configPath -Encoding UTF8
        
        Write-ColorOutput "Configuración de RabbitMQ agregada a config.json" "Green"
        return $true
    } catch {
        Write-ColorOutput "Error al actualizar config.json: $_" "Red"
        return $false
    }
}

# Función para instalar/restaurar paquetes NuGet
function Install-NuGetPackages {
    Write-ColorOutput "Verificando paquetes NuGet..." "Yellow"
    
    if (-not (Test-Path "$ProjectPath\GateKeep.Api.csproj")) {
        Write-ColorOutput "No se encontró el archivo del proyecto" "Red"
        return $false
    }
    
    try {
        Set-Location $ProjectPath
        
        Write-ColorOutput "Restaurando paquetes NuGet..." "Cyan"
        dotnet restore
        
        if ($LASTEXITCODE -eq 0) {
            Write-ColorOutput "Paquetes NuGet restaurados exitosamente" "Green"
        } else {
            Write-ColorOutput "Error al restaurar paquetes NuGet" "Red"
            return $false
        }
        
        # Verificar si MassTransit está instalado
        $csprojContent = Get-Content "GateKeep.Api.csproj" -Raw
        
        if ($csprojContent -match "MassTransit") {
            Write-ColorOutput "MassTransit ya está en el proyecto" "Green"
        } else {
            Write-ColorOutput "MassTransit no está instalado. Agregando paquetes..." "Yellow"
            
            dotnet add package MassTransit --version 8.2.5
            dotnet add package MassTransit.RabbitMQ --version 8.2.5
            dotnet add package MassTransit.Extensions.DependencyInjection --version 8.2.5
            
            if ($LASTEXITCODE -eq 0) {
                Write-ColorOutput "Paquetes MassTransit instalados exitosamente" "Green"
            } else {
                Write-ColorOutput "Error al instalar paquetes MassTransit" "Red"
                return $false
            }
        }
        
        return $true
    } catch {
        Write-ColorOutput "Error: $($_.Exception.Message)" "Red"
        return $false
    } finally {
        Set-Location "..\.."
    }
}

# Función para verificar conexiones
function Test-Connections {
    Write-ColorOutput "Verificando conexiones..." "Yellow"
    
    # Verificar RabbitMQ
    try {
        $tcpClient = New-Object System.Net.Sockets.TcpClient
        $tcpClient.Connect("localhost", 5672)
        if ($tcpClient.Connected) {
            Write-ColorOutput "Conexión a RabbitMQ (puerto 5672): OK" "Green"
            $tcpClient.Close()
        }
    } catch {
        Write-ColorOutput "Conexión a RabbitMQ (puerto 5672): ERROR - $_" "Red"
    }
    
    # Verificar PostgreSQL
    if (Test-Path "$ProjectPath\config.json") {
        try {
            $config = Get-Content "$ProjectPath\config.json" | ConvertFrom-Json
            if ($config.database) {
                $dbHost = $config.database.host
                $dbPort = $config.database.port
                
                $tcpClient = New-Object System.Net.Sockets.TcpClient
                $tcpClient.Connect($dbHost, $dbPort)
                if ($tcpClient.Connected) {
                    Write-ColorOutput "Conexión a PostgreSQL ($dbHost`:$dbPort): OK" "Green"
                    $tcpClient.Close()
                }
            }
        } catch {
            Write-ColorOutput "Conexión a PostgreSQL: ERROR - Asegúrate de que PostgreSQL esté corriendo" "Yellow"
        }
    }
}

# Función principal
function Main {
    Write-ColorOutput "========================================" "Cyan"
    Write-ColorOutput "  GateKeep Infrastructure Setup" "Cyan"
    Write-ColorOutput "========================================" "Cyan"
    Write-Host ""
    
    # Verificar que estamos en el directorio correcto
    if (-not (Test-Path $ProjectPath)) {
        Write-ColorOutput "No se encontró el proyecto en: $ProjectPath" "Red"
        Write-ColorOutput "Asegúrate de ejecutar este script desde la raíz del proyecto" "Yellow"
        exit 1
    }
    
    $success = $true
    
    # 1. Verificar Docker (solo advertencia, no bloquea el proceso)
    $dockerAvailable = Test-Docker
    if (-not $dockerAvailable) {
        Write-ColorOutput "Docker es requerido para RabbitMQ. El script continuará con otras tareas." "Yellow"
    }
    
    # 2. Iniciar RabbitMQ (solo si Docker está disponible)
    if ($dockerAvailable) {
        if (-not (Start-RabbitMQ)) {
            Write-ColorOutput "Advertencia: No se pudo iniciar RabbitMQ, pero el script continuará" "Yellow"
        }
    } else {
        Write-ColorOutput "Omitiendo configuración de RabbitMQ - Docker no está disponible" "Yellow"
    }
    
    # 3. Verificar .NET SDK
    if (-not (Test-DotNetSDK)) {
        $success = $false
    }
    
    # 4. Verificar PostgreSQL (solo verificación, no instalación)
    $pgResult = Test-PostgreSQL
    if (-not $pgResult) {
        Write-ColorOutput "Advertencia: No se pudo verificar PostgreSQL" "Yellow"
    }
    
    # 5. Agregar configuración RabbitMQ a config.json
    if (-not (Add-RabbitMQConfig)) {
        Write-ColorOutput "Advertencia: No se pudo actualizar config.json automáticamente" "Yellow"
    }
    
    # 6. Instalar/restaurar paquetes NuGet (siempre intentar, incluso si Docker falló)
    if (-not (Install-NuGetPackages)) {
        Write-ColorOutput "Error al instalar paquetes NuGet" "Red"
        $success = $false
    }
    
    # 7. Verificar conexiones
    Write-Host ""
    Test-Connections
    
    # Resumen
    Write-Host ""
    Write-ColorOutput "========================================" "Cyan"
    if ($success) {
        Write-ColorOutput "  Proceso completado exitosamente" "Green"
        Write-Host ""
        Write-ColorOutput "Próximos pasos:" "Yellow"
        Write-ColorOutput "1. Asegúrate de que PostgreSQL esté corriendo" "Cyan"
        Write-ColorOutput "2. Configura el código de mensajería en la aplicación" "Cyan"
        Write-ColorOutput "3. Accede a RabbitMQ Management: http://localhost:15672" "Cyan"
    } else {
        Write-ColorOutput "  Proceso completado con errores" "Red"
        Write-ColorOutput "  Revisa los mensajes anteriores para más detalles" "Yellow"
        Write-Host ""
        Write-ColorOutput "Notas:" "Yellow"
        Write-ColorOutput "- Docker es requerido para RabbitMQ. Instálalo si planeas usar mensajería asíncrona." "Cyan"
        Write-ColorOutput "- PostgreSQL debe estar corriendo. Verifica que esté instalado y en ejecución." "Cyan"
    }
    Write-ColorOutput "========================================" "Cyan"
}

# Ejecutar función principal
Main

