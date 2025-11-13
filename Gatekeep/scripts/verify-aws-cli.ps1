# Script de Verificación de AWS CLI
# Verifica que AWS CLI esté instalado y configurado correctamente

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Verificación de AWS CLI" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$errors = 0

# Verificar instalación
Write-Host "1. Verificando instalación de AWS CLI..." -ForegroundColor Yellow
try {
    $version = aws --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   [OK] AWS CLI instalado: $version" -ForegroundColor Green
    } else {
        Write-Host "   [ERROR] AWS CLI no está instalado" -ForegroundColor Red
        Write-Host "   Instalar con: winget install Amazon.AWSCLI" -ForegroundColor Yellow
        $errors++
    }
} catch {
    Write-Host "   [ERROR] AWS CLI no está instalado" -ForegroundColor Red
    Write-Host "   Instalar con: winget install Amazon.AWSCLI" -ForegroundColor Yellow
    $errors++
}

Write-Host ""

# Verificar configuración
Write-Host "2. Verificando configuración..." -ForegroundColor Yellow
try {
    $config = aws configure list 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   [OK] Configuración encontrada:" -ForegroundColor Green
        $config | ForEach-Object {
            Write-Host "   $_" -ForegroundColor White
        }
        
        # Verificar que tiene access_key configurado
        if ($config -match "access_key.*\*{4,}") {
            Write-Host "   [OK] Access Key configurado" -ForegroundColor Green
        } else {
            Write-Host "   [ADVERTENCIA] Access Key no configurado" -ForegroundColor Yellow
            Write-Host "   Configurar con: aws configure" -ForegroundColor Yellow
        }
        
        # Verificar región
        if ($config -match "region.*us-east-1") {
            Write-Host "   [OK] Región configurada: us-east-1" -ForegroundColor Green
        } else {
            Write-Host "   [ADVERTENCIA] Región no configurada o diferente" -ForegroundColor Yellow
            Write-Host "   Configurar con: aws configure set region us-east-1" -ForegroundColor Yellow
        }
    } else {
        Write-Host "   [ERROR] No se pudo leer configuración" -ForegroundColor Red
        $errors++
    }
} catch {
    Write-Host "   [ERROR] Error al verificar configuración" -ForegroundColor Red
    $errors++
}

Write-Host ""

# Verificar conexión
Write-Host "3. Verificando conexión con AWS..." -ForegroundColor Yellow
try {
    $identity = aws sts get-caller-identity 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   [OK] Conexión exitosa:" -ForegroundColor Green
        $identity | ConvertFrom-Json | ForEach-Object {
            Write-Host "   Account ID: $($_.Account)" -ForegroundColor White
            Write-Host "   User ARN: $($_.Arn)" -ForegroundColor White
        }
    } else {
        Write-Host "   [ERROR] No se pudo conectar a AWS" -ForegroundColor Red
        Write-Host "   Verificar credenciales con: aws configure" -ForegroundColor Yellow
        Write-Host "   Error: $identity" -ForegroundColor Red
        $errors++
    }
} catch {
    Write-Host "   [ERROR] Error al verificar conexión" -ForegroundColor Red
    $errors++
}

Write-Host ""

# Verificar permisos básicos
Write-Host "4. Verificando permisos básicos..." -ForegroundColor Yellow
try {
    # Verificar acceso a ECR
    $ecr = aws ecr describe-repositories --region us-east-1 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   [OK] Permisos ECR: OK" -ForegroundColor Green
    } else {
        Write-Host "   [ADVERTENCIA] No se pudo acceder a ECR" -ForegroundColor Yellow
        Write-Host "   Verificar políticas IAM: AmazonEC2ContainerRegistryFullAccess" -ForegroundColor Yellow
    }
    
    # Verificar acceso a IAM
    $iam = aws iam get-user 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   [OK] Permisos IAM: OK" -ForegroundColor Green
    } else {
        Write-Host "   [ADVERTENCIA] No se pudo acceder a IAM" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   [ADVERTENCIA] Error al verificar permisos" -ForegroundColor Yellow
}

Write-Host ""

# Resumen
Write-Host "========================================" -ForegroundColor Cyan
if ($errors -eq 0) {
    Write-Host "  [OK] AWS CLI está listo para usar" -ForegroundColor Green
    Write-Host ""
    Write-Host "Próximos pasos:" -ForegroundColor Cyan
    Write-Host "  1. Crear recursos AWS (ECR, RDS, etc.)" -ForegroundColor White
    Write-Host "  2. Configurar GitHub Secrets" -ForegroundColor White
    Write-Host "  3. Seguir guía: docs/DEPLOYMENT.md" -ForegroundColor White
} else {
    Write-Host "  [ERROR] Hay $errors error(es) que resolver" -ForegroundColor Red
    Write-Host ""
    Write-Host "Solución:" -ForegroundColor Yellow
    Write-Host "  1. Instalar AWS CLI si falta" -ForegroundColor White
    Write-Host "  2. Configurar con: aws configure" -ForegroundColor White
    Write-Host "  3. Verificar credenciales IAM" -ForegroundColor White
}
Write-Host "========================================" -ForegroundColor Cyan

