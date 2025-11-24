# ============================================
# S3 BACKEND PARA TERRAFORM STATE
# ============================================
# Bucket S3 y tabla DynamoDB para almacenar el estado de Terraform
# Estos recursos deben crearse ANTES de configurar el backend remoto
# Usar: terraform/setup-remote-backend.ps1 para crearlos

# S3 Bucket para Terraform State
resource "aws_s3_bucket" "terraform_state" {
  bucket = "${var.project_name}-terraform-state"

  tags = {
    Name        = "${var.project_name}-terraform-state"
    Environment = var.environment
    ManagedBy   = "Terraform"
    Purpose     = "Terraform State Storage"
  }

  lifecycle {
    prevent_destroy = true
  }
}

# Habilitar versionado del bucket
resource "aws_s3_bucket_versioning" "terraform_state" {
  bucket = aws_s3_bucket.terraform_state.id

  versioning_configuration {
    status = "Enabled"
  }
}

# Bloquear acceso público al bucket
resource "aws_s3_bucket_public_access_block" "terraform_state" {
  bucket = aws_s3_bucket.terraform_state.id

  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

# Encriptación del bucket
resource "aws_s3_bucket_server_side_encryption_configuration" "terraform_state" {
  bucket = aws_s3_bucket.terraform_state.id

  rule {
    apply_server_side_encryption_by_default {
      sse_algorithm = "AES256"
    }
  }
}

# Tabla DynamoDB para locks de Terraform
resource "aws_dynamodb_table" "terraform_locks" {
  name         = "${var.project_name}-terraform-locks"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "LockID"

  attribute {
    name = "LockID"
    type = "S"
  }

  tags = {
    Name        = "${var.project_name}-terraform-locks"
    Environment = var.environment
    ManagedBy   = "Terraform"
    Purpose     = "Terraform State Locking"
  }

  lifecycle {
    prevent_destroy = true
  }
}

