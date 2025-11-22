# 🔧 Plan de Reparación AWS - Corrección de Problemas

**Fecha:** 2025-01-21  
**Región:** sa-east-1  
**Objetivo:** Corregir todos los problemas identificados en la configuración de AWS

---

## 📋 RESUMEN DE PROBLEMAS A CORREGIR

### 🔴 Prioridad Alta

1. **Variable de Entorno Incorrecta en Task Definition del Frontend**
   - Variable: `NEXT_PUBLIC_API_URL`
   - Valor Actual: `https://zimmzimmgames.com` ❌
   - Valor Correcto: `https://api.zimmzimmgames.com` ✅
   - Impacto: Puede causar errores en SSR y llamadas al backend

---

## 🎯 PLAN DE ACCIÓN

### Paso 1: Obtener Task Definition Actual del Frontend

**Objetivo:** Obtener la configuración actual para modificarla

```bash
# Obtener la Task Definition actual
aws ecs describe-task-definition \
  --task-definition gatekeep-frontend:3 \
  --region sa-east-1 \
  --query 'taskDefinition' \
  --output json > task-definition-frontend-current.json
```

**Verificación:**
```bash
# Verificar la variable de entorno actual
aws ecs describe-task-definition \
  --task-definition gatekeep-frontend:3 \
  --region sa-east-1 \
  --query 'taskDefinition.containerDefinitions[0].environment[?name==`NEXT_PUBLIC_API_URL`]' \
  --output json
```

---

### Paso 2: Crear Nueva Task Definition con Variable Corregida

**Objetivo:** Crear una nueva revisión de la Task Definition con la variable corregida

#### Opción A: Usando archivo JSON (Recomendado)

```bash
# 1. Obtener Task Definition completa
aws ecs describe-task-definition \
  --task-definition gatekeep-frontend:3 \
  --region sa-east-1 \
  --query 'taskDefinition' \
  --output json > task-definition-frontend-new.json

# 2. Editar el archivo JSON (usar editor o script)
# Cambiar: "value": "https://zimmzimmgames.com"
# A: "value": "https://api.zimmzimmgames.com"

# 3. Eliminar campos que no se pueden especificar al registrar
# Eliminar: taskDefinitionArn, revision, status, requiresAttributes, 
#           placementConstraints, compatibilities, registeredAt, registeredBy

# 4. Registrar nueva revisión
aws ecs register-task-definition \
  --cli-input-json file://task-definition-frontend-new.json \
  --region sa-east-1
```

#### Opción B: Usando jq para modificar automáticamente

```bash
# Obtener y modificar en un solo paso
aws ecs describe-task-definition \
  --task-definition gatekeep-frontend:3 \
  --region sa-east-1 \
  --query 'taskDefinition' \
  --output json | \
jq 'del(.taskDefinitionArn, .revision, .status, .requiresAttributes, .placementConstraints, .compatibilities, .registeredAt, .registeredBy) | 
    .containerDefinitions[0].environment[] |= if .name == "NEXT_PUBLIC_API_URL" then .value = "https://api.zimmzimmgames.com" else . end' > task-definition-frontend-new.json

# Registrar nueva revisión
aws ecs register-task-definition \
  --cli-input-json file://task-definition-frontend-new.json \
  --region sa-east-1
```

**Verificación:**
```bash
# Verificar que la nueva revisión tiene la variable correcta
aws ecs describe-task-definition \
  --task-definition gatekeep-frontend:4 \
  --region sa-east-1 \
  --query 'taskDefinition.containerDefinitions[0].environment[?name==`NEXT_PUBLIC_API_URL`]' \
  --output json
```

---

### Paso 3: Actualizar el Servicio ECS para Usar la Nueva Task Definition

**Objetivo:** Hacer que el servicio use la nueva revisión de la Task Definition

```bash
# Actualizar el servicio (reemplazar :4 con el número de revisión obtenido)
aws ecs update-service \
  --cluster gatekeep-cluster \
  --service gatekeep-frontend-service \
  --task-definition gatekeep-frontend:4 \
  --region sa-east-1 \
  --force-new-deployment
```

**Verificación:**
```bash
# Verificar que el servicio está usando la nueva Task Definition
aws ecs describe-services \
  --cluster gatekeep-cluster \
  --services gatekeep-frontend-service \
  --region sa-east-1 \
  --query 'services[0].{TaskDefinition:taskDefinition,Desired:desiredCount,Running:runningCount,Deployments:deployments[*].{Status:status,TaskDef:taskDefinition}}' \
  --output json
```

---

### Paso 4: Monitorear el Deployment

**Objetivo:** Verificar que el nuevo deployment se complete exitosamente

```bash
# Monitorear el estado del servicio
aws ecs describe-services \
  --cluster gatekeep-cluster \
  --services gatekeep-frontend-service \
  --region sa-east-1 \
  --query 'services[0].events[0:5].{Time:createdAt,Message:message}' \
  --output table

# Verificar estado de las tareas
aws ecs list-tasks \
  --cluster gatekeep-cluster \
  --service-name gatekeep-frontend-service \
  --region sa-east-1 \
  --output json

# Verificar health status de los targets
aws elbv2 describe-target-health \
  --target-group-arn arn:aws:elasticloadbalancing:sa-east-1:126588786097:targetgroup/gatekeep-frontend-tg/fe6df7144ffc7ee4 \
  --region sa-east-1 \
  --query 'TargetHealthDescriptions[*].{Target:Target.Id,Status:TargetHealth.State}' \
  --output table
```

---

### Paso 5: Verificar que la Variable Está Correcta en la Tarea Ejecutándose

**Objetivo:** Confirmar que la nueva tarea tiene la variable correcta

```bash
# Obtener ARN de la tarea actual
TASK_ARN=$(aws ecs list-tasks \
  --cluster gatekeep-cluster \
  --service-name gatekeep-frontend-service \
  --region sa-east-1 \
  --query 'taskArns[0]' \
  --output text)

# Verificar la Task Definition de la tarea
aws ecs describe-tasks \
  --cluster gatekeep-cluster \
  --tasks $TASK_ARN \
  --region sa-east-1 \
  --query 'tasks[0].{TaskDef:taskDefinitionArn,Status:lastStatus,Health:healthStatus}' \
  --output json
```

---

## 📝 SCRIPT COMPLETO DE REPARACIÓN

### Script PowerShell (Windows)

```powershell
# ============================================
# Script de Reparación: Task Definition Frontend
# ============================================

$region = "sa-east-1"
$cluster = "gatekeep-cluster"
$service = "gatekeep-frontend-service"
$taskFamily = "gatekeep-frontend"
$currentRevision = 3

Write-Host "`n=== REPARACIÓN DE TASK DEFINITION FRONTEND ===" -ForegroundColor Cyan

# Paso 1: Obtener Task Definition actual
Write-Host "`n[1/5] Obteniendo Task Definition actual..." -ForegroundColor Yellow
$currentTaskDef = aws ecs describe-task-definition `
  --task-definition "$taskFamily`:$currentRevision" `
  --region $region `
  --query 'taskDefinition' `
  --output json | ConvertFrom-Json

Write-Host "  Variable actual NEXT_PUBLIC_API_URL:" -ForegroundColor Gray
$currentEnv = $currentTaskDef.containerDefinitions[0].environment | Where-Object { $_.name -eq "NEXT_PUBLIC_API_URL" }
Write-Host "    $($currentEnv.value)" -ForegroundColor $(if ($currentEnv.value -eq "https://api.zimmzimmgames.com") { "Green" } else { "Red" })

# Paso 2: Crear nueva Task Definition con variable corregida
Write-Host "`n[2/5] Creando nueva Task Definition..." -ForegroundColor Yellow

# Eliminar campos que no se pueden especificar
$newTaskDef = $currentTaskDef | Select-Object -Property * -ExcludeProperty taskDefinitionArn, revision, status, requiresAttributes, placementConstraints, compatibilities, registeredAt, registeredBy

# Corregir variable de entorno
$newTaskDef.containerDefinitions[0].environment | ForEach-Object {
    if ($_.name -eq "NEXT_PUBLIC_API_URL") {
        $_.value = "https://api.zimmzimmgames.com"
        Write-Host "  ✅ Variable corregida: $($_.value)" -ForegroundColor Green
    }
}

# Guardar en archivo temporal
$tempFile = "task-definition-frontend-new.json"
$newTaskDef | ConvertTo-Json -Depth 10 | Out-File -FilePath $tempFile -Encoding UTF8

# Paso 3: Registrar nueva revisión
Write-Host "`n[3/5] Registrando nueva revisión de Task Definition..." -ForegroundColor Yellow
$registerResult = aws ecs register-task-definition `
  --cli-input-json "file://$tempFile" `
  --region $region `
  --output json | ConvertFrom-Json

$newRevision = $registerResult.taskDefinition.revision
Write-Host "  ✅ Nueva revisión creada: $taskFamily`:$newRevision" -ForegroundColor Green

# Paso 4: Actualizar servicio
Write-Host "`n[4/5] Actualizando servicio ECS..." -ForegroundColor Yellow
aws ecs update-service `
  --cluster $cluster `
  --service $service `
  --task-definition "$taskFamily`:$newRevision" `
  --region $region `
  --force-new-deployment | Out-Null

Write-Host "  ✅ Servicio actualizado, nuevo deployment iniciado" -ForegroundColor Green

# Paso 5: Monitorear deployment
Write-Host "`n[5/5] Monitoreando deployment..." -ForegroundColor Yellow
Write-Host "  Esperando a que el servicio alcance steady state..." -ForegroundColor Gray

$maxWait = 300 # 5 minutos
$elapsed = 0
$interval = 10

do {
    Start-Sleep -Seconds $interval
    $elapsed += $interval
    
    $serviceStatus = aws ecs describe-services `
      --cluster $cluster `
      --services $service `
      --region $region `
      --query 'services[0].{Running:runningCount,Desired:desiredCount,Deployments:deployments[*].{Status:status,Running:runningCount}}' `
      --output json | ConvertFrom-Json
    
    Write-Host "  [$elapsed/$maxWait] Running: $($serviceStatus.Running)/$($serviceStatus.Desired)" -ForegroundColor Gray
    
    if ($serviceStatus.Running -eq $serviceStatus.Desired -and $serviceStatus.Deployments[0].Status -eq "PRIMARY") {
        Write-Host "  ✅ Deployment completado exitosamente" -ForegroundColor Green
        break
    }
    
    if ($elapsed -ge $maxWait) {
        Write-Host "  ⚠️  Tiempo de espera agotado, verificar manualmente" -ForegroundColor Yellow
        break
    }
} while ($true)

# Limpiar archivo temporal
Remove-Item $tempFile -ErrorAction SilentlyContinue

# Verificación final
Write-Host "`n=== VERIFICACIÓN FINAL ===" -ForegroundColor Cyan
$finalTaskDef = aws ecs describe-task-definition `
  --task-definition "$taskFamily`:$newRevision" `
  --region $region `
  --query 'taskDefinition.containerDefinitions[0].environment[?name==`NEXT_PUBLIC_API_URL`]' `
  --output json | ConvertFrom-Json

Write-Host "`nVariable de entorno en nueva Task Definition:" -ForegroundColor Yellow
Write-Host "  $($finalTaskDef.name) = $($finalTaskDef.value)" -ForegroundColor $(if ($finalTaskDef.value -eq "https://api.zimmzimmgames.com") { "Green" } else { "Red" })

Write-Host "`n✅ Proceso de reparación completado" -ForegroundColor Green
```

---

## 🔍 VERIFICACIONES POST-REPARACIÓN

### 1. Verificar Variable en Tarea Ejecutándose

```bash
# Obtener tarea actual
TASK_ARN=$(aws ecs list-tasks \
  --cluster gatekeep-cluster \
  --service-name gatekeep-frontend-service \
  --region sa-east-1 \
  --query 'taskArns[0]' \
  --output text)

# Obtener Task Definition de la tarea
TASK_DEF_ARN=$(aws ecs describe-tasks \
  --cluster gatekeep-cluster \
  --tasks $TASK_ARN \
  --region sa-east-1 \
  --query 'tasks[0].taskDefinitionArn' \
  --output text)

# Verificar variable
aws ecs describe-task-definition \
  --task-definition $TASK_DEF_ARN \
  --region sa-east-1 \
  --query 'taskDefinition.containerDefinitions[0].environment[?name==`NEXT_PUBLIC_API_URL`]' \
  --output json
```

### 2. Verificar Health Status

```bash
aws elbv2 describe-target-health \
  --target-group-arn arn:aws:elasticloadbalancing:sa-east-1:126588786097:targetgroup/gatekeep-frontend-tg/fe6df7144ffc7ee4 \
  --region sa-east-1 \
  --query 'TargetHealthDescriptions[*].{Target:Target.Id,Status:TargetHealth.State,Reason:TargetHealth.Reason}' \
  --output table
```

### 3. Probar Endpoint del Frontend

```bash
# Probar que el frontend responde
curl -I https://zimmzimmgames.com

# Verificar logs (si es necesario)
aws logs tail /aws/ecs/gatekeep-frontend \
  --region sa-east-1 \
  --since 10m \
  --format short
```

---

## ⚠️ CONSIDERACIONES IMPORTANTES

### 1. Downtime Mínimo
- El deployment con `--force-new-deployment` causará un breve downtime
- ECS realizará un rolling deployment (nueva tarea → health check → eliminar antigua)
- Tiempo estimado: 2-5 minutos

### 2. Rollback Plan
Si algo sale mal, puedes volver a la revisión anterior:

```bash
aws ecs update-service \
  --cluster gatekeep-cluster \
  --service gatekeep-frontend-service \
  --task-definition gatekeep-frontend:3 \
  --region sa-east-1 \
  --force-new-deployment
```

### 3. Verificar Terraform
Después de corregir en AWS, verificar si Terraform tiene la configuración correcta:

```bash
# Verificar configuración en Terraform
grep -A 3 "NEXT_PUBLIC_API_URL" Gatekeep/terraform/ecs.tf
```

Si Terraform tiene el valor correcto pero AWS no, puede ser que:
- Terraform no se aplicó después de un cambio
- La Task Definition fue modificada manualmente
- Hay un `lifecycle { ignore_changes = all }` que previene actualizaciones

---

## 📊 CHECKLIST DE REPARACIÓN

- [ ] Paso 1: Obtener Task Definition actual
- [ ] Paso 2: Crear nueva Task Definition con variable corregida
- [ ] Paso 3: Registrar nueva revisión
- [ ] Paso 4: Actualizar servicio ECS
- [ ] Paso 5: Monitorear deployment
- [ ] Verificación: Variable correcta en nueva revisión
- [ ] Verificación: Servicio usando nueva Task Definition
- [ ] Verificación: Health status healthy
- [ ] Verificación: Frontend responde correctamente
- [ ] Verificación: Logs sin errores relacionados

---

## 🎯 RESULTADO ESPERADO

Después de completar este plan:

1. ✅ La Task Definition del frontend tendrá `NEXT_PUBLIC_API_URL=https://api.zimmzimmgames.com`
2. ✅ El servicio ECS usará la nueva revisión
3. ✅ Las nuevas tareas tendrán la variable correcta
4. ✅ El frontend podrá hacer llamadas correctas al backend
5. ✅ No habrá errores relacionados con URLs incorrectas

---

**Documento generado:** 2025-01-21  
**Última actualización:** 2025-01-21

