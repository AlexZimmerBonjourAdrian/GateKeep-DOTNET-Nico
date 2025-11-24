# ============================================
# S3 BUCKET PARA FRONTEND (STATIC HOSTING)
# ============================================
# Bucket S3 para almacenar el build estático del frontend
# Usado por los scripts: terraform/scripts/upload-frontend-to-s3.ps1 y .sh

# S3 Bucket para Frontend
resource "aws_s3_bucket" "frontend" {
  bucket = "${var.project_name}-frontend-${var.environment}"

  tags = {
    Name        = "${var.project_name}-frontend-${var.environment}"
    Environment = var.environment
    ManagedBy   = "Terraform"
    Purpose     = "Frontend Static Hosting"
  }

  lifecycle {
    ignore_changes = all
  }
}

# Configuración de website hosting (opcional, si se usa sin CloudFront)
resource "aws_s3_bucket_website_configuration" "frontend" {
  bucket = aws_s3_bucket.frontend.id

  index_document {
    suffix = "index.html"
  }

  error_document {
    key = "error.html"
  }
}

# Política de acceso público (solo lectura)
resource "aws_s3_bucket_policy" "frontend_public_read" {
  bucket = aws_s3_bucket.frontend.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Sid       = "PublicReadGetObject"
        Effect    = "Allow"
        Principal = "*"
        Action    = "s3:GetObject"
        Resource  = "${aws_s3_bucket.frontend.arn}/*"
      }
    ]
  })
}

# Bloquear acceso público (se desbloqueará solo para CloudFront)
resource "aws_s3_bucket_public_access_block" "frontend" {
  bucket = aws_s3_bucket.frontend.id

  block_public_acls       = true
  block_public_policy     = false  # Permitir política pública para CloudFront
  ignore_public_acls      = true
  restrict_public_buckets = false  # Permitir acceso público vía CloudFront
}

# CORS configuration para el frontend
resource "aws_s3_bucket_cors_configuration" "frontend" {
  bucket = aws_s3_bucket.frontend.id

  cors_rule {
    allowed_headers = ["*"]
    allowed_methods = ["GET", "HEAD"]
    allowed_origins = ["https://${var.domain_name}", "https://www.${var.domain_name}"]
    expose_headers  = ["ETag"]
    max_age_seconds = 3000
  }
}

# Lifecycle policy para limpiar archivos antiguos
resource "aws_s3_bucket_lifecycle_configuration" "frontend" {
  bucket = aws_s3_bucket.frontend.id

  rule {
    id     = "delete-old-versions"
    status = "Enabled"

    noncurrent_version_expiration {
      noncurrent_days = 30
    }
  }

  rule {
    id     = "delete-incomplete-multipart"
    status = "Enabled"

    abort_incomplete_multipart_upload {
      days_after_initiation = 7
    }
  }
}

