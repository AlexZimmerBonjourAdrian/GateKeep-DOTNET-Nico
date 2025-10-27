# GateKeep Database Update Script
# Script simple para actualizar migraciones de Entity Framework Core

# Configuración
$ProjectPath = "src\GateKeep.Api"

# Colores para output
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

# Función para aplicar migraciones
function Update-Database {
    Write-ColorOutput "Actualizando base de datos..." "Yellow"
    
    try {
        Set-Location $ProjectPath
        
        Write-ColorOutput "Aplicando migraciones pendientes..." "Cyan"
        $result = dotnet ef database update 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-ColorOutput "Base de datos actualizada exitosamente" "Green"
            Write-Host $result
        } else {
            Write-ColorOutput "Error actualizando base de datos: $result" "Red"
            exit 1
        }
    } catch {
        Write-ColorOutput "Error: $($_.Exception.Message)" "Red"
        exit 1
    } finally {
        Set-Location "..\.."
    }
}

# Función principal
function Main {
    Write-ColorOutput "GateKeep Database Update" "Cyan"
    Write-ColorOutput "===========================" "Cyan"
    
    # Verificar que estamos en el directorio correcto
    if (!(Test-Path $ProjectPath)) {
        Write-ColorOutput "No se encontró el proyecto en: $ProjectPath" "Red"
        Write-ColorOutput "Asegúrate de ejecutar este script desde la raíz del proyecto" "Yellow"
        exit 1
    }
    
    # Verificar que existe el proyecto
    if (!(Test-Path "$ProjectPath\GateKeep.Api.csproj")) {
        Write-ColorOutput "No se encontró el archivo del proyecto" "Red"
        exit 1
    }
    
    # Actualizar base de datos
    Update-Database
    
    Write-ColorOutput "Proceso completado" "Green"
}

# Ejecutar función principal
Main
