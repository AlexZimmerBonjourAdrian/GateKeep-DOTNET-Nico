# Guía de Uso de AWS en GateKeep

Esta guía explica cómo usar las credenciales de AWS configuradas en GateKeep.

## Estado Actual

- **Credenciales configuradas**: ✅
- **Región**: `sa-east-1` (São Paulo)
- **AWS CLI**: Configurado y funcionando

## 1. Uso desde AWS CLI (Terminal)

### Verificar Conexión

```powershell
# Verificar identidad
aws sts get-caller-identity --region sa-east-1

# Ver configuración actual
aws configure list
```

### Comandos Útiles

#### ECR (Elastic Container Registry)

```powershell
# Listar repositorios
aws ecr describe-repositories --region sa-east-1

# Login a ECR (para hacer push de imágenes Docker)
aws ecr get-login-password --region sa-east-1 | docker login --username AWS --password-stdin [TU_ACCOUNT_ID].dkr.ecr.sa-east-1.amazonaws.com

# Crear repositorio
aws ecr create-repository --repository-name gatekeep-api --region sa-east-1
```

#### Secrets Manager

```powershell
# Listar secrets
aws secretsmanager list-secrets --region sa-east-1

# Obtener valor de un secret
aws secretsmanager get-secret-value --secret-id gatekeep/db/password --region sa-east-1

# Crear un nuevo secret
aws secretsmanager create-secret --name gatekeep/jwt/key --secret-string "tu-clave-secreta" --region sa-east-1
```

#### Parameter Store (Systems Manager)

```powershell
# Listar parámetros
aws ssm describe-parameters --region sa-east-1

# Obtener parámetro
aws ssm get-parameter --name /gatekeep/db/host --region sa-east-1

# Crear parámetro
aws ssm put-parameter --name /gatekeep/db/host --value "tu-host" --type String --region sa-east-1
```

#### RDS

```powershell
# Listar instancias RDS
aws rds describe-db-instances --region sa-east-1

# Ver detalles de una instancia
aws rds describe-db-instances --db-instance-identifier gatekeep-db --region sa-east-1
```

#### App Runner

```powershell
# Listar servicios App Runner
aws apprunner list-services --region sa-east-1

# Ver detalles de un servicio
aws apprunner describe-service --service-arn [ARN] --region sa-east-1
```

## 2. Uso desde Aplicación .NET

### Paso 1: Instalar Paquetes AWS SDK

Agregar los paquetes NuGet necesarios al proyecto `GateKeep.Api.csproj`:

```xml
<!-- AWS SDK Core -->
<PackageReference Include="AWSSDK.Core" Version="3.7.400.42" />

<!-- Servicios específicos que necesites -->
<PackageReference Include="AWSSDK.SecretsManager" Version="3.7.400.42" />
<PackageReference Include="AWSSDK.SimpleSystemsManagement" Version="3.7.400.42" />
<PackageReference Include="AWSSDK.S3" Version="3.7.400.42" />
<PackageReference Include="AWSSDK.ECR" Version="3.7.400.42" />
```

### Paso 2: Configurar Servicios AWS en Program.cs

```csharp
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Amazon.S3;
using Amazon.ECR;

// En el método de configuración de servicios (builder.Services)
var awsRegion = Environment.GetEnvironmentVariable("AWS_REGION") ?? "sa-east-1";
var regionEndpoint = RegionEndpoint.GetBySystemName(awsRegion);

// Configurar AWS SDK para usar credenciales de variables de entorno
// Las credenciales se leen automáticamente de:
// - AWS_ACCESS_KEY_ID
// - AWS_SECRET_ACCESS_KEY
// - AWS_REGION

// Secrets Manager
builder.Services.AddSingleton<IAmazonSecretsManager>(sp =>
{
    var config = new AmazonSecretsManagerConfig
    {
        RegionEndpoint = regionEndpoint
    };
    return new AmazonSecretsManagerClient(config);
});

// Parameter Store (Systems Manager)
builder.Services.AddSingleton<IAmazonSimpleSystemsManagement>(sp =>
{
    var config = new AmazonSimpleSystemsManagementConfig
    {
        RegionEndpoint = regionEndpoint
    };
    return new AmazonSimpleSystemsManagementClient(config);
});

// S3 (opcional, si necesitas almacenar archivos)
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var config = new AmazonS3Config
    {
        RegionEndpoint = regionEndpoint
    };
    return new AmazonS3Client(config);
});
```

### Paso 3: Crear Servicio para AWS

Ejemplo: `Infrastructure/AWS/AwsSecretsService.cs`

```csharp
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Logging;

namespace GateKeep.Api.Infrastructure.AWS;

public interface IAwsSecretsService
{
    Task<string> GetSecretAsync(string secretName);
}

public class AwsSecretsService : IAwsSecretsService
{
    private readonly IAmazonSecretsManager _secretsManager;
    private readonly ILogger<AwsSecretsService> _logger;

    public AwsSecretsService(
        IAmazonSecretsManager secretsManager,
        ILogger<AwsSecretsService> logger)
    {
        _secretsManager = secretsManager;
        _logger = logger;
    }

    public async Task<string> GetSecretAsync(string secretName)
    {
        try
        {
            var request = new GetSecretValueRequest
            {
                SecretId = secretName
            };

            var response = await _secretsManager.GetSecretValueAsync(request);
            return response.SecretString;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo secret {SecretName}", secretName);
            throw;
        }
    }
}
```

Ejemplo: `Infrastructure/AWS/AwsParameterService.cs`

```csharp
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Logging;

namespace GateKeep.Api.Infrastructure.AWS;

public interface IAwsParameterService
{
    Task<string> GetParameterAsync(string parameterName);
    Task PutParameterAsync(string parameterName, string value);
}

public class AwsParameterService : IAwsParameterService
{
    private readonly IAmazonSimpleSystemsManagement _ssm;
    private readonly ILogger<AwsParameterService> _logger;

    public AwsParameterService(
        IAmazonSimpleSystemsManagement ssm,
        ILogger<AwsParameterService> logger)
    {
        _ssm = ssm;
        _logger = logger;
    }

    public async Task<string> GetParameterAsync(string parameterName)
    {
        try
        {
            var request = new GetParameterRequest
            {
                Name = parameterName,
                WithDecryption = false
            };

            var response = await _ssm.GetParameterAsync(request);
            return response.Parameter.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo parámetro {ParameterName}", parameterName);
            throw;
        }
    }

    public async Task PutParameterAsync(string parameterName, string value)
    {
        try
        {
            var request = new PutParameterRequest
            {
                Name = parameterName,
                Value = value,
                Type = ParameterType.String,
                Overwrite = true
            };

            await _ssm.PutParameterAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error guardando parámetro {ParameterName}", parameterName);
            throw;
        }
    }
}
```

### Paso 4: Registrar Servicios en Program.cs

```csharp
// Registrar servicios AWS
builder.Services.AddScoped<IAwsSecretsService, AwsSecretsService>();
builder.Services.AddScoped<IAwsParameterService, AwsParameterService>();
```

### Paso 5: Usar en tu Código

Ejemplo de uso en un endpoint o servicio:

```csharp
using GateKeep.Api.Infrastructure.AWS;

public class MiServicio
{
    private readonly IAwsSecretsService _secretsService;
    private readonly IAwsParameterService _parameterService;

    public MiServicio(
        IAwsSecretsService secretsService,
        IAwsParameterService parameterService)
    {
        _secretsService = secretsService;
        _parameterService = parameterService;
    }

    public async Task<string> ObtenerPasswordDeBaseDeDatos()
    {
        // Obtener secret de AWS Secrets Manager
        var password = await _secretsService.GetSecretAsync("gatekeep/db/password");
        return password;
    }

    public async Task<string> ObtenerHostDeBaseDeDatos()
    {
        // Obtener parámetro de AWS Parameter Store
        var host = await _parameterService.GetParameterAsync("/gatekeep/db/host");
        return host;
    }
}
```

## 3. Configuración de Permisos IAM

Para que las credenciales funcionen, el usuario IAM necesita tener las siguientes políticas:

### Políticas Mínimas Recomendadas

1. **Secrets Manager**:
   - `SecretsManagerReadWrite` o permisos personalizados:
     - `secretsmanager:GetSecretValue`
     - `secretsmanager:CreateSecret`
     - `secretsmanager:UpdateSecret`

2. **Parameter Store**:
   - `AmazonSSMFullAccess` o permisos personalizados:
     - `ssm:GetParameter`
     - `ssm:PutParameter`
     - `ssm:DescribeParameters`

3. **ECR** (si vas a usar contenedores):
   - `AmazonEC2ContainerRegistryFullAccess` o permisos personalizados:
     - `ecr:GetAuthorizationToken`
     - `ecr:BatchCheckLayerAvailability`
     - `ecr:GetDownloadUrlForLayer`
     - `ecr:BatchGetImage`
     - `ecr:PutImage`
     - `ecr:InitiateLayerUpload`
     - `ecr:UploadLayerPart`
     - `ecr:CompleteLayerUpload`

4. **S3** (si vas a almacenar archivos):
   - `AmazonS3FullAccess` o permisos personalizados según tus buckets

### Agregar Políticas al Usuario IAM

1. Ir a [AWS Console](https://console.aws.amazon.com/iam/)
2. Ir a **Users** → Seleccionar tu usuario (`AlexZimmer2`)
3. Click en **Add permissions** → **Attach policies directly**
4. Seleccionar las políticas necesarias
5. Click en **Add permissions**

## 4. Ejemplos de Uso Común

### Obtener Configuración de Base de Datos desde AWS

```csharp
public async Task<string> ObtenerConnectionString()
{
    var host = await _parameterService.GetParameterAsync("/gatekeep/db/host");
    var port = await _parameterService.GetParameterAsync("/gatekeep/db/port");
    var name = await _parameterService.GetParameterAsync("/gatekeep/db/name");
    var user = await _parameterService.GetParameterAsync("/gatekeep/db/username");
    var password = await _secretsService.GetSecretAsync("gatekeep/db/password");

    return $"Host={host};Port={port};Database={name};Username={user};Password={password}";
}
```

### Subir Imagen Docker a ECR

```powershell
# 1. Login a ECR
$accountId = "548481212172"
aws ecr get-login-password --region sa-east-1 | docker login --username AWS --password-stdin $accountId.dkr.ecr.sa-east-1.amazonaws.com

# 2. Crear repositorio (si no existe)
aws ecr create-repository --repository-name gatekeep-api --region sa-east-1

# 3. Tag de la imagen
docker tag gatekeep-api:latest $accountId.dkr.ecr.sa-east-1.amazonaws.com/gatekeep-api:latest

# 4. Push de la imagen
docker push $accountId.dkr.ecr.sa-east-1.amazonaws.com/gatekeep-api:latest
```

## 5. Troubleshooting

### Error: "Unable to locate credentials"

**Solución**: Verificar que las variables de entorno están configuradas:
```powershell
docker exec gatekeep-api printenv AWS_ACCESS_KEY_ID
docker exec gatekeep-api printenv AWS_SECRET_ACCESS_KEY
docker exec gatekeep-api printenv AWS_REGION
```

### Error: "Access Denied"

**Solución**: El usuario IAM no tiene los permisos necesarios. Agregar las políticas IAM correspondientes.

### Error: "InvalidClientTokenId"

**Solución**: Las credenciales son incorrectas. Verificar Access Key ID y Secret Access Key.

## 6. Próximos Pasos

1. **Agregar permisos IAM** al usuario para los servicios que necesites
2. **Instalar paquetes AWS SDK** en el proyecto .NET
3. **Crear servicios** para interactuar con AWS
4. **Probar** con comandos de AWS CLI primero
5. **Integrar** en la aplicación .NET

## Referencias

- [AWS SDK for .NET Documentation](https://docs.aws.amazon.com/sdk-for-net/)
- [AWS CLI Documentation](https://docs.aws.amazon.com/cli/latest/userguide/)
- [AWS IAM Best Practices](https://docs.aws.amazon.com/IAM/latest/UserGuide/best-practices.html)

