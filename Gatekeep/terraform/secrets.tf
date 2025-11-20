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

  lifecycle {
    ignore_changes = all
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

  lifecycle {
    ignore_changes = all
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

# Secret para MongoDB Connection String
resource "aws_secretsmanager_secret" "mongodb_connection" {
  name        = "${var.project_name}/mongodb/connection"
  description = "MongoDB Atlas connection string"

  recovery_window_in_days = 7

  tags = {
    Name        = "${var.project_name}-mongodb-connection"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }

  lifecycle {
    ignore_changes = all
  }
}

# Versión del secret con MongoDB connection string
# Usar try() para manejar cuando no existe versión
# data "aws_secretsmanager_secret_version" "mongodb_connection" {
#   count = var.manage_secret_versions ? 0 : 1
#   
#   secret_id = aws_secretsmanager_secret.mongodb_connection.id
# }

# Secret para RabbitMQ Password - Usar data source porque ya existe
data "aws_secretsmanager_secret" "rabbitmq_password" {
  name = "${var.project_name}/rabbitmq/password"
}

# Resource comentado porque el secret ya existe en AWS
# resource "aws_secretsmanager_secret" "rabbitmq_password" {
#   name        = "${var.project_name}/rabbitmq/password"
#   description = "Password para Amazon MQ RabbitMQ"
#
#   recovery_window_in_days = 7
#
#   tags = {
#     Name        = "${var.project_name}-rabbitmq-password"
#     Environment = var.environment
#     ManagedBy   = "Terraform"
#   }
#
#   lifecycle {
#     ignore_changes = all
#   }
# }

# Generar password aleatorio para RabbitMQ - COMENTADO (secret ya existe)
# resource "random_password" "rabbitmq_password" {
#   count = var.manage_secret_versions ? 1 : 0
#
#   length           = 32
#   special          = true
#   override_special = "!#$%&*()-_=+[]{}<>:?"
# }

# Versión del secret con RabbitMQ password - COMENTADO (secret ya existe)
# resource "aws_secretsmanager_secret_version" "rabbitmq_password" {
#   count = var.manage_secret_versions ? 1 : 0
#
#   secret_id     = aws_secretsmanager_secret.rabbitmq_password.id
#   secret_string = random_password.rabbitmq_password[0].result
# }

data "aws_secretsmanager_secret_version" "rabbitmq_password" {
  secret_id = data.aws_secretsmanager_secret.rabbitmq_password.id
}

locals {
  db_password_secret_string = var.manage_secret_versions ? aws_secretsmanager_secret_version.db_password[0].secret_string : data.aws_secretsmanager_secret_version.db_password[0].secret_string

  jwt_key_secret_string = var.manage_secret_versions ? aws_secretsmanager_secret_version.jwt_key[0].secret_string : data.aws_secretsmanager_secret_version.jwt_key[0].secret_string

  mongodb_connection_secret_string = ""  # Se debe crear manualmente la versión del secret en AWS Console

  rabbitmq_password_secret_string = try(data.aws_secretsmanager_secret_version.rabbitmq_password.secret_string, "")
}

