# AWS Secrets Manager - Para almacenar secretos de forma segura

# Secret para password de RDS
resource "aws_secretsmanager_secret" "db_password" {
  name        = "${var.project_name}/db/password"
  description = "Password para la base de datos RDS PostgreSQL"

  recovery_window_in_days = 7

  tags = {
    Name        = "${var.project_name}-db-password"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }
}

# Generar password aleatorio para RDS
resource "random_password" "db_password" {
  count = var.manage_secret_versions ? 1 : 0

  length           = 32
  special          = true
  override_special = "!#$%&*()-_=+[]{}<>:?"
}

# Versión del secret con el password
resource "aws_secretsmanager_secret_version" "db_password" {
  count = var.manage_secret_versions ? 1 : 0

  secret_id     = aws_secretsmanager_secret.db_password.id
  secret_string = random_password.db_password[0].result
}

data "aws_secretsmanager_secret_version" "db_password" {
  count = var.manage_secret_versions ? 0 : 1

  secret_id = aws_secretsmanager_secret.db_password.id
}

# Secret para JWT Key
resource "aws_secretsmanager_secret" "jwt_key" {
  name        = "${var.project_name}/jwt/key"
  description = "Clave secreta para JWT tokens"

  recovery_window_in_days = 7

  tags = {
    Name        = "${var.project_name}-jwt-key"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }
}

# Generar JWT key aleatorio
resource "random_password" "jwt_key" {
  count = var.manage_secret_versions ? 1 : 0

  length  = 64
  special = false
}

# Versión del secret con el JWT key
resource "aws_secretsmanager_secret_version" "jwt_key" {
  count = var.manage_secret_versions ? 1 : 0

  secret_id     = aws_secretsmanager_secret.jwt_key.id
  secret_string = random_password.jwt_key[0].result
}

data "aws_secretsmanager_secret_version" "jwt_key" {
  count = var.manage_secret_versions ? 0 : 1

  secret_id = aws_secretsmanager_secret.jwt_key.id
}

locals {
  db_password_secret_string = var.manage_secret_versions ? aws_secretsmanager_secret_version.db_password[0].secret_string : data.aws_secretsmanager_secret_version.db_password[0].secret_string

  jwt_key_secret_string = var.manage_secret_versions ? aws_secretsmanager_secret_version.jwt_key[0].secret_string : data.aws_secretsmanager_secret_version.jwt_key[0].secret_string
}

