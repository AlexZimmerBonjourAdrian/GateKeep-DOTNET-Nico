# ============================================
# WEB APPLICATION FIREWALL (WAF)
# ============================================
# Protección adicional contra ataques comunes (SQL injection, XSS, etc.)
# Opcional: Se habilita con var.enable_waf = true

# WAF Web ACL
resource "aws_wafv2_web_acl" "main" {
  count    = var.enable_waf ? 1 : 0
  name     = "${var.project_name}-waf"
  scope    = "REGIONAL"
  provider = aws

  default_action {
    allow {}
  }

  # Regla: AWS Managed Rules - Core Rule Set
  rule {
    name     = "AWSManagedRulesCommonRuleSet"
    priority = 1

    override_action {
      none {}
    }

    statement {
      managed_rule_group_statement {
        name        = "AWSManagedRulesCommonRuleSet"
        vendor_name = "AWS"
      }
    }

    visibility_config {
      cloudwatch_metrics_enabled = true
      metric_name                = "${var.project_name}-CommonRuleSetMetric"
      sampled_requests_enabled    = true
    }
  }

  # Regla: AWS Managed Rules - Known Bad Inputs
  rule {
    name     = "AWSManagedRulesKnownBadInputsRuleSet"
    priority = 2

    override_action {
      none {}
    }

    statement {
      managed_rule_group_statement {
        name        = "AWSManagedRulesKnownBadInputsRuleSet"
        vendor_name = "AWS"
      }
    }

    visibility_config {
      cloudwatch_metrics_enabled = true
      metric_name                = "${var.project_name}-KnownBadInputsMetric"
      sampled_requests_enabled    = true
    }
  }

  # Regla: AWS Managed Rules - Linux Operating System
  rule {
    name     = "AWSManagedRulesLinuxRuleSet"
    priority = 3

    override_action {
      none {}
    }

    statement {
      managed_rule_group_statement {
        name        = "AWSManagedRulesLinuxRuleSet"
        vendor_name = "AWS"
      }
    }

    visibility_config {
      cloudwatch_metrics_enabled = true
      metric_name                = "${var.project_name}-LinuxRuleSetMetric"
      sampled_requests_enabled    = true
    }
  }

  # Regla: Rate Limiting (protección contra DDoS)
  rule {
    name     = "RateLimitRule"
    priority = 4

    action {
      block {}
    }

    statement {
      rate_based_statement {
        limit              = 2000 # Máximo 2000 requests por 5 minutos por IP
        aggregate_key_type = "IP"
      }
    }

    visibility_config {
      cloudwatch_metrics_enabled = true
      metric_name                = "${var.project_name}-RateLimitMetric"
      sampled_requests_enabled    = true
    }
  }

  # Regla: Geo-blocking (opcional - bloquear países específicos)
  # Descomentar y ajustar según necesidades
  # rule {
  #   name     = "GeoBlockRule"
  #   priority = 5
  #
  #   action {
  #     block {}
  #   }
  #
  #   statement {
  #     geo_match_statement {
  #       country_codes = ["CN", "RU"] # Ejemplo: bloquear China y Rusia
  #     }
  #   }
  #
  #   visibility_config {
  #     cloudwatch_metrics_enabled = true
  #     metric_name                = "${var.project_name}-GeoBlockMetric"
  #     sampled_requests_enabled    = true
  #   }
  # }

  visibility_config {
    cloudwatch_metrics_enabled = true
    metric_name                = "${var.project_name}-WAFMetric"
    sampled_requests_enabled    = true
  }

  tags = {
    Name        = "${var.project_name}-waf"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }
}

# Asociar WAF con el ALB
resource "aws_wafv2_web_acl_association" "alb" {
  count        = var.enable_waf ? 1 : 0
  resource_arn = aws_lb.main.arn
  web_acl_arn  = aws_wafv2_web_acl.main[0].arn
}

# CloudWatch Log Group para WAF
resource "aws_cloudwatch_log_group" "waf" {
  count             = var.enable_waf ? 1 : 0
  name              = "/aws/waf/${var.project_name}"
  retention_in_days = 30

  tags = {
    Name        = "${var.project_name}-waf-logs"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }
}

# Configurar logging para WAF
resource "aws_wafv2_web_acl_logging_configuration" "main" {
  count                   = var.enable_waf ? 1 : 0
  resource_arn            = aws_wafv2_web_acl.main[0].arn
  log_destination_configs = [aws_cloudwatch_log_group.waf[0].arn]
}

