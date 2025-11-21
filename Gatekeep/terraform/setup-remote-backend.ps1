#!/usr/bin/env pwsh
# Script para crear el backend remoto en S3 + DynamoDB
# Ejecuta esto UNA SOLA VEZ antes de inicializar terraform

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Configurando Backend Remoto en S3"
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$bucket = "gatekeep-terraform-state"
$dynamoTable = "gatekeep-terraform-locks"
$region = "sa-east-1"

# 1. Crear S3 Bucket para el state
Write-Host "1. Creando S3 Bucket para Terraform state..." -ForegroundColor Yellow
try {
    $bucketExists = aws s3api head-bucket --bucket $bucket --region $region 2>&1
    if ($bucketExists) {
        Write-Host "   ✓ Bucket '$bucket' ya existe" -ForegroundColor Green
    }
} catch {
    Write-Host "   Creando nuevo bucket..." -ForegroundColor Cyan
    aws s3api create-bucket `
        --bucket $bucket `
        --region $region `
        --create-bucket-configuration LocationConstraint=$region 2>&1 | Out-Null
    Write-Host "   ✓ Bucket creado" -ForegroundColor Green
}

# 2. Habilitar versionado
Write-Host "2. Habilitando versionado del bucket..." -ForegroundColor Yellow
aws s3api put-bucket-versioning `
    --bucket $bucket `
    --versioning-configuration Status=Enabled 2>&1 | Out-Null
Write-Host "   ✓ Versionado habilitado" -ForegroundColor Green

# 3. Bloquear acceso público
Write-Host "3. Bloqueando acceso público al bucket..." -ForegroundColor Yellow
aws s3api put-public-access-block `
    --bucket $bucket `
    --public-access-block-configuration `
    "BlockPublicAcls=true,IgnorePublicAcls=true,BlockPublicPolicy=true,RestrictPublicBuckets=true" 2>&1 | Out-Null
Write-Host "   ✓ Acceso público bloqueado" -ForegroundColor Green

# 4. Crear tabla DynamoDB para locks
Write-Host "4. Creando tabla DynamoDB para state locks..." -ForegroundColor Yellow
try {
    $tableExists = aws dynamodb describe-table --table-name $dynamoTable --region $region 2>&1
    if ($tableExists) {
        Write-Host "   ✓ Tabla '$dynamoTable' ya existe" -ForegroundColor Green
    }
} catch {
    Write-Host "   Creando nueva tabla..." -ForegroundColor Cyan
    aws dynamodb create-table `
        --table-name $dynamoTable `
        --attribute-definitions AttributeName=LockID,AttributeType=S `
        --key-schema AttributeName=LockID,KeyType=HASH `
        --billing-mode PAY_PER_REQUEST `
        --region $region 2>&1 | Out-Null
    
    Write-Host "   Esperando a que la tabla esté lista..." -ForegroundColor Cyan
    aws dynamodb wait table-exists `
        --table-name $dynamoTable `
        --region $region
    Write-Host "   ✓ Tabla creada y lista" -ForegroundColor Green
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Backend Remoto Configurado" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Proximos pasos:" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Ejecuta en el directorio terraform:" -ForegroundColor White
Write-Host "   terraform init" -ForegroundColor Cyan
Write-Host ""
Write-Host "   Terraform te preguntara si quieres migrar el state local"
Write-Host "   Responde 'yes' para migrar todo a S3" -ForegroundColor Yellow
Write-Host ""
Write-Host "2. Verifica que funciono:" -ForegroundColor White
Write-Host "   terraform state list" -ForegroundColor Cyan
Write-Host ""
Write-Host "3. Comunica a tu equipo que pueden hacer:" -ForegroundColor White
Write-Host "   rm -rf .terraform" -ForegroundColor Cyan
Write-Host "   rm -f terraform.tfstate*" -ForegroundColor Cyan
Write-Host "   terraform init" -ForegroundColor Cyan
Write-Host ""
Write-Host "Beneficios:" -ForegroundColor Green
Write-Host "  - Todos ven el mismo state" -ForegroundColor White
Write-Host "  - Locks automáticos previenen conflictos" -ForegroundColor White
Write-Host "  - Versionado automático del state" -ForegroundColor White
Write-Host "  - Compatible con CI/CD" -ForegroundColor White
Write-Host ""
