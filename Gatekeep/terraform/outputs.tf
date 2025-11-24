# Outputs de Terraform
# Estos valores se mostrarán después de aplicar la configuración

output "aws_region" {
  description = "Región de AWS utilizada"
  value       = var.aws_region
}

output "environment" {
  description = "Ambiente de despliegue"
  value       = var.environment
}

# ECR
output "ecr_repository_url" {
  description = "URL del repositorio ECR"
  value       = aws_ecr_repository.gatekeep_api.repository_url
}

output "ecr_repository_arn" {
  description = "ARN del repositorio ECR"
  value       = aws_ecr_repository.gatekeep_api.arn
}

# RDS
output "rds_endpoint" {
  description = "Endpoint de RDS PostgreSQL"
  value       = aws_db_instance.main.address
  sensitive   = false
}

output "rds_port" {
  description = "Puerto de RDS PostgreSQL"
  value       = aws_db_instance.main.port
}

output "rds_database_name" {
  description = "Nombre de la base de datos"
  value       = aws_db_instance.main.db_name
}

# Secrets Manager
output "db_password_secret_arn" {
  description = "ARN del secret de password de DB"
  value       = aws_secretsmanager_secret.db_password.arn
  sensitive   = true
}

output "jwt_key_secret_arn" {
  description = "ARN del secret de JWT key"
  value       = aws_secretsmanager_secret.jwt_key.arn
  sensitive   = true
}

# App Runner (Comentado - no disponible en sa-east-1)
# output "apprunner_service_url" {
#   description = "URL del servicio App Runner"
#   value       = "https://${aws_apprunner_service.main.service_url}"
# }

# output "apprunner_service_arn" {
#   description = "ARN del servicio App Runner"
#   value       = aws_apprunner_service.main.arn
# }

# VPC
output "vpc_id" {
  description = "ID de la VPC"
  value       = aws_vpc.main.id
}

output "vpc_cidr" {
  description = "CIDR de la VPC"
  value       = aws_vpc.main.cidr_block
}

# Parameter Store
output "ssm_db_host_parameter" {
  description = "Nombre del parámetro SSM para DB host"
  value       = aws_ssm_parameter.db_host.name
}

output "ssm_db_password_secret_name" {
  description = "Nombre del secret para DB password"
  value       = aws_secretsmanager_secret.db_password.name
}

# ECS
output "ecs_cluster_name" {
  description = "Nombre del cluster ECS"
  value       = aws_ecs_cluster.main.name
}

output "ecs_service_name" {
  description = "Nombre del servicio ECS"
  value       = aws_ecs_service.main.name
}

# Load Balancer
output "alb_dns_name" {
  description = "DNS name del Application Load Balancer"
  value       = aws_lb.main.dns_name
}

output "application_url" {
  description = "URL pública de la aplicación"
  value       = "https://${var.domain_name}"
}

output "frontend_url" {
  description = "URL pública del frontend (CloudFront)"
  value       = try("https://${aws_cloudfront_distribution.frontend.domain_name}", "https://${var.domain_name}")
}

# CloudFront
output "cloudfront_distribution_id" {
  description = "ID de la distribución de CloudFront"
  value       = try(aws_cloudfront_distribution.frontend.id, null)
}

output "cloudfront_domain_name" {
  description = "Nombre de dominio de CloudFront"
  value       = try(aws_cloudfront_distribution.frontend.domain_name, null)
}

# S3
output "s3_bucket_frontend" {
  description = "Nombre del bucket S3 para el frontend"
  value       = try(aws_s3_bucket.frontend.bucket, null)
}

output "s3_bucket_terraform_state" {
  description = "Nombre del bucket S3 para Terraform state"
  value       = try(aws_s3_bucket.terraform_state.bucket, null)
  sensitive   = false
}

# DynamoDB
output "dynamodb_table_terraform_locks" {
  description = "Nombre de la tabla DynamoDB para Terraform locks"
  value       = try(aws_dynamodb_table.terraform_locks.name, null)
}

output "backend_api_url" {
  description = "URL pública del backend API (ALB)"
  value       = "https://api.${var.domain_name}"
}

output "custom_domain_aliases" {
  description = "Dominios públicos apuntando al ALB"
  value       = local.all_public_domains
}

# ECR Frontend
output "ecr_frontend_repository_url" {
  description = "URL del repositorio ECR para frontend"
  value       = aws_ecr_repository.gatekeep_frontend.repository_url
}

# ECS Frontend
output "ecs_frontend_service_name" {
  description = "Nombre del servicio ECS del frontend"
  value       = try(aws_ecs_service.frontend.name, null)
}

# ElastiCache Redis
output "redis_endpoint" {
  description = "Endpoint de ElastiCache Redis"
  value       = try(coalesce(aws_elasticache_replication_group.main.configuration_endpoint_address, aws_elasticache_replication_group.main.primary_endpoint_address), "")
}

output "redis_port" {
  description = "Puerto de ElastiCache Redis"
  value       = aws_elasticache_replication_group.main.port
}

output "redis_connection_string" {
  description = "Connection string completo de Redis (endpoint:port)"
  value       = try("${coalesce(aws_elasticache_replication_group.main.configuration_endpoint_address, aws_elasticache_replication_group.main.primary_endpoint_address, "")}:${coalesce(aws_elasticache_replication_group.main.port, 6379)}", "")
}

# Secrets Manager - Nuevos secretos
output "mongodb_connection_secret_arn" {
  description = "ARN del secret de MongoDB connection string"
  value       = aws_secretsmanager_secret.mongodb_connection.arn
  sensitive   = true
}

