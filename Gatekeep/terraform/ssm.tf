# AWS Systems Manager Parameter Store - Para configuración no sensible

# Parámetro para DB Host
resource "aws_ssm_parameter" "db_host" {
  name        = "/${var.project_name}/db/host"
  description = "Host de la base de datos RDS"
  type        = "String"
  value       = aws_db_instance.main.address

  tags = {
    Name        = "${var.project_name}-db-host"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }

  lifecycle {
    ignore_changes = all
  }
}

# Parámetro para DB Port
resource "aws_ssm_parameter" "db_port" {
  name        = "/${var.project_name}/db/port"
  description = "Puerto de la base de datos RDS"
  type        = "String"
  value       = tostring(aws_db_instance.main.port)

  tags = {
    Name        = "${var.project_name}-db-port"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }

  lifecycle {
    ignore_changes = all
  }
}

# Parámetro para DB Name
resource "aws_ssm_parameter" "db_name" {
  name        = "/${var.project_name}/db/name"
  description = "Nombre de la base de datos"
  type        = "String"
  value       = aws_db_instance.main.db_name

  tags = {
    Name        = "${var.project_name}-db-name"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }

  lifecycle {
    ignore_changes = all
  }
}

# Parámetro para DB Username
resource "aws_ssm_parameter" "db_username" {
  name        = "/${var.project_name}/db/username"
  description = "Usuario de la base de datos"
  type        = "String"
  value       = aws_db_instance.main.username

  tags = {
    Name        = "${var.project_name}-db-username"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }

  lifecycle {
    ignore_changes = all
  }
}

# Parámetro para ECR Repository URI
resource "aws_ssm_parameter" "ecr_repository_uri" {
  name        = "/${var.project_name}/ecr/repository-uri"
  description = "URI del repositorio ECR"
  type        = "String"
  value       = aws_ecr_repository.gatekeep_api.repository_url

  tags = {
    Name        = "${var.project_name}-ecr-repository-uri"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }

  lifecycle {
    ignore_changes = all
  }
}

