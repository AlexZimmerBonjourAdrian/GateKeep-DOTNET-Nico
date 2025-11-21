# Setup de Terraform para GateKeep

## Estado Actual

Los siguientes recursos **YA EXISTEN EN AWS** y han sido comentados en la configuración de Terraform para evitar conflictos:

- ✅ **S3 Bucket**: `gatekeep-frontend-dev`
- ✅ **S3 Versioning**: Habilitado
- ✅ **S3 CORS Configuration**: Configurado
- ✅ **S3 Public Access Block**: Configurado
- ✅ **HTTPS Listener**: Puerto 443 en ALB
- ✅ **CloudFront Distribution**: Para servir frontend
- ✅ **CloudFront Response Headers Policy**: Para CORS y seguridad
- ✅ **CloudFront Cache Policy**: Para optimización de assets

## Recursos Creados Recientemente (19/11/2025)

Los siguientes recursos fueron creados exitosamente con `terraform apply`:

- ✅ `aws_lb_listener_rule.backend_api` - Ruta `/api/*`
- ✅ `aws_lb_listener_rule.backend_auth` - Ruta `/auth/*`
- ✅ `aws_lb_listener_rule.backend_usuarios` - Ruta `/usuarios/*`
- ✅ `aws_lb_listener_rule.backend_swagger` - Rutas `/swagger*`
- ✅ `aws_lb_listener_rule.backend_health` - Ruta `/health`

## Para Otros Desarrolladores

### Opción 1: Clonar y ejecutar (Recomendado)

```powershell
# 1. Clonar el repositorio
git clone https://github.com/AlexZimmerBonjourAdrian/GateKeep-DOTNET-Nico.git
cd GateKeep-DOTNET-Nico/Gatekeep/terraform

# 2. Configurar AWS CLI
aws configure  # Ingresar access key, secret key, región: sa-east-1

# 3. Inicializar Terraform
terraform init

# 4. Ver cambios (sin aplicar)
terraform plan

# 5. Aplicar cambios (cuando esté listo)
terraform apply
```

### Opción 2: Usar el backend S3 compartido (TODO)

Para que múltiples desarrolladores compartan el mismo estado de Terraform:

1. **Crear bucket S3 para estado** (administrador):
```bash
aws s3 mb s3://gatekeep-terraform-state --region sa-east-1
aws s3api put-bucket-versioning \
  --bucket gatekeep-terraform-state \
  --versioning-configuration Status=Enabled

# Crear tabla DynamoDB para locks
aws dynamodb create-table \
  --table-name gatekeep-terraform-locks \
  --attribute-definitions AttributeName=LockID,AttributeType=S \
  --key-schema AttributeName=LockID,KeyType=HASH \
  --billing-mode PAY_PER_REQUEST \
  --region sa-east-1
```

2. **Descomentar backend en `backend.tf`**:
```hcl
terraform {
  backend "s3" {
    bucket         = "gatekeep-terraform-state"
    key            = "prod/terraform.tfstate"
    region         = "sa-east-1"
    encrypt        = true
    dynamodb_table = "gatekeep-terraform-locks"
  }
}
```

3. **Reinicializar Terraform**:
```powershell
terraform init -migrate-state
```

## Configuración Actual

- **Estado Local**: `.terraform/` en máquina local
- **Permisos AWS**: Usuario `Alex` (requiere S3 y DynamoDB para backend remoto)
- **Región**: `sa-east-1` (São Paulo)
- **Entorno**: `dev`

## Problemas Resolvidos

### Error: "DuplicateListener: A listener already exists on this port"
**Solución**: Se comentó el recurso `aws_lb_listener.https` que ya existía en AWS. Los listener rules ahora usan `aws_lb_listener.main` (puerto 80).

### Error: "BucketAlreadyOwnedByYou"
**Solución**: Se comentó `aws_s3_bucket.frontend` porque ya existe en AWS.

### Error: "ResponseHeadersPolicyAlreadyExists"
**Solución**: Se comentó `aws_cloudfront_response_headers_policy.frontend` y `aws_cloudfront_cache_policy.frontend_static`.

## Próximos Pasos

1. ✅ **Crear backend S3 compartido** (cuando tengas permisos)
2. ⏳ **Descomentar y limpiar recursos de CloudFront**
3. ⏳ **Configurar CloudFront en lugar de ALB para frontend**
4. ⏳ **Importar políticas existentes a Terraform state**

## Comandos Útiles

```powershell
# Ver estado actual
terraform state list
terraform state show aws_lb_listener_rule.backend_api

# Plan sin aplicar
terraform plan

# Aplicar específico
terraform apply -target=aws_lb_listener_rule.backend_api

# Destruir recursos (cuidado!)
terraform destroy

# Ver outputs
terraform output
```

## Notas Importantes

⚠️ **IMPORTANTE**: Los archivos `terraform.tfvars` contienen secretos. Nunca commitear en git.

Usar variables de entorno en su lugar:
```powershell
$env:TF_VAR_jwt_key = "tu-jwt-secret"
$env:TF_VAR_db_password = "tu-db-password"
```

## Contacto

Para preguntas o problemas con Terraform, contactar al equipo de DevOps.
