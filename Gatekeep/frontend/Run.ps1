# Script para levantar el frontend de GateKeep
# Autor: Sistema automatizado
# Fecha: $(Get-Date -Format "yyyy-MM-dd")

Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "    GATEKEEP FRONTEND - SCRIPT DE INICIO" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host ""

# Asegurar que el directorio de trabajo es el del script
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
try {
    Set-Location -Path $ScriptDir
    Write-Host "Directorio de trabajo establecido a: $ScriptDir" -ForegroundColor Green
} catch {
    Write-Host "ERROR no se pudo establecer el directorio de trabajo: $ScriptDir" -ForegroundColor Red
}

# Funcion para verificar si Node.js esta instalado
function Test-NodeJS {
    try {
        $nodeVersion = node --version 2>$null
        if ($nodeVersion) {
            Write-Host "OK Node.js detectado: $nodeVersion" -ForegroundColor Green
            return $true
        }
    }
    catch {
        Write-Host "ERROR Node.js no esta instalado o no esta en el PATH" -ForegroundColor Red
        return $false
    }
}

# Funcion para verificar si npm esta instalado
function Test-NPM {
    try {
        $npmVersion = npm --version 2>$null
        if ($npmVersion) {
            Write-Host "OK npm detectado: v$npmVersion" -ForegroundColor Green
            return $true
        }
    }
    catch {
        Write-Host "ERROR npm no esta instalado o no esta en el PATH" -ForegroundColor Red
        return $false
    }
}

# Funcion para verificar dependencias
function Test-Dependencies {
    Write-Host "Verificando dependencias..." -ForegroundColor Yellow
    
    if (Test-Path "package.json") {
        Write-Host "OK package.json encontrado" -ForegroundColor Green
    } else {
        Write-Host "ERROR package.json no encontrado" -ForegroundColor Red
        return $false
    }
    
    if (Test-Path "node_modules") {
        Write-Host "OK node_modules encontrado" -ForegroundColor Green
    } else {
        Write-Host "WARNING node_modules no encontrado - ejecutando npm install..." -ForegroundColor Yellow
        npm install
        if ($LASTEXITCODE -ne 0) {
            Write-Host "ERROR al instalar dependencias" -ForegroundColor Red
            return $false
        }
    }
    
    # Verificar dependencias criticas
    $criticalDeps = @("next", "react", "primereact")
    foreach ($dep in $criticalDeps) {
        if (Test-Path "node_modules\$dep") {
            Write-Host "OK $dep instalado" -ForegroundColor Green
        } else {
            Write-Host "ERROR $dep no encontrado" -ForegroundColor Red
            return $false
        }
    }
    
    return $true
}

# Funcion para verificar configuracion de Next.js
function Test-NextJSConfig {
    Write-Host "Verificando configuracion de Next.js..." -ForegroundColor Yellow
    
    if (Test-Path "next.config.js") {
        Write-Host "OK next.config.js encontrado" -ForegroundColor Green
    } else {
        Write-Host "ERROR next.config.js no encontrado" -ForegroundColor Red
        return $false
    }
    
    if (Test-Path "app") {
        Write-Host "OK Directorio app encontrado" -ForegroundColor Green
    } else {
        Write-Host "ERROR Directorio app no encontrado" -ForegroundColor Red
        return $false
    }
    
    if (Test-Path "app/layout.js") {
        Write-Host "OK app/layout.js encontrado" -ForegroundColor Green
    } else {
        Write-Host "ERROR app/layout.js no encontrado" -ForegroundColor Red
        return $false
    }
    
    if (Test-Path "app/page.js") {
        Write-Host "OK app/page.js encontrado" -ForegroundColor Green
    } else {
        Write-Host "ERROR app/page.js no encontrado" -ForegroundColor Red
        return $false
    }
    
    return $true
}

# Funcion para limpiar procesos anteriores
function Clear-PreviousProcesses {
    Write-Host "Limpiando procesos anteriores..." -ForegroundColor Yellow
    
    # Buscar procesos de Node.js que puedan estar usando el puerto 3000
    $processes = Get-Process -Name "node" -ErrorAction SilentlyContinue
    if ($processes) {
        foreach ($process in $processes) {
            try {
                $process.Kill()
                Write-Host "OK Proceso Node.js terminado (PID: $($process.Id))" -ForegroundColor Green
            }
            catch {
                Write-Host "WARNING No se pudo terminar el proceso (PID: $($process.Id))" -ForegroundColor Yellow
            }
        }
    }
    
    # Esperar un momento para que los puertos se liberen
    Start-Sleep -Seconds 2
}

# Funcion principal
function Start-Frontend {
    Write-Host "Iniciando servidor de desarrollo..." -ForegroundColor Yellow
    Write-Host ""
    
    # Mostrar informacion del proyecto
    Write-Host "Informacion del proyecto:" -ForegroundColor Cyan
    Write-Host "- Framework: Next.js 15" -ForegroundColor White
    Write-Host "- UI Library: PrimeReact" -ForegroundColor White
    Write-Host "- Estilos: CSS Personalizado + PrimeFlex" -ForegroundColor White
    Write-Host "- Puerto: 3000" -ForegroundColor White
    Write-Host ""
    
    # Ejecutar el servidor de desarrollo
    Write-Host "Ejecutando: npm run dev" -ForegroundColor Cyan
    Write-Host "Presiona Ctrl+C para detener el servidor" -ForegroundColor Yellow
    Write-Host ""
    
    npm run dev
}

# ===============================================
# EJECUCION PRINCIPAL
# ===============================================

try {
    # Verificaciones previas
    Write-Host "Realizando verificaciones previas..." -ForegroundColor Yellow
    Write-Host ""
    
    if (-not (Test-NodeJS)) {
        Write-Host "Error: Node.js es requerido para ejecutar este proyecto" -ForegroundColor Red
        exit 1
    }
    
    if (-not (Test-NPM)) {
        Write-Host "Error: npm es requerido para ejecutar este proyecto" -ForegroundColor Red
        exit 1
    }
    
    Write-Host ""
    
    # Verificar dependencias
    if (-not (Test-Dependencies)) {
        Write-Host "Error: Las dependencias no estan correctamente instaladas" -ForegroundColor Red
        exit 1
    }
    
    Write-Host ""
    
    # Verificar configuracion
    if (-not (Test-NextJSConfig)) {
        Write-Host "Error: La configuracion de Next.js no es valida" -ForegroundColor Red
        exit 1
    }
    
    Write-Host ""
    
    # Limpiar procesos anteriores
    Clear-PreviousProcesses
    
    Write-Host ""
    Write-Host "OK Todas las verificaciones completadas exitosamente" -ForegroundColor Green
    Write-Host ""
    
    # Iniciar el frontend
    Start-Frontend
    
} catch {
    Write-Host ""
    Write-Host "ERROR inesperado: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Por favor, revisa la configuracion del proyecto" -ForegroundColor Yellow
    exit 1
}