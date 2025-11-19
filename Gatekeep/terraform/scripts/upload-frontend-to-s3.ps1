# Script PowerShell para subir el build del frontend a S3 y invalidar CloudFront
# Uso: .\upload-frontend-to-s3.ps1 -BucketName <bucket-name> [-DistributionId <cloudfront-distribution-id>]

param(
    [Parameter(Mandatory=$true)]
    [string]$BucketName,
    
    [Parameter(Mandatory=$false)]
    [string]$DistributionId
)

$ErrorActionPreference = "Stop"

Write-Host "ðŸ“¦ Subiendo build del frontend a S3: $BucketName" -ForegroundColor Cyan

# Verificar que existe el directorio de build
$BuildDir = $null
$FrontendPath = Join-Path $PSScriptRoot "..\..\frontend"

if (Test-Path (Join-Path $FrontendPath ".next\out")) {
    $BuildDir = Join-Path $FrontendPath ".next\out"
} elseif (Test-Path (Join-Path $FrontendPath "out")) {
    $BuildDir = Join-Path $FrontendPath "out"
} elseif (Test-Path (Join-Path $FrontendPath "dist")) {
    $BuildDir = Join-Path $FrontendPath "dist"
} else {
    Write-Host "âŒ Error: No se encontrÃ³ el directorio de build. Ejecuta 'npm run build' primero." -ForegroundColor Red
    exit 1
}

Write-Host "ðŸ“‚ Directorio de build encontrado: $BuildDir" -ForegroundColor Green

# Subir archivos HTML con cache-control especÃ­fico
Write-Host "ðŸ“¤ Subiendo archivos HTML..." -ForegroundColor Yellow
Get-ChildItem -Path $BuildDir -Filter "*.html" -Recurse | ForEach-Object {
    $relativePath = $_.FullName.Substring($BuildDir.Length + 1).Replace("\", "/")
    $s3Key = $relativePath
    Write-Host "  â†’ $s3Key"
    aws s3 cp $_.FullName "s3://$BucketName/$s3Key" `
        --content-type "text/html" `
        --cache-control "public, max-age=0, must-revalidate" `
        --metadata-directive REPLACE
}

# Subir archivos WASM con Content-Type correcto
Write-Host "ðŸ“¤ Subiendo archivos WASM..." -ForegroundColor Yellow
Get-ChildItem -Path $BuildDir -Filter "*.wasm" -Recurse | ForEach-Object {
    $relativePath = $_.FullName.Substring($BuildDir.Length + 1).Replace("\", "/")
    $s3Key = $relativePath
    Write-Host "  â†’ $s3Key (application/wasm)"
    aws s3 cp $_.FullName "s3://$BucketName/$s3Key" `
        --content-type "application/wasm" `
        --cache-control "public, max-age=31536000, immutable" `
        --metadata-directive REPLACE
}

# Subir Service Worker
Write-Host "ðŸ“¤ Subiendo Service Worker..." -ForegroundColor Yellow
$swPath = Join-Path $BuildDir "sw.js"
if (Test-Path $swPath) {
    aws s3 cp $swPath "s3://$BucketName/sw.js" `
        --content-type "application/javascript" `
        --cache-control "public, max-age=0, must-revalidate" `
        --metadata-directive REPLACE
    Write-Host "  â†’ sw.js" -ForegroundColor Green
} else {
    Write-Host "  âš ï¸ sw.js no encontrado (puede estar en public/)" -ForegroundColor Yellow
}

# Subir resto de archivos estÃ¡ticos
Write-Host "ðŸ“¤ Subiendo resto de archivos estÃ¡ticos..." -ForegroundColor Yellow
aws s3 sync $BuildDir "s3://$BucketName" `
    --delete `
    --exclude "*.html" `
    --exclude "*.wasm" `
    --exclude "sw.js" `
    --cache-control "public, max-age=31536000, immutable"

# Invalidar CloudFront si se proporcionÃ³ el Distribution ID
if ($DistributionId) {
    Write-Host "ðŸ”„ Invalidando cachÃ© de CloudFront: $DistributionId" -ForegroundColor Cyan
    $InvalidationId = aws cloudfront create-invalidation `
        --distribution-id $DistributionId `
        --paths "/*" `
        --query 'Invalidation.Id' `
        --output text
    
    Write-Host "âœ… InvalidaciÃ³n creada: $InvalidationId" -ForegroundColor Green
    Write-Host "â³ Esperando a que la invalidaciÃ³n se complete..." -ForegroundColor Yellow
    
    $maxWait = 300 # 5 minutos mÃ¡ximo
    $elapsed = 0
    do {
        Start-Sleep -Seconds 5
        $elapsed += 5
        $status = aws cloudfront get-invalidation `
            --distribution-id $DistributionId `
            --id $InvalidationId `
            --query 'Invalidation.Status' `
            --output text
        
        if ($status -eq "Completed") {
            Write-Host "âœ… InvalidaciÃ³n completada" -ForegroundColor Green
            break
        }
        
        if ($elapsed -ge $maxWait) {
            Write-Host "âš ï¸ Timeout esperando invalidaciÃ³n. Verifica manualmente." -ForegroundColor Yellow
            break
        }
    } while ($true)
} else {
    Write-Host "âš ï¸ No se proporcionÃ³ Distribution ID. ObtÃ©n el ID con:" -ForegroundColor Yellow
    Write-Host "   terraform output cloudfront_distribution_id" -ForegroundColor Gray
    Write-Host "   Luego invalida manualmente con:" -ForegroundColor Gray
    Write-Host "   aws cloudfront create-invalidation --distribution-id [ID] --paths '/*'" -ForegroundColor Gray
}

Write-Host "Frontend subido exitosamente a S3: $BucketName" -ForegroundColor Green


