# Pruebas de Redis y RabbitMQ

## Estado de los Recursos

### Redis (ElastiCache)
- ✅ **Estado**: `available`
- ✅ **Endpoint**: `gatekeep-redis.35dilc.ng.0001.sae1.cache.amazonaws.com:6379`
- ✅ **Puerto**: `6379`
- ✅ **Importado en Terraform**: Sí

### RabbitMQ (Amazon MQ)
- ✅ **Estado**: `RUNNING`
- ✅ **Broker ID**: `b-e1ab35ba-c0f6-4846-83f1-cf14e97eb62a`
- ✅ **Endpoint AMQP**: `b-e1ab35ba-c0f6-4846-83f1-cf14e97eb62a.mq.sa-east-1.on.aws:5671`
- ✅ **Console URL**: `https://b-e1ab35ba-c0f6-4846-83f1-cf14e97eb62a.mq.sa-east-1.on.aws`
- ✅ **Versión**: `3.13.7`

## Configuración en ECS

### Variables de Entorno Configuradas

#### Redis
- `REDIS_CONNECTION`: `gatekeep-redis.35dilc.ng.0001.sae1.cache.amazonaws.com:6379`
- `REDIS_INSTANCE`: `GateKeep:`

#### RabbitMQ
- `RABBITMQ__HOST`: `b-e1ab35ba-c0f6-4846-83f1-cf14e97eb62a.mq.sa-east-1.on.aws`
- `RABBITMQ__PORT`: `5671`
- `RABBITMQ__USE_SSL`: `true`
- `RABBITMQ__MANAGEMENT_PORT`: `443`
- `RABBITMQ__USE_HTTPS`: `true`
- `RABBITMQ__USERNAME`: `admin`
- `RABBITMQ__VIRTUALHOST`: `/`
- `RABBITMQ__PASSWORD`: Desde Secrets Manager (`gatekeep/rabbitmq/password`)

## Security Groups

### Redis Security Group
- **Puerto**: `6379` (TCP)
- **Origen**: ECS Security Group
- **Estado**: Configurado

### RabbitMQ Security Group
- **Puerto AMQP**: `5671` (TCP) - Desde ECS
- **Puerto Management**: `443` (TCP) - Desde ECS
- **Estado**: Configurado

## Pruebas de Conexión

### Desde la Aplicación (.NET)

La aplicación tiene endpoints de health check:

1. **Redis Health Check**
   ```
   GET /health/redis
   ```
   - Verifica conexión a Redis
   - Retorna estado de conexión y endpoints

2. **RabbitMQ**
   - La conexión se verifica automáticamente al iniciar MassTransit
   - Los logs mostrarán si la conexión es exitosa

### Verificar desde ECS

```bash
# Ver logs de ECS
aws logs tail /aws/ecs/gatekeep-api --follow --region sa-east-1

# Verificar que las variables de entorno estén correctas
aws ecs describe-task-definition --task-definition gatekeep-api --region sa-east-1
```

### Verificar Conexiones Manualmente

#### Redis
```bash
# Desde un contenedor en ECS (si tienes acceso)
redis-cli -h gatekeep-redis.35dilc.ng.0001.sae1.cache.amazonaws.com -p 6379 PING
```

#### RabbitMQ
```bash
# Verificar desde la consola de administración
# URL: https://b-e1ab35ba-c0f6-4846-83f1-cf14e97eb62a.mq.sa-east-1.on.aws
# Usuario: admin
# Password: Desde Secrets Manager
```

## Comandos de Verificación

### Verificar Estado de Recursos
```bash
# Redis
aws elasticache describe-replication-groups --replication-group-id gatekeep-redis --region sa-east-1

# RabbitMQ
aws mq describe-broker --broker-id b-e1ab35ba-c0f6-4846-83f1-cf14e97eb62a --region sa-east-1
```

### Verificar Security Groups
```bash
# Redis
aws ec2 describe-security-groups --filters "Name=group-name,Values=gatekeep-redis-sg" --region sa-east-1

# RabbitMQ
aws ec2 describe-security-groups --filters "Name=group-name,Values=gatekeep-rabbitmq-sg" --region sa-east-1
```

### Verificar Task Definition
```bash
aws ecs describe-task-definition --task-definition gatekeep-api --region sa-east-1 --query "taskDefinition.containerDefinitions[0].environment" --output json
```

## Notas Importantes

1. **Redis**: El endpoint puede tardar unos minutos en estar completamente disponible después de la creación
2. **RabbitMQ**: El broker está en modo SINGLE_INSTANCE para reducir costos
3. **SSL/TLS**: Ambos servicios usan conexiones seguras (Redis sin SSL por defecto en ElastiCache, RabbitMQ con SSL en puerto 5671)
4. **Management API**: RabbitMQ Management API está disponible en HTTPS (puerto 443)

## Resultados de las Pruebas

### ✅ Recursos AWS
- **Redis**: Estado `available`, endpoint disponible
- **RabbitMQ**: Estado `RUNNING`, endpoint disponible, versión 3.13.7

### ✅ Security Groups
- **Redis**: Regla configurada para puerto 6379 desde ECS Security Group (`sg-0ac373e020d6211ed`)
- **RabbitMQ**: Reglas configuradas para puertos 5671 (AMQP) y 443 (Management) desde ECS Security Group (`sg-030d0ad634f7432af`)

### ✅ Configuración
- **Task Definition**: Actualizada con variables de entorno correctas
- **Secrets Manager**: Password de RabbitMQ configurado
- **Terraform**: Estado sincronizado con recursos en AWS

## Próximos Pasos

1. ✅ Recursos creados
2. ✅ Configuración actualizada en ECS
3. ⏳ Actualizar servicio ECS para usar nueva task definition
4. ⏳ Verificar conexiones desde la aplicación
5. ⏳ Probar health checks (`/health/redis`)
6. ⏳ Verificar logs de la aplicación

### Actualizar Servicio ECS

Para aplicar la nueva task definition al servicio ECS:

```bash
aws ecs update-service --cluster gatekeep-cluster --service gatekeep-api --force-new-deployment --region sa-east-1
```

