# Script de Guía para Setup Inicial de AWS
# Este script NO ejecuta comandos, solo muestra instrucciones

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Guía de Setup AWS para GateKeep" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Este script te guiará paso a paso para configurar AWS." -ForegroundColor Yellow
Write-Host ""

Write-Host "PASO 1: Crear ECR Repositories" -ForegroundColor Green
Write-Host "  1. Ir a: https://console.aws.amazon.com/ecr" -ForegroundColor White
Write-Host "  2. Click en 'Create repository'" -ForegroundColor White
Write-Host "  3. Crear dos repositorios:" -ForegroundColor White
Write-Host "     - gatekeep-api (Private)" -ForegroundColor White
Write-Host "     - gatekeep-frontend (Private)" -ForegroundColor White
Write-Host "  4. Anotar las URIs (ej: 123456789.dkr.ecr.us-east-1.amazonaws.com/gatekeep-api)" -ForegroundColor White
Write-Host ""

Write-Host "PASO 2: Crear RDS PostgreSQL" -ForegroundColor Green
Write-Host "  1. Ir a: https://console.aws.amazon.com/rds" -ForegroundColor White
Write-Host "  2. Click en 'Create database'" -ForegroundColor White
Write-Host "  3. Configuración:" -ForegroundColor White
Write-Host "     - Engine: PostgreSQL 16" -ForegroundColor White
Write-Host "     - Template: Free tier (si aplica) o Dev/Test" -ForegroundColor White
Write-Host "     - DB instance identifier: gatekeep-db" -ForegroundColor White
Write-Host "     - Master username: postgres" -ForegroundColor White
Write-Host "     - Master password: [GENERAR Y GUARDAR]" -ForegroundColor White
Write-Host "     - DB instance class: db.t4g.micro" -ForegroundColor White
Write-Host "     - Public access: SÍ" -ForegroundColor White
Write-Host "     - Database name: Gatekeep" -ForegroundColor White
Write-Host "  4. Anotar el endpoint de RDS" -ForegroundColor White
Write-Host ""

Write-Host "PASO 3: Configurar Secrets Manager" -ForegroundColor Green
Write-Host "  1. Ir a: https://console.aws.amazon.com/secretsmanager" -ForegroundColor White
Write-Host "  2. Click en 'Store a new secret'" -ForegroundColor White
Write-Host "  3. Crear secret para DB password:" -ForegroundColor White
Write-Host "     - Secret type: Other type of secret" -ForegroundColor White
Write-Host "     - Key: password" -ForegroundColor White
Write-Host "     - Value: [password de RDS]" -ForegroundColor White
Write-Host "     - Secret name: gatekeep/db/password" -ForegroundColor White
Write-Host "  4. Crear secret para JWT key:" -ForegroundColor White
Write-Host "     - Secret name: gatekeep/jwt/key" -ForegroundColor White
Write-Host "     - Key: key" -ForegroundColor White
Write-Host "     - Value: [generar clave JWT de 256 bits]" -ForegroundColor White
Write-Host ""

Write-Host "PASO 4: Configurar Parameter Store" -ForegroundColor Green
Write-Host "  1. Ir a: https://console.aws.amazon.com/systems-manager/parameters" -ForegroundColor White
Write-Host "  2. Click en 'Create parameter' para cada uno:" -ForegroundColor White
Write-Host "     - /gatekeep/db/host = [RDS endpoint]" -ForegroundColor White
Write-Host "     - /gatekeep/db/port = 5432" -ForegroundColor White
Write-Host "     - /gatekeep/db/name = Gatekeep" -ForegroundColor White
Write-Host "     - /gatekeep/db/username = postgres" -ForegroundColor White
Write-Host "     - /gatekeep/app/environment = Production" -ForegroundColor White
Write-Host "     - /gatekeep/app/port = 5011" -ForegroundColor White
Write-Host ""

Write-Host "PASO 5: Crear App Runner Services" -ForegroundColor Green
Write-Host "  1. Ir a: https://console.aws.amazon.com/apprunner" -ForegroundColor White
Write-Host "  2. Click en 'Create service'" -ForegroundColor White
Write-Host "  3. Para API:" -ForegroundColor White
Write-Host "     - Source: Container registry > Amazon ECR" -ForegroundColor White
Write-Host "     - Container image: gatekeep-api:latest" -ForegroundColor White
Write-Host "     - Service name: gatekeep-api" -ForegroundColor White
Write-Host "     - CPU: 1 vCPU, Memory: 2 GB" -ForegroundColor White
Write-Host "     - Port: 5011" -ForegroundColor White
Write-Host "     - Health check: /health" -ForegroundColor White
Write-Host "  4. Para Frontend:" -ForegroundColor White
Write-Host "     - Similar pero: gatekeep-frontend, 0.5 vCPU, 1 GB, Port: 3000" -ForegroundColor White
Write-Host ""

Write-Host "PASO 6: Configurar GitHub Secrets" -ForegroundColor Green
Write-Host "  1. Ir a tu repositorio en GitHub" -ForegroundColor White
Write-Host "  2. Settings > Secrets and variables > Actions" -ForegroundColor White
Write-Host "  3. Agregar:" -ForegroundColor White
Write-Host "     - AWS_ACCESS_KEY_ID" -ForegroundColor White
Write-Host "     - AWS_SECRET_ACCESS_KEY" -ForegroundColor White
Write-Host ""

Write-Host "Para más detalles, consulta: docs/PLAN_DESPLIEGUE_AUTOMATIZACION.md" -ForegroundColor Cyan
Write-Host ""

