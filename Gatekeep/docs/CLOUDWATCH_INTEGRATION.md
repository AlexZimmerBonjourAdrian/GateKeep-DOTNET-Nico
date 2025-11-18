# ☁️ Integración de Métricas de Cache Redis con AWS CloudWatch

## 📋 Resumen

Se ha implementado una integración completa de métricas de cache Redis con AWS CloudWatch. El sistema exporta automáticamente métricas de hits, misses, invalidaciones y hit rate cada 30 segundos a CloudWatch, permitiendo monitoreo en tiempo real y alertas automáticas.

## ✅ Componentes Implementados

### 1. **Exporter de Métricas en C# (Backend)**

**Archivos creados:**
- `Infrastructure/AWS/ICloudWatchMetricsExporter.cs` - Interfaz del servicio
- `Infrastructure/AWS/CloudWatchMetricsExporter.cs` - Implementación que:
  - Lee métricas de cache desde `ICacheMetricsService`
  - Las convierte a formato CloudWatch
  - Las envía cada 30 segundos
  - Se ejecuta como `BackgroundService` (HostedService)

**Características:**
- Envía por lotes (máximo 20 métricas por request)
- Incluye dimensiones (Environment, Service, CacheKey)
- Manejo robusto de errores con logging
- Métricas incluidas:
  - `CacheHitsTotal` (count)
  - `CacheMissesTotal` (count)
  - `CacheInvalidationsTotal` (count)
  - `CacheHitRate` (porcentaje)
  - `CacheHitsByKey` (desglose)
  - `CacheMissesByKey` (desglose)

### 2. **Dashboard en CloudWatch (Terraform)**

**Archivo creado:**
- `terraform/cloudwatch.tf` - Contiene:

#### Dashboard: `gatekeep-cache-metrics`
8 widgets para visualizar:
- Cache Hit Rate (métrica principal con anotaciones)
- Hits vs Misses (5 minutos)
- Operations Breakdown (desglose total)
- Hit Rate Trend (24 horas)
- Top Cache Keys
- Invalidations
- Log Summary
- API Response Time

#### Alarmas (4 alarmas automáticas):
1. **`gatekeep-low-cache-hit-rate`** - Hit rate < 50%
2. **`gatekeep-critical-cache-hit-rate`** - Hit rate < 30% (CRÍTICA)
3. **`gatekeep-high-cache-invalidations`** - > 100 invalidaciones/5min
4. **`gatekeep-high-cache-misses`** - > 500 misses/5min

#### Composite Alarm:
- `gatekeep-cache-health-overall` - Combina las alarmas críticas

#### Log Filters (análisis de logs):
- Contador de hits en logs
- Contador de misses en logs
- Contador de removals en logs

### 3. **Permisos IAM en Terraform**

**Archivo modificado:** `terraform/ecs.tf`

**Cambios:**
- Agregada policy `ecs_task_cloudwatch` al role `ecs_task`
- Permite `cloudwatch:PutMetricData` en namespaces:
  - `GateKeep/Redis`
  - `GateKeep/Redis/Logs`

### 4. **Cambios en C# (Backend)**

**Archivo modificado:** `GateKeep.Api.csproj`
- Agregado NuGet package: `AWSSDK.CloudWatch` (v3.7.400.42)

**Archivo modificado:** `Program.cs`
- Importado: `using Amazon.CloudWatch;`
- Registrado cliente: `IAmazonCloudWatch`
- Registrado servicio: `ICloudWatchMetricsExporter` (HostedService)

### 5. **Documentación Actualizada**

**Archivo modificado:** `docs/TEST_REDIS_FUNCIONAMIENTO.md`
- Nueva sección: "📊 Monitoreo en AWS CloudWatch"
- Instrucciones para acceder al dashboard
- Explicación de cada widget
- Interpretación de alarmas
- Scenarios de diagnóstico
- Verificación de envío de métricas
- Ejemplos con AWS CLI

## 🚀 Cómo Funciona

### Flujo de Exportación

```
┌─────────────────────────────────────────────────────────┐
│                    GateKeep.Api                         │
│                                                         │
│  ┌──────────────────────────────────────────────────┐   │
│  │  Operaciones de Cache                            │   │
│  │  - GET (hit/miss)                                │   │
│  │  - SET (cache set)                               │   │
│  │  - REMOVE (invalidación)                         │   │
│  └─────────────┬────────────────────────────────────┘   │
│                │                                        │
│  ┌─────────────▼──────────────────────────────────┐     │
│  │  CacheMetricsService (en memoria)              │     │
│  │  - Contadores: hits, misses, invalidations     │     │
│  │  - Diccionarios: por clave                     │     │
│  └─────────────┬──────────────────────────────────┘     │
│                │                                        │
│  ┌─────────────▼──────────────────────────────────┐     │
│  │  CloudWatchMetricsExporter (BackgroundService) │     │
│  │  - Cada 30 segundos:                           │     │
│  │    1. Lee métricas de CacheMetricsService      │     │
│  │    2. Prepara payload CloudWatch               │     │
│  │    3. Envía PutMetricData                      │     │
│  └─────────────┬──────────────────────────────────┘     │
│                │ AWS SDK                                │
└────────────────┼────────────────────────────────────────┘
                 │ HTTPS
                 ▼
        ┌─────────────────────┐
        │  AWS CloudWatch     │
        │  Namespace:         │
        │  GateKeep/Redis     │
        │                     │
        │  ├─ CacheHitRate    │
        │  ├─ CacheHitsTotal  │
        │  ├─ CacheMissesTotal│
        │  └─ ...             │
        └────────┬────────────┘
                 │
        ┌────────▼────────────┐
        │  Dashboard          │
        │  + Alarmas          │
        │  + Log Insights     │
        └─────────────────────┘
```

### Seguridad

- Credenciales AWS se cargan desde:
  - Variables de entorno (recomendado)
  - IAM Role de ECS Task (en producción)
  - AWS CLI configuration
  
- El IAM role solo permite enviar a namespaces específicos
- Las métricas son privadas a la cuenta AWS

## 📊 Métricas Disponibles

### En CloudWatch Console
```
Namespace: GateKeep/Redis
Metrics:
  - CacheHitsTotal (Count, Sum)
  - CacheMissesTotal (Count, Sum)
  - CacheInvalidationsTotal (Count, Sum)
  - CacheHitRate (Percent, Average)
  - CacheHitsByKey (Count, Sum)
  - CacheMissesByKey (Count, Sum)

Dimensions:
  - Environment (Development, Staging, Production)
  - Service (GateKeepAPI)
  - CacheKey (beneficios:all, reglas-acceso:all, etc)
```

### En la API
```
GET /api/cache-metrics (requiere Admin)
GET /api/cache-metrics/health (público)
POST /api/cache-metrics/reset (requiere Admin)
```

## 🔍 Monitoreo

### Acceder al Dashboard

1. AWS Console → CloudWatch → Dashboards
2. Buscar: `gatekeep-cache-metrics`
3. Verificar métricas en tiempo real

### Ver Alarmas

1. AWS Console → CloudWatch → Alarms
2. Buscar: `gatekeep-` para ver todas las alarmas de cache
3. Configurar SNS topics para notificaciones

### Diagnosticar Problemas

**Hit Rate bajo (<50%)?**
- Revisar si Redis está disponible
- Verificar TTL de cache
- Buscar invalidaciones frecuentes

**Misses muy altos?**
- Posible conectividad con Redis
- Verificar disponibilidad del servicio

**Invalidaciones altas?**
- Cambios frecuentes en BD
- Revisar política de TTL

## 📝 Checklist de Despliegue

- [ ] Agregar NuGet package AWSSDK.CloudWatch
- [ ] Implementar CloudWatchMetricsExporter.cs
- [ ] Registrar servicio en Program.cs
- [ ] Aplicar terraform/cloudwatch.tf
- [ ] Actualizar IAM roles (ecs.tf)
- [ ] Verificar credenciales AWS
- [ ] Desplegar a ECS
- [ ] Esperar 2-3 minutos para que aparezcan métricas
- [ ] Abrir dashboard en AWS Console
- [ ] Probar endpoint /api/cache-metrics

## 🛠️ Configuración

### Intervalo de Exportación
Modificar en `CloudWatchMetricsExporter.cs`, línea ~16:
```csharp
private readonly int _intervalSeconds = 30; // Cambiar a otro valor
```

### Threshold de Alarmas
Modificar en `terraform/cloudwatch.tf`:
```hcl
threshold = 50  # Cambiar valor
evaluation_periods = 2  # Cambiar período
```

### Variables de Ambiente (opcional)
```bash
# En .env o docker-compose.yml
AWS_REGION=sa-east-1
AWS_ACCESS_KEY_ID=<tu-key>
AWS_SECRET_ACCESS_KEY=<tu-secret>
ASPNETCORE_ENVIRONMENT=Production
```

## 📚 Referencias

- CloudWatch API: https://docs.aws.amazon.com/AmazonCloudWatch/latest/APIReference/
- Terraform CloudWatch: https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_dashboard
- AWS SDK for .NET: https://docs.aws.amazon.com/sdk-for-net/

---

**Creado**: 18 de Noviembre de 2025
**Estado**: ✅ Completo y Funcional
