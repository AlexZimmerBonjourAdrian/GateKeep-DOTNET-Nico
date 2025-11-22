# Configuración de Redis y RabbitMQ

## Estado Actual

### Redis (ElastiCache)
- ✅ Configuración en `redis.tf` - Lista para crear
- ✅ Security Group configurado - Permite conexión desde ECS (puerto 6379)
- ✅ Variables de entorno en ECS - Configuradas
- ⏳ Recurso NO creado aún en AWS

### RabbitMQ (Amazon MQ)
- ✅ Configuración en `mq.tf` - Lista para crear
- ✅ Security Group configurado - Permite AMQP (5671) y Management (443)
- ✅ Variables de entorno en ECS - Configuradas
- ✅ Secret de password existe en Secrets Manager
- ⏳ Recurso NO creado aún en AWS

## Variables de Entorno Configuradas en ECS

### Redis
- `REDIS_CONNECTION`: Endpoint de ElastiCache (se configurará automáticamente)
- `REDIS_INSTANCE`: "GateKeep:"

### RabbitMQ
- `RABBITMQ__HOST`: Endpoint de Amazon MQ (se configurará automáticamente)
- `RABBITMQ__PORT`: "5671" (SSL)
- `RABBITMQ__USE_SSL`: "true"
- `RABBITMQ__MANAGEMENT_PORT`: "443"
- `RABBITMQ__USE_HTTPS`: "true"
- `RABBITMQ__USERNAME`: "admin"
- `RABBITMQ__VIRTUALHOST`: "/"
- `RABBITMQ__PASSWORD`: Desde Secrets Manager

## Crear Recursos

Para crear Redis y RabbitMQ:

```bash
# Ver qué se va a crear
terraform plan -target=aws_elasticache_replication_group.main -target=aws_mq_broker.main

# Crear los recursos
terraform apply -target=aws_elasticache_replication_group.main -target=aws_mq_broker.main
```

## Verificar Conexión

Después de crear los recursos, verificar que ECS puede conectarse:

```bash
# Verificar Redis
aws elasticache describe-replication-groups --replication-group-id gatekeep-redis --region sa-east-1

# Verificar RabbitMQ
aws mq describe-broker --broker-id gatekeep-rabbitmq --region sa-east-1

# Verificar que ECS tiene las variables de entorno correctas
aws ecs describe-task-definition --task-definition gatekeep-api --region sa-east-1 --query "taskDefinition.containerDefinitions[0].environment" --output json
```

## Notas Importantes

1. **Redis** se creará en subnets privadas para mayor seguridad
2. **RabbitMQ** se creará en subnets privadas y NO será públicamente accesible
3. Ambos servicios usarán Security Groups que solo permiten conexión desde ECS
4. Las variables de entorno se actualizarán automáticamente cuando se creen los recursos

