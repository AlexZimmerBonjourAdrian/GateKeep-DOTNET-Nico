# Script PowerShell para importar recursos existentes de AWS al estado de Terraform
# Importa recursos que ya existen para evitar errores "already exists"

# Ejecutar con: cd terraform; .\import-resources.ps1

$ErrorActionPreference = "Continue"
$WarningPreference = "SilentlyContinue"

Write-Host ""
Write-Host "===== Importando Recursos Existentes a Terraform =====" -ForegroundColor Cyan
Write-Host ""

$imported = @()
$failed = @()

# Funcion auxiliar para importar
function Try-ImportResource {
    param(
        [string]$ResourceType,
        [string]$ResourceId,
        [string]$TerraformName,
        [string]$Description
    )
    
    Write-Host "Importando: $Description..." -NoNewline
    
    $output = terraform import "$ResourceType.$TerraformName" "$ResourceId" 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host " [OK]" -ForegroundColor Green
        $imported += "$ResourceType.$TerraformName"
        return $true
    } else {
        if ($output -match "already in" -or $output -match "already exists") {
            Write-Host " [SKIP - Ya existe]" -ForegroundColor Yellow
            return $false
        } else {
            Write-Host " [FAIL]" -ForegroundColor Red
            $failed += "$ResourceType.$TerraformName"
            return $false
        }
    }
}

# SSM Parameters - Intentar importar los que sabemos que existen
Try-ImportResource "aws_ssm_parameter" "/gatekeep/db/host" "db_host" "SSM DB Host"
Try-ImportResource "aws_ssm_parameter" "/gatekeep/db/port" "db_port" "SSM DB Port"
Try-ImportResource "aws_ssm_parameter" "/gatekeep/db/name" "db_name" "SSM DB Name"
Try-ImportResource "aws_ssm_parameter" "/gatekeep/db/username" "db_username" "SSM DB Username"
Try-ImportResource "aws_ssm_parameter" "/gatekeep/ecr/repository-uri" "ecr_repository_uri" "SSM ECR URI"

# RDS Instance
Try-ImportResource "aws_db_instance" "gatekeep-db" "main" "RDS DB Instance"

Write-Host ""
Write-Host "===== Resumen de Importacion =====" -ForegroundColor Cyan
Write-Host "Importados exitosamente: $($imported.Count)"
Write-Host "Fallos/Omitidos: $($failed.Count)"
Write-Host ""

if ($failed.Count -gt 0) {
    Write-Host "Recursos con error:" -ForegroundColor Yellow
    foreach ($item in $failed) {
        Write-Host "  - $item"
    }
    Write-Host ""
}

Write-Host "Para importar recursos adicionales manualmente, usa:" -ForegroundColor Cyan
Write-Host "  terraform import <resource_type>.<name> <aws_id>" -ForegroundColor Gray
Write-Host ""
