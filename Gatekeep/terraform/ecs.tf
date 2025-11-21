# ECS Fargate - Despliegue simple de la aplicación
# Nota: data.aws_caller_identity.current está definido en cloudwatch.tf

# ECS Cluster
resource "aws_ecs_cluster" "main" {
  name = "${var.project_name}-cluster"

  setting {
    name  = "containerInsights"
    value = "enabled"
  }

  tags = {
    Name        = "${var.project_name}-cluster"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }

  lifecycle {
    ignore_changes = all
  }
}

# CloudWatch Log Group
resource "aws_cloudwatch_log_group" "ecs" {
  name              = "/ecs/${var.project_name}"
  retention_in_days = 7

  tags = {
    Name        = "${var.project_name}-ecs-logs"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }

  lifecycle {
    ignore_changes = all
  }
}

# IAM Role para ECS Task Execution
resource "aws_iam_role" "ecs_execution" {
  name = "${var.project_name}-ecs-execution-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "ecs-tasks.amazonaws.com"
        }
      }
    ]
  })

  tags = {
    Name        = "${var.project_name}-ecs-execution-role"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }

  lifecycle {
    ignore_changes = all
  }
}

# Attach AWS managed policy for ECS task execution
resource "aws_iam_role_policy_attachment" "ecs_execution" {
  role       = aws_iam_role.ecs_execution.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"
}

# Policy adicional para acceder a Secrets Manager y Parameter Store
resource "aws_iam_role_policy" "ecs_execution_secrets" {
  name = "${var.project_name}-ecs-execution-secrets"
  role = aws_iam_role.ecs_execution.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "secretsmanager:GetSecretValue",
          "secretsmanager:DescribeSecret"
        ]
        Resource = [
          aws_secretsmanager_secret.db_password.arn,
          aws_secretsmanager_secret.jwt_key.arn,
          aws_secretsmanager_secret.mongodb_connection.arn
          # aws_secretsmanager_secret.rabbitmq_password.arn  # COMENTADO TEMPORALMENTE
        ]
      },
      {
        Effect = "Allow"
        Action = [
          "ssm:GetParameter",
          "ssm:GetParameters",
          "ssm:GetParametersByPath"
        ]
        Resource = [
          "arn:aws:ssm:${var.aws_region}:*:parameter/${var.project_name}/*"
        ]
      }
    ]
  })
}

# IAM Role para ECS Task (la aplicación misma)
resource "aws_iam_role" "ecs_task" {
  name = "${var.project_name}-ecs-task-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "ecs-tasks.amazonaws.com"
        }
      }
    ]
  })

  tags = {
    Name        = "${var.project_name}-ecs-task-role"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }

  lifecycle {
    ignore_changes = all
  }
}

# Policy para ECS Task - CloudWatch Metrics
# Permite que la aplicación envíe métricas customizadas a CloudWatch
resource "aws_iam_role_policy" "ecs_task_cloudwatch" {
  name = "${var.project_name}-ecs-task-cloudwatch"
  role = aws_iam_role.ecs_task.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "cloudwatch:PutMetricData"
        ]
        Resource = "*"
        Condition = {
          StringEquals = {
            "cloudwatch:namespace" = [
              "GateKeep/Redis",
              "GateKeep/Redis/Logs"
              # "GateKeep/RabbitMQ"  # COMENTADO TEMPORALMENTE
            ]
          }
        }
      }
    ]
  })
}

# ECS Task Definition
resource "aws_ecs_task_definition" "main" {
  family                   = "${var.project_name}-api"
  requires_compatibilities = ["FARGATE"]
  network_mode             = "awsvpc"
  cpu                      = "512"  # 0.5 vCPU
  memory                   = "1024" # 1 GB
  execution_role_arn       = aws_iam_role.ecs_execution.arn
  task_role_arn            = aws_iam_role.ecs_task.arn

  container_definitions = jsonencode([
    {
      name  = "${var.project_name}-api"
      image = "${aws_ecr_repository.gatekeep_api.repository_url}:latest"

      portMappings = [
        {
          containerPort = 5011
          protocol      = "tcp"
        }
      ]

      environment = [
        {
          name  = "ASPNETCORE_ENVIRONMENT"
          value = var.environment
        },
        {
          name  = "ASPNETCORE_URLS"
          value = "http://+:5011"
        },
        {
          name  = "GATEKEEP_PORT"
          value = "5011"
        },
        {
          name  = "AWS_REGION"
          value = var.aws_region
        },
        # Base de datos - valores desde Parameter Store (no sensibles)
        {
          name  = "DATABASE__HOST"
          value = aws_ssm_parameter.db_host.value
        },
        {
          name  = "DATABASE__PORT"
          value = aws_ssm_parameter.db_port.value
        },
        {
          name  = "DATABASE__NAME"
          value = aws_ssm_parameter.db_name.value
        },
        {
          name  = "DATABASE__USER"
          value = aws_ssm_parameter.db_username.value
        },
        # MongoDB - valores no sensibles
        {
          name  = "MONGODB_DATABASE"
          value = "GateKeepMongo"
        },
        {
          name  = "MONGODB_USE_STABLE_API"
          value = "true"
        },
        # Redis - COMENTADO TEMPORALMENTE (ElastiCache no disponible)
        # {
        #   name  = "REDIS_CONNECTION"
        #   value = "${aws_elasticache_replication_group.main.configuration_endpoint_address != "" ? aws_elasticache_replication_group.main.configuration_endpoint_address : aws_elasticache_replication_group.main.primary_endpoint_address}:${aws_elasticache_replication_group.main.port}"
        # },
        # {
        #   name  = "REDIS_INSTANCE"
        #   value = "GateKeep:"
        # }
        # RabbitMQ - COMENTADO TEMPORALMENTE
        # {
        #   name  = "RABBITMQ__HOST"
        #   value = "${aws_mq_broker.main.broker_id}.mq.${var.aws_region}.amazonaws.com"
        # },
        # {
        #   name  = "RABBITMQ__PORT"
        #   value = "5671"
        # },
        # {
        #   name  = "RABBITMQ__USE_SSL"
        #   value = "true"
        # },
        # {
        #   name  = "RABBITMQ__USERNAME"
        #   value = "admin"
        # },
        # {
        #   name  = "RABBITMQ__VIRTUALHOST"
        #   value = "/"
        # }
      ]

      secrets = [
        # Base de datos - desde Secrets Manager (valores sensibles)
        {
          name      = "DATABASE__PASSWORD"
          valueFrom = aws_secretsmanager_secret.db_password.arn
        },
        # JWT - desde Secrets Manager
        {
          name      = "JWT__KEY"
          valueFrom = aws_secretsmanager_secret.jwt_key.arn
        },
        # MongoDB - desde Secrets Manager
        {
          name      = "MONGODB_CONNECTION"
          valueFrom = aws_secretsmanager_secret.mongodb_connection.arn
        }
        # RabbitMQ - COMENTADO TEMPORALMENTE
        # {
        #   name      = "RABBITMQ__PASSWORD"
        #   valueFrom = aws_secretsmanager_secret.rabbitmq_password.arn
        # }
      ]

      logConfiguration = {
        logDriver = "awslogs"
        options = {
          "awslogs-group"         = aws_cloudwatch_log_group.ecs.name
          "awslogs-region"        = var.aws_region
          "awslogs-stream-prefix" = "ecs"
        }
      }

      healthCheck = {
        command     = ["CMD-SHELL", "curl -f http://localhost:5011/health || exit 1"]
        interval    = 30
        timeout     = 5
        retries     = 3
        startPeriod = 60
      }
    }
  ])

  tags = {
    Name        = "${var.project_name}-api-task"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }
}

# Security Group para ECS
resource "aws_security_group" "ecs" {
  name        = "${var.project_name}-ecs-sg"
  description = "Security group para ECS Fargate"
  vpc_id      = aws_vpc.main.id

  # Permitir tráfico HTTP saliente
  egress {
    description = "Allow all outbound traffic"
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  # Permitir tráfico HTTP entrante (para ALB) - Backend
  ingress {
    description     = "HTTP from ALB - Backend"
    from_port       = 5011
    to_port         = 5011
    protocol        = "tcp"
    security_groups = [aws_security_group.alb.id]
  }

  # Permitir tráfico HTTP entrante (para ALB) - Frontend
  ingress {
    description     = "HTTP from ALB - Frontend"
    from_port       = 3000
    to_port         = 3000
    protocol        = "tcp"
    security_groups = [aws_security_group.alb.id]
  }

  tags = {
    Name        = "${var.project_name}-ecs-sg"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }
}

# Application Load Balancer (simple)
resource "aws_lb" "main" {
  name               = "${var.project_name}-alb"
  internal           = false
  load_balancer_type = "application"
  security_groups    = [aws_security_group.alb.id]
  subnets            = [aws_subnet.public_1.id, aws_subnet.public_2.id]

  enable_deletion_protection = false

  tags = {
    Name        = "${var.project_name}-alb"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }

  lifecycle {
    ignore_changes = all
  }
}

# Security Group para ALB
resource "aws_security_group" "alb" {
  name        = "${var.project_name}-alb-sg"
  description = "Security group para Application Load Balancer"
  vpc_id      = aws_vpc.main.id

  # Permitir HTTP entrante
  ingress {
    description = "HTTP from internet"
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  # Permitir HTTPS entrante (opcional, para futuro)
  ingress {
    description = "HTTPS from internet"
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    description = "Allow all outbound traffic"
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name        = "${var.project_name}-alb-sg"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }
}

# Target Group
resource "aws_lb_target_group" "main" {
  name        = "${var.project_name}-tg"
  port        = 5011
  protocol    = "HTTP"
  vpc_id      = aws_vpc.main.id
  target_type = "ip"

  health_check {
    enabled             = true
    healthy_threshold   = 2
    unhealthy_threshold = 3
    timeout             = 5
    interval            = 30
    path                = "/health"
    protocol            = "HTTP"
    matcher             = "200"
  }

  tags = {
    Name        = "${var.project_name}-tg"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }

  lifecycle {
    ignore_changes = all
  }
}

# Target Group para Frontend
resource "aws_lb_target_group" "frontend" {
  name        = "${var.project_name}-frontend-tg"
  port        = 3000
  protocol    = "HTTP"
  vpc_id      = aws_vpc.main.id
  target_type = "ip"

  health_check {
    enabled             = true
    healthy_threshold   = 2
    unhealthy_threshold = 3
    timeout             = 5
    interval            = 30
    path                = "/"
    protocol            = "HTTP"
    matcher             = "200,404"
  }

  tags = {
    Name        = "${var.project_name}-frontend-tg"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }

  lifecycle {
    ignore_changes = all
  }
}

# ALB Listener con reglas de enrutamiento
# NOTA: El frontend ahora está en S3+CloudFront, el ALB solo sirve el backend
resource "aws_lb_listener" "main" {
  load_balancer_arn = aws_lb.main.arn
  port              = "80"
  protocol          = "HTTP"

  # Por defecto, redirigir a HTTPS
  default_action {
    type = "redirect"
    redirect {
      port        = "443"
      protocol    = "HTTPS"
      status_code = "HTTP_301"
      host        = "#{host}"
      path        = "/#{path}"
      query       = "#{query}"
    }
  }
}

# HTTPS Listener para ALB
# Crear el listener HTTPS si no existe
resource "aws_lb_listener" "https" {
  count = var.enable_https ? 1 : 0
  
  load_balancer_arn = aws_lb.main.arn
  port              = "443"
  protocol          = "HTTPS"
  ssl_policy        = "ELBSecurityPolicy-TLS-1-2-2017-01"
  certificate_arn   = aws_acm_certificate_validation.alb[0].certificate_arn

  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.frontend.arn
  }

  lifecycle {
    ignore_changes = [certificate_arn, ssl_policy]
  }
}

# Local para usar el listener creado
locals {
  https_listener_arn = var.enable_https ? aws_lb_listener.https[0].arn : null
}

# Listener Rule para Backend API - /api/*
resource "aws_lb_listener_rule" "backend_api" {
  listener_arn = aws_lb_listener.main.arn
  priority     = 100

  action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.main.arn
  }

  condition {
    path_pattern {
      values = ["/api/*"]
    }
  }

  lifecycle {
    ignore_changes = all
  }
}

# Listener Rule para Backend Auth - /auth/*
resource "aws_lb_listener_rule" "backend_auth" {
  listener_arn = aws_lb_listener.main.arn
  priority     = 110

  action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.main.arn
  }

  condition {
    path_pattern {
      values = ["/auth/*"]
    }
  }

  lifecycle {
    ignore_changes = all
  }
}

# Listener Rule para Backend Usuarios - /usuarios/*
resource "aws_lb_listener_rule" "backend_usuarios" {
  listener_arn = aws_lb_listener.main.arn
  priority     = 120

  action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.main.arn
  }

  condition {
    path_pattern {
      values = ["/usuarios/*"]
    }
  }

  lifecycle {
    ignore_changes = all
  }
}

# Listener Rule para Swagger
resource "aws_lb_listener_rule" "backend_swagger" {
  listener_arn = aws_lb_listener.main.arn
  priority     = 130

  action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.main.arn
  }

  condition {
    path_pattern {
      values = ["/swagger*", "/swagger/*"]
    }
  }

  lifecycle {
    ignore_changes = all
  }
}

# Listener Rule para Health Check
resource "aws_lb_listener_rule" "backend_health" {
  listener_arn = aws_lb_listener.main.arn
  priority     = 140

  action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.main.arn
  }

  condition {
    path_pattern {
      values = ["/health"]
    }
  }

  lifecycle {
    ignore_changes = all
  }
}

# ============================================
# HTTPS LISTENER RULES (Puerto 443)
# ============================================
# Estas reglas son necesarias porque el frontend se conecta vía HTTPS

# Listener Rule para Backend API - /api/* (HTTPS)
resource "aws_lb_listener_rule" "backend_api_https" {
  count = var.enable_https ? 1 : 0
  
  listener_arn = local.https_listener_arn
  priority     = 100

  action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.main.arn
  }

  condition {
    path_pattern {
      values = ["/api/*"]
    }
  }

  lifecycle {
    ignore_changes = all
  }
}

# Listener Rule para Backend Auth - /auth/* (HTTPS)
resource "aws_lb_listener_rule" "backend_auth_https" {
  count = var.enable_https ? 1 : 0
  
  listener_arn = local.https_listener_arn
  priority     = 110

  action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.main.arn
  }

  condition {
    path_pattern {
      values = ["/auth/*"]
    }
  }

  lifecycle {
    ignore_changes = all
  }
}

# Listener Rule para Backend Usuarios - /usuarios/* (HTTPS)
resource "aws_lb_listener_rule" "backend_usuarios_https" {
  count = var.enable_https ? 1 : 0
  
  listener_arn = local.https_listener_arn
  priority     = 120

  action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.main.arn
  }

  condition {
    path_pattern {
      values = ["/usuarios/*"]
    }
  }

  lifecycle {
    ignore_changes = all
  }
}

# Listener Rule para Swagger (HTTPS)
resource "aws_lb_listener_rule" "backend_swagger_https" {
  count = var.enable_https ? 1 : 0
  
  listener_arn = local.https_listener_arn
  priority     = 130

  action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.main.arn
  }

  condition {
    path_pattern {
      values = ["/swagger*", "/swagger/*"]
    }
  }

  lifecycle {
    ignore_changes = all
  }
}

# Listener Rule para Health Check (HTTPS)
resource "aws_lb_listener_rule" "backend_health_https" {
  count = var.enable_https ? 1 : 0
  
  listener_arn = local.https_listener_arn
  priority     = 140

  action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.main.arn
  }

  condition {
    path_pattern {
      values = ["/health"]
    }
  }

  lifecycle {
    ignore_changes = all
  }
}

# ECS Service
resource "aws_ecs_service" "main" {
  name            = "${var.project_name}-api-service"
  cluster         = aws_ecs_cluster.main.id
  task_definition = aws_ecs_task_definition.main.arn
  desired_count   = 1
  launch_type     = "FARGATE"

  network_configuration {
    subnets          = [aws_subnet.public_1.id, aws_subnet.public_2.id]
    security_groups  = [aws_security_group.ecs.id]
    assign_public_ip = true
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.main.arn
    container_name   = "${var.project_name}-api"
    container_port   = 5011
  }

  # Configuración de despliegue
  deployment_maximum_percent         = 200
  deployment_minimum_healthy_percent = 100

  depends_on = [
    aws_lb_listener.main,
    aws_iam_role_policy.ecs_execution_secrets
  ]

  lifecycle {
    ignore_changes = all
  }

  tags = {
    Name        = "${var.project_name}-api-service"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }
}

# ============================================
# FRONTEND ECS RESOURCES
# ============================================

# ECS Task Definition para Frontend
resource "aws_ecs_task_definition" "frontend" {
  family                   = "${var.project_name}-frontend"
  requires_compatibilities = ["FARGATE"]
  network_mode             = "awsvpc"
  cpu                      = "256" # 0.25 vCPU
  memory                   = "512" # 0.5 GB
  execution_role_arn       = aws_iam_role.ecs_execution.arn
  task_role_arn            = aws_iam_role.ecs_task.arn

  container_definitions = jsonencode([
    {
      name  = "${var.project_name}-frontend"
      image = "${aws_ecr_repository.gatekeep_frontend.repository_url}:latest"

      portMappings = [
        {
          containerPort = 3000
          protocol      = "tcp"
        }
      ]

      environment = [
        {
          name  = "NODE_ENV"
          value = "production"
        },
        {
          name  = "PORT"
          value = "3000"
        },
        {
          name  = "NEXT_PUBLIC_API_URL"
          value = "https://api.${var.domain_name}"
        }
      ]

      logConfiguration = {
        logDriver = "awslogs"
        options = {
          "awslogs-group"         = aws_cloudwatch_log_group.ecs.name
          "awslogs-region"        = var.aws_region
          "awslogs-stream-prefix" = "ecs-frontend"
        }
      }

      healthCheck = {
        command     = ["CMD-SHELL", "node -e \"require('http').get('http://localhost:3000', (r) => {process.exit(r.statusCode === 200 ? 0 : 1)}).on('error', () => process.exit(1))\" || exit 1"]
        interval    = 30
        timeout     = 5
        retries     = 3
        startPeriod = 90
      }
    }
  ])

  tags = {
    Name        = "${var.project_name}-frontend-task"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }
}

# Security Group para Frontend ECS (mismo que backend, puede compartir)
# Usaremos el mismo security group para simplificar

# ECS Service para Frontend - HABILITADO (PWA funciona en ECS)
# Temporalmente usando ECS mientras esperamos propagación DNS de CloudFront
resource "aws_ecs_service" "frontend" {
  name            = "${var.project_name}-frontend-service"
  cluster         = aws_ecs_cluster.main.id
  task_definition = aws_ecs_task_definition.frontend.arn
  desired_count   = 1  # Habilitado - PWA funciona en ECS
  launch_type     = "FARGATE"

  network_configuration {
    subnets          = [aws_subnet.public_1.id, aws_subnet.public_2.id]
    security_groups  = [aws_security_group.ecs.id]
    assign_public_ip = true
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.frontend.arn
    container_name   = "${var.project_name}-frontend"
    container_port   = 3000
  }

  # Configuración de despliegue
  deployment_maximum_percent         = 200
  deployment_minimum_healthy_percent = 100

  depends_on = [
    aws_lb_listener.main,
    aws_lb_listener_rule.backend_api,
    aws_lb_listener_rule.backend_auth,
    aws_lb_listener_rule.backend_usuarios,
    aws_lb_listener_rule.backend_swagger,
    aws_lb_listener_rule.backend_health
  ]

  lifecycle {
    ignore_changes = all
  }

  tags = {
    Name        = "${var.project_name}-frontend-service"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }
}

