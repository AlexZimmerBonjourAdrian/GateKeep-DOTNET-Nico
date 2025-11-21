# Guía de Despliegue Automático a AWS

Este documento explica cómo usar el script de despliegue automático `deploy-aws-complete.ps1` para desplegar GateKeep a AWS.

## Requisitos Previos

1. **AWS CLI** instalado y configurado
2. **Docker Desktop** instalado y corriendo
3. **.NET SDK 8.0** instalado (para migraciones)
4. **PowerShell 5.1** o superior

## Configuración Inicial

### 1. Crear archivo `.env`

Copia el archivo de ejemplo y edítalo con tus credenciales:

```powershell
cp env.example .env
```

Edita el archivo `.env` con tus credenciales reales:

```env
# AWS Credentials
AWS_ACCESS_KEY_ID=tu_access_key_real
AWS_SECRET_ACCESS_KEY=tu_secret_key_real
AWS_REGION=sa-east-1

# ECR Repositories
ECR_API_REPOSITORY=126588786097.dkr.ecr.sa-east-1.amazonaws.com/gatekeep-api
ECR_FRONTEND_REPOSITORY=126588786097.dkr.ecr.sa-east-1.amazonaws.com/gatekeep-frontend

# ECS Configuration
ECS_CLUSTER_NAME=gatekeep-cluster
ECS_SERVICE_NAME_API=gatekeep-api-service
ECS_SERVICE_NAME_FRONTEND=gatekeep-frontend-service

# Database (para migraciones)
DB_HOST=tu_rds_endpoint
DB_PORT=5432
DB_NAME=Gatekeep
DB_USER=postgres
DB_PASSWORD=tu_password
```

### 2. Configurar Variables de Entorno para ECS (Opcional)

Edita `ecs-env-vars.json` si necesitas actualizar variables de entorno en ECS. Este archivo preserva las variables existentes y solo actualiza las especificadas.

## Uso del Script

### Despliegue Completo

```powershell
.\deploy-aws-complete.ps1
```

Este comando ejecuta:
1. Validación de prerequisitos
2. Actualización de configuración AWS (preservando existente)
3. Construcción de imágenes Docker (API y Frontend)
4. Subida de imágenes a ECR
5. Ejecución de migraciones de base de datos
6. Actualización de variables de entorno en ECS
7. Deployment en ECS
8. Monitoreo del estado del deployment

### Opciones Disponibles

#### Saltar Migraciones

```powershell
.\deploy-aws-complete.ps1 -SkipMigrations
```

#### Saltar Frontend

```powershell
.\deploy-aws-complete.ps1 -SkipFrontend
```

#### Modo Dry Run (Simulación)

```powershell
.\deploy-aws-complete.ps1 -DryRun
```

Este modo muestra qué se haría sin ejecutar cambios reales.

#### Especificar Archivo .env Personalizado

```powershell
.\deploy-aws-complete.ps1 -EnvFile ".env.production"
```

### Combinar Opciones

```powershell
.\deploy-aws-complete.ps1 -SkipMigrations -SkipFrontend
```

## Flujo del Script

### 1. Validación Inicial
- Verifica que existe el archivo `.env`
- Valida que todas las herramientas estén instaladas
- Carga variables de entorno

### 2. Actualización de Configuración AWS
- Verifica y actualiza Parameter Store (solo si hay cambios)
- Verifica Secrets Manager (no modifica por seguridad)
- **Preserva toda la configuración existente** que no esté en el archivo

### 3. Construcción de Imágenes
- Construye imagen de API desde `src/`
- Construye imagen de Frontend desde `frontend/` (si está habilitado)
- Usa builder tradicional de Docker (evita problemas con buildx)

### 4. Subida a ECR
- Autentica con ECR usando credenciales del `.env`
- Sube imágenes con tag `latest`

### 5. Migraciones de Base de Datos
- Conecta a la base de datos usando credenciales del `.env`
- Ejecuta `dotnet ef database update`
- Verifica que las migraciones se aplicaron correctamente

### 6. Actualización de Variables de Entorno en ECS
- Lee configuración desde `ecs-env-vars.json`
- Obtiene task definition actual
- **Preserva variables existentes** que no están en el archivo
- Crea nueva task definition con variables actualizadas
- Actualiza el servicio ECS

### 7. Deployment
- Fuerza nuevo deployment en ECS
- Monitorea el estado del deployment
- Muestra progreso y estado final

## Función Update-AWSConfig

La función `Update-AWSConfig` es clave para actualizar AWS sin romper configuraciones existentes:

- **Parameter Store**: Solo actualiza parámetros si el valor cambió
- **Secrets Manager**: Solo verifica existencia, no modifica (por seguridad)
- **Preserva**: Toda configuración que no esté explícitamente en el archivo se mantiene intacta

## Archivos de Configuración

### `.env`
Contiene todas las credenciales y configuración necesaria. **NO se sube a git** (está en `.gitignore`).

### `ecs-env-vars.json`
Define las variables de entorno que se actualizarán en ECS. Estructura:

```json
{
  "environment": [
    {
      "name": "ASPNETCORE_ENVIRONMENT",
      "value": "Production"
    }
  ],
  "secrets": [
    {
      "name": "DATABASE__PASSWORD",
      "valueFrom": "{{resolve:secretsmanager:gatekeep/db/password:password::}}"
    }
  ]
}
```

## Solución de Problemas

### Error: "No se encontró el archivo .env"
Crea el archivo `.env` basándote en `env.example`:
```powershell
cp env.example .env
```

### Error: "AWS CLI no está configurado"
Configura AWS CLI:
```powershell
aws configure
```

O usa las credenciales en el archivo `.env`.

### Error: "Docker no está corriendo"
Inicia Docker Desktop y espera a que esté completamente iniciado.

### Error al construir imágenes Docker
El script usa builder tradicional. Si falla, verifica:
- Docker Desktop está corriendo
- Tienes espacio en disco
- El Dockerfile es correcto

### Error en migraciones
Verifica:
- Las credenciales de base de datos en `.env` son correctas
- La base de datos es accesible desde tu red
- .NET SDK está instalado

### Deployment no completa
El script espera hasta 15 minutos por defecto. Puedes ajustar `DEPLOYMENT_TIMEOUT_MINUTES` en `.env`.

Verifica manualmente:
```powershell
aws ecs describe-services --cluster gatekeep-cluster --services gatekeep-api-service --region sa-east-1
```

## Seguridad

- El archivo `.env` contiene credenciales sensibles y **NO debe subirse a git**
- Las credenciales se cargan en memoria y no se muestran en la consola
- Los secrets de AWS Secrets Manager no se modifican automáticamente por seguridad

## Ejemplos de Uso

### Despliegue Rápido (sin migraciones ni frontend)
```powershell
.\deploy-aws-complete.ps1 -SkipMigrations -SkipFrontend
```

### Simular Despliegue
```powershell
.\deploy-aws-complete.ps1 -DryRun
```

### Despliegue Solo API
```powershell
.\deploy-aws-complete.ps1 -SkipFrontend
```

## Verificación Post-Despliegue

1. Verifica el estado del servicio:
```powershell
aws ecs describe-services --cluster gatekeep-cluster --services gatekeep-api-service --region sa-east-1
```

2. Verifica los logs:
```powershell
aws logs tail /ecs/gatekeep --follow --region sa-east-1
```

3. Prueba el endpoint de health:
```powershell
curl https://tu-api-url/health
```

## Notas Importantes

- El script **preserva toda la configuración existente** en AWS que no esté explícitamente en los archivos de configuración
- Las migraciones se ejecutan antes del deployment para evitar problemas
- El frontend es opcional y se puede saltar si no hay cambios
- El modo Dry Run es útil para verificar qué se haría sin ejecutar cambios

## Soporte

Si encuentras problemas:
1. Revisa los logs del script
2. Verifica la configuración en `.env`
3. Usa `-DryRun` para simular sin cambios
4. Consulta la documentación de AWS ECS

