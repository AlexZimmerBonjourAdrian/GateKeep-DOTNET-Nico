# Reflexión de AWS CLI en Terraform

Este documento explica cómo los comandos y recursos documentados en `docs/AWS_CLI_COMPLETO.md` se reflejan en los archivos de Terraform.

**Fecha:** 2025-01-21

---

## Resumen de Cambios

Se han creado y actualizado archivos de Terraform para que todos los recursos mencionados en la documentación de AWS CLI estén definidos como código de infraestructura.

---

## Archivos Creados

### 1. `s3-backend.tf`
**Propósito:** Define el bucket S3 y la tabla DynamoDB para el backend remoto de Terraform.

**Recursos definidos:**
- `aws_s3_bucket.terraform_state` - Bucket para almacenar el estado de Terraform
- `aws_s3_bucket_versioning.terraform_state` - Versionado del bucket
- `aws_s3_bucket_public_access_block.terraform_state` - Bloqueo de acceso público
- `aws_s3_bucket_server_side_encryption_configuration.terraform_state` - Encriptación
- `aws_dynamodb_table.terraform_locks` - Tabla para locks de Terraform

**Comandos AWS CLI relacionados:**
```powershell
aws s3api head-bucket --bucket gatekeep-terraform-state
aws s3api create-bucket --bucket gatekeep-terraform-state
aws s3api put-bucket-versioning --bucket gatekeep-terraform-state
aws s3api put-public-access-block --bucket gatekeep-terraform-state
aws dynamodb describe-table --table-name gatekeep-terraform-locks
aws dynamodb create-table --table-name gatekeep-terraform-locks
```

**Script relacionado:** `terraform/setup-remote-backend.ps1`

---

### 2. `s3-frontend.tf`
**Propósito:** Define el bucket S3 para almacenar el build estático del frontend.

**Recursos definidos:**
- `aws_s3_bucket.frontend` - Bucket para el frontend
- `aws_s3_bucket_website_configuration.frontend` - Configuración de website hosting
- `aws_s3_bucket_policy.frontend_public_read` - Política de acceso público
- `aws_s3_bucket_public_access_block.frontend` - Control de acceso público
- `aws_s3_bucket_cors_configuration.frontend` - Configuración CORS
- `aws_s3_bucket_lifecycle_configuration.frontend` - Política de lifecycle

**Comandos AWS CLI relacionados:**
```powershell
aws s3 cp archivo.txt s3://gatekeep-frontend-dev/archivo.txt
aws s3 sync ./build s3://gatekeep-frontend-dev --delete
aws s3 ls
```

**Scripts relacionados:**
- `terraform/scripts/upload-frontend-to-s3.ps1`
- `terraform/scripts/upload-frontend-to-s3.sh`

---

### 3. `cloudfront-frontend.tf`
**Propósito:** Define la distribución CloudFront para servir el frontend desde S3.

**Recursos definidos:**
- `aws_cloudfront_origin_access_control.frontend` - OAC para S3
- `aws_cloudfront_distribution.frontend` - Distribución CloudFront
- `aws_acm_certificate.cloudfront` - Certificado SSL (us-east-1)
- `aws_acm_certificate_validation.cloudfront` - Validación del certificado

**Comandos AWS CLI relacionados:**
```powershell
aws cloudfront create-invalidation --distribution-id E1234567890ABC --paths "/*"
aws cloudfront get-invalidation --distribution-id E1234567890ABC --id I1234567890ABC
aws cloudfront wait invalidation-completed --distribution-id E1234567890ABC --id I1234567890ABC
aws cloudfront list-response-headers-policies
aws cloudfront list-cache-policies
```

**Scripts relacionados:**
- `terraform/scripts/upload-frontend-to-s3.ps1`
- `terraform/scripts/upload-frontend-to-s3.sh`

---

## Archivos Actualizados

### 1. `outputs.tf`
**Cambios realizados:**
- Agregado output para `cloudfront_distribution_id`
- Agregado output para `cloudfront_domain_name`
- Agregado output para `s3_bucket_frontend`
- Agregado output para `s3_bucket_terraform_state`
- Agregado output para `dynamodb_table_terraform_locks`
- Habilitado output para `ecs_frontend_service_name`

**Outputs nuevos:**
```hcl
output "cloudfront_distribution_id" {
  description = "ID de la distribución de CloudFront"
  value       = try(aws_cloudfront_distribution.frontend.id, null)
}

output "s3_bucket_frontend" {
  description = "Nombre del bucket S3 para el frontend"
  value       = try(aws_s3_bucket.frontend.bucket, null)
}

output "s3_bucket_terraform_state" {
  description = "Nombre del bucket S3 para Terraform state"
  value       = try(aws_s3_bucket.terraform_state.bucket, null)
}

output "dynamodb_table_terraform_locks" {
  description = "Nombre de la tabla DynamoDB para Terraform locks"
  value       = try(aws_dynamodb_table.terraform_locks.name, null)
}
```

---

## Mapeo de Recursos AWS CLI → Terraform

### ECS (Elastic Container Service)
| Comando AWS CLI | Recurso Terraform | Archivo |
|----------------|-------------------|---------|
| `aws ecs list-services` | `aws_ecs_service.main` | `ecs.tf` |
| `aws ecs describe-services` | `aws_ecs_service.main` | `ecs.tf` |
| `aws ecs update-service` | `aws_ecs_service.main` | `ecs.tf` |
| `aws ecs describe-task-definition` | `aws_ecs_task_definition.main` | `ecs.tf` |
| `aws ecs register-task-definition` | `aws_ecs_task_definition.main` | `ecs.tf` |
| `aws ecs list-tasks` | N/A (runtime) | - |
| `aws ecs describe-tasks` | N/A (runtime) | - |

### ECR (Elastic Container Registry)
| Comando AWS CLI | Recurso Terraform | Archivo |
|----------------|-------------------|---------|
| `aws ecr describe-repositories` | `aws_ecr_repository.gatekeep_api` | `ecr.tf` |
| `aws ecr get-login-password` | N/A (runtime) | - |
| `aws ecr list-images` | N/A (runtime) | - |
| `aws ecr create-repository` | `aws_ecr_repository.gatekeep_api` | `ecr.tf` |

### CloudWatch Logs
| Comando AWS CLI | Recurso Terraform | Archivo |
|----------------|-------------------|---------|
| `aws logs tail /ecs/gatekeep` | `aws_cloudwatch_log_group.ecs` | `ecs.tf` |

### S3
| Comando AWS CLI | Recurso Terraform | Archivo |
|----------------|-------------------|---------|
| `aws s3 ls` | `aws_s3_bucket.*` | `s3-backend.tf`, `s3-frontend.tf` |
| `aws s3api head-bucket` | `aws_s3_bucket.*` | `s3-backend.tf`, `s3-frontend.tf` |
| `aws s3api create-bucket` | `aws_s3_bucket.*` | `s3-backend.tf`, `s3-frontend.tf` |
| `aws s3api put-bucket-versioning` | `aws_s3_bucket_versioning.*` | `s3-backend.tf` |
| `aws s3api put-public-access-block` | `aws_s3_bucket_public_access_block.*` | `s3-backend.tf`, `s3-frontend.tf` |
| `aws s3 cp` | N/A (runtime) | - |
| `aws s3 sync` | N/A (runtime) | - |

### CloudFront
| Comando AWS CLI | Recurso Terraform | Archivo |
|----------------|-------------------|---------|
| `aws cloudfront create-invalidation` | N/A (runtime) | - |
| `aws cloudfront get-invalidation` | N/A (runtime) | - |
| `aws cloudfront wait invalidation-completed` | N/A (runtime) | - |
| `aws cloudfront list-response-headers-policies` | N/A (runtime) | - |
| `aws cloudfront list-cache-policies` | N/A (runtime) | - |

### DynamoDB
| Comando AWS CLI | Recurso Terraform | Archivo |
|----------------|-------------------|---------|
| `aws dynamodb describe-table` | `aws_dynamodb_table.terraform_locks` | `s3-backend.tf` |
| `aws dynamodb create-table` | `aws_dynamodb_table.terraform_locks` | `s3-backend.tf` |
| `aws dynamodb wait table-exists` | N/A (runtime) | - |

### Secrets Manager
| Comando AWS CLI | Recurso Terraform | Archivo |
|----------------|-------------------|---------|
| `aws secretsmanager list-secrets` | `aws_secretsmanager_secret.*` | `secrets.tf` |
| `aws secretsmanager get-secret-value` | `aws_secretsmanager_secret.*` | `secrets.tf` |
| `aws secretsmanager create-secret` | `aws_secretsmanager_secret.*` | `secrets.tf` |
| `aws secretsmanager describe-secret` | `aws_secretsmanager_secret.*` | `secrets.tf` |

### Systems Manager (Parameter Store)
| Comando AWS CLI | Recurso Terraform | Archivo |
|----------------|-------------------|---------|
| `aws ssm describe-parameters` | `aws_ssm_parameter.*` | `ssm.tf` |
| `aws ssm get-parameter` | `aws_ssm_parameter.*` | `ssm.tf` |
| `aws ssm put-parameter` | `aws_ssm_parameter.*` | `ssm.tf` |

### RDS
| Comando AWS CLI | Recurso Terraform | Archivo |
|----------------|-------------------|---------|
| `aws rds describe-db-instances` | `aws_db_instance.main` | `rds.tf` |
| `aws rds describe-db-instances --db-instance-identifier` | `aws_db_instance.main` | `rds.tf` |

### ELB (Elastic Load Balancer)
| Comando AWS CLI | Recurso Terraform | Archivo |
|----------------|-------------------|---------|
| `aws elbv2 describe-target-health` | `aws_lb_target_group.*` | `ecs.tf` |

---

## Recursos que NO se definen en Terraform

Los siguientes comandos AWS CLI son operaciones de runtime y no se definen como recursos de infraestructura:

1. **Operaciones de runtime:**
   - `aws ecs list-tasks` - Lista tareas en ejecución
   - `aws ecs describe-tasks` - Describe tareas específicas
   - `aws ecr get-login-password` - Obtiene token de autenticación
   - `aws ecr list-images` - Lista imágenes en repositorio
   - `aws s3 cp` - Copia archivos
   - `aws s3 sync` - Sincroniza directorios
   - `aws cloudfront create-invalidation` - Crea invalidación de caché
   - `aws logs tail` - Sigue logs en tiempo real

2. **Operaciones de consulta:**
   - `aws sts get-caller-identity` - Verifica identidad
   - `aws configure list` - Lista configuración
   - `aws iam get-user` - Obtiene información de usuario

---

## Uso de los Recursos

### Para aplicar los cambios:

```powershell
cd terraform
terraform init
terraform plan
terraform apply
```

### Para verificar los outputs:

```powershell
terraform output
```

### Para obtener valores específicos:

```powershell
# ID de CloudFront
terraform output cloudfront_distribution_id

# Nombre del bucket S3 frontend
terraform output s3_bucket_frontend

# Nombre del bucket S3 backend
terraform output s3_bucket_terraform_state
```

---

## Notas Importantes

1. **Backend Remoto:** Los recursos de S3 y DynamoDB para el backend de Terraform están definidos, pero el backend remoto en `backend.tf` está comentado. Para habilitarlo:
   - Descomentar el bloque `terraform { backend "s3" { ... } }` en `backend.tf`
   - Ejecutar `terraform init` para migrar el estado

2. **CloudFront:** La distribución CloudFront está configurada pero puede requerir certificados ACM. Asegúrate de que los certificados estén validados antes de aplicar.

3. **S3 Frontend:** El bucket S3 para el frontend está configurado con acceso público controlado. Asegúrate de que la política de acceso sea adecuada para tu caso de uso.

4. **Lifecycle Policies:** Los recursos tienen `lifecycle { ignore_changes = all }` en algunos casos para evitar que Terraform sobrescriba cambios manuales.

---

## Próximos Pasos

1. Revisar y ajustar las configuraciones según tus necesidades
2. Ejecutar `terraform plan` para ver los cambios propuestos
3. Aplicar los cambios con `terraform apply` cuando estés listo
4. Actualizar los scripts de despliegue para usar los nuevos outputs

---

**Última actualización:** 2025-01-21

