# Terraform Configuration para GateKeep

Esta carpeta contiene la configuración de Terraform para gestionar la infraestructura de GateKeep en AWS.

## Prerequisitos

1. **Terraform instalado** (v1.0 o superior)
   ```powershell
   terraform version
   ```

2. **AWS CLI configurado** (recomendado)
   ```powershell
   aws configure
   ```
   
   O configurar variables de entorno:
   ```powershell
   $env:AWS_ACCESS_KEY_ID = "tu-access-key-id"
   $env:AWS_SECRET_ACCESS_KEY = "tu-secret-access-key"
   $env:AWS_REGION = "sa-east-1"
   ```

## Configuración de Credenciales AWS

Terraform puede usar credenciales de AWS de varias formas (en orden de prioridad):

### 1. Variables de Entorno (Recomendado para CI/CD)

```powershell
$env:AWS_ACCESS_KEY_ID = "AKIA..."
$env:AWS_SECRET_ACCESS_KEY = "wJalr..."
$env:AWS_REGION = "sa-east-1"
```

### 2. AWS CLI Configure (Recomendado para desarrollo local)

```powershell
aws configure
```

Esto crea los archivos:
- `~/.aws/credentials`
- `~/.aws/config`

Terraform los leerá automáticamente.

### 3. Archivo de Credenciales Manual

Crear `~/.aws/credentials`:
```ini
[default]
aws_access_key_id = AKIA...
aws_secret_access_key = wJalr...
```

Crear `~/.aws/config`:
```ini
[default]
region = sa-east-1
output = json
```

### 4. Variables de Terraform (No recomendado)

Solo para pruebas. No usar en producción:
```hcl
provider "aws" {
  access_key = var.aws_access_key_id
  secret_key = var.aws_secret_access_key
  region     = var.aws_region
}
```

## Uso Básico

### Inicializar Terraform

```powershell
cd terraform
terraform init
```

Esto descargará el provider de AWS.

### Verificar Configuración

```powershell
terraform validate
terraform plan
```

### Aplicar Configuración

```powershell
terraform apply
```

### Destruir Recursos

```powershell
terraform destroy
```

## Verificar Conexión con AWS

Antes de usar Terraform, verifica que puedes conectarte a AWS:

```powershell
# Verificar identidad
aws sts get-caller-identity --region sa-east-1

# Ver configuración
aws configure list
```

## Estructura de Archivos

```
terraform/
├── main.tf          # Configuración principal y provider
├── variables.tf     # Variables de entrada
├── outputs.tf      # Valores de salida
├── .gitignore      # Archivos a ignorar en Git
└── README.md       # Esta documentación
```

## Próximos Pasos

1. **Agregar recursos**: Crear archivos para recursos específicos (RDS, ECR, App Runner, etc.)
2. **Configurar backend**: Usar S3 para almacenar el estado de Terraform
3. **Crear módulos**: Organizar recursos en módulos reutilizables
4. **Variables de entorno**: Configurar para diferentes ambientes (dev, staging, prod)

## Recursos Comunes para GateKeep

Basado en la documentación del proyecto, estos son los recursos que probablemente necesites:

- **ECR (Elastic Container Registry)**: Para almacenar imágenes Docker
- **RDS PostgreSQL**: Base de datos
- **Secrets Manager**: Para secretos
- **Parameter Store (SSM)**: Para parámetros de configuración
- **App Runner**: Para ejecutar la aplicación
- **VPC**: Red virtual (si es necesario)
- **Security Groups**: Reglas de firewall

## Referencias

- [Terraform AWS Provider Documentation](https://registry.terraform.io/providers/hashicorp/aws/latest/docs)
- [AWS CLI Configuration](https://docs.aws.amazon.com/cli/latest/userguide/cli-configure-quickstart.html)
- [Terraform Best Practices](https://www.terraform.io/docs/cloud/guides/recommended-practices/index.html)

