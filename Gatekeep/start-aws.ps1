# Script para iniciar servicios en AWS
# Aplica la infraestructura con Terraform

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  GateKeep - Iniciar Servicios AWS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Función para verificar AWS CLI
function Test-AwsCli {
    $awsCli = Get-Command aws -ErrorAction SilentlyContinue
    if (-not $awsCli) {
        Write-Host "Error: AWS CLI no está instalado" -ForegroundColor Red
        Write-Host "Instala desde: https://aws.amazon.com/cli/" -ForegroundColor Yellow
        return $false
    }
    
    Write-Host "AWS CLI instalado" -ForegroundColor Green
    
    try {
        $awsVersion = aws --version
        Write-Host "  Versión: $awsVersion" -ForegroundColor Gray
    } catch {
        Write-Host "Error al verificar versión de AWS CLI" -ForegroundColor Red
        return $false
    }
    
    # Verificar configuración
    try {
        $awsIdentity = aws sts get-caller-identity 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "AWS CLI configurado correctamente" -ForegroundColor Green
            return $true
        } else {
            Write-Host "Error: AWS CLI no está configurado" -ForegroundColor Red
            Write-Host "Ejecuta: aws configure" -ForegroundColor Yellow
            return $false
        }
    } catch {
        Write-Host "Error: AWS CLI no está configurado" -ForegroundColor Red
        Write-Host "Ejecuta: aws configure" -ForegroundColor Yellow
        return $false
    }
}

# Función para verificar Terraform
function Test-Terraform {
    $terraform = Get-Command terraform -ErrorAction SilentlyContinue
    if (-not $terraform) {
        Write-Host "Error: Terraform no está instalado" -ForegroundColor Red
        Write-Host "Instala desde: https://www.terraform.io/downloads" -ForegroundColor Yellow
        return $false
    }
    
    Write-Host "Terraform instalado" -ForegroundColor Green
    
    try {
        $tfVersion = terraform version
        Write-Host "  $($tfVersion.Split("`n")[0])" -ForegroundColor Gray
    } catch {
        Write-Host "Error al verificar versión de Terraform" -ForegroundColor Red
        return $false
    }
    
    return $true
}

# Verificar requisitos
Write-Host "Verificando requisitos..." -ForegroundColor Yellow
Write-Host ""

$awsOk = Test-AwsCli
Write-Host ""

if (-not $awsOk) {
    exit 1
}

$terraformOk = Test-Terraform
Write-Host ""

if (-not $terraformOk) {
    exit 1
}

# Navegar al directorio de Terraform
$terraformPath = Join-Path $PSScriptRoot "terraform"

if (-not (Test-Path $terraformPath)) {
    Write-Host "Error: No se encontró el directorio terraform" -ForegroundColor Red
    Write-Host "Ruta buscada: $terraformPath" -ForegroundColor Gray
    exit 1
}

Set-Location $terraformPath

# Verificar archivo terraform.tfvars
$tfvarsPath = Join-Path $terraformPath "terraform.tfvars"
if (-not (Test-Path $tfvarsPath)) {
    Write-Host "Advertencia: No se encontró terraform.tfvars" -ForegroundColor Yellow
    $tfvarsExample = Join-Path $terraformPath "terraform.tfvars.example"
    if (Test-Path $tfvarsExample) {
        Write-Host "Copia terraform.tfvars.example a terraform.tfvars y configura tus valores" -ForegroundColor Yellow
    }
    Write-Host ""
}

# Inicializar Terraform (si es necesario)
Write-Host "Inicializando Terraform..." -ForegroundColor Yellow
terraform init
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Falló la inicialización de Terraform" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Aplicar infraestructura
Write-Host "Aplicando infraestructura..." -ForegroundColor Yellow
Write-Host "Esto puede tardar varios minutos..." -ForegroundColor Gray
Write-Host ""

terraform apply

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Error: Falló la aplicación de la infraestructura" -ForegroundColor Red
    exit 1
}

# Mostrar outputs
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Infraestructura desplegada" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

Write-Host "Obteniendo URLs de los servicios..." -ForegroundColor Cyan
Write-Host ""

terraform output

Write-Host ""
Write-Host "Para ver los outputs completos:" -ForegroundColor Cyan
Write-Host "  cd terraform && terraform output" -ForegroundColor White
Write-Host ""

