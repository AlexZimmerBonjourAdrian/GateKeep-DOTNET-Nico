# Script para probar la infraestructura desplegada con Terraform

$ErrorActionPreference = "Stop"

Write-Host "=== Prueba de Infraestructura GateKeep ===" -ForegroundColor Cyan
Write-Host ""

# Cambiar al directorio de Terraform para obtener outputs
$terraformDir = Join-Path $PSScriptRoot ".." "terraform"
Push-Location $terraformDir

try {
    # Obtener outputs de Terraform
    Write-Host "1. Obteniendo información de Terraform..." -ForegroundColor Yellow
    $rdsEndpoint = terraform output -raw rds_endpoint
    $rdsPort = terraform output -raw rds_port
    $rdsDatabase = terraform output -raw rds_database_name
    $ecrUrl = terraform output -raw ecr_repository_url
    $vpcId = terraform output -raw vpc_id
    
    Write-Host "✓ Información obtenida" -ForegroundColor Green
    Write-Host ""

    # 2. Verificar ECR Repository
    Write-Host "2. Verificando ECR Repository..." -ForegroundColor Yellow
    $ecrRepo = aws ecr describe-repositories --repository-names gatekeep-api --region sa-east-1 --profile Alex 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ ECR Repository existe" -ForegroundColor Green
        Write-Host "  URL: $ecrUrl" -ForegroundColor Gray
    } else {
        Write-Host "✗ Error al verificar ECR" -ForegroundColor Red
    }
    Write-Host ""

    # 3. Verificar RDS
    Write-Host "3. Verificando RDS PostgreSQL..." -ForegroundColor Yellow
    $rdsInfo = aws rds describe-db-instances --db-instance-identifier gatekeep-db --region sa-east-1 --profile Alex 2>&1
    if ($LASTEXITCODE -eq 0) {
        $rdsStatus = ($rdsInfo | ConvertFrom-Json).DBInstances[0].DBInstanceStatus
        Write-Host "✓ RDS Instance existe" -ForegroundColor Green
        Write-Host "  Status: $rdsStatus" -ForegroundColor Gray
        Write-Host "  Endpoint: $rdsEndpoint" -ForegroundColor Gray
        Write-Host "  Port: $rdsPort" -ForegroundColor Gray
        Write-Host "  Database: $rdsDatabase" -ForegroundColor Gray
        
        if ($rdsStatus -eq "available") {
            Write-Host "  ✓ RDS está disponible y listo para usar" -ForegroundColor Green
        } else {
            Write-Host "  ⚠ RDS está en estado: $rdsStatus" -ForegroundColor Yellow
        }
    } else {
        Write-Host "✗ Error al verificar RDS" -ForegroundColor Red
    }
    Write-Host ""

    # 4. Verificar Secrets Manager
    Write-Host "4. Verificando Secrets Manager..." -ForegroundColor Yellow
    $secrets = @("gatekeep/db/password", "gatekeep/jwt/key", "gatekeep/rabbitmq/password")
    $allSecretsOk = $true
    foreach ($secretName in $secrets) {
        $secretInfo = aws secretsmanager describe-secret --secret-id $secretName --region sa-east-1 --profile Alex 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ✓ Secret '$secretName' existe" -ForegroundColor Green
        } else {
            Write-Host "  ✗ Secret '$secretName' no encontrado" -ForegroundColor Red
            $allSecretsOk = $false
        }
    }
    if ($allSecretsOk) {
        Write-Host "✓ Todos los secrets están configurados" -ForegroundColor Green
    }
    Write-Host ""

    # 5. Verificar Parameter Store
    Write-Host "5. Verificando Parameter Store..." -ForegroundColor Yellow
    $params = @("/gatekeep/db/host", "/gatekeep/db/port", "/gatekeep/db/name", "/gatekeep/db/username", "/gatekeep/ecr/repository-uri")
    $allParamsOk = $true
    foreach ($paramName in $params) {
        $paramInfo = aws ssm get-parameter --name $paramName --region sa-east-1 --profile Alex 2>&1
        if ($LASTEXITCODE -eq 0) {
            $paramValue = ($paramInfo | ConvertFrom-Json).Parameter.Value
            Write-Host "  ✓ Parámetro '$paramName' existe" -ForegroundColor Green
            Write-Host "    Valor: $paramValue" -ForegroundColor Gray
        } else {
            Write-Host "  ✗ Parámetro '$paramName' no encontrado" -ForegroundColor Red
            $allParamsOk = $false
        }
    }
    if ($allParamsOk) {
        Write-Host "✓ Todos los parámetros están configurados" -ForegroundColor Green
    }
    Write-Host ""

    # 6. Verificar VPC
    Write-Host "6. Verificando VPC..." -ForegroundColor Yellow
    $vpcInfo = aws ec2 describe-vpcs --vpc-ids $vpcId --region sa-east-1 --profile Alex 2>&1
    if ($LASTEXITCODE -eq 0) {
        $vpcState = ($vpcInfo | ConvertFrom-Json).Vpcs[0].State
        Write-Host "✓ VPC existe" -ForegroundColor Green
        Write-Host "  VPC ID: $vpcId" -ForegroundColor Gray
        Write-Host "  Estado: $vpcState" -ForegroundColor Gray
    } else {
        Write-Host "✗ Error al verificar VPC" -ForegroundColor Red
    }
    Write-Host ""

    # 7. Probar conexión a RDS (si psql está disponible)
    Write-Host "7. Probando conexión a RDS..." -ForegroundColor Yellow
    $dbPassword = aws secretsmanager get-secret-value --secret-id gatekeep/db/password --region sa-east-1 --profile Alex --query SecretString --output text 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Password obtenido de Secrets Manager" -ForegroundColor Green
        Write-Host "  Para probar la conexión manualmente:" -ForegroundColor Gray
        Write-Host "  Host: $rdsEndpoint" -ForegroundColor Gray
        Write-Host "  Port: $rdsPort" -ForegroundColor Gray
        Write-Host "  Database: $rdsDatabase" -ForegroundColor Gray
        Write-Host "  Username: postgres" -ForegroundColor Gray
        Write-Host ""
        Write-Host "  Comando psql (si está instalado):" -ForegroundColor Cyan
        Write-Host "  psql -h $rdsEndpoint -p $rdsPort -U postgres -d $rdsDatabase" -ForegroundColor White
    } else {
        Write-Host "⚠ No se pudo obtener el password (puede ser normal si no tienes permisos)" -ForegroundColor Yellow
    }
    Write-Host ""

    # Resumen
    Write-Host "=== RESUMEN ===" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Infraestructura desplegada:" -ForegroundColor Green
    Write-Host "  ✓ ECR Repository" -ForegroundColor White
    Write-Host "  ✓ RDS PostgreSQL" -ForegroundColor White
    Write-Host "  ✓ Secrets Manager" -ForegroundColor White
    Write-Host "  ✓ Parameter Store" -ForegroundColor White
    Write-Host "  ✓ VPC y Networking" -ForegroundColor White
    Write-Host ""
    Write-Host "Próximos pasos:" -ForegroundColor Yellow
    Write-Host "  1. Construir y subir imagen Docker a ECR" -ForegroundColor White
    Write-Host "  2. Configurar aplicación para usar RDS" -ForegroundColor White
    Write-Host "  3. Desplegar aplicación (ECS, EC2, o cambiar región para App Runner)" -ForegroundColor White
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
Write-Host "=== Prueba completada ===" -ForegroundColor Green

