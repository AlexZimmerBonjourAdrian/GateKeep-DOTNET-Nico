# IAM Roles y Policies para App Runner y otros servicios
# NOTA: App Runner está comentado porque no está disponible en sa-east-1

# IAM Role para App Runner (comentado - App Runner no disponible en sa-east-1)
# resource "aws_iam_role" "apprunner" {
#   name = "${var.project_name}-apprunner-role"
#
#   assume_role_policy = jsonencode({
#     Version = "2012-10-17"
#     Statement = [
#       {
#         Action = "sts:AssumeRole"
#         Effect = "Allow"
#         Principal = {
#           Service = "build.apprunner.amazonaws.com"
#         }
#       },
#       {
#         Action = "sts:AssumeRole"
#         Effect = "Allow"
#         Principal = {
#           Service = "tasks.apprunner.amazonaws.com"
#         }
#       }
#     ]
#   })
#
#   tags = {
#     Name        = "${var.project_name}-apprunner-role"
#     Environment = var.environment
#     ManagedBy   = "Terraform"
#   }
# }

# Policy para que App Runner pueda acceder a ECR (comentado)
# resource "aws_iam_role_policy" "apprunner_ecr" {
#   name = "${var.project_name}-apprunner-ecr-policy"
#   role = aws_iam_role.apprunner.id
#
#   policy = jsonencode({
#     Version = "2012-10-17"
#     Statement = [
#       {
#         Effect = "Allow"
#         Action = [
#           "ecr:GetAuthorizationToken",
#           "ecr:BatchCheckLayerAvailability",
#           "ecr:GetDownloadUrlForLayer",
#           "ecr:BatchGetImage"
#         ]
#         Resource = "*"
#       }
#     ]
#   })
# }

# Policy para que App Runner pueda acceder a Secrets Manager (comentado)
# resource "aws_iam_role_policy" "apprunner_secrets" {
#   name = "${var.project_name}-apprunner-secrets-policy"
#   role = aws_iam_role.apprunner.id
#
#   policy = jsonencode({
#     Version = "2012-10-17"
#     Statement = [
#       {
#         Effect = "Allow"
#         Action = [
#           "secretsmanager:GetSecretValue",
#           "secretsmanager:DescribeSecret"
#         ]
#         Resource = [
#           aws_secretsmanager_secret.db_password.arn,
#           aws_secretsmanager_secret.jwt_key.arn
#         ]
#       }
#     ]
#   })
# }

# Policy para que App Runner pueda acceder a Parameter Store (comentado)
# resource "aws_iam_role_policy" "apprunner_ssm" {
#   name = "${var.project_name}-apprunner-ssm-policy"
#   role = aws_iam_role.apprunner.id
#
#   policy = jsonencode({
#     Version = "2012-10-17"
#     Statement = [
#       {
#         Effect = "Allow"
#         Action = [
#           "ssm:GetParameter",
#           "ssm:GetParameters",
#           "ssm:GetParametersByPath"
#         ]
#         Resource = [
#           "arn:aws:ssm:${var.aws_region}:*:parameter/${var.project_name}/*"
#         ]
#       }
#     ]
#   })
# }

# Policy para CloudWatch Logs (comentado)
# resource "aws_iam_role_policy" "apprunner_logs" {
#   name = "${var.project_name}-apprunner-logs-policy"
#   role = aws_iam_role.apprunner.id
#
#   policy = jsonencode({
#     Version = "2012-10-17"
#     Statement = [
#       {
#         Effect = "Allow"
#         Action = [
#           "logs:CreateLogGroup",
#           "logs:CreateLogStream",
#           "logs:PutLogEvents"
#         ]
#         Resource = "arn:aws:logs:${var.aws_region}:*:log-group:/aws/apprunner/${var.project_name}/*"
#       }
#     ]
#   })
# }
