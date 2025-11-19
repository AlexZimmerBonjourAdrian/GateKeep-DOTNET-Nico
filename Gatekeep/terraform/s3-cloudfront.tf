# ============================================
# S3 + CLOUDFRONT PARA FRONTEND PWA
# ============================================

# S3 Bucket para Frontend estático
resource "aws_s3_bucket" "frontend" {
  bucket = "${var.project_name}-frontend-${var.environment}"

  tags = {
    Name        = "${var.project_name}-frontend"
    Environment = var.environment
    ManagedBy    = "Terraform"
  }

  # Ignorar cambios en configuraciones que no necesitamos gestionar
  # Esto evita errores de permisos para configuraciones que Terraform intenta leer pero no gestionamos
  lifecycle {
    ignore_changes = [
      # Ignorar aceleración de transferencia (requiere permisos especiales que no necesitamos)
    ]
  }
}

# Versioning para el bucket
resource "aws_s3_bucket_versioning" "frontend" {
  bucket = aws_s3_bucket.frontend.id

  versioning_configuration {
    status = "Enabled"
  }
}

# Configuración de CORS para el bucket
resource "aws_s3_bucket_cors_configuration" "frontend" {
  bucket = aws_s3_bucket.frontend.id

  cors_rule {
    allowed_headers = ["*"]
    allowed_methods = ["GET", "HEAD"]
    allowed_origins = ["https://${var.domain_name}", "https://www.${var.domain_name}"]
    expose_headers  = ["ETag"]
    max_age_seconds = 3600
  }
}

# Política pública para el bucket (solo lectura)
resource "aws_s3_bucket_public_access_block" "frontend" {
  bucket = aws_s3_bucket.frontend.id

  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

# Origin Access Control para CloudFront
resource "aws_cloudfront_origin_access_control" "frontend" {
  name                              = "${var.project_name}-frontend-oac"
  description                       = "OAC para ${var.project_name} frontend bucket"
  origin_access_control_origin_type = "s3"
  signing_behavior                  = "always"
  signing_protocol                  = "sigv4"
}

# Política del bucket para permitir acceso desde CloudFront
# Se crea después de CloudFront para evitar dependencia circular
resource "aws_s3_bucket_policy" "frontend" {
  bucket = aws_s3_bucket.frontend.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Sid    = "AllowCloudFrontServicePrincipal"
        Effect = "Allow"
        Principal = {
          Service = "cloudfront.amazonaws.com"
        }
        Action   = "s3:GetObject"
        Resource = "arn:aws:s3:::${aws_s3_bucket.frontend.bucket}/*"
        Condition = {
          StringLike = {
            "AWS:SourceArn" = "${aws_cloudfront_distribution.frontend.arn}/*"
          }
        }
      }
    ]
  })

  depends_on = [aws_cloudfront_distribution.frontend]
}

# Certificado ACM para CloudFront (debe estar en us-east-1)
resource "aws_acm_certificate" "cloudfront" {
  provider = aws.us_east_1

  domain_name               = var.domain_name
  subject_alternative_names = local.alternate_domains
  validation_method         = "DNS"

  lifecycle {
    create_before_destroy = true
  }

  tags = {
    Name        = "${var.project_name}-cloudfront-cert"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }
}

# Validación del certificado CloudFront
resource "aws_route53_record" "cloudfront_acm_validation" {
  for_each = {
    for dvo in aws_acm_certificate.cloudfront.domain_validation_options :
    dvo.domain_name => {
      name   = dvo.resource_record_name
      type   = dvo.resource_record_type
      record = dvo.resource_record_value
    }
  }

  allow_overwrite = true
  name            = each.value.name
  records         = [each.value.record]
  ttl             = 60
  type            = each.value.type
  zone_id         = data.aws_route53_zone.primary.zone_id
}

resource "aws_acm_certificate_validation" "cloudfront" {
  provider = aws.us_east_1

  certificate_arn         = aws_acm_certificate.cloudfront.arn
  validation_record_fqdns = [for record in aws_route53_record.cloudfront_acm_validation : record.fqdn]
}

# CloudFront Distribution
resource "aws_cloudfront_distribution" "frontend" {
  enabled             = true
  is_ipv6_enabled     = true
  comment             = "CloudFront distribution para ${var.project_name} frontend PWA"
  default_root_object = "index.html"
  price_class         = "PriceClass_100" # Solo Norteamérica y Europa (más barato)

  aliases = local.all_public_domains

  origin {
    domain_name              = "${aws_s3_bucket.frontend.bucket}.s3.${var.aws_region}.amazonaws.com"
    origin_id                = "S3-${aws_s3_bucket.frontend.bucket}"
    origin_access_control_id = aws_cloudfront_origin_access_control.frontend.id
  }

  # Comportamiento por defecto
  default_cache_behavior {
    allowed_methods        = ["GET", "HEAD", "OPTIONS"]
    cached_methods         = ["GET", "HEAD"]
    target_origin_id       = "S3-${aws_s3_bucket.frontend.bucket}"
    viewer_protocol_policy = "redirect-to-https"
    compress               = true

    # Cache Policy para optimizar
    cache_policy_id = aws_cloudfront_cache_policy.frontend_static.id

    # Headers importantes para PWA
    response_headers_policy_id = aws_cloudfront_response_headers_policy.frontend.id
  }

  # Custom Error Responses para fallback offline
  custom_error_response {
    error_code         = 403
    response_code      = 200
    response_page_path = "/offline.html"
    error_caching_min_ttl = 60
  }

  custom_error_response {
    error_code         = 404
    response_code      = 200
    response_page_path = "/offline.html"
    error_caching_min_ttl = 60
  }

  custom_error_response {
    error_code         = 500
    response_code      = 200
    response_page_path = "/offline.html"
    error_caching_min_ttl = 60
  }

  custom_error_response {
    error_code         = 502
    response_code      = 200
    response_page_path = "/offline.html"
    error_caching_min_ttl = 60
  }

  custom_error_response {
    error_code         = 503
    response_code      = 200
    response_page_path = "/offline.html"
    error_caching_min_ttl = 60
  }

  custom_error_response {
    error_code         = 504
    response_code      = 200
    response_page_path = "/offline.html"
    error_caching_min_ttl = 60
  }

  # Viewer Certificate (HTTPS)
  viewer_certificate {
    acm_certificate_arn      = aws_acm_certificate_validation.cloudfront.certificate_arn
    ssl_support_method       = "sni-only"
    minimum_protocol_version = "TLSv1.2_2021"
  }

  restrictions {
    geo_restriction {
      restriction_type = "none"
    }
  }

  tags = {
    Name        = "${var.project_name}-cloudfront"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }

  depends_on = [
    aws_acm_certificate_validation.cloudfront,
    aws_s3_bucket.frontend
  ]
}

# Response Headers Policy para CloudFront
# Incluye headers correctos para WASM, Service Workers y PWA
resource "aws_cloudfront_response_headers_policy" "frontend" {
  name = "${var.project_name}-frontend-headers"

  cors_config {
    access_control_allow_credentials = true
    access_control_allow_headers {
      items = [
        "Content-Type",
        "Authorization",
        "X-Requested-With",
        "Accept",
        "Origin",
      ]
    }
    access_control_allow_methods {
      items = ["GET", "HEAD", "OPTIONS"]
    }
    access_control_allow_origins {
      items = ["https://${var.domain_name}", "https://www.${var.domain_name}"]
    }
    access_control_max_age_sec = 3600
    origin_override            = true
  }

  # Los headers de seguridad están en security_headers_config, no aquí
  # custom_headers_config solo para headers personalizados no de seguridad

  security_headers_config {
    content_type_options {
      override = true
    }
    frame_options {
      frame_option = "DENY"
      override     = true
    }
    referrer_policy {
      referrer_policy = "strict-origin-when-cross-origin"
      override        = true
    }
    strict_transport_security {
      access_control_max_age_sec = 31536000
      include_subdomains         = true
      override                   = true
    }
  }
}

# Cache Policy para optimizar caché de assets estáticos
resource "aws_cloudfront_cache_policy" "frontend_static" {
  name        = "${var.project_name}-frontend-static-cache"
  comment     = "Cache policy para assets estáticos del frontend"
  default_ttl = 86400
  max_ttl     = 31536000
  min_ttl     = 0

  parameters_in_cache_key_and_forwarded_to_origin {
    enable_accept_encoding_brotli = true
    enable_accept_encoding_gzip   = true

    cookies_config {
      cookie_behavior = "none"
    }

    headers_config {
      header_behavior = "whitelist"
      headers {
        items = ["Origin", "Access-Control-Request-Headers", "Access-Control-Request-Method"]
      }
    }

    query_strings_config {
      query_string_behavior = "none"
    }
  }
}

