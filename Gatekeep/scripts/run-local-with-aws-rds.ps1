# Script para ejecutar la aplicación localmente conectándose a RDS en AWS

Write-Host "=== Configurando aplicación local con RDS AWS ===" -ForegroundColor Cyan
Write-Host ""

# Obtener configuración desde AWS
Write-Host "1. Obteniendo configuración desde AWS..." -ForegroundColor Yellow

$dbHost = aws ssm get-parameter --name /gatekeep/db/host --region sa-east-1 --profile Alex --query "Parameter.Value" --output text
$dbPort = aws ssm get-parameter --name /gatekeep/db/port --region sa-east-1 --profile Alex --query "Parameter.Value" --output text
$dbName = aws ssm get-parameter --name /gatekeep/db/name --region sa-east-1 --profile Alex --query "Parameter.Value" --output text
$dbUsername = aws ssm get-parameter --name /gatekeep/db/username --region sa-east-1 --profile Alex --query "Parameter.Value" --output text
$dbPassword = aws secretsmanager get-secret-value --secret-id gatekeep/db/password --region sa-east-1 --profile Alex --query "SecretString" --output text
$jwtKey = aws secretsmanager get-secret-value --secret-id gatekeep/jwt/key --region sa-east-1 --profile Alex --query "SecretString" --output text

Write-Host "✓ Configuración obtenida" -ForegroundColor Green
Write-Host ""

# Configurar variables de entorno
Write-Host "2. Configurando variables de entorno..." -ForegroundColor Yellow

$env:DATABASE__HOST = $dbHost
$env:DATABASE__PORT = $dbPort
$env:DATABASE__NAME = $dbName
$env:DATABASE__USER = $dbUsername
$env:DATABASE__PASSWORD = $dbPassword
$env:JWT__KEY = $jwtKey
$env:JWT__ISSUER = "GateKeep"
$env:JWT__AUDIENCE = "GateKeepUsers"
$env:JWT__EXPIRATIONHOURS = "8"
$env:AWS_REGION = "sa-east-1"
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:GATEKEEP_PORT = "5011"

Write-Host "✓ Variables de entorno configuradas" -ForegroundColor Green
Write-Host ""

# Mostrar información
Write-Host "=== Configuración ===" -ForegroundColor Cyan
Write-Host "  RDS Host: $dbHost" -ForegroundColor White
Write-Host "  RDS Port: $dbPort" -ForegroundColor White
Write-Host "  Database: $dbName" -ForegroundColor White
Write-Host "  Username: $dbUsername" -ForegroundColor White
Write-Host ""
Write-Host "=== URLs de Acceso ===" -ForegroundColor Cyan
Write-Host "  API: http://localhost:5011" -ForegroundColor Green
Write-Host "  Swagger: http://localhost:5011/swagger" -ForegroundColor Green
Write-Host "  Health: http://localhost:5011/health" -ForegroundColor Green
Write-Host ""

# Ejecutar aplicación
Write-Host "3. Iniciando aplicación..." -ForegroundColor Yellow
Write-Host ""

Push-Location "src\GateKeep.Api"
dotnet run
Pop-Location

