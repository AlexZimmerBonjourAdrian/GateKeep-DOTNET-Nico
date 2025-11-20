# ElastiCache Redis - Cache en memoria para la aplicación

# Subnet Group para ElastiCache (debe estar en subnets privadas)
resource "aws_elasticache_subnet_group" "main" {
  name       = "${var.project_name}-redis-subnet-group"
  subnet_ids = [aws_subnet.private_1.id, aws_subnet.private_2.id]

  tags = {
    Name        = "${var.project_name}-redis-subnet-group"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }

  lifecycle {
    ignore_changes = all
  }
}

# Parameter Group para Redis
resource "aws_elasticache_parameter_group" "main" {
  family = "redis7"
  name   = "${var.project_name}-redis-params"

  parameter {
    name  = "maxmemory-policy"
    value = "allkeys-lru"
  }

  tags = {
    Name        = "${var.project_name}-redis-params"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }

  lifecycle {
    ignore_changes = all
  }
}

# ElastiCache Redis Cluster
resource "aws_elasticache_replication_group" "main" {
  replication_group_id       = "${var.project_name}-redis"
  description                = "Redis cache para ${var.project_name}"
  
  # Engine
  engine                     = "redis"
  engine_version             = "7.0"
  node_type                 = "cache.t3.micro" # 0.5 vCPU, 0.6GB RAM
  
  # Configuration
  port                       = 6379
  parameter_group_name       = aws_elasticache_parameter_group.main.name
  subnet_group_name          = aws_elasticache_subnet_group.main.name
  security_group_ids         = [aws_security_group.redis.id]
  
  # High Availability (desactivado para reducir costos)
  num_cache_clusters         = 1
  automatic_failover_enabled = false
  multi_az_enabled          = false
  
  # Backup
  snapshot_retention_limit   = 1
  snapshot_window            = "03:00-05:00"
  
  # Maintenance
  maintenance_window         = "sun:05:00-sun:06:00"
  
  # Tags
  tags = {
    Name        = "${var.project_name}-redis"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }

  lifecycle {
    ignore_changes = all
  }
}

# Output para obtener la URL de conexión
output "redis_endpoint" {
  description = "Endpoint de ElastiCache Redis"
  value       = aws_elasticache_replication_group.main.configuration_endpoint_address != "" ? aws_elasticache_replication_group.main.configuration_endpoint_address : aws_elasticache_replication_group.main.primary_endpoint_address
}

output "redis_port" {
  description = "Puerto de ElastiCache Redis"
  value       = aws_elasticache_replication_group.main.port
}

