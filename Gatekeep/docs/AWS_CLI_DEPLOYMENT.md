# Guía de Despliegue con AWS CLI - Frontend y Backend por Separado

Este documento contiene todos los comandos necesarios para levantar, gestionar y monitorear el frontend y backend de GateKeep en AWS usando AWS CLI.

## Requisitos Previos

1. **AWS CLI instalado y configurado**
   ```bash
   aws --version
   aws configure list
   ```

2. **Credenciales configuradas**
   ```bash
   aws configure
   # O usar variables de entorno:
   export AWS_ACCESS_KEY_ID=tu_access_key
   export AWS_SECRET_ACCESS_KEY=tu_secret_key
   export AWS_REGION=sa-east-1
   ```

3. **Verificar identidad**
   ```bash
   aws sts get-caller-identity
   ```

## Variables de Configuración

Antes de ejecutar los comandos, configura estas variables según tu entorno:

```bash
# Región de AWS
AWS_REGION="sa-east-1"

# Cluster ECS
ECS_CLUSTER_NAME="gatekeep-cluster"

# Servicios ECS
ECS_SERVICE_API="gatekeep-api-service"
ECS_SERVICE_FRONTEND="gatekeep-frontend-service"

# Repositorios ECR
ECR_API_REPO="126588786097.dkr.ecr.sa-east-1.amazonaws.com/gatekeep-api"
ECR_FRONTEND_REPO="126588786097.dkr.ecr.sa-east-1.amazonaws.com/gatekeep-frontend"

# Task Definitions
TASK_DEF_API="gatekeep-api"
TASK_DEF_FRONTEND="gatekeep-frontend"
```

## 1. Levantar el Backend (API)

### 1.1 Verificar Estado Actual del Servicio

```bash
aws ecs describe-services \
  --cluster $ECS_CLUSTER_NAME \
  --services $ECS_SERVICE_API \
  --region $AWS_REGION \
  --query 'services[0].{Status:status,DesiredCount:desiredCount,RunningCount:runningCount,PendingCount:pendingCount}' \
  --output table
```

### 1.2 Actualizar el Servicio (Forzar Nuevo Deployment)

```bash
aws ecs update-service \
  --cluster $ECS_CLUSTER_NAME \
  --service $ECS_SERVICE_API \
  --force-new-deployment \
  --region $AWS_REGION \
  --output json
```

### 1.3 Escalar el Servicio (Aumentar Número de Tareas)

```bash
# Escalar a 2 tareas
aws ecs update-service \
  --cluster $ECS_CLUSTER_NAME \
  --service $ECS_SERVICE_API \
  --desired-count 2 \
  --region $AWS_REGION \
  --output json
```

### 1.4 Iniciar el Servicio (Si está Detenido)

```bash
# Verificar si el servicio existe
aws ecs describe-services \
  --cluster $ECS_CLUSTER_NAME \
  --services $ECS_SERVICE_API \
  --region $AWS_REGION

# Si el servicio no existe o está detenido, actualizar desired-count a 1
aws ecs update-service \
  --cluster $ECS_CLUSTER_NAME \
  --service $ECS_SERVICE_API \
  --desired-count 1 \
  --region $AWS_REGION \
  --output json
```

### 1.5 Verificar Estado del Deployment

```bash
aws ecs describe-services \
  --cluster $ECS_CLUSTER_NAME \
  --services $ECS_SERVICE_API \
  --region $AWS_REGION \
  --query 'services[0].deployments[*].{Status:status,DesiredCount:desiredCount,RunningCount:runningCount,Id:id,CreatedAt:createdAt}' \
  --output table
```

### 1.6 Monitorear el Progreso del Deployment

```bash
# Monitorear hasta que el deployment esté estable
while true; do
  STATUS=$(aws ecs describe-services \
    --cluster $ECS_CLUSTER_NAME \
    --services $ECS_SERVICE_API \
    --region $AWS_REGION \
    --query 'services[0].deployments[0].status' \
    --output text)
  
  RUNNING=$(aws ecs describe-services \
    --cluster $ECS_CLUSTER_NAME \
    --services $ECS_SERVICE_API \
    --region $AWS_REGION \
    --query 'services[0].deployments[0].runningCount' \
    --output text)
  
  DESIRED=$(aws ecs describe-services \
    --cluster $ECS_CLUSTER_NAME \
    --services $ECS_SERVICE_API \
    --region $AWS_REGION \
    --query 'services[0].deployments[0].desiredCount' \
    --output text)
  
  echo "$(date): Status=$STATUS, Running=$RUNNING/$DESIRED"
  
  if [ "$STATUS" = "PRIMARY" ] && [ "$RUNNING" = "$DESIRED" ]; then
    echo "Deployment completado exitosamente"
    break
  fi
  
  sleep 10
done
```

## 2. Levantar el Frontend

### 2.1 Verificar Estado Actual del Servicio

```bash
aws ecs describe-services \
  --cluster $ECS_CLUSTER_NAME \
  --services $ECS_SERVICE_FRONTEND \
  --region $AWS_REGION \
  --query 'services[0].{Status:status,DesiredCount:desiredCount,RunningCount:runningCount,PendingCount:pendingCount}' \
  --output table
```

### 2.2 Actualizar el Servicio (Forzar Nuevo Deployment)

```bash
aws ecs update-service \
  --cluster $ECS_CLUSTER_NAME \
  --service $ECS_SERVICE_FRONTEND \
  --force-new-deployment \
  --region $AWS_REGION \
  --output json
```

### 2.3 Escalar el Servicio

```bash
# Escalar a 2 tareas
aws ecs update-service \
  --cluster $ECS_CLUSTER_NAME \
  --service $ECS_SERVICE_FRONTEND \
  --desired-count 2 \
  --region $AWS_REGION \
  --output json
```

### 2.4 Iniciar el Servicio (Si está Detenido)

```bash
aws ecs update-service \
  --cluster $ECS_CLUSTER_NAME \
  --service $ECS_SERVICE_FRONTEND \
  --desired-count 1 \
  --region $AWS_REGION \
  --output json
```

### 2.5 Verificar Estado del Deployment

```bash
aws ecs describe-services \
  --cluster $ECS_CLUSTER_NAME \
  --services $ECS_SERVICE_FRONTEND \
  --region $AWS_REGION \
  --query 'services[0].deployments[*].{Status:status,DesiredCount:desiredCount,RunningCount:runningCount,Id:id,CreatedAt:createdAt}' \
  --output table
```

## 3. Ver Logs

### 3.1 Ver Logs del Backend

```bash
# Ver logs recientes
aws logs tail /ecs/gatekeep \
  --follow \
  --region $AWS_REGION \
  --filter-pattern "ecs-api"

# Ver últimas 100 líneas
aws logs tail /ecs/gatekeep \
  --since 1h \
  --region $AWS_REGION \
  --filter-pattern "ecs-api" \
  --format short
```

### 3.2 Ver Logs del Frontend

```bash
# Ver logs recientes
aws logs tail /ecs/gatekeep \
  --follow \
  --region $AWS_REGION \
  --filter-pattern "ecs-frontend"

# Ver últimas 100 líneas
aws logs tail /ecs/gatekeep \
  --since 1h \
  --region $AWS_REGION \
  --filter-pattern "ecs-frontend" \
  --format short
```

### 3.3 Ver Todos los Logs del Cluster

```bash
aws logs tail /ecs/gatekeep \
  --follow \
  --region $AWS_REGION \
  --format short
```

## 4. Detener Servicios

### 4.1 Detener el Backend

```bash
aws ecs update-service \
  --cluster $ECS_CLUSTER_NAME \
  --service $ECS_SERVICE_API \
  --desired-count 0 \
  --region $AWS_REGION \
  --output json
```

### 4.2 Detener el Frontend

```bash
aws ecs update-service \
  --cluster $ECS_CLUSTER_NAME \
  --service $ECS_SERVICE_FRONTEND \
  --desired-count 0 \
  --region $AWS_REGION \
  --output json
```

### 4.3 Detener Ambos Servicios

```bash
# Detener backend
aws ecs update-service \
  --cluster $ECS_CLUSTER_NAME \
  --service $ECS_SERVICE_API \
  --desired-count 0 \
  --region $AWS_REGION

# Detener frontend
aws ecs update-service \
  --cluster $ECS_CLUSTER_NAME \
  --service $ECS_SERVICE_FRONTEND \
  --desired-count 0 \
  --region $AWS_REGION
```

## 5. Verificar Estado General

### 5.1 Listar Todos los Servicios del Cluster

```bash
aws ecs list-services \
  --cluster $ECS_CLUSTER_NAME \
  --region $AWS_REGION \
  --output table
```

### 5.2 Ver Estado de Todos los Servicios

```bash
aws ecs describe-services \
  --cluster $ECS_CLUSTER_NAME \
  --services $ECS_SERVICE_API $ECS_SERVICE_FRONTEND \
  --region $AWS_REGION \
  --query 'services[*].{ServiceName:serviceName,Status:status,DesiredCount:desiredCount,RunningCount:runningCount,PendingCount:pendingCount}' \
  --output table
```

### 5.3 Ver Tareas en Ejecución

```bash
# Ver tareas del backend
aws ecs list-tasks \
  --cluster $ECS_CLUSTER_NAME \
  --service-name $ECS_SERVICE_API \
  --region $AWS_REGION \
  --output table

# Ver tareas del frontend
aws ecs list-tasks \
  --cluster $ECS_CLUSTER_NAME \
  --service-name $ECS_SERVICE_FRONTEND \
  --region $AWS_REGION \
  --output table
```

### 5.4 Ver Detalles de una Tarea Específica

```bash
# Obtener ARN de una tarea
TASK_ARN=$(aws ecs list-tasks \
  --cluster $ECS_CLUSTER_NAME \
  --service-name $ECS_SERVICE_API \
  --region $AWS_REGION \
  --query 'taskArns[0]' \
  --output text)

# Ver detalles de la tarea
aws ecs describe-tasks \
  --cluster $ECS_CLUSTER_NAME \
  --tasks $TASK_ARN \
  --region $AWS_REGION \
  --query 'tasks[0].{LastStatus:lastStatus,HealthStatus:healthStatus,StartedAt:startedAt,StoppedAt:stoppedAt}' \
  --output table
```

## 6. Actualizar Imágenes Docker

### 6.1 Autenticarse con ECR

```bash
aws ecr get-login-password --region $AWS_REGION | \
  docker login --username AWS --password-stdin $ECR_API_REPO
```

### 6.2 Construir y Subir Nueva Imagen del Backend

```bash
# Construir imagen
docker build -t $ECR_API_REPO:latest -f src/Dockerfile src/

# Subir a ECR
docker push $ECR_API_REPO:latest

# Forzar nuevo deployment
aws ecs update-service \
  --cluster $ECS_CLUSTER_NAME \
  --service $ECS_SERVICE_API \
  --force-new-deployment \
  --region $AWS_REGION
```

### 6.3 Construir y Subir Nueva Imagen del Frontend

```bash
# Construir imagen
docker build -t $ECR_FRONTEND_REPO:latest -f frontend/Dockerfile frontend/

# Subir a ECR
docker push $ECR_FRONTEND_REPO:latest

# Forzar nuevo deployment
aws ecs update-service \
  --cluster $ECS_CLUSTER_NAME \
  --service $ECS_SERVICE_FRONTEND \
  --force-new-deployment \
  --region $AWS_REGION
```

## 7. Actualizar Variables de Entorno

### 7.1 Obtener Task Definition Actual

```bash
# Obtener task definition del backend
aws ecs describe-services \
  --cluster $ECS_CLUSTER_NAME \
  --services $ECS_SERVICE_API \
  --region $AWS_REGION \
  --query 'services[0].taskDefinition' \
  --output text

# Obtener task definition completa
TASK_DEF_ARN=$(aws ecs describe-services \
  --cluster $ECS_CLUSTER_NAME \
  --services $ECS_SERVICE_API \
  --region $AWS_REGION \
  --query 'services[0].taskDefinition' \
  --output text)

aws ecs describe-task-definition \
  --task-definition $TASK_DEF_ARN \
  --region $AWS_REGION \
  --output json > task-definition-backend.json
```

### 7.2 Actualizar Task Definition y Servicio

```bash
# Registrar nueva task definition (después de editar el JSON)
aws ecs register-task-definition \
  --cli-input-json file://task-definition-backend.json \
  --region $AWS_REGION

# Actualizar servicio con nueva task definition
NEW_TASK_DEF=$(aws ecs describe-task-definition \
  --task-definition $TASK_DEF_API \
  --region $AWS_REGION \
  --query 'taskDefinition.taskDefinitionArn' \
  --output text)

aws ecs update-service \
  --cluster $ECS_CLUSTER_NAME \
  --service $ECS_SERVICE_API \
  --task-definition $NEW_TASK_DEF \
  --region $AWS_REGION
```

## 8. Scripts de Ejemplo

### 8.1 Script para Levantar Solo el Backend

```bash
#!/bin/bash

# Configuración
AWS_REGION="sa-east-1"
ECS_CLUSTER_NAME="gatekeep-cluster"
ECS_SERVICE_API="gatekeep-api-service"

echo "Levantando backend..."

# Verificar estado actual
echo "Verificando estado actual..."
aws ecs describe-services \
  --cluster $ECS_CLUSTER_NAME \
  --services $ECS_SERVICE_API \
  --region $AWS_REGION \
  --query 'services[0].{Status:status,DesiredCount:desiredCount,RunningCount:runningCount}' \
  --output table

# Actualizar servicio
echo "Actualizando servicio..."
aws ecs update-service \
  --cluster $ECS_CLUSTER_NAME \
  --service $ECS_SERVICE_API \
  --desired-count 1 \
  --force-new-deployment \
  --region $AWS_REGION \
  --output json

echo "Backend iniciado. Monitoreando deployment..."
```

### 8.2 Script para Levantar Solo el Frontend

```bash
#!/bin/bash

# Configuración
AWS_REGION="sa-east-1"
ECS_CLUSTER_NAME="gatekeep-cluster"
ECS_SERVICE_FRONTEND="gatekeep-frontend-service"

echo "Levantando frontend..."

# Verificar estado actual
echo "Verificando estado actual..."
aws ecs describe-services \
  --cluster $ECS_CLUSTER_NAME \
  --services $ECS_SERVICE_FRONTEND \
  --region $AWS_REGION \
  --query 'services[0].{Status:status,DesiredCount:desiredCount,RunningCount:runningCount}' \
  --output table

# Actualizar servicio
echo "Actualizando servicio..."
aws ecs update-service \
  --cluster $ECS_CLUSTER_NAME \
  --service $ECS_SERVICE_FRONTEND \
  --desired-count 1 \
  --force-new-deployment \
  --region $AWS_REGION \
  --output json

echo "Frontend iniciado. Monitoreando deployment..."
```

### 8.3 Script para Ver Estado de Ambos Servicios

```bash
#!/bin/bash

AWS_REGION="sa-east-1"
ECS_CLUSTER_NAME="gatekeep-cluster"
ECS_SERVICE_API="gatekeep-api-service"
ECS_SERVICE_FRONTEND="gatekeep-frontend-service"

echo "=== Estado del Backend ==="
aws ecs describe-services \
  --cluster $ECS_CLUSTER_NAME \
  --services $ECS_SERVICE_API \
  --region $AWS_REGION \
  --query 'services[0].{ServiceName:serviceName,Status:status,DesiredCount:desiredCount,RunningCount:runningCount,PendingCount:pendingCount}' \
  --output table

echo ""
echo "=== Estado del Frontend ==="
aws ecs describe-services \
  --cluster $ECS_CLUSTER_NAME \
  --services $ECS_SERVICE_FRONTEND \
  --region $AWS_REGION \
  --query 'services[0].{ServiceName:serviceName,Status:status,DesiredCount:desiredCount,RunningCount:runningCount,PendingCount:pendingCount}' \
  --output table
```

## 9. Solución de Problemas

### 9.1 Verificar Por Qué una Tarea No Inicia

```bash
# Obtener ARN de tareas detenidas
aws ecs list-tasks \
  --cluster $ECS_CLUSTER_NAME \
  --service-name $ECS_SERVICE_API \
  --desired-status STOPPED \
  --region $AWS_REGION

# Ver detalles de una tarea detenida
TASK_ARN="arn:aws:ecs:sa-east-1:ACCOUNT:task/CLUSTER/TASK_ID"
aws ecs describe-tasks \
  --cluster $ECS_CLUSTER_NAME \
  --tasks $TASK_ARN \
  --region $AWS_REGION \
  --query 'tasks[0].{StoppedReason:stoppedReason,StoppedAt:stoppedAt,Containers:containers[*].{Name:name,Reason:reason,ExitCode:exitCode}}' \
  --output json
```

### 9.2 Ver Eventos del Servicio

```bash
aws ecs describe-services \
  --cluster $ECS_CLUSTER_NAME \
  --services $ECS_SERVICE_API \
  --region $AWS_REGION \
  --query 'services[0].events[0:5]' \
  --output table
```

### 9.3 Verificar Health Checks

```bash
# Ver estado de salud de las tareas
aws ecs describe-tasks \
  --cluster $ECS_CLUSTER_NAME \
  --tasks $(aws ecs list-tasks --cluster $ECS_CLUSTER_NAME --service-name $ECS_SERVICE_API --region $AWS_REGION --query 'taskArns[0]' --output text) \
  --region $AWS_REGION \
  --query 'tasks[0].healthStatus' \
  --output text
```

## 10. Comandos Rápidos de Referencia

### Backend

```bash
# Levantar
aws ecs update-service --cluster gatekeep-cluster --service gatekeep-api-service --desired-count 1 --force-new-deployment --region sa-east-1

# Detener
aws ecs update-service --cluster gatekeep-cluster --service gatekeep-api-service --desired-count 0 --region sa-east-1

# Estado
aws ecs describe-services --cluster gatekeep-cluster --services gatekeep-api-service --region sa-east-1 --query 'services[0].{Status:status,Running:runningCount,Desired:desiredCount}' --output table

# Logs
aws logs tail /ecs/gatekeep --follow --region sa-east-1 --filter-pattern "ecs-api"
```

### Frontend

```bash
# Levantar
aws ecs update-service --cluster gatekeep-cluster --service gatekeep-frontend-service --desired-count 1 --force-new-deployment --region sa-east-1

# Detener
aws ecs update-service --cluster gatekeep-cluster --service gatekeep-frontend-service --desired-count 0 --region sa-east-1

# Estado
aws ecs describe-services --cluster gatekeep-cluster --services gatekeep-frontend-service --region sa-east-1 --query 'services[0].{Status:status,Running:runningCount,Desired:desiredCount}' --output table

# Logs
aws logs tail /ecs/gatekeep --follow --region sa-east-1 --filter-pattern "ecs-frontend"
```

## Notas Importantes

1. **Región**: Asegúrate de usar la región correcta (`sa-east-1` por defecto)
2. **Permisos**: Necesitas permisos IAM adecuados para ECS, ECR, CloudWatch Logs
3. **Tiempo de Deployment**: Los deployments pueden tardar 5-15 minutos
4. **Costos**: Mantener servicios con `desired-count > 0` genera costos
5. **Health Checks**: Los servicios tienen health checks configurados, espera a que pasen antes de considerar el servicio listo

## Referencias

- [AWS ECS CLI Reference](https://docs.aws.amazon.com/cli/latest/reference/ecs/)
- [AWS CloudWatch Logs CLI Reference](https://docs.aws.amazon.com/cli/latest/reference/logs/)
- Documentación del proyecto: `DEPLOY_README.md`

