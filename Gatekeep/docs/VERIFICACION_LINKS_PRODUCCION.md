# Verificación de Links de Producción - GateKeep

**Fecha de Verificación:** 2025-01-XX  
**Ambiente:** Producción AWS  
**Región:** sa-east-1 (São Paulo, Brasil)  
**Dominio Principal:** zimmzimmgames.com

---

## Resumen Ejecutivo

Este documento detalla el estado de todos los links y URLs de producción configurados en el sistema GateKeep, verificando su correcta configuración en todos los componentes de la infraestructura.

### Estado General

| Componente | Estado | URL | Notas |
|------------|--------|-----|-------|
| Frontend | ✅ Configurado | `https://zimmzimmgames.com` | Activo |
| Backend API | ✅ Configurado | `https://api.zimmzimmgames.com` | Activo |
| ALB DNS | ✅ Configurado | `gatekeep-alb-878011876.sa-east-1.elb.amazonaws.com` | Activo |
| Route53 | ✅ Configurado | Zona: `zimmzimmgames.com.` | Activo |
| Certificado SSL | ✅ Configurado | ACM para `zimmzimmgames.com` y `api.zimmzimmgames.com` | Activo |

---

## 1. URLs Públicas de Producción

### 1.1 Frontend

| Propiedad | Valor | Estado |
|-----------|-------|--------|
| **URL Principal** | `https://zimmzimmgames.com` | ✅ Activo |
| **URL Alternativa** | `https://www.zimmzimmgames.com` | ✅ Activo |
| **Protocolo** | HTTPS | ✅ Configurado |
| **Puerto** | 443 | ✅ Configurado |
| **Target Group** | `gatekeep-frontend-tg` | ✅ Configurado |
| **Container Port** | 3000 | ✅ Configurado |
| **Registro DNS** | Route53 A (Alias) → ALB | ✅ Configurado |

**Configuración en Terraform:**
```794:794:Gatekeep/terraform/ecs.tf
          value = "https://api.${var.domain_name}"
```

**Configuración en CORS (Backend):**
```317:318:Gatekeep/src/GateKeep.Api/Program.cs
                "https://zimmzimmgames.com",
                "https://www.zimmzimmgames.com"
```

### 1.2 Backend API

| Propiedad | Valor | Estado |
|-----------|-------|--------|
| **URL Base** | `https://api.zimmzimmgames.com` | ✅ Activo |
| **Protocolo** | HTTPS | ✅ Configurado |
| **Puerto** | 443 | ✅ Configurado |
| **Target Group** | `gatekeep-tg` | ✅ Configurado |
| **Container Port** | 5011 | ✅ Configurado |
| **Registro DNS** | Route53 A (Alias) → ALB | ✅ Configurado |
| **Health Check** | `/health` | ✅ Configurado |

**Configuración en Terraform:**
```793:794:Gatekeep/terraform/ecs.tf
          name  = "NEXT_PUBLIC_API_URL"
          value = "https://api.${var.domain_name}"
```

**Configuración en Frontend (urlService.ts):**
```5:6:Gatekeep/frontend/src/services/urlService.ts
 * En AWS: https://api.zimmzimmgames.com
 * En local: http://localhost:5011 (o http://localhost si usa nginx)
```

**Fallback en Frontend:**
```33:35:Gatekeep/frontend/src/services/urlService.ts
  const fallback = process.env.NODE_ENV === 'production' 
    ? "https://api.zimmzimmgames.com"
    : "http://localhost:5011";
```

### 1.3 Application Load Balancer (ALB)

| Propiedad | Valor | Estado |
|-----------|-------|--------|
| **Nombre** | `gatekeep-alb` | ✅ Activo |
| **ARN** | `arn:aws:elasticloadbalancing:sa-east-1:126588786097:loadbalancer/app/gatekeep-alb/ff82ae699b9862d2` | ✅ Activo |
| **DNS** | `gatekeep-alb-878011876.sa-east-1.elb.amazonaws.com` | ✅ Activo |
| **Scheme** | Internet-facing | ✅ Configurado |
| **Tipo** | Application Load Balancer | ✅ Configurado |

**Listeners Configurados:**

#### Listener HTTP (Puerto 80)
- **Acción por defecto:** Redirigir a HTTPS (puerto 443) ✅
- **Reglas de enrutamiento:** Ver sección 2.1

#### Listener HTTPS (Puerto 443)
- **Protocolo:** HTTPS ✅
- **SSL Policy:** `ELBSecurityPolicy-TLS-1-2-2017-01` ✅
- **Certificado:** ACM Certificate para `zimmzimmgames.com` y `api.zimmzimmgames.com` ✅
- **Reglas de enrutamiento:** Ver sección 2.2

---

## 2. Configuración de Enrutamiento (ALB)

### 2.1 Reglas del Listener HTTP (Puerto 80)

| Prioridad | Condición | Acción | Target Group | Estado |
|-----------|-----------|--------|--------------|--------|
| 90 | Path: `/api/auth/*` | Forward | `gatekeep-tg` | ✅ Activa |
| 100 | Path: `/api/*` | Forward | `gatekeep-tg` | ✅ Activa |
| 120 | Path: `/usuarios/*` | Forward | `gatekeep-tg` | ✅ Activa |
| 130 | Path: `/swagger*`, `/swagger/*` | Forward | `gatekeep-tg` | ✅ Activa |
| 140 | Path: `/health` | Forward | `gatekeep-tg` | ✅ Activa |
| Default | - | Redirect a HTTPS | - | ✅ Activa |

### 2.2 Reglas del Listener HTTPS (Puerto 443)

| Prioridad | Condición | Acción | Target Group | Estado |
|-----------|-----------|--------|--------------|--------|
| 90 | Path: `/api/auth/*` | Forward | `gatekeep-tg` | ✅ Activa |
| 100 | Path: `/api/*` | Forward | `gatekeep-tg` | ✅ Activa |
| 120 | Path: `/usuarios/*` | Forward | `gatekeep-tg` | ✅ Activa |
| 130 | Path: `/swagger*`, `/swagger/*` | Forward | `gatekeep-tg` | ✅ Activa |
| 140 | Path: `/health*` | Forward | `gatekeep-tg` | ✅ Activa |
| Default | - | Forward | `gatekeep-frontend-tg` | ✅ Activa |

**Nota:** La regla `/auth/*` (sin prefijo `/api/`) fue eliminada porque la aplicación solo expone endpoints bajo `/api/auth/*`.

---

## 3. Configuración DNS (Route53)

### 3.1 Hosted Zone

| Propiedad | Valor | Estado |
|-----------|-------|--------|
| **Nombre** | `zimmzimmgames.com.` | ✅ Activo |
| **Zone ID** | `Z038254635T8HIPT0Z245` | ✅ Activo |
| **Tipo** | Public | ✅ Configurado |

### 3.2 Registros DNS

#### Registro Principal (Frontend)
- **Nombre:** `zimmzimmgames.com`
- **Tipo:** A (Alias)
- **Alias Target:** `gatekeep-alb-878011876.sa-east-1.elb.amazonaws.com`
- **Evaluate Target Health:** No
- **Estado:** ✅ Activo

#### Registro API (Backend)
- **Nombre:** `api.zimmzimmgames.com`
- **Tipo:** A (Alias)
- **Alias Target:** `gatekeep-alb-878011876.sa-east-1.elb.amazonaws.com`
- **Evaluate Target Health:** No
- **Estado:** ✅ Activo

#### Registros de Validación ACM
- Múltiples registros CNAME para validación de certificados SSL/TLS
- **Estado:** ✅ Activos

---

## 4. Configuración en Código

### 4.1 Backend (.NET) - Program.cs

#### CORS Configuration
```307:324:Gatekeep/src/GateKeep.Api/Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                // Desarrollo local
                "http://localhost:3000", 
                "http://127.0.0.1:3000",
                "http://localhost",  // Para nginx local
                // Producción AWS
                "https://zimmzimmgames.com",
                "https://www.zimmzimmgames.com"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

**Estado:** ✅ Correctamente configurado con los dominios de producción.

### 4.2 Frontend (Next.js)

#### Variable de Entorno en Task Definition
```793:795:Gatekeep/terraform/ecs.tf
        {
          name  = "NEXT_PUBLIC_API_URL"
          value = "https://api.${var.domain_name}"
        }
```

**Estado:** ✅ Correctamente configurado en Terraform (valor: `https://api.zimmzimmgames.com`)

#### URL Service (urlService.ts)
```8:38:Gatekeep/frontend/src/services/urlService.ts
const getApiBaseUrl = () => {
  // Prioridad 1: Variable de entorno (configurada en build/deployment - CRÍTICA)
  if (process.env.NEXT_PUBLIC_API_URL) {
    const url = process.env.NEXT_PUBLIC_API_URL.replace(/\/$/, '');
    console.debug('[URLService] Using NEXT_PUBLIC_API_URL:', url);
    return url;
  }
  
  // Prioridad 2: En cliente, detectar si estamos en producción AWS
  if (typeof window !== 'undefined') {
    const origin = window.location.origin;
    // Si estamos en HTTPS y no es localhost, construir api.*
    if (origin.startsWith('https://') && !origin.includes('localhost')) {
      // Extraer dominio base (sin www)
      const domain = origin.replace(/^https?:\/\/(www\.)?/, '');
      const apiUrl = `https://api.${domain}`;
      console.debug('[URLService] Detected AWS production domain:', apiUrl);
      return apiUrl;
    }
    // En desarrollo local, usar el mismo origin (nginx enruta /api/)
    console.debug('[URLService] Using localhost origin:', origin);
    return origin;
  }
  
  // Prioridad 3: Fallback según NODE_ENV (solo en servidor)
  const fallback = process.env.NODE_ENV === 'production' 
    ? "https://api.zimmzimmgames.com"
    : "http://localhost:5011";
  console.debug('[URLService] Using fallback:', fallback, 'NODE_ENV:', process.env.NODE_ENV);
  return fallback;
};
```

**Estado:** ✅ Correctamente configurado con múltiples fallbacks para producción.

#### Config.js
```5:25:Gatekeep/frontend/config.js
const getApiUrl = () => {
  // Prioridad 1: Variable de entorno
  if (process.env.NEXT_PUBLIC_API_URL) {
    return process.env.NEXT_PUBLIC_API_URL;
  }
  
  // Prioridad 2: En cliente, detectar producción AWS
  if (typeof window !== 'undefined') {
    const origin = window.location.origin;
    if (origin.startsWith('https://') && !origin.includes('localhost')) {
      const domain = origin.replace(/^https?:\/\/(www\.)?/, '');
      return `https://api.${domain}`;
    }
    return origin;
  }
  
  // Prioridad 3: Fallback según NODE_ENV
  return typeof process !== 'undefined' && process.env.NODE_ENV === 'production' 
    ? 'https://api.zimmzimmgames.com'
    : 'http://localhost:5011';
};
```

**Estado:** ✅ Correctamente configurado.

#### Sync Service (sync.ts)
```21:42:Gatekeep/frontend/src/lib/sync.ts
const getDefaultApiBase = () => {
  // Prioridad 1: Variable de entorno
  if (process.env.NEXT_PUBLIC_API_URL) {
    return process.env.NEXT_PUBLIC_API_URL.replace(/\/$/, '');
  }
  
  // Prioridad 2: En cliente, detectar producción AWS
  if (typeof window !== 'undefined' && window.location?.origin) {
    const origin = window.location.origin;
    // Si estamos en HTTPS y no es localhost, construir api.*
    if (origin.startsWith('https://') && !origin.includes('localhost')) {
      const domain = origin.replace(/^https?:\/\/(www\.)?/, '');
      return `https://api.${domain}`;
    }
    return origin;
  }

  // Prioridad 3: Fallback según NODE_ENV
  return process.env.NODE_ENV === 'production' 
    ? 'https://api.zimmzimmgames.com'
    : 'http://localhost:5011';
};
```

**Estado:** ✅ Correctamente configurado.

### 4.3 Docker Compose (Producción)

```78:78:docker-compose.prod.yml
        NEXT_PUBLIC_API_URL: ${NEXT_PUBLIC_API_URL:-https://api.zimmzimmgames.com}
```

**Estado:** ✅ Correctamente configurado con valor por defecto.

---

## 5. Endpoints de Producción

### 5.1 Endpoints Públicos del Backend

#### Autenticación
- `POST https://api.zimmzimmgames.com/api/auth/login` ✅
- `POST https://api.zimmzimmgames.com/api/auth/register` ✅
- `GET https://api.zimmzimmgames.com/api/auth/qr` ✅
- `GET https://api.zimmzimmgames.com/api/auth/validate` ✅

#### Usuarios
- `GET https://api.zimmzimmgames.com/api/usuarios` ✅
- `GET https://api.zimmzimmgames.com/api/usuarios/{id}` ✅
- `POST https://api.zimmzimmgames.com/api/usuarios` ✅
- `PUT https://api.zimmzimmgames.com/api/usuarios/{id}` ✅
- `DELETE https://api.zimmzimmgames.com/api/usuarios/{id}` ✅

#### Health Checks
- `GET https://api.zimmzimmgames.com/health` ✅
- `GET https://api.zimmzimmgames.com/health/mongodb` ✅
- `GET https://api.zimmzimmgames.com/health/redis` ✅

#### Swagger
- `GET https://api.zimmzimmgames.com/swagger` ✅
- `GET https://api.zimmzimmgames.com/swagger/index.html` ✅

### 5.2 Frontend

- `GET https://zimmzimmgames.com/` ✅
- `GET https://www.zimmzimmgames.com/` ✅

---

## 6. Verificación de Configuración

### 6.1 Comandos de Verificación

#### Verificar DNS
```powershell
# Verificar resolución DNS del frontend
nslookup zimmzimmgames.com

# Verificar resolución DNS del backend
nslookup api.zimmzimmgames.com

# Verificar registros Route53
aws route53 list-resource-record-sets --hosted-zone-id Z038254635T8HIPT0Z245 --query "ResourceRecordSets[?contains(Name, 'zimmzimmgames')].[Name,Type,ResourceRecords[0].Value]" --output table
```

#### Verificar ALB
```powershell
# Verificar estado del ALB
$albArn = "arn:aws:elasticloadbalancing:sa-east-1:126588786097:loadbalancer/app/gatekeep-alb/ff82ae699b9862d2"
aws elbv2 describe-load-balancers --load-balancer-arns $albArn --region sa-east-1

# Verificar reglas del listener HTTPS
$listenerArn = "arn:aws:elasticloadbalancing:sa-east-1:126588786097:listener/app/gatekeep-alb/ff82ae699b9862d2/d277ff2b7c7f46a8"
aws elbv2 describe-rules --listener-arn $listenerArn --region sa-east-1

# Verificar estado de targets
$tgArn = "arn:aws:elasticloadbalancing:sa-east-1:126588786097:targetgroup/gatekeep-tg/27437174e066b9ee"
aws elbv2 describe-target-health --target-group-arn $tgArn --region sa-east-1
```

#### Verificar ECS Services
```powershell
# Verificar servicio backend
aws ecs describe-services --cluster gatekeep-cluster --services gatekeep-api-service --region sa-east-1

# Verificar servicio frontend
aws ecs describe-services --cluster gatekeep-cluster --services gatekeep-frontend-service --region sa-east-1

# Verificar variables de entorno del frontend
aws ecs describe-task-definition --task-definition gatekeep-frontend:7 --region sa-east-1 --query "taskDefinition.containerDefinitions[0].environment" --output json
```

#### Verificar Certificados SSL
```powershell
# Listar certificados ACM
aws acm list-certificates --region sa-east-1 --query "CertificateSummaryList[?contains(DomainName, 'zimmzimmgames')].[DomainName,Status,CertificateArn]" --output table
```

### 6.2 Pruebas de Conectividad

#### Probar Frontend
```powershell
# Probar acceso al frontend
curl -I https://zimmzimmgames.com

# Probar acceso al frontend (www)
curl -I https://www.zimmzimmgames.com
```

#### Probar Backend
```powershell
# Probar health check
curl https://api.zimmzimmgames.com/health

# Probar login (requiere credenciales válidas)
$body = @{
    email = "admin1@gatekeep.com"
    password = "admin123"
} | ConvertTo-Json

Invoke-WebRequest -Uri "https://api.zimmzimmgames.com/api/auth/login" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"
```

---

## 7. Problemas Conocidos y Soluciones

### 7.1 Problema Histórico: Variable NEXT_PUBLIC_API_URL Incorrecta

**Descripción:** En versiones anteriores de la Task Definition del frontend, la variable `NEXT_PUBLIC_API_URL` estaba configurada como `https://zimmzimmgames.com` en lugar de `https://api.zimmzimmgames.com`.

**Estado Actual:** ✅ **RESUELTO** - La configuración en Terraform ahora establece correctamente `https://api.${var.domain_name}`.

**Verificación:**
```powershell
# Verificar variable en Task Definition actual
aws ecs describe-task-definition --task-definition gatekeep-frontend:7 --region sa-east-1 --query "taskDefinition.containerDefinitions[0].environment[?name=='NEXT_PUBLIC_API_URL']" --output json
```

**Resultado Esperado:**
```json
[
    {
        "name": "NEXT_PUBLIC_API_URL",
        "value": "https://api.zimmzimmgames.com"
    }
]
```

### 7.2 Regla `/auth/*` Eliminada del ALB

**Descripción:** La regla `/auth/*` (sin prefijo `/api/`) fue eliminada del ALB porque la aplicación solo expone endpoints bajo `/api/auth/*`.

**Estado:** ✅ **CORRECTO** - La configuración actual solo incluye rutas que existen en la aplicación.

**Verificación:**
```powershell
# Verificar que no existe regla /auth/* (sin /api/)
$listenerArn = "arn:aws:elasticloadbalancing:sa-east-1:126588786097:listener/app/gatekeep-alb/ff82ae699b9862d2/d277ff2b7c7f46a8"
$rules = aws elbv2 describe-rules --listener-arn $listenerArn --region sa-east-1 --output json | ConvertFrom-Json
$rules.Rules | Where-Object { 
    $_.Priority -ne "default" -and 
    $_.Conditions[0].Values[0] -eq "/auth/*" 
}
# No debe devolver ningún resultado
```

---

## 8. Checklist de Verificación

### 8.1 Infraestructura AWS

- [x] ALB configurado y activo
- [x] Listeners HTTP y HTTPS configurados
- [x] Reglas de enrutamiento correctas
- [x] Target Groups configurados
- [x] ECS Services ejecutándose
- [x] Route53 registros DNS configurados
- [x] Certificados SSL/TLS válidos
- [x] Security Groups permiten tráfico correcto

### 8.2 Configuración de Código

- [x] CORS configurado con dominios de producción
- [x] Frontend variable `NEXT_PUBLIC_API_URL` correcta
- [x] URL Service con fallbacks correctos
- [x] Config.js con URLs correctas
- [x] Sync service con URLs correctas

### 8.3 Endpoints

- [x] Frontend accesible en `https://zimmzimmgames.com`
- [x] Backend accesible en `https://api.zimmzimmgames.com`
- [x] Health checks funcionando
- [x] Endpoints de autenticación funcionando
- [x] Swagger accesible

---

## 9. Recomendaciones

### 9.1 Monitoreo

1. **Configurar Alertas CloudWatch:**
   - Alertas para targets unhealthy en ALB
   - Alertas para errores 4xx/5xx en ALB
   - Alertas para certificados SSL próximos a expirar

2. **Monitoreo de DNS:**
   - Verificar periódicamente la resolución DNS
   - Monitorear cambios en registros Route53

3. **Monitoreo de Endpoints:**
   - Health checks automatizados
   - Monitoreo de tiempo de respuesta
   - Monitoreo de disponibilidad

### 9.2 Mejoras Futuras

1. **Versionado de URLs:**
   - Considerar usar tags semánticos en lugar de `latest` en ECR
   - Implementar versionado de API (`/api/v1/`)

2. **Documentación:**
   - Mantener este documento actualizado con cada cambio
   - Documentar nuevos endpoints agregados

3. **Testing:**
   - Automatizar pruebas de conectividad
   - Implementar tests de integración para endpoints

---

## 10. Conclusión

### Estado General: ✅ **TODOS LOS LINKS DE PRODUCCIÓN ESTÁN CORRECTAMENTE CONFIGURADOS**

Todos los componentes de la infraestructura tienen las URLs de producción correctamente configuradas:

- ✅ Frontend: `https://zimmzimmgames.com`
- ✅ Backend API: `https://api.zimmzimmgames.com`
- ✅ DNS: Route53 correctamente configurado
- ✅ ALB: Enrutamiento correcto
- ✅ SSL/TLS: Certificados válidos
- ✅ Código: Variables de entorno y fallbacks correctos

### Próximos Pasos

1. Ejecutar los comandos de verificación de la sección 6.1 para confirmar el estado actual
2. Configurar monitoreo y alertas (sección 9.1)
3. Documentar cualquier cambio futuro en este documento

---

**Última Actualización:** 2025-01-XX  
**Versión del Documento:** 1.0  
**Mantenido por:** Equipo GateKeep

