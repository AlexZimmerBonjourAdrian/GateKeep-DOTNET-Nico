#!/usr/bin/env pwsh
# Script para importar recursos existentes en AWS al estado de Terraform

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Importando Recursos a Terraform State"
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 1. Importar S3 Bucket
Write-Host "1. Importando S3 Bucket..." -ForegroundColor Yellow
try {
    terraform import aws_s3_bucket.frontend gatekeep-frontend-dev
    Write-Host "   OK - S3 Bucket importado" -ForegroundColor Green
} catch {
    Write-Host "   Error: $_" -ForegroundColor Red
}

# 2. Importar S3 Versioning
Write-Host "2. Importando S3 Versioning..." -ForegroundColor Yellow
try {
    terraform import aws_s3_bucket_versioning.frontend gatekeep-frontend-dev
    Write-Host "   OK - S3 Versioning importado" -ForegroundColor Green
} catch {
    Write-Host "   Error: $_" -ForegroundColor Red
}

# 3. Importar S3 CORS
Write-Host "3. Importando S3 CORS Configuration..." -ForegroundColor Yellow
try {
    terraform import aws_s3_bucket_cors_configuration.frontend gatekeep-frontend-dev
    Write-Host "   OK - S3 CORS importado" -ForegroundColor Green
} catch {
    Write-Host "   Error: $_" -ForegroundColor Red
}

# 4. Importar S3 Public Access Block
Write-Host "4. Importando S3 Public Access Block..." -ForegroundColor Yellow
try {
    terraform import aws_s3_bucket_public_access_block.frontend gatekeep-frontend-dev
    Write-Host "   OK - S3 Public Access Block importado" -ForegroundColor Green
} catch {
    Write-Host "   Error: $_" -ForegroundColor Red
}

# 5. Obtener IDs de CloudFront Policies
Write-Host ""
Write-Host "5. Buscando CloudFront Policies existentes..." -ForegroundColor Yellow

$responsePolicy = (aws cloudfront list-response-headers-policies --query "ResponseHeadersPoliciesList[?Name=='gatekeep-frontend-headers'].Id" --output text 2>&1)
$cachePolicy = (aws cloudfront list-cache-policies --query "CachePoliciesList[?CachePolicyConfig.Name=='gatekeep-frontend-static-cache'].Id" --output text 2>&1)

if ($responsePolicy -and $responsePolicy -ne "None" -and $responsePolicy -ne "") {
    Write-Host "   Encontrada Response Headers Policy: $responsePolicy" -ForegroundColor Green
    try {
        terraform import aws_cloudfront_response_headers_policy.frontend $responsePolicy
        Write-Host "   OK - Response Headers Policy importada" -ForegroundColor Green
    } catch {
        Write-Host "   Error: $_" -ForegroundColor Red
    }
}

if ($cachePolicy -and $cachePolicy -ne "None" -and $cachePolicy -ne "") {
    Write-Host "   Encontrada Cache Policy: $cachePolicy" -ForegroundColor Green
    try {
        terraform import aws_cloudfront_cache_policy.frontend_static $cachePolicy
        Write-Host "   OK - Cache Policy importada" -ForegroundColor Green
    } catch {
        Write-Host "   Error: $_" -ForegroundColor Red
    }
}

# 6. Importar HTTPS Listener
Write-Host ""
Write-Host "6. Importando HTTPS Listener..." -ForegroundColor Yellow
$listenerArn = "arn:aws:elasticloadbalancing:sa-east-1:126588786097:listener/app/gatekeep-alb/ff82ae699b9862d2/11a4db9a8c84bc13"
try {
    terraform import 'aws_lb_listener.https[0]' $listenerArn
    Write-Host "   OK - HTTPS Listener importado" -ForegroundColor Green
} catch {
    Write-Host "   Error: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Importacion Completada" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Proximos pasos:" -ForegroundColor Cyan
Write-Host "  1. terraform plan -out=tfplan"
Write-Host "  2. Revisa los cambios propuestos"
Write-Host "  3. terraform apply tfplan"
Write-Host ""
