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
  length           = 32
  special          = true
  override_special = "!#$%&*()-_=+[]{}<>:?"
}

# Versión del secret con el password
resource "aws_secretsmanager_secret_version" "db_password" {
  secret_id     = aws_secretsmanager_secret.db_password.id
  secret_string = random_password.db_password.result
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
  length  = 64
  special = false
}

# Versión del secret con el JWT key
resource "aws_secretsmanager_secret_version" "jwt_key" {
  secret_id     = aws_secretsmanager_secret.jwt_key.id
  secret_string = random_password.jwt_key.result
}

# Secret para RabbitMQ password (si se usa)
resource "aws_secretsmanager_secret" "rabbitmq_password" {
  name        = "${var.project_name}/rabbitmq/password"
  description = "Password para RabbitMQ"

  recovery_window_in_days = 7

  tags = {
    Name        = "${var.project_name}-rabbitmq-password"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }
}

resource "random_password" "rabbitmq_password" {
  length           = 24
  special          = true
  override_special = "!#$%&*()-_=+[]{}<>:?"
}

resource "aws_secretsmanager_secret_version" "rabbitmq_password" {
  secret_id     = aws_secretsmanager_secret.rabbitmq_password.id
  secret_string = random_password.rabbitmq_password.result
}

