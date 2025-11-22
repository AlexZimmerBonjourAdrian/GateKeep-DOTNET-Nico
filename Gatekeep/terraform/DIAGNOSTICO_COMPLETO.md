# Diagnóstico Completo - Redis y RabbitMQ

## Estado Actual

### ✅ Recursos AWS
- **Redis**: `available` - Endpoint: `gatekeep-redis.35dilc.ng.0001.sae1.cache.amazonaws.com:6379`
- **RabbitMQ**: `RUNNING` - Endpoint: `b-e1ab35ba-c0f6-4846-83f1-cf14e97eb62a.mq.sa-east-1.on.aws:5671`
- **Task Definition**: Versión 7 con todas las variables configuradas

### ✅ Configuración de Red
- **VPC**: Todos los recursos en la misma VPC (`vpc-020bbcab6b221869d`)
- **Security Groups**: Configurados correctamente
- **Subnets**: ECS en públicas, Redis/RabbitMQ en privadas (misma VPC)

### ✅ Variables de Entorno en Task Definition v7
- `REDIS_CONNECTION`: `gatekeep-redis.35dilc.ng.0001.sae1.cache.amazonaws.com:6379`
- `REDIS_INSTANCE`: `GateKeep:`
- `RABBITMQ__HOST`: `b-e1ab35ba-c0f6-4846-83f1-cf14e97eb62a.mq.sa-east-1.on.aws`
- `RABBITMQ__PORT`: `5671`
- `RABBITMQ__USE_SSL`: `true`
- `RABBITMQ__MANAGEMENT_PORT`: `443`
- `RABBITMQ__USE_HTTPS`: `true`
- `RABBITMQ__USERNAME`: `admin`
- `RABBITMQ__VIRTUALHOST`: `/`
- `RABBITMQ__PASSWORD`: Desde Secrets Manager

## Problemas Identificados

### Problema 1: La aplicación intenta conectarse a localhost
**Síntoma**: Logs muestran `Connection refused (localhost:15672)` y `rabbitmq://localhost/`

**Causa posible**:
1. El nuevo deployment aún no ha terminado completamente
2. Las variables de entorno no se están cargando correctamente en el contenedor
3. El código puede estar usando valores por defecto antes de leer las variables

**Solución**:
1. Esperar a que el deployment termine completamente (2-5 minutos)
2. Verificar logs después del deployment para ver si las variables se cargan
3. Verificar que el log muestre "RabbitMQ Settings configurado" con los valores correctos

### Problema 2: Variables de entorno pueden no estar cargándose
**Causa posible**:
- En .NET Core, las variables de entorno con doble guion bajo (`RABBITMQ__HOST`) se convierten automáticamente a configuración anidada (`RABBITMQ:HOST`)
- El código lee primero desde `builder.Configuration["RABBITMQ:HOST"]` y luego desde `Environment.GetEnvironmentVariable("RABBITMQ__HOST")`
- Puede haber un problema con el orden de lectura o con cómo se cargan las variables

**Solución**:
- Verificar que las variables se estén pasando correctamente al contenedor
- Verificar los logs para ver qué valores está usando la aplicación

## Verificaciones Necesarias

### 1. Verificar que el deployment haya terminado
```bash
aws ecs describe-services --cluster gatekeep-cluster --services gatekeep-api-service --region sa-east-1
```
- `RunningCount` debe ser igual a `DesiredCount`
- `PendingCount` debe ser 0

### 2. Verificar logs de configuración
```bash
aws logs tail /ecs/gatekeep --region sa-east-1 --since 5m | grep -i "RabbitMQ Settings configurado"
```
- Debe mostrar el host correcto de AWS, no `localhost`
- Debe mostrar `ManagementPort: 443` y `UseHttps: true`

### 3. Verificar logs de conexión
```bash
aws logs tail /ecs/gatekeep --region sa-east-1 --since 5m | grep -i "Configurando RabbitMQ"
```
- Debe mostrar el host correcto de AWS
- No debe mostrar `localhost`

### 4. Probar endpoints
```bash
curl https://api.zimmzimmgames.com/health/redis
```
- Debe retornar 200 OK con información de conexión

## Soluciones Aplicadas

1. ✅ **Actualizado servicio ECS** para usar task definition versión 7
2. ✅ **Forzado nuevo deployment** para aplicar los cambios
3. ⏳ **Esperando** a que el deployment termine completamente

## Próximos Pasos

1. ⏳ **Esperar** a que el deployment termine (puede tardar 2-5 minutos)
2. ⏳ **Verificar logs** para confirmar que las variables se cargan correctamente
3. ⏳ **Probar endpoints** para verificar conexiones
4. ⏳ **Si persiste el problema**, verificar:
   - Que las variables de entorno se estén pasando correctamente al contenedor
   - Que el código esté leyendo las variables en el orden correcto
   - Que no haya problemas con el formato de las variables

## Notas Importantes

- El deployment puede tardar 2-5 minutos en completarse
- Las tareas antiguas pueden seguir ejecutándose mientras se despliega la nueva versión
- Los logs pueden mostrar errores de las tareas antiguas hasta que se detengan
- Es importante esperar a que todas las tareas antiguas se detengan y solo queden las nuevas

