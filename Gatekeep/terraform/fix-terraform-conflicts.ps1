# Script para resolver conflictos de Terraform
# Importa recursos existentes y ajusta la configuración

$ErrorActionPreference = "Continue"
$ProjectRoot = Split-Path -Parent $PSScriptRoot

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Solucionando Conflictos de Terraform"
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 1. Importar CloudFront Response Headers Policy
Write-Host "Importando políticas existentes..." -ForegroundColor Yellow

# Obtener el ID de la política existente
$policyOutput = aws cloudfront list-response-headers-policies --query "ResponseHeadersPoliciesList[?Name=='gatekeep-frontend-headers'].Id" --output text 2>&1

if ($policyOutput -and $policyOutput -ne "None" -and $policyOutput -ne "") {
    Write-Host "  Encontrada Response Headers Policy: $policyOutput" -ForegroundColor Green
    $importCmd = "terraform import aws_cloudfront_response_headers_policy.frontend $policyOutput"
    Write-Host "  Ejecutando: $importCmd"
    Invoke-Expression $importCmd
}

# 2. Importar CloudFront Cache Policy
$cachePolicy = aws cloudfront list-cache-policies --query "CachePoliciesList[?CachePolicyConfig.Name=='gatekeep-frontend-static-cache'].Id" --output text 2>&1

if ($cachePolicy -and $cachePolicy -ne "None" -and $cachePolicy -ne "") {
    Write-Host "  Encontrada Cache Policy: $cachePolicy" -ForegroundColor Green
    $importCmd = "terraform import aws_cloudfront_cache_policy.frontend_static $cachePolicy"
    Write-Host "  Ejecutando: $importCmd"
    Invoke-Expression $importCmd
}

# 3. Importar S3 Bucket
Write-Host "  Importando S3 Bucket..." -ForegroundColor Green
$importCmd = "terraform import aws_s3_bucket.frontend gatekeep-frontend-dev"
Write-Host "  Ejecutando: $importCmd"
Invoke-Expression $importCmd

# 4. Importar S3 Bucket versioning
Write-Host "  Importando S3 Bucket Versioning..." -ForegroundColor Green
$importCmd = "terraform import aws_s3_bucket_versioning.frontend gatekeep-frontend-dev"
Write-Host "  Ejecutando: $importCmd"
Invoke-Expression $importCmd

# 5. Importar S3 Bucket CORS
Write-Host "  Importando S3 Bucket CORS..." -ForegroundColor Green
$importCmd = "terraform import aws_s3_bucket_cors_configuration.frontend gatekeep-frontend-dev"
Write-Host "  Ejecutando: $importCmd"
Invoke-Expression $importCmd

# 6. Importar S3 Public Access Block
Write-Host "  Importando S3 Public Access Block..." -ForegroundColor Green
$importCmd = "terraform import aws_s3_bucket_public_access_block.frontend gatekeep-frontend-dev"
Write-Host "  Ejecutando: $importCmd"
Invoke-Expression $importCmd

# 7. El HTTPS listener ya existe, importarlo
Write-Host ""
Write-Host "Importando HTTPS Listener..." -ForegroundColor Yellow
$lbArn = "arn:aws:elasticloadbalancing:sa-east-1:126588786097:loadbalancer/app/gatekeep-alb/ff82ae699b9862d2"
$listenerId = "arn:aws:elasticloadbalancing:sa-east-1:126588786097:listener/app/gatekeep-alb/ff82ae699b9862d2/11a4db9a8c84bc13"

if ($listenerId) {
    $importCmd = "terraform import `"aws_lb_listener.https[0]`" $listenerId"
    Write-Host "  Ejecutando: $importCmd"
    Invoke-Expression $importCmd
}

Write-Host ""
Write-Host "✓ Importación completada" -ForegroundColor Green
Write-Host ""
Write-Host "Próximos pasos:" -ForegroundColor Cyan
Write-Host "  1. Ejecuta: terraform plan"
Write-Host "  2. Revisa los cambios propuestos"
Write-Host "  3. Ejecuta: terraform apply"
Write-Host ""
