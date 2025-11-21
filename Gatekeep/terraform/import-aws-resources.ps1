# Script para importar todos los recursos de AWS a Terraform
# Este script importa recursos que existen en AWS pero no están en Terraform state

$ErrorActionPreference = "Continue"
$WarningPreference = "SilentlyContinue"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Importar Recursos AWS a Terraform" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Determinar comando de terraform a usar
$terraformCmd = "terraform"
$terraformExePath = Join-Path $PSScriptRoot "terraform.exe"
if (Test-Path $terraformExePath) {
    $terraformCmd = $terraformExePath
}

$imported = @()
$failed = @()
$skipped = @()

# Función auxiliar para importar
function Try-ImportResource {
    param(
        [string]$ResourceType,
        [string]$ResourceId,
        [string]$TerraformName,
        [string]$Description
    )
    
    Write-Host "Importando: $Description..." -NoNewline
    
    # Verificar si ya está en el state
    $stateList = & $terraformCmd state list 2>&1
    $alreadyInState = $stateList | Select-String "$ResourceType.$TerraformName"
    
    if ($alreadyInState) {
        Write-Host " [SKIP - Ya existe en state]" -ForegroundColor Yellow
        $skipped += "$ResourceType.$TerraformName"
        return $false
    }
    
    $output = & $terraformCmd import "$ResourceType.$TerraformName" "$ResourceId" 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host " [OK]" -ForegroundColor Green
        $imported += "$ResourceType.$TerraformName"
        return $true
    } else {
        if ($output -match "already in" -or $output -match "already exists" -or $output -match "Resource already managed") {
            Write-Host " [SKIP - Ya existe]" -ForegroundColor Yellow
            $skipped += "$ResourceType.$TerraformName"
            return $false
        } else {
            Write-Host " [FAIL]" -ForegroundColor Red
            Write-Host "    Error: $output" -ForegroundColor Gray
            $failed += "$ResourceType.$TerraformName"
            return $false
        }
    }
}

Write-Host "[1] Importando Secret: gatekeep/mongodb/connection" -ForegroundColor Yellow
Try-ImportResource "aws_secretsmanager_secret" "gatekeep/mongodb/connection" "mongodb_connection" "MongoDB Connection Secret"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Resumen de Importacion" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Importados exitosamente: $($imported.Count)" -ForegroundColor Green
if ($imported.Count -gt 0) {
    foreach ($item in $imported) {
        Write-Host "  OK: $item" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "Omitidos (ya existian): $($skipped.Count)" -ForegroundColor Yellow
if ($skipped.Count -gt 0) {
    foreach ($item in $skipped) {
        Write-Host "  - $item" -ForegroundColor Gray
    }
}

Write-Host ""
$failColor = if($failed.Count -eq 0){"Green"}else{"Red"}
Write-Host "Fallos: $($failed.Count)" -ForegroundColor $failColor
if ($failed.Count -gt 0) {
    foreach ($item in $failed) {
        Write-Host "  ERROR: $item" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Verificacion Post-Import" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Ejecutando terraform state list..." -ForegroundColor Yellow
$stateList = & $terraformCmd state list 2>&1
$totalResources = ($stateList | Where-Object { $_ -notmatch "data\." }).Count
Write-Host "Total de recursos en Terraform state: $totalResources" -ForegroundColor Green

Write-Host ""
Write-Host "Ejecutando terraform plan para verificar estado..." -ForegroundColor Yellow
$planOutput = & $terraformCmd plan -no-color 2>&1
$planSummary = $planOutput | Select-String "Plan:"
if ($planSummary) {
    $summaryText = $planSummary.ToString()
    $summaryColor = if($summaryText -match "0 to add, 0 to change, 0 to destroy"){"Green"}else{"Yellow"}
    Write-Host $summaryText -ForegroundColor $summaryColor
} else {
    Write-Host "Plan ejecutado. Revisar salida arriba." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Importacion Completada" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

