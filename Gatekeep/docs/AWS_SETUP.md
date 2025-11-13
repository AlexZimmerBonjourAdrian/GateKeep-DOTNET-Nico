# Guía de Instalación y Configuración de AWS CLI

Esta guía te ayudará a instalar y configurar AWS CLI para trabajar con GateKeep en AWS.

## Prerequisitos

- Windows 10/11
- Permisos de administrador (para instalación)
- Cuenta AWS activa
- Credenciales IAM (Access Key ID y Secret Access Key)

---

## Paso 1: Instalar AWS CLI

### Método 1: Instalador MSI (Recomendado)

#### Opción A: Descarga y Instalación Manual

1. **Descargar el instalador:**
   ```powershell
   Invoke-WebRequest -Uri "https://awscli.amazonaws.com/AWSCLIV2.msi" -OutFile "$env:TEMP\AWSCLIV2.msi"
   ```

2. **Instalar (requiere permisos de administrador):**
   ```powershell
   # Ejecutar PowerShell como Administrador, luego:
   msiexec.exe /i "$env:TEMP\AWSCLIV2.msi" /quiet /norestart
   ```

   O hacer doble clic en el archivo descargado y seguir el asistente.

#### Opción B: Instalación Directa desde URL

```powershell
# Ejecutar PowerShell como Administrador
msiexec.exe /i https://awscli.amazonaws.com/AWSCLIV2.msi /quiet /norestart
```

#### Opción C: Usando Winget (Windows Package Manager)

```powershell
winget install Amazon.AWSCLI
```

#### Opción D: Usando Chocolatey

```powershell
choco install awscli
```

### Verificar Instalación

Después de instalar, **cierra y vuelve a abrir PowerShell** para refrescar el PATH, luego verifica:

```powershell
aws --version
```

**Salida esperada:**
```
aws-cli/2.31.34 Python/3.13.9 Windows/10 exe/AMD64
```

Si no funciona, refresca el PATH manualmente:

```powershell
$env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")
aws --version
```

---

## Paso 2: Crear IAM User en AWS

Antes de configurar AWS CLI, necesitas crear un usuario IAM con las credenciales necesarias.

### 2.1 Acceder a AWS Console

1. Ir a [AWS Console](https://console.aws.amazon.com/)
2. Iniciar sesión con tu cuenta AWS

### 2.2 Crear IAM User

1. Ir a **IAM** → **Users** → **Create user**
2. **User name**: `gatekeep-deployment` (o el nombre que prefieras)
3. Seleccionar **Provide user access to the AWS Management Console** (opcional, para acceso web)
4. O seleccionar **Access key - Programmatic access** (requerido para AWS CLI)
5. Click en **Next**

### 2.3 Asignar Permisos

Seleccionar **Attach policies directly** y agregar las siguientes políticas:

#### Políticas Requeridas:

- **AmazonEC2ContainerRegistryFullAccess**
  - Permite push/pull de imágenes Docker a ECR
  
- **AmazonAppRunnerFullAccess**
  - Permite crear y gestionar servicios de App Runner
  
- **AmazonRDSFullAccess**
  - Permite crear y gestionar instancias de RDS
  
- **SecretsManagerReadWrite**
  - Permite leer y escribir secrets en Secrets Manager
  
- **AmazonSSMFullAccess**
  - Permite gestionar parámetros en Parameter Store
  
- **CloudWatchLogsFullAccess**
  - Permite ver logs de App Runner y otros servicios

**Alternativa:** Crear una política personalizada con permisos mínimos (más seguro):

```json
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Action": [
                "ecr:*",
                "apprunner:*",
                "rds:*",
                "secretsmanager:*",
                "ssm:*",
                "logs:*",
                "iam:PassRole"
            ],
            "Resource": "*"
        }
    ]
}
```

### 2.4 Crear y Guardar Credenciales

1. Click en **Create user**
2. **IMPORTANTE:** Guardar las credenciales:
   - **Access Key ID**: `AKIA...` (ejemplo)
   - **Secret Access Key**: `wJalr...` (ejemplo)
   
   ⚠️ **La Secret Access Key solo se muestra una vez**. Guárdala de forma segura.

3. Click en **Download .csv** para guardar las credenciales

---

## Paso 3: Configurar AWS CLI

### 3.1 Configuración Básica

Ejecutar el comando de configuración:

```powershell
aws configure
```

Te pedirá la siguiente información:

```
AWS Access Key ID [None]: AKIAIOSFODNN7EXAMPLE
AWS Secret Access Key [None]: wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY
Default region name [None]: us-east-1
Default output format [None]: json
```

**Valores recomendados:**
- **Access Key ID**: Tu Access Key ID del IAM User
- **Secret Access Key**: Tu Secret Access Key del IAM User
- **Default region**: `us-east-1` (o la región que prefieras)
- **Default output format**: `json` (más legible)

### 3.2 Configuración por Comandos Individuales

Si prefieres configurar cada valor por separado:

```powershell
# Configurar Access Key
aws configure set aws_access_key_id AKIAIOSFODNN7EXAMPLE

# Configurar Secret Key
aws configure set aws_secret_access_key wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY

# Configurar región
aws configure set region us-east-1

# Configurar formato de salida
aws configure set output json
```

### 3.3 Verificar Configuración

```powershell
aws configure list
```

**Salida esperada:**
```
      Name                    Value             Type    Location
      ----                    -----             ----    --------
   profile                <not set>             None    None
   access_key ****************Key]  config-file    ~/.aws/credentials
   secret_key ****************Key]  config-file    ~/.aws/credentials
   region                us-east-1      config-file    ~/.aws/config
```

---

## Paso 4: Verificar Conexión con AWS

### 4.1 Verificar Identidad

```powershell
aws sts get-caller-identity
```

**Salida esperada:**
```json
{
    "UserId": "AIDAEXAMPLE",
    "Account": "123456789012",
    "Arn": "arn:aws:iam::123456789012:user/gatekeep-deployment"
}
```

Si ves un error, verifica:
- Las credenciales están correctas
- El IAM User tiene permisos
- La región está configurada

### 4.2 Probar Acceso a Servicios

```powershell
# Listar regiones disponibles
aws ec2 describe-regions --output table

# Ver información del usuario
aws iam get-user

# Listar buckets S3 (si tienes)
aws s3 ls
```

---

## Paso 5: Ubicación de Archivos de Configuración

AWS CLI guarda la configuración en:

### Windows

- **Credenciales**: `C:\Users\[tu-usuario]\.aws\credentials`
- **Configuración**: `C:\Users\[tu-usuario]\.aws\config`

### Ver Contenido de Archivos

```powershell
# Ver credenciales (cuidado, contiene secrets)
Get-Content $env:USERPROFILE\.aws\credentials

# Ver configuración
Get-Content $env:USERPROFILE\.aws\config
```

### Estructura de Archivos

**credentials:**
```ini
[default]
aws_access_key_id = AKIAIOSFODNN7EXAMPLE
aws_secret_access_key = wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY
```

**config:**
```ini
[default]
region = us-east-1
output = json
```

---

## Paso 6: Comandos Útiles de AWS CLI

### ECR (Elastic Container Registry)

```powershell
# Login a ECR
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin [ECR_URI]

# Listar repositorios
aws ecr describe-repositories --region us-east-1

# Crear repositorio (desde CLI)
aws ecr create-repository --repository-name gatekeep-api --region us-east-1
```

### RDS

```powershell
# Listar instancias RDS
aws rds describe-db-instances --region us-east-1

# Ver detalles de una instancia
aws rds describe-db-instances --db-instance-identifier gatekeep-db --region us-east-1
```

### App Runner

```powershell
# Listar servicios App Runner
aws apprunner list-services --region us-east-1

# Ver detalles de un servicio
aws apprunner describe-service --service-arn [ARN] --region us-east-1
```

### Secrets Manager

```powershell
# Listar secrets
aws secretsmanager list-secrets --region us-east-1

# Obtener valor de un secret
aws secretsmanager get-secret-value --secret-id gatekeep/db/password --region us-east-1
```

### Parameter Store

```powershell
# Listar parámetros
aws ssm describe-parameters --region us-east-1

# Obtener parámetro
aws ssm get-parameter --name /gatekeep/db/host --region us-east-1
```

---

## Paso 7: Configurar Múltiples Perfiles (Opcional)

Si trabajas con múltiples cuentas AWS, puedes usar perfiles:

```powershell
# Configurar perfil personalizado
aws configure --profile gatekeep-prod

# Usar perfil específico
aws s3 ls --profile gatekeep-prod

# Ver perfiles configurados
aws configure list-profiles
```

---

## Troubleshooting

### Error: "aws: command not found"

**Solución:**
1. Verificar que AWS CLI está instalado:
   ```powershell
   Test-Path "C:\Program Files\Amazon\AWSCLIV2\aws.exe"
   ```

2. Refrescar PATH:
   ```powershell
   $env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")
   ```

3. Reiniciar PowerShell

### Error: "Unable to locate credentials"

**Solución:**
1. Verificar que las credenciales están configuradas:
   ```powershell
   aws configure list
   ```

2. Reconfigurar:
   ```powershell
   aws configure
   ```

### Error: "Access Denied" o "UnauthorizedOperation"

**Solución:**
1. Verificar que el IAM User tiene las políticas necesarias
2. Verificar que las credenciales son correctas
3. Verificar permisos del IAM User en AWS Console

### Error: "InvalidClientTokenId"

**Solución:**
- Las credenciales son incorrectas o han sido revocadas
- Verificar Access Key ID y Secret Access Key
- Crear nuevas credenciales si es necesario

### Error al hacer login a ECR

**Solución:**
1. Verificar que tienes permisos en ECR:
   ```powershell
   aws ecr describe-repositories --region us-east-1
   ```

2. Verificar que la región es correcta
3. Verificar formato de ECR URI

---

## Comandos de Verificación Rápida

Crea un script de verificación:

```powershell
# Verificar instalación
Write-Host "=== Verificación AWS CLI ===" -ForegroundColor Cyan
aws --version

# Verificar configuración
Write-Host "`n=== Configuración ===" -ForegroundColor Cyan
aws configure list

# Verificar conexión
Write-Host "`n=== Conexión AWS ===" -ForegroundColor Cyan
aws sts get-caller-identity

Write-Host "`n=== Verificación Completada ===" -ForegroundColor Green
```

---

## Próximos Pasos

Una vez que AWS CLI esté instalado y configurado:

1. ✅ **Crear recursos AWS** (desde consola o CLI)
   - ECR Repositories
   - RDS PostgreSQL
   - Secrets Manager
   - Parameter Store
   - App Runner Services

2. ✅ **Configurar GitHub Secrets**
   - `AWS_ACCESS_KEY_ID`
   - `AWS_SECRET_ACCESS_KEY`

3. ✅ **Probar push a ECR**
   ```powershell
   aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin [ECR_URI]
   ```

4. ✅ **Seguir la guía de despliegue**
   - Consultar `docs/DEPLOYMENT.md`

---

## Referencias

- [Documentación oficial AWS CLI](https://docs.aws.amazon.com/cli/latest/userguide/)
- [Instalación AWS CLI Windows](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html)
- [Configuración AWS CLI](https://docs.aws.amazon.com/cli/latest/userguide/cli-configure-quickstart.html)
- [IAM Best Practices](https://docs.aws.amazon.com/IAM/latest/UserGuide/best-practices.html)

---

**Última actualización:** 11 de noviembre de 2025

