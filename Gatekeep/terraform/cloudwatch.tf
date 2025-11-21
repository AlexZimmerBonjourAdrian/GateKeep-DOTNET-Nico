# CloudWatch Configuration for GateKeep Redis Cache Metrics
# Propósito: Crear dashboard y alarmas para monitorear métricas de cache Redis
# Namespace: GateKeep/Redis

# ============================================
# CloudWatch Log Group para métricas customizadas
# ============================================
# Nota: El CloudWatch Log Group para ECS ya está definido en ecs.tf
# Este archivo se enfoca en métricas de cache y dashboards

# ============================================
# CloudWatch Dashboard para métricas de cache
# ============================================
resource "aws_cloudwatch_dashboard" "cache_metrics" {
  dashboard_name = "${var.project_name}-cache-metrics"

  dashboard_body = jsonencode({
    widgets = [
      # Widget: Cache Hit Rate (métrica más importante)
      {
        type = "metric"
        properties = {
          metrics = [
            ["GateKeep/Redis", "CacheHitRate", { stat = "Average" }]
          ]
          period = 60
          stat   = "Average"
          region = var.aws_region
          title  = "Cache Hit Rate (%)"
          yAxis = {
            left = {
              min = 0
              max = 100
            }
          }
          annotations = {
            horizontal = [
              {
                label = "Target Hit Rate (80%)"
                value = 80
                color = "#2ca02c"
              },
              {
                label = "Warning Threshold (50%)"
                value = 50
                color = "#ff7f0e"
              }
            ]
          }
        }
      },

      # Widget: Total Cache Hits vs Misses (comparación)
      {
        type = "metric"
        properties = {
          metrics = [
            ["GateKeep/Redis", "CacheHitsTotal", { stat = "Sum", label = "Total Hits" }],
            [".", "CacheMissesTotal", { stat = "Sum", label = "Total Misses" }]
          ]
          period = 300
          stat   = "Sum"
          region = var.aws_region
          title  = "Cache Hits vs Misses (5min)"
          yAxis = {
            left = {
              min = 0
            }
          }
        }
      },

      # Widget: Cache Operations Breakdown
      {
        type = "metric"
        properties = {
          metrics = [
            ["GateKeep/Redis", "CacheHitsTotal", { stat = "Sum" }],
            [".", "CacheMissesTotal", { stat = "Sum" }],
            [".", "CacheInvalidationsTotal", { stat = "Sum" }]
          ]
          period = 300
          stat   = "Sum"
          region = var.aws_region
          title  = "Cache Operations Breakdown"
        }
      },

      # Widget: Cache Hit Rate Trend (últimas 24 horas)
      {
        type = "metric"
        properties = {
          metrics = [
            ["GateKeep/Redis", "CacheHitRate", { stat = "Average" }]
          ]
          period = 300
          stat   = "Average"
          region = var.aws_region
          title  = "Cache Hit Rate Trend (24h)"
          start  = "-PT24H"
          end    = "P0D"
        }
      },

      # Widget: Cache Hits by Key (top keys)
      {
        type = "metric"
        properties = {
          metrics = [
            ["GateKeep/Redis", "CacheHitsByKey", { stat = "Sum" }]
          ]
          period = 300
          stat   = "Sum"
          region = var.aws_region
          title  = "Top Cache Keys (Hits)"
        }
      },

      # Widget: Cache Invalidations
      {
        type = "metric"
        properties = {
          metrics = [
            ["GateKeep/Redis", "CacheInvalidationsTotal", { stat = "Sum" }]
          ]
          period = 300
          stat   = "Sum"
          region = var.aws_region
          title  = "Cache Invalidations (5min)"
        }
      },

      # Widget: Logs - Cache Operations
      {
        type = "log"
        properties = {
          query  = "fields @timestamp, @message | filter @message like /\\[CACHE\\]/ | stats count() by @message"
          region = var.aws_region
          title  = "Cache Operations Log Summary"
        }
      },

      # Widget: API Response Health (CloudWatch Logs Insights)
      {
        type = "log"
        properties = {
          query  = "fields @duration | stats avg(@duration) as avg_duration, max(@duration) as max_duration, pct(@duration, 95) as p95_duration"
          region = var.aws_region
          title  = "API Response Time Metrics"
        }
      }
    ]
  })
}

# ============================================
# CloudWatch Alarms para cache metrics
# ============================================

# Alarma: Hit Rate bajo (< 50%)
resource "aws_cloudwatch_metric_alarm" "low_cache_hit_rate" {
  alarm_name          = "${var.project_name}-low-cache-hit-rate"
  comparison_operator = "LessThanThreshold"
  evaluation_periods  = 2
  metric_name         = "CacheHitRate"
  namespace           = "GateKeep/Redis"
  period              = 300
  statistic           = "Average"
  threshold           = 50
  alarm_description   = "Alarma cuando el cache hit rate cae por debajo del 50%"
  treat_missing_data  = "notBreaching"

  dimensions = {
    Environment = var.environment
    Service     = "GateKeepAPI"
  }

  tags = {
    Name        = "${var.project_name}-low-cache-hit-rate"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }
}

# Alarma: Hit Rate muy bajo (< 30%)
resource "aws_cloudwatch_metric_alarm" "critical_cache_hit_rate" {
  alarm_name          = "${var.project_name}-critical-cache-hit-rate"
  comparison_operator = "LessThanThreshold"
  evaluation_periods  = 1
  metric_name         = "CacheHitRate"
  namespace           = "GateKeep/Redis"
  period              = 60
  statistic           = "Average"
  threshold           = 30
  alarm_description   = "Alarma CRÍTICA cuando el cache hit rate cae por debajo del 30%"
  treat_missing_data  = "notBreaching"
  alarm_actions       = var.alarm_actions != null ? var.alarm_actions : []

  dimensions = {
    Environment = var.environment
    Service     = "GateKeepAPI"
  }

  tags = {
    Name        = "${var.project_name}-critical-cache-hit-rate"
    Environment = var.environment
    Severity    = "CRITICAL"
    ManagedBy   = "Terraform"
  }
}

# Alarma: Invalidaciones altas (posible cache trashing)
resource "aws_cloudwatch_metric_alarm" "high_cache_invalidations" {
  alarm_name          = "${var.project_name}-high-cache-invalidations"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 2
  metric_name         = "CacheInvalidationsTotal"
  namespace           = "GateKeep/Redis"
  period              = 300
  statistic           = "Sum"
  threshold           = 100
  alarm_description   = "Alarma cuando hay muchas invalidaciones de cache (posible thrashing)"
  treat_missing_data  = "notBreaching"

  dimensions = {
    Environment = var.environment
    Service     = "GateKeepAPI"
  }

  tags = {
    Name        = "${var.project_name}-high-cache-invalidations"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }
}

# Alarma: Misses creciendo constantemente
resource "aws_cloudwatch_metric_alarm" "high_cache_misses" {
  alarm_name          = "${var.project_name}-high-cache-misses"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 3
  metric_name         = "CacheMissesTotal"
  namespace           = "GateKeep/Redis"
  period              = 300
  statistic           = "Sum"
  threshold           = 500
  alarm_description   = "Alarma cuando hay demasiados misses de cache"
  treat_missing_data  = "notBreaching"

  dimensions = {
    Environment = var.environment
    Service     = "GateKeepAPI"
  }

  tags = {
    Name        = "${var.project_name}-high-cache-misses"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }
}

# ============================================
# CloudWatch Log Groups Filter (opcional para búsquedas)
# ============================================

# Metric filter para contar Cache Hits
resource "aws_cloudwatch_log_metric_filter" "cache_hits_log_filter" {
  name           = "${var.project_name}-cache-hits"
  log_group_name = aws_cloudwatch_log_group.ecs.name
  pattern        = "\"[CACHE]\" \"cache\" \"hit\""

  metric_transformation {
    name      = "CacheHitLogCount"
    namespace = "GateKeep/Redis/Logs"
    value     = "1"
  }
}

# Metric filter para contar Cache Misses
resource "aws_cloudwatch_log_metric_filter" "cache_misses_log_filter" {
  name           = "${var.project_name}-cache-misses"
  log_group_name = aws_cloudwatch_log_group.ecs.name
  pattern        = "\"[CACHE]\" \"cache\" \"miss\""

  metric_transformation {
    name      = "CacheMissLogCount"
    namespace = "GateKeep/Redis/Logs"
    value     = "1"
  }
}

# Metric filter para contar Cache Removals
resource "aws_cloudwatch_log_metric_filter" "cache_removals_log_filter" {
  name           = "${var.project_name}-cache-removed"
  log_group_name = aws_cloudwatch_log_group.ecs.name
  pattern        = "\"[CACHE]\" \"cache\" \"removed\""

  metric_transformation {
    name      = "CacheRemovalLogCount"
    namespace = "GateKeep/Redis/Logs"
    value     = "1"
  }
}

# ============================================
# Composite Alarm (combina múltiples alarmas)
# ============================================

resource "aws_cloudwatch_composite_alarm" "cache_health_overall" {
  alarm_name        = "${var.project_name}-cache-health-overall"
  alarm_description = "Estado general de salud del cache (combina múltiples alarmas)"
  actions_enabled   = true
  alarm_actions     = var.alarm_actions != null ? var.alarm_actions : []

  alarm_rule = "ALARM(\"${aws_cloudwatch_metric_alarm.critical_cache_hit_rate.alarm_name}\") OR ALARM(\"${aws_cloudwatch_metric_alarm.high_cache_invalidations.alarm_name}\")"

  tags = {
    Name        = "${var.project_name}-cache-health-overall"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }
}

# ============================================
# Datos (data sources) requeridos
# ============================================

data "aws_caller_identity" "current" {}

# Referencias a resources de otros archivos (importadas)
# - aws_cloudwatch_log_group.ecs (definido en ecs.tf)
# - var.aws_region (definido en variables.tf)
# - var.project_name (definido en variables.tf)
# - var.environment (definido en variables.tf)
# - var.alarm_actions (definido en variables.tf)

# ============================================
# CLOUDWATCH PARA RABBITMQ
# ============================================

# CloudWatch Dashboard para métricas de RabbitMQ
resource "aws_cloudwatch_dashboard" "rabbitmq_metrics" {
  dashboard_name = "${var.project_name}-rabbitmq-metrics"

  dashboard_body = jsonencode({
    widgets = [
      # Widget: Total Queue Depth (mensajes pendientes)
      {
        type = "metric"
        properties = {
          metrics = [
            ["GateKeep/RabbitMQ", "TotalQueueDepth", { stat = "Average" }]
          ]
          period = 60
          stat   = "Average"
          region = var.aws_region
          title  = "Total Queue Depth (Mensajes Pendientes)"
          yAxis = {
            left = {
              min = 0
            }
          }
        }
      },

      # Widget: Mensajes Publicados vs Consumidos
      {
        type = "metric"
        properties = {
          metrics = [
            ["GateKeep/RabbitMQ", "TotalMessagesPublished", { stat = "Sum", label = "Publicados" }],
            [".", "TotalMessagesConsumed", { stat = "Sum", label = "Consumidos" }]
          ]
          period = 300
          stat   = "Sum"
          region = var.aws_region
          title  = "Mensajes Publicados vs Consumidos (5min)"
          yAxis = {
            left = {
              min = 0
            }
          }
        }
      },

      # Widget: DLQ Messages (crítico)
      {
        type = "metric"
        properties = {
          metrics = [
            ["GateKeep/RabbitMQ", "TotalDLQMessages", { stat = "Average" }]
          ]
          period = 60
          stat   = "Average"
          region = var.aws_region
          title  = "Dead Letter Queue Messages"
          yAxis = {
            left = {
              min = 0
            }
          }
          annotations = {
            horizontal = [
              {
                label = "Warning Threshold (10)"
                value = 10
                color = "#ff7f0e"
              }
            ]
          }
        }
      },

      # Widget: Queue Depth por Cola (acceso-rechazado-queue)
      {
        type = "metric"
        properties = {
          metrics = [
            ["GateKeep/RabbitMQ", "QueueDepth", "QueueName", "acceso-rechazado-queue", { stat = "Average" }]
          ]
          period = 60
          stat   = "Average"
          region = var.aws_region
          title  = "Queue Depth - Acceso Rechazado"
          yAxis = {
            left = {
              min = 0
            }
          }
        }
      },

      # Widget: Queue Depth por Cola (beneficio-canjeado-queue)
      {
        type = "metric"
        properties = {
          metrics = [
            ["GateKeep/RabbitMQ", "QueueDepth", "QueueName", "beneficio-canjeado-queue", { stat = "Average" }]
          ]
          period = 60
          stat   = "Average"
          region = var.aws_region
          title  = "Queue Depth - Beneficio Canjeado"
          yAxis = {
            left = {
              min = 0
            }
          }
        }
      },

      # Widget: Consumers por Cola (acceso-rechazado-queue)
      {
        type = "metric"
        properties = {
          metrics = [
            ["GateKeep/RabbitMQ", "QueueConsumers", "QueueName", "acceso-rechazado-queue", { stat = "Average" }]
          ]
          period = 60
          stat   = "Average"
          region = var.aws_region
          title  = "Consumers - Acceso Rechazado"
          yAxis = {
            left = {
              min = 0
            }
          }
        }
      },

      # Widget: Consumers por Cola (beneficio-canjeado-queue)
      {
        type = "metric"
        properties = {
          metrics = [
            ["GateKeep/RabbitMQ", "QueueConsumers", "QueueName", "beneficio-canjeado-queue", { stat = "Average" }]
          ]
          period = 60
          stat   = "Average"
          region = var.aws_region
          title  = "Consumers - Beneficio Canjeado"
          yAxis = {
            left = {
              min = 0
            }
          }
        }
      },

      # Widget: Logs - RabbitMQ Events
      {
        type = "log"
        properties = {
          query  = "fields @timestamp, @message | filter @message like /RabbitMQ/ | stats count() by @message"
          region = var.aws_region
          title  = "RabbitMQ Events Log Summary"
        }
      }
    ]
  })
}

# ============================================
# CloudWatch Alarms para RabbitMQ
# ============================================

# Alarma: DLQ tiene mensajes (crítico)
resource "aws_cloudwatch_metric_alarm" "rabbitmq_dlq_messages" {
  alarm_name          = "${var.project_name}-rabbitmq-dlq-messages"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 1
  metric_name         = "TotalDLQMessages"
  namespace           = "GateKeep/RabbitMQ"
  period              = 60
  statistic           = "Average"
  threshold           = 1
  alarm_description   = "Alarma CRÍTICA cuando hay mensajes en Dead Letter Queue"
  treat_missing_data  = "notBreaching"
  alarm_actions       = var.alarm_actions != null ? var.alarm_actions : []

  dimensions = {
    Environment = var.environment
    Service     = "GateKeepAPI"
  }

  tags = {
    Name        = "${var.project_name}-rabbitmq-dlq-messages"
    Environment = var.environment
    Severity    = "CRITICAL"
    ManagedBy   = "Terraform"
  }
}

# Alarma: Queue Depth muy alto (cola llena)
resource "aws_cloudwatch_metric_alarm" "rabbitmq_high_queue_depth" {
  alarm_name          = "${var.project_name}-rabbitmq-high-queue-depth"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 2
  metric_name         = "TotalQueueDepth"
  namespace           = "GateKeep/RabbitMQ"
  period              = 300
  statistic           = "Average"
  threshold           = 1000
  alarm_description   = "Alarma cuando el total de mensajes en colas excede 1000"
  treat_missing_data  = "notBreaching"

  dimensions = {
    Environment = var.environment
    Service     = "GateKeepAPI"
  }

  tags = {
    Name        = "${var.project_name}-rabbitmq-high-queue-depth"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }
}

# Alarma: Sin consumidores activos
resource "aws_cloudwatch_metric_alarm" "rabbitmq_no_consumers" {
  alarm_name          = "${var.project_name}-rabbitmq-no-consumers"
  comparison_operator = "LessThanThreshold"
  evaluation_periods  = 2
  metric_name         = "QueueConsumers"
  namespace           = "GateKeep/RabbitMQ"
  period              = 300
  statistic           = "Minimum"
  threshold           = 1
  alarm_description   = "Alarma cuando no hay consumidores activos en las colas principales"
  treat_missing_data  = "breaching"

  dimensions = {
    Environment = var.environment
    Service     = "GateKeepAPI"
    QueueName   = "acceso-rechazado-queue"
  }

  tags = {
    Name        = "${var.project_name}-rabbitmq-no-consumers"
    Environment = var.environment
    Severity    = "HIGH"
    ManagedBy   = "Terraform"
  }
}

# Metric filter para eventos de RabbitMQ en logs
resource "aws_cloudwatch_log_metric_filter" "rabbitmq_events_log_filter" {
  name           = "${var.project_name}-rabbitmq-events"
  log_group_name = aws_cloudwatch_log_group.ecs.name
  pattern        = "\"RabbitMQ\""

  metric_transformation {
    name      = "RabbitMQEventLogCount"
    namespace = "GateKeep/RabbitMQ/Logs"
    value     = "1"
  }
}

# Metric filter para errores de RabbitMQ
resource "aws_cloudwatch_log_metric_filter" "rabbitmq_errors_log_filter" {
  name           = "${var.project_name}-rabbitmq-errors"
  log_group_name = aws_cloudwatch_log_group.ecs.name
  pattern        = "\"RabbitMQ\" \"Error\""

  metric_transformation {
    name      = "RabbitMQErrorLogCount"
    namespace = "GateKeep/RabbitMQ/Logs"
    value     = "1"
  }
}

# Composite Alarm para salud general de RabbitMQ
resource "aws_cloudwatch_composite_alarm" "rabbitmq_health_overall" {
  alarm_name        = "${var.project_name}-rabbitmq-health-overall"
  alarm_description = "Estado general de salud de RabbitMQ (combina múltiples alarmas)"
  actions_enabled   = true
  alarm_actions     = var.alarm_actions != null ? var.alarm_actions : []

  alarm_rule = "ALARM(\"${aws_cloudwatch_metric_alarm.rabbitmq_dlq_messages.alarm_name}\") OR ALARM(\"${aws_cloudwatch_metric_alarm.rabbitmq_no_consumers.alarm_name}\")"

  tags = {
    Name        = "${var.project_name}-rabbitmq-health-overall"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }
}
