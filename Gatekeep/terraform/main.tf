# Terraform Configuration for GateKeep AWS Infrastructure
# Provider: AWS
# Region: sa-east-1 (São Paulo)
# 
# Las versiones requeridas están en versions.tf

# Configure the AWS Provider
provider "aws" {
  region  = var.aws_region
  profile = var.aws_profile # Usar perfil específico si está definido

  # Las credenciales se pueden configurar de varias formas:
  # 1. Variables de entorno: AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY
  # 2. Archivo de credenciales: ~/.aws/credentials (usando aws configure)
  # 3. Perfil específico: var.aws_profile (ej: "Alex")
  # 4. Variables de Terraform (menos seguro, no recomendado para producción)
  # 5. IAM Roles (si ejecutas desde EC2/ECS/Lambda)

  # Si quieres usar variables de Terraform directamente (no recomendado):
  # access_key = var.aws_access_key_id
  # secret_key = var.aws_secret_access_key

  default_tags {
    tags = {
      Project     = "GateKeep"
      ManagedBy   = "Terraform"
      Environment = var.environment
    }
  }
}

