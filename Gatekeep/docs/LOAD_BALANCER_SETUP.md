# Configuración del Load Balancer - GateKeep

## Introducción

Este documento describe la configuración mejorada del Application Load Balancer (ALB) para GateKeep, diseñada para proporcionar alta disponibilidad, escalabilidad y seguridad para la aplicación PWA universitaria.

## Características Implementadas

### 1. Application Load Balancer (ALB) Mejorado

#### Configuración Avanzada
- **HTTP/2 habilitado**: Mejora el rendimiento de las conexiones
- **Cross-Zone Load Balancing**: Distribuye el tráfico de manera uniforme entre todas las zonas de disponibilidad
- **Idle Timeout configurable**: Controla cuánto tiempo mantener conexiones inactivas (por defecto: 60 segundos)
- **Deletion Protection**: Habilitado en producción para prevenir eliminaciones accidentales

#### Access Logs
- **Bucket S3 dedicado**: Todos los logs de acceso del ALB se almacenan en S3
- **Retención**: Los logs se mantienen por 90 días con política de lifecycle automática
- **Encriptación**: Los logs están encriptados con AES256
- **Auditoría**: Los logs permiten análisis de tráfico, debugging y cumplimiento

### 2. Target Groups Mejorados

#### Backend API (Puerto 5011)
- **Health Checks robustos** (Cumple con requerimiento 3.10):
  - Timeout: 10 segundos
  - Interval: 30 segundos
  - Healthy threshold: 2
  - Unhealthy threshold: 3
  - Path: `/ready` (Readiness probe - verifica dependencias críticas)
  - **Endpoints disponibles**:
    - `/health` - Health check general (retorna OK si la app está funcionando)
    - `/ready` - Readiness probe (verifica PostgreSQL, MongoDB, Redis - retorna 200 si ready, 503 si no)
    - `/live` - Liveness probe (verifica que el proceso esté vivo - usado por ECS container health check)
- **Connection Draining**: 60 segundos para permitir graceful shutdown completo
- **Sticky Sessions**: Deshabilitado por defecto (API stateless con JWT)
- **Graceful Shutdown**: Configurado para despliegues sin interrupción

#### Frontend PWA (Puerto 3000)
- **Health Checks optimizados para PWA**:
  - Acepta códigos HTTP: 200, 404, 301, 302 (compatible con redirecciones de Next.js)
  - Timeout: 10 segundos
  - Interval: 30 segundos
  - Path: `/` (root endpoint)
- **Connection Draining**: 60 segundos para graceful shutdown
- **Sticky Sessions**: Deshabilitado

### 3. Auto Scaling

#### Configuración de Auto Scaling para ECS

El sistema de auto scaling está configurado para escalar automáticamente las tareas ECS basándose en múltiples métricas:

**Backend API:**
- **CPU Utilization**: Escala cuando el uso promedio de CPU > 70%
- **Memory Utilization**: Escala cuando el uso promedio de memoria > 80%
- **ALB Request Count**: Escala cuando hay > 1000 requests por target

**Frontend:**
- **CPU Utilization**: Escala cuando el uso promedio de CPU > 70%
- **Memory Utilization**: Escala cuando el uso promedio de memoria > 80%
- **ALB Request Count**: Escala cuando hay > 1000 requests por target

**Cooldowns:**
- **Scale Out**: 60 segundos (rápido para responder a picos de tráfico)
- **Scale In**: 300 segundos (5 minutos para evitar escalado excesivo)

**Límites:**
- **Backend**: Mínimo 2, Máximo 10 tareas (configurable)
- **Frontend**: Mínimo 2, Máximo 5 tareas (configurable)

### 4. Web Application Firewall (WAF) - Opcional

El WAF proporciona protección adicional contra ataques comunes:

**Reglas Implementadas:**
- **AWS Managed Rules - Common Rule Set**: Protección contra SQL injection, XSS, etc.
- **AWS Managed Rules - Known Bad Inputs**: Bloquea inputs maliciosos conocidos
- **AWS Managed Rules - Linux Rule Set**: Protección específica para sistemas Linux
- **Rate Limiting**: Limita a 2000 requests por IP cada 5 minutos (protección DDoS)

**Habilitación:**
```hcl
enable_waf = true  # En variables.tf o terraform.tfvars
```

**Logging:**
- Los eventos del WAF se registran en CloudWatch Logs
- Retención: 30 días

### 5. Alta Disponibilidad

#### Múltiples Instancias
- **Backend**: Mínimo 2 instancias en diferentes zonas de disponibilidad
- **Frontend**: Mínimo 2 instancias en diferentes zonas de disponibilidad
- **ALB**: Distribuido en 2 subnets públicas en diferentes zonas

#### Health Checks
- Health checks continuos en todos los targets
- Eliminación automática de targets no saludables
- Reincorporación automática cuando se recuperan

### 6. Cumplimiento del Requerimiento 3.10 - Balanceo de Carga

#### ✅ API Stateless
- **Implementación**: La aplicación usa autenticación JWT (stateless)
- **Sin sesión local**: No hay dependencia de sesión local, todas las instancias pueden manejar cualquier request
- **Sin sticky sessions**: No se requieren sticky sessions, mejor distribución de carga

#### ✅ Health Checks Múltiples
- **`/health`**: Health check general - retorna OK si la aplicación está funcionando
- **`/ready`**: Readiness probe - verifica dependencias críticas (PostgreSQL, MongoDB, Redis)
  - Retorna 200 si todas las dependencias están listas
  - Retorna 503 si alguna dependencia crítica no está disponible
  - Usado por el ALB Target Group para health checks
- **`/live`**: Liveness probe - verifica que el proceso esté vivo
  - Retorna 200 si el proceso está funcionando
  - Usado por ECS container health check
  - No verifica dependencias, solo que el proceso responda

#### ✅ Graceful Shutdown
- **Connection Draining**: 60 segundos de delay antes de remover un target del ALB
- **Stop Timeout**: 30 segundos para que el contenedor termine gracefulmente
- **Rolling Updates**: 
  - `deployment_maximum_percent = 200` - Permite tener hasta 2x la capacidad durante despliegues
  - `deployment_minimum_healthy_percent = 100` - Mantiene 100% de capacidad saludable
- **Proceso de despliegue sin interrupción**:
  1. Nueva tarea se inicia y pasa health checks (`/live` y `/ready`)
  2. Nueva tarea se registra en el Target Group
  3. ALB comienza a enviar tráfico a la nueva tarea
  4. Tarea antigua deja de recibir tráfico nuevo (deregistration)
  5. Espera 60 segundos para que las conexiones existentes terminen
  6. Tarea antigua se detiene gracefulmente (30 segundos stop timeout)
  7. Proceso completo sin interrupción del servicio

#### ✅ Data Protection (No aplica)
- **Razón**: La aplicación usa JWT (stateless), no hay sesiones por cookies
- **Si en el futuro se requieren sesiones por cookies**:
  - Configurar Data Protection compartido usando Redis o S3
  - O habilitar sticky sessions en el Target Group (justificado)

## Variables de Configuración

### Variables Principales

```hcl
# Número de instancias ECS
ecs_desired_count = 2              # Backend (mínimo recomendado: 2)
ecs_frontend_desired_count = 2     # Frontend (mínimo recomendado: 2)

# Límites de auto scaling
ecs_max_capacity = 10              # Máximo de instancias backend
ecs_frontend_max_capacity = 5      # Máximo de instancias frontend

# Configuración del ALB
alb_idle_timeout = 60              # Segundos (1-4000)

# WAF (opcional)
enable_waf = false                 # Habilitar WAF para protección adicional
```

### Configuración Recomendada por Ambiente

#### Desarrollo
```hcl
ecs_desired_count = 1
ecs_frontend_desired_count = 1
ecs_max_capacity = 3
ecs_frontend_max_capacity = 2
enable_waf = false
```

#### Producción
```hcl
ecs_desired_count = 2
ecs_frontend_desired_count = 2
ecs_max_capacity = 10
ecs_frontend_max_capacity = 5
enable_waf = true
alb_idle_timeout = 60
```

## Monitoreo

### CloudWatch Metrics

El ALB expone automáticamente métricas en CloudWatch:
- **RequestCount**: Número total de requests
- **TargetResponseTime**: Tiempo de respuesta promedio
- **HTTPCode_Target_2XX_Count**: Requests exitosos
- **HTTPCode_Target_4XX_Count**: Errores del cliente
- **HTTPCode_Target_5XX_Count**: Errores del servidor
- **HealthyHostCount**: Número de targets saludables
- **UnHealthyHostCount**: Número de targets no saludables

### Access Logs

Los logs de acceso del ALB están disponibles en:
```
s3://gatekeep-alb-logs-{environment}-{account-id}/alb-access-logs/
```

Formato de logs: [Formato estándar de ALB](https://docs.aws.amazon.com/elasticloadbalancing/latest/application/load-balancer-access-logs.html)

### WAF Logs (si está habilitado)

Los logs del WAF están disponibles en CloudWatch Logs:
```
/aws/waf/gatekeep
```

## Despliegue

### Pasos para Aplicar la Configuración

1. **Revisar variables** en `terraform/variables.tf` o crear `terraform.tfvars`:
   ```hcl
   ecs_desired_count = 2
   ecs_frontend_desired_count = 2
   enable_waf = true  # Para producción
   ```

2. **Inicializar Terraform** (si es necesario):
   ```bash
   cd terraform
   terraform init
   ```

3. **Planificar cambios**:
   ```bash
   terraform plan -out=tfplan
   ```

4. **Aplicar cambios**:
   ```bash
   terraform apply tfplan
   ```

### Verificación Post-Despliegue

1. **Verificar estado del ALB**:
   ```bash
   aws elbv2 describe-load-balancers --names gatekeep-alb
   ```

2. **Verificar Target Groups**:
   ```bash
   aws elbv2 describe-target-health --target-group-arn <target-group-arn>
   ```

3. **Verificar Auto Scaling**:
   ```bash
   aws application-autoscaling describe-scalable-targets --service-namespace ecs
   ```

4. **Probar endpoints**:
   ```bash
   curl https://api.zimmzimmgames.com/health
   curl https://zimmzimmgames.com/
   ```

## Troubleshooting

### Targets no saludables

**Síntoma**: Targets aparecen como "unhealthy" en el Target Group

**Soluciones**:
1. Verificar que el health check endpoint (`/health`) esté funcionando
2. Verificar security groups (ALB debe poder comunicarse con ECS)
3. Verificar logs de ECS en CloudWatch
4. Aumentar el timeout del health check si es necesario

### Auto Scaling no funciona

**Síntoma**: Las instancias no escalan automáticamente

**Soluciones**:
1. Verificar que las métricas de CloudWatch estén disponibles
2. Verificar los límites de auto scaling (min/max)
3. Verificar los cooldowns (pueden estar en período de espera)
4. Revisar logs de Application Auto Scaling en CloudWatch

### WAF bloquea requests legítimos

**Síntoma**: Requests válidos son bloqueados por el WAF

**Soluciones**:
1. Revisar logs del WAF en CloudWatch Logs
2. Ajustar reglas del WAF según necesidades
3. Agregar excepciones para IPs o paths específicos
4. Considerar deshabilitar reglas específicas si causan falsos positivos

## Costos Estimados

### ALB
- **Costo base**: ~$16.20/mes (ALB)
- **LCU (Load Balancer Capacity Units)**: Variable según tráfico
  - ~$0.008 por LCU-hora
  - Estimación: 10-50 LCU-hora/mes para tráfico moderado

### Auto Scaling
- **Sin costo adicional**: El auto scaling no tiene costo, solo paga por las instancias ECS que se crean

### WAF
- **Web ACL**: $5/mes
- **Requests**: $1.00 por millón de requests
- **Reglas**: $1.00 por regla por millón de requests

### S3 para Logs
- **Storage**: ~$0.023/GB/mes
- **Requests**: PUT requests (~$0.005 por 1000 requests)

**Estimación total mensual** (tráfico moderado):
- ALB: ~$20-30/mes
- WAF (si habilitado): ~$10-15/mes
- S3 Logs: ~$1-2/mes
- **Total**: ~$31-47/mes (sin WAF) o ~$41-62/mes (con WAF)

## Mejores Prácticas

1. **Siempre usar múltiples instancias** en producción (mínimo 2)
2. **Habilitar WAF en producción** para protección adicional
3. **Monitorear métricas de CloudWatch** regularmente
4. **Revisar access logs** periódicamente para detectar patrones anómalos
5. **Ajustar auto scaling** según patrones de tráfico reales
6. **Probar health checks** antes de desplegar a producción:
   - Verificar que `/health` retorna 200
   - Verificar que `/ready` retorna 200 cuando todas las dependencias están disponibles
   - Verificar que `/ready` retorna 503 cuando alguna dependencia crítica falla
   - Verificar que `/live` siempre retorna 200 si el proceso está vivo
7. **Usar deletion protection** en producción
8. **Configurar alertas** en CloudWatch para métricas críticas
9. **Verificar graceful shutdown** durante despliegues:
   - Monitorear que no hay errores 502/503 durante despliegues
   - Verificar que las conexiones existentes terminan correctamente
10. **Mantener API stateless**: No usar sesiones locales, siempre usar JWT o tokens stateless

## Referencias

- [AWS Application Load Balancer Documentation](https://docs.aws.amazon.com/elasticloadbalancing/latest/application/)
- [AWS Auto Scaling Documentation](https://docs.aws.amazon.com/autoscaling/)
- [AWS WAF Documentation](https://docs.aws.amazon.com/waf/)
- [Terraform AWS Provider - ALB](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/lb)

