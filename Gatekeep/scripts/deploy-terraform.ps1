# Script para desplegar la infraestructura de GateKeep con Terraform

param(
    [switch]$Plan,
    [switch]$Apply,
    [switch]$Destroy,
    [switch]$Output
)

$ErrorActionPreference = "Stop"

Write-Host "=== Despliegue de Infraestructura GateKeep con Terraform ===" -ForegroundColor Cyan
Write-Host ""

# Cambiar al directorio de Terraform
$terraformDir = Join-Path $PSScriptRoot ".." "terraform"
Push-Location $terraformDir

try {
    # Verificar que Terraform está instalado
    Write-Host "1. Verificando Terraform..." -ForegroundColor Yellow
    $terraformVersion = terraform version 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Terraform no está instalado" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ Terraform instalado" -ForegroundColor Green
    terraform version | Select-Object -First 1

    # Inicializar Terraform
    Write-Host ""
    Write-Host "2. Inicializando Terraform..." -ForegroundColor Yellow
    terraform init
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Fallo al inicializar Terraform" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ Terraform inicializado" -ForegroundColor Green

    # Validar configuración
    Write-Host ""
    Write-Host "3. Validando configuración..." -ForegroundColor Yellow
    terraform validate
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: La configuración de Terraform tiene errores" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ Configuración válida" -ForegroundColor Green

    # Ejecutar acción solicitada
    if ($Plan) {
        Write-Host ""
        Write-Host "4. Ejecutando terraform plan..." -ForegroundColor Yellow
        terraform plan
    }
    elseif ($Apply) {
        Write-Host ""
        Write-Host "4. Ejecutando terraform apply..." -ForegroundColor Yellow
        Write-Host "⚠ ADVERTENCIA: Esto creará recursos en AWS que pueden generar costos" -ForegroundColor Yellow
        $confirm = Read-Host "¿Continuar? (yes/no)"
        if ($confirm -eq "yes") {
            terraform apply -auto-approve
            if ($LASTEXITCODE -eq 0) {
                Write-Host ""
                Write-Host "✓ Infraestructura desplegada exitosamente" -ForegroundColor Green
                Write-Host ""
                Write-Host "5. Mostrando outputs..." -ForegroundColor Yellow
                terraform output
            }
        } else {
            Write-Host "Operación cancelada" -ForegroundColor Yellow
        }
    }
    elseif ($Destroy) {
        Write-Host ""
        Write-Host "4. Ejecutando terraform destroy..." -ForegroundColor Yellow
        Write-Host "⚠ ADVERTENCIA: Esto DESTRUIRÁ todos los recursos creados" -ForegroundColor Red
        $confirm = Read-Host "¿Estás seguro? Escribe 'yes' para continuar"
        if ($confirm -eq "yes") {
            terraform destroy -auto-approve
        } else {
            Write-Host "Operación cancelada" -ForegroundColor Yellow
        }
    }
    elseif ($Output) {
        Write-Host ""
        Write-Host "4. Mostrando outputs..." -ForegroundColor Yellow
        terraform output
    }
    else {
        Write-Host ""
        Write-Host "Uso:" -ForegroundColor Cyan
        Write-Host "  .\deploy-terraform.ps1 -Plan      # Ver qué se va a crear" -ForegroundColor White
        Write-Host "  .\deploy-terraform.ps1 -Apply      # Crear la infraestructura" -ForegroundColor White
        Write-Host "  .\deploy-terraform.ps1 -Output    # Ver outputs" -ForegroundColor White
        Write-Host "  .\deploy-terraform.ps1 -Destroy  # Destruir la infraestructura" -ForegroundColor White
    }
}
catch {
    Write-Host ""
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
finally {
    Pop-Location
}

Write-Host ""
Write-Host "=== Completado ===" -ForegroundColor Green

