# Problemas y Soluciones - Redis y RabbitMQ

## Problema Principal Identificado

### ❌ El servicio ECS estaba usando task definition versión 4
- **Síntoma**: La aplicación intenta conectarse a `localhost` en lugar de los endpoints de AWS
- **Causa**: La task definition versión 4 no tiene las variables de entorno configuradas
- **Solución**: Actualizar el servicio para usar la task definition versión 7

## Estado de los Recursos

### ✅ Recursos AWS
- **Redis**: `available` - Endpoint disponible
- **RabbitMQ**: `RUNNING` - Endpoint disponible
- **Task Definition**: Versión 7 con todas las variables configuradas

### ✅ Configuración de Red
- **VPC**: Todos los recursos están en la misma VPC (`vpc-020bbcab6b221869d`)
- **Subnets ECS**: Públicas (10.0.1.0/24, 10.0.2.0/24)
- **Subnets Redis**: Privadas (misma VPC)
- **Subnet RabbitMQ**: Privada (misma VPC)
- **Conectividad**: ECS en subnets públicas puede comunicarse con recursos en subnets privadas dentro de la misma VPC

### ✅ Security Groups
- **Redis SG**: Permite puerto 6379 desde ECS Security Group
- **RabbitMQ SG**: Permite puertos 5671 (AMQP) y 443 (Management) desde ECS Security Group
- **Configuración**: Correcta

## Variables de Entorno Configuradas (Task Definition v7)

### Redis
- `REDIS_CONNECTION`: `gatekeep-redis.35dilc.ng.0001.sae1.cache.amazonaws.com:6379`
- `REDIS_INSTANCE`: `GateKeep:`

### RabbitMQ
- `RABBITMQ__HOST`: `b-e1ab35ba-c0f6-4846-83f1-cf14e97eb62a.mq.sa-east-1.on.aws`
- `RABBITMQ__PORT`: `5671`
- `RABBITMQ__USE_SSL`: `true`
- `RABBITMQ__MANAGEMENT_PORT`: `443`
- `RABBITMQ__USE_HTTPS`: `true`
- `RABBITMQ__USERNAME`: `admin`
- `RABBITMQ__VIRTUALHOST`: `/`
- `RABBITMQ__PASSWORD`: Desde Secrets Manager

## Solución Aplicada

1. ✅ **Actualizado servicio ECS** para usar task definition versión 7
2. ✅ **Forzado nuevo deployment** para aplicar los cambios
3. ⏳ **Esperando** a que el deployment termine (2-5 minutos)

## Verificación Post-Deployment

Después de que el deployment termine, verificar:

1. **Logs de la aplicación**:
   ```bash
   aws logs tail /ecs/gatekeep --region sa-east-1 --since 5m
   ```
   - Debe mostrar conexión a los endpoints correctos de AWS
   - No debe mostrar intentos de conexión a `localhost`

2. **Endpoints de health check**:
   - `/health` - Debe retornar 200 OK
   - `/health/redis` - Debe retornar 200 OK con información de conexión

3. **Logs de RabbitMQ**:
   - Debe mostrar conexión exitosa a RabbitMQ
   - No debe mostrar errores de conexión

## Comandos de Verificación

```bash
# Verificar estado del servicio
aws ecs describe-services --cluster gatekeep-cluster --services gatekeep-api-service --region sa-east-1

# Verificar logs
aws logs tail /ecs/gatekeep --region sa-east-1 --since 5m

# Probar endpoints
curl https://api.zimmzimmgames.com/health
curl https://api.zimmzimmgames.com/health/redis
```

## Notas Importantes

1. **Task Definition**: El servicio debe usar la versión 7 o superior
2. **Variables de Entorno**: Deben estar configuradas en la task definition
3. **Security Groups**: Deben permitir comunicación entre ECS y Redis/RabbitMQ
4. **VPC**: Todos los recursos deben estar en la misma VPC
5. **Tiempo de Deployment**: Puede tardar 2-5 minutos en completarse

## Próximos Pasos

1. ⏳ Esperar a que el deployment termine
2. ⏳ Verificar logs para confirmar conexiones exitosas
3. ⏳ Probar endpoints de health check
4. ⏳ Verificar que no haya errores en los logs

