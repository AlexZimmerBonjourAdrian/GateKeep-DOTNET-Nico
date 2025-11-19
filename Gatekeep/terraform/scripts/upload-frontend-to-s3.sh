#!/bin/bash
# Script para subir el build del frontend a S3 y invalidar CloudFront
# Uso: ./upload-frontend-to-s3.sh <bucket-name> <cloudfront-distribution-id>

set -e

BUCKET_NAME="${1:-gatekeep-frontend-dev}"
DISTRIBUTION_ID="${2}"

if [ -z "$BUCKET_NAME" ]; then
    echo "❌ Error: Se requiere el nombre del bucket S3"
    echo "Uso: $0 <bucket-name> [cloudfront-distribution-id]"
    exit 1
fi

echo "📦 Subiendo build del frontend a S3: $BUCKET_NAME"

# Verificar que existe el directorio .next/out o out/ (build estático de Next.js)
BUILD_DIR=""
if [ -d "../frontend/.next/out" ]; then
    BUILD_DIR="../frontend/.next/out"
elif [ -d "../frontend/out" ]; then
    BUILD_DIR="../frontend/out"
elif [ -d "../frontend/dist" ]; then
    BUILD_DIR="../frontend/dist"
else
    echo "❌ Error: No se encontró el directorio de build. Ejecuta 'npm run build' primero."
    exit 1
fi

echo "📂 Directorio de build encontrado: $BUILD_DIR"

# Subir todos los archivos a S3
# --exclude y --include para optimizar
aws s3 sync "$BUILD_DIR" "s3://$BUCKET_NAME" \
    --delete \
    --exclude "*.map" \
    --cache-control "public, max-age=31536000, immutable" \
    --exclude "*.html" \
    --include "*.html" \
    --cache-control "public, max-age=0, must-revalidate" \
    --include "*.wasm" \
    --cache-control "public, max-age=31536000, immutable" \
    --content-type "application/wasm" \
    --include "sw.js" \
    --cache-control "public, max-age=0, must-revalidate" \
    --include "offline.html" \
    --cache-control "public, max-age=0, must-revalidate" \
    --metadata-directive REPLACE

# Configurar Content-Type específico para archivos WASM
echo "🔧 Configurando Content-Type para archivos WASM..."
aws s3 cp "$BUILD_DIR" "s3://$BUCKET_NAME" \
    --recursive \
    --exclude "*" \
    --include "*.wasm" \
    --content-type "application/wasm" \
    --metadata-directive REPLACE

# Configurar Content-Type para Service Worker
echo "🔧 Configurando Content-Type para Service Worker..."
aws s3 cp "$BUILD_DIR/sw.js" "s3://$BUCKET_NAME/sw.js" \
    --content-type "application/javascript" \
    --cache-control "public, max-age=0, must-revalidate" \
    --metadata-directive REPLACE 2>/dev/null || echo "⚠️ sw.js no encontrado (puede estar en public/)"

# Invalidar CloudFront si se proporcionó el Distribution ID
if [ -n "$DISTRIBUTION_ID" ]; then
    echo "🔄 Invalidando caché de CloudFront: $DISTRIBUTION_ID"
    INVALIDATION_ID=$(aws cloudfront create-invalidation \
        --distribution-id "$DISTRIBUTION_ID" \
        --paths "/*" \
        --query 'Invalidation.Id' \
        --output text)
    
    echo "✅ Invalidación creada: $INVALIDATION_ID"
    echo "⏳ Esperando a que la invalidación se complete..."
    aws cloudfront wait invalidation-completed \
        --distribution-id "$DISTRIBUTION_ID" \
        --id "$INVALIDATION_ID"
    echo "✅ Invalidación completada"
else
    echo "⚠️ No se proporcionó Distribution ID. Obtén el ID con:"
    echo "   terraform output cloudfront_distribution_id"
    echo "   Luego invalida manualmente con:"
    echo "   aws cloudfront create-invalidation --distribution-id <ID> --paths '/*'"
fi

echo "✅ Frontend subido exitosamente a S3: $BUCKET_NAME"

