# ============================================
# CLOUDFRONT DISTRIBUTION PARA FRONTEND
# ============================================
# Distribución CloudFront para servir el frontend desde S3
# Usado por los scripts de invalidación de caché

# Origin Access Control (OAC) para CloudFront
resource "aws_cloudfront_origin_access_control" "frontend" {
  name                              = "${var.project_name}-frontend-oac"
  description                       = "OAC para ${var.project_name} frontend S3 bucket"
  origin_access_control_origin_type = "s3"
  signing_behavior                  = "always"
  signing_protocol                  = "sigv4"
}

# CloudFront Distribution para Frontend
resource "aws_cloudfront_distribution" "frontend" {
  enabled             = true
  is_ipv6_enabled     = true
  comment             = "CloudFront distribution para ${var.project_name} frontend"
  default_root_object = "index.html"
  price_class         = "PriceClass_100"  # Solo US, Canada, Europe

  aliases = concat([var.domain_name], var.alternate_domain_names)

  origin {
    domain_name              = aws_s3_bucket.frontend.bucket_regional_domain_name
    origin_id                = "S3-${aws_s3_bucket.frontend.id}"
    origin_access_control_id = aws_cloudfront_origin_access_control.frontend.id
  }

  default_cache_behavior {
    allowed_methods  = ["DELETE", "GET", "HEAD", "OPTIONS", "PATCH", "POST", "PUT"]
    cached_methods   = ["GET", "HEAD"]
    target_origin_id = "S3-${aws_s3_bucket.frontend.id}"

    forwarded_values {
      query_string = false
      cookies {
        forward = "none"
      }
    }

    viewer_protocol_policy = "redirect-to-https"
    min_ttl                = 0
    default_ttl            = 3600
    max_ttl                = 86400
    compress               = true
  }

  # Cache behavior para archivos estáticos (JS, CSS, imágenes)
  ordered_cache_behavior {
    path_pattern     = "*.js"
    allowed_methods  = ["GET", "HEAD", "OPTIONS"]
    cached_methods   = ["GET", "HEAD"]
    target_origin_id = "S3-${aws_s3_bucket.frontend.id}"

    forwarded_values {
      query_string = false
      cookies {
        forward = "none"
      }
    }

    viewer_protocol_policy = "redirect-to-https"
    min_ttl                = 0
    default_ttl            = 31536000  # 1 año
    max_ttl                = 31536000
    compress               = true
  }

  ordered_cache_behavior {
    path_pattern     = "*.css"
    allowed_methods  = ["GET", "HEAD", "OPTIONS"]
    cached_methods   = ["GET", "HEAD"]
    target_origin_id = "S3-${aws_s3_bucket.frontend.id}"

    forwarded_values {
      query_string = false
      cookies {
        forward = "none"
      }
    }

    viewer_protocol_policy = "redirect-to-https"
    min_ttl                = 0
    default_ttl            = 31536000  # 1 año
    max_ttl                = 31536000
    compress               = true
  }

  ordered_cache_behavior {
    path_pattern     = "*.wasm"
    allowed_methods  = ["GET", "HEAD", "OPTIONS"]
    cached_methods   = ["GET", "HEAD"]
    target_origin_id = "S3-${aws_s3_bucket.frontend.id}"

    forwarded_values {
      query_string = false
      cookies {
        forward = "none"
      }
      headers = ["Origin"]
    }

    viewer_protocol_policy = "redirect-to-https"
    min_ttl                = 0
    default_ttl            = 31536000  # 1 año
    max_ttl                = 31536000
    compress               = false  # WASM ya está comprimido
  }

  # Error pages
  custom_error_response {
    error_code         = 404
    response_code      = 200
    response_page_path = "/index.html"
  }

  custom_error_response {
    error_code         = 403
    response_code      = 200
    response_page_path = "/index.html"
  }

  restrictions {
    geo_restriction {
      restriction_type = "none"
    }
  }

  viewer_certificate {
    acm_certificate_arn      = var.enable_https ? aws_acm_certificate.cloudfront[0].arn : null
    ssl_support_method       = var.enable_https ? "sni-only" : null
    minimum_protocol_version = var.enable_https ? "TLSv1.2_2021" : null
  }

  tags = {
    Name        = "${var.project_name}-frontend-cdn"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }

  lifecycle {
    ignore_changes = [
      viewer_certificate,
      aliases
    ]
  }
}

# ACM Certificate para CloudFront (debe estar en us-east-1)
resource "aws_acm_certificate" "cloudfront" {
  count = var.enable_https ? 1 : 0

  provider = aws.us_east_1  # CloudFront requiere certificados en us-east-1

  domain_name       = var.domain_name
  validation_method = "DNS"

  subject_alternative_names = var.alternate_domain_names

  lifecycle {
    create_before_destroy = true
  }

  tags = {
    Name        = "${var.project_name}-cloudfront-cert"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }
}

# Validación del certificado ACM
resource "aws_acm_certificate_validation" "cloudfront" {
  count = var.enable_https ? 1 : 0

  provider = aws.us_east_1

  certificate_arn = aws_acm_certificate.cloudfront[0].arn

  timeouts {
    create = "5m"
  }
}

# Nota: El provider para us-east-1 ya está definido en main.tf

