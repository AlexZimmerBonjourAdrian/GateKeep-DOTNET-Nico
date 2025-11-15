# ECR (Elastic Container Registry) - Para almacenar imágenes Docker

resource "aws_ecr_repository" "gatekeep_api" {
  name                 = "${var.project_name}-api"
  image_tag_mutability = "MUTABLE"

  image_scanning_configuration {
    scan_on_push = true
  }

  encryption_configuration {
    encryption_type = "AES256"
  }

  tags = {
    Name        = "${var.project_name}-api"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }
}

resource "aws_ecr_lifecycle_policy" "gatekeep_api" {
  repository = aws_ecr_repository.gatekeep_api.name

  policy = jsonencode({
    rules = [
      {
        rulePriority = 1
        description  = "Mantener las últimas 10 imágenes"
        selection = {
          tagStatus   = "any"
          countType   = "imageCountMoreThan"
          countNumber = 10
        }
        action = {
          type = "expire"
        }
      }
    ]
  })
}

# ECR Repository para Frontend
resource "aws_ecr_repository" "gatekeep_frontend" {
  name                 = "${var.project_name}-frontend"
  image_tag_mutability = "MUTABLE"

  image_scanning_configuration {
    scan_on_push = true
  }

  encryption_configuration {
    encryption_type = "AES256"
  }

  tags = {
    Name        = "${var.project_name}-frontend"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }
}

resource "aws_ecr_lifecycle_policy" "gatekeep_frontend" {
  repository = aws_ecr_repository.gatekeep_frontend.name

  policy = jsonencode({
    rules = [
      {
        rulePriority = 1
        description  = "Mantener las últimas 10 imágenes"
        selection = {
          tagStatus   = "any"
          countType   = "imageCountMoreThan"
          countNumber = 10
        }
        action = {
          type = "expire"
        }
      }
    ]
  })
}

