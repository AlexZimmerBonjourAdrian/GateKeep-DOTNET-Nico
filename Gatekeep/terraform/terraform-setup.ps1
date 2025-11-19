#!/usr/bin/env pwsh
<#
.SYNOPSIS
Setup de Terraform para GateKeep - Inicia infraestructura en AWS

.DESCRIPTION
Script que inicializa y aplica la configuración de Terraform para el proyecto GateKeep.
Verifica requisitos, inicializa Terraform y aplica la configuración.

.EXAMPLE
./terraform-setup.ps1
#>

param(
    [switch]$Auto,  # Auto-approve sin preguntar
    [switch]$Plan   # Solo mostrar plan, no aplicar
)

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  GateKeep - Setup de Terraform"
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 1. Verificar requisitos
Write-Host "1. Verificando requisitos..." -ForegroundColor Yellow

$checks = @{
    "AWS CLI" = { aws --version 2>&1 }
    "Terraform" = { terraform version 2>&1 }
    "Git" = { git --version 2>&1 }
}

foreach ($check in $checks.GetEnumerator()) {
    try {
        $result = & $check.Value
        Write-Host "   ✓ $($check.Key) instalado" -ForegroundColor Green
    } catch {
        Write-Host "   ✗ $($check.Key) NO instalado" -ForegroundColor Red
        exit 1
    }
}

# 2. Verificar AWS CLI configurado
Write-Host ""
Write-Host "2. Verificando AWS CLI..." -ForegroundColor Yellow
try {
    $identity = aws sts get-caller-identity --output json 2>&1 | ConvertFrom-Json
    Write-Host "   ✓ AWS CLI configurado" -ForegroundColor Green
    Write-Host "   Account: $($identity.Account)" -ForegroundColor Gray
    Write-Host "   User: $($identity.Arn)" -ForegroundColor Gray
} catch {
    Write-Host "   ✗ AWS CLI no configurado. Ejecuta: aws configure" -ForegroundColor Red
    exit 1
}

# 3. Verificar terraform.tfvars
Write-Host ""
Write-Host "3. Verificando terraform.tfvars..." -ForegroundColor Yellow
if (-not (Test-Path "terraform.tfvars")) {
    if (Test-Path "terraform.tfvars.example") {
        Write-Host "   ⚠  terraform.tfvars no encontrado" -ForegroundColor Yellow
        Write-Host "   Copiar de terraform.tfvars.example? (S/N)" -ForegroundColor Cyan
        $response = Read-Host
        if ($response -eq "S" -or $response -eq "s") {
            Copy-Item terraform.tfvars.example terraform.tfvars
            Write-Host "   Edita terraform.tfvars con tus valores" -ForegroundColor Yellow
        }
    }
} else {
    Write-Host "   ✓ terraform.tfvars encontrado" -ForegroundColor Green
}

# 4. Inicializar Terraform
Write-Host ""
Write-Host "4. Inicializando Terraform..." -ForegroundColor Yellow
try {
    terraform init -input=false
    Write-Host "   ✓ Terraform inicializado" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Error inicializando Terraform" -ForegroundColor Red
    exit 1
}

# 5. Validar configuración
Write-Host ""
Write-Host "5. Validando configuración..." -ForegroundColor Yellow
try {
    terraform validate | Out-Null
    Write-Host "   ✓ Configuración válida" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Configuración inválida" -ForegroundColor Red
    exit 1
}

# 6. Plan de Terraform
Write-Host ""
Write-Host "6. Generando plan..." -ForegroundColor Yellow
try {
    terraform plan -out=tfplan
} catch {
    Write-Host "   ✗ Error generando plan" -ForegroundColor Red
    exit 1
}

# 7. Aplicar si no es solo plan
if ($Plan) {
    Write-Host ""
    Write-Host "Plan guardado en tfplan. Para aplicar:" -ForegroundColor Cyan
    Write-Host "  terraform apply tfplan" -ForegroundColor Cyan
    exit 0
}

# 8. Aplicar cambios
Write-Host ""
Write-Host "7. Aplicando cambios..." -ForegroundColor Yellow

if ($Auto) {
    Write-Host "   Auto-approve habilitado" -ForegroundColor Yellow
    terraform apply tfplan
} else {
    Write-Host ""
    Write-Host "¿Aplicar los cambios anteriores? (S/N)" -ForegroundColor Cyan
    $response = Read-Host
    if ($response -eq "S" -o $response -eq "s") {
        terraform apply tfplan
    } else {
        Write-Host "   Cancelado por el usuario" -ForegroundColor Yellow
        exit 0
    }
}

# 9. Mostrar outputs
Write-Host ""
Write-Host "8. Outputs de Terraform:" -ForegroundColor Yellow
terraform output

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  ✓ Setup completado exitosamente"
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "URLs de acceso:" -ForegroundColor Cyan
Write-Host "  Frontend: $(terraform output -raw frontend_url 2>/dev/null)" -ForegroundColor Green
Write-Host "  API: $(terraform output -raw backend_api_url 2>/dev/null)" -ForegroundColor Green
Write-Host ""
