# Variables de configuración para Terraform

variable "aws_region" {
  description = "Región de AWS donde se desplegarán los recursos"
  type        = string
  default     = "sa-east-1" # São Paulo
}

variable "environment" {
  description = "Ambiente de despliegue (dev, staging, prod)"
  type        = string
  default     = "dev"
}

variable "project_name" {
  description = "Nombre del proyecto"
  type        = string
  default     = "gatekeep"
}

variable "domain_name" {
  description = "Dominio público que apuntará al ALB"
  type        = string
  default     = "zimmzimmgames.com"
}

variable "alternate_domain_names" {
  description = "Dominios alternos/SAN (ej. www.zimmzimmgames.com)"
  type        = list(string)
  default     = ["www.zimmzimmgames.com"]
}

variable "enable_https" {
  description = "Habilitar recursos HTTPS (certificado ACM y listener 443)"
  type        = bool
  default     = true
}

variable "manage_secret_versions" {
  description = "Si es true, Terraform rotará/regenerará el contenido de los secretos administrados"
  type        = bool
  default     = false
}

# Variables opcionales para credenciales (NO recomendado para producción)
# Es mejor usar AWS CLI configure o variables de entorno
variable "aws_access_key_id" {
  description = "AWS Access Key ID (opcional, mejor usar AWS CLI configure)"
  type        = string
  default     = ""
  sensitive   = true
}

variable "aws_secret_access_key" {
  description = "AWS Secret Access Key (opcional, mejor usar AWS CLI configure)"
  type        = string
  default     = ""
  sensitive   = true
}

variable "aws_profile" {
  description = "Perfil de AWS a usar (opcional, si tienes múltiples perfiles en ~/.aws/credentials)"
  type        = string
  default     = "" # Si está vacío, usa el perfil 'default' o variables de entorno
}

variable "alarm_actions" {
  description = "SNS Topic ARNs para acciones de alarmas de CloudWatch (alertas)"
  type        = list(string)
  default     = [] # Vacío por defecto, se puede configurar para enviar notificaciones
}

