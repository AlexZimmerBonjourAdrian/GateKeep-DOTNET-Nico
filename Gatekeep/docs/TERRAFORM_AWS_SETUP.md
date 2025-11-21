# Guía: Conectar Terraform con AWS

Esta guía explica cómo configurar Terraform para trabajar con AWS en el proyecto GateKeep.

## Estado Actual

- ✅ **Terraform instalado**: v1.13.5
- ✅ **Configuración creada**: Carpeta `terraform/` con archivos base
- ⚠️ **AWS CLI**: Necesita configuración de credenciales

## Pasos para Conectar Terraform con AWS

### Paso 1: Configurar Credenciales de AWS

Terraform puede usar credenciales de AWS de varias formas. Elige la que prefieras:

#### Opción A: AWS CLI Configure (Recomendado para desarrollo local)

```powershell
aws configure
```

Te pedirá:
- **AWS Access Key ID**: Tu Access Key ID
- **AWS Secret Access Key**: Tu Secret Access Key  
- **Default region**: `sa-east-1`
- **Default output format**: `json`

Esto crea los archivos:
- `C:\Users\[tu-usuario]\.aws\credentials`
- `C:\Users\[tu-usuario]\.aws\config`

Terraform los leerá automáticamente.

#### Opción B: Variables de Entorno (Recomendado para CI/CD)

```powershell
$env:AWS_ACCESS_KEY_ID = "AKIA..."
$env:AWS_SECRET_ACCESS_KEY = "wJalr..."
$env:AWS_REGION = "sa-east-1"
```

**Nota**: Estas variables solo duran en la sesión actual de PowerShell. Para hacerlas permanentes, agrégalas a las Variables de Entorno del Sistema.

#### Opción C: Script de Configuración Automática

Ejecuta el script de ayuda:

```powershell
.\scripts\setup-terraform-aws.ps1
```

Este script te guiará paso a paso.

### Paso 2: Verificar Conexión con AWS

Antes de usar Terraform, verifica que puedes conectarte a AWS:

```powershell
# Verificar identidad
aws sts get-caller-identity --region sa-east-1

# Ver configuración
aws configure list
```

**Salida esperada:**
```json
{
    "UserId": "AIDA...",
    "Account": "123456789012",
    "Arn": "arn:aws:iam::123456789012:user/tu-usuario"
}
```

### Paso 3: Inicializar Terraform

Si aún no lo has hecho, inicializa Terraform:

```powershell
cd terraform
terraform init
```

Esto descargará el provider de AWS (v5.x).

### Paso 4: Validar Configuración

```powershell
terraform validate
```

Debería mostrar: `Success! The configuration is valid.`

### Paso 5: Probar Conexión con Terraform

```powershell
terraform plan
```

Si las credenciales están configuradas correctamente, Terraform se conectará a AWS y mostrará el plan (aunque no haya recursos para crear aún).

## Estructura de Archivos Creados

```
terraform/
├── main.tf          # Configuración del provider AWS
├── variables.tf     # Variables de configuración
├── outputs.tf       # Valores de salida
├── versions.tf      # Versiones requeridas
├── .gitignore       # Archivos a ignorar
└── README.md        # Documentación detallada
```

## Cómo Funciona la Autenticación

Terraform busca credenciales en este orden:

1. **Variables de entorno**: `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`
2. **Archivo de credenciales**: `~/.aws/credentials` (creado con `aws configure`)
3. **Archivo de configuración**: `~/.aws/config` (región)
4. **IAM Roles**: Si ejecutas desde EC2/ECS/Lambda
5. **Variables de Terraform**: `var.aws_access_key_id` (no recomendado)

## Ejemplo de Uso

Una vez configurado, puedes usar Terraform normalmente:

```powershell
cd terraform

# Ver qué se va a crear/modificar
terraform plan

# Aplicar cambios
terraform apply

# Ver outputs
terraform output

# Destruir recursos (cuidado!)
terraform destroy
```

## Crear Recursos AWS

Para agregar recursos (ECR, RDS, etc.), crea nuevos archivos `.tf` en la carpeta `terraform/`:

**Ejemplo: `terraform/ecr.tf`**
```hcl
resource "aws_ecr_repository" "gatekeep_api" {
  name = "gatekeep-api"
  
  image_tag_mutability = "MUTABLE"
  
  image_scanning_configuration {
    scan_on_push = true
  }
  
  tags = {
    Name = "gatekeep-api"
  }
}
```

Luego ejecuta:
```powershell
terraform plan
terraform apply
```

## Troubleshooting

### Error: "Unable to locate credentials"

**Solución**: Configura las credenciales usando una de las opciones del Paso 1.

### Error: "InvalidClientTokenId"

**Solución**: Las credenciales son incorrectas. Verifica tu Access Key ID y Secret Access Key.

### Error: "Access Denied"

**Solución**: El usuario IAM no tiene los permisos necesarios. Consulta `docs/AWS_SETUP.md` para ver qué políticas necesitas.

### Error: "No valid credential sources found"

**Solución**: Verifica que:
1. Las variables de entorno están configuradas, O
2. El archivo `~/.aws/credentials` existe y tiene las credenciales correctas

```powershell
# Verificar archivo de credenciales
Get-Content $env:USERPROFILE\.aws\credentials

# Verificar variables de entorno
$env:AWS_ACCESS_KEY_ID
$env:AWS_SECRET_ACCESS_KEY
```

## Próximos Pasos

1. ✅ **Configurar credenciales** usando `aws configure` o variables de entorno
2. ✅ **Verificar conexión** con `aws sts get-caller-identity`
3. ✅ **Inicializar Terraform** con `terraform init`
4. ✅ **Validar configuración** con `terraform validate`
5. 📝 **Crear recursos** según tus necesidades (ECR, RDS, App Runner, etc.)

## Referencias

- [Terraform AWS Provider](https://registry.terraform.io/providers/hashicorp/aws/latest/docs)
- [AWS CLI Configuration](https://docs.aws.amazon.com/cli/latest/userguide/cli-configure-quickstart.html)
- [Terraform Authentication](https://registry.terraform.io/providers/hashicorp/aws/latest/docs#authentication)
- [Documentación AWS Setup](./AWS_SETUP.md)

