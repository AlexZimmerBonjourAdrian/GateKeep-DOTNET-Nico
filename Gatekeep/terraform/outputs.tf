# Outputs de Terraform
# Estos valores se mostrarán después de aplicar la configuración

output "aws_region" {
  description = "Región de AWS utilizada"
  value       = var.aws_region
}

output "environment" {
  description = "Ambiente de despliegue"
  value       = var.environment
}

