# Amazon MQ RabbitMQ - Message broker para la aplicación
# COMENTADO TEMPORALMENTE - Causa problemas con atributos

# Broker RabbitMQ
# resource "aws_mq_broker" "main" {
#   broker_name         = "${var.project_name}-rabbitmq"
#   engine_type         = "RabbitMQ"
#   engine_version      = "3.11.20" # Última versión estable disponible
#   host_instance_type = "mq.t3.micro" # 0.5 vCPU, 0.7GB RAM
#   
#   # Usuarios
#   user {
#     username = "admin"
#     password = "temp-password-change-me" # Se actualizará manualmente o desde Secrets Manager después de crear el broker
#   }
#   
#   # Network
#   subnet_ids            = [aws_subnet.private_1.id, aws_subnet.private_2.id]
#   security_groups       = [aws_security_group.rabbitmq.id]
#   publicly_accessible   = false
#   
#   # Configuration
#   deployment_mode      = "SINGLE_INSTANCE" # Para reducir costos, usar SINGLE_INSTANCE
#   auto_minor_version_upgrade = true
#   
#   # Logs
#   logs {
#     general = true
#     audit   = false
#   }
#   
#   # Maintenance
#   maintenance_window_start_time {
#     day_of_week = "SUNDAY"
#     time_of_day = "05:00"
#     time_zone   = "UTC"
#   }
#   
#   tags = {
#     Name        = "${var.project_name}-rabbitmq"
#     Environment = var.environment
#     ManagedBy   = "Terraform"
#   }
#
#   lifecycle {
#     ignore_changes = all
#   }
# }

# Outputs movidos a outputs.tf para centralización

