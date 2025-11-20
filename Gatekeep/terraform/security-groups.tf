# Security Groups - Reglas de firewall

# Security Group para RDS
resource "aws_security_group" "rds" {
  name        = "${var.project_name}-rds-sg"
  description = "Security group para RDS PostgreSQL"
  vpc_id      = aws_vpc.main.id

  # Permitir PostgreSQL desde VPC
  ingress {
    description = "PostgreSQL desde VPC"
    from_port   = 5432
    to_port     = 5432
    protocol    = "tcp"
    cidr_blocks = [aws_vpc.main.cidr_block]
  }

  egress {
    description = "Allow all outbound traffic"
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name        = "${var.project_name}-rds-sg"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }
}

# Security Group para App Runner VPC Connector (si se usa)
resource "aws_security_group" "apprunner_connector" {
  name        = "${var.project_name}-apprunner-connector-sg"
  description = "Security group para App Runner VPC Connector"
  vpc_id      = aws_vpc.main.id

  egress {
    description = "Allow all outbound traffic"
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name        = "${var.project_name}-apprunner-connector-sg"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }
}

# Security Group para ElastiCache Redis
resource "aws_security_group" "redis" {
  name        = "${var.project_name}-redis-sg"
  description = "Security group para ElastiCache Redis"
  vpc_id      = aws_vpc.main.id

  # Permitir Redis desde ECS
  ingress {
    description     = "Redis desde ECS"
    from_port       = 6379
    to_port         = 6379
    protocol        = "tcp"
    security_groups = [aws_security_group.ecs.id]
  }

  egress {
    description = "Allow all outbound traffic"
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name        = "${var.project_name}-redis-sg"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }
}

# Security Group para Amazon MQ RabbitMQ
resource "aws_security_group" "rabbitmq" {
  name        = "${var.project_name}-rabbitmq-sg"
  description = "Security group para Amazon MQ RabbitMQ"
  vpc_id      = aws_vpc.main.id

  # Permitir AMQP desde ECS
  ingress {
    description     = "AMQP desde ECS"
    from_port       = 5671
    to_port         = 5671
    protocol        = "tcp"
    security_groups = [aws_security_group.ecs.id]
  }

  # Permitir Management Console desde ECS (opcional)
  ingress {
    description     = "RabbitMQ Management desde ECS"
    from_port       = 443
    to_port         = 443
    protocol        = "tcp"
    security_groups = [aws_security_group.ecs.id]
  }

  egress {
    description = "Allow all outbound traffic"
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name        = "${var.project_name}-rabbitmq-sg"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }
}

