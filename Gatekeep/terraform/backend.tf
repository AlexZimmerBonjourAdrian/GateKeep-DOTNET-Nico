# ============================================
# BACKEND REMOTO - STATE EN S3 + DYNAMODB LOCK
# ============================================
# Esto permite que múltiples desarrolladores
# trabajen con el mismo estado de Terraform
# COMENTADO: Se habilitará cuando se cree el bucket S3 y se configure el acceso

/*
terraform {
  backend "s3" {
    bucket         = "gatekeep-terraform-state"
    key            = "prod/terraform.tfstate"
    region         = "sa-east-1"
    encrypt        = true
    dynamodb_table = "gatekeep-terraform-locks"
  }
}
*/

