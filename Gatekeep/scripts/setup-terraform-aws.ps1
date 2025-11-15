# Script para configurar Terraform con AWS
# Este script ayuda a configurar las credenciales de AWS para usar con Terraform

Write-Host "=== Configuración de Terraform con AWS ===" -ForegroundColor Cyan
Write-Host ""

# Verificar que Terraform está instalado
Write-Host "1. Verificando instalación de Terraform..." -ForegroundColor Yellow
$terraformVersion = terraform version 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Terraform no está instalado o no está en el PATH" -ForegroundColor Red
    Write-Host "Instala Terraform primero: winget install HashiCorp.Terraform" -ForegroundColor Yellow
    exit 1
}
Write-Host "✓ Terraform instalado" -ForegroundColor Green
terraform version | Select-Object -First 1

Write-Host ""
Write-Host "2. Verificando instalación de AWS CLI..." -ForegroundColor Yellow
$awsVersion = aws --version 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ADVERTENCIA: AWS CLI no está instalado" -ForegroundColor Yellow
    Write-Host "Puedes instalarlo con: winget install Amazon.AWSCLI" -ForegroundColor Yellow
    Write-Host "O configurar credenciales usando variables de entorno" -ForegroundColor Yellow
} else {
    Write-Host "✓ AWS CLI instalado" -ForegroundColor Green
    aws --version
}

Write-Host ""
Write-Host "3. Verificando configuración de AWS..." -ForegroundColor Yellow
$awsConfig = aws configure list 2>&1
if ($awsConfig -match "access_key.*<not set>") {
    Write-Host "⚠ AWS CLI no está configurado" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Opciones para configurar credenciales:" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "OPCIÓN 1: Usar AWS CLI configure (Recomendado)" -ForegroundColor Green
    Write-Host "  Ejecuta: aws configure" -ForegroundColor White
    Write-Host "  Te pedirá:" -ForegroundColor Gray
    Write-Host "    - AWS Access Key ID" -ForegroundColor Gray
    Write-Host "    - AWS Secret Access Key" -ForegroundColor Gray
    Write-Host "    - Default region: sa-east-1" -ForegroundColor Gray
    Write-Host "    - Default output format: json" -ForegroundColor Gray
    Write-Host ""
    Write-Host "OPCIÓN 2: Variables de entorno" -ForegroundColor Green
    Write-Host "  Ejecuta estos comandos en PowerShell:" -ForegroundColor White
    Write-Host "    `$env:AWS_ACCESS_KEY_ID = 'tu-access-key-id'" -ForegroundColor Gray
    Write-Host "    `$env:AWS_SECRET_ACCESS_KEY = 'tu-secret-access-key'" -ForegroundColor Gray
    Write-Host "    `$env:AWS_REGION = 'sa-east-1'" -ForegroundColor Gray
    Write-Host ""
    
    $configure = Read-Host "¿Quieres configurar AWS CLI ahora? (s/n)"
    if ($configure -eq "s" -or $configure -eq "S") {
        aws configure
    }
} else {
    Write-Host "✓ AWS CLI configurado" -ForegroundColor Green
    aws configure list
}

Write-Host ""
Write-Host "4. Verificando conexión con AWS..." -ForegroundColor Yellow
$identity = aws sts get-caller-identity --region sa-east-1 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Conexión exitosa con AWS" -ForegroundColor Green
    $identity | ConvertFrom-Json | Format-List
} else {
    Write-Host "⚠ No se pudo conectar a AWS" -ForegroundColor Yellow
    Write-Host "Verifica tus credenciales y permisos IAM" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "5. Inicializando Terraform..." -ForegroundColor Yellow
Push-Location "$PSScriptRoot\..\terraform"
if (Test-Path ".terraform") {
    Write-Host "✓ Terraform ya está inicializado" -ForegroundColor Green
} else {
    terraform init
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Terraform inicializado correctamente" -ForegroundColor Green
    } else {
        Write-Host "✗ Error al inicializar Terraform" -ForegroundColor Red
    }
}
Pop-Location

Write-Host ""
Write-Host "=== Resumen ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Para usar Terraform con AWS:" -ForegroundColor Yellow
Write-Host "1. Asegúrate de tener credenciales configuradas (AWS CLI o variables de entorno)" -ForegroundColor White
Write-Host "2. Ve a la carpeta terraform: cd terraform" -ForegroundColor White
Write-Host "3. Valida la configuración: terraform validate" -ForegroundColor White
Write-Host "4. Planifica cambios: terraform plan" -ForegroundColor White
Write-Host "5. Aplica cambios: terraform apply" -ForegroundColor White
Write-Host ""
Write-Host "Documentación completa en: terraform/README.md" -ForegroundColor Cyan

