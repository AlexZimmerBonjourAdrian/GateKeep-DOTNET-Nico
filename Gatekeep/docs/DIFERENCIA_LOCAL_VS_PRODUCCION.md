# 🔄 Diferencia entre Local y Producción

**Fecha:** 2025-01-21

---

## 📋 Cómo el Programa Diferencia entre Local y Producción

El programa GateKeep diferencia entre entorno local y producción usando **variables de entorno** y **detección automática**.

---

## 🔍 Variables de Entorno Clave

### 1. `ASPNETCORE_ENVIRONMENT` o `APP_ENVIRONMENT`

Esta es la variable **principal** que determina el entorno:

- **Development** = Modo desarrollo local
- **Production** = Modo producción (AWS)

**Ubicación:**
- **Local:** Archivo `.env` en `Gatekeep/src/.env`
- **Docker:** Variable de entorno en `docker-compose.yml`
- **AWS:** Variable de entorno en ECS Task Definition

### 2. `DOTNET_RUNNING_IN_CONTAINER`

Variable automática que detecta si está ejecutándose en Docker:

- **`true`** = Está en contenedor Docker
- **`null` o `false`** = Está ejecutándose localmente (sin Docker)

---

## 🏠 Modo LOCAL (Development)

### Cómo se Detecta

```csharp
// En Program.cs línea 86
if (!Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER")?.Equals("true", StringComparison.OrdinalIgnoreCase) == true)
{
    // Estamos ejecutando LOCALMENTE
    // Carga archivo .env
}
```

### Características del Modo Local

1. **Carga archivo `.env`** automáticamente
2. **Recrea la base de datos** al iniciar (`EnsureDeleted()` + `EnsureCreated()`)
3. **Seeding automático** de datos de prueba
4. **Swagger habilitado** siempre
5. **Logs más detallados**
6. **AWS SDK opcional** (no requiere credenciales)

### Cómo Levantarlo en Local

#### Opción 1: Con Docker Compose (Recomendado)

```powershell
cd Gatekeep\src
# Asegúrate de que .env tenga:
# APP_ENVIRONMENT=Development
docker-compose up -d
```

#### Opción 2: Directamente con .NET

```powershell
cd Gatekeep\src\GateKeep.Api
# Asegúrate de tener .env en src/
# APP_ENVIRONMENT=Development
dotnet run
```

### Variables Necesarias en `.env` (Local)

```env
# Entorno
APP_ENVIRONMENT=Development
ASPNETCORE_ENVIRONMENT=Development

# Base de Datos
DB_HOST=localhost  # O 'postgres' si usas Docker
DB_PORT=5432
DB_NAME=Gatekeep
DB_USER=postgres
DB_PASSWORD=1234

# JWT
JWT_KEY=clave-secreta-minimo-256-bits

# AWS (opcional en local)
AWS_REGION=sa-east-1
AWS_ACCESS_KEY_ID=  # Puede estar vacío
AWS_SECRET_ACCESS_KEY=  # Puede estar vacío
```

---

## ☁️ Modo PRODUCCIÓN (AWS)

### Cómo se Detecta

```csharp
// En Program.cs línea 120
if (builder.Environment.IsProduction())
{
    // Estamos en PRODUCCIÓN
    // Carga config.Production.json
}
```

### Características del Modo Producción

1. **NO carga archivo `.env`** (usa variables de entorno del contenedor)
2. **Aplica migraciones** de base de datos (no recrea)
3. **NO hace seeding automático** (solo crea admin de respaldo)
4. **Swagger puede estar deshabilitado** (según configuración)
5. **Logs optimizados** para producción
6. **AWS SDK requerido** (lee secrets de AWS Secrets Manager)

### Cómo se Levanta en Producción (AWS)

1. **ECS Task Definition** define las variables de entorno
2. **Secrets Manager** almacena contraseñas y claves
3. **RDS PostgreSQL** es la base de datos
4. **El contenedor** se ejecuta en ECS Fargate

### Variables en ECS Task Definition (Producción)

```json
{
  "environment": [
    {
      "name": "ASPNETCORE_ENVIRONMENT",
      "value": "Production"
    },
    {
      "name": "DATABASE__HOST",
      "value": "gatekeep-db.c7o0qk42qmwh.sa-east-1.rds.amazonaws.com"
    }
  ],
  "secrets": [
    {
      "name": "DATABASE__PASSWORD",
      "valueFrom": "arn:aws:secretsmanager:sa-east-1:126588786097:secret:gatekeep/db/password-14XBlu"
    }
  ]
}
```

---

## 🔄 Flujo de Detección

```
┌─────────────────────────────────────┐
│  Inicio de la Aplicación            │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│ ¿DOTNET_RUNNING_IN_CONTAINER?      │
└──────────────┬──────────────────────┘
               │
        ┌──────┴──────┐
        │            │
       NO           SÍ
        │            │
        ▼            ▼
┌──────────────┐  ┌──────────────────┐
│  LOCAL       │  │  DOCKER/AWS      │
│              │  │                  │
│ 1. Carga .env│  │ 1. Usa ENV vars  │
│ 2. Development│  │ 2. Production   │
└──────────────┘  └──────────────────┘
        │            │
        └──────┬─────┘
               │
               ▼
┌─────────────────────────────────────┐
│ ¿ASPNETCORE_ENVIRONMENT?            │
└──────────────┬──────────────────────┘
               │
        ┌──────┴──────┐
        │            │
  Development   Production
        │            │
        ▼            ▼
┌──────────────┐  ┌──────────────────┐
│ - Recrea BD  │  │ - Migraciones    │
│ - Seeding    │  │ - Sin seeding    │
│ - Swagger    │  │ - AWS requerido  │
└──────────────┘  └──────────────────┘
```

---

## 📊 Comparación Rápida

| Característica | Local (Development) | Producción (AWS) |
|----------------|-------------------|------------------|
| **Variable de Entorno** | `APP_ENVIRONMENT=Development` | `ASPNETCORE_ENVIRONMENT=Production` |
| **Archivo .env** | ✅ Se carga automáticamente | ❌ No se usa |
| **Base de Datos** | Recrea al iniciar | Aplica migraciones |
| **Seeding** | ✅ Automático | ❌ Solo admin de respaldo |
| **Swagger** | ✅ Siempre habilitado | ⚠️ Según configuración |
| **AWS SDK** | ⚠️ Opcional | ✅ Requerido |
| **Logs** | Detallados | Optimizados |
| **Host BD** | `localhost` o `postgres` | RDS endpoint |
| **Contraseña BD** | Desde `.env` | Desde Secrets Manager |

---

## 🛠️ Solución al Problema Actual

El error actual es que **AWS SDK se está configurando siempre**, incluso en desarrollo local sin credenciales válidas.

### Solución: Hacer AWS Opcional en Development

El código debe verificar el entorno antes de configurar AWS:

```csharp
// Solo configurar AWS si NO estamos en Development
if (!builder.Environment.IsDevelopment())
{
    // Configurar AWS SDK
    var awsRegion = Environment.GetEnvironmentVariable("AWS_REGION") ?? "sa-east-1";
    var regionEndpoint = RegionEndpoint.GetBySystemName(awsRegion);
    // ... resto de configuración AWS
}
```

---

## ✅ Checklist para Levantar en Local

- [ ] Archivo `.env` existe en `Gatekeep/src/.env`
- [ ] `APP_ENVIRONMENT=Development` en `.env`
- [ ] `DB_PASSWORD=1234` (o tu contraseña local)
- [ ] `DB_HOST=postgres` (si usas Docker) o `localhost` (si PostgreSQL local)
- [ ] Docker Desktop ejecutándose (si usas Docker Compose)
- [ ] Variables AWS pueden estar vacías (opcional en local)

---

## ✅ Checklist para Producción (AWS)

- [ ] ECS Task Definition tiene `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Secrets Manager tiene `DATABASE__PASSWORD`
- [ ] RDS PostgreSQL está disponible
- [ ] Variables AWS configuradas correctamente
- [ ] Target Groups saludables
- [ ] ALB configurado correctamente

---

**Última actualización:** 2025-01-21

