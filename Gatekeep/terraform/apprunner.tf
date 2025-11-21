# AWS App Runner - Servicio para ejecutar la aplicación
# NOTA: App Runner NO está disponible en sa-east-1 (São Paulo)
# Solo disponible en: us-east-1, us-east-2, us-west-2, eu-west-1, ap-southeast-1, ap-northeast-1
# Este recurso está comentado porque la región es sa-east-1
# Para usar App Runner, cambia la región a una de las soportadas

# Auto Scaling Configuration
# resource "aws_apprunner_auto_scaling_configuration_version" "main" {
#   auto_scaling_configuration_name = "${var.project_name}-autoscaling"
#
#   max_concurrency = 100
#   max_size        = 10
#   min_size        = 1
#
#   tags = {
#     Name        = "${var.project_name}-autoscaling"
#     Environment = var.environment
#     ManagedBy   = "Terraform"
#   }
# }

# App Runner Service
# resource "aws_apprunner_service" "main" {
#   service_name = "${var.project_name}-api"
#   
#   # Source Configuration - ECR
#   source_configuration {
#     image_repository {
#       image_identifier      = "${aws_ecr_repository.gatekeep_api.repository_url}:latest"
#       image_configuration {
#         port = "5011"
#         runtime_environment_variables = {
#           ASPNETCORE_ENVIRONMENT = var.environment
#           ASPNETCORE_URLS        = "http://+:5011"
#         }
#         runtime_environment_secrets = {
#           DATABASE__PASSWORD = aws_secretsmanager_secret.db_password.arn
#           JWT__KEY          = aws_secretsmanager_secret.jwt_key.arn
#         }
#       }
#       image_repository_type = "ECR"
#     }
#     auto_deployments_enabled = true
#   }
#
#   # Instance Configuration
#   instance_configuration {
#     cpu               = "1 vCPU"
#     memory            = "2 GB"
#     instance_role_arn = aws_iam_role.apprunner.arn
#   }
#
#   # Auto Scaling
#   auto_scaling_configuration_arn = aws_apprunner_auto_scaling_configuration_version.main.arn
#
#   # Health Check
#   health_check_configuration {
#     protocol            = "HTTP"
#     path                = "/health"
#     healthy_threshold   = 1
#     unhealthy_threshold = 5
#     interval            = 10
#     timeout             = 5
#   }
#
#   # Network Configuration (opcional - para acceder a RDS privado)
#   # Descomentar si necesitas acceso a RDS en VPC privada
#   # network_configuration {
#   #   egress_configuration {
#   #     egress_type       = "VPC"
#   #     vpc_connector_arn = aws_apprunner_vpc_connector.main.arn
#   #   }
#   # }
#
#   tags = {
#     Name        = "${var.project_name}-api"
#     Environment = var.environment
#     ManagedBy   = "Terraform"
#   }
#
#   # Dependencias
#   depends_on = [
#     aws_ecr_repository.gatekeep_api,
#     aws_db_instance.main,
#     aws_secretsmanager_secret_version.db_password,
#     aws_secretsmanager_secret_version.jwt_key
#   ]
# }

# VPC Connector para App Runner (opcional - solo si necesitas acceso a RDS privado)
# Descomentar si necesitas que App Runner acceda a RDS en VPC privada
# resource "aws_apprunner_vpc_connector" "main" {
#   vpc_connector_name = "${var.project_name}-vpc-connector"
#   subnets            = [aws_subnet.private_1.id, aws_subnet.private_2.id]
#   security_groups    = [aws_security_group.apprunner_connector.id]
# }
