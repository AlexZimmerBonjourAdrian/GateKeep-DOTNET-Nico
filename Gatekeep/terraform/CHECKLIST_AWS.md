# Checklist: Verificación de Recursos AWS para RabbitMQ y Redis

## Estado de Recursos en Terraform

### ✅ Recursos Definidos en Terraform (se crean automáticamente)

#### Redis (ElastiCache)
- ✅ `aws_elasticache_subnet_group.main` - Subnet group para Redis
- ✅ `aws_elasticache_parameter_group.main` - Parameter group con maxmemory-policy
- ✅ `aws_elasticache_replication_group.main` - Cluster Redis 7.0
- ✅ `aws_security_group.redis` - Security group permitiendo conexión desde ECS

#### RabbitMQ (Amazon MQ)
- ✅ `aws_mq_broker.main` - Broker RabbitMQ 3.11.20
- ✅ `aws_security_group.rabbitmq` - Security group para AMQP (5671) y Management (443)

#### ECS Configuration
- ✅ Variables de entorno configuradas:
  - `REDIS_CONNECTION` - Endpoint de ElastiCache
  - `REDIS_INSTANCE` - "GateKeep:"
  - `RABBITMQ__HOST` - Endpoint de Amazon MQ
  - `RABBITMQ__PORT` - "5671"
  - `RABBITMQ__USE_SSL` - "true"
  - `RABBITMQ__MANAGEMENT_PORT` - "443"
  - `RABBITMQ__USE_HTTPS` - "true"
  - `RABBITMQ__USERNAME` - "admin"
  - `RABBITMQ__VIRTUALHOST` - "/"

#### Permisos IAM
- ✅ `aws_iam_role_policy.ecs_execution_secrets` - Permite leer secretos (incluye RabbitMQ password)
- ✅ `aws_iam_role_policy.ecs_task_cloudwatch` - Permite enviar métricas de RabbitMQ

### ⚠️ Requisitos Previos (deben existir ANTES de aplicar Terraform)

#### Secrets Manager
1. **Secret de RabbitMQ Password** (REQUERIDO)
   - Nombre: `{project_name}/rabbitmq/password`
   - Debe existir ANTES de ejecutar `terraform apply`
   - Si no existe, crear con AWS CLI:

```bash
# Crear el secret (si no existe)
aws secretsmanager create-secret \
  --name gatekeep/rabbitmq/password \
  --description "Password para Amazon MQ RabbitMQ" \
  --region sa-east-1

# Agregar el valor del password
aws secretsmanager put-secret-value \
  --secret-id gatekeep/rabbitmq/password \
  --secret-string "TU_PASSWORD_AQUI" \
  --region sa-east-1
```

2. **Secret de MongoDB Connection** (OPCIONAL pero recomendado)
   - Nombre: `{project_name}/mongodb/connection`
   - Debe tener una versión con el connection string

3. **Secrets de DB y JWT** (OPCIONAL)
   - Si `manage_secret_versions = false` (default), Terraform espera que ya existan
   - Si `manage_secret_versions = true`, Terraform los crea automáticamente

## Comandos para Verificar y Crear

### 1. Verificar que AWS CLI está configurado

```bash
aws sts get-caller-identity --region sa-east-1
```

### 2. Verificar si el secret de RabbitMQ existe

```bash
aws secretsmanager describe-secret \
  --secret-id gatekeep/rabbitmq/password \
  --region sa-east-1
```

Si no existe, crearlo (ver arriba).

### 3. Aplicar Terraform

```bash
cd Gatekeep/terraform

# Inicializar Terraform
terraform init

# Verificar qué se va a crear
terraform plan

# Aplicar cambios (crear recursos)
terraform apply
```

### 4. Verificar que los recursos se crearon

```bash
# Verificar Redis
aws elasticache describe-replication-groups \
  --replication-group-id gatekeep-redis \
  --region sa-east-1

# Verificar RabbitMQ
aws mq describe-broker \
  --broker-id gatekeep-rabbitmq \
  --region sa-east-1
```

## Verificación con AWS CLI (Ejecutada)

### ✅ Estado Actual Verificado:

1. **AWS CLI Configurado** ✅
   - Usuario: `arn:aws:iam::126588786097:user/Alex`
   - Región: `sa-east-1`
   - Estado: Funcionando correctamente

2. **Secrets Manager** ✅
   - ✅ `gatekeep/db/password` - Existe
   - ✅ `gatekeep/jwt/key` - Existe
   - ✅ `gatekeep/rabbitmq/password` - Existe (creado el 2025-11-15)
   - ✅ `gatekeep/mongodb/connection` - Existe
   - **Todos los secrets requeridos están creados**

3. **ECS Cluster** ✅
   - ✅ Cluster: `gatekeep-cluster` - Existe
   - ✅ Servicio: `gatekeep-api-service` - Activo
   - ✅ Servicio: `gatekeep-frontend-service` - Activo
   - ✅ Task Definition: `gatekeep-api:4` - Existe

4. **Permisos IAM** ⚠️
   - ⚠️ No se tienen permisos para listar brokers de MQ directamente
   - ⚠️ No se tienen permisos para listar replication groups de ElastiCache directamente
   - ✅ Los permisos están configurados en Terraform para ECS

## Resumen

### ✅ TODO ESTÁ CONFIGURADO EN TERRAFORM

Todos los recursos necesarios para RabbitMQ y Redis están definidos en Terraform y se crearán automáticamente cuando ejecutes `terraform apply`.

### ✅ SECRETS MANAGER - TODO LISTO

**Todos los secrets requeridos ya existen en AWS:**
- ✅ `gatekeep/rabbitmq/password` - Creado y con valor
- ✅ `gatekeep/db/password` - Existe
- ✅ `gatekeep/jwt/key` - Existe
- ✅ `gatekeep/mongodb/connection` - Existe

### 📝 Notas Importantes

- El secret de RabbitMQ se lee como `data` source, por lo que **DEBE existir antes** de aplicar Terraform
- Si el secret no existe, Terraform fallará con un error
- Una vez creado el secret, Terraform puede crear todos los demás recursos automáticamente
- Los Security Groups están configurados para permitir conexión desde ECS
- Las variables de entorno están configuradas en la Task Definition de ECS

## Próximos Pasos

1. ✅ **COMPLETADO**: Verificar/crear el secret de RabbitMQ password (ya existe)
2. ⏭️ Ejecutar `terraform init` (si no se ha hecho)
3. ⏭️ Ejecutar `terraform plan` para revisar cambios
4. ⏭️ Ejecutar `terraform apply` para crear recursos (si no están creados)
5. ⏭️ Verificar que los recursos se crearon correctamente
6. ✅ **COMPLETADO**: Desplegar la aplicación en ECS (servicios ya están activos)

## Estado Final

### ✅ Configuración Completa

- ✅ AWS CLI configurado y funcionando
- ✅ Todos los secrets requeridos existen
- ✅ ECS Cluster y servicios activos
- ✅ Task Definition con variables de entorno configuradas
- ✅ Permisos IAM configurados en Terraform

### ⏭️ Pendiente (si los recursos no están creados)

- ⏭️ Ejecutar `terraform apply` para crear:
  - Redis (ElastiCache)
  - RabbitMQ (Amazon MQ)
  - Security Groups
  - Otras dependencias

**Nota**: Si los recursos de Redis y RabbitMQ ya están creados, no es necesario ejecutar `terraform apply` a menos que quieras actualizar la configuración.

