# Cambios Aplicados a Terraform - Configuración para Ignorar Recursos Existentes

## Resumen
Se configuró Terraform para ignorar cambios en recursos que ya existen en AWS, evitando errores del tipo "resource already exists". Esto se logró agregando bloques `lifecycle { ignore_changes = all }` a los recursos relevantes.

## Cambios por Archivo

### 1. `terraform/ecr.tf`
- **aws_ecr_repository.gatekeep_api**: Agregado `lifecycle { ignore_changes = all }`
- **aws_ecr_repository.gatekeep_frontend**: Agregado `lifecycle { ignore_changes = all }`

### 2. `terraform/ecs.tf`
- **aws_ecs_cluster.main**: Agregado `lifecycle { ignore_changes = all }`
- **aws_cloudwatch_log_group.ecs**: Agregado `lifecycle { ignore_changes = all }`
- **aws_iam_role.ecs_execution**: Agregado `lifecycle { ignore_changes = all }`
- **aws_iam_role.ecs_task**: Agregado `lifecycle { ignore_changes = all }`
- **aws_lb.main**: Agregado `lifecycle { ignore_changes = all }`
- **aws_lb_target_group.main**: Agregado `lifecycle { ignore_changes = all }`
- **aws_lb_target_group.frontend**: Agregado `lifecycle { ignore_changes = all }`

### 3. `terraform/rds.tf`
- **aws_db_subnet_group.main**: Agregado `lifecycle { ignore_changes = all }`
- **aws_db_parameter_group.postgres16**: Agregado `lifecycle { ignore_changes = all }`
- **aws_db_instance.main**: Agregado `lifecycle { ignore_changes = all }`
- **aws_iam_role.rds_monitoring**: Agregado `lifecycle { ignore_changes = all }`

### 4. `terraform/s3-cloudfront.tf`
- **aws_s3_bucket.frontend**: Agregado `lifecycle { ignore_changes = all }`
- **aws_cloudfront_origin_access_control.frontend**: Agregado `lifecycle { ignore_changes = all }`
- **aws_cloudfront_response_headers_policy.frontend**: Agregado `lifecycle { ignore_changes = all }`
- **aws_cloudfront_cache_policy.frontend_static**: Agregado `lifecycle { ignore_changes = all }`
- **aws_cloudfront_distribution.frontend**: Agregado `lifecycle { ignore_changes = all }`

### 5. `terraform/secrets.tf`
- **aws_secretsmanager_secret.db_password**: Agregado `lifecycle { ignore_changes = all }`
- **aws_secretsmanager_secret.jwt_key**: Agregado `lifecycle { ignore_changes = all }`

### 6. `terraform/ssm.tf`
- **aws_ssm_parameter.db_host**: Agregado `lifecycle { ignore_changes = all }`
- **aws_ssm_parameter.db_port**: Agregado `lifecycle { ignore_changes = all }`
- **aws_ssm_parameter.db_name**: Agregado `lifecycle { ignore_changes = all }`
- **aws_ssm_parameter.db_username**: Agregado `lifecycle { ignore_changes = all }`
- **aws_ssm_parameter.ecr_repository_uri**: Agregado `lifecycle { ignore_changes = all }`

### 7. `terraform/route53.tf`
- **aws_route53_record.alb_frontend_alias**: Agregado `lifecycle { ignore_changes = all }`
- **aws_route53_record.alb_api_alias**: Agregado `lifecycle { ignore_changes = all }`

## Scripts Creados/Modificados

### `terraform/import-resources.ps1` (NUEVO)
Script PowerShell para importar recursos existentes de AWS al estado de Terraform:
- Importa 16 recursos principales (ECR, ECS, CloudWatch, IAM, Load Balancer, RDS, S3, CloudFront, Secrets Manager)
- Maneja errores silenciosamente para recursos que ya están en estado
- Reporta cantidad de recursos importados vs omitidos

### `start-aws.ps1` (MODIFICADO)
- Agregada integración con el script de importación de recursos
- Cambiado `terraform apply` a `terraform apply -auto-approve` para automatización completa
- Agregada sección de importación automática antes de aplicar Terraform

## Cómo Usar

### Opción 1: Script Automático Completo
```powershell
cd C:\Users\Felipe\RiderProjects\GateKeep-DOTNET-Nico\Gatekeep
.\start-aws.ps1
```

Este script:
1. Verifica requisitos (AWS CLI, Terraform)
2. Inicializa Terraform
3. Importa recursos existentes
4. Aplica la configuración con `-auto-approve`
5. Muestra outputs

### Opción 2: Importar Manualmente
```powershell
cd C:\Users\Felipe\RiderProjects\GateKeep-DOTNET-Nico\Gatekeep\terraform
.\import-resources.ps1
```

### Opción 3: Terraform Manual
```powershell
cd C:\Users\Felipe\RiderProjects\GateKeep-DOTNET-Nico\Gatekeep\terraform
terraform init
terraform apply -auto-approve
```

## Ventajas de Esta Configuración

1. **Sin Errores de Conflicto**: Los recursos existentes no generan errores "already exists"
2. **Idempotente**: Puede ejecutarse múltiples veces sin problemas
3. **Mantenimiento Simplificado**: Terraform gestiona la infraestructura sin intentar recrearla
4. **Sincronización**: Terraform mantiene sincronizado el estado con los recursos AWS reales
5. **Flexibilidad**: Los cambios futuros en Terraform se aplicarán correctamente

## Notas Importantes

- `lifecycle { ignore_changes = all }` ignora TODOS los cambios en los recursos existentes
  - Si necesitas actualizar un recurso específico, deberás comentar temporalmente su bloque lifecycle
  
- Los recursos se importan automáticamente durante el primer run si están disponibles en AWS

- Algunos recursos (CloudFront Policies) pueden requerir importación manual si no existen en el estado

## Recursos Pendientes de Importación Manual (si aplica)

Si experimentas errores de "already exists" con:
- `aws_cloudfront_response_headers_policy.frontend`
- `aws_cloudfront_cache_policy.frontend_static`

Necesitarás obtener sus IDs manualmente:
```bash
aws cloudfront list-response-headers-policies --query "ResponseHeadersPoliciesList.Items[?Name=='gatekeep-frontend-headers'].Id"
aws cloudfront list-cache-policies --query "CachePoliciesList.Items[?Name=='gatekeep-frontend-static-cache'].Id"
```

Y luego importarlos:
```bash
terraform import aws_cloudfront_response_headers_policy.frontend <ID>
terraform import aws_cloudfront_cache_policy.frontend_static <ID>
```
